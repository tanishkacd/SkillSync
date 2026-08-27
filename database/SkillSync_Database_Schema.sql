IF DB_ID('SkillSyncDB') IS NULL
    CREATE DATABASE SkillSyncDB;
GO

USE SkillSyncDB;
GO
/* 
   1.MASTER DATA
 */
 
CREATE TABLE Department (
    DepartmentID    INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(100)   NOT NULL,
    IsActive        BIT             NOT NULL DEFAULT 1
);
 
CREATE TABLE SkillCategory (
    SkillCategoryID INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(100)   NOT NULL   -- e.g. 'Programming Languages', 'Databases', 'Cloud'
);
 
CREATE TABLE Skill (
    SkillID         INT IDENTITY(1,1) PRIMARY KEY,
    SkillCategoryID INT             NOT NULL REFERENCES SkillCategory(SkillCategoryID),
    Name            NVARCHAR(100)   NOT NULL   -- e.g. 'C#', 'SQL Server', 'Kubernetes'
);
 
CREATE TABLE Certification (
    CertificationID     INT IDENTITY(1,1) PRIMARY KEY,
    Name                NVARCHAR(150)   NOT NULL,   -- e.g. 'Microsoft Certified: Azure Developer'
    IssuingBody         NVARCHAR(150)   NULL,
    ValidityPeriodMonths INT            NULL        -- NULL = does not expire
);

 
/* 2. EMPLOYEE & SKILL PROFILE */
 
CREATE TABLE Employee (
    EmployeeID          INT IDENTITY(1,1) PRIMARY KEY,
    FirstName            NVARCHAR(80)    NOT NULL,
    LastName             NVARCHAR(80)    NOT NULL,
    Email                NVARCHAR(150)   NOT NULL UNIQUE,
    DepartmentID         INT             NOT NULL REFERENCES Department(DepartmentID),
    JobTitle             NVARCHAR(100)   NULL,
    HireDate             DATE            NOT NULL,
    WeeklyCapacityHours  DECIMAL(5,2)    NOT NULL DEFAULT 40.00,   -- standard capacity per week
    CostRatePerHour      DECIMAL(10,2)   NOT NULL,                  -- internal cost, used in profitability
    IsActive             BIT             NOT NULL DEFAULT 1,
    IdentityUserId       NVARCHAR(450)   NULL                       -- links to AspNetUsers.Id (ASP.NET Identity); nullable because an
                                                                     -- Employee record can exist (e.g. HR bulk import) before the
                                                                     -- person has registered/logged in for the first time
);
 
/* One Identity account maps to exactly one Employee, and vice versa */
ALTER TABLE Employee ADD CONSTRAINT UQ_Employee_IdentityUserId UNIQUE (IdentityUserId);
 
/* ---------------------------------------------------------------------------
   NOTE ON SEQUENCING: the FK below references AspNetUsers, which is created
   by ASP.NET Core Identity's own EF Core migration — not by this script.
   Run this ALTER only AFTER the Identity migration has been applied and the
   AspNetUsers table exists (i.e., after `dotnet ef database update` for the
   Identity migration, or after Visual Studio's Identity scaffolding runs).
 
   ALTER TABLE Employee ADD CONSTRAINT FK_Employee_AspNetUsers
       FOREIGN KEY (IdentityUserId) REFERENCES AspNetUsers(Id);
   --------------------------------------------------------------------------- */
 
CREATE TABLE EmployeeSkill (
    EmployeeSkillID     INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeID          INT             NOT NULL REFERENCES Employee(EmployeeID),
    SkillID             INT             NOT NULL REFERENCES Skill(SkillID),
    ProficiencyScore    TINYINT         NOT NULL CHECK (ProficiencyScore BETWEEN 0 AND 5),
    YearsExperience     DECIMAL(4,1)    NOT NULL DEFAULT 0,
    LastAssessedDate    DATE            NOT NULL,
    AssessedBy          INT             NULL REFERENCES Employee(EmployeeID),  -- HR admin / manager
    CONSTRAINT UQ_EmployeeSkill UNIQUE (EmployeeID, SkillID)
);
 
