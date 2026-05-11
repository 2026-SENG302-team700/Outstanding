using Microsoft.EntityFrameworkCore;
using SENG302.Api.DataAccess;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Resources.DefaultDatabase;

namespace SENG302.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // If no environment is set default it to Development (instead of Production)
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
        {
            builder.Environment.EnvironmentName = Environments.Development;
        }
        Console.WriteLine($"*** CURRENT ENVIRONMENT: {builder.Environment.EnvironmentName} ***");

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddOpenApi();
        }

        // Add services to the container Using `WithViews` registers the Antiforgery filters required for [ValidateAntiForgeryToken]
        builder.Services.AddControllersWithViews(options =>
        {
            // tells browsers to not cache API responses (was breaking deployment on firefox)
            options.Filters.Add(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None });
        });

        // Add authorization service
        builder.Services.AddAuthorization();


        if (builder.Environment.IsProduction() || builder.Environment.IsStaging())
        {
            var dbHost = builder.Configuration["DB_HOST"];
            var dbName = builder.Configuration["DB_NAME"];
            var dbUser = builder.Configuration["DB_USER"];
            var dbPass = builder.Configuration["DB_PASS"];

            if (dbHost == null || dbName == null || dbUser == null || dbPass == null)
            {
                throw new InvalidOperationException("Missing required database environment variables.");
            }

            // build the connection string to be used by PosgreSQL
            var connectionString =
                $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPass};SSL Mode=Require;Trust Server Certificate=true";

            builder.Services.AddDbContextFactory<DatabaseContext>((_, options) =>
                options.UseNpgsql(connectionString)
                );
        }
        else
        {
            // Configure database context with factory pattern
            builder.Services.AddDbContextFactory<DatabaseContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"));

        }


        builder.Services.AddHttpContextAccessor();

        // Register custom services 
        RegisterServices(builder.Services);

        // Setup antiforgery (CSRF)
        var cookiePolicy = builder.Environment.IsProduction() || builder.Environment.IsStaging()
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;

        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "X-CSRF-TOKEN-COOKIE";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.HttpOnly = true;

            options.Cookie.SecurePolicy = cookiePolicy;
        });

        // Configure the cookie-based authentication and set security options 
        builder.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "OUTSTANDING-AUTH-COOKIE";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = cookiePolicy;
                options.Cookie.SameSite = SameSiteMode.Lax;

                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/api/auth/access-denied";
            })
            .AddCookie("PasswordResetScheme", options =>
            {
                options.Cookie.Name = "OUTSTANDING-RESET-COOKIE";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = cookiePolicy;
                options.Cookie.SameSite = SameSiteMode.Lax;

                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });

        // Add CORS for development only
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }

        // add the custom environment file
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true).AddEnvironmentVariables();

        // bind it in email service
        builder.Services.AddSingleton(new EmailSettings());
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddTransient<ISmtpClientWrapper, SmtpClientWrapper>();

        var app = builder.Build();

        // Make sure we use forwarded headers in production so our api works behind reverse proxy (nginx) with https
        if (builder.Environment.IsProduction() || builder.Environment.IsStaging())
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
            });
        }

        InitializeDatabase(app.Services.CreateScope().ServiceProvider, !(app.Environment.IsProduction() || app.Environment.IsStaging()));

        var pathBase = app.Configuration["PathBase"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        app.UseRouting();

        // Only use https redirects in production
        if (builder.Environment.IsProduction() || builder.Environment.IsStaging())
        {
            app.UseHttpsRedirection();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseCors("AllowFrontend");
        }

        // Tell app to use authentication and authorization middleware
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        // CSRF token endpoint`
        app.MapGet("/api/csrf-token", (Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery, HttpContext context) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Ok(new { token = tokens.RequestToken });
        });

        app.MapControllers();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "SENG302  API v1");
            });
        }

        app.Run();
    }

    protected static async Task InitializeDatabase(IServiceProvider serviceProvider, bool isDevelopment)
    {
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<DatabaseContext>>();
        var dbContext = dbContextFactory.CreateDbContext();

        try
        {
            if (isDevelopment)
            {
                dbContext.Database.EnsureCreated();
            }
            else
            {
                await dbContext.Database.MigrateAsync();
            }

            await CreateExampleUsersHelper.CreateExamples(dbContext);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error occured: ", e);
            throw;
        }

    }

    protected static void RegisterServices(IServiceCollection services)
    {
        // Register a TimeProvider so we don't need to rely on DateTime.Now, and can mock the time in automated tests
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<ITaskListService, TaskListService>();
        services.AddScoped<ITaskItemService, TaskItemService>();
        services.AddScoped<IOneTimeCodeService, OneTimeCodeService>();

        // Make sure you know the differences between AddSingleton, AddScoped, and AddTransient.
        // (If in doubt, you probably just want AddScoped
    }

}
