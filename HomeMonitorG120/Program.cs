using System;
using System.Threading;

using Microsoft.SPOT.IO;
using System.IO.Ports; //for Serial Port
using System.Text; //for Encoding
using System.IO;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHI.Premium.Hardware;

using GHI.Glide;
using GHI.Glide.Display;
using GHI.Glide.UI;

namespace OakhillLandroverController
{
    public class Program
    {
        #region HeadUnit 3.7 Built-ins

        public static Storage SDCard = new Storage("SD", _sdDetect);
        private static Microsoft.SPOT.Hardware.Cpu.Pin _sdDetect = GHI.Hardware.G120.Pin.P1_16;
        public static string SD_ROOT_DIRECTORY;
        static string[] sdCardJpgFileNames;

        private static Microsoft.SPOT.Hardware.Cpu.Pin chargeStatePin = GHI.Hardware.G120.Pin.P1_10;
        private static Microsoft.SPOT.Hardware.InputPort batChargeState = new Microsoft.SPOT.Hardware.InputPort(chargeStatePin, false, Microsoft.SPOT.Hardware.Port.ResistorMode.Disabled);

        private static Microsoft.SPOT.Hardware.Cpu.Pin lcdBacklightPin = GHI.Hardware.G120.Pin.P2_21;
        private static Microsoft.SPOT.Hardware.OutputPort lcdBacklight = new Microsoft.SPOT.Hardware.OutputPort(lcdBacklightPin, true);

        private static Microsoft.SPOT.Hardware.Cpu.Pin onBoardLedPin = GHI.Hardware.G120.Pin.P1_5;
        private static Microsoft.SPOT.Hardware.OutputPort onBoardLed = new Microsoft.SPOT.Hardware.OutputPort(onBoardLedPin, false);
        static bool SYSTEM_LED = false;

        // Create touch inputs that are used as arguments
        static private Microsoft.SPOT.Touch.TouchInput[] touches = new Microsoft.SPOT.Touch.TouchInput[] { new Microsoft.SPOT.Touch.TouchInput() };

        #endregion

        #region WINDOW GUI COMPONENTS

        // Window references.
        public static Window _mainWindow = new Window();
        public static Window pictureWindow = new Window();
        public static SetupWindow diagnosticWindow;
        public static GroundEfxWindow gEfxWindow;
        public static EcoCamWindow ecoCamWindow;

        static ProgressBar _pBarConnected;
        static TextBlock _txtTitle;
        static TextBlock _txtCntrlBattery;
        static TextBlock _txtRoverBattery;
        static TextBlock _txtRoverRange;
        static TextBlock _txtRoverHeading;
        static TextBlock _txtRoverTemp;
        static TextBlock _txtRoverPress;
        static TextBlock _txtRoverLat;
        static TextBlock _txtRoverLon;

        static GHI.Glide.UI.Button _btnSettings;
        static GHI.Glide.UI.Button _btnPics;
        static GHI.Glide.UI.Button _btnGndEfx;
        static GHI.Glide.UI.Button _btnTalk;
        static GHI.Glide.UI.Button _btnCamera;
        static GHI.Glide.UI.Button _btnVideo;

        #endregion

        #region COMMUNICATION ARRAYS

        static byte[] roverData = new byte[] { (byte)'$', 0, 0, (byte)',', 0, 0, (byte)',', 0, 0, (byte)',', 0, 0, (byte)',', 0, 0, (byte)',', 0, 0, (byte)',', (byte)'*', 0, 0, 0x0D, 0x0A };
        //static char[] ps2Data = new char[] { '$', '0', '0', ',', '0', '0', ',', '0', '0', ',', '0', '0', ',', '0', '0', ',', '0', '0', ',', '*', '0', '0', '\r', '\n' };
        static char[] ps2Data = new char[] { '$', 'O', 'J', 'C', ',', '0', '0', ',', '0', '0', ',', '0', '0', '*', '0', '0', '\r', '\n' };

        //char ps2Data[] = {'$'
        //  ,'0','0',',' //
        //  ,'0','0',',' //accessories: Lights L/H, Camera, Weapon, ACC 
        //  ,'0','0',',' //right joystick
        //  ,'0','0',','
        //  ,'0','0',','
        //  ,'0','0',',' //left joystick
        //  ,'*'
        //  ,'0','0'
        //  ,0x0D,0x0A};
        static string roverDataLine;
        static string quadPowerDataLine;

