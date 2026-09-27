# Application logging

The API uses Serilog with three outputs. Only Error and Critical/Fatal events are emitted; Warning, Information, and Debug events are filtered out before reaching any sink.

- Console, for local terminal output.
- Seq, for searchable structured logs and exception details.
- SQL Server, in the `dbo.LogEvents` table of the database in `ConnectionStrings:Default`.

The SQL sink creates `dbo.LogEvents` when it is missing. This is a logging table managed by the sink, not an EF migration. Seq keeps its own persistent event store in the Docker volume; the API sends each event independently to Seq and SQL Server.

## Start Seq locally

In PowerShell, set an administrator password for the first Seq startup and start the service:

```powershell
$env:SEQ_ADMIN_PASSWORD = "choose-a-strong-local-password"
docker compose -f docker-compose.logging.yml up -d
```

Open [http://localhost:5341](http://localhost:5341) and sign in as `admin` with that password. The Seq volume preserves its configuration and events after the container restarts.

The API defaults to `http://localhost:5341`. Override it with `Seq__ServerUrl`; set `Seq__ApiKey` if the Seq instance requires an ingestion API key. Start Seq before the API so local Seq ingestion is available immediately.

## Log coverage

HTTP requests that end in a server error are logged with method, path, status, duration, and trace ID. MediatR requests log unexpected exceptions without serializing command data. The global exception handler records unhandled exceptions with the request trace ID. Caught notification exceptions are logged as errors. Warnings, successful requests, validation failures, authentication outcomes, and expected business rejections are below the configured threshold and are not stored.

Passwords and request bodies are not included in application log events.
