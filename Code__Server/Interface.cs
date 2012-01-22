/*
 * License:     Creative Commons Attribution-ShareAlike 3.0 unported
 * File:        Interface.cs
 * Author(s):   limpygnome
 * 
 * Provides a virtual interface for playing media items, inherited by actual interfaces.
 * 
 * Improvements/bugs:
 *          -   none.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace UberMediaServer.Interfaces
{
    public class Interface
    {
        #region "Events"
        public delegate void _Error(string message);
        public delegate void _MediaStart();
        public delegate void _MediaPause(bool nowPaused);
        public delegate void _MediaEnd();
        public delegate void _MediaPositionChange(double posOld, double posNew);
        public virtual event _Error Error;
        public virtual event _MediaStart MediaStart;
        public virtual event _MediaPause MediaPause;
        public virtual event _MediaEnd MediaEnd;
        public virtual event _MediaPositionChange MediaPositionChange;
        #endregion

        #region "Enums"
        public enum States // Any updates need to be copied to the website in Default.aspx.c
        {
            Playing = 1,
            Paused = 3,
            Stopped = 5,
            Error = 7,
            Loading = 9,
            WaitingForInput = 11,
            Idle = 13,
        }
        #endregion

        #region "Variables"
        public Main formMain;
        #endregion

        #region "Methods - virtual"
        /// <summary>
        /// Initializes the new interface.
        /// </summary>
        /// <param name="main">The form/window used for display.</param>
        /// <param name="path">The physical path of the media to play.</param>
        /// <param name="vitemid">The virtual item ID in the database.</param>
        public Interface(Main main, string path, string vitemid) { formMain = main; }
        /// <summary>
        /// Gets the state of the interface.
        /// </summary>
        /// <returns></returns>
        public virtual States State() { return States.Error; }
        /// <summary>
        /// Causes the interface to start playing the current media.
        /// </summary>
        /// <returns></returns>
        public virtual void Play() { }
        /// <summary>
        /// Checks if the media is paused or sets if the media is paused.
        /// </summary>
        public virtual void Pause() { }
        /// <summary>
        /// Causes the interface to stop playing the current media.
        /// </summary>
        public virtual void Stop() { }
        /// <summary>
        /// The volume of the interface between 0.0 to 1.0.
        /// </summary>
        public virtual double Volume { get { return 0; } set { } }
        /// <summary>
        /// Mutes the interface's volume.
        /// </summary>
        public virtual void Mute() { }
        /// <summary>
        ///  Unmutes the interface's volume.
        /// </summary>
        public virtual void Unmute() { }
        /// <summary>
        /// Polls if the interface is muted.
        /// </summary>
        /// <returns>True = muted, false = unmuted.</returns>
        public virtual bool IsMuted() { return true; }
        /// <summary>
        /// The position of the media currently being played.
        /// </summary>
        public virtual double Position { get { return 0; } set { } }
        /// <summary>
        /// The length/duration of the media.
        /// </summary>
        public virtual double Duration { get { return 0; } }
        /// <summary>
        /// Used to carry-out special commands specific to this interface.
        /// </summary>
        public virtual string API(string data) { return null; }
        /// <summary>
        /// Causes the interface to be disposed.
        /// </summary>
        public virtual void Dispose() { }
        #endregion
    }
}