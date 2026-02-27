using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SENG302.Api.Filters;

/// <summary>
/// Validates anti-forgery tokens except in Test environment
/// </summary>
public class ConditionalValidateAntiForgeryTokenAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => true;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();

        // Skip validation in Test environment
        if (env.EnvironmentName.Equals("Test", StringComparison.OrdinalIgnoreCase))
        {
            return new NoOpFilter();
        }

        // Use normal antiforgery validation in other environments
        return new ValidateAntiForgeryTokenAttribute();
    }

    private class NoOpFilter : IFilterMetadata
    {
    }
}

