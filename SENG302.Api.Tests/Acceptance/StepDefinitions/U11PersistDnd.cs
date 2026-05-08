using System.Net;
using System.Net.Http.Json;
using Reqnroll;
using SENG302.Api.Models.Entities;
using SENG302.Api.Tests.Acceptance.Setup;
using Shouldly;

namespace SENG302.Api.Tests.Acceptance.StepDefinitions;

[Binding]
public class U11PersistDnd
{
    private readonly AcceptanceTestFixture _fixture;
    private HttpResponseMessage? _lastResponse;
    
    public U11PersistDnd(AcceptanceTestFixture fixture)
    {
        _fixture = fixture;
    }

    //[Given(@"I am a registered user")]
    //public async Task GivenIAmARegisteredUser()
    //{
    //    await using var context = await _fixture.DbContextFactory.CreateDbContextAsync();
    //    context.Users.Add(new User                                                                                            
    //        {
    //            Email = "test@example.com",                                                                                       
    //            DisplayName = "Tipene",
    //            PasswordKey = "password",                                                                                         
    //            Country = "NZ"                                                                                                    
    //        }
    //    );
    //    await context.SaveChangesAsync();
    //    _fixture.CurrentUserId = 1;
    //}

    [Given(@"I have a task list")]
    public async Task GivenIHaveATaskList()
    {
        await using var context = await _fixture.DbContextFactory.CreateDbContextAsync();
        context.TaskLists.Add(new Models.Entities.TaskList
        {
            Id = 1,
            Name = "My Task List",
            UserId = _fixture.CurrentUserId!.Value,
        });
        await context.SaveChangesAsync();
    }

    [Given(@"I have 3 tasks \(Task 1, Task 2, Task 3\) in order")]
    public async Task GivenIHave3TasksInOrder()
    {
        await using var context = await _fixture.DbContextFactory.CreateDbContextAsync();
        
        context.TaskItems.AddRange(
            new TaskItem
            {
                TaskListId = 1,
                Name = "Task 1",
                Description = "",
                OrderPosition = 1
            },
            new TaskItem
            {
                TaskListId = 1,
                Name = "Task 2",
                Description = "",
                OrderPosition = 2
            },
            new TaskItem
            {
                TaskListId = 1,
                Name = "Task 3",
                Description = "",
                OrderPosition = 3
            }
        );
        
        await context.SaveChangesAsync();
    }

    [When(@"I reorder Task 3 to the top of the list")]
    public async Task WhenIReorderTask3ToTheTopOfTheList()
    {
        await using var context = await _fixture.DbContextFactory.CreateDbContextAsync();
        var task1Id = context.TaskItems.Single(t => t.Name == "Task 1").TaskId;                                                   
        var task2Id = context.TaskItems.Single(t => t.Name == "Task 2").TaskId;                                                   
        var task3Id = context.TaskItems.Single(t => t.Name == "Task 3").TaskId;
        
        _lastResponse = await _fixture.HttpClient.PatchAsJsonAsync("/api/taskItem/order", new List<int> 
        {
            task3Id, task1Id, task2Id
        });
    }
    
    [Then(@"The tasks are returned in the order \(Task 3, Task 1, Task 2\)")]                                                     
    public async Task ThenTasksAreReturnedInOrder()
    {
        _lastResponse!.StatusCode.ShouldBe(HttpStatusCode.OK);
                                                                                                                                
        var response = await _fixture.HttpClient.GetAsync("/api/taskItem");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();                                                   
        tasks![0].Name.ShouldBe("Task 3");                                                                                        
        tasks[1].Name.ShouldBe("Task 1");
        tasks[2].Name.ShouldBe("Task 2");                                                                                         
    } 
}