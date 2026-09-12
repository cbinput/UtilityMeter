namespace CleanMinimalApi.Infrastructure.Databases.UtilityMeter.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;

internal static class ValueComparers
{
    public static readonly ValueComparer<List<Guid>> GuidListComparer = new(
        (c1, c2) => c1 != null && c2 != null ? c1.SequenceEqual(c2) : c1 == c2,
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
        c => c.ToList());

    public static readonly ValueComparer<Dictionary<string, string>> StringDictionaryComparer = new(
        (c1, c2) => c1 != null && c2 != null ? c1.Count == c2.Count && !c1.Except(c2).Any() : c1 == c2,
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.GetHashCode())),
        c => c.ToDictionary(k => k.Key, v => v.Value));
}
