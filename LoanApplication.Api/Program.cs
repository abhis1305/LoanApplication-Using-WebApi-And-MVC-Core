using LoanApplication.Application.Interface;
using LoanApplication.Application.Interfcae;
using LoanApplication.Application.Mapper;
using LoanApplication.Infrastructure.Data;
using LoanApplication.Infrastructure.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using Hangfire;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)]
                = new List<string>()
        });
});

// 2. Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// 3. Register application services
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

// 4. Register KYC, penalty, payment and authentication services
builder.Services.AddScoped<IKycService, KycService>();
builder.Services.AddScoped<IPenaltyService, PenaltyService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 5. Configure memory cache and AutoMapper
builder.Services.AddMemoryCache();

builder.Services.AddAutoMapper(
    cfg => { },
    typeof(SupportTicketProfile).Assembly
);

// 6. Configure SQL Server database
var connectionString =
    builder.Configuration.GetConnectionString("dbconn")
    ?? throw new InvalidOperationException(
        "Database connection string 'dbconn' is not configured."
    );

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(connectionString)
);

// 7. Configure Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(connectionString)
);

builder.Services.AddHangfireServer();

// 8. Configure JWT authentication
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
                ClockSkew = TimeSpan.Zero,

                RoleClaimType = ClaimTypes.Role
            };
    });

// 9. Configure authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AccessTokenOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("Purpose", "Access");
    });
});

// 10. Build application
var app = builder.Build();

// 11. Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 12. Configure middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("AngularFrontend");

app.UseAuthentication();
app.UseAuthorization();

// 13. Enable Hangfire Dashboard
app.UseHangfireDashboard();

// 14. Register daily EMI reminder job
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

// 15. Map controllers
app.MapControllers();

// 16. Start application
app.Run();