        #endregion

        #region TRIGGER AND STEERING WHEEL SETUP

        Microsoft.SPOT.Hardware.Cpu.Pin selectRightPin;
        static Microsoft.SPOT.Hardware.InputPort rightJoystickSelect;
        Microsoft.SPOT.Hardware.Cpu.Pin selectLeftPin;
        static Microsoft.SPOT.Hardware.InputPort leftJoystickSelect;

        public static AnalogInput speedTriggerAnalog = new AnalogInput(Cpu.AnalogChannel.ANALOG_4); //p1.30
        public static AnalogInput steeringWheelAnalog = new AnalogInput(Cpu.AnalogChannel.ANALOG_5);//p1.31

        public static readonly byte speedTriggerNullMin = 70; //deadband around neutral/stop
        public static readonly byte speedTriggerNullMax = 90;  //deadband around neutral/stop
        public static readonly int speedTriggerMin = 300;  //full reverse
        public static readonly int speedTriggerMax = 3660; //full forward
        public static readonly int steeringWheelMin = 260; 
        public static readonly int steeringWheelMax = 3460;
        private static byte speedTriggerMaxOutput = 255;

        #endregion

        #region LAIRD XCVR

        public static SerialPort lairdComPort;
        static Microsoft.SPOT.Hardware.Cpu.Pin lairdResetPin = GHI.Hardware.G120.Pin.P0_0;
        static Microsoft.SPOT.Hardware.OutputPort lairdReset;
        static SerialBuffer lairdWirelessBuffer;
        static Microsoft.SPOT.Hardware.Cpu.Pin lairdCtsPin = GHI.Hardware.G120.Pin.P1_19;
        static Microsoft.SPOT.Hardware.OutputPort lairdCts;
        static Microsoft.SPOT.Hardware.Cpu.Pin lairdRangePin = GHI.Hardware.G120.Pin.P0_1;
        static Microsoft.SPOT.Hardware.InputPort lairdRange;

        //Microsoft.SPOT.Hardware.Cpu.Pin lairdRtsPin;
        //Microsoft.SPOT.Hardware.InputPort lairdRts;

        #endregion

        #region OHS_QuadPower

        public static SerialPort quadPowerComPort;
        static SerialBuffer quadPowerBuffer;
        //static Microsoft.SPOT.Hardware.Cpu.Pin lairdResetPin = GHI.Hardware.G120.Pin.P0_0;
        //static Microsoft.SPOT.Hardware.OutputPort lairdReset;
        //static Microsoft.SPOT.Hardware.Cpu.Pin lairdCtsPin = GHI.Hardware.G120.Pin.P1_19;
        //static Microsoft.SPOT.Hardware.OutputPort lairdCts;
        //static Microsoft.SPOT.Hardware.Cpu.Pin lairdRangePin = GHI.Hardware.G120.Pin.P0_1;
        //static Microsoft.SPOT.Hardware.InputPort lairdRange;

        #endregion

        #region TIMERS
        
        //Timers
        public static Timer RoverJoystickControlTimer;
        public static Timer PictureViewerTimer;
        public static Timer UpdateMainWindowTimer;
        public static readonly int RoverJoystickTimerPeriod = 50;//50
        public static readonly int PictureViewerTimerPeriod = 100;
        public static readonly int UpdateMainWindowTimerPeriod = 500;
        public static int updateWindowCounter = 0;
        #endregion

        //static int IDLE_TIME = 20000;
        static int picCounter = 0;
        //static bool roverHeadLights = false;
        //static bool roverTakePicture = false;

