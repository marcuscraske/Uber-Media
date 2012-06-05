/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /Exceptions/LibraryConnectionFailure.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
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