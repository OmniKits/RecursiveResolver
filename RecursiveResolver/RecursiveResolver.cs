using System;
using System.Collections.Generic;

public abstract class RecursiveResolver<TQuery, TResult>
{
    private class Frame
    {
        public TQuery Key;
        public Frame Parent;
    }

    #region Static Resolve

    public static TResult Resolve(TQuery query, Func<TQuery, Func<TQuery, TResult>, TResult> resolver, Func<TQuery, TQuery, bool> comparer = null)
    {
        if (comparer == null)
            comparer = EqualityComparer<TQuery>.Default.Equals;

        var dict = new Dictionary<TQuery, TResult>();

        Func<TQuery, Frame, TResult> recursive = null;
        recursive = (q, f) =>
        {
            for (var p = f; p != null; p = p.Parent)
            {
                if (comparer(q, p.Key))
                    throw new ResolveRecursionException();
            }

            TResult result;
            if (dict.TryGetValue(q, out result))
                return result;

            f = new Frame { Key = q, Parent = f };

            return dict[q] = resolver(q, r => recursive(r, f));
        };

        return recursive(query, null);
    }

    #endregion

    public abstract bool Equals(TQuery x, TQuery y);
    public abstract TResult Resolve(TQuery t, Func<TQuery, TResult> recursive);
    public TResult Resolve(TQuery query)
    {
        var dict = new Dictionary<TQuery, TResult>();

        Func<TQuery, Frame, TResult> recursive = null;
        recursive = (q, f) =>
        {
            for (var p = f; p != null; p = p.Parent)
            {
                if (Equals(q, p.Key))
                    throw new Exception();
            }

            TResult result;
            if (dict.TryGetValue(q, out result))
                return result;

            f = new Frame { Key = q, Parent = f };

            return dict[q] = Resolve(q, r => recursive(r, f));
        };

        return recursive(query, null);
    }
}

public static class RecursiveResolver<TResult>
{
    public static TResult Resolve<TQuery>(TQuery query, Func<TQuery, Func<TQuery, TResult>, TResult> resolver, Func<TQuery, TQuery, bool> comparer = null)
        => RecursiveResolver<TQuery, TResult>.Resolve(query, resolver, comparer);
}
