using Microsoft.AspNetCore.Http;
using SuCoS.Models.CommandLineOptions;
using SuCoS.ServerHandlers;
using Xunit;

namespace Tests.ServerHandlers;

public class RegisteredPageRequestHandlerTests : TestSetup
{
    [Theory]
    [InlineData("/", true)]
    [InlineData("/testPage", false)]
    public void Check_ReturnsTrueForRegisteredPage(string requestPath, bool exist)
    {
        // Arrange
        var siteFullPath = Path.GetFullPath(Path.Combine(testSitesPath, testSitePathCONST06));
        site.Options = new GenerateOptions
        {
            Source = siteFullPath
        };
        var registeredPageRequest = new RegisteredPageRequest(site);

        // Act
        site.ParseAndScanSourceFiles(Path.Combine(siteFullPath, "content"));

        // Assert
        Assert.Equal(exist, registeredPageRequest.Check(requestPath));
    }

    [Theory]
    [InlineData("/", testSitePathCONST06, false)]
    [InlineData("/", testSitePathCONST08, true)]
    public async Task Handle_ReturnsExpectedContent2(string requestPath, string testSitePath, bool contains)
    {
        // Arrange
        var siteFullPath = Path.GetFullPath(Path.Combine(testSitesPath, testSitePath));
        site.Options = new GenerateOptions
        {
            Source = siteFullPath
        };
        var registeredPageRequest = new RegisteredPageRequest(site);

        var context = new DefaultHttpContext();
        var stream = new MemoryStream();
        context.Response.Body = stream;

        // Act
        site.ParseAndScanSourceFiles(Path.Combine(siteFullPath, "content"));
        registeredPageRequest.Check(requestPath);
        await registeredPageRequest.Handle(context, requestPath, DateTime.Now);

        // Assert
        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        // You may want to adjust this assertion depending on the actual format of your injected script
        if (contains)
        {
            Assert.Contains("<script>", content, StringComparison.InvariantCulture);
            Assert.Contains("</script>", content, StringComparison.InvariantCulture);
        }
        else
        {
            Assert.DoesNotContain("</cript>", content, StringComparison.InvariantCulture);
            Assert.DoesNotContain("</script>", content, StringComparison.InvariantCulture);
        }
        Assert.Contains("Index Content", content, StringComparison.InvariantCulture);

    }

}
