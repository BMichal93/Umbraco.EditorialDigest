# Mock Umbraco test host

The next validation phase will add an isolated Umbraco host backed by a disposable database. It will run the package migration and exercise the Settings API through authenticated administrator requests.

The configuration store has no HTTP or static Umbraco dependencies and receives `IScopeProvider` through DI, so the host can replace the database and scope implementation without changing production code.
