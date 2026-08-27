# SkillSync

### Smart Employee Skill Matrix & Project Allocation Engine

SkillSync is an internal HR-tech portal that maps employee skill sets, certifications, and past performance to assign them to new company projects efficiently. It combines a skill-tracking dashboard, an automated team-builder/matching engine, and project allocation + timesheet workflows into a single ASP.NET Core application backed by SQL Server.

---

## Key Features

- **Skill Gap & Radar Profiling** — Interactive dashboard showing team competencies and individual skill scores.
- **Automated Team Builder** — Project Managers input project requirements (e.g. "2 C# devs, 1 SQL DBA with 3+ years experience"), and a matching engine recommends the optimal available candidates based on proficiency, experience, and availability.
- **Project Allocation & Timesheet Tracking** — Employees are locked to projects with capacity/double-booking checks; weekly timesheets are submitted and approved against those allocations.
- **Progress, Reports & Notifications** — Milestone tracking, resource availability reports, and project profitability reports (with CSV export), plus in-app notifications.
- **Database-Driven Reporting** — Complex SQL joins, views, and aggregation functions calculate resource availability percentages and project profitability directly in SQL Server.

---

## Tech Stack

- **Backend:** ASP.NET Core (C#), Entity Framework Core
- **Auth:** ASP.NET Core Identity (6 roles)
- **Database:** SQL Server (schema + 5 reporting views, seeded via `database/init.sh`)
- **DevOps:** Docker, Docker Compose
- **API Docs:** Swagger / OpenAPI

---

## Team

**Yashika Changrani (IN26010907)** and **Ashutosh Khandelwal (IN26011511)** — Database: schema, keys, relationships, 5 reporting views, seed/demo data script, ERD.

**Tanishi Dubey (IN26012278)** and **Tanishka Shandilya (IN26011009)** — Backend + Auth + Skill Profiling: ASP.NET Core project setup, EF Core, Identity with 6 roles, login/logout, CSRF handling, skill categories/skills endpoints, employee skill scores, certifications, team-level skill radar aggregation.

**Vaibhavi Sachin Pathak (IN26011069)** — Team Builder: matching engine that ranks candidates by proficiency, experience, and availability.

**Manya Khandelwal (IN26011791)** — Project Allocation + Timesheet: requirement CRUD, locking allocations, double-booking prevention, weekly timesheet entry/approval workflow with hour caps.

**Riya Agarwal (IN26011166)** — Progress + Reports + DevOps: milestone tracking, availability/profitability reports with export, notifications, Docker deployment.

---

## Definition of Done, per module

- **Database:** schema is complete and all 5 reporting views return correct results against seeded data.
- **Backend + Auth + Skill Profiling:** every role logs in with correct access control, **and** an HR Admin can score skills while a Resource Manager views the radar.
- **Team Builder:** the ranked candidate list correctly excludes underqualified or fully-booked people.
- **Allocation + Timesheet:** a PM can lock a candidate to a project, and an employee's timesheet can be approved or rejected.
- **Progress + Reports + DevOps:** reports return correct figures, and the app spins up with a single command (`docker compose up`).

---

## Running the Project

**Run locally:**
```bash
cd src/SkillSync.Reports
dotnet restore
dotnet run
```
Then open the printed localhost URL in your browser and append `/swagger`.

**Run with Docker Compose (app + SQL Server together):**
```bash
cd src/SkillSync.Reports
docker compose up --build
```
Then visit `http://localhost:8080/swagger`.

**Seeding demo data:** run `database/SkillSync_Database_Schema.sql` first, then `database/init.sh` (or the provided seed script) to populate sample employees, projects, allocations, and timesheets so reports return real figures.

---

## API Overview (current)

| Module | Sample Endpoints |
|---|---|
| Reports | `GET /api/reports/availability`, `GET /api/reports/profitability`, `.../export` |
| Milestones | `GET /api/milestones`, `POST /api/milestones`, `PUT /api/milestones/{id}/status` |
| Notifications | `GET /api/notifications`, `PUT /api/notifications/{id}/read` |
| Allocations | `POST /api/allocations`, `PUT /api/allocations/{id}`, `PUT /api/allocations/{id}/lock` |
| Project Requirements | `POST /api/projects/{projectId}/requirements`, `GET .../requirements` |

Full interactive documentation is available at `/swagger` once the app is running.