        public static void Main()
        {
            #region Newhaven 3.5" Display Graphic Setup
            /*
                var lcdConfig3 = new Configuration.LCD.Configurations();

                lcdConfig3.Width = 320;
                lcdConfig3.Height = 240;

                lcdConfig3.OutputEnableIsFixed = true;
                lcdConfig3.OutputEnablePolarity = true;

                lcdConfig3.HorizontalSyncPolarity = false;
                lcdConfig3.VerticalSyncPolarity = false;
                lcdConfig3.PixelPolarity = true;

                lcdConfig3.HorizontalSyncPulseWidth = 68;
                lcdConfig3.HorizontalBackPorch = 2;
                lcdConfig3.HorizontalFrontPorch = 18;
                lcdConfig3.VerticalSyncPulseWidth = 10;
                lcdConfig3.VerticalBackPorch = 3;
                lcdConfig3.VerticalFrontPorch = 10;

                lcdConfig3.PixelClockRateKHz = 6400;

                Configuration.LCD.Set(lcdConfig3);

            */
            //if (Configuration.LCD.Set(lcdConfig3)) PowerState.RebootDevice(false);
            

            //Initialiazing and hooking to events
            var display = new DisplayNhd5(new Microsoft.SPOT.Hardware.I2CDevice(new Microsoft.SPOT.Hardware.I2CDevice.Configuration(0x38, 400)));
            //display.TouchUp += (sender, e) => Debug.Print("Finger " + e.FingerNumber + " up!");
            display.TouchUp += new TouchEventHandler(display_TouchUp);
            //display.TouchDown += (sender, e) => Debug.Print("Finger " + e.FingerNumber + " down!");
            display.TouchDown += new TouchEventHandler(display_TouchDown);
            display.ZoomIn += (sender, e) => Debug.Print("Zoom in");
            display.ZoomOut += (sender, e) => Debug.Print("Zoom out");

            /*
            //Method one: polling manually. Uncomment the while cycle to use it.

            //while (true) {
            // display.ReadAndProcessTouchData();
            // Thread.Sleep(50);
            //}
            */

            //Method two: using interrupt (for G120).
            var touchPin = new Microsoft.SPOT.Hardware.InterruptPort(GHI.Hardware.G120.Pin.P0_25, false, Microsoft.SPOT.Hardware.Port.ResistorMode.PullUp, Microsoft.SPOT.Hardware.Port.InterruptMode.InterruptEdgeLow);
            var wakePin = new Microsoft.SPOT.Hardware.InputPort(GHI.Hardware.G120.Pin.P0_24, false, Microsoft.SPOT.Hardware.Port.ResistorMode.Disabled);
            //var touchPin = new Microsoft.SPOT.Hardware.InterruptPort(GHI.Hardware.G120.Pin.P0_23, false, Microsoft.SPOT.Hardware.Port.ResistorMode.PullUp, Microsoft.SPOT.Hardware.Port.InterruptMode.InterruptEdgeLow); //I DID THIS
            touchPin.OnInterrupt += (data1, data2, time) => display.ReadAndProcessTouchData();

            #endregion

            #region LAIRD XCVR SETUP

            //lairdWirelss.Configure(9600, GT.Interfaces.Serial.SerialParity.None, GT.Interfaces.Serial.SerialStopBits.One, 7);
            lairdComPort = new SerialPort("COM2", 115200); //TX2 = P2.0, RX2 = P0.16
            //lairdComPort = new SerialPort("COM2", 9600);

            lairdComPort.Open();

            lairdReset = new Microsoft.SPOT.Hardware.OutputPort(lairdResetPin, true);
            lairdCts = new Microsoft.SPOT.Hardware.OutputPort(lairdCtsPin, false);
            //lairdRts = new Microsoft.SPOT.Hardware.InputPort(lairdRtsPin, false, Microsoft.SPOT.Hardware.Port.ResistorMode.PullUp);
            lairdRange = new Microsoft.SPOT.Hardware.InputPort(lairdRangePin, false, Microsoft.SPOT.Hardware.Port.ResistorMode.Disabled);
            
            /*
            lairdCtsPin = GT.Socket.GetSocket(5, true, null, null).CpuPins[6];
            //lairdRtsPin = GT.Socket.GetSocket(5, true, null, null).CpuPins[7];
            lairdResetPin = GT.Socket.GetSocket(5, true, null, null).CpuPins[3];
            lairdRangePin = GT.Socket.GetSocket(5, true, null, null).CpuPins[8];
            */

            lairdWirelessBuffer = new SerialBuffer(72);

            #endregion

            #region QUAD POWER SETUP

            quadPowerComPort = new SerialPort("COM3", 9600);
            quadPowerComPort.Open();
            quadPowerBuffer = new SerialBuffer(36);

            #endregion

            // Load the window
            _mainWindow = GlideLoader.LoadWindow(OakhillLandroverController.Resources.GetString(OakhillLandroverController.Resources.StringResources.wndMain));
            //pictureWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.Picture_Window));
            gEfxWindow = new GroundEfxWindow();//gndEfxWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.GroundEfx_Window));
            ecoCamWindow = new EcoCamWindow();
            diagnosticWindow = new SetupWindow();

            // Activate touch
            //GlideTouch.Initialize();

            // Initialize the windows.
            initMainWindow(_mainWindow);

            // Assigning a window to MainWindow flushes it to the screen.
            // This also starts event handling on the window.
            Glide.MainWindow = _mainWindow;

            //bool mybool = SDCard.sdCardDetect;
            //bool yourBool  = batChargeState.Read();
            //lcdBacklight.Write(false); //turn lcd screen off
            //onBoardLed.Write(true);

            picCounter = -1;
            //Glide.MessageBoxManager.Show("SD Card not found.", "SD Card");

            UpdateMainWindowTimer = new Timer(new TimerCallback(UpdateMainWindowTimer_Tick), null,0, UpdateMainWindowTimerPeriod);
            PictureViewerTimer = new Timer(new TimerCallback(PictureViewerTimer_Tick) ,null,-1,PictureViewerTimerPeriod);
            RoverJoystickControlTimer = new Timer(new TimerCallback(RoverJoystickControlTimer_Tick),null,0,RoverJoystickTimerPeriod);

            //display_T35.SimpleGraphics.DisplayImage(SD_ROOT_DIRECTORY + @"\roverPic17.jpg", Bitmap.BitmapImageType.Jpeg, 0, 0);

            Thread.Sleep(-1);
        }

