/// <summary>
/// Exception to throw when e-mail already exists in the db.
/// </summary>
public class DuplicateEmailException : Exception
{
    public DuplicateEmailException() { }

    public DuplicateEmailException(string message) : base(message) { }

    public DuplicateEmailException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Exception to throw when the display name has an invalid name length.
/// </summary>
public class InvalidLengthException : Exception
{
    public InvalidLengthException() { }

    public InvalidLengthException(string message) : base(message) { }

    public InvalidLengthException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Exception to throw when the display name has invalid characters
/// </summary>
public class InvalidDisplayNameCharsException : Exception
{
    public InvalidDisplayNameCharsException() { }

    public InvalidDisplayNameCharsException(string message) : base(message) { }

    public InvalidDisplayNameCharsException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Exception to throw when the email is of an invalid format
/// </summary>
public class InvalidEmailFormatException : Exception
{
    public InvalidEmailFormatException() { }

    public InvalidEmailFormatException(string message) : base(message) { }

    public InvalidEmailFormatException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Exception to throw when the password formatting is invalid (doesn't meet requirements)
/// </summary>
public class InvalidPasswordException : Exception
{
    public InvalidPasswordException() { }

    public InvalidPasswordException(string message) : base(message) { }

    public InvalidPasswordException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Exception to throw when passwords are mismatched
/// </summary>
public class MismatchedPasswordException : Exception
{
    public MismatchedPasswordException() { }

    public MismatchedPasswordException(string message) : base(message) { }

    public MismatchedPasswordException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Exception to throw when the country code given from the front-end is invalid
/// </summary>
public class InvalidCountryException : Exception
{
    public InvalidCountryException() { }

    public InvalidCountryException(string message) : base(message) { }

    public InvalidCountryException(string message, Exception inner) : base(message, inner) { }
}