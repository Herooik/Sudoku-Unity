# ADR 003: Board interaction and theming

- Status: accepted
- Date: 2026-10-15

## Context

The board becomes interactive: the player selects a cell, sees related cells highlighted, places digits from a number pad and erases them. Wrong digits are shown in red and a solved board shows a message. This fixes where the selection lives, who decides how a cell looks and where colours come from, for every later feature that changes the board (notes, hints, undo, keyboard).

## Decision

- **Game state lives in `Board` (Core), selection lives in `BoardPresenter` (UI).** The selection is not part of a game: it is not saved and the rules do not depend on it.
- **The presenter sends meaning, the view picks colours.** `IBoardView.SetCellHighlight(index, CellHighlight)` (`None`, `Peer`, `SameDigit`, `Selected`) and `SetCellContent(index, text, CellTextStyle)` (`Given`, `Player`, `Mistake`). `Peer` is the whole row, the whole column and the box of the selected cell; `SameDigit` wins over `Peer`, because a matching digit in the same unit is the conflict the player needs to see.
- **All board colours come from one `BoardTheme` ScriptableObject** referenced by `BoardView`. It is read-only at runtime.
- **The presenter reacts to `Board.CellChanged`**: it redraws the text of the changed cell and recomputes the highlights of the whole board, because same-digit highlights depend on every value. Recomputing 81 (at most 256) cells per change costs nothing measurable.
- **The number pad is built at runtime** with one button per digit of the grid size, behind `INumberPadView`.
- The presenter filters input that `Board` would reject: digits and Erase are ignored without a selection, on a given and on a solved board.

## Consequences

- Presenter tests assert on enums with fake views; changing the colours or adding a second theme touches no logic and no test.
- Later input paths (keyboard, hints, undo) only call `Board`; the view updates through `CellChanged` without extra wiring.
- `BoardView` depends on a theme asset; a missing reference is a scene setup error that shows up on the first cell update.

## Alternatives considered

- **Colours in the presenter** (`UnityEngine.Color` in presenter calls). Tests would compare colours instead of meaning, and every theme change would touch the presenter.
- **Selection in `Board`.** Mixes UI state into the game model and into future saves.
- **Redrawing only the cells whose highlight changed.** Needs the previous state and a diff for the same visible result; not worth it at this size.
