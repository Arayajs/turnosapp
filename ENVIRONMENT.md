# Environment Variables

## Required for production

| Variable | Description | Example |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Azure SQL connection string | `Server=tcp:...;Database=TurnOS;...` |
| `Jwt__Key` | HS256 secret — **min 32 chars** | `change-me-to-a-long-random-string` |
| `Jwt__Issuer` | Token issuer | `TurnOS` |
| `Jwt__Audience` | Token audience | `TurnOS` |
| `Jwt__ExpirationMinutes` | Access token TTL in minutes | `60` |
| `SendGrid__ApiKey` | SendGrid API key | `SG.xxx` |
| `SendGrid__FromEmail` | Sender address | `noreply@turnosapp.com` |
| `SendGrid__FromName` | Sender display name | `TurnOS` |

## GitHub Actions secrets (Settings → Secrets → Actions)

| Secret | Description |
|---|---|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | Download from Azure Portal → App Service → Get publish profile |

## Local development

Copy `appsettings.Development.json` and fill in your local SQL Server instance.  
Do **not** commit files with real credentials — they are listed in `.gitignore`.

## Azure App Service — application settings

Set each variable above in **Configuration → Application settings** (not in `appsettings.Production.json` directly, which contains `#{PLACEHOLDER}#` tokens).

## SendGrid setup

1. Create a free account at <https://sendgrid.com>
2. Go to **Settings → API Keys → Create API Key** (Mail Send permission)
3. Copy the key into the `SendGrid__ApiKey` application setting
4. Verify your sender domain to avoid spam filters
5. The `EmailService` will no-op (log a warning) if the key is not set, so the rest of the app works without it

## Refresh-token configuration

The refresh token lifetime is hardcoded to **7 days** (`AuthService.cs`).  
Adjust `RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)` to change it.
