using LoanApplication.Application.Interfcae;
using LoanApplication.Application.Mapper;
using LoanApplication.Application.Interface;
using LoanApplication.Infrastructure.Data;
using LoanApplication.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CIBIL Service
builder.Services.AddScoped<ICibilService, CibilService>();
builder.Services.AddScoped<IScoreCardService, ScoreCardService>();

// Database Connection
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("dbconn")
    ));

builder.Services.AddMemoryCache();                        // "create the notebook (cache)"
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddAutoMapper(typeof(SupportTicketProfile));

var app = builder.Build();

// Swagger Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();