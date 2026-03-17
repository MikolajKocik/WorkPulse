namespace WorkPulse.Utils;

/// <summary>
/// Provides utility methods for filtering and processing collections of data.
/// </summary>
/// <remarks>The <see cref="DataUtils"/> class contains static methods designed to assist with common data
/// manipulation tasks, such as filtering lists based on string queries. All methods operate on generic collections and
/// rely on standard .NET conventions for object representation and comparison. This class is intended for use in
/// scenarios where simple, reusable data utilities are needed without instantiating objects.</remarks>
public static class DataUtils
{
    /// <summary>
    /// Filters a list of objects by matching their string representation against a specified query.
    /// </summary>
    /// <remarks>The filtering is performed using each object's <see cref="object.ToString"/> method. Ensure
    /// that the <see cref="object.ToString"/> implementation provides meaningful output for filtering
    /// purposes.</remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">The list of objects to filter. Cannot be <c>null</c>.</param>
    /// <param name="query">The string to search for within each object's string representation. If <paramref name="query"/> is <c>null</c>,
    /// empty, or whitespace, the original list is returned unfiltered.</param>
    /// <returns>A list containing only the objects whose string representation contains the specified query, using a
    /// case-insensitive comparison. If no objects match, an empty list is returned.</returns>
    public static List<T> FilterData<T>(List<T> data, string query) where T : class 
    {
        if (string.IsNullOrWhiteSpace(query)) return data;

        return data.Where(item =>
            item.ToString()!.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
