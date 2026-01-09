namespace Terrarium.Core.Models.Data;

/// <summary>
/// A generic container for wrapping data with metadata such as versioning and timestamps.
/// Useful for serialization and data portability.
/// </summary>
/// <typeparam name="T">The type of the data being contained.</typeparam>
public class DataContainer<T>
{
    public int Version { get; set; } = 1;
    public DateTime LastSaved { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the actual data payload.
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataContainer{T}"/> class.
    /// </summary>
    public DataContainer() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataContainer{T}"/> class with specified data and version.
    /// </summary>
    /// <param name="data">The data payload.</param>
    /// <param name="version">The version of the data.</param>
    public DataContainer(T data, int version)
    {
        Data = data;
        Version = version;
    }
}