        // Initializes main window buttons
        static void initMainWindow(Window window)
        {
            //Image imgLogo = (Image)window.GetChildByName("imgLogo");
            //imgLogo.Bitmap = Resources.GetBitmap(Resources.BitmapResources.Stinkmeaner_Thumb);

            // Create a Canvas
            GHI.Glide.UI.Canvas canvas = new GHI.Glide.UI.Canvas();
            window.AddChild(canvas);

            // Draw a separator line.
            //canvas.DrawLine(GHI.Glide.Colors.White, 1, 30, 50, window.Width - 30, 50);

            // Draw a fieldset around our "Login" text block.
            _txtTitle = (TextBlock)window.GetChildByName("txtTitle");
            _txtTitle.FontColor = GHI.Glide.Colors.White;
            canvas.DrawFieldset(_txtTitle, 30, 100, 220, GHI.Glide.Colors.White, 1);

            _pBarConnected = (ProgressBar)_mainWindow.GetChildByName("pBarConnected");
            _pBarConnected.Value = 0;

            _txtCntrlBattery = (TextBlock)_mainWindow.GetChildByName("txtCntrlBattery");
            _txtCntrlBattery.Text = "Battery: 100%";

            _txtRoverBattery = (TextBlock)_mainWindow.GetChildByName("txtRoverBattery");
            _txtRoverBattery.Text = "Rover: 100%";

            _txtRoverRange = (TextBlock)_mainWindow.GetChildByName("txtRoverRange");
            _txtRoverRange.Text = "Range: 0 in.";

            _txtRoverHeading = (TextBlock)_mainWindow.GetChildByName("txtRoverHeading");
            _txtRoverHeading.Text = "Heading: 0 deg";

            _txtRoverTemp = (TextBlock)_mainWindow.GetChildByName("txtRoverTemp");
            _txtRoverTemp.Text = "Temp: 0 F";

            _txtRoverPress = (TextBlock)_mainWindow.GetChildByName("txtRoverPress");
            _txtRoverPress.Text = "Press: 0 psi";

            _txtRoverLat = (TextBlock)_mainWindow.GetChildByName("txtRoverLat");
            _txtRoverLat.Text = "Lat: 0";

            _txtRoverLon = (TextBlock)_mainWindow.GetChildByName("txtRoverLon");
            _txtRoverLon.Text = "Lon: 0";

            _btnSettings = (GHI.Glide.UI.Button)_mainWindow.GetChildByName("btnSettings");
            _btnSettings.TapEvent += new OnTap(btnSettings_TapEvent);

            _btnPics = (GHI.Glide.UI.Button)_mainWindow.GetChildByName("btnPics");
            _btnPics.TapEvent += new OnTap(btnPics_TapEvent);

            _btnGndEfx = (GHI.Glide.UI.Button)_mainWindow.GetChildByName("btnGndEfx");
            _btnGndEfx.TapEvent += new OnTap(btnGndEfx_TapEvent);

            _btnTalk = (GHI.Glide.UI.Button)_mainWindow.GetChildByName("btnTalk");
            _btnTalk.TapEvent += new OnTap(btnTalk_TapEvent);

            _btnVideo = (GHI.Glide.UI.Button)_mainWindow.GetChildByName("btnVideo");
            _btnVideo.TapEvent += new OnTap(btnVideo_TapEvent);

            _btnCamera = (GHI.Glide.UI.Button)_mainWindow.GetChildByName("btnCamera");
            _btnCamera.TapEvent += new OnTap(btnCamera_TapEvent);
        }

