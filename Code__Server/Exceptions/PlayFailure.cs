/*
 * License:     Creative Commons Attribution-ShareAlike 3.0 unported
 * File:        Exceptions\PlayFailure.cs
 * Authors:     limpygnome              limpygnome@gmail.com
 * 
 * Thrown when a virtual item cannot be played.
 */
using System;
using System.Runtime.Serialization;

namespace UberMediaServer
{
    [Serializable]
    public class PlayFailure : Exception
    {
        public PlayFailure() : base() { }
        public PlayFailure(string message) : base(message) { }
        public PlayFailure(string message, Exception innerException) : base(message, innerException) { }
        public PlayFailure(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}