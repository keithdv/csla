﻿namespace Csla.Serialization.Mobile
{
  /// <summary>
  /// Represents a class that can be used to write a list of <see cref="SerializationInfo"/> objects
  /// into a stream, typically MemoryStream
  /// </summary>
  public interface ICslaWriter
  {
    /// <summary>
    /// Write a list of <see cref="SerializationInfo"/> objects
    /// into a stream, typically MemoryStream
    /// </summary>
    /// <param name="serializationStream">Stream to write the data into</param>
    /// <param name="objectData">List of <see cref="SerializationInfo"/> objects to write</param>
    /// <exception cref="ArgumentNullException"><paramref name="serializationStream"/> or <paramref name="objectData"/> is <see langword="null"/>.</exception>
    void Write(Stream serializationStream, List<SerializationInfo> objectData);
  }
}
