using System;
using Sudoku.UI;

namespace Sudoku.Tests.EditMode
{
	internal sealed class FakeBoardKeyboardView : IBoardKeyboardView
	{
		private Action<int> _digitPressed;
		private Action _erasePressed;
		private Action<int, int> _movePressed;

		public event Action<int> DigitPressed
		{
			add => _digitPressed += value;
			remove => _digitPressed -= value;
		}

		public event Action ErasePressed
		{
			add => _erasePressed += value;
			remove => _erasePressed -= value;
		}

		public event Action<int, int> MovePressed
		{
			add => _movePressed += value;
			remove => _movePressed -= value;
		}

		public bool HasSubscribers => _digitPressed != null || _erasePressed != null || _movePressed != null;

		public void PressDigit(int digit)
		{
			_digitPressed?.Invoke(digit);
		}

		public void PressErase()
		{
			_erasePressed?.Invoke();
		}

		public void PressMove(int rowDelta, int columnDelta)
		{
			_movePressed?.Invoke(rowDelta, columnDelta);
		}
	}
}
