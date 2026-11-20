using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SportsClub.Data;
using SportsClub.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
var connection = new SqliteConnectionStringBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=sportsclub.db");
if (!Path.IsPathRooted(connection.DataSource))
    connection.DataSource = Path.Combine(builder.Environment.ContentRootPath, connection.DataSource);
connection.ForeignKeys = true;
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection.ToString()));
builder.Services.AddScoped<EnrollmentService>();
var app = builder.Build();

// Explicit setup command; normal startup never silently changes the schema.
if (args.Contains("--migrate") || args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    if (args.Contains("--seed")) await DbInitializer.SeedAsync(db);
    app.Logger.LogInformation("Database setup complete: {Database}", connection.DataSource);
    return;
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();

public partial class Program;
