

namespace RRSOS_PCC.Classes
{
    /// <summary>
    /// Splits save text on a separator character, but only where the separator is not inside a
    /// JSON string literal. The save format uses '@' between blocks and '|' between entries;
    /// a plain string Split breaks records whose text (sign, note, player name...) contains
    /// either character.
    /// </summary>
    public static class SaveSplitter
    {
        /// <summary>Empty parts are dropped, matching StringSplitOptions.RemoveEmptyEntries.</summary>
        public static List<string> Split(string text, char separator) =>
            Split(text, separator, out _);

        /// <param name="sawSeparator">True if a top-level separator appeared at all, even if it produced no parts.</param>
        public static List<string> Split(string text, char separator, out bool sawSeparator)
        {
            var parts = new List<string>();
            sawSeparator = false;

            int start = 0;
            bool inString = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (inString)
                {
                    if (c == '\\')
                        i++; // skip the escaped character
                    else if (c == '"')
                        inString = false;
                }
                else if (c == '"')
                {
                    inString = true;
                }
                else if (c == separator)
                {
                    sawSeparator = true;

                    if (i > start)
                        parts.Add(text.Substring(start, i - start));

                    start = i + 1;
                }
            }

            if (start < text.Length)
                parts.Add(text.Substring(start));

            return parts;
        }
    }
}
