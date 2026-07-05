using Microsoft.EntityFrameworkCore;
using TaskManagment.Domain.Models;

namespace TaskManagment.Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAdminUser(ApplicationDbContext context)
        {
            // Check if admin user already exists
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@example.com");
            
            if (adminUser == null)
            {
                // Create admin user
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    Email = "admin@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(admin);
                await context.SaveChangesAsync();
                
                Console.WriteLine("Admin user seeded successfully");
            }
            else
            {
                Console.WriteLine("Admin user already exists");
            }
        }
    }
}
