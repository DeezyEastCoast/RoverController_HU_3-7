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
    public class OutputsWindow
    {
        public GW.Window _window;
        char[] GNDEFX_ARRAY = new char[] { '$', 'G', 'F', 'X', ',', '0', '0', '*', '0', '0', '\r', '\n' };
        enum GRDEFX { RED = 0, GREEN, BLUE, SHOW_ON, OFF};
        static GRDEFX EFX_STATE = GRDEFX.OFF;

        #region WINDOW GUI COMPONENTS

        TextBlock _txtTitle;

        Button _btnGndEfxShow;
        Button _btnBackGndEfx;

        #endregion

        public OutputsWindow()
        {
            _window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.wndOutputs));
            initGndEfxWindow(_window);
        }

        //Initializes ground efx window.
        void initGndEfxWindow(GW.Window window)
        {
            // Create a Canvas
            GHI.Glide.UI.Canvas canvas = new GHI.Glide.UI.Canvas();
            window.AddChild(canvas);

            // Draw a separator line.
            //canvas.DrawLine(GHI.Glide.Colors.White, 1, 30, 50, window.Width - 30, 50);

            // Draw a fieldset around our "Login" text block.
            _txtTitle = (TextBlock)window.GetChildByName("txtTitle");
            _txtTitle.FontColor = GHI.Glide.Colors.White;
            canvas.DrawFieldset(_txtTitle, 40, 160, 220, GHI.Glide.Colors.White, 1);

            _btnGndEfxShow = (Button)_window.GetChildByName("btnGndEfxShow");
            _btnGndEfxShow.TapEvent += new OnTap(btnGndEfxShow_TapEvent);

            _btnBackGndEfx = (Button)_window.GetChildByName("btnBack");
            _btnBackGndEfx.TapEvent += new OnTap(btnBackGndEfx_TapEvent);
        }

        #region GROUND EFX WINDOW BUTTON EVENTS

        /// <summary>
        /// Send command byte for ground efx.
        /// </summary>
        void sendGndEfxArray()
        {
            //get checksum
            Array.Copy(Program.byteToHex(Program.getChecksum(Encoding.UTF8.GetBytes(new string(GNDEFX_ARRAY)))), 0, GNDEFX_ARRAY, 8, 2);

            Program.lairdComPort.Write(Encoding.UTF8.GetBytes(new string(GNDEFX_ARRAY)), 0, GNDEFX_ARRAY.Length);

            //'$','G','F','X',','  
            //   ,'0','0',        //bytes 5,6    get byte 1
            //   ,'*'
            //   ,'0','0'
            //   ,0x0D,0x0A};
        }

        /// <summary>
        /// Button tap event to configure ground efx byte to start light show.
        /// </summary>
        /// <param name="sender"></param>
        void btnGndEfxShow_TapEvent(object sender)
        {
            string tempString = new string(GNDEFX_ARRAY);
            byte tempConfigByte = (byte)Convert.ToInt32(tempString.Substring(5, 2), 16);

            EFX_STATE = (GRDEFX)(((int)EFX_STATE + 1) % ((int)GRDEFX.OFF + 1));

            switch (EFX_STATE)
            {
                case GRDEFX.OFF:
                    //if ((tempConfigByte & 1) == 1) //if show is on, turn it off
                    tempConfigByte = (byte)(tempConfigByte & 0xFE);
                    _btnGndEfxShow.Text = "EfxOff";
                    _btnGndEfxShow.TintColor = GHI.Glide.Colors.LightGray;
                    break;
                
                case GRDEFX.SHOW_ON://turn show on

                    tempConfigByte = (byte)(tempConfigByte | 1);
                    _btnGndEfxShow.Text = "EfxOn";
                    _btnGndEfxShow.TintColor = GHI.Glide.Colors.Fuchsia;
                    break;
                
                case GRDEFX.BLUE:

                    //if right blue led is on, turn it off
                    if ((tempConfigByte & 64) == 64) 
                        tempConfigByte = (byte)(tempConfigByte & (0xFF - 64));
                    else
                        tempConfigByte = (byte)(tempConfigByte | 64);

                    //if left blue led is on, turn it off
                    if ((tempConfigByte & 8) == 8) //if led is on, turn it off
                        tempConfigByte = (byte)(tempConfigByte & (0xFF - 8));
                    else
                        tempConfigByte = (byte)(tempConfigByte | 8);

                    _btnGndEfxShow.TintColor = GHI.Glide.Colors.Blue;
                    _btnGndEfxShow.Text = "EfxBlue";

                    break;

                case GRDEFX.RED:

                    //if right red led is on, turn it off
                    if ((tempConfigByte & 16) == 16) 
                        tempConfigByte = (byte)(tempConfigByte & (0xFF - 16));
                    else
                        tempConfigByte = (byte)(tempConfigByte | 16);

                    //if left red led is on, turn it off
                    if ((tempConfigByte & 2) == 2) //if led is on, turn it off
                        tempConfigByte = (byte)(tempConfigByte & (0xFF - 2));
                    else
                        tempConfigByte = (byte)(tempConfigByte | 2);

                    _btnGndEfxShow.Text = "EfxRed";
                    _btnGndEfxShow.TintColor = GHI.Glide.Colors.Red;

                    break;

                case GRDEFX.GREEN:

                    //if right green led is on, turn it off
                    if ((tempConfigByte & 32) == 32) 
                        tempConfigByte = (byte)(tempConfigByte & (0xFF - 32));
                    else
                        tempConfigByte = (byte)(tempConfigByte | 32);

                    //if left green led is on, turn it off
                    if ((tempConfigByte & 4) == 4) 
                        tempConfigByte = (byte)(tempConfigByte & (0xFF - 4));
                    else
                        tempConfigByte = (byte)(tempConfigByte | 4);

                    _btnGndEfxShow.TintColor = GHI.Glide.Colors.Green;
                    _btnGndEfxShow.Text = "EfxGreen";

                    break;
            }

            Array.Copy(Program.byteToHex(tempConfigByte), 0, GNDEFX_ARRAY, 5, 2);

            _btnGndEfxShow.TintAmount = 50;
            _window.FillRect(_btnGndEfxShow.Rect);
            _btnGndEfxShow.Invalidate();

            sendGndEfxArray();
        }

        /// <summary>
        /// Ground effects window back button tap event.
        /// </summary>
        /// <param name="sender"></param>
        void btnBackGndEfx_TapEvent(object sender)
        {
            Program.RoverJoystickControlTimer.Change(0,Program.RoverJoystickTimerPeriod);
            Program.UpdateMainWindowTimer.Change(0,Program.UpdateMainWindowTimerPeriod);

            Tween.SlideWindow(_window, Program._mainWindow, Direction.Right);
        }

        #endregion

    }
}

