using System;
using System.Security.Cryptography;
using SENG302.Api.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Timer = System.Timers.Timer;


namespace SENG302.Api.Services;

public interface IOneTimeCodeService
{
    public string GenerateOneTimeCode();
    public int GetEpochTime();
}

public class OneTimeCodeService : IOneTimeCodeService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly TimeProvider _timeProvider;
    private readonly MemoryCache _memoryCache;

    private static Timer timer;
    
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
}