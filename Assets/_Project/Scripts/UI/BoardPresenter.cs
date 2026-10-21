using System;
using Sudoku.Core;

namespace Sudoku.UI
{
	public sealed class BoardPresenter : IDisposable
	{
		private const int NoSelection = -1;

		private readonly INumberPadView _numberPad;
		private readonly IBoardView _view;

		private Board _board;
		private bool[] _isPeerOfSelected = Array.Empty<bool>();
		private int _selectedCell = NoSelection;

		public BoardPresenter(IBoardView view, INumberPadView numberPad)
		{
			_view = view;
			_numberPad = numberPad;

			_view.CellClicked += OnCellClicked;
			_numberPad.DigitClicked += OnDigitClicked;
			_numberPad.EraseClicked += OnEraseClicked;
		}

		public void Dispose()
		{
			_view.CellClicked -= OnCellClicked;
			_numberPad.DigitClicked -= OnDigitClicked;
			_numberPad.EraseClicked -= OnEraseClicked;
			DetachBoard();
		}

		public void Show(Puzzle puzzle)
		{
			// A board from a previous game must not keep redrawing cells of the new one.
			DetachBoard();

			_board = new Board(puzzle);
			_board.CellChanged += OnCellChanged;
			_selectedCell = NoSelection;
			_isPeerOfSelected = new bool[puzzle.Shape.CellCount];

			_view.Build(puzzle.Shape);
			_numberPad.Build(puzzle.Shape.Size);

			for (var i = 0; i < puzzle.Shape.CellCount; i++)
			{
				RefreshCellContent(i);
			}

			RefreshHighlights();
			_view.SetSolvedVisible(false);
			_view.SetVisible(true);
		}

		private void OnCellClicked(int cellIndex)
		{
			if (_board == null)
			{
				return;
			}

			_selectedCell = cellIndex;
			RefreshHighlights();
		}

		private void OnDigitClicked(int digit)
		{
			if (CanEditSelectedCell())
			{
				_board.SetValue(_selectedCell, digit);
			}
		}

		private void OnEraseClicked()
		{
			if (CanEditSelectedCell())
			{
				_board.Clear(_selectedCell);
			}
		}

		// The board itself throws on a given; a player tapping one is a normal action, so it is filtered here.
		private bool CanEditSelectedCell()
		{
			return _board != null
			       && !_board.IsSolved
			       && _selectedCell != NoSelection
			       && !_board.IsGiven(_selectedCell);
		}

		private void OnCellChanged(int cellIndex)
		{
			RefreshCellContent(cellIndex);

			// Same-digit highlights depend on the values of every cell, not only the changed one.
			RefreshHighlights();

			if (_board.IsSolved)
			{
				_view.SetSolvedVisible(true);
			}
		}

		private void RefreshCellContent(int cellIndex)
		{
			var style = CellTextStyle.Player;
			if (_board.IsGiven(cellIndex))
			{
				style = CellTextStyle.Given;
			}
			else if (_board.HasMistake(cellIndex))
			{
				style = CellTextStyle.Mistake;
			}

			_view.SetCellContent(cellIndex, CellSymbol.ToSymbol(_board.GetValue(cellIndex)), style);
		}

		private void RefreshHighlights()
		{
			Array.Clear(_isPeerOfSelected, 0, _isPeerOfSelected.Length);
			var selectedValue = 0;

			if (_selectedCell != NoSelection)
			{
				selectedValue = _board.GetValue(_selectedCell);
				foreach (var peer in _board.Shape.GetPeers(_selectedCell))
				{
					_isPeerOfSelected[peer] = true;
				}
			}

			for (var i = 0; i < _isPeerOfSelected.Length; i++)
			{
				_view.SetCellHighlight(i, HighlightOf(i, selectedValue));
			}
		}

		private CellHighlight HighlightOf(int cellIndex, int selectedValue)
		{
			if (cellIndex == _selectedCell)
			{
				return CellHighlight.Selected;
			}

			// Checked before Peer: a matching digit in the same row is the information the player is looking for.
			if (selectedValue != 0 && _board.GetValue(cellIndex) == selectedValue)
			{
				return CellHighlight.SameDigit;
			}

			return _isPeerOfSelected[cellIndex] ? CellHighlight.Peer : CellHighlight.None;
		}

		private void DetachBoard()
		{
			if (_board != null)
			{
				_board.CellChanged -= OnCellChanged;
			}
		}
	}
}
