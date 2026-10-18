using System;
using Sudoku.UI;

namespace Sudoku.Tests.EditMode
{
	internal sealed class FakeNumberPadView : INumberPadView
	{
		private Action<int> _digitClicked;
		private Action _eraseClicked;

		public event Action<int> DigitClicked
		{
			add => _digitClicked += value;
			remove => _digitClicked -= value;
		}

		public event Action EraseClicked
		{
			add => _eraseClicked += value;
			remove => _eraseClicked -= value;
		}

		public bool HasSubscribers => _digitClicked != null || _eraseClicked != null;

		public int BuiltSize { get; private set; }

		public void Build(int size)
		{
			BuiltSize = size;
		}

		public void ClickDigit(int digit)
		{
			_digitClicked?.Invoke(digit);
		}

		public void ClickErase()
		{
			_eraseClicked?.Invoke();
		}
	}
}
