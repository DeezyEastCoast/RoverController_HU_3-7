using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;

using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Microsoft.SPOT.IO;
//using GHIElectronics.NETMF.IO;
using GHI.Glide;
using GHI.Glide.Display;
using GHI.Glide.UI;
using GW = GHI.Glide.Display;
using System.IO.Ports;
using System.Text;
using System.IO;


namespace OakhillLandroverController
{
    public class SetupWindow
    {
        public GW.Window _window;

         #region WINDOW GUI COMPONENTS

        TextBlock _txtTitle;
        TextBlock _txtBlWheelOut;
        TextBlock _txtBlTriggerOut;
        TextBlock _txtBlTrigScaled;
        TextBlock _txtBlWhlScaled;
        TextBlock _txtBlSpeedLimit;

        Button _btnBackSetup;
        Button _btnMode;

        CheckBox _chkBoxSpeedLimit;

        #endregion

        public Timer DiagnosticWindowTimer;
        public static readonly int diagnosticWindowTimerPeriod = 100;

        public SetupWindow()
        {
            _window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.wndSetup));
            initSetupWindow(_window);
        }

        //Initializes setup window.
        void initSetupWindow(GW.Window window)
        {
           // Create a Canvas
           GHI.Glide.UI.Canvas canvas = new GHI.Glide.UI.Canvas();
           _window.AddChild(canvas);

           // Draw a separator line.
           //canvas.DrawLine(GHI.Glide.Colors.White, 1, 30, 35, window.Width - 30, 35);

           // Draw a fieldset around our "Login" text block.
           _txtTitle = (TextBlock)window.GetChildByName("txtTitle");
           _txtTitle.FontColor = GHI.Glide.Colors.White;
           canvas.DrawFieldset(_txtTitle, 90, 110, 220, GHI.Glide.Colors.White, 1);

           _txtBlWheelOut = (TextBlock)_window.GetChildByName("txtBlWheelOut");
           _txtBlTriggerOut = (TextBlock)_window.GetChildByName("txtBlTriggerOut");
           _txtBlTrigScaled = (TextBlock)_window.GetChildByName("txtBlTrigScaled");
           _txtBlWhlScaled = (TextBlock)_window.GetChildByName("txtBlWhlScaled");
           _txtBlSpeedLimit = (TextBlock)_window.GetChildByName("txtBlSpeedLimit");

           _chkBoxSpeedLimit = (CheckBox)_window.GetChildByName("chkBoxSpeedLimit");
           _chkBoxSpeedLimit.TapEvent += _chkBoxSpeedLimit_TapEvent;

           _btnBackSetup = (Button)_window.GetChildByName("btnBack");
           _btnBackSetup.TapEvent += new OnTap(btnBackSetup_TapEvent);

           _btnMode = (Button)_window.GetChildByName("btnMode");
           _btnMode.TapEvent += new OnTap(btnMode_TapEvent);

           DiagnosticWindowTimer = new Timer(new TimerCallback(SetupWindowTimer_Tick), null, -1, diagnosticWindowTimerPeriod);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="temp"></param>
        void SetupWindowTimer_Tick(object temp)
        {
            _txtBlWheelOut.Text = Program.steeringWheelAnalog.ReadRaw().ToString();
            _window.FillRect(_txtBlWheelOut.Rect);
            _txtBlWheelOut.Invalidate();

            _txtBlWhlScaled.Text =  Program.convertLinearScale(Program.steeringWheelAnalog.ReadRaw(), Program.steeringWheelMin, Program.steeringWheelMax, 0, 255).ToString();
            _window.FillRect(_txtBlWhlScaled.Rect);
            _txtBlWhlScaled.Invalidate();

            _txtBlTriggerOut.Text = Program.speedTriggerAnalog.ReadRaw().ToString();
            _window.FillRect(_txtBlTriggerOut.Rect);
            _txtBlTriggerOut.Invalidate();

            //_txtBlTrigScaled.Text = Program.convertLinearScale(Program.speedTriggerAnalog.ReadRaw(), Program.speedTriggerMin, Program.speedTriggerMax, 0, 255).ToString();
            _txtBlTrigScaled.Text = Program.triggerSpeedCmd().ToString();
            _window.FillRect(_txtBlTrigScaled.Rect);
            _txtBlTrigScaled.Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        void _chkBoxSpeedLimit_TapEvent(object sender)
        {
            if (_chkBoxSpeedLimit.Checked)
                Program.SPEED_TRIGGER_MAX_OUTPUT = (byte)(155);
            else Program.SPEED_TRIGGER_MAX_OUTPUT = (byte)(255);
        }

        /// <summary>
        /// Setup window back button tap event.
        /// </summary>
        /// <param name="sender"></param>
        void btnMode_TapEvent(object sender)
        {
            Program.DRIVE_MODE_ROVER = (Program.DRIVE_MODE)(((int)Program.DRIVE_MODE_ROVER + 1) % (int)Program.DRIVE_MODE.LIMIT);

            switch (Program.DRIVE_MODE_ROVER)
            {
                case Program.DRIVE_MODE.MANUAL:
                    _btnMode.Text = "Manual";
                    break;

                case Program.DRIVE_MODE.AUTO:
                    _btnMode.Text = "Auto";
                    break;

                case Program.DRIVE_MODE.COMPASS:
                    _btnMode.Text = "Compass";
                    break;
            }

            _window.FillRect(_btnMode.Rect);
            _btnMode.Invalidate();
        }

        /// <summary>
        /// Setup window back button tap event.
        /// </summary>
        /// <param name="sender"></param>
        void btnBackSetup_TapEvent(object sender)
        {
            Program.RoverJoystickControlTimer.Change(0, Program.RoverJoystickTimerPeriod);
            Program.UpdateMainWindowTimer.Change(0, Program.UpdateMainWindowTimerPeriod);
            DiagnosticWindowTimer.Change(-1, -1);

            Tween.SlideWindow(_window, Program._mainWindow, Direction.Right);
        }

    }
}

