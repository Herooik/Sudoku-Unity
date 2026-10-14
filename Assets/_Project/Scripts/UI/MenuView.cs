using System;
using UnityEngine;
using UnityEngine.UI;

namespace Sudoku.UI
{
	public sealed class MenuView : MonoBehaviour, IMenuView
	{
		[SerializeField] private Button _newGameButton;

		private void Awake()
		{
			_newGameButton.onClick.AddListener(OnNewGameButtonClicked);
		}

		private void OnDestroy()
		{
			_newGameButton.onClick.RemoveListener(OnNewGameButtonClicked);
		}

		public event Action NewGameClicked;

		public void SetVisible(bool visible)
		{
			gameObject.SetActive(visible);
		}

		private void OnNewGameButtonClicked()
		{
			NewGameClicked?.Invoke();
		}
	}
}
