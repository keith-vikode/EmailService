using System.Text.RegularExpressions;

namespace System
{
    public static class StringExtensions
    {
        private const string WordSeparator = " $0";
        private static readonly Regex WordRegex = new Regex(@"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", RegexOptions.Compiled);

        public static string Decamelise(this string value)
        {
            if (value == null)
            {
                return null;
            }

            return WordRegex.Replace(value, WordSeparator);
        }

        public static string Decamelise(this Enum value) => value.ToString().Decamelise();
    }
}
