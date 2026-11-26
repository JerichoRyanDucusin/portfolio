using Npgsql;
using portfolio.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// Database: read the connection string from User Secrets and create one shared data source.
var connectionString = builder.Configuration.GetConnectionString("Library");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'Library' is missing. Add it in Manage User Secrets.");
}
var dataSource = NpgsqlDataSource.Create(connectionString);
builder.Services.AddSingleton(dataSource);

// Repositories: one new object per web request.
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<ProjectImageRepository>();

var app = builder.Build();

// Friendly pages: unexpected errors, and 404 or any other empty error status.
app.UseExceptionHandler("/Error/Unexpected");
app.UseStatusCodePagesWithReExecute("/Error/Status/{0}");

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Courses}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();