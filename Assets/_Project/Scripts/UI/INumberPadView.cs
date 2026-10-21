using System;

namespace Sudoku.UI
{
	public interface INumberPadView
	{
		// The digit 1..N, not the button index.
		event Action<int> DigitClicked;

		event Action EraseClicked;

		void Build(int size);
	}
}
