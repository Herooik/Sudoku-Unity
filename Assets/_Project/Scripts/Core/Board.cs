#nullable enable

using System;
using System.Collections.Generic;

namespace Sudoku.Core
{
	// The mutable state of one game. The puzzle stays untouched, so a new game or a save restore can start from it again.
	public sealed class Board
	{
		private readonly int[] _values;

		// default(CandidateSet) is the empty set, so a new array already means "no notes anywhere".
		private readonly CandidateSet[] _notes;

		// Reused on every placement, so clearing peer notes allocates nothing after the first move.
		private readonly List<int> _changedPeers = new List<int>();

		public Board(Puzzle puzzle)
		{
			Puzzle = puzzle ?? throw new ArgumentNullException(nameof(puzzle));
			Shape = puzzle.Shape;

			_values = new int[puzzle.Givens.Count];
			for (var i = 0; i < _values.Length; i++)
			{
				_values[i] = puzzle.Givens[i];
			}

			_notes = new CandidateSet[_values.Length];
		}

		// Raised whenever what a cell shows changes: its digit or its notes. Every path (number pad, hint, undo,
		// note clearing on peers) ends here, so the presenter redraws only what changed without each caller telling it.
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

		public CandidateSet GetNotes(int cellIndex)
		{
			ValidateIndex(cellIndex);
			return _notes[cellIndex];
		}

		public void ToggleNote(int cellIndex, int digit)
		{
			ValidateIndex(cellIndex);

			if (digit < 1 || digit > Shape.Size)
			{
				throw new ArgumentOutOfRangeException(nameof(digit), digit, $"Digit must be in 1..{Shape.Size}.");
			}

			// Notes belong to empty cells only; the presenter filters taps on givens and filled cells.
			if (_values[cellIndex] != 0)
			{
				throw new InvalidOperationException($"Cell {cellIndex} holds a digit and cannot take notes.");
			}

			_notes[cellIndex] = _notes[cellIndex].Toggle(digit);
			CellChanged?.Invoke(cellIndex);
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
			_notes[cellIndex] = CandidateSet.Empty;

			// Only a correct digit clears it from peer notes: a wrong one must not wipe notes the player still needs.
			_changedPeers.Clear();
			if (value != 0 && value == Puzzle.Solution[cellIndex])
			{
				foreach (var peer in Shape.GetPeers(cellIndex))
				{
					var notes = _notes[peer].Without(value);
					if (notes != _notes[peer])
					{
						_notes[peer] = notes;
						_changedPeers.Add(peer);
					}
				}
			}

			// Events go out after all state is updated, so a subscriber never sees a half-applied move.
			CellChanged?.Invoke(cellIndex);
			foreach (var peer in _changedPeers)
			{
				CellChanged?.Invoke(peer);
			}
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
