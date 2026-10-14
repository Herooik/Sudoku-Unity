using System;
using Sudoku.UI;

namespace Sudoku.Tests.EditMode
{
	internal sealed class FakeMenuView : IMenuView
	{
		private Action _newGameClicked;

		public event Action NewGameClicked
		{
			add => _newGameClicked += value;
			remove => _newGameClicked -= value;
		}

		public bool HasSubscribers => _newGameClicked != null;

		public bool IsVisible { get; private set; } = true;

		public void SetVisible(bool visible)
		{
			IsVisible = visible;
		}

		public void ClickNewGame()
		{
			_newGameClicked?.Invoke();
		}
	}
}
