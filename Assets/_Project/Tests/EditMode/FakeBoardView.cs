using System;
using System.Collections.Generic;
using Sudoku.Core;
using Sudoku.UI;

namespace Sudoku.Tests.EditMode
{
	internal sealed class FakeBoardView : IBoardView
	{
		private Action<int> _cellClicked;

		public event Action<int> CellClicked
		{
			add => _cellClicked += value;
			remove => _cellClicked -= value;
		}

		public bool HasSubscribers => _cellClicked != null;

		public GridShape BuiltShape { get; private set; }

		public Dictionary<int, string> CellTexts { get; } = new Dictionary<int, string>();

		public Dictionary<int, CellTextStyle> CellStyles { get; } = new Dictionary<int, CellTextStyle>();

		public Dictionary<int, CellHighlight> CellHighlights { get; } = new Dictionary<int, CellHighlight>();

		public bool IsSolvedVisible { get; private set; }

		public bool IsVisible { get; private set; }

		// Lets tests check order, e.g. that the board becomes visible only after it is filled.
		public List<string> Calls { get; } = new List<string>();

		public void Build(GridShape shape)
		{
			BuiltShape = shape;
			CellTexts.Clear();
			CellStyles.Clear();
			CellHighlights.Clear();
			Calls.Add(nameof(Build));
		}

		public void SetCellContent(int cellIndex, string text, CellTextStyle style)
		{
			CellTexts[cellIndex] = text;
			CellStyles[cellIndex] = style;
			Calls.Add(nameof(SetCellContent));
		}

		public void SetCellHighlight(int cellIndex, CellHighlight highlight)
		{
			CellHighlights[cellIndex] = highlight;
			Calls.Add(nameof(SetCellHighlight));
		}

		public void SetSolvedVisible(bool visible)
		{
			IsSolvedVisible = visible;
			Calls.Add($"{nameof(SetSolvedVisible)}({visible})");
		}

		public void SetVisible(bool visible)
		{
			IsVisible = visible;
			Calls.Add($"{nameof(SetVisible)}({visible})");
		}

		public void ClickCell(int cellIndex)
		{
			_cellClicked?.Invoke(cellIndex);
		}

		public List<int> CellsWith(CellHighlight highlight)
		{
			var cells = new List<int>();
			foreach (var pair in CellHighlights)
			{
				if (pair.Value == highlight)
				{
					cells.Add(pair.Key);
				}
			}

			cells.Sort();
			return cells;
		}
	}
}
