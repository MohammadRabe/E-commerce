# Application logging

The API uses Serilog and currently writes events to the console. Error and Critical/Fatal events are emitted globally. The Fawaterak payment service also emits Information and Warning events so each payment attempt can be followed without enabling verbose logs across the rest of the API. Debug events remain filtered out.

For Azure App Service, enable **App Service logs → Application logging (Filesystem)** to collect console events, then inspect **Log stream**. Locally, the same events appear in the API process console.

## Log coverage

HTTP requests that end in a server error are logged with method, path, status, duration, and trace ID. MediatR requests log unexpected exceptions without serializing command data. The global exception handler records unhandled exceptions with the request trace ID. Payment logs cover create and verify attempts, order state, OAuth and gateway HTTP statuses, persistence, outcomes, elapsed time, and exceptions. They include the operation, order ID, and request trace ID, but exclude customer details, credentials, access tokens, checkout URLs, product data, amounts, and raw provider response bodies.

Passwords and request bodies are not included in application log events.
