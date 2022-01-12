using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using TwittorAPI.Models;

namespace TwittorAPI.Data
{
    public class PrepDB
    {
        public static class PrepDb
        {
            public static void PrepPopulation(IApplicationBuilder app, bool isProd)
            {
                using (var serviceScope = app.ApplicationServices.CreateScope())
                {
                    SeedData(serviceScope.ServiceProvider.GetService<TwittorDBContext>(), isProd);
                }
            }

            private static void SeedData(TwittorDBContext context, bool isProd)
            {
                if (isProd)
                {
                    Console.WriteLine("--> Menjalankan Migrasi");
                    try
                    {
                        context.Database.Migrate();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"--> Gagal melakukan migrasi {ex.Message}");
                    }
                }

                if (!context.Users.Any())
                {
                    Console.WriteLine("--> Seeding data for Users....");
                    context.Users.AddRange(
                        new User() { FullName = "Administrator", Email = "admin@mail.com", Username = "admin", Password="admin" },
                        new User() { FullName = "Account1", Email = "account1@mail.com", Username = "account1", Password = "account1" },
                        new User() { FullName = "Account2", Email = "account2@mail.com", Username = "account2", Password = "account2" }
                    );
                    context.SaveChanges();
                }
                else
                {
                    Console.WriteLine("--> User Table is already have data...");
                }

                if (!context.Tweets.Any())
                {
                    Console.WriteLine("--> Seeding data Tweets....");
                    context.Tweets.AddRange(
                        new Tweet() { UserId = 1, Text = "My First Tweet as Admin", CreatedAt = DateTime.Now },
                        new Tweet() { UserId = 2, Text = "My First Tweet as Account1", CreatedAt = DateTime.Now },
                        new Tweet() { UserId = 3, Text = "My First Tweet as Account2", CreatedAt = DateTime.Now }
                    );
                    context.SaveChanges();
                }
                else
                {
                    Console.WriteLine("--> Tweet Table is already have data...");
                }

                if (!context.Roles.Any())
                {
                    Console.WriteLine("--> Seeding data for Roles....");
                    context.Roles.AddRange(
                        new Role() { Name = "ADMIN"},
                        new Role() { Name = "MEMBER"},
                        new Role() { Name = "MEMBER"}
                    );
                    context.SaveChanges();
                }
                else
                {
                    Console.WriteLine("--> Roles Table is already have data...");
                }
            }
        }
    }
}
