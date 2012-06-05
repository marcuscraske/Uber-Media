/*
 * License:     Creative Commons Attribution-ShareAlike 3.0 unported
 * File:        Exceptions\LibraryConnectionFailure.cs
 * Authors:     limpygnome              limpygnome@gmail.com
 * 
 * Thrown when data cannot be fetched from the media library.
 */
using System;
using System.Runtime.Serialization;

namespace UberMediaServer
{
    [Serializable]
    public class LibraryConnectionFailure : Exception
    {
        public LibraryConnectionFailure() : base() { }
        public LibraryConnectionFailure(string message) : base(message) { }
        public LibraryConnectionFailure(string message, Exception innerException) : base(message, innerException) { }
        public LibraryConnectionFailure(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}