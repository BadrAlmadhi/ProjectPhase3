using Microsoft.AspNetCore.Identity;

namespace ProjectPhase3.Authorization
{
    public static class Roles
    {
        public const string Administrator = "Administrator";
        public const string Professor = "Professor";
        public const string Student = "Student";

        public static readonly string[] AllRoles =
        {
            Administrator,
            Professor,
            Student
        };

        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in AllRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
