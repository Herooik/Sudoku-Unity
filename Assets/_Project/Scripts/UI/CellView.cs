using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sudoku.UI
{
	public sealed class CellView : MonoBehaviour
	{
		[SerializeField] private Image _background;
		[SerializeField] private Button _button;
		[SerializeField] private TMP_Text _label;

		public event Action Clicked;

		private void Awake()
		{
			_button.onClick.AddListener(OnButtonClicked);
		}

		private void OnDestroy()
		{
			_button.onClick.RemoveListener(OnButtonClicked);
		}

		public void SetText(string text, Color color)
		{
			_label.text = text;
			_label.color = color;
		}

		public void SetBackground(Color color)
		{
			_background.color = color;
		}

		private void OnButtonClicked()
		{
			Clicked?.Invoke();
		}
	}
}
