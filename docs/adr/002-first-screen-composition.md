# ADR 002: Composition of the first playable screen

- Status: accepted
- Date: 2026-10-09

## Context

The first vertical slice is "Menu → New game → board with the givens of one puzzle". It is the first code outside `Sudoku.Core`, so it fixes how screens are structured, how objects are wired together and how puzzles reach the game. These choices shape every later screen.

## Decision

- **VContainer** is the DI container. `GameLifetimeScope` in `Sudoku.Bootstrap` is the composition root and the only place that knows concrete implementations.
- **MVP**: views are passive `MonoBehaviour`s (uGUI + TMP) that render and raise events; presenters are plain C# classes that hold the logic and are testable with fake views.
- **One scene with two screen panels** (menu, game) for now. Switching screens is enabling and disabling panels.
- **`IPuzzleProvider` is async from day one** and returns `UniTask<Puzzle>`, taking a `CancellationToken`.
- **Temporary `TextAssetPuzzleProvider`** reads one JSON puzzle assigned in the Inspector. It will be replaced by an Addressables-based provider behind the same interface.
- JSON is deserialised in `Sudoku.Infrastructure` (it needs Unity's `JsonUtility`); `Puzzle` in `Sudoku.Core` only receives plain strings and validates them.

## Consequences

- Swapping the puzzle source later touches one class and one registration line.
- Presenters can be unit-tested in EditMode without a scene.
- A second scene (e.g. for a loading screen) would need a parent `LifetimeScope` to pass state between scenes; that is deferred until it is needed.
- Two packages are added now: VContainer 1.19.0 and UniTask 2.5.11.

## Alternatives considered

- **Zenject / Extenject.** Larger feature set (signals, pools, subcontainers) that we do not need, heavier reflection and slower start-up on WebGL, and the original project is no longer maintained.
- **Two scenes (Menu, Game).** Requires passing the chosen puzzle across a scene load without static state, i.e. a parent scope with `DontDestroyOnLoad`. More plumbing than one screen switch is worth today.
- **Synchronous `IPuzzleProvider`.** Simpler now, but Addressables loading is asynchronous, so every caller would change when the provider is replaced.
- **Addressables right away.** Matches the final plan, but adds groups, build steps and handle ownership before there is more than one puzzle to load.
