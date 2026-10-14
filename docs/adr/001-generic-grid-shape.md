# ADR 001: Generic N×N grid shape in Core

- Status: accepted
- Date: 2026-10-07

## Context

The MVP ships 9×9 puzzles, but smaller grids (6×6 for mobile, 4×4) and 16×16 are realistic follow-ups. If the core assumes 9 and 3 everywhere, adding a size later touches the rules, the solver, the uniqueness validator, the save format, the puzzle JSON and the views at once.

## Decision

`Sudoku.Core` describes the board with an immutable `GridShape`: size N and box dimensions (rows × columns) with N = boxRows × boxCols, so boxes may be rectangular (2×3 for 6×6).

- On creation the shape computes its **units** (N rows, N columns, N boxes as lists of cell indexes) and the **peers** of every cell (cells sharing at least one unit, excluding the cell itself).
- Rules, highlighting and note clearing iterate over these precomputed lists; no code does box arithmetic like `row / 3 * 3`.
- The only rule is "every unit contains each value 1..N exactly once".
- Cells are flat indexes `0..N²-1` internally; values are `int` 1..N with 0 for empty; notes are a bitmask.
- The shape is stored in puzzle JSON and in save files from day one.
- The grid view is built at runtime from the shape instead of a fixed 81-cell layout in the scene.

## Consequences

- Tests can use hand-written 4×4 cases (16 cells) for solver and validator edge cases, then run the same tests parameterised over 6×6 and 9×9.
- Slightly more abstraction than a 9×9-only MVP needs: one shape type and a peers table.
- A naive backtracking solver is too slow for 16×16; it needs a "fewest candidates first" heuristic before 16×16 content ships.
- The unit model could also express Jigsaw or X-Sudoku, but those are out of scope (separate UI and content).

## Alternatives considered

- **Hard-coded 9×9 now, refactor later.** Simpler for the first week, but the refactor would touch every layer and break saved data.
- **Computing units on the fly** for every query. Less memory, but slower in the solver and spreads box arithmetic across the codebase.
