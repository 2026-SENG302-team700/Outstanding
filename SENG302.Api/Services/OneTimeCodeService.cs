using System;
using System.Security.Cryptography;
using SENG302.Api.DataAccess;
using Microsoft.EntityFrameworkCore;


namespace SENG302.Api.Services;

public interface IOneTimeCodeService
{
    public string GenerateOneTimeCode();
    public int GetEpochTime();
    public bool CompareTimes(int startTime, int endTime);
    public bool CompareCodes(string enteredCode, string originalCode);
}

public class OneTimeCodeService : IOneTimeCodeService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly TimeProvider _timeProvider;
    
    // Static variable representing the time limit for the code to be entered in
    private static int timeoutTimeSeconds = 120;
    
    public OneTimeCodeService(IDbContextFactory<DatabaseContext> dbContextFactory, TimeProvider timeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _timeProvider = timeProvider;
        

    }

    /// <summary>
    /// Generates a six digit one time code to be used
    /// </summary>
    /// <returns></returns>
    public string GenerateOneTimeCode()
    {
        byte[] bytes = new byte[4];
        // Modifies array of bytes with random crytographically secure bytes
        RandomNumberGenerator.Fill(bytes);

        int value = BitConverter.ToInt32(bytes, 0);
        // Convert to 6 digits and pads with zeroes if necessary
        string sixDigitCode = Math.Abs(value % 1000000).ToString("D6");

        return sixDigitCode;
    }

    /// <summary>
    /// Returns the Unix time or Epoch time of the server which is the number of seconds passed since Jan 1st 1970 midnight
    /// </summary>
    /// <returns>int representing amount of seconds since Jan 1st 1970</returns>
    public int GetEpochTime()
    {
        DateTimeOffset currentTime = _timeProvider.GetUtcNow();
        TimeSpan currentEpochTime = currentTime - DateTimeOffset.UnixEpoch;
        int epochTimeSeconds = (int)currentEpochTime.TotalSeconds;
        
        // currentEpochTime
        return epochTimeSeconds;
    }
    
    /// <summary>
    /// Compares the start and end time to see if it is under the time limit specified in timeoutTimeSeconds.
    /// </summary>
    /// <param name="startTime"></param> The time the code was generated (stored in the database)
    /// <param name="endTime"></param> The time that the user called the api/register/code/validation endpoint in the
    /// Registration Controller.
    /// <returns>
    /// A boolean indicating if the code was entered in time or not
    /// </returns>
    public bool CompareTimes(int startTime, int endTime)
    {
        return endTime - startTime < timeoutTimeSeconds;
    }
    
    /// <summary>
    /// Compares the two codes passed in
    /// </summary>
    /// <param name="enteredCode"></param> This is the code that the user entered
    /// <param name="originalCode"></param> The original code that was generated
    /// <returns>
    /// Returns a boolean indicating if the codes are equal
    /// </returns>
    public bool CompareCodes(string enteredCode, string originalCode)
    {
        return enteredCode == originalCode;
    }
}