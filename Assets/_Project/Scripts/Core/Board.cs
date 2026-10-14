#nullable enable

using System;

namespace Sudoku.Core
{
	// The mutable state of one game. The puzzle stays untouched, so a new game or a save restore can start from it again.
	public sealed class Board
	{
		private readonly int[] _values;

		public Board(Puzzle puzzle)
		{
			Puzzle = puzzle ?? throw new ArgumentNullException(nameof(puzzle));
			Shape = puzzle.Shape;

			_values = new int[puzzle.Givens.Count];
			for (var i = 0; i < _values.Length; i++)
			{
				_values[i] = puzzle.Givens[i];
			}
		}

		// Every path that changes a digit (number pad, hint, undo, note clearing later) ends here,
		// so the presenter redraws only what changed without each caller having to tell it.
		public event Action<int>? CellChanged;

		public Puzzle Puzzle { get; }

		public GridShape Shape { get; }

		public bool IsSolved
		{
			get
			{
				for (var i = 0; i < _values.Length; i++)
				{
					if (_values[i] != Puzzle.Solution[i])
					{
						return false;
					}
				}

				return true;
			}
		}

		public int GetValue(int cellIndex)
		{
			ValidateIndex(cellIndex);
			return _values[cellIndex];
		}

		public bool IsGiven(int cellIndex)
		{
			ValidateIndex(cellIndex);
			return Puzzle.Givens[cellIndex] != 0;
		}

		public bool HasMistake(int cellIndex)
		{
			ValidateIndex(cellIndex);
			var value = _values[cellIndex];
			return value != 0 && value != Puzzle.Solution[cellIndex];
		}

		public void SetValue(int cellIndex, int value)
		{
			ValidateIndex(cellIndex);

			// 0 means empty, but emptying a cell is a separate player action (Erase), so it gets its own method.
			if (value < 1 || value > Shape.Size)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"Value must be in 1..{Shape.Size}.");
			}

			ChangeValue(cellIndex, value);
		}

		public void Clear(int cellIndex)
		{
			ValidateIndex(cellIndex);
			ChangeValue(cellIndex, 0);
		}

		private void ChangeValue(int cellIndex, int value)
		{
			// Locked here rather than only in the UI, so no later path (hint, undo, save restore) can overwrite a given.
			if (IsGiven(cellIndex))
			{
				throw new InvalidOperationException($"Cell {cellIndex} is a given and cannot be changed.");
			}

			if (_values[cellIndex] == value)
			{
				return;
			}

			_values[cellIndex] = value;
			CellChanged?.Invoke(cellIndex);
		}

		private void ValidateIndex(int cellIndex)
		{
			if (cellIndex < 0 || cellIndex >= _values.Length)
			{
				throw new ArgumentOutOfRangeException(
					nameof(cellIndex), cellIndex, $"Cell index must be in 0..{_values.Length - 1}.");
			}
		}
	}
}
