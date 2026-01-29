# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction
- If First-chance exceptions are displayed in Visual Studio but do not affect the application, do not address them; treat them as Visual Studio behavior to be ignored.

## Logging Guidelines
- Keep Azure App Service Linux F1 logging to 7 days and do not use Application Insights.
- Record persistent logs on Azure App Service Linux F1 by writing Console errors to `docker.log` and grepping `/home/LogFiles/*docker.log`.
- In production, avoid `AddDebug` outside of development and instead record Console errors to `docker.log` and grep `/home/LogFiles/*docker.log`.
- On Windows/IIS, use EventLog via `builder.Logging.AddEventLog()`.
- Prefer minimal persistent logging (Error only) and maintain the current minimal `Program.cs` (with startup try/catch and AppDomain/TaskScheduler handlers) as-is.

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Project-Specific Rules
