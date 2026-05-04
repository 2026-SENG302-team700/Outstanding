

using SENG302.Api.Models.Entities;

namespace SENG302.Api.Resources;

public class ProfanityTools
{

    public bool ContainsProfanity(string text, User user)
    {
        if (user.ProfanityFiltering)
        {
            var profanityFilter = new ProfanityFilter.ProfanityFilter();
            var swearList = profanityFilter.DetectAllProfanities(text);
            if (swearList.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}