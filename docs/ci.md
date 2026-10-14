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

## One-time setup: Unity license secrets

GitHub → repository → Settings → Secrets and variables → Actions → New repository secret:

| Secret | Value |
| --- | --- |
| `UNITY_EMAIL` | the Unity account e-mail |
| `UNITY_PASSWORD` | the Unity account password |
| `UNITY_LICENSE` | the contents of the license file for that account |

Recent Unity Hub versions store a Personal license as `%LOCALAPPDATA%\Unity\licenses\UnityEntitlementLicense.xml` instead of the older `C:\ProgramData\Unity\Unity_lic.ulf`. Follow the current GameCI activation guide (game.ci → Getting started → Activation) for which file `UNITY_LICENSE` must contain; the first workflow run shows whether activation works. Never paste these values anywhere else.

## Protecting `master`

Branch rules are only enforced in public repositories on the free GitHub plan. While the repository is private, the flow above is a working agreement. When it becomes public (with the README and the first release), add a ruleset:

Settings → Rules → Rulesets → New branch ruleset, target `master`:

- Restrict deletions; Block force pushes.
- Require a pull request before merging, with **0** required approvals (GitHub does not let you approve your own pull request).
- Require status checks to pass: the "EditMode tests" job; require branches to be up to date before merging.
- Leave the bypass list empty, so the rules apply to the owner too.

## Cost

Private repositories on the free plan get 2,000 Actions minutes a month; one run takes roughly 10–15 minutes without a warm cache. Public repositories have no limit on standard runners.
