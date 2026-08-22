# SkillSync Backend Starter

This starter is based on the SkillSync Requirement Analysis Report.

## Scope
Person 3 + 4:
- ASP.NET Core backend foundation
- EF Core + SQL Server
- ASP.NET Core Identity
- Six roles:
  - Employee
  - Project Manager
  - Resource Manager
  - HR Administrator
  - Finance / Operations
  - System Administrator
- SkillCategory
- Skill
- EmployeeSkill
- Certification
- EmployeeCertification
- Skill APIs
- HR skill scoring
- Team radar aggregation API

## Important
This is a starter scaffold. Integrate it into the team's main SkillSync project and coordinate entity/table names with Person 1 + 2's database schema before creating final migrations.

## Suggested next commands
dotnet restore
dotnet build
dotnet ef migrations add InitialSkillSync
dotnet ef database update
dotnet run
