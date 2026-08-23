using Microsoft.EntityFrameworkCore;
using SkillRadarReports.Data;

var builder = WebApplication.CreateBuilder(args);

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core - connection string comes from appsettings.json or env var ConnectionStrings__DefaultConnection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Always enable Swagger for now (student project, not production) so it's easy to demo
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "SkillRadar Reports & DevOps service is running. Visit /swagger to explore endpoints.");

app.Run();
