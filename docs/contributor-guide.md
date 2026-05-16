# Contributor Guide

This guide is for people who want to help ImageJCsharp but do not know where to start.

ImageJCsharp is a C# native ImageJ-like scientific image analysis application. The project welcomes both code and non-code contributions.

## Who Can Help?

You can help if you are:

- A C# developer.
- A WinForms developer.
- An ImageJ user.
- A microscopy or scientific imaging user.
- A tester.
- A documentation writer.
- A student looking for open-source tasks.

## Best First Tasks

Good first tasks should be small, understandable, and testable.

Examples:

- Improve README wording.
- Add screenshots.
- Add sample images for manual testing.
- Add tests for `GrayImage`.
- Add tests for measurement edge cases.
- Add a simple processing command.
- Improve error messages.
- Compare one ImageJ menu item against ImageJCsharp and document the gap.

## Project Map

```text
src/ImageJCsharp.Core
```

Core image model, ROI model, measurements, and algorithms. This is the best place for tested logic.

```text
src/ImageJCsharp.App
```

WinForms application shell, menus, dialogs, image display, and user interaction.

```text
tests/ImageJCsharp.Core.Tests
```

Automated tests for the core library.

```text
docs
```

Roadmap, planning, release process, and contributor documentation.

## Development Loop

1. Pick one small issue.
2. Create a branch.
3. Write or update tests if the change affects core behavior.
4. Change the code.
5. Run:

```powershell
dotnet build ImageJCsharp.sln
dotnet test tests/ImageJCsharp.Core.Tests/ImageJCsharp.Core.Tests.csproj
```

6. Open a pull request.

## How To Choose Work

Prefer work in this order:

1. Fix bugs that block basic workflows.
2. Improve tested core behavior.
3. Add small, common ImageJ features.
4. Improve documentation and examples.
5. Refactor only when it directly supports a feature or fix.

## How To Discuss ImageJ Compatibility

When proposing ImageJ-like behavior, include:

- ImageJ version or distribution used for comparison.
- Menu path or tool name.
- A small example image or described input.
- Expected result.
- Whether exact behavior matters for scientific reproducibility.

## Labels To Use On GitHub

Suggested labels:

- `good first issue`
- `help wanted`
- `bug`
- `enhancement`
- `documentation`
- `imagej-compatibility`
- `core`
- `ui`
- `testing`
- `release`

## Maintainer Notes

For maintainers, good issues should include:

- A clear title.
- Expected behavior.
- Current behavior.
- A small scope.
- A suggested file or module if known.
- A note if the issue is suitable for first-time contributors.
