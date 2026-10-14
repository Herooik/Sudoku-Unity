# Building for the web (itch.io)

The game ships as a WebGL build embedded on itch.io in a portrait frame (decision E in `game-design.md`).

## Build profile

- Active platform: **Web**. Profiles live in `Assets/Settings/Build Profiles/`; use *Web - Desktop - Development* for test builds.
- Scenes in the build: only `Assets/_Project/Scenes/Main.unity`.
- Output folder: `Builds/WebGL/` (ignored by Git). WebGL builds also write Burst output to `/Data/` in the project root, which is ignored too.

## Player Settings → Web

These values are stored in the active build profile (`Web - Desktop - Development.asset`), which carries its own Player Settings in Unity 6, not in `ProjectSettings/ProjectSettings.asset`.

| Setting | Value | Why |
| --- | --- | --- |
| Default Canvas Width × Height | 540 × 960 | the portrait frame the UI is designed for (Canvas Scaler reference 540×960) |
| Compression Format | Gzip | itch.io does not reliably send the `Content-Encoding` headers that Brotli builds need |
| Decompression Fallback | on | the loader decompresses in JavaScript when headers are missing, so the build works on any static host, including a local test server |
| Run In Background | off | the timer must stop when the tab loses focus |

Rejected: Brotli (smallest files, but fails on hosts without the right headers), no compression (works everywhere, but noticeably larger download).

## Uploading to itch.io

1. Zip the **contents** of `Builds/WebGL/`, so `index.html` sits at the root of the zip.
2. On itch.io create or edit the project:
   - Kind of project: **HTML**; upload the zip and tick *This file will be played in the browser*.
   - Embed options: viewport **540 × 960**, *Mobile friendly* (portrait), *Fullscreen button*.
   - Visibility: **Draft** or **Restricted** until the game is ready to publish.
3. Test on the itch.io page in a desktop browser and on a phone.

## Known limits

- No threads in the browser: avoid `Task.Run` and `Thread`; UniTask runs on the main thread and is safe.
- The browser gives no chance to save on tab close, which is why saves happen after every move (see `game-design.md`).

## Build size

Goal: the smallest web build we can get. Measure after every change (Editor log, "Build Report" section) and log it here.

| Date | Change | Total | `.wasm` | `.data` |
| --- | --- | --- | --- | --- |
| 2026-10-12 | first build: URP, TMP, default stripping, Gzip | 12.35 MB | 9.2 MB | 3.6 MB |

Checklist before the first public release, in order of expected gain:

- [ ] **Managed Stripping Level: High**, then play-test, because stripping can remove code used only through reflection (VContainer registrations, `JsonUtility` DTOs); add a `link.xml` for anything that breaks.
- [ ] **Code Optimization: Disk Size with LTO** (Player Settings → Web) for release builds.
- [ ] **Remove unused engine modules** from `Packages/manifest.json` (e.g. 3D/2D physics, animation, particles, video, audio if the game stays silent); Strip Engine Code then drops them from `.wasm`.
- [ ] **Static font atlas** with only the characters the game uses (digits, `A`–`G`, UI text) instead of the dynamic LiberationSans SDF (about 2.2 MB in the project).
- [ ] **Brotli instead of Gzip** if itch.io serves it correctly (smaller download); keep Decompression Fallback.
- [ ] **Exception support** stays at "Explicitly Thrown Exceptions Only"; "None" is smaller but hides errors.
- [ ] Re-check the render pipeline cost: URP shader variants and runtime are part of `.wasm`; compare against a build with only what the UI needs.
