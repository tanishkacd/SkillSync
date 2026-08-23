# SkillRadar — Reports & DevOps (Person 7 module)

This is the **Reports, Milestones, Notifications, and DevOps** slice of the SkillRadar
(Smart Employee Skill Matrix & Project Allocation Engine) project.

## What's in here
- `Controllers/ReportsController.cs` — availability report, profitability report, CSV export
- `Controllers/MilestonesController.cs` — milestone CRUD + progress updates
- `Controllers/NotificationsController.cs` — basic in-app notifications
- `Models/` — `Milestone`, `Notification` entities
- `Dtos/` — request/response shapes for the above
- `Dockerfile` + `docker-compose.yml` — one-command deployment (app + SQL Server)
- `.devcontainer/` — GitHub Codespaces config (dotnet SDK + Docker-in-Docker preinstalled)

> Note: report data is currently **stubbed with demo values** in `ReportsController`.
> Once Person 1+2 share the real reporting SQL views, and Person 6 finishes the
> Allocation/Timesheet tables, swap the stub arrays for real EF Core queries against
> those views.

## Running in GitHub Codespaces

1. Open this repo in a Codespace (Code → Codespaces → Create codespace). The
   devcontainer will automatically install the .NET 8 SDK and Docker.
2. Once the Codespace loads, restore packages (usually happens automatically, but just in case):
   ```bash
   dotnet restore
   ```
3. **Option A — run the app directly (fastest for coding/debugging):**
   ```bash
   dotnet run
   ```
   Then open the forwarded port (5000) → append `/swagger` to explore the API.
   Note: this needs a SQL Server instance reachable at `localhost,1433` — see Option B
   if you don't have one running yet.

4. **Option B — run everything with Docker (matches the "Done when: app spins up
   with one command" requirement):**
   ```bash
   docker compose up --build
   ```
   This starts both the SQL Server container and the app container together.
   Once healthy, open the forwarded port (8080) → `/swagger`.

5. To apply migrations (once you add your first one):
   ```bash
   dotnet tool install --global dotnet-ef   # first time only
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

## API endpoints (current stubs)

| Method | Route | Description |
|---|---|---|
| GET | `/api/reports/availability?start=&end=` | Employee availability % for a date range |
| GET | `/api/reports/profitability` | Budget vs cost vs margin per project |
| GET | `/api/reports/availability/export` | CSV download of availability report |
| GET | `/api/reports/profitability/export` | CSV download of profitability report |
| GET | `/api/milestones?projectId=` | List milestones (optionally filtered) |
| POST | `/api/milestones` | Create a milestone |
| PUT | `/api/milestones/{id}/progress` | Update % complete / status |
| GET | `/api/notifications?userId=` | List a user's notifications |
| PUT | `/api/notifications/{id}/read` | Mark a notification as read |

## Next steps for this module
- [ ] Replace demo arrays in `ReportsController` with real EF Core queries once shared schema lands
- [ ] Add PDF export (QuestPDF) alongside CSV, if time allows
- [ ] Wire notification creation triggers (milestone overdue, timesheet approved/rejected) once
      Person 6's timesheet workflow exists
- [ ] Merge `AppDbContext` with the team's shared DbContext once schema is finalized
