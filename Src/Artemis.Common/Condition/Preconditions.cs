using System;
namespace Com.Ctrip.Soa.Artemis.Common.Condition
{
    public class Preconditions
    {
        public static void CheckArgument(bool condition, string errorMessage)
        {
            if (!condition)
            {
                throw new ArgumentException(errorMessage);
            }
        }

        public static void CheckArgument(bool condition)
        {
            CheckArgument(condition, string.Empty);
        }

        public static void NotNull(Object value, String name)
        {
            CheckArgument(value != null, name + " should not be null");
        }

        public static void NotNullOrEmpty(object[] value, String name)
        {
            CheckArgument(value != null && value.Length != 0, name + " should not be null");
        }

        public static void NotNullOrWhiteSpace(String value, String name)
        {
            CheckArgument(!string.IsNullOrWhiteSpace(value), name + " should not be null or whitespace");
        }
    }
}
