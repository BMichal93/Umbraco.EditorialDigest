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

## License and contributors

Licensed under the [MIT License](LICENSE). See [CONTRIBUTORS.md](CONTRIBUTORS.md) for project contributors.
