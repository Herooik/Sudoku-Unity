using System;
using System.Collections.Generic;
using Sudoku.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Sudoku.UI
{
	public sealed class NumberPadView : MonoBehaviour, INumberPadView
	{
		[SerializeField] private RectTransform _digitsParent;
		[SerializeField] private DigitButtonView _digitButtonPrefab;
		[SerializeField] private Button _eraseButton;

		private readonly List<DigitButtonView> _digitButtons = new();

		public event Action<int> DigitClicked;

		public event Action EraseClicked;

		private void Awake()
		{
			_eraseButton.onClick.AddListener(OnEraseButtonClicked);
		}

		private void OnDestroy()
		{
			_eraseButton.onClick.RemoveListener(OnEraseButtonClicked);
		}

		// Built at runtime so a 6×6 grid gets 6 buttons and a 16×16 grid gets 16, with no prefab per size.
		public void Build(int size)
		{
			ClearDigitButtons();

			for (var digit = 1; digit <= size; digit++)
			{
				var button = Instantiate(_digitButtonPrefab, _digitsParent);
				button.SetLabel(CellSymbol.ToSymbol(digit));

				// A for-loop variable is shared by every lambda; without the copy every button would report size + 1.
				var clickedDigit = digit;
				button.Clicked += () => DigitClicked?.Invoke(clickedDigit);

				_digitButtons.Add(button);
			}
		}

		private void OnEraseButtonClicked()
		{
			EraseClicked?.Invoke();
		}

		private void ClearDigitButtons()
		{
			foreach (var button in _digitButtons)
			{
				// Destroy runs at the end of the frame; deactivating first keeps the layout from counting old buttons.
				button.gameObject.SetActive(false);
				Destroy(button.gameObject);
			}

			_digitButtons.Clear();
		}
	}
}
