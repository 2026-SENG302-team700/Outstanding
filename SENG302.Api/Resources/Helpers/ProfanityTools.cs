using SENG302.Api.Models.Entities;

namespace SENG302.Api.Resources.Helpers;

public class ProfanityTools
{

    public bool ContainsProfanity(string text, bool profanityFiltering)
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