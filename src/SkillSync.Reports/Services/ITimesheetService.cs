using SkillRadarReports.Dtos;

namespace SkillRadarReports.Services;

public interface ITimesheetService
{
    Task<TimesheetDto> GetOrInitializeTimesheetAsync(int employeeId, DateTime weekStartDate);
    Task<TimesheetDto> SaveTimesheetAsync(SaveTimesheetDto dto);
    Task<TimesheetDto> SubmitTimesheetAsync(int timesheetId);
    Task<TimesheetDto> ApproveTimesheetAsync(int timesheetId, int managerEmployeeId);
    Task<TimesheetDto> RejectTimesheetAsync(int timesheetId, int managerEmployeeId, string reason);
}