        /// <summary>
        /// Drive rover.
        /// </summary>
        static void RoverJoystickControlTimer_Tick(object temp)
        {
            //get check box accessories
            //Array.Copy(byteToHex(getChkBoxAccessories()), 0, ps2Data, 4, 2);
            Array.Copy(byteToHex((byte)(convertLinearScale(steeringWheelAnalog.ReadRaw(), steeringWheelMin ,steeringWheelMax, 0, 255))), 0, ps2Data, 5, 2);
            Array.Copy(byteToHex(triggerSpeedCmd()), 0, ps2Data, 8, 2);                    

            //get checksum
            Array.Copy(byteToHex(getChecksum(Encoding.UTF8.GetBytes(new string(ps2Data)))), 0, ps2Data, 14, 2);
            lairdComPort.Write(Encoding.UTF8.GetBytes(new string(ps2Data)), 0, ps2Data.Length);

            SYSTEM_LED = !SYSTEM_LED;
            onBoardLed.Write(SYSTEM_LED);
        }

        /// <summary>
        /// View pictures stored on local SD card.
        /// </summary>
        static void PictureViewerTimer_Tick(object temp)
        {
            FileStream fileStream = null;
            byte[] data;
            Bitmap myPic;
            GHI.Glide.UI.Image image;

            int joystickLevel = analogLinearScale(steeringWheelAnalog.Read(), 0, 255);

            if (joystickLevel > 200)
            {
                picCounter = (picCounter + 1) % sdCardJpgFileNames.Length;
                fileStream = new FileStream(sdCardJpgFileNames[picCounter], FileMode.Open);

                //load the data from the stream
                data = new byte[fileStream.Length];
                fileStream.Read(data, 0, data.Length);

                myPic = new Bitmap(data, Bitmap.BitmapImageType.Jpeg);

                image = (GHI.Glide.UI.Image)pictureWindow.GetChildByName("imgPicture");
                //image.Bitmap = Resources.GetBitmap(Resources.BitmapResources.logo);
                image.Bitmap.DrawImage(0, 0, myPic, 0, 0, 320, 240);

                fileStream.Close();

                image.Invalidate();

                Thread.Sleep(500);
            }
            else if (!rightJoystickSelect.Read()) //button select pressed equals false
            {
                PictureViewerTimer.Change(-1,-1); //stop timer
                Thread.Sleep(100);
                Tween.SlideWindow(_mainWindow, pictureWindow, GHI.Glide.Direction.Up);
                Glide.MainWindow = _mainWindow;

                RoverJoystickControlTimer.Change(0,RoverJoystickTimerPeriod);
                UpdateMainWindowTimer.Change(0, UpdateMainWindowTimerPeriod);
            }
        }

