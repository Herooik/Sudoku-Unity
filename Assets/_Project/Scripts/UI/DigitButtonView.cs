using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sudoku.UI
{
	public sealed class DigitButtonView : MonoBehaviour
	{
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

		public void SetLabel(string text)
		{
			_label.text = text;
		}

		private void OnButtonClicked()
		{
			Clicked?.Invoke();
		}
	}
}