CREATE TABLE EmployeeCertification (
    EmployeeCertificationID INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeID              INT           NOT NULL REFERENCES Employee(EmployeeID),
    CertificationID         INT           NOT NULL REFERENCES Certification(CertificationID),
    IssueDate                DATE          NOT NULL,
    ExpiryDate               DATE          NULL,
    VerifiedBy               INT           NULL REFERENCES Employee(EmployeeID),
    CONSTRAINT UQ_EmployeeCertification UNIQUE (EmployeeID, CertificationID, IssueDate)
);
 
/* 3. PROJECTS, REQUIREMENTS & ALLOCATION (Automated Team Builder) */
 
CREATE TABLE Project (
    ProjectID           INT IDENTITY(1,1) PRIMARY KEY,
    Name                 NVARCHAR(150)   NOT NULL,
    ClientName            NVARCHAR(150)   NULL,
    ProjectManagerID      INT             NOT NULL REFERENCES Employee(EmployeeID),
    StartDate             DATE            NOT NULL,
    EndDate                DATE            NULL,
    Status                 NVARCHAR(20)    NOT NULL DEFAULT 'Planning', -- Planning/Active/Completed/OnHold
    BudgetAmount           DECIMAL(14,2)   NULL,
    BillingRatePerHour     DECIMAL(10,2)   NULL       -- default client billing rate for revenue calc
);
 
CREATE TABLE ProjectRequirement (
    ProjectRequirementID  INT IDENTITY(1,1) PRIMARY KEY,
    ProjectID              INT             NOT NULL REFERENCES Project(ProjectID),
    SkillID                 INT             NOT NULL REFERENCES Skill(SkillID),
    MinProficiency           TINYINT         NOT NULL CHECK (MinProficiency BETWEEN 0 AND 5),
    MinYearsExperience       DECIMAL(4,1)    NOT NULL DEFAULT 0,
    HeadcountNeeded           INT             NOT NULL CHECK (HeadcountNeeded > 0)
);
 
CREATE TABLE Allocation (
    AllocationID           INT IDENTITY(1,1) PRIMARY KEY,
    ProjectID               INT             NOT NULL REFERENCES Project(ProjectID),
    EmployeeID               INT             NOT NULL REFERENCES Employee(EmployeeID),
    ProjectRequirementID      INT             NULL REFERENCES ProjectRequirement(ProjectRequirementID),
    AllocationPercent          DECIMAL(5,2)    NOT NULL CHECK (AllocationPercent > 0 AND AllocationPercent <= 100),
    StartDate                   DATE            NOT NULL,
    EndDate                      DATE            NULL,
    Status                        NVARCHAR(20)    NOT NULL DEFAULT 'Proposed', -- Proposed/Locked/Completed/Cancelled
    CreatedBy                     INT             NOT NULL REFERENCES Employee(EmployeeID),
    CreatedDate                    DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME()
);
 
/* Prevents an employee being locked beyond 100% capacity across overlapping allocations
   — enforced in application/service layer (C# Matching Engine) because it requires
   evaluating overlapping date ranges; a supporting index is added below. */
CREATE INDEX IX_Allocation_Employee_Dates ON Allocation (EmployeeID, StartDate, EndDate) INCLUDE (AllocationPercent, Status);
 
/* 4. MILESTONES, TASKS & TIMESHEETS */
 
CREATE TABLE Milestone (
    MilestoneID     INT IDENTITY(1,1) PRIMARY KEY,
    ProjectID        INT             NOT NULL REFERENCES Project(ProjectID),
    Name              NVARCHAR(150)   NOT NULL,
    DueDate            DATE            NOT NULL,
    Status              NVARCHAR(20)    NOT NULL DEFAULT 'Not Started' -- Not Started/In Progress/Completed
);
 
CREATE TABLE Task (
    TaskID          INT IDENTITY(1,1) PRIMARY KEY,
    MilestoneID      INT             NOT NULL REFERENCES Milestone(MilestoneID),
    Name              NVARCHAR(150)   NOT NULL,
    Description        NVARCHAR(MAX)   NULL,
    Status              NVARCHAR(20)    NOT NULL DEFAULT 'Not Started'
);
 