        /// <summary>
        /// Update the main window.
        /// </summary>
        static void UpdateMainWindowTimer_Tick(object temp)
        {
            if ((updateWindowCounter % 10) == 0) //do the non critical stuff
            {
                quadPowerBuffer.LoadSerial(quadPowerComPort);
                if ((quadPowerDataLine = quadPowerBuffer.ReadLine()) != null)
                {
                    string[] tempStrArr = quadPowerDataLine.Split(new char[] { ',' });
                    string chgStateStr = "";

                    try
                    {
                        if ((tempStrArr[2] == "PCH") || (tempStrArr[2] == "DCH") || (tempStrArr[2] == "FCH")) chgStateStr = "+";

                        _txtCntrlBattery.Text = "Battery: " + chgStateStr + tempStrArr[5] + "%";
                        _mainWindow.FillRect(_txtCntrlBattery.Rect);
                        _txtCntrlBattery.Invalidate();
                    }
                    catch (Exception) { }
                }
            }

            lairdWirelessBuffer.LoadSerial(lairdComPort);
            if ((roverDataLine = lairdWirelessBuffer.ReadLine()) != null)
            {
                try
                {
                    //verify checksum
                    if ((byte)(Convert.ToInt32(roverDataLine.Substring(roverDataLine.IndexOf('*')+1, 2),16)) == getChecksum(Encoding.UTF8.GetBytes(roverDataLine)))
                    {
                        string[] splitRoverData = roverDataLine.Split(new char[] { ',','*' });

                        _txtRoverBattery.Text = "  Rover: " + splitRoverData[1] + " " + splitRoverData[2];                       
                        _mainWindow.FillRect(_txtRoverBattery.Rect);
                        _txtRoverBattery.Invalidate();

                        _txtRoverRange.Text = "  Range: " + splitRoverData[3];
                        _mainWindow.FillRect(_txtRoverRange.Rect);
                        _txtRoverRange.Invalidate();

                        _txtRoverHeading.Text = "Heading: " + splitRoverData[4];
                        _mainWindow.FillRect(_txtRoverHeading.Rect);
                        _txtRoverHeading.Invalidate();

                        _txtRoverTemp.Text = "Temp: " + splitRoverData[5];
                        _mainWindow.FillRect(_txtRoverTemp.Rect);
                        _txtRoverTemp.Invalidate();

                        _txtRoverPress.Text = "Press: " + splitRoverData[6];
                        _mainWindow.FillRect(_txtRoverPress.Rect);
                        _txtRoverPress.Invalidate();

                        _txtRoverLat.Text = "Lat: " + splitRoverData[7];
                        _mainWindow.FillRect(_txtRoverLat.Rect);
                        _txtRoverLat.Invalidate();

                        _txtRoverLon.Text = "Lon: " + splitRoverData[8];
                        _mainWindow.FillRect(_txtRoverLon.Rect);
                        _txtRoverLon.Invalidate();

                        _pBarConnected.Value = 1;
                        _mainWindow.FillRect(_pBarConnected.Rect);
                        _pBarConnected.Invalidate();

                        ////example output oakhill rover data
                        //roverData = "$ORD," +
                        //                    getBatteryVoltage().ToString() + " V"
                        //            + "," + getBatteryCurrent().ToString() + " A"
                        //            + "," + getRangeFwd() + " in"
                        //            + "," + IMU_Adafruit.Heading + " deg"
                        //            + "," + ((IMU_Adafruit.Bmp180.Temperature * 1.8000) + 32).ToString().Substring(0, 4) + " F"
                        //            + "," + (IMU_Adafruit.Bmp180.Pressure / 6895).ToString().Substring(0, 4) + " psi"
                        //    //+ "," + (1.0 - ((IMU_Adafruit.Bmp180.Pressure/101910)^.19)) //convert pressure to altitude in meters, 1019.1hPa as sealevel in Pawtucket
                        //            + "*";

                    }
                }
                catch (Exception)
                {

                }
            }
            else
            {
                //RadioButton radBtnConnected = (RadioButton)mainWindow.GetChildByName("radBtnConnected");
                _pBarConnected.Value = 0;
                _mainWindow.FillRect(_pBarConnected.Rect);
                _pBarConnected.Invalidate();
            }

            updateWindowCounter++;
        }

        /// <summary>
        /// Create speed cmd for land rover vehicle.
        /// </summary>
        /// <returns></returns>
        static public byte triggerSpeedCmd()
        {
            byte speedCmd = 0;

            //on land rover  0 to 134 is reverse, 135 = stop, 136 to 255 is forward.
            //about 139 is start of forward on truck

            speedCmd = (byte)convertLinearScale(speedTriggerAnalog.ReadRaw(), speedTriggerMin, speedTriggerMax, 0, 255);

            if ((speedCmd > speedTriggerNullMin) && (speedCmd < speedTriggerNullMax)) speedCmd = 135; //deadband around neutral
            else if (speedCmd >= speedTriggerNullMax) speedCmd = (byte)convertLinearScale(speedCmd, speedTriggerNullMax, 255, 136, speedTriggerMaxOutput);
            else speedCmd = (byte)convertLinearScale(speedCmd, 0, speedTriggerNullMin, 0, 134);

            return speedCmd;
            //Debug.Print("Motor Speed: " + speed.ToString());
        }

        /// <summary>
        /// Get and sets the maximum speed the controller can send to the rover.
        /// </summary>
        public static byte SPEED_TRIGGER_MAX_OUTPUT
        {
            get
            {
                return speedTriggerMaxOutput;
            }

            set 
            {
                if ((value > 136) && (value <= 255))
                    speedTriggerMaxOutput = value;
            }
        }

