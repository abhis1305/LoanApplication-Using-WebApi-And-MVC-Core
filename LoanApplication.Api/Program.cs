using LoanApplication.Application.Interface;
using LoanApplication.Application.Interfcae;
using LoanApplication.Application.Mapper;
using LoanApplication.Infrastructure.Data;
using LoanApplication.Infrastructure.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Hangfire;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Register application services
builder.Services.AddScoped<ICibilService, CibilService>();
builder.Services.AddScoped<IScoreCardService, ScoreCardService>();
builder.Services.AddScoped<IEligibilityService, EligibilityService>();
builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddScoped<ILoanAccountRepository, LoanAccountRepository>();
builder.Services.AddScoped<IEmiScheduleRepository, EmiScheduleRepository>();
builder.Services.AddScoped<IEmiReminderRepository, EmiReminderRepository>();

builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();

// 3. Register penalty, payment and authentication services
builder.Services.AddScoped<IPenaltyService, PenaltyService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 4. Memory cache and AutoMapper
builder.Services.AddMemoryCache();

builder.Services.AddAutoMapper(
    cfg => { },
    typeof(SupportTicketProfile).Assembly
);

// 5. Configure SQL Server database
var connectionString =
    builder.Configuration.GetConnectionString("dbconn")
    ?? throw new InvalidOperationException(
        "Database connection string 'dbconn' is not configured."
    );

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(connectionString)
);

// 6. Configure Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(connectionString)
);

builder.Services.AddHangfireServer();

// 7. Configure JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Jwt:Key is not configured."
    );

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? "LoanAppAPI";

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? "LoanAppClients";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)
                ),

                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
    });

// 8. Configure authorization
builder.Services.AddAuthorization();

// 9. Build the application
var app = builder.Build();

// 10. Register the daily EMI reminder job
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager =
        scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<IEmiReminderRepository>(
        "emi-reminder-check",
        service => service.CheckUpcomingEmis(),
        Cron.Daily
    );
}

// 11. Serve static files
app.UseStaticFiles();

// 12. Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 13. Configure middleware
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// 14. Enable Hangfire Dashboard
app.UseHangfireDashboard();

// 15. Map API controllers
app.MapControllers();

// 16. Start the application
app.Run();