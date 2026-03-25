using Microsoft.AspNetCore.Identity;
using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;

namespace SENG302.Api.Resources.Helpers;

public static class CreateExampleUsersHelper
{
    /// <summary>
    /// Makes some pre-made examples of users
    /// </summary>
    /// <param name="db">The database context</param>
    public static async Task CreateExamples(DatabaseContext db)
    {
        await CreateExample_Admin(db);
        await CreateExample_John(db);
        await CreateExample_Jane(db);
    }

    /// <summary>
    /// Creates a blank admin account
    /// </summary>
    /// <param name="dbContext">The database context</param>
    private static async Task CreateExample_Admin(DatabaseContext dbContext)
    {
        // Add admin user
        if (!dbContext.Users.Any(u => u.Email.ToLower() == "admin@outstanding.com"))
        {
            var user = new User
            {
                DisplayName = "admin",
                Email = "admin@outstanding.com",
                Country = "NZ",
                EmailVerified = true
            };
            var hasher = new PasswordHasher<User>();
            user.PasswordKey = hasher.HashPassword(user, "Team700!");
            dbContext.Users.Add(user);
            dbContext.SaveChanges();
        }
    }

    /// <summary>
    /// Creates a user John Whitaker, john@example.com
    /// He has 20 task lists with a varying amount of tasks in each
    /// Some tasks have a long description
    /// </summary>
    /// <param name="dbContext">The database context</param>
    private static async Task CreateExample_John(DatabaseContext dbContext)
    {
        if (!dbContext.Users.Any(u => u.Email.ToLower() == "john@example.com"))
        {
            var user = new User
            {
                DisplayName = "John Whitaker",
                Email = "john@example.com",
                Country = "US",
                EmailVerified = true
            };
            var hasher = new PasswordHasher<User>();
            user.PasswordKey = hasher.HashPassword(user, "P4$$word");
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            for (int i = 0; i < 20; i++)
            {
                var taskList = new TaskList
                {
                    Name = "Name_" + i.ToString(),
                    UserId = user.Id,
                };

                dbContext.Add(taskList);

                await dbContext.SaveChangesAsync();

                for (int j = 0; j < (i % 10); j++)
                {
                    var task = new TaskItem
                    {
                        Name = "Task_" + j.ToString(),
                        TaskListId = taskList.Id,
                        Description = (j % 5) != 4 ? "Number: " + j.ToString() : "antimony, arsenic, aluminum, selenium And hydrogen, and oxygen, and nitrogen, and rhenium And nickel, neodymium, neptunium, germanium And iron, americium, ruthenium, uranium Europium, zirconium, lutetium, vanadium And lanthanum, and osmium, and astatine, and radium And gold, protactinium, and indium, and gallium And iodine, and thorium, and thulium, and thallium There's yttrium, ytterbium, actinium, rubidium And boron, gadolinium, niobium, iridium And strontium, and silicon, and silver, and samarium And bismuth, bromine, lithium, beryllium, and barium",
                        DueDate = DateTime.MaxValue,
                        creationTime = DateTimeOffset.Now,
                        CurrentStatus = (CurrentTaskStatus)(j % 3)
                    };
                    dbContext.Add(task);
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }


    /// <summary>
    /// Adds a blank user Jane Watson, jane@example.com to the database
    /// </summary>
    /// <param name="dbContext">The database context</param>
    private static async Task CreateExample_Jane(DatabaseContext dbContext)
    {
        if (!dbContext.Users.Any(u => u.Email.ToLower() == "jane@example.com"))
        {
            var user = new User
            {
                DisplayName = "Jane Watson",
                Email = "jane@example.com",
                Country = "CR",
                EmailVerified = true
            };
            var hasher = new PasswordHasher<User>();
            user.PasswordKey = hasher.HashPassword(user, "P4$$word");
            dbContext.Users.Add(user);
            dbContext.SaveChanges();
        }
    }
}