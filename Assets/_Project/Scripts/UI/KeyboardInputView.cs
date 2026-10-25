using System;
using Sudoku.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sudoku.UI
{
	// Lives on the game screen, so the keyboard drives the board only while the board is shown.
	public sealed class KeyboardInputView : MonoBehaviour, IBoardKeyboardView
	{
		// Generated from BoardControls.inputactions: a renamed action is a compile error, not a runtime lookup failure.
		// Each instance owns its copy of the actions, so enabling them never touches the shared asset.
		private BoardControls _controls;
		private Keyboard _keyboard;

		public event Action<int> DigitPressed;

		public event Action ErasePressed;

		public event Action<int, int> MovePressed;

		private void Awake()
		{
			_controls = new BoardControls();
		}

		private void OnEnable()
		{
			_controls.Board.Move.performed += OnMovePerformed;
			_controls.Board.Erase.performed += OnErasePerformed;
			_controls.Board.Enable();

			// Characters, not keys: the top row, the numpad and letters for 16×16 all arrive the same way, on any layout.
			// A phone without a keyboard simply never raises anything.
			_keyboard = Keyboard.current;
			if (_keyboard != null)
			{
				_keyboard.onTextInput += OnTextInput;
			}
		}

		private void OnDisable()
		{
			_controls.Board.Move.performed -= OnMovePerformed;
			_controls.Board.Erase.performed -= OnErasePerformed;
			_controls.Board.Disable();

			if (_keyboard != null)
			{
				_keyboard.onTextInput -= OnTextInput;
				_keyboard = null;
			}
		}

		private void OnDestroy()
		{
			_controls.Dispose();
		}

		private void OnTextInput(char character)
		{
			if (CellSymbol.TryParse(char.ToUpperInvariant(character), out var value) && value > 0)
			{
				DigitPressed?.Invoke(value);
			}
		}

		private void OnMovePerformed(InputAction.CallbackContext context)
		{
			var direction = context.ReadValue<Vector2>();

			// Screen up is a smaller row index.
			MovePressed?.Invoke(-Mathf.RoundToInt(direction.y), Mathf.RoundToInt(direction.x));
		}

		private void OnErasePerformed(InputAction.CallbackContext context)
		{
			ErasePressed?.Invoke();
		}
	}
}
