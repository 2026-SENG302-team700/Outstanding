namespace SENG302.Api.Resources.Helpers;

public static class ProfanityTools
{
    /// <summary>
    /// given some text and a boolean, this method uses the profanity filter
    /// to detect whether profanity is present in the provided text.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="profanityFiltering"></param>
    /// <returns>bool</returns>
    public static bool ContainsProfanity(string text, bool profanityFiltering)
    {
        if (profanityFiltering)
        {
            var profanityFilter = new ProfanityFilter.ProfanityFilter();
            var swearList = profanityFilter.DetectAllProfanities(text);
            if (swearList.Count > 0)
            {
                return true;
            }

        }
        return false;
    }
}