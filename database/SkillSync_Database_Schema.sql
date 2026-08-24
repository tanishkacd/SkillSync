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
 
 
/* ---------------------------------------------------------------------------
