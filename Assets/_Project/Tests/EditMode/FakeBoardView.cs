using System.Collections.Generic;
using Sudoku.Core;
using Sudoku.UI;

namespace Sudoku.Tests.EditMode
{
	internal sealed class FakeBoardView : IBoardView
	{
		public GridShape BuiltShape { get; private set; }

		public Dictionary<int, string> CellTexts { get; } = new Dictionary<int, string>();

		public bool IsVisible { get; private set; }

		// Lets tests check order, e.g. that the board becomes visible only after it is filled.
		public List<string> Calls { get; } = new List<string>();

		public void Build(GridShape shape)
		{
			BuiltShape = shape;
			CellTexts.Clear();
			Calls.Add(nameof(Build));
		}

		public void SetCellText(int cellIndex, string text)
		{
			CellTexts[cellIndex] = text;
			Calls.Add(nameof(SetCellText));
		}

		public void SetVisible(bool visible)
		{
			IsVisible = visible;
			Calls.Add($"{nameof(SetVisible)}({visible})");
		}
	}
}
