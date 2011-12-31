using System;
using System.Collections.Generic;
using System.Text;
using WMPLib;
using System.Windows.Forms;
using Microsoft.Win32;

namespace UberMediaServer.Interfaces
{
    public class video_wmp : Interface
    {
        public static bool used = false;
        public override event Interface._MediaEnd MediaEnd;
        AxWMPLib.AxWindowsMediaPlayer p;
        public video_wmp(Main main, string path, string vitemid) : base(main, path, vitemid)
        {
            try
            {
                if (!used)
                {
                    // Set the virtualization
                    used = true;
                    try // This is unimportant, we wont stop the interface from playing if it errors...
                    { ChangeVisualization("Bars", 3); } // Bars > Scope
                    catch { }
                }
                main.Invoke((MethodInvoker)delegate()
                {
                    p = new AxWMPLib.AxWindowsMediaPlayer();
                    p.EndOfStream += new AxWMPLib._WMPOCXEvents_EndOfStreamEventHandler(p_EndOfStream);
                    p.PlayStateChange += new AxWMPLib._WMPOCXEvents_PlayStateChangeEventHandler(p_PlayStateChange);
                    main.panel1.Controls.Add(p);
                    p.Dock = System.Windows.Forms.DockStyle.Fill;
                    p.uiMode = "none";
                    p.settings.enableErrorDialogs = false;
                    p.URL = path;
                    p.stretchToFit = true;
                });
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + ": " + ex.StackTrace);
            }
        }
        void p_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (p.playState == WMPPlayState.wmppsMediaEnded)
                if (MediaEnd != null) MediaEnd();
        }
        void p_EndOfStream(object sender, AxWMPLib._WMPOCXEvents_EndOfStreamEvent e)
        {
            if (MediaEnd != null) MediaEnd();
        }
        public override void Play()
        {
            if (p.playState != WMPPlayState.wmppsPlaying)
                p.Ctlcontrols.play();
        }
        public override void Pause()
        {
            if (p.playState == WMPPlayState.wmppsPlaying)
                p.Ctlcontrols.pause();
        }
        public override void Stop()
        {
            p.Ctlcontrols.stop();
        }
        public override double Duration
        {
            get
            {
                return p.currentMedia.duration;
            }
        }
        public override double Volume
        {
            get
            {
                return (double)p.settings.volume / 100;
            }
            set
            {
                if (value < 0.0 || value > 1.0) return;
                p.settings.volume = (int)(value * 100);
            }
        }
        public override bool IsMuted()
        {
            return p.settings.mute;
        }
        public override void Mute()
        {
            p.settings.mute = true;
        }
        public override void Unmute()
        {
            p.settings.mute = false;
        }
        public override States State()
        {
            switch (p.playState)
            {
                case WMPPlayState.wmppsPlaying:
                    return States.Playing;
                case WMPPlayState.wmppsPaused:
                    return States.Paused;
                case WMPPlayState.wmppsStopped:
                    return States.Stopped;
                case WMPPlayState.wmppsReady | WMPPlayState.wmppsMediaEnded:
                    return States.Idle;
                case WMPPlayState.wmppsBuffering:
                    return States.Loading;
                case WMPPlayState.wmppsWaiting:
                    return States.WaitingForInput;
                default: return States.Error;
            }
        }
        public override double Position
        {
            get
            {
                return p.Ctlcontrols.currentPosition;
            }
            set
            {
                if (value > 0 && value < Duration)
                    p.Ctlcontrols.currentPosition = value;
            }
        }
        public override string API(string data)
        {
            return base.API(data);
        }
        public override void Dispose()
        {
            p.Invoke((MethodInvoker)delegate()
            {
                p.Ctlcontrols.stop();
                p.Dispose();
            });
            p = null;
        }
        public void ChangeVisualization(string type, int present)
        {
            // Settings are in the registry, there doesn't appear to be
            // any other way to change the visualization
            //Registry.Cu
            RegistryKey regPref = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\MediaPlayer\Preferences", true);
            regPref.SetValue("CurrentEffectType", type, RegistryValueKind.String);
            regPref.SetValue("CurrentEffectPreset", present, RegistryValueKind.DWord);
            regPref.Close();
        }
    }
}