        #region MAIN WINDOW TAP EVENTS

        // Handles the next button tap event.
        static void btnSettings_TapEvent(object sender)
        {
            RoverJoystickControlTimer.Change(-1, -1); //stop timer
            UpdateMainWindowTimer.Change(-1, -1); //stop timer
            
            Tween.SlideWindow(_mainWindow, diagnosticWindow._window, GHI.Glide.Direction.Left);

            diagnosticWindow.DiagnosticWindowTimer.Change(0, SetupWindow.diagnosticWindowTimerPeriod);
        }

        // Handles the pictures button tap event.
        static void btnPics_TapEvent(object sender)
        {
            //check for sd card
            if (picCounter < 0)
            {
                Glide.MessageBoxManager.Show("SD Card not found.", "SD Card");
                return;
            }

            RoverJoystickControlTimer.Change(-1, -1); //stop timer
            UpdateMainWindowTimer.Change(-1, -1); //stop timer

            string[] sdCardfiles = Directory.GetFiles(SD_ROOT_DIRECTORY);
            string[] tempFileNames = new string[sdCardfiles.Length];

            //string[] folders = Directory.GetDirectories(SD_ROOT_DIRECTORY);
            FileStream fileStream = null;
            picCounter = 0;

            var i = 0;
            foreach (string sdfile in sdCardfiles)
            {
                if (sdfile.IndexOf(".jpg") != -1)
                    tempFileNames[i++] = sdfile;
            }

            if (tempFileNames.Length > 0)
            {
                sdCardJpgFileNames = new string[i];
                Array.Copy(tempFileNames, sdCardJpgFileNames, i);

                Tween.SlideWindow(_mainWindow, pictureWindow, GHI.Glide.Direction.Up);

                //Use FileStream to read a text file
                //fileStream = new FileStream(SD_ROOT_DIRECTORY + @"\" + sdCardJpgFileNames[0] + ".jpg", FileMode.Open);
                fileStream = new FileStream(sdCardJpgFileNames[picCounter], FileMode.Open);

                //load the data from the stream
                byte[] data = new byte[fileStream.Length];
                fileStream.Read(data, 0, data.Length);

                Bitmap myPic = new Bitmap(data, Bitmap.BitmapImageType.Jpeg);

                GHI.Glide.UI.Image image = (GHI.Glide.UI.Image)pictureWindow.GetChildByName("imgPicture");
                //image.Bitmap = Resources.GetBitmap(Resources.BitmapResources.logo);
                image.Bitmap.DrawImage(0, 0, myPic, 0, 0, 320, 240);

                fileStream.Close();

                image.Invalidate();

                PictureViewerTimer.Change(0,PictureViewerTimerPeriod);
            }
            else
            {
                Glide.MessageBoxManager.Show("No Pictures found.", "Picture Viewer");
                RoverJoystickControlTimer.Change(0,RoverJoystickTimerPeriod);
                UpdateMainWindowTimer.Change(0,UpdateMainWindowTimerPeriod);
            }

        }

        // Handles the next button tap event.
        static void btnGndEfx_TapEvent(object sender)
        {
            RoverJoystickControlTimer.Change(-1, -1); //stop timer
            UpdateMainWindowTimer.Change(-1, -1); //stop timer

            Tween.SlideWindow(_mainWindow, gEfxWindow._window, GHI.Glide.Direction.Left);
        }

        // Handles the next button tap event.
        static void btnTalk_TapEvent(object sender)
        {

        }

        /// <summary>
        /// Handle button video tap event.
        /// </summary>
        /// <param name="sender"></param>
        static void btnVideo_TapEvent(object sender)
        {

        }

        // Handles the next button tap event.
        static void btnCamera_TapEvent(object sender)
        {
            RoverJoystickControlTimer.Change(-1, -1); //stop timer
            UpdateMainWindowTimer.Change(-1, -1); //stop timer

            Tween.SlideWindow(_mainWindow, ecoCamWindow._window, GHI.Glide.Direction.Left);
        }

        #endregion

        #region UTILITIES

