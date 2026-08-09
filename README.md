# Umbraco.EditorialDigest

Scheduled editorial intelligence digests for Umbraco 17 and 18, built on .NET 10.

## Install

```shell
dotnet add package Umbraco.EditorialDigest
```

The package creates its database schema during Umbraco startup. Administrators manage digests from **Settings > Editorial Digest**. Editorial activity is available at **Content > Editorial Overview**.

## Backoffice screens

- **Editorial Digest**: create and manage digest configurations and global delivery settings.
- **Editorial Overview**: recently published, pending review, stale content, and active digest status.

The package uses the Umbraco 14+ extension manifest and Management API model. It does not ship deprecated AngularJS assets or legacy `package.manifest` files.

## Local acceptance site

Create a disposable Umbraco 18 SQLite site with the locally packed package:

```powershell
pwsh ./tools/New-LocalAcceptanceSite.ps1
dotnet run --project ./samples/EditorialDigest.TestSite/EditorialDigest.TestSite.csproj
```

Finish installation in the browser and create the administrator directly in the installer. The generated site, database, and credentials are ignored by Git.

## Mock Umbraco sites

For repeatable local package development against Umbraco 17.6 and 18.1, use the tracked mock hosts and deterministic seed migration described in [docs/MockUmbraco.md](docs/MockUmbraco.md).

## License and contributors

Licensed under the [MIT License](LICENSE). See [CONTRIBUTORS.md](CONTRIBUTORS.md) for project contributors.
