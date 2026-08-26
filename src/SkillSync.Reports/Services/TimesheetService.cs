using Microsoft.EntityFrameworkCore;
using SkillRadarReports.Data;
using SkillRadarReports.Dtos;
using SkillRadarReports.Models;

namespace SkillRadarReports.Services;

public class TimesheetService : ITimesheetService
{
    private readonly AppDbContext _db;

    public TimesheetService(AppDbContext db)
    {
        _db = db;
    }

    public static DateTime GetMondayOfWeek(DateTime date)
    {
        var d = date.Date;
        int diff = (int)d.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0)
        {
            diff += 7;
        }
        return d.AddDays(-diff);
    }

    public async Task<TimesheetDto> GetOrInitializeTimesheetAsync(int employeeId, DateTime weekStartDate)
    {
        var monday = GetMondayOfWeek(weekStartDate);

        var employeeExists = await _db.Employees.AnyAsync(e => e.EmployeeID == employeeId);
        if (!employeeExists)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");
        }

        var timesheet = await _db.Timesheets
            .Include(t => t.Employee)
            .Include(t => t.Approver)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Task)
            .FirstOrDefaultAsync(t => t.EmployeeID == employeeId && t.WeekStartDate == monday);

        if (timesheet == null)
        {
            timesheet = new Timesheet
            {
                EmployeeID = employeeId,
                WeekStartDate = monday,
                Status = "Draft"
            };

            _db.Timesheets.Add(timesheet);
            await _db.SaveChangesAsync();

            timesheet = await _db.Timesheets
                .Include(t => t.Employee)
                .Include(t => t.Approver)
                .Include(t => t.Entries)
                    .ThenInclude(e => e.Project)
                .Include(t => t.Entries)
                    .ThenInclude(e => e.Task)
                .FirstAsync(t => t.TimesheetID == timesheet.TimesheetID);
        }

        return MapToDto(timesheet);
    }

    public async Task<TimesheetDto> SaveTimesheetAsync(SaveTimesheetDto dto)
    {
        var monday = GetMondayOfWeek(dto.WeekStartDate);

        var employeeExists = await _db.Employees.AnyAsync(e => e.EmployeeID == dto.EmployeeID);
        if (!employeeExists)
        {
            throw new KeyNotFoundException($"Employee with ID {dto.EmployeeID} not found.");
        }

        // 1. Validate entries: HoursWorked between 0 and 24
        foreach (var entryDto in dto.Entries)
        {
            if (entryDto.HoursWorked < 0 || entryDto.HoursWorked > 24)
            {
                throw new ArgumentException($"Hours worked per entry must be between 0 and 24. Received {entryDto.HoursWorked} for entry on {entryDto.EntryDate:yyyy-MM-dd}.");
            }

            var projectExists = await _db.Projects.AnyAsync(p => p.ProjectID == entryDto.ProjectID);
            if (!projectExists)
            {
                throw new KeyNotFoundException($"Project with ID {entryDto.ProjectID} not found.");
            }

            if (entryDto.TaskID.HasValue)
            {
                var taskExists = await _db.Tasks.AnyAsync(t => t.TaskID == entryDto.TaskID.Value);
                if (!taskExists)
                {
                    throw new KeyNotFoundException($"Task with ID {entryDto.TaskID.Value} not found.");
                }
            }
        }

        // 2. Validate aggregate total weekly hours <= 168
        var totalHours = dto.Entries.Sum(e => e.HoursWorked);
        if (totalHours > 168.00m)
        {
            throw new ArgumentException($"Total weekly hours cannot exceed 168 hours. Attempted to log {totalHours} hours.");
        }

        var timesheet = await _db.Timesheets
            .Include(t => t.Entries)
            .FirstOrDefaultAsync(t => t.EmployeeID == dto.EmployeeID && t.WeekStartDate == monday);

        if (timesheet == null)
        {
            timesheet = new Timesheet
            {
                EmployeeID = dto.EmployeeID,
                WeekStartDate = monday,
                Status = "Draft"
            };
            _db.Timesheets.Add(timesheet);
            await _db.SaveChangesAsync();
        }
        else
        {
            if (timesheet.Status == "Submitted" || timesheet.Status == "Approved")
            {
                throw new InvalidOperationException($"Timesheet is in '{timesheet.Status}' status and cannot be modified.");
            }
        }

        // Clear existing entries and replace with updated list
        _db.TimesheetEntries.RemoveRange(timesheet.Entries);
        timesheet.Entries.Clear();

        foreach (var entryDto in dto.Entries)
        {
            timesheet.Entries.Add(new TimesheetEntry
            {
                TimesheetID = timesheet.TimesheetID,
                ProjectID = entryDto.ProjectID,
                TaskID = entryDto.TaskID,
                EntryDate = entryDto.EntryDate.Date,
                HoursWorked = entryDto.HoursWorked,
                Notes = entryDto.Notes
            });
        }

        await _db.SaveChangesAsync();

        var reloadedTimesheet = await _db.Timesheets
            .Include(t => t.Employee)
            .Include(t => t.Approver)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Task)
            .FirstAsync(t => t.TimesheetID == timesheet.TimesheetID);

        return MapToDto(reloadedTimesheet);
    }

    public async Task<TimesheetDto> SubmitTimesheetAsync(int timesheetId)
    {
        var timesheet = await _db.Timesheets
            .Include(t => t.Employee)
            .Include(t => t.Approver)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Task)
            .FirstOrDefaultAsync(t => t.TimesheetID == timesheetId);

        if (timesheet == null)
        {
            throw new KeyNotFoundException($"Timesheet with ID {timesheetId} not found.");
        }

        if (timesheet.Status != "Draft" && timesheet.Status != "Rejected")
        {
            throw new InvalidOperationException($"Only Draft or Rejected timesheets can be submitted. Current status: '{timesheet.Status}'.");
        }

        timesheet.Status = "Submitted";
        await _db.SaveChangesAsync();

        return MapToDto(timesheet);
    }

    public async Task<TimesheetDto> ApproveTimesheetAsync(int timesheetId, int managerEmployeeId)
    {
        var timesheet = await _db.Timesheets
            .Include(t => t.Employee)
            .Include(t => t.Approver)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Task)
            .FirstOrDefaultAsync(t => t.TimesheetID == timesheetId);

        if (timesheet == null)
        {
            throw new KeyNotFoundException($"Timesheet with ID {timesheetId} not found.");
        }

        var managerExists = await _db.Employees.AnyAsync(e => e.EmployeeID == managerEmployeeId);
        if (!managerExists)
        {
            throw new KeyNotFoundException($"Manager Employee with ID {managerEmployeeId} not found.");
        }

        if (timesheet.Status != "Submitted")
        {
            throw new InvalidOperationException($"Only Submitted timesheets can be approved. Current status: '{timesheet.Status}'.");
        }

        timesheet.Status = "Approved";
        timesheet.ApprovedBy = managerEmployeeId;
        timesheet.ApprovedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var reloadedTimesheet = await _db.Timesheets
            .Include(t => t.Employee)
            .Include(t => t.Approver)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Task)
            .FirstAsync(t => t.TimesheetID == timesheetId);

        return MapToDto(reloadedTimesheet);
    }

    public async Task<TimesheetDto> RejectTimesheetAsync(int timesheetId, int managerEmployeeId, string reason)
    {
        var timesheet = await _db.Timesheets
            .Include(t => t.Employee)
            .Include(t => t.Approver)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Project)
            .Include(t => t.Entries)
                .ThenInclude(e => e.Task)
            .FirstOrDefaultAsync(t => t.TimesheetID == timesheetId);

        if (timesheet == null)
        {
            throw new KeyNotFoundException($"Timesheet with ID {timesheetId} not found.");
        }

        var managerExists = await _db.Employees.AnyAsync(e => e.EmployeeID == managerEmployeeId);
        if (!managerExists)
        {
            throw new KeyNotFoundException($"Manager Employee with ID {managerEmployeeId} not found.");
        }

        if (timesheet.Status != "Submitted")
        {
            throw new InvalidOperationException($"Only Submitted timesheets can be rejected. Current status: '{timesheet.Status}'.");
        }

        timesheet.Status = "Rejected";
        await _db.SaveChangesAsync();

        return MapToDto(timesheet);
    }

    private static TimesheetDto MapToDto(Timesheet t)
    {
        return new TimesheetDto
        {
            TimesheetID = t.TimesheetID,
            EmployeeID = t.EmployeeID,
            EmployeeName = t.Employee != null ? $"{t.Employee.FirstName} {t.Employee.LastName}" : null,
            WeekStartDate = t.WeekStartDate,
            Status = t.Status,
            ApprovedBy = t.ApprovedBy,
            ApprovedByName = t.Approver != null ? $"{t.Approver.FirstName} {t.Approver.LastName}" : null,
            ApprovedDate = t.ApprovedDate,
            TotalHours = t.Entries?.Sum(e => e.HoursWorked) ?? 0.00m,
            Entries = t.Entries?.Select(e => new TimesheetEntryDto
            {
                TimesheetEntryID = e.TimesheetEntryID,
                TimesheetID = e.TimesheetID,
                ProjectID = e.ProjectID,
                ProjectName = e.Project?.Name,
                TaskID = e.TaskID,
                TaskName = e.Task?.Name,
                EntryDate = e.EntryDate,
                HoursWorked = e.HoursWorked,
                Notes = e.Notes
            }).ToList() ?? new List<TimesheetEntryDto>()
        };
    }
}
