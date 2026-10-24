# ADR 004: Keyboard input for the board

- Status: accepted
- Date: 2026-10-21

## Context

On desktop WebGL most players expect to type digits and move with the arrows. The board already reacts to clicks through `BoardPresenter` (ADR 003). Keyboard input must reach the same rules (givens locked, no input after solving) and work for every grid size, including letters A–G on 16×16.

## Decision

- **The keyboard is one more passive view.** `KeyboardInputView` (`MonoBehaviour`, implements `IBoardKeyboardView`) turns key presses into `DigitPressed(int)`, `ErasePressed` and `MovePressed(rowDelta, columnDelta)`. `BoardPresenter` handles them through the same private methods as the number pad and cell clicks.
- **Digits come from text input**, `Keyboard.onTextInput`, parsed with `CellSymbol.TryParse` (after `char.ToUpperInvariant`). The top row, the numpad and letters arrive the same way on any keyboard layout. The presenter drops digits larger than the grid, because `Board.SetValue` would rightly throw on them.
- **Movement and erase are Input System actions** in a separate asset, `Assets/_Project/Input/BoardControls.inputactions` (map `Board`: `Move` on the arrow keys, `Erase` on Backspace and Delete). The template `InputSystem_Actions` used by the EventSystem stays untouched.
- **The actions are used through the generated C# class** `BoardControls` (`Scripts/UI/Input/BoardControls.cs`, "Generate C# Class" in the asset's import settings). `_controls.Board.Move` instead of `FindAction("Board/Move")`: renaming an action breaks the build instead of failing when the game screen opens. Each `KeyboardInputView` creates its own instance, so enabling the actions never changes the shared asset; the view disposes it in `OnDestroy`.
- **Movement is computed in the presenter** with `GridShape.ToPosition` / `ToIndex`. Arrows stop at the edge; the first arrow without a selection selects the centre cell `(Size / 2, Size / 2)` without moving.
- **UI navigation is off on board buttons** (`Navigation = None` on cells, digit buttons and Erase), so the arrows don't also move the EventSystem's selected button.
- The view sits on the game screen root, so it listens only while the board is shown. Without a keyboard (`Keyboard.current == null`) it raises nothing.

## Consequences

- Movement and filtering are covered by EditMode tests on a fake keyboard view; the `MonoBehaviour` only maps keys to events.
- Notes (N) and undo (Ctrl+Z) will be two more actions in the same map.
- Holding an arrow moves one cell; auto-repeat can be added later if it feels slow.

## Alternatives considered

- **One action per digit.** Up to 16 bindings plus numpad duplicates, and key positions depend on the layout; text input gives the typed character directly.
- **Polling `Keyboard.current` in `Update`.** Works, but checks every key every frame and has no place for rebinding or a gamepad later.
- **Looking actions up by name** (`FindAction("Board/Move", throwIfNotFound: true)`). A typo or a rename is found only at runtime.
- **`InputActionReference` fields.** Rename-safe (they store the action id), but every action has to be wired by hand in the prefab and an empty slot is again a runtime error.
- **Reusing the UI `Navigate` action.** It belongs to the EventSystem; sharing it would make board movement and UI focus fight over the same input.
