# Contributing to ImageJCsharp

Thank you for considering a contribution.

ImageJCsharp is a long-term effort to build a C# native replacement for ImageJ on Windows. Contributions are welcome from C# developers, image processing users, scientists, testers, writers, and people who know ImageJ workflows well.

## Project Direction

Before contributing, please read:

- [README.md](README.md)
- [ROADMAP.md](ROADMAP.md)
- [docs/near-term-plan.md](docs/near-term-plan.md)

The current priority is feasibility. We are building a reliable MVP before expanding into broad ImageJ feature parity.

## Good First Contribution Areas

Good first areas include:

- Documentation improvements.
- Small UI fixes.
- Core unit tests.
- Simple image processing filters.
- ROI behavior tests.
- Measurement table improvements.
- Example images for manual testing.
- Reproducing ImageJ behavior and documenting differences.

## Development Setup

Required:

- Windows 10 or newer.
- Visual Studio 2022 or compatible .NET SDK environment.
- .NET Framework 4.8 targeting pack.

Build:

```powershell
dotnet build ImageJCsharp.sln
```

Test:

```powershell
dotnet test tests/ImageJCsharp.Core.Tests/ImageJCsharp.Core.Tests.csproj
```

Run:

```powershell
dotnet run --project src/ImageJCsharp.App/ImageJCsharp.App.csproj
```

## Contribution Workflow

1. Open or find an issue.
2. Comment that you want to work on it.
3. Create a branch from `main`.
4. Make a focused change.
5. Add or update tests when changing core behavior.
6. Run build and tests.
7. Open a pull request.

Recommended branch names:

- `feature/rectangle-roi-resize`
- `fix/threshold-edge-case`
- `docs/contributor-guide`
- `test/measurement-results`

## Pull Request Expectations

A good pull request should:

- Solve one focused problem.
- Explain what changed and why.
- Include tests for core behavior.
- Keep UI and core logic separated.
- Avoid unrelated refactoring.
- Mention known limitations.

## Coding Guidelines

- Keep image algorithms in `ImageJCsharp.Core`.
- Keep WinForms code focused on UI and interaction.
- Prefer small classes with clear responsibilities.
- Prefer simple, readable code over clever code.
- Add comments only when they clarify non-obvious behavior.
- Do not introduce large dependencies without discussion.
- Do not add plugin infrastructure before the command path is stable.

## Fast-Fail Development Principle

ImageJCsharp follows a fast-fail principle, especially during the MVP stage.

- Prefer simple, explicit behavior over broad fallback behavior.
- Do not add compatibility layers for unclear future requirements.
- Do not silently swallow errors that would help diagnose a problem.
- Avoid hidden automatic correction unless the requirement explicitly asks for it.
- If input is invalid, fail clearly with an understandable error.
- If a feature is not implemented, say so or leave it out rather than pretending partial support.
- Do not build generalized abstractions before repeated real needs appear.
- Do not add defensive complexity for imaginary callers.

This keeps bugs easier to reproduce and helps preserve trust in scientific image processing results.

## Testing Guidelines

Core behavior should be tested.

Important areas:

- Pixel buffer access.
- ROI bounds behavior.
- Measurement calculations.
- Threshold behavior.
- Filters and morphology.
- File-independent algorithm behavior.

UI tests are not required in the near-term MVP, but manual smoke testing is helpful.

## Reporting Bugs

Please include:

- What you tried.
- What happened.
- What you expected.
- Windows version.
- App version or commit.
- Image type if relevant.
- A small sample image if possible.

## Suggesting Features

Feature suggestions are welcome, especially when they are tied to real ImageJ workflows.

Please explain:

- Which ImageJ feature or workflow this replaces.
- Why it matters.
- Example input and expected result.
- Whether it is near-term, mid-term, or long-term scope.

## Community

Be kind, specific, and patient. Many contributors may come from different backgrounds: C# development, scientific imaging, biology, microscopy, teaching, or open-source learning.
