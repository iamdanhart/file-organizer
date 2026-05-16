using System;
using System.Linq;

namespace file_organizer;

public static class CollectionExtensions
{
    /// <summary>Returns true if the flag (e.g. "--dry-run") is present in the args array.</summary>
    public static bool FlagSet(this string[] args, string flag)
    {
        return args.Contains(flag);
    }

    /// <summary>Returns the index of val in the array, or -1 if not found. Wraps the static Array.IndexOf.</summary>
    public static int Index(this string[] args, string val)
    {
        return Array.IndexOf(args, val);
    }

    /// <summary>
    /// Returns the value following a named parameter (e.g. "--output value").
    /// Returns empty string if the flag is absent, duplicated, at the end of args, or followed by another flag.
    /// </summary>
    public static string GetParamValue(this string[] args, string parameter)
    {
        var count = args.Count(a => a == parameter);
        if (count == 0)
        {
            return "";
        }

        if (count > 1)
        {
            return "";
        }

        var paramKeyIndex = args.Index(parameter);
        if (paramKeyIndex >= args.Length - 1)
        {
            return "";
        }

        // works as long as valid value doesn't start with --
        // otherwise need --param=value pattern
        if (args[paramKeyIndex + 1].StartsWith("--"))
        {
            return "";
        }

        return args[paramKeyIndex + 1];
    }
}