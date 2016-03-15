using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

public class Tests
{
    static readonly Regex MatchBegin = new Regex(@"^\<(.*?)\>", RegexOptions.ECMAScript);
    static readonly Regex MatchAny = new Regex(@"\<(.*?)\>", RegexOptions.ECMAScript);

    static Dictionary<string, string> UriMapings = new Dictionary<string, string>
    {
        { "", "http://i.example.com/" },
        { "i", "i.example.com" },
        { "api", "<>api" },
        { "auth", "auth.<i>" },
        { "no-api", "<api>/no-api" },
        { "i0", "http:i.example.com" },
        { "i1", "http://i.example.com" },
        { "r0", "<r0>" },
        { "r1", "<r0>" },
        { "r2", "<r4>" },
        { "r3", "<r2>" },
        { "r4", "<r3>" },
    };

    static Uri CreateUri(string input)
    {
        Uri result;
        if (Uri.TryCreate(input, UriKind.Absolute, out result))
            return result;

        var p = input.IndexOf(':');

        var q = input.IndexOf('/');
        if (p < 0 || (q >= 0 && p > q))
            return new Uri(input, UriKind.Relative);

        return new Uri(input.Substring(0, p) + "://" + input.Substring(++p));
    }
    [Fact]
    public void ForUriCreation()
    {
        Assert.Equal(new Uri("http://asd"), CreateUri("http://asd"));
        Assert.Equal(new Uri("my:asd"), CreateUri("my:asd"));
        Assert.Equal(new Uri("asd", UriKind.Relative), CreateUri("asd"));
        Assert.Equal(new Uri("./http:asd", UriKind.Relative), CreateUri("./http:asd"));
        Assert.Equal(new Uri("http://asd"), CreateUri("http:asd"));
    }

    static string Resolve(string query, Func<string, string> recursion)
    {
        string part;
        if (!UriMapings.TryGetValue(query, out part))
            part = UriMapings[null];

        part = MatchAny.Replace(part, _ => recursion(_.Groups[1].Value));

        var uri = CreateUri(part);
        if (uri.IsAbsoluteUri)
            return uri.AbsoluteUri;

        return part;
    }

    [Theory]
    [InlineData("", "http://i.example.com/")]
    [InlineData("i", "i.example.com")]
    [InlineData("api", "http://i.example.com/api")]
    [InlineData("auth", "auth.i.example.com")]
    //[InlineData("no-api", "http://i.example.com/no-api")]
    [InlineData("no-api", "http://i.example.com/api/no-api")]
    [InlineData("i0", "http://i.example.com/")]
    [InlineData("i1", "http://i.example.com/")]
    public void ForResolveByKey(string query, string result)
    {
        Assert.Equal(result, RecursiveResolver<string>.Resolve(query, Resolve));
    }

    [Theory]
    [InlineData("r0")]
    [InlineData("r1")]
    [InlineData("r2")]
    [InlineData("r3")]
    [InlineData("r4")]
    public void ForRecursion(string query)
    {
        Assert.Throws<ResolveRecursionException>(() => RecursiveResolver<string>.Resolve(query, Resolve));
    }
}

