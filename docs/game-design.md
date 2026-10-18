# Sudoku — game design and decisions

Living document. It records what we agreed on before writing code, so every session (human or AI) starts from the same context. The wider portfolio plan lives outside the repo; this file is the source of truth for the Sudoku project.

## Project setup

- Unity 6.6 (6000.6.4f1), URP 2D, uGUI + TextMeshPro, Input System (UI module). Not 6.3 LTS: moving to 6.7 LTS later is a small step, and 6.6 has Fast Enter Play Mode (no domain reload) on by default.
- Branch: `master`.
- Packages are added only when code needs them. Current manifest: URP, uGUI, Input System, Test Framework, MCP for Unity, VContainer 1.19.0, UniTask 2.5.11 (added with the first screen, see ADR 002). Planned later: Addressables (puzzle loading).
- AI tooling: MCP for Unity (CoplayDev, v10.3.0) with Claude Code, HTTP server on `127.0.0.1:8080`. Rejected: the official Unity MCP (pre-release, needs a paid Unity AI plan on Personal).
- Target: WebGL on itch.io, portrait layout. Android port later. **Build size: as small as possible**; the size of every release build is tracked in `docs/build.md`.
- `Sudoku.Core` is a regular asmdef in `Assets/`, not a UPM package.

## Game flow

```
Menu ── Continue (only if a save exists) ───────────────► Game (restored)
  └──── New game ──► difficulty picker ─────────────────► Game (new)

Game:  Playing ⇄ Paused
       Playing ──(all cells correct)──► Won ──────► New game / Menu
       Playing ──(mistake limit hit)──► Game over ─► Second chance (once per game) → Playing (mistakes - 1)
                                                   └► New game / Menu
```

### Game screen (portrait, top to bottom)

1. Header: difficulty, mistakes (`x/3`), timer, pause button.
2. Board, interactive. Selecting a cell highlights its row, column and box, and every cell with the same digit.
3. Action buttons: Undo, Erase, Notes (toggle), Hint.
4. Number pad: one button per symbol (N buttons for an N×N grid).

## Scope

| Feature | Status |
| --- | --- |
| Menu: Continue (if save exists), New game → difficulty | MVP |
| Game screen: header, board, 4 actions, number pad | MVP |
| Highlight row/column/box and same digit | MVP |
| Wrong digit shown in red, mistake limit 3, game over | MVP |
| Hint: reveal the correct digit in the selected cell, 3 per game | MVP |
| Pause: stops the timer and hides the board | MVP |
| Second chance (via fake rewarded ad), once per game | MVP |
| Difficulty as data, 3 packs at start | MVP |
| Generic N×N core, runtime-built grid | MVP (9×9 content first) |
| Score | Later (needs a formula; plugs into game events) |
| Colour themes | Later (all colours come from one theme asset from day one) |
| Settings screen | Later (collect the option list first) |
| Step-by-step hints, puzzle generator, 16×16 content | Later |
| Jigsaw / Killer / X-Sudoku | Out of scope (the unit model could support them) |
| Accounts, online ranking, real ads on WebGL | Out of scope |

## Decisions

**A. A mistake is a digit that differs from the stored solution.** Every puzzle ships with its solution; the import validator checks that the solution is valid and unique. Rejected: "mistake = rule conflict", because a wrong digit without a conflict would slip through and the mistake limit would lose its meaning.

**B. Notes.** Placing a digit removes that digit from the notes of all peers (same row, column, box). Undo restores those notes, so the command stores what it removed. Undo never refunds a mistake.

**C. Difficulty is data, not an enum.** Each difficulty = id, display name, puzzle pack. Start with 3 packs; more levels are content, not code. AI can generate puzzles but cannot reliably rate them, so ratings stay coarse until there is a technique-based rater.

**D. Second chance via `IRewardedAdService`.** WebGL/itch uses a fake implementation that reports "watched" immediately. The Android port swaps the implementation in the composition root only. Available once per game; without a limit the mistake limit would mean nothing. Rejected: cutting the feature (loses a clear DI example).

