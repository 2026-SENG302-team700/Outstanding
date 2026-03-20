using Xunit;
using NSubstitute;
using SENG302.Api.Services;

namespace SENG302.Api.Tests.Unit.Services;

public class EmailServiceUnitTests
{
    private readonly IEmailService _email = Substitute.For<IEmailService>();
    private readonly UserService _service;
}