        /// <summary>
        /// Linearly scales standard 0 to 3.3V input.
        /// </summary>
        /// <param name="analogOutput">Analog input reading to convert.</param>
        /// <param name="scaleMin">Minimum scale reading.</param>
        /// <param name="scaleMax">Maximum scale reading.</param>
        static int analogLinearScale(double analogOutput, double scaleMin, double scaleMax)
        {
            int test = ((int)(analogOutput * ((scaleMax - scaleMin) / 3.3)));
            return ((int)(analogOutput * ((scaleMax - scaleMin) / 3.3)));
        }

        /// <summary>
        /// Scale an input linear scale to an output linear scale
        /// </summary>
        /// <param name="inputData"></param>
        /// <param name="inputMin"></param>
        /// <param name="inputMax"></param>
        /// <param name="outputMin"></param>
        /// <param name="outputMax"></param>
        /// <returns></returns>
        static public int convertLinearScale(double inputData, double inputMin, double inputMax, double outputMin, double outputMax)
        {
            if(inputData > inputMax)
              inputData = inputMax;

            if (inputData < inputMin)
                inputData = inputMin;

            double outputData = (((inputData - inputMin) / (inputMax - inputMin)) * (outputMax - outputMin)) + outputMin;
            //int test = (int)outputData;
            return (int)outputData;
        }

        /// <summary>
        /// Convert a byte to its ascii 2 character representation.
        /// </summary>
        /// <param name="data">Byte to convert.</param>
        /// <remarks>Example: if data = 255, FF will be returned in a character array.</remarks>
        public static char[] byteToHex(byte data)
        {
            byte[] nib = new byte[] { 0, 0 };

            nib[1] = (byte)(data & 0x0F);        //LSB
            nib[0] = (byte)((data & 0xF0) / 16); //MSB

            for (var i = 0; i < nib.Length; i++)
            {
                if (nib[i] <= 9)
                {
                    nib[i] = (byte)(nib[i] + '0');
                }
                else
                {
                    nib[i] = (byte)(nib[i] + 55);
                }
            }

            return (Encoding.UTF8.GetChars(nib));

            //return new string(Encoding.UTF8.GetChars(nib));
        }

        /// <summary>
        /// Calculate a checksum for given byte array.
        /// </summary>
        /// <param name="data">Byte array used to calculate checksum.</param>
        /// <remarks>Checksum includes all bytes in array beteen '$ and '*'.
        /// This function returns XOR checksum of data
        /// XOR is the odd function, high when odd inputs</remarks>
        public static byte getChecksum(byte[] data)
        {
            int i = 2;
            byte result = 0;

            result = data[1];

            while ((data[i] != 0x2A) && (i < data.Length))
            {
                result = (byte)(result ^ data[i]);
                i++;
            }

            return result;
        }

        public static void refreshButton(Window myWindow, GHI.Glide.UI.Button myBtn)
        {
            myWindow.FillRect(myBtn.Rect);
            myBtn.Invalidate();
        }

        public static void writeTextBlock(Window myWindow, TextBlock myTextBlock, string strOutput)
        {
            myTextBlock.Text = strOutput;
            myWindow.FillRect(myTextBlock.Rect);
            myTextBlock.Invalidate();
        }

        #endregion

        #region TouchScreen
        static void display_TouchDown(DisplayNhd5 sender, TouchEventArgs e)
        {
            //point loc;

            touches[0].X = e.X;
            touches[0].Y = e.Y;

            GlideTouch.RaiseTouchDownEvent(null, new GHI.Glide.TouchEventArgs(touches));

            //Debug.Print("Finger " + e.FingerNumber + " down!");
            //Debug.Print("Where " + e.X + "," + e.Y);

            //loc.X = e.X;
            //loc.Y = e.Y;

            //Core.RaiseTouchEvent(TouchType.TouchDown, loc);
        }

        static void display_TouchUp(DisplayNhd5 sender, TouchEventArgs e)
        {
            //point loc;

            touches[0].X = e.X;
            touches[0].Y = e.Y;

            GlideTouch.RaiseTouchUpEvent(null, new GHI.Glide.TouchEventArgs(touches));

            //Debug.Print("Finger " + e.FingerNumber + " up!");

            //loc.X = e.X;
            //loc.Y = e.Y;

            //Core.RaiseTouchEvent(TouchType.TouchUp, loc);
        }
        #endregion

        //uint[] timings1 = new uint[] { 200, 300, 200 };  //times in microseconds
        //SignalGenerator generator = new SignalGenerator(G120.Pin.P0_0, false, 10);
        //generator.Set(false, timings1, 0, 3, true);

    }
} 

        