using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX.AudioVideoPlayback;

namespace UberMediaServer.Interfaces
{
    public class video_dx : Interface
    {
        public override event Interface._Error Error;
        public override event Interface._MediaStart MediaStart;
        public override event Interface._MediaPause MediaPause;
        public override event Interface._MediaEnd MediaEnd;
        public override event Interface._MediaPositionChange MediaPositionChange;
        Video v;
        bool error = false;
        bool idle = false;
        Main _main;
        public video_dx(Main main, string path, string vitemid) : base(main, path, vitemid)
        {
            _main = main;
            try
            {
                v = new Video(path, true);
                _main.Invoke((MethodInvoker)delegate()
                {
                    v.Owner = main.panel1;
                    HookEvents();
                });
            }
            catch(Exception ex)
            {
                error = true;
                if (Error != null) Error("Could not load media - " + ex.Message);
            }
        }
        void HookEvents()
        {   // Events have to be rehooked when changing audio etc, as detailed here:
            // http://social.msdn.microsoft.com/Forums/en/direct3d/thread/0d4abfd2-0d04-47af-8a1c-25953846a174
            v.Ending += new EventHandler(v_Ending);
            v.Audio.Ending += new EventHandler(v_Ending);
        }
        void v_Ending(object sender, EventArgs e)
        {
            if(MediaEnd != null) MediaEnd();
            idle = true;
        }
        public override void Play()
        {
            if (v == null) return;
            _main.Invoke((MethodInvoker)delegate()
            {
                if (v.State != StateFlags.Running)
                {
                    bool paused = v.State == StateFlags.Paused;
                    v.Play();
                    if (paused) if(MediaPause != null) MediaPause(false);
                    else if(MediaStart != null) MediaStart();
                }
            });
        }
        public override void Stop()
        {
            if (v == null) return;
            _main.Invoke((MethodInvoker)delegate()
            {
                if (v.State != StateFlags.Stopped)
                {
                    v.Stop();
                }
            });
        }
        public override void Pause()
        {
            if (v == null) return;
            _main.Invoke((MethodInvoker)delegate()
            {
                if (v.State == StateFlags.Running)
                {
                    v.Pause();
                    if (MediaPause != null) MediaPause(true);
                }
            });
        }
        double unmutedVolume = -1;
        public override double Volume
        { // Range -10000 to 0
            get
            {
                try
                {
                    return ((double)v.Audio.Volume + 10000.0) / 10000.0;
                }
                catch { return 1; }
            }
            set
            {
                // Range protection
                if (value < 0) value = 0;
                else if (value > 1) value = 1;
                // Set audio - wrapped in-case e.g. spammed and an error occurs
                try
                {
                    _main.Invoke((MethodInvoker)delegate()
                    {
                        if (unmutedVolume != -1)
                            unmutedVolume = value;
                        else
                        {
                            SetVolume(value);
                            HookEvents();
                        }
                    });
                }
                catch { }
            }
        }
        void SetVolume(double amount)
        {
            v.Audio.Volume = -10000 + (int)(amount * 10000.0);
        }
        public override bool IsMuted()
        {
            return unmutedVolume != -1;
        }
        public override void Mute()
        {
            if (unmutedVolume != -1) return;
            unmutedVolume = Volume;
            SetVolume(0);
        }
        public override void Unmute()
        {
            if (unmutedVolume == -1) return;
            SetVolume(unmutedVolume);
            unmutedVolume = -1;
        }
        public override Interface.States State()
        {
            if (error) return States.Error;
            if (idle || v == null) return States.Idle;
            switch (v.State)
            {
                case StateFlags.Running: return States.Playing;
                case StateFlags.Paused: return States.Paused;
                case StateFlags.Stopped: return States.Stopped;
            }
            return States.Error;
        }
        public override double Position
        {
            get
            {
                return v.CurrentPosition;
            }
            set
            {
                try
                {
                    if (value > Duration) return;
                    double old = v.CurrentPosition;
                    _main.Invoke((MethodInvoker)delegate()
                    {
                        v.CurrentPosition = value;
                    });
                    if(MediaPositionChange != null) MediaPositionChange(old, value);
                }
                catch { }
            }
        }
        public override double Duration
        {
            get
            {
                return v.Duration;
            }
        }
        public override void Dispose()
        {
            if (v == null) return;
            // Using fixes mentioned at:
            // http://blogs.msdn.com/b/toub/archive/2004/04/16/114630.aspx
            // This is terrible...memory does not get deallocated;
            // consider using alternative
            PictureBox pb = new PictureBox();
            pb.Visible = false;
            v.Owner = pb;
            v.Ending -= v_Ending;
            Audio a = v.Audio;
            a.Ending -= v_Ending;
            a.Dispose();
            a = null;
            v.Dispose();
            v = null;
        }
    }
}
