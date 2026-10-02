namespace WineFilesApi.Application.Services;

/// <summary>
/// Port of the FoxPro unit conversion routines:
///   GNCHGUNT  — normalize a value to be between 1 and 1000
///   GNSMLUNT  — convert a value to the smallest unit in the chain
///   gnuntmch  — convert a value from one unit to another
///   GNUNITLIST — build the chain of units leading to a given unit
/// </summary>
public static class UnitConverter
{
    /// <summary>
    /// The gaunits array — smaller unit, larger unit, factor.
    /// Order matters: each row represents a step in a chain.
    /// Populate from the `units` FoxPro table for a complete set.
    /// </summary>
    private static readonly (string Small, string Large, decimal Factor)[] DefaultUnits =
    [
        // Volume
        ("ML", "L",    1000m),
        ("L",  "HL",   100m),
        ("HL", "KL",   10m),
        ("KL", "ML",   1000000m),   // not used; guard ensures loop ends

        // Weight
        ("MG", "G",    1000m),
        ("G",  "KG",   1000m),
        ("KG", "T",    1000m),

        // Concentration / misc
        ("PPM","PCT",  10000m),
        ("PCT","PPM",  0.0001m)
    ];

    /// <summary>
    /// Normalize a value so it sits between 1 and 1000, adjusting the unit.
    /// Equivalent to GNCHGUNT.
    /// </summary>
    public static (decimal Value, string Unit) Normalize(decimal value, string unit)
    {
        if (value == 0) return (value, unit);
        if (unit == "EA") return (value, unit);

        // Multiply up while value < 1
        while (value < 1m)
        {
            var row = FindByLargeUnit(unit);
            if (row is null) break;
            value *= row.Value.Factor;
            unit = row.Value.Small;
        }

        // Divide down while value >= 1000
        while (value >= 1000m)
        {
            var row = FindBySmallUnit(unit);
            if (row is null) break;
            value /= row.Value.Factor;
            unit = row.Value.Large;
        }

        return (value, unit);
    }

    /// <summary>
    /// Convert a value down to the smallest unit in the chain.
    /// Equivalent to GNSMLUNT.
    /// </summary>
    public static (decimal Value, string Unit) ToSmallest(decimal value, string unit)
    {
        while (true)
        {
            var row = FindByLargeUnit(unit);
            if (row is null) break;
            value *= row.Value.Factor;
            unit = row.Value.Small;
        }
        return (value, unit);
    }

    /// <summary>
    /// Convert a value from one unit to another.
    /// Equivalent to gnuntmch.
    /// </summary>
    public static decimal Convert(decimal amount, string oldUnit, string newUnit)
    {
        if (string.IsNullOrWhiteSpace(newUnit)) return amount;

        // Try forward path
        var forward = TryForward(amount, oldUnit, newUnit);
        if (forward is not null) return forward.Value;

        // Try backward path
        var backward = TryBackward(amount, oldUnit, newUnit);
        if (backward is not null) return backward.Value;

        return amount;   // couldn't convert
    }

    /// <summary>
    /// Build the chain of units from the given unit up to the largest.
    /// Equivalent to GNUNITLIST.
    /// </summary>
    public static List<string> GetUnitChain(string unit)
    {
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(unit)) return list;

        // Walk down to smallest
        var (_, small) = ToSmallest(1m, unit);
        var current = small;

        while (!string.IsNullOrEmpty(current))
        {
            list.Add(current);
            var row = FindBySmallUnit(current);
            if (row is null) break;
            current = row.Value.Large;
        }
        return list;
    }

    // -------------------- Private helpers --------------------

    private static (string Small, string Large, decimal Factor)? FindByLargeUnit(string unit) =>
        DefaultUnits.FirstOrDefault(r =>
            string.Equals(r.Large, unit, StringComparison.OrdinalIgnoreCase)) is var r && r != default
            ? r
            : null;

    private static (string Small, string Large, decimal Factor)? FindBySmallUnit(string unit) =>
        DefaultUnits.FirstOrDefault(r =>
            string.Equals(r.Small, unit, StringComparison.OrdinalIgnoreCase)) is var r && r != default
            ? r
            : null;

    private static decimal? TryForward(decimal amount, string oldUnit, string newUnit)
    {
        var current = oldUnit;
        while (true)
        {
            var row = FindBySmallUnit(current);
            if (row is null) return null;
            amount /= row.Value.Factor;
            if (string.Equals(row.Value.Large, newUnit, StringComparison.OrdinalIgnoreCase))
                return amount;
            current = row.Value.Large;
        }
    }

    private static decimal? TryBackward(decimal amount, string oldUnit, string newUnit)
    {
        var current = oldUnit;
        while (true)
        {
            var row = FindByLargeUnit(current);
            if (row is null) return null;
            amount *= row.Value.Factor;
            if (string.Equals(row.Value.Small, newUnit, StringComparison.OrdinalIgnoreCase))
                return amount;
            current = row.Value.Small;
        }
    }
}