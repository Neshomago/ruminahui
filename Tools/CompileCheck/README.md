# CompileCheck — offline sanity check (no Unity needed)

```bash
Tools/CompileCheck/check.sh
```

1. Compiles `Assets/Scripts` (runtime) against hand-written Unity API stubs (`Stubs/`).
2. Compiles the Editor tools + EditMode tests (`EditorStubs/`, `TestStubs/`).
3. Runs the pure-logic EditMode tests with a tiny reflection runner (`TestRunner/`).

Needs only the .NET SDK (6+). Uses the .NET Standard 2.1 reference pack, the same API level as Unity, and downloads nothing.

**What a pass means:** C# syntax, types and every reference *between our own scripts* are correct, and the logic tests pass.
**What it does NOT prove:** that Unity API names are right. The stubs encode the API as we believe it to be. Members verified against the real package sources are listed in `LESSONS_LEARNED.md` (L-010, L-012).

**Rules:**
- When code starts using a new Unity API, add it to the stubs with the *real* signature (check the package source), not whatever makes it compile.
- This folder is outside `Assets/`, so Unity ignores it. Keep it that way.
