using TMPro;
using UnityEngine;

namespace Sudoku.UI
{
	public sealed class CellView : MonoBehaviour
	{
		[SerializeField] private TMP_Text _label;

		public void SetText(string text)
		{
			_label.text = text;
		}
	}
}
