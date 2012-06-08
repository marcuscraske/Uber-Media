/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /Interfaces/YouTube/video_vlc.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 * Provides an interface for playing general audio and video media through VLC.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Declarations;
using Declarations.Events;
using Declarations.Media;
using Declarations.Players;
using Implementation;

namespace UberMediaServer.Interfaces
{
    public class video_vlc : Interface
    {
        #region "Variables"
        private IMediaPlayerFactory m_factory;
        private IDiskPlayer m_player;
        private IMedia m_media;
        private float duration = 0;
        private MediaState currentState = MediaState.Opening;
        #endregion

        #region "Events"
        public override event Interface._MediaEnd MediaEnd;
        public override event Interface._Error Error;
        #endregion

        #region "Methods - Constructors"
        public video_vlc(Main main, string path, string vitemid)
            : base(main, path, vitemid)
        {
            try
            {
                main.Invoke((MethodInvoker)delegate()
                {
                    // Create player
                    m_factory = new MediaPlayerFactory();
                    m_player = m_factory.CreatePlayer<IDiskPlayer>();
                    m_player.Events.MediaEnded += new EventHandler(Events_MediaEnded);
                    m_player.Events.PlayerStopped += new EventHandler(Events_PlayerStopped);
                    m_player.WindowHandle = main.panel1.Handle;
                });
                main.Invoke((MethodInvoker)delegate()
                {
                    // Open media
                    m_media = m_factory.CreateMedia<IMedia>(path);
                    m_media.Events.DurationChanged += new EventHandler<MediaDurationChange>(Events_DurationChanged);
                    m_media.Events.StateChanged += new EventHandler<MediaStateChange>(Events_StateChanged);
                    m_player.Open(m_media);
                    m_media.Parse(true);
                    m_player.Play();
                });
            }
            catch(Exception ex)
            {
                if (Error != null) Error("Could not load media - " + ex.Message + "!");
            }
        }
        #endregion

        #region "Methods - Events"
        void Events_StateChanged(object sender, MediaStateChange e)
        {
            currentState = e.NewState;
        }
        void Events_DurationChanged(object sender, MediaDurationChange e)
        {
            duration = e.NewDuration;
        }
        void Events_PlayerStopped(object sender, EventArgs e)
        {
            if (MediaEnd != null) MediaEnd();
        }
        void Events_MediaEnded(object sender, EventArgs e)
        {
            if (MediaEnd != null) MediaEnd();
        }
        #endregion

        #region "Methods - Overrides"
        public override void Play()
        {
            if (!m_player.IsPlaying) m_player.Play();
        }
        public override void Pause()
        {
            m_player.Pause();
        }
        public override void Stop()
        {
            m_player.Stop();
        }
        public override double Duration
        {
            get
            {
                return duration / 1000;
            }
        }
        public override double Volume
        {
            get
            {
                return (double)m_player.Volume / 100.0;
            }
            set
            {
                m_player.Volume = (int)(value * 100.0);
            }
        }
        public override bool IsMuted()
        {
            return m_player.Mute;
        }
        public override void Mute()
        {
            m_player.Mute = true;
        }
        public override void Unmute()
        {
            m_player.Mute = false;
        }
        public override States State()
        {
            switch (currentState)
            {
                case MediaState.Buffering:
                case MediaState.Opening:
                    return States.Loading;
                case MediaState.Playing:
                    return States.Playing;
                case MediaState.Paused:
                    return States.Paused;
                case MediaState.Ended:
                case MediaState.Stopped:
                    return States.Stopped; ;
                case MediaState.Error:
                case MediaState.NothingSpecial:
                default:
                    return States.Error;
            }
        }
        public override double Position
        {
            get
            {
                return (m_player.Position * duration) / 1000;
            }
            set
            {
                m_player.Position = ((float)value / duration) * 1000;
            }
        }
        public override void Dispose()
        {
            formMain.Invoke((MethodInvoker)delegate()
            {
                m_player.WindowHandle = IntPtr.Zero;
                if (m_player.IsPlaying) m_player.Stop();
                m_media.Dispose();
                m_player.Dispose();
                m_factory.Dispose();
            });
        }
        #endregion
    }
}