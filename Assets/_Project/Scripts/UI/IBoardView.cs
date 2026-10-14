using Sudoku.Core;

namespace Sudoku.UI
{
	// Passive: the presenter decides what each cell shows; the view only lays out and renders text.
	public interface IBoardView
	{
		void Build(GridShape shape);

		void SetCellText(int cellIndex, string text);

		void SetVisible(bool visible);
	}
}
