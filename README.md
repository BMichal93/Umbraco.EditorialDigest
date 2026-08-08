# Umbraco.EditorialDigest

Scheduled editorial intelligence digests for Umbraco 13+.

## Install

```shell
dotnet add package Umbraco.EditorialDigest
```

The package creates its database schema during Umbraco startup. Administrators can access the initial package settings at **Settings > Editorial Digest > Global Settings**.

## Phase 1 screens

- **Settings tree**: an Editorial Digest tree in Settings with Digests and Global Settings nodes.
- **Global Settings**: default sender, logo URL, Razor template base path, dashboard refresh interval, package kill switch, and logging level.

The remaining digest configuration and delivery features are intentionally delivered in later phases.

## License and contributors

Licensed under the [MIT License](LICENSE). See [CONTRIBUTORS.md](CONTRIBUTORS.md) for project contributors.
