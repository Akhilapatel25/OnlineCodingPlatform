/*using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineCodingAPI.Enums;
using OnlineCodingAPI.Models;

namespace OnlineCodingAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(
            ApplicationDbContext context)
        {
            // Check whether an admin already exists
            var existingAdmin = await context.Users
                .FirstOrDefaultAsync(u => u.Role == UserRole.Admin);

            if (existingAdmin != null)
            {
                Console.WriteLine(
                    $"Admin already exists: {existingAdmin.Email}"
                );

                return;
            }

            // Check whether admin email already exists
            var existingEmail = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "admin@gmail.com");

            if (existingEmail != null)
            {
                Console.WriteLine(
                    "ERROR: Admin email 'admin@gmail.com' already exists."
                );

                Console.WriteLine(
                    "Admin was not created. Please use a different email."
                );

                return;
            }

            // Check whether admin username already exists
            var existingUsername = await context.Users
                .FirstOrDefaultAsync(u => u.Username == "admin");

            if (existingUsername != null)
            {
                Console.WriteLine(
                    "ERROR: Admin username 'admin' already exists."
                );

                Console.WriteLine(
                    "Admin was not created. Please use a different username."
                );

                return;
            }

            // Create admin
            var passwordHasher = new PasswordHasher<User>();

            var admin = new User
            {
                Username = "admin",
                Email = "admin@gmail.com",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            Console.WriteLine("Admin created successfully: admin@gmail.com");
        }
    }
}
*/
using Microsoft.EntityFrameworkCore;
using OnlineCodingAPI.Enums;
using OnlineCodingAPI.Models;

namespace OnlineCodingAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(
            ApplicationDbContext context)
        {
            var admin = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "admin@gmail.com");

            if (admin != null)
            {
                admin.Username = "admin";
                admin.Role = UserRole.Admin;

                // Development only:
                admin.PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword("Admin@123");

                await context.SaveChangesAsync();

                Console.WriteLine(
                    "Admin already existed. Admin password reset."
                );

                return;
            }

            admin = new User
            {
                Username = "admin",
                Email = "admin@gmail.com",
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(admin);

            await context.SaveChangesAsync();

            Console.WriteLine(
                "Admin created successfully: admin@gmail.com"
            );
        }
    }
}