using System;

namespace Sudoku.UI
{
	public interface IMenuView
	{
		event Action NewGameClicked;

		void SetVisible(bool visible);
	}
}
