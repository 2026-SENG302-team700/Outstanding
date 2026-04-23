using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SENG302.Api.DataAccess;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using Shouldly;

namespace SENG302.Api.Tests.Unit;

public class TaskItemServiceTests
{
    private static readonly DateTimeOffset TestNow = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly TaskItemService ServiceUnderTest;
    public TaskItemServiceTests()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>().Options;
        var dummyContextFactory = new PooledDbContextFactory<DatabaseContext>(options);

        var fakeTimeProvider = new FakeTimeProvider(TestNow);

        ServiceUnderTest = new TaskItemService(dummyContextFactory, fakeTimeProvider);
    }

    // Fake time provider defined INSIDE the test class
    private class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FakeTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    [Theory]
    [InlineData("Test")] // basic test
    [InlineData("Tēst")] // macron
    public void ValidateName_ValidName_ReturnsNothing(string name)
    {
        Should.NotThrow(() => ServiceUnderTest.ValidateTaskItemName(name));
    }

    [Theory]
    [InlineData("Hi")] // short 2 chars
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")] // 129 chars
    public void ValidateName_InValidName_ThrowError(string name)
    {
        var errors = new Dictionary<string, string>();

        foreach (var (key, value) in ServiceUnderTest.ValidateTaskItemName(name))
        {
            errors[key] = value;
        }        
        
        errors.ShouldNotBeEmpty();
        errors.ShouldHaveSingleItem();
        errors.Keys.ShouldContain("name");
        errors.Values.ShouldContain("Title is required and must be between 3 and 128 characters long");
    }

    [Fact]
    public void ValidateDescription_LongDescription_ThrowError()
    {
        var errors = new Dictionary<string, string>();

        foreach (var (key, value) in ServiceUnderTest.ValidateTaskItemDescription(new string('a', 2049)))
        {
            errors[key] = value;
        }        
        
        errors.ShouldNotBeEmpty();
        errors.ShouldHaveSingleItem();
        errors.Keys.ShouldContain("description");
        errors.Values.ShouldContain("Description must be 2048 characters or less");
    }

    [Fact]
    public void ValidateDueDate_FutureDate_NoError()
    {
        var errors = new Dictionary<string, string>();
        
        foreach (var (key, value) in ServiceUnderTest.ValidateTaskItemDueDate(DateTime.UtcNow.Add(TimeSpan.FromDays(1))))
        {
            errors[key] = value;
        }        
        
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateDueDate_PastDate_ThrowError()
    {
        var errors = new Dictionary<string, string>();
        
        foreach (var (key, value) in ServiceUnderTest.ValidateTaskItemDueDate(DateTime.UtcNow.Subtract(TimeSpan.FromDays(1))))
        {
            errors[key] = value;
        }        
        
        errors.ShouldNotBeEmpty();
        errors.ShouldHaveSingleItem();
        errors.Keys.ShouldContain("dueDate");  
        errors.Values.ShouldContain("Invalid due date, date must be in the future");
    }

    [Fact]
    public void ValidateCurrentStatus_Valid_NoProblems()
    {
        Should.NotThrow(() => ServiceUnderTest.ValidateTaskItemCurrentStatus(CurrentTaskStatus.Todo));                            
        Should.NotThrow(() => ServiceUnderTest.ValidateTaskItemCurrentStatus(CurrentTaskStatus.InProgress));                      
        Should.NotThrow(() => ServiceUnderTest.ValidateTaskItemCurrentStatus(CurrentTaskStatus.Done));  
    }
}