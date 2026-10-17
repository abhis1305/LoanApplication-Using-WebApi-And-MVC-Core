using LoanApplication.Application.Interface;
using LoanApplication.Application.Interfcae;
using LoanApplication.Application.Mapper;
using LoanApplication.Infrastructure.Data;
using LoanApplication.Infrastructure.Repositories;

using Hangfire;
using Hangfire.SqlServer;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("App", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// 3. Database connection
var connectionString =
    builder.Configuration.GetConnectionString("dbconn")
    ?? throw new InvalidOperationException(
        "Database connection string 'dbconn' is not configured.");

// 4. Entity Framework Core
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(connectionString));

// 5. QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

// 6. Swagger with JWT Bearer authentication
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your valid ACCESS token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)]
                = new List<string>()
        });
});

// 7. Application services
builder.Services.AddScoped<ICibilService, CibilService>();
builder.Services.AddScoped<IScoreCardService, ScoreCardService>();
builder.Services.AddScoped<IEligibilityService, EligibilityService>();
builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddScoped<ILoanAccountRepository, LoanAccountRepository>();
builder.Services.AddScoped<IEmiScheduleRepository, EmiScheduleRepository>();
builder.Services.AddScoped<IEmiReminderRepository, EmiReminderRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<IKycService, KycService>();
builder.Services.AddScoped<IPenaltyService, PenaltyService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 8. Memory cache and AutoMapper
builder.Services.AddMemoryCache();

builder.Services.AddAutoMapper(
    cfg => { },
    typeof(SupportTicketProfile).Assembly);

// 9. Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer();

// 10. JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key is not configured.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LoanAppAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LoanAppClients";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            RoleClaimType = ClaimTypes.Role
        };
    });

// 11. Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AccessTokenOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("Purpose", "Access");
    });
});

// 12. Build the application (only once)
var app = builder.Build();

// 13. Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 14. Middleware (order matters)
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("App");

app.UseAuthentication();
app.UseAuthorization();

// 15. Hangfire Dashboard
app.UseHangfireDashboard();

// 16. Schedule daily EMI reminder (only once)
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager =
        scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<IEmiReminderRepository>(
    "emi-reminder-check",
    service => service.CheckUpcomingEmis(),
    Cron.Minutely);
}

// 17. Map controllers
app.MapControllers();

// 18. Run
app.Run();