# Continuous integration and the pull request flow

## Flow

- Work happens on branches named by Conventional Commit type: `feat/…`, `fix/…`, `chore/…`, `docs/…`, `refactor/…`, `test/…`.
- A branch reaches `master` only through a pull request with a short description: what changed, why, what to review.
- Every pull request to `master` and every push to `master` runs the EditMode tests (`.github/workflows/tests.yml`). A pull request is merged only when they are green.

## Tests workflow

- GameCI `unity-test-runner` runs the EditMode tests in Docker, on the Unity version from `ProjectSettings/ProjectVersion.txt`.
- The `Library` folder is cached between runs, keyed on `Assets`, `Packages` and `ProjectSettings`, so later runs skip most of the import.
- Results appear as a check on the pull request ("EditMode test results") and as a downloadable artifact.
- Runs for an older commit on the same branch are cancelled when a newer one starts.
- It can also be started by hand: Actions → Tests → Run workflow.
- The game-ci CLI version is pinned with `cliVersion`. To update it, pick a tag from `https://github.com/game-ci/cli/releases` and confirm the pull request run is green.
- Code coverage is off (`GAME_CI_COVERAGE_ENABLED: 'false'` on the test step): nothing reads the report, and the instrumentation costs memory. The action's `coverageEnabled` input does not work with CLI v0.1.72, which rejects the `--no-coverageEnabled` flag it sends.

## One-time setup: Unity license secrets

GitHub → repository → Settings → Secrets and variables → Actions → New repository secret:

| Secret | Value |
| --- | --- |
| `UNITY_EMAIL` | the Unity account e-mail |
| `UNITY_PASSWORD` | the Unity account password |
| `UNITY_LICENSE` | the contents of the license file for that account |

Recent Unity Hub versions store a Personal license as `%LOCALAPPDATA%\Unity\licenses\UnityEntitlementLicense.xml` instead of the older `C:\ProgramData\Unity\Unity_lic.ulf`. Follow the current GameCI activation guide (game.ci → Getting started → Activation) for which file `UNITY_LICENSE` must contain; the first workflow run shows whether activation works. Never paste these values anywhere else.

## Protecting `master`

The repository is public, so GitHub enforces the "Protect master" ruleset (Settings → Rules → Rulesets) instead of relying on a working agreement:

- Restrict deletions; Block force pushes.
- Require a pull request before merging, with **0** required approvals (GitHub does not let you approve your own pull request). Review comment threads must be resolved before merging.
- Require status checks to pass: the "EditMode tests" job; branches must be up to date with `master` before merging.
- The bypass list is empty, so the rules apply to the owner too.

To check what is enforced without opening the settings: `https://api.github.com/repos/Herooik/Sudoku-Unity/rules/branches/master`.

## Cost

Public repositories have no limit on Actions minutes for standard runners, and those runners have 16 GB of RAM. Private repositories on the free plan get 2,000 minutes a month and 7 GB runners, which is not enough memory for a Unity import (the job is killed with exit code 137). One run takes roughly 10–15 minutes without a warm cache.
