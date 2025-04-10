using Microsoft.AspNetCore.Identity;

public class DbSeeder
{
    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<DbSeeder>>();

        try
        {
            var userManager =
                services.GetService<UserManager<AppUser>>()
                ?? throw new NullReferenceException("Unable to retrieve UserManager");
            var roleManager =
                services.GetService<RoleManager<IdentityRole>>()
                ?? throw new NullReferenceException("Unable to retrieve RoleManager");

            if (userManager.Users.Any() == true)
            {
                logger.LogInformation("User table not empty. Admin not created.");
                return;
            }

            var user = new AppUser
            {
                UserName = "admin",
                Email = "admin@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            // Make sure Admin role exists
            if ((await roleManager.RoleExistsAsync(AppRoles.Admin)) == false)
            {
                throw new Exception("Migration error. Roles missing from table");
            }

            // Attempt to create admin user
            var createUserResult = await userManager.CreateAsync(
                user: user,
                password: "loggainadminförfileserverprojekt."
            );

            // Validate user creation
            if (createUserResult.Succeeded == false)
            {
                var errors = createUserResult.Errors.Select(e => e.Description);
                logger.LogError(
                    "Failed to create admin user. Errors : {errors}",
                    string.Join(", ", errors)
                );
                return;
            }

            // adding role to user
            var addUserToRoleResult = await userManager.AddToRoleAsync(
                user: user,
                role: AppRoles.Admin
            );

            if (addUserToRoleResult.Succeeded == false)
            {
                var errors = addUserToRoleResult.Errors.Select(e => e.Description);
                logger.LogError(
                    "Failed to add admin role to user. Errors : {errors}",
                    string.Join(", ", errors)
                );
            }
            logger.LogInformation("Admin user is created");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Critical error while seeding admin user.");
        }
    }
}
