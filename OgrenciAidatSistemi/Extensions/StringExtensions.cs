public static class StringExtensions
{
    public static string SanitizeString(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        // Define the characters to remove
        string[] charsToRemove = [";", "'", "\"", "=", "--", "/*", "*/", "xp_", "sp_"];

        // Remove the characters
        foreach (string c in charsToRemove)
        {
            str = str.Replace(c, string.Empty);
        }

        return str;
    }

    public static string ToSanitizedLowercase(this string str)
    {
        return str.SanitizeString().ToLower();
    }
}
