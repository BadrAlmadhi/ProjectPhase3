using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectPhase3.Areas.Identity.Data;
using ProjectPhase3.Data;

System.AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("IdentityContextConnection") ?? throw new InvalidOperationException("Connection string 'IdentityContextConnection' not found.");;

// LMS1: courses, students, assignments, enrollments, etc.
var lmsConnectionString =
    builder.Configuration["LMS:ConnectionString"]
    ?? throw new InvalidOperationException(
        "LMS connection string was not found.");

// LMSUsers1: accounts, passwords, roles and login information.
var identityConnectionString =
    builder.Configuration["LMS:IdentityConnectionString"]
    ?? throw new InvalidOperationException(
        "Identity connection string was not found.");

// Connect LmsContext to LMS1.
builder.Services.AddDbContext<LmsContext>(options =>
    options.UseNpgsql(lmsConnectionString));

// Connect IdentityContext to LMSUsers1.
builder.Services.AddDbContext<IdentityContext>(options =>
    options.UseNpgsql(identityConnectionString));



// Add ASP.NET Core Identity.
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole>() // added Role
    .AddEntityFrameworkStores<IdentityContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 1;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
});

// Add MVC and Razor Pages.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Identify who the user is first.
app.UseAuthentication();

// Then check what the user is allowed to access.
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Makes /Identity/Account/Register and other Identity pages reachable.
app.MapRazorPages();

//adapted from https://codewithmukesh.com/blog/user-management-in-aspnet-core-mvc/
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await ProjectPhase3.Authorization.Roles.SeedRolesAsync(roleManager);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();
