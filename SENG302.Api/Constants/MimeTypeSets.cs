namespace SENG302.Api.Constants;

public class MimeTypeSets
{
    /// <summary>
    /// Mime types allowed for images in the app.
    /// </summary>
    public static readonly IReadOnlyList<string> Images = new List<string>
    {
        "image/webp",
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/svg+xml",
    };
}