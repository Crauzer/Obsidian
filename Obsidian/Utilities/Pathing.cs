namespace Obsidian.Utilities
{
    public static class Pathing
    {
        public static char GetPathSeparator(string path)
        {
            if (path.Contains('\\'))
            {
                return '\\';
            }
            else
            {
                return '/';
            }
        }
        public static char GetInvertedPathSeparator(char separator)
        {
            if (separator == '\\')
            {
                return '/';
            }
            else
            {
                return '\\';
            }
        }

        public static bool ContainsPathSeparator(string path)
        {
            if(path.Contains('\\') || path.Contains('/'))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
