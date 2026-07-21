using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectPhase3.Areas.Identity.Data;
using ProjectPhase3.Data;

var builder = WebApplication.CreateBuilder(args);

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

app.Run();
