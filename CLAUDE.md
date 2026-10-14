# Sudoku — instructions for Claude

The personal working agreement (who writes code, how understanding is checked, language of the conversation) lives outside the repo:

@~/.claude/sudoku-how-we-work.md

## Architecture

Game design, scope and agreed decisions: `docs/game-design.md`. Read it before proposing design or scope changes.

| Assembly | Responsibility | May depend on | Status |
| --- | --- | --- | --- |
| `Sudoku.Core` | grid shape, board, rules, solver, uniqueness validator, undo/redo commands, game session; "No Engine References" | nothing | created |
| `Sudoku.Services` | interfaces: `IPuzzleProvider`, `ISaveService`, `IRewardedAdService` | Core, UniTask | created |
| `Sudoku.Infrastructure` | implementations: Addressables puzzle packs, JSON save, fake rewarded ad | Core, Services, Addressables (later), UniTask | created |
| `Sudoku.UI` | MVP: passive views (MonoBehaviour, uGUI + TMP) and presenters (plain C#) | Core, Services, UniTask, VContainer (`[Inject]` and entry-point interfaces only) | created |
| `Sudoku.Bootstrap` | `LifetimeScope` composition root; the only place that knows concrete implementations | everything above + VContainer | created |
| `Sudoku.Tests.EditMode` | tests for Core and presenters with fake views | Core, Services, Infrastructure, UI, UniTask, VContainer | created |

New assemblies are created together with their first code, and packages are added only when that code needs them.

Architecture decisions are recorded as ADRs in `docs/adr/`.

## Code rules

- No singletons, no static mutable state, no `FindObjectOfType` / `FindFirstObjectByType` / `GameObject.Find`. This keeps the project ready for Fast Enter Play Mode.
- Dependencies come through the constructor (plain classes) or an `[Inject] void Construct(...)` method (MonoBehaviours).
- Inspector data uses `[SerializeField] private` fields, never public fields.
- Async code uses UniTask, not coroutines. Every async method takes a `CancellationToken`; MonoBehaviours pass `destroyCancellationToken`.
- Every Addressables handle has exactly one owner that releases it.
- Never write to ScriptableObjects at runtime.
- Don't use `?.` or `??` on `UnityEngine.Object` — they bypass Unity's overloaded null check.
- Use `TMP_Text`, never legacy `Text`.
- Naming: `PascalCase` for types and methods, `_camelCase` for private fields, `I` prefix for interfaces. One public type per file, named after the type.
- Code, comments, commit messages and docs are in English.
- Comments explain why, not what. No comments that restate the code.
- Indentation: tabs, always braces around bodies (see `.editorconfig`).
- `Sudoku.Core`: `#nullable enable` at the top of every file; `T?` marks values that may be null. Not in Unity-facing assemblies (Inspector fields and Unity's fake null make the analysis noisy and misleading there).

## Unity files: hands off

- Don't hand-edit `.unity`, `.prefab` or `.asset` YAML. Use Unity MCP or ask me.
- Never create, delete or edit `.meta` files manually. Never delete an asset without its `.meta`, or the other way round.
- Don't change `ProjectSettings/` or `Packages/manifest.json` without asking.
- Ignore `Library/`, `Temp/`, `Logs/`, `UserSettings/`.

## Unity MCP

- Use it for the console, scene and prefab setup, creating assets and running tests.
- After changing a scene or prefab, list exactly what you changed so I can review it in Git.
- Don't enter Play Mode, start a build or change project settings unless I ask.
- If an MCP call fails, tell me. Don't work around it by editing YAML.

## Tests

- Test names: `Method_Scenario_Expected`.
- Bug fix: write a failing test first, then fix it.
- Editor open: run tests and read the console through Unity MCP.
- Editor closed: use the CLI (it fails if the project is open in the Editor):

  ```bash
  "$UNITY_PATH" -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults TestResults/editmode.xml -logFile -
  ```

## Git

- Don't commit or push unless I ask.
- When I do: small, focused commits using Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`).
- Never commit to `master` directly. Work on a branch named after the change type (`feat/…`, `fix/…`, `chore/…`, `docs/…`); `master` changes only through a pull request whose EditMode tests are green (see `docs/ci.md`). I merge.
- A pull request description says what changed, why, and what to review.
- Never force-push and never rewrite history on `master`.

## Definition of done

- No new compile errors or warnings in the Unity console.
- EditMode tests pass.
- The architecture table and code rules are respected.
- New architecture decisions are recorded in `docs/adr/`.
- A short summary: what changed, why, and what I should review.
