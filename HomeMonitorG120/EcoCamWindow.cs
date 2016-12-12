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
    public class EcoCamWindow
    {
        public GW.Window _window;
        char[] ECOCAM_ARRAY = new char[] { '$', 'E', 'F', 'C', ',', '0', '0', '*', '0', '0', '\r', '\n' };

        #region WINDOW GUI COMPONENTS

        static ProgressBar _pBarConnected;
        static TextBlock _txtTitle;
        static TextBlock _txtStatus;
        static Button _btnBack;
        static Button _btnStop;
        static Button _btnSerialCam;
        static Button _btnSingleCam;
        static Button _btnVideo;

        #endregion

        public EcoCamWindow()
        {
            _window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.EcoCam_Window));
            initWindow(_window);
        }

        // Initializes main window buttons
        void initWindow(GW.Window window)
        {
            //Image imgLogo = (Image)window.GetChildByName("imgLogo");
            //imgLogo.Bitmap = Resources.GetBitmap(Resources.BitmapResources.Stinkmeaner_Thumb);

            // Create a Canvas
            GHI.Glide.UI.Canvas canvas = new GHI.Glide.UI.Canvas();
            window.AddChild(canvas);

            // Draw a separator line.
            canvas.DrawLine(GHI.Glide.Colors.White, 1, 30, 50, window.Width - 30, 50);

            // Draw a fieldset around our "Login" text block.
            _txtTitle = (TextBlock)window.GetChildByName("txtTitle");
            _txtTitle.FontColor = GHI.Glide.Colors.White;
            canvas.DrawFieldset(_txtTitle, 30, 100, 220, GHI.Glide.Colors.White, 1);

            _pBarConnected = (ProgressBar)_window.GetChildByName("pBarConnected");
            _pBarConnected.Value = 0;

            _txtStatus = (TextBlock)_window.GetChildByName("txtStatus");
            _txtStatus.Text = "Status:";

            _btnBack = (Button)_window.GetChildByName("btnBack");
            _btnBack.TapEvent += new OnTap(btnBack_TapEvent);

            _btnStop = (Button)_window.GetChildByName("btnStop");
            _btnStop.TapEvent += new OnTap(btnStop_TapEvent);

            _btnSerialCam = (Button)_window.GetChildByName("btnSerialCam");
            _btnSerialCam.TapEvent += new OnTap(btnSerialCam_TapEvent);

            _btnVideo = (Button)_window.GetChildByName("btnVideo");
            _btnVideo.TapEvent += new OnTap(btnVideo_TapEvent);

            _btnSingleCam = (Button)_window.GetChildByName("btnSingleCam");
            _btnSingleCam.TapEvent += new OnTap(btnSingleCam_TapEvent);
        }

        /// <summary>
        /// Send command byte for ECO CAM.
        /// </summary>
        void sendEcoCamArray()
        {
            //get checksum
            Array.Copy(Program.byteToHex(Program.getChecksum(Encoding.UTF8.GetBytes(new string(ECOCAM_ARRAY)))), 0, ECOCAM_ARRAY, 8, 2);

            Program.lairdComPort.Write(Encoding.UTF8.GetBytes(new string(ECOCAM_ARRAY)), 0, ECOCAM_ARRAY.Length);
        }

        /*
         
         case "$EFC": //Eco Fly Cam
          EcoCamBool = ecoFlyCam((byte)Convert.ToInt32(ps2DataLine.Substring(5, 2), 16));
           //'$','E','F','C',','  
           //   ,'0','0',       //bytes 5,6    eco cam byte 1
           //   ,'*'
           //   ,'0','0'
           //   ,0x0D,0x0A};
         
        static bool ecoFlyCam(byte data)
        {
            if ((data & 1) == 1)
                return startEcoCamMode(videoCamMode.Video);
            if ((data & 2) == 2)
                return startEcoCamMode(videoCamMode.SerialPhoto);
            if ((data & 4) == 4)
                return startEcoCamMode(videoCamMode.SinglePhoto);
            if ((data & 8) == 8)
                return stopEcoCamMode();

            return false;
        }
         
        */

        #region BUTTON TAP EVENTS

        /// <summary>
        /// Handle button video tap event.
        /// </summary>
        /// <param name="sender"></param>
        void btnVideo_TapEvent(object sender)
        {
            //string tempString = new string(ECOCAM_ARRAY);
            //byte tempConfigByte = (byte)Convert.ToInt32(tempString.Substring(5, 2), 16);

            //tempConfigByte = (byte)(tempConfigByte | 1);

            //if ((data & 1) == 1)
            //return startEcoCamMode(videoCamMode.Video);

            Array.Copy(Program.byteToHex((byte)1), 0, ECOCAM_ARRAY, 5, 2);

            sendEcoCamArray();
        }

        // Handles the next button tap event.
        void btnSerialCam_TapEvent(object sender)
        {
            //string tempString = new string(ECOCAM_ARRAY);
            //byte tempConfigByte = (byte)Convert.ToInt32(tempString.Substring(5, 2), 16);

            //tempConfigByte = (byte)(tempConfigByte | 2);

            //if ((data & 2) == 2)
            //    return startEcoCamMode(videoCamMode.SerialPhoto);

            Array.Copy(Program.byteToHex((byte)2), 0, ECOCAM_ARRAY, 5, 2);

            sendEcoCamArray();
        }

        // Handles the next button tap event.
        void btnSingleCam_TapEvent(object sender)
        {
            //string tempString = new string(ECOCAM_ARRAY);
            //byte tempConfigByte = (byte)Convert.ToInt32(tempString.Substring(5, 2), 16);

            //tempConfigByte = (byte)(tempConfigByte | 4);

            //if ((data & 4) == 4)
            //    return startEcoCamMode(videoCamMode.SinglePhoto);

            Array.Copy(Program.byteToHex((byte)4), 0, ECOCAM_ARRAY, 5, 2);

            sendEcoCamArray();
        }

        // Handles the pictures button tap event.
        void btnStop_TapEvent(object sender)
        {
            //string tempString = new string(ECOCAM_ARRAY);
            //byte tempConfigByte = (byte)Convert.ToInt32(tempString.Substring(5, 2), 16);

            //tempConfigByte = (byte)(tempConfigByte | 8);

            //if ((data & 8) == 8)
            //    return stopEcoCamMode();

            Array.Copy(Program.byteToHex((byte)8), 0, ECOCAM_ARRAY, 5, 2);

            sendEcoCamArray();
        }

        // Handles the next button tap event.
        void btnBack_TapEvent(object sender)
        {
            Program.RoverJoystickControlTimer.Change(0,Program.RoverJoystickTimerPeriod);
            Program.UpdateMainWindowTimer.Change(0,Program.UpdateMainWindowTimerPeriod);

            Tween.SlideWindow(_window, Program._mainWindow, Direction.Left);
        }

        #endregion


    }
}

