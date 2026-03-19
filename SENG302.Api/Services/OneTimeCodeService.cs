using System;
using System.Security.Cryptography;
using SENG302.Api.DataAccess;
using Microsoft.EntityFrameworkCore;
// using System.Timers.Timer;

namespace SENG302.Api.Services;

public interface IOneTimeCodeService
{
    public int generateOneTimeCode();
}

public class OneTimeCodeService : IOneTimeCodeService
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly TimeProvider _timeProvider;

    private static Timer timer;
    
    
    public OneTimeCodeService(IDbContextFactory<DatabaseContext> dbContextFactory, TimeProvider timeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _timeProvider = timeProvider;
    }

    public int generateOneTimeCode()
    {
        byte[] bytes = new byte[4];
        // Modifies array of bytes with random crytographically secure bytes
        RandomNumberGenerator.Fill(bytes);

        int value = BitConverter.ToInt32(bytes, 0);
        // Convert to 6 digits and pads with zeroes if necessary
        string paddedInt = Math.Abs(value % 1000000).ToString("D6");
        int oneTimeCode = int.Parse(paddedInt);

        return oneTimeCode;
    }

    // public startCodeTimer()
    // {
    //     
    // }
}