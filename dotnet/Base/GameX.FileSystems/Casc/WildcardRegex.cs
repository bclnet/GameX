﻿using System.Text.RegularExpressions;

namespace GameX.FileSystems.Casc
{
    /// <summary>
    /// Represents a wildcard running on the
    /// <see cref="System.Text.RegularExpressions"/> engine.
    /// </summary>
    public class WildcardRegex : Regex
    {
        /// <summary>
        /// Initializes a wildcard with the given search pattern.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to match.</param>
        public WildcardRegex(string pattern, bool matchStartEnd) : base(WildcardToRegex(pattern, matchStartEnd)) { }

        /// <summary>
        /// Initializes a wildcard with the given search pattern and options.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to match.</param>
        /// <param name="options">A combination of one or more
        /// <see cref="RegexOptions"/>.</param>
        public WildcardRegex(string pattern, bool matchStartEnd, RegexOptions options) : base(WildcardToRegex(pattern, matchStartEnd), options) { }

        /// <summary>
        /// Converts a wildcard to a regex.
        /// </summary>
        /// <param name="pattern">The wildcard pattern to convert.</param>
        /// <returns>A regex equivalent of the given wildcard.</returns>
        public static string WildcardToRegex(string pattern, bool matchStartEnd)
            => matchStartEnd
            ? $"^{Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".")}$"
            : Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".");
    }
}
