using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;

namespace SENG302.Api.Helpers;

public static class ImageTypeValidation
{
    public static string invalidMimeString = "INVALID MIME";

    /// <summary>
    /// A list of image types that record which magic numbers correspond to which mime types
    /// </summary>
    static readonly Dictionary<string, string> imageTypes = new() {
        {"89504e47", "image/png"},
        {"47494638", "image/gif"},
        {"ffd8ffe0", "image/jpeg"},
        {"ffd8ffe1", "image/jpeg"},
        {"ffd8ffe2", "image/jpeg"},
        {"ffd8ffe3", "image/jpeg"},
        {"ffd8ffe8", "image/jpeg"},
        {"3c737667", "image/svg+xml"},
        {"52494646????????57454250", "image/webp"}
    };

    /// <summary>
    /// Utilizes the first 4-12 bytes of a file to check the real mime type of the file
    /// It will return the real mime type, or "INVALID MIME" if it is not one of the valid types
    /// </summary>
    /// <param name="file">The file to be checked</param>
    /// <returns>The real mime type based on the first 4-12 bytes of the file if it is valid, else "INVALID MIME"</returns>
    public static string GetRealImageMime(IFormFile file)
    {
        var stream = file.OpenReadStream();

        byte[] magicNumber = new byte[12];

        var bytesRead = stream.Read(magicNumber, 0, magicNumber.Length);

        string fileMagicNumber = Convert.ToHexStringLower(magicNumber);

        foreach (var (refrMagicNumber, mimeType) in imageTypes)
        {

            var valid = true;

            for (int i = 0; i < fileMagicNumber.Length / 8; i++)
            {
                if (i * 8 >= refrMagicNumber.Length) break;

                var fileByteStr = fileMagicNumber.Substring(i * 8, 8);
                var refrByteStr = refrMagicNumber.Substring(i * 8, 8);

                if (!(refrByteStr == "????????" || fileByteStr == refrByteStr))
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                return mimeType;
            }
        }

        return invalidMimeString;
    }
}