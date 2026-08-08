# Mock Umbraco test host

The test project uses Umbraco's official integration-test host with a disposable SQLite schema. It runs the package migration and verifies the configuration store against a real Umbraco scope and database.

`appsettings.Tests.json` contains only non-secret test settings. Local overrides remain ignored.