CREATE TABLE Timesheet (
    TimesheetID     INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeID       INT             NOT NULL REFERENCES Employee(EmployeeID),
    WeekStartDate     DATE            NOT NULL,   -- Monday of the reporting week
    Status              NVARCHAR(20)    NOT NULL DEFAULT 'Draft', -- Draft/Submitted/Approved/Rejected
    ApprovedBy           INT             NULL REFERENCES Employee(EmployeeID),
    ApprovedDate           DATETIME2       NULL,
    CONSTRAINT UQ_Timesheet_Week UNIQUE (EmployeeID, WeekStartDate)
);
 
CREATE TABLE TimesheetEntry (
    TimesheetEntryID  INT IDENTITY(1,1) PRIMARY KEY,
    TimesheetID         INT             NOT NULL REFERENCES Timesheet(TimesheetID),
    ProjectID             INT             NOT NULL REFERENCES Project(ProjectID),
    TaskID                 INT             NULL REFERENCES Task(TaskID),
    EntryDate               DATE            NOT NULL,
    HoursWorked              DECIMAL(4,2)    NOT NULL CHECK (HoursWorked >= 0 AND HoursWorked <= 24),
    Notes                     NVARCHAR(500)   NULL
);
 
/* Enforce <= 168 hours per employee per week at the application layer
   (aggregate check across TimesheetEntry rows joined to Timesheet). */
CREATE INDEX IX_TimesheetEntry_Timesheet ON TimesheetEntry (TimesheetID);
 
 
/*  ---------------------------------------------------------------------------
   5. BILLING & AUDIT
   --------------------------------------------------------------------------- */
 
CREATE TABLE RateCard (
    RateCardID       INT IDENTITY(1,1) PRIMARY KEY,
    ProjectID          INT             NOT NULL REFERENCES Project(ProjectID),
    BillingRatePerHour   DECIMAL(10,2)   NOT NULL,
    EffectiveDate         DATE            NOT NULL
);
 
CREATE TABLE AuditLog (
    AuditLogID     INT IDENTITY(1,1) PRIMARY KEY,
    EntityName       NVARCHAR(50)    NOT NULL,   -- e.g. 'EmployeeSkill', 'Allocation'
    EntityID           INT             NOT NULL,
    ChangedField         NVARCHAR(100)   NOT NULL,
    OldValue               NVARCHAR(200)   NULL,
    NewValue                 NVARCHAR(200)   NULL,
    ChangedBy                 INT             NOT NULL REFERENCES Employee(EmployeeID),
    ChangedDate                 DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME()
);
 
GO
 
/* 6. REPORTING VIEWS  (Database Highlights: complex joins, aggregation) */
 
/* 6.1 Team Skill Radar: average proficiency per department per skill */
CREATE VIEW vw_TeamSkillRadar AS
SELECT
    d.DepartmentID,
    d.Name                          AS DepartmentName,
    s.SkillID,
    s.Name                          AS SkillName,
    sc.Name                         AS SkillCategory,
    AVG(CAST(es.ProficiencyScore AS DECIMAL(4,2))) AS AvgProficiency,
    COUNT(DISTINCT es.EmployeeID)   AS EmployeeCount
FROM EmployeeSkill es
JOIN Employee e        ON e.EmployeeID = es.EmployeeID AND e.IsActive = 1
JOIN Department d      ON d.DepartmentID = e.DepartmentID
JOIN Skill s            ON s.SkillID = es.SkillID
JOIN SkillCategory sc   ON sc.SkillCategoryID = s.SkillCategoryID
GROUP BY d.DepartmentID, d.Name, s.SkillID, s.Name, sc.Name;
GO
 
/* 6.2 Resource Availability: weekly capacity vs. locked allocations */
CREATE VIEW vw_ResourceAvailability AS
SELECT
    e.EmployeeID,
    e.FirstName + ' ' + e.LastName          AS EmployeeName,
    e.DepartmentID,
    e.WeeklyCapacityHours,
    ISNULL(SUM(a.AllocationPercent), 0)      AS AllocatedPercent,
    e.WeeklyCapacityHours
        * (1 - ISNULL(SUM(a.AllocationPercent), 0) / 100.0)  AS AvailableHoursPerWeek,
    CASE
        WHEN ISNULL(SUM(a.AllocationPercent), 0) >= 100 THEN 0
        ELSE 100 - ISNULL(SUM(a.AllocationPercent), 0)
    END                                       AS AvailabilityPercent
