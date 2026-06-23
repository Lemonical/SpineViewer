namespace SpineViewer.Core.Utilities;

internal static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", parameterName);
        }

        return value;
    }

    public static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static T NotNull<T>(T? value, string parameterName)
        where T : class
    {
        return value ?? throw new ArgumentNullException(parameterName);
    }

    public static double PositiveFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be a positive finite number.");
        }

        return value;
    }

    public static double Finite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be a finite number.");
        }

        return value;
    }

    public static int NonNegative(int value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
        }

        return value;
    }

    public static int Positive(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be greater than zero.");
        }

        return value;
    }

    public static TimeSpan NonNegative(TimeSpan value, string parameterName)
    {
        if (value < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
        }

        return value;
    }

    public static TimeSpan Positive(TimeSpan value, string parameterName)
    {
        if (value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be greater than zero.");
        }

        return value;
    }

    public static Guid NonEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Value cannot be an empty GUID.", parameterName);
        }

        return value;
    }

    public static IReadOnlyList<T> MaterializeReadOnlyList<T>(
        IEnumerable<T>? values,
        string parameterName)
    {
        if (values is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        return values.ToArray();
    }

    public static IReadOnlyList<string> MaterializeNonEmptyStrings(
        IEnumerable<string>? values,
        string parameterName)
    {
        if (values is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        List<string> materializedValues = [];

        foreach (string value in values)
        {
            materializedValues.Add(NotNullOrWhiteSpace(value, parameterName));
        }

        return materializedValues;
    }
}
