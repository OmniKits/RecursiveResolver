using System;

public class ResolveRecursionException : Exception
{
    public ResolveRecursionException()
        : base()
    { }
    public ResolveRecursionException(string message)
        : base(message)
    { }
    public ResolveRecursionException(string message, Exception innerException)
        : base(message, innerException)
    { }
}

