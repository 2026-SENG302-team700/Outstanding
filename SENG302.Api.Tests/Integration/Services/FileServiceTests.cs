using System.Text;
using Microsoft.AspNetCore.Http;

namespace SENG302.Api.Tests.Integration.Services;
using SENG302.Api.Services;
using Shouldly;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

public class FileServiceTests : BaseIntegrationTestFixture
{
    private IFileService ServiceUnderTest => ServiceProvider.GetRequiredService<IFileService>();

    public FileServiceTests(WebApplicationFactory<Program> webAppFactory) : base(webAppFactory) { }

    private IFormFile GetMockFile(string filename, Byte[] content, string mimeType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, stream.Length, "file", filename)
        {
            Headers = new HeaderDictionary(),
            ContentType = mimeType
        };
    }
    
    [Fact]
    public async Task SaveFileEntity_Success_ReturnsCustomFileEntity()
    {
        await using var context = await DbContextFactory.CreateDbContextAsync();

        var mockFileContent = File.ReadAllBytes("resources/panda.webp");
        
        var mockFile = GetMockFile(
            "mocka",
            mockFileContent,
            "image/webp");

        
        var fileEntity = await ServiceUnderTest.SaveFileAsync(mockFile, 0);

        var singleItemInDb = context.CustomFiles.ShouldHaveSingleItem();
        singleItemInDb.OwnerId.ShouldBe(0);
        singleItemInDb.OriginalFileName.ShouldBe("mocka");
        singleItemInDb.MimeType.ShouldBe("image/webp");
        singleItemInDb.FileSize.ShouldBe(mockFile.Length);

        var readFile = File.ReadAllBytes(Path.Combine(FakeTestDirectory, singleItemInDb.FileKey));
        readFile.ShouldBe(mockFileContent);
    }
    
    [Fact]
    public async Task GetFileEntity_Success_ReturnsCustomFileEntity()
    {
        await using var context = await DbContextFactory.CreateDbContextAsync();
        
        var mockFileContent = File.ReadAllBytes("resources/panda.webp");
        
        var mockFile = GetMockFile(
            "mocka",
            mockFileContent,
            "image/webp");
        
        var fileEntity = await ServiceUnderTest.SaveFileAsync(mockFile, 0);
        
        var getFileEntity = await ServiceUnderTest.GetFileByIdAsync(fileEntity.Id);
        getFileEntity.ShouldBeEquivalentTo(fileEntity);
    }
}