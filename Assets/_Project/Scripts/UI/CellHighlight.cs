namespace Sudoku.UI
{
	// What a cell means relative to the selection; the view decides which colour that is.
	public enum CellHighlight
	{
		None,
		Peer,
		SameDigit,
		Selected,
	}
}
