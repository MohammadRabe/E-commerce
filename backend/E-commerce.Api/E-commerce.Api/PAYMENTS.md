# Fawaterak payment setup

The API reads gateway settings from the `Fawaterak` configuration section. Keep the client secret out of source control. For local development, set .NET user secrets from the API project directory:

```powershell
dotnet user-secrets set "Fawaterak:ClientId" "<your-client-id>"
dotnet user-secrets set "Fawaterak:ClientSecret" "<your-client-secret>"
dotnet user-secrets set "Fawaterak:TokenUrl" "https://app.fawaterk.com/oauth/token"
dotnet user-secrets set "Fawaterak:ApiBaseUrl" "https://app.fawaterk.com"
dotnet user-secrets set "Fawaterak:FrontendBaseUrl" "http://localhost:5173"
```

Alternatively, configure `Fawaterak__ClientId`, `Fawaterak__ClientSecret`, `Fawaterak__TokenUrl`, `Fawaterak__ApiBaseUrl`, and `Fawaterak__FrontendBaseUrl` in the API process environment. Restart the API after changing them. Use the OAuth client credentials issued for the same Fawaterak account/environment as the transaction API. Do not use a merchant dashboard password or expose these values in frontend configuration.

The API now reports which settings are missing at startup. If the provider rejects a configured credential, the payment error includes the HTTP status and a bounded provider response for diagnosis.
