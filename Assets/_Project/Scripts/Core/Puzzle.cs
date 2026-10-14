#nullable enable

using System;
using System.Collections.Generic;

namespace Sudoku.Core
{
	// Validating on construction means an invalid puzzle from a broken pack can never reach the board.
	public sealed class Puzzle
	{
		public Puzzle(string id, GridShape shape, string givens, string solution)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentException("Puzzle id must not be empty.", nameof(id));
			}

			if (shape == null)
			{
				throw new ArgumentNullException(nameof(shape));
			}

			var givenCells = ParseCells(givens, shape, nameof(givens));
			var solutionCells = ParseCells(solution, shape, nameof(solution));
			ValidateSolutionIsComplete(solutionCells, nameof(solution));
			ValidateGivensMatchSolution(givenCells, solutionCells, nameof(givens));
			ValidateSolutionFollowsRules(solutionCells, shape, nameof(solution));

			Id = id;
			Shape = shape;
			Givens = Array.AsReadOnly(givenCells);
			Solution = Array.AsReadOnly(solutionCells);
		}

		public string Id { get; }

		public GridShape Shape { get; }

		public IReadOnlyList<int> Givens { get; }

		public IReadOnlyList<int> Solution { get; }

		private static int[] ParseCells(string? text, GridShape shape, string paramName)
		{
			if (text == null)
			{
				throw new ArgumentNullException(paramName);
			}

			if (text.Length != shape.CellCount)
			{
				throw new ArgumentException(
					$"Expected {shape.CellCount} symbols for a {shape.Size}×{shape.Size} grid, got {text.Length}.",
					paramName);
			}

			var cells = new int[text.Length];
			for (var i = 0; i < text.Length; i++)
			{
				if (!CellSymbol.TryParse(text[i], out var value) || value > shape.Size)
				{
					throw new ArgumentException($"Invalid symbol '{text[i]}' at index {i}.", paramName);
				}

				cells[i] = value;
			}

			return cells;
		}

		private static void ValidateSolutionIsComplete(int[] solutionCells, string paramName)
		{
			var emptyIndex = Array.IndexOf(solutionCells, 0);
			if (emptyIndex >= 0)
			{
				throw new ArgumentException($"Solution has an empty cell at index {emptyIndex}.", paramName);
			}
		}

		private static void ValidateGivensMatchSolution(int[] givenCells, int[] solutionCells, string paramName)
		{
			for (var i = 0; i < givenCells.Length; i++)
			{
				if (givenCells[i] != 0 && givenCells[i] != solutionCells[i])
				{
					throw new ArgumentException(
						$"Given {givenCells[i]} at index {i} differs from the solution value {solutionCells[i]}.",
						paramName);
				}
			}
		}

		private static void ValidateSolutionFollowsRules(int[] solutionCells, GridShape shape, string paramName)
		{
			// One set reused for every unit: Clear is cheaper than allocating 3N sets.
			var seen = new HashSet<int>();
			foreach (var unit in shape.Units)
			{
				seen.Clear();
				foreach (var cell in unit)
				{
					if (!seen.Add(solutionCells[cell]))
					{
						throw new ArgumentException(
							$"Solution repeats {solutionCells[cell]} in a unit (found again at index {cell}).",
							paramName);
					}
				}
			}
		}
	}
}
