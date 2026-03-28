# Releasing

This project uses [EasyBuild.ShipIt](https://github.com/easybuild-org/EasyBuild.ShipIt)
for release automation and [Conventional Commits](https://www.conventionalcommits.org/)
for versioning.

## Commit conventions

PR titles must follow the conventional commit format:

| Prefix | Version bump | Example |
| --- | --- | --- |
| `feat:` | minor | `feat: add Beam logger provider` |
| `fix:` | patch | `fix: correct log level filtering` |
| `feat!:` | major | `feat!: rename ILogger interface` |
| `chore:` | patch | `chore: update dependencies` |
| `docs:` | patch | `docs: update README` |
| `refactor:` | patch | `refactor: simplify format translation` |

Other valid prefixes: `test`, `perf`, `ci`, `build`, `style`, `revert`.

## Creating a release

```bash
just shipit
```

This will:

1. Analyze commits since the last release
2. Determine the next semantic version
3. Update `CHANGELOG.md`
4. Create a GitHub release with the version tag (e.g. `v0.1.0`)

The GitHub release triggers the publish workflow, which:

1. Packs all NuGet packages (`Fable.Logging`, `Fable.Logging.Structlog`, etc.)
2. Pushes them to nuget.org using the `NUGET_API_KEY` secret

## Prerequisites

- `NUGET_API_KEY` repository secret (glob pattern: `Fable.Logging*`)
- `GITHUB_TOKEN` or `gh` CLI authenticated (for ShipIt to create releases)
