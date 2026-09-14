# Configuration contracts

`storage.example.json` is a provider-neutral, **not yet consumed** configuration contract for the future evidence slice. Values describe policy choices for review, not functioning upload validation. `Container` is a logical private container/bucket; `ServiceEndpoint` is optional for a future compatible service. No provider, key or credential is selected here. The AWS adapter will use workload identity, not access keys in this file.

The future adapter must fail startup for enabled-but-incomplete configuration and reject unsupported constraints. Application services authorize organization ownership before generating opaque object IDs or requesting grants. Never serialize grant records into logs: they contain bearer capabilities. Size/type policy must also be verified after upload; a declared Content-Type does not establish actual content.

API configuration is in appsettings.json and may be overridden with standard double-underscore environment variables. Defaults have identity disabled. The frontend public OIDC example lives at `src/frontend/identity.config.example.json`. Neither example contains real credentials. Real local environment files are ignored.
