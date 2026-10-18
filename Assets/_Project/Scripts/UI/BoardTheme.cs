using System;
using UnityEngine;

namespace Sudoku.UI
{
	// Every board colour lives here, so a second theme later is a new asset, not a code change. Read-only at runtime.
	[CreateAssetMenu(fileName = "BoardTheme", menuName = "Sudoku/Board Theme")]
	public sealed class BoardTheme : ScriptableObject
	{
		[Header("Cell background")]
		[SerializeField] private Color _cellBackground = Color.white;
		[SerializeField] private Color _peerBackground = new(0.91f, 0.93f, 0.96f);
		[SerializeField] private Color _sameDigitBackground = new(0.78f, 0.86f, 0.98f);
		[SerializeField] private Color _selectedBackground = new(0.62f, 0.76f, 0.96f);

		[Header("Cell text")]
		[SerializeField] private Color _givenText = new(0.13f, 0.13f, 0.13f);
		[SerializeField] private Color _playerText = new(0.16f, 0.36f, 0.75f);
		[SerializeField] private Color _mistakeText = new(0.85f, 0.2f, 0.2f);

		public Color GetBackground(CellHighlight highlight)
		{
			return highlight switch
			{
				CellHighlight.None => _cellBackground,
				CellHighlight.Peer => _peerBackground,
				CellHighlight.SameDigit => _sameDigitBackground,
				CellHighlight.Selected => _selectedBackground,
				_ => throw new ArgumentOutOfRangeException(nameof(highlight), highlight, null),
			};
		}

		public Color GetTextColor(CellTextStyle style)
		{
			return style switch
			{
				CellTextStyle.Given => _givenText,
				CellTextStyle.Player => _playerText,
				CellTextStyle.Mistake => _mistakeText,
				_ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
			};
		}
	}
}
