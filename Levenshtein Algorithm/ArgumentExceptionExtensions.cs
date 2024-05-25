namespace Levenshtein_Algorithm.Extensions
{
    public static class ArgumentExceptionExtensions
    {
        public static void ThrowIfInvalidMaxItems(
            int maxItems,
            string paramName
        )
        {
            if (maxItems <= 0)
            {
                throw new ArgumentException($"{paramName} must be greater than zero", paramName);
            }
        }
    }
}
