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
    public class GroundEfxWindow
    {
        public GW.Window _window;
        char[] GNDEFX_ARRAY = new char[] { '$', 'G', 'F', 'X', ',', '0', '0', '*', '0', '0', '\r', '\n' };

        #region WINDOW GUI COMPONENTS

        RadioButton _radBtnGreenLeft;
        RadioButton _radBtnRedLeft;
        RadioButton _radBtnBlueLeft;
        RadioButton _radBtnBlueRight;
        RadioButton _radBtnRedRight;
        RadioButton _radBtnGreenRight;
        RadioButton _radBtnLeftOff;
        RadioButton _radBtnRightOff;
        Button _btnGndEfxShow;
        Button _btnBackGndEfx;

        #endregion

        public GroundEfxWindow()
        {
            _window = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.GroundEfx_Window));
            initGndEfxWindow(_window);
        }

        //Initializes ground efx window.
        void initGndEfxWindow(GW.Window window)
        {
            _radBtnGreenLeft = (RadioButton)_window.GetChildByName("radBtnGreenLeft");
            _radBtnGreenLeft.TapEvent += new OnTap(radBtnGreenLeft_TapEvent);

            _radBtnRedLeft = (RadioButton)_window.GetChildByName("radBtnRedLeft");
            _radBtnRedLeft.TapEvent += new OnTap(radBtnRedLeft_TapEvent);

            _radBtnBlueLeft = (RadioButton)_window.GetChildByName("radBtnBlueLeft");
            _radBtnBlueLeft.TapEvent += new OnTap(radBtnBlueLeft_TapEvent);

            _radBtnBlueRight = (RadioButton)_window.GetChildByName("radBtnBlueRight");
            _radBtnBlueRight.TapEvent += new OnTap(radBtnBlueRight_TapEvent);

            _radBtnRedRight = (RadioButton)_window.GetChildByName("radBtnRedRight");
            _radBtnRedRight.TapEvent += new OnTap(radBtnRedRight_TapEvent);

            _radBtnGreenRight = (RadioButton)_window.GetChildByName("radBtnGreenRight");
            _radBtnGreenRight.TapEvent += new OnTap(radBtnGreenRight_TapEvent);

            _radBtnLeftOff = (RadioButton)_window.GetChildByName("radBtnLeftOff");
            _radBtnLeftOff.TapEvent += new OnTap(radBtnLeftOff_TapEvent);

            _radBtnRightOff = (RadioButton)_window.GetChildByName("radBtnRightOff");
            _radBtnRightOff.TapEvent += new OnTap(radBtnRightOff_TapEvent);

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

            //toggle show button
            if ((tempConfigByte & 1) == 1) //if show is on, turn it off
            {
                tempConfigByte = (byte)(tempConfigByte & 0xFE);
                _btnGndEfxShow.Text = "Show Off";
                _btnGndEfxShow.TintColor = GHI.Glide.Colors.Red;
            }
            else
            {
                tempConfigByte = (byte)(tempConfigByte | 1);
                _btnGndEfxShow.Text = "Show On";
                _btnGndEfxShow.TintColor = GHI.Glide.Colors.Green;
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

        /// <summary>
        /// Turn all right leds off.
        /// </summary>
        /// <param name="sender"></param>
        void radBtnRightOff_TapEvent(object sender)
        {
            string tempString = new string(GNDEFX_ARRAY);
            byte tempConfigByte = (byte)Convert.ToInt32(tempString.Substring(5, 2), 16);

            tempConfigByte = (byte)(tempConfigByte & 0x8F);
            //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Green;

            Array.Copy(Program.byteToHex(tempConfigByte), 0, GNDEFX_ARRAY, 5, 2);

            //radBtnGreenRight.
            _window.FillRect(_radBtnRightOff.Rect);
            _radBtnRightOff.Invalidate();

            sendGndEfxArray();
        }

        /// <summary>
        /// Turn all left leds off.
        /// </summary>
        /// <param name="sender"></param>
        void radBtnLeftOff_TapEvent(object sender)
        {
            string tempString = new string(GNDEFX_ARRAY);
            byte tempConfigByte = (byte)Convert.ToInt32(tempString.Substring(5, 2), 16);

            tempConfigByte = (byte)(tempConfigByte & 0xF1);
            //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Green;

            Array.Copy(Program.byteToHex(tempConfigByte), 0, GNDEFX_ARRAY, 5, 2);

            //radBtnGreenRight.
            _window.FillRect(_radBtnLeftOff.Rect);
            _radBtnLeftOff.Invalidate();

            sendGndEfxArray();
        }

        /// <summary>
        /// Turn right green led on/off.
        /// </summary>
        /// <param name="sender"></param>
        void radBtnGreenRight_TapEvent(object sender)
        {
            string tempString = new string(GNDEFX_ARRAY);
            byte tempConfigByte = (byte)Convert.ToInt32(tempString.Substring(5, 2), 16);

            //toggle show button
            if ((tempConfigByte & 32) == 32) //if led is on, turn it off
            {
                tempConfigByte = (byte)(tempConfigByte & (0xFF - 32));
                //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Red;
            }
            else
            {
                tempConfigByte = (byte)(tempConfigByte | 32);
                //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Green;
            }

            Array.Copy(Program.byteToHex(tempConfigByte), 0, GNDEFX_ARRAY, 5, 2);

            //radBtnGreenRight.
            _window.FillRect(_radBtnGreenRight.Rect);
            _radBtnGreenRight.Invalidate();

            sendGndEfxArray();
        }

        /// <summary>
        /// Turn right red led on/off.
        /// </summary>
        /// <param name="sender"></param>
        void radBtnRedRight_TapEvent(object sender)
        {
            string tempString = new string(GNDEFX_ARRAY);
            byte tempConfigByte = (byte)Convert.ToInt32(tempString.Substring(5, 2), 16);

            //toggle show button
            if ((tempConfigByte & 16) == 16) //if led is on, turn it off
            {
                tempConfigByte = (byte)(tempConfigByte & (0xFF - 16));
                //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Red;
            }
            else
            {
                tempConfigByte = (byte)(tempConfigByte | 16);
                //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Green;
            }

            Array.Copy(Program.byteToHex(tempConfigByte), 0, GNDEFX_ARRAY, 5, 2);

            //radBtnGreenRight.
            _window.FillRect(_radBtnRedRight.Rect);
            _radBtnRedRight.Invalidate();

            sendGndEfxArray();
        }

        /// <summary>
        /// Turn right blue led on/off.
        /// </summary>
        /// <param name="sender"></param>
        void radBtnBlueRight_TapEvent(object sender)
        {
            string tempString = new string(GNDEFX_ARRAY);
            byte tempConfigByte = (byte)Convert.ToInt32(tempString.Substring(5, 2), 16);

            //toggle show button
            if ((tempConfigByte & 64) == 64) //if led is on, turn it off
            {
                tempConfigByte = (byte)(tempConfigByte & (0xFF - 64));
                //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Red;
            }
            else
            {
                tempConfigByte = (byte)(tempConfigByte | 64);
                //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Green;
            }

            Array.Copy(Program.byteToHex(tempConfigByte), 0, GNDEFX_ARRAY, 5, 2);

            //radBtnGreenRight.
            _window.FillRect(_radBtnBlueRight.Rect);
            _radBtnBlueRight.Invalidate();

            sendGndEfxArray();
        }

        /// <summary>
        /// Turn left blue led on/off.
        /// </summary>
        /// <param name="sender"></param>
        void radBtnBlueLeft_TapEvent(object sender)
        {
            string tempString = new string(GNDEFX_ARRAY);
            byte tempConfigByte = (byte)Convert.ToInt32(tempString.Substring(5, 2), 16);

            //toggle show button
            if ((tempConfigByte & 8) == 8) //if led is on, turn it off
            {
                tempConfigByte = (byte)(tempConfigByte & (0xFF - 8));
                //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Red;
            }
            else
            {
                tempConfigByte = (byte)(tempConfigByte | 8);
                //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Green;
            }

            Array.Copy(Program.byteToHex(tempConfigByte), 0, GNDEFX_ARRAY, 5, 2);

            //radBtnGreenRight.
            _window.FillRect(_radBtnBlueLeft.Rect);
            _radBtnBlueLeft.Invalidate();

            sendGndEfxArray();
        }

        /// <summary>
        /// Turn left red led on/off.
        /// </summary>
        /// <param name="sender"></param>
        void radBtnRedLeft_TapEvent(object sender)
        {
            string tempString = new string(GNDEFX_ARRAY);
            byte tempConfigByte = (byte)Convert.ToInt32(tempString.Substring(5, 2), 16);

            //toggle show button
            if ((tempConfigByte & 2) == 2) //if led is on, turn it off
            {
                tempConfigByte = (byte)(tempConfigByte & (0xFF - 2));
                //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Red;
            }
            else
            {
                tempConfigByte = (byte)(tempConfigByte | 2);
                //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Green;
            }

            Array.Copy(Program.byteToHex(tempConfigByte), 0, GNDEFX_ARRAY, 5, 2);

            //radBtnGreenRight.
            _window.FillRect(_radBtnRedLeft.Rect);
            _radBtnRedLeft.Invalidate();

            sendGndEfxArray();
        }

        /// <summary>
        /// Turn left green led on/off.
        /// </summary>
        /// <param name="sender"></param>
        void radBtnGreenLeft_TapEvent(object sender)
        {
            string tempString = new string(GNDEFX_ARRAY);
            byte tempConfigByte = (byte)Convert.ToInt32(tempString.Substring(5, 2), 16);

            //toggle show button
            if ((tempConfigByte & 4) == 4) //if led is on, turn it off
            {
                tempConfigByte = (byte)(tempConfigByte & (0xFF - 4));
                //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Red;
            }
            else
            {
                tempConfigByte = (byte)(tempConfigByte | 4);
                //radBtnGreenRight.TintColor = GHIElectronics.NETMF.Glide.Colors.Green;
            }

            Array.Copy(Program.byteToHex(tempConfigByte), 0, GNDEFX_ARRAY, 5, 2);

            //radBtnGreenRight.
            _window.FillRect(_radBtnGreenLeft.Rect);
            _radBtnGreenLeft.Invalidate();

            sendGndEfxArray();
        }

        #endregion

    }
}

