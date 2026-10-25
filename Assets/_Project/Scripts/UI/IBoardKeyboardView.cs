using System;

namespace Sudoku.UI
{
	public interface IBoardKeyboardView
	{
		// The digit as typed, 1..16; it may be larger than the grid, the presenter decides.
		event Action<int> DigitPressed;

		event Action ErasePressed;

		// Row and column deltas, each -1, 0 or 1.
		event Action<int, int> MovePressed;
	}
}