FROM Employee e
LEFT JOIN Allocation a
    ON a.EmployeeID = e.EmployeeID
    AND a.Status = 'Locked'
    AND GETDATE() BETWEEN a.StartDate AND ISNULL(a.EndDate, '9999-12-31')
WHERE e.IsActive = 1
GROUP BY e.EmployeeID, e.FirstName, e.LastName, e.DepartmentID, e.WeeklyCapacityHours;
GO
 
/* 6.3 Candidate Match: scores employees against a project's requirement
           line — consumed by the C# Automated Team Builder engine */
CREATE VIEW vw_CandidateMatch AS
SELECT
    pr.ProjectRequirementID,
    pr.ProjectID,
    es.EmployeeID,
    pr.SkillID,
    es.ProficiencyScore,
    pr.MinProficiency,
    es.YearsExperience,
    pr.MinYearsExperience,
    ra.AvailabilityPercent,
    -- simple weighted match score: proficiency fit (50%), experience fit (20%), availability (30%)
    CASE WHEN es.ProficiencyScore >= pr.MinProficiency
              AND es.YearsExperience >= pr.MinYearsExperience
         THEN
            (CAST(es.ProficiencyScore AS DECIMAL(5,2)) / 5.0) * 50
            + (CASE WHEN es.YearsExperience >= pr.MinYearsExperience * 2 THEN 20
                    ELSE (es.YearsExperience / NULLIF(pr.MinYearsExperience, 0)) * 20 END)
            + (ra.AvailabilityPercent / 100.0) * 30
         ELSE 0
    END                                       AS MatchScore
FROM ProjectRequirement pr
JOIN EmployeeSkill es       ON es.SkillID = pr.SkillID
JOIN vw_ResourceAvailability ra ON ra.EmployeeID = es.EmployeeID
WHERE ra.AvailabilityPercent > 0;
GO
 
/* 6.4 Project Profitability: billed revenue vs. actual cost  */
CREATE VIEW vw_ProjectProfitability AS
SELECT
    p.ProjectID,
    p.Name                                    AS ProjectName,
    p.ClientName,
    SUM(te.HoursWorked)                       AS ActualHours,
    SUM(te.HoursWorked * ISNULL(rc.BillingRatePerHour, p.BillingRatePerHour)) AS BilledRevenue,
    SUM(te.HoursWorked * e.CostRatePerHour)   AS ActualCost,
    SUM(te.HoursWorked * ISNULL(rc.BillingRatePerHour, p.BillingRatePerHour))
        - SUM(te.HoursWorked * e.CostRatePerHour)                            AS Profitability
FROM Project p
JOIN TimesheetEntry te   ON te.ProjectID = p.ProjectID
JOIN Timesheet ts         ON ts.TimesheetID = te.TimesheetID AND ts.Status = 'Approved'
JOIN Employee e            ON e.EmployeeID = ts.EmployeeID
LEFT JOIN RateCard rc       ON rc.ProjectID = p.ProjectID
                            AND rc.EffectiveDate = (
                                SELECT MAX(rc2.EffectiveDate) FROM RateCard rc2
                                WHERE rc2.ProjectID = p.ProjectID AND rc2.EffectiveDate <= te.EntryDate)
GROUP BY p.ProjectID, p.Name, p.ClientName;
GO
 
/* 6.5 Milestone Progress: rollup of task completion per milestone */
CREATE VIEW vw_MilestoneProgress AS
SELECT
    m.MilestoneID,
    m.ProjectID,
    m.Name                                     AS MilestoneName,
    m.DueDate,
    COUNT(t.TaskID)                             AS TotalTasks,
    SUM(CASE WHEN t.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedTasks,
    CAST(SUM(CASE WHEN t.Status = 'Completed' THEN 1 ELSE 0 END) AS DECIMAL(5,2))
        / NULLIF(COUNT(t.TaskID), 0) * 100      AS PercentComplete
FROM Milestone m
LEFT JOIN Task t ON t.MilestoneID = m.MilestoneID
GROUP BY m.MilestoneID, m.ProjectID, m.Name, m.DueDate;
GO
 
