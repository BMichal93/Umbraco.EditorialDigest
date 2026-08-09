# Mock Umbraco sites

The repository contains two deterministic local Umbraco hosts for package validation:

- `test/MockUmbraco17` uses Umbraco 17.6.
- `test/MockUmbraco18` uses Umbraco 18.1.

Both compile the package source directly and include an idempotent package migration that creates an `Editorial Page` document type and these test pages:

- Editorial Home
- Recently Published Release Notes
- Content Standards
- Draft Pending Review

No credentials are committed. The start script reads credentials from environment variables or prompts for them, then keeps them only for the child `dotnet run` process.

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Start-MockUmbraco.ps1 -Version 18
```

Open `https://localhost:18443/umbraco` and sign in with the values supplied to the script. Use `-Version 17` for the 17.6 host on port `17443`.

If the .NET development certificate is not trusted, run `dotnet dev-certs https --trust` once.

To discard the local database and seed a fresh site on the next start:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Reset-MockUmbraco.ps1 -Version 18
```

The databases, logs, runtime files, media, build outputs, and local environment files are excluded from Git.
