using System.Text.Json.Serialization;
using Magik_Johnson.Data;
using Magik_Johnson.Data.Seeder;
using Magik_Johnson.Extensions;
using Magik_Johnson.Models;
using Magik_Johnson.Services;
using Magik_Johnson.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder
    .Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder
    .Services.AddOptions<BearerTokenOptions>(IdentityConstants.BearerScheme)
    .Configure(options =>
    {
        options.BearerTokenExpiration = TimeSpan.FromMinutes(5);
        options.RefreshTokenExpiration = TimeSpan.FromHours(1);
    });

builder.Services.AddAuthorization();

builder.Services.AddConfiguration(config: builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "MyCorsPolicy",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "https://localhost:5173",
                    "https://sqlinfocg.cegepgranby.qc.ca/",
                    "https://checkout.stripe.com/"
                )
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

builder.Services.AddQuartz(q =>
{
    var updateCardsJobKey = new JobKey("UpdateCards");
    q.AddJob<IDbDataInsertion>(j => j.WithIdentity(updateCardsJobKey));
    q.AddTrigger(j =>
        j.ForJob(updateCardsJobKey)
            .WithIdentity("UpdateCards-trigger")
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0))
    );

    var resetClickCountJobKey = new JobKey("ResetClickCount");
    q.AddJob<IResetClickCountJob>(j => j.WithIdentity(resetClickCountJobKey));
    q.AddTrigger(j =>
        j.ForJob(resetClickCountJobKey)
            .WithIdentity("ResetClickCount-trigger")
            .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Sunday, 0, 0))
    );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

StripeConfiguration.ApiKey = app.Configuration["Stripe:SecretKey"];

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("MyCorsPolicy");

app.MapIdentityApi<ApplicationUser>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roleNames =
    [
        ApplicationRoles.Client.Value,
        ApplicationRoles.Employee.Value,
        ApplicationRoles.Manager.Value,
    ];

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await QuantitySeeder.SeedAsync(dbContext);
    DatabaseInitializer.Initialize(dbContext);
}
app.Run();