**E. Portrait layout**, embedded on itch.io in a portrait frame (e.g. 540×960). Rejected: a separate landscape layout (double UI work).

**F. Hints: 3 per game, a hint is a regular move.** It places the correct digit in the selected cell, clears that digit from peer notes and goes into the undo history like any other move. Undo removes the digit (and restores the notes) but never refunds the hint, the same way undo never refunds a mistake. Remaining hints and whether the second chance was used are part of the save.

**G. Timer: Core accumulates time, the Unity layer measures it.** `GameSession` keeps the total play time and exposes `Tick(TimeSpan delta)`; it counts a tick only while `Playing` and rejects negative deltas. The Unity layer feeds `Time.unscaledDeltaTime` every frame and stops feeding it on focus loss and while a dialog is open. This is testable without waiting, saves as plain seconds, and a backgrounded WebGL tab stops the timer on its own because the browser stops sending frames. Rejected: an `ITimeProvider` with timestamps (start, now, minus every pause), because every pause start and end must be recorded and restored after loading a save, which adds edge cases for the same result.

**Defaults (agreed without discussion):** givens are locked and Erase only clears player entries; autosave after every move (WebGL gives no chance to save on tab close); save is deleted on win and on abandoning a game; the timer stops on pause, in dialogs and on focus loss; keyboard: 1–9 (and letters for N > 9), arrows, Backspace, N for notes, Ctrl+Z.

**Puzzles are generated by AI** and must pass the uniqueness validator (an editor tool) before they enter a pack.

## Core model (N×N from day one)

```
GridShape    N, box height × box width, units, peers     immutable, shared
Puzzle       id, shape, givens, solution                  immutable, loaded from JSON
Board        values, notes, which cells are givens        mutable game state
Commands     set digit, toggle note, erase + history      undo / redo
GameSession  mistakes, hints, second chance, play time,   orchestrates the rules
             win / lose
```

The solver and the uniqueness validator work on `GridShape` plus a digit array only. They know nothing about `Board` or the session.

- **`GridShape`**: N = boxRows × boxCols, validated on creation. Units (N rows, N columns, N boxes as cell-index lists) and peers per cell are computed once. Rules, highlighting and note clearing are loops over these lists, with no `row / 3 * 3` arithmetic anywhere. Supported shapes: 2×2 (4×4), 2×3 (6×6), 3×3 (9×9), 4×4 (16×16).
  - API: `new GridShape(boxRows, boxColumns)` takes box dimensions, not board size (a 6×6 board could have 2×3 or 3×2 boxes). Only the four shapes above are accepted (3×2 too is rejected until content needs it); anything else throws `ArgumentException`. Rejected: a `TryCreate` returning `bool`, because a bad shape is a programmer or content error that should be loud.
  - Queries: `BoxRows`, `BoxColumns`, `Size`, `CellCount`, `Units`, `Boxes`, `GetPeers(index)`, `ToIndex(CellPosition)`, `ToPosition(index)`. A position or index outside the grid throws `ArgumentOutOfRangeException`. The order of `Units` is internal; `Boxes` has a guaranteed order (boxes and their cells row by row) because the board view lays cells out with it.
  - Collections are `IReadOnlyList<int>` wrapped once with `Array.AsReadOnly`, so the shared shape cannot be modified through a cast. Rejected: returning `int[]` (mutable by callers), `ReadOnlySpan<int>` (cannot be stored in fields or used in lambdas; premature before the solver is measured).
  - `CellPosition` is a readonly struct (row, column), a pure address. Not named `GridCell`, because "cell" is reserved for the board's view of a cell with its contents.
- **`Board`**: the mutable state of one game, built from a `Puzzle` (the givens are copied, the puzzle stays untouched).
  - API: `GetValue(index)`, `IsGiven(index)`, `HasMistake(index)` (a digit that differs from the solution, decision A), `IsSolved`, `SetValue(index, value)` with value 1..N, `Clear(index)` for Erase. A bad index or value throws `ArgumentOutOfRangeException`; changing a given throws `InvalidOperationException`, so givens are locked by the board itself and no later path (hint, undo, save restore) can overwrite them.
  - `CellChanged(index)` is raised only when a value really changes. Digits will change through many paths (number pad, keyboard, hint, undo, note clearing), so the presenter subscribes once instead of every caller telling the view. Rejected: methods returning the list of changed cells.
  - `Board` does not count mistakes or decide win/lose; that is `GameSession`. `IsSolved` scans all cells on every call. Rejected: a running counter of correct cells (more state to keep right, no measurable gain at 81–256 cells).
  - Addressing is the flat index, the same as `GridShape.GetPeers` and the board view; no `Cell` view struct until a caller needs value, given and mistake together.
- **Cell addressing**: a flat index `0..N²-1` internally, a small row/column struct in the API for UI.
- **Values**: `int` 1..N, 0 = empty. Showing "7" or "C" is a UI concern.
- **Notes**: a `uint` bitmask wrapped in a small struct (`CandidateSet`), bit k = digit k, bit 0 unused. Digits 1..16 need bits 1..16, which is 17 bits, so a `ushort` is too small. Rejected: bit k-1 = digit k in a `ushort` (saves 2 bytes per cell, invites off-by-one errors); `HashSet<int>` per cell (allocations, expensive undo copies).
- **`Puzzle` vs `Board`**: the puzzle is pure data, reusable for new game, save restore and validation. The board holds arrays; a "cell" is a lightweight read-only view. Rejected: one stateful `Cell` object per cell.
- **Tests**: hand-written edge cases on 4×4, then the same tests parameterised over 6×6 and 9×9.

### Puzzle JSON

```json
{ "version": 1, "id": "9x9-easy-001", "boxRows": 3, "boxCols": 3,
  "givens":   "53..7....6..195...",
  "solution": "534678912672195348..." }
```

One character per cell: `.` empty, `1`–`9`, then `A`–`G` for values 10–16. Packs and saves carry a format `version` from day one, so a later format change is detected and migrated instead of guessed. Storing the shape means a new grid size needs no migration.

## Next steps

1. ~~Check question~~ done: on a 6×6 grid (boxes 2 rows × 3 columns), cell (3,4) is in box 3 (zero-based, numbered row by row) and has 12 peers: row (3,0) (3,1) (3,2) (3,3) (3,5), column (0,4) (1,4) (2,4) (4,4) (5,4), box (2,3) (2,5). This becomes the first `GridShape` test case.
2. ~~Skeleton~~ done: `Sudoku.Core` and `Sudoku.Tests.EditMode` asmdefs under `Assets/_Project/`, ADR 001, architecture table in `CLAUDE.md`, MCP permissions in `.claude/settings.json`. Other asmdefs come with their first code.
3. ~~`GridShape`~~ done: units, peers and cell addressing, covered by `GridShapeTests`.
4. ~~First vertical slice "Menu → New game → board with givens"~~ done (ADR 002): `Puzzle`, `IPuzzleProvider` / `TextAssetPuzzleProvider`, menu and board views and presenters, `GameLifetimeScope`, scene `Main` with screen prefabs. The board fits portrait and landscape screens.
5. ~~First WebGL build~~ done, see `docs/build.md`; CI runs the EditMode tests on every pull request (`docs/ci.md`).
6. ~~The interactive board~~ done (ADR 003): `Board` in Core (values, locked givens, mistakes, solved), cell selection with row, column, box and same-digit highlighting, number pad with Erase, wrong digits in red, a "Solved" message, all board colours from `BoardTheme`. Next: keyboard input (digits, arrows, Backspace).
7. Before the first public release: build size pass (checklist in `docs/build.md`).
8. Then: `CandidateSet` (the notes bitmask; first test case: on the 6×6 example puzzle, cell (3,4) has candidates {5, 6}), undo/redo commands, `GameSession` (mistake limit, win), solver and uniqueness validator.
