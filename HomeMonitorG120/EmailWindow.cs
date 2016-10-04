using System;
using System.Text;
using Microsoft.SPOT;

using GHI.Premium.Hardware;
//using GHIElectronics.NETMF.Hardware;

using System.Threading;
using System.Collections;

using GHIElectronics.NETMF.Glide;
using GHIElectronics.NETMF.Glide.Display;
using GHIElectronics.NETMF.Glide.UI;

namespace HomeMonitor
{
    class EmailWindow
    {
        private Window emailWindow;

        // This will hold DataGrid objects.
        private DataGrid dGridMailbox;
        private ConnectOneDevice emailDevice;
        private string settingsFilename = "EmailWindow.txt";
        private bool emailChange = false;

        public struct EmailAddress
        {
            private string _address;
            private string _state;

            public EmailAddress(string address, string state)
            {
                _address = address;
                _state = state;

                Address = address;
                State = state;
            }

            public string Address
            {
                get { return _address; }
                set 
                {
                    if (value == String.Empty)
                    {
                        _address = "nobody@nobody.com";
                        State = "OFF";
                    }
                    else if (value.Length <= 32)
                        _address = value;
                    else
                        _address = value.Substring(0, 32);
                }
            }

            public string State
            {
                get { return _state; }
                set
                {
                    if (value == "OFF" || value == "ON")
                    {
                        _state = value;
                    }
                    else
                        _state = "OFF";
                }
            }

        }

        public struct EmailMailbox
        {
            private string _username;
            private string _password;
            private string _smtpServer;
            private Int16 _smtpPort;
            private string _pop3Server;
            private Int16 _pop3Port;
            

            public EmailMailbox(string username, string password, string smtpServer, Int16 smtpPort, string pop3Server, Int16 pop3Port)
            {
                _username = username;
                _password = password;
                _smtpServer = smtpServer;
                _smtpPort = smtpPort;
                _pop3Server = pop3Server;
                _pop3Port = pop3Port;
            }

            public string Username
            {
                get { return _username; }
                set 
                {
                    if (value == String.Empty)
                        _username = "nobody@nobody.com";
                    else if (value.Length <= 32)
                        _username = value;
                    else
                        _username = value.Substring(0, 32);
                }
            }

            public string Password
            {
                get { return _password; }
                set
                {
                    if (value == String.Empty)
                        _password = "password";
                    else if (value.Length <= 32)
                        _password = value;
                    else
                        _password = value.Substring(0, 32);
                }
            }

            public string SmtpServer
            {
                get { return _smtpServer; }
                set
                {
                    if (value == String.Empty)
                        _smtpServer = "smtp.server.com";
                    else if (value.Length <= 32)
                        _smtpServer = value;
                    else
                        _smtpServer = value.Substring(0, 32);
                }
            }

            public Int16 SmtpPort
            {
                get { return _smtpPort; }
                set
                {
                    if (value < 0)
                    {
                        _smtpPort = 0;
                    }
                    else
                    {
                        _smtpPort = value;
                    }
                }
            }

            public string Pop3Server
            {
                get { return _pop3Server; }
                set
                {
                    if (value == String.Empty)
                        _pop3Server = "pop3.server.com";
                    else if (value.Length <= 32)
                        _pop3Server = value;
                    else
                        _pop3Server = value.Substring(0, 32);
                }
            }

            public Int16 Pop3Port
            {
                get { return _pop3Port; }
                set
                {
                    if (value < 0)
                    {
                        _pop3Port = 0;
                    }
                    else
                    {
                        _pop3Port = value;
                    }
                }
            }
        }

        public static EmailAddress[] EmailAddressArray = new EmailAddress[7];
        public static EmailMailbox UserMailbox;

        /// <summary>
        /// Creates an email setup window.
        /// </summary>
        /// <param name="_emailDevice">A device with email capabiilities.</param>
        public EmailWindow(ConnectOneDevice _emailDevice)
        {
            Glide.FitToScreen = true;
            // Tell Glide to use our custom keyboard.
            Glide.Keyboard = InitKeyboard();

            //load the window graphics
            this.emailWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.wndEmail));

            TextBox txtBoxEmailSetup = (TextBox)emailWindow.GetChildByName("txtBoxEmailSetup");
            txtBoxEmailSetup.TapEvent += new OnTap(Glide.OpenKeyboard);

            Button btnEnter = (Button)emailWindow.GetChildByName("btnEnter");
            btnEnter.TapEvent += new OnTap(btnEnter_TapEvent);

            Button btnExit = (Button)emailWindow.GetChildByName("btnExit");
            btnExit.TapEvent += new OnTap(btnExit_TapEvent);

            emailDevice =_emailDevice;

            #region Mailbox Datagrid Initialize

            // Setup the dataGrid reference.
            dGridMailbox = (DataGrid)emailWindow.GetChildByName("dGridMailbox");

            // Listen for tap cell events.
            dGridMailbox.TapCellEvent += new OnTapCell(dGridMailbox_TapCellEvent);

            // Create columns in data grid.
            dGridMailbox.AddColumn(new DataGridColumn("        MAILBOX", 125));
            dGridMailbox.AddColumn(new DataGridColumn("SETUP", 185)); 

            // Populate the data grid with data.
            PopulateMailbox(false);

            // Add the data grid to the window before rendering it.
            emailWindow.AddChild(dGridMailbox);
            dGridMailbox.Render();

            #endregion

        }

        /// <summary>
        /// Returns the primary window for this code module.
        /// </summary>
        public Window getWindow()
        {
            return emailWindow;
        }

        /// <summary>
        /// Handles the dGridMailbox tap event.
        /// </summary>
        private void dGridMailbox_TapCellEvent(object sender, TapCellEventArgs args)
        {
            object[] data = dGridMailbox.GetRowData(args.RowIndex);

            if (args.ColumnIndex == 0  && args.RowIndex >= 1 && args.RowIndex <= 7 ) //if you tapped zeroeth column, turn email addresses on/off
            {
                if (EmailAddressArray[args.RowIndex - 1].State == "OFF")
                {
                    dGridMailbox.SetCellData(args.ColumnIndex, args.RowIndex, "ON");
                    EmailAddressArray[args.RowIndex - 1].State = "ON";
                    //setMailboxField(EmailAddressArray[args.RowIndex-1].Address, args.RowIndex);
                }
                else
                {
                    dGridMailbox.SetCellData(args.ColumnIndex, args.RowIndex, "OFF");
                    EmailAddressArray[args.RowIndex - 1].State = "OFF";
                    //setMailboxField(String.Empty, args.RowIndex); //clear the email address in the ATI so it wont send to this address
                }

                emailChange = true;
                dGridMailbox.Invalidate();    
            }
            /*
            if ((args.ColumnIndex == 1) && (data[0].ToString().IndexOf("SMTP Authen:") != -1)) //if you tapped SMTP Authent field
            {

                if (data[1].ToString() == "no")
                {
                    dGridMailbox.SetCellData(args.ColumnIndex, args.RowIndex, "yes");
                    emailDevice.setSMTPAuthent(true);
                }
                else
                {
                    dGridMailbox.SetCellData(args.ColumnIndex, args.RowIndex, "no");
                    emailDevice.setSMTPAuthent(false);
                }
               
                emailChange = true;
                dGridMailbox.Invalidate();
            }
             */
        }

        /// <summary>
        /// Handles the enter button tap event.
        /// </summary>
        private void btnEnter_TapEvent(object sender)
        {            
            TextBox txtBoxEmailSetup = (TextBox)emailWindow.GetChildByName("txtBoxEmailSetup");

            switch (dGridMailbox.SelectedIndex)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:

                    EmailAddressArray[dGridMailbox.SelectedIndex - 1].Address = txtBoxEmailSetup.Text.Trim();
                    
                    dGridMailbox.SetCellData(1, dGridMailbox.SelectedIndex, EmailAddressArray[dGridMailbox.SelectedIndex - 1].Address);
                    dGridMailbox.SetCellData(0, dGridMailbox.SelectedIndex, EmailAddressArray[dGridMailbox.SelectedIndex - 1].State);
                    
                    /*
                    //if the on/off column set to off, clear email address in connect one device, so email is not sent
                    if (EmailAddressArray[dGridMailbox.SelectedIndex - 1].State == "OFF")
                        setMailboxField(String.Empty, dGridMailbox.SelectedIndex); 
                    else
                        setMailboxField(EmailAddressArray[dGridMailbox.SelectedIndex - 1].Address, dGridMailbox.SelectedIndex);
                    */

                    txtBoxEmailSetup.Text = String.Empty;
                    emailChange = true;

                    break;

                case 9: //mailbox username

                    if (txtBoxEmailSetup.Text.Trim() == String.Empty)
                        UserMailbox.Username = "";  
                    else
                        UserMailbox.Username = txtBoxEmailSetup.Text.Trim();

                    dGridMailbox.SetCellData(1, dGridMailbox.SelectedIndex, UserMailbox.Username);
                    emailChange = true;
                    break;

                case 10: //mailbox password

                    if (txtBoxEmailSetup.Text.Trim() == String.Empty)
                        UserMailbox.Password = "";  
                    else
                        UserMailbox.Password = txtBoxEmailSetup.Text.Trim();

                    dGridMailbox.SetCellData(1, dGridMailbox.SelectedIndex, UserMailbox.Password);
                    emailChange = true;
                    break;

                case 11: //SMTP server

                    if (txtBoxEmailSetup.Text.Trim() == String.Empty)
                        UserMailbox.SmtpServer = "";  
                    else
                        UserMailbox.SmtpServer = txtBoxEmailSetup.Text.Trim();

                    dGridMailbox.SetCellData(1, dGridMailbox.SelectedIndex, UserMailbox.SmtpServer);
                    emailChange = true;
                    break;

                case 12: //SMTP port

                    if (txtBoxEmailSetup.Text.Trim() == String.Empty)
                        UserMailbox.SmtpPort = 0;
                    else
                    {
                        try
                        {
                            UserMailbox.SmtpPort = Convert.ToInt16(txtBoxEmailSetup.Text.Trim());
                        }
                        catch (Exception)
                        {
                            UserMailbox.SmtpPort = 0;
                        }
                    }

                    dGridMailbox.SetCellData(1, dGridMailbox.SelectedIndex, UserMailbox.SmtpPort.ToString());
                    emailChange = true;
                    break;

                case 13: //Pop3 Server

                    if (txtBoxEmailSetup.Text.Trim() == String.Empty)
                        UserMailbox.Pop3Server= "";  
                    else
                        UserMailbox.Pop3Server = txtBoxEmailSetup.Text.Trim();

                    dGridMailbox.SetCellData(1, dGridMailbox.SelectedIndex, UserMailbox.Pop3Server);
                    emailChange = true;
                    break;

                case 14://Pop3 Port

                    if (txtBoxEmailSetup.Text.Trim() == String.Empty)
                        UserMailbox.Pop3Port = 0;
                    else
                    {
                        try
                        {
                            UserMailbox.Pop3Port = Convert.ToInt16(txtBoxEmailSetup.Text.Trim());
                        }
                        catch (Exception)
                        {
                            UserMailbox.Pop3Port = 0;
                        }
                    }

                    dGridMailbox.SetCellData(1, dGridMailbox.SelectedIndex, UserMailbox.Pop3Port.ToString());
                    emailChange = true;
                    break;

                default:
                    break;
            }

            dGridMailbox.Invalidate();
            updateTxtBox("txtBoxEmailSetup");
        }

        /// <summary>
        /// Handles the exit button tap event.
        /// </summary>
        private void btnExit_TapEvent(object sender)
        {
            //updateButtons();
            if (emailChange)
            {
                //blank out passwords
                dGridMailbox.SetCellData(1, 10, "*******");
                dGridMailbox.Invalidate();

                //saveWindow();
                saveWindowLines();
                emailChange = false;
            }

            Tween.SlideWindow(emailWindow, Program.mainWnd, Direction.Down);
        }

        /// <summary>
        /// Initialization of mailbox data grid with info.
        /// </summary>
        /// <param name="invalidate">Determines whether or not to refresh data grid.</param>
        private void PopulateMailbox(bool invalidate)
        {
            // DataGridItems must contain an object array whose length matches the number of columns.
            dGridMailbox.AddItem(new DataGridItem(new object[2] { "-----E-Mail-----", "-----Addresses-----" }));

            for (int i = 0; i < EmailAddressArray.Length; i++)
            {
                EmailAddressArray[i] = new EmailAddress("nobody@nobody.com", "OFF");
                dGridMailbox.AddItem(new DataGridItem(new object[2] { EmailAddressArray[i].State, EmailAddressArray[i].Address }));
            }

            UserMailbox = new EmailMailbox("nobody@nobody.com", "password", "smtp.server.com", 0, "pop3.server.com", 0);

            dGridMailbox.AddItem(new DataGridItem(new object[2] { "-----Mailbox----", "-------Setup------" }));
            dGridMailbox.AddItem(new DataGridItem(new object[2] { "Username:"       , UserMailbox.Username }));
            dGridMailbox.AddItem(new DataGridItem(new object[2] { "Password:"       , UserMailbox.Password }));
            dGridMailbox.AddItem(new DataGridItem(new object[2] { "SMTP Server:"    , UserMailbox.SmtpServer}));
            dGridMailbox.AddItem(new DataGridItem(new object[2] { "SMTP Port:"      , UserMailbox.SmtpPort.ToString() }));
            dGridMailbox.AddItem(new DataGridItem(new object[2] { "POP3 Server:"    , UserMailbox.Pop3Server }));
            dGridMailbox.AddItem(new DataGridItem(new object[2] { "POP3 Port:"      , UserMailbox.Pop3Port.ToString() }));
     
            if (invalidate)
                dGridMailbox.Invalidate();
        }

        /// <summary>
        /// Sends the mailbox field from the datagrid to the email device.
        /// </summary>
        /// <param name="input">The selected text from the data grid.</param>
        /// <param name="selectedIndex">The selected index on the datagrid.</param>
        private void setMailboxField(string input, int selectedIndex)
        { 
            switch (selectedIndex)
            {             
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    emailDevice.setEmailAddressee(input.Trim(), selectedIndex-1);
                    break;

                case 9: //SMTP username
                    emailDevice.setSMTPUsername(input);
                    break;

                case 10:
                    emailDevice.setSMTPServer(input);
                    break;

                case 11:
                    emailDevice.setSMTPport(Convert.ToInt16(input));
                    break;

                case 12:
                    emailDevice.setSMTPpswd(input);
                    break;

              //case 13: //handled in tap event
                  //  if(input== "yes")
                  //      emailDevice.setSMTPAuthent(true);
                  //  else if (input == "no")
                  //      emailDevice.setSMTPAuthent(false);
                  //  break;

                case 14:
                    emailDevice.setPOP3server(input);
                    break;

                case 15:
                    emailDevice.setPOP3mailbox(input);
                    break;

                case 16:
                    emailDevice.setPOP3pswd(input);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Saves window's data
        /// </summary>
        public void saveWindow()
        {
            byte[] tempArray = new byte[35]; //array length is EmailAddress.Address.Length + EmailAddress.State.Length
            byte[] saveArray = new byte[tempArray.Length * EmailAddressArray.Length];

            // Add setpoint data to data grid
            for (int i = 0; i < EmailAddressArray.Length; i++)
            {
                Array.Copy(Encoding.UTF8.GetBytes(EmailAddressArray[i].Address), tempArray, EmailAddressArray[i].Address.Length);
                Array.Copy(Encoding.UTF8.GetBytes(EmailAddressArray[i].State), 0, tempArray, EmailAddressArray[i].Address.Length, EmailAddressArray[i].State.Length);
                Array.Copy(tempArray, 0, saveArray, i * tempArray.Length, tempArray.Length);
            }

            Program.SDCard.WriteFileBytes(settingsFilename, saveArray);
        }

        /// <summary>
        /// Saves window's data
        /// </summary>
        public void saveWindowLines()
        {
            int i = 0;
            string[] saveArray = new string[20];

            // Each saveArray index is a line in the saved .txt file on the SD card
            //for (i = 0; i < EmailAddressArray.Length; i++)
            for (i = 0; i < 8; i++)
            {
                saveArray[i] = "Hi";
            }
             
            //saveArray[i++] = UserMailbox.Username;
            //saveArray[i++] = UserMailbox.Password;
            //saveArray[i++] = UserMailbox.SmtpServer;
            //saveArray[i++] = UserMailbox.SmtpPort.ToString();
            //saveArray[i++] = UserMailbox.Pop3Server;
            //saveArray[i++] = UserMailbox.Pop3Port.ToString();

            saveArray[i++] = "Hi";
            saveArray[i++] = "Hi";
            saveArray[i++] = "Hi";
            saveArray[i++] = "Hi";
            saveArray[i++] = "Hi";
            saveArray[i++] = "Hi";

            Program.SDCard.WriteFileLines(settingsFilename, saveArray);
        }

        /// <summary>
        /// Recover's window's data from SD card.
        /// </summary>
        public bool recoverWindow()
        {
            int j = 0;
            byte[] tempArray = new byte[35]; //array length is SerialNumber.Length + Location.Length + Setting.Length
            byte[] saveArray = new byte[tempArray.Length * EmailAddressArray.Length];

            saveArray = Program.SDCard.ReadFile(settingsFilename, saveArray.Length);

            if (saveArray == null) return false;

            string strSaveArray = new string(System.Text.Encoding.UTF8.GetChars(saveArray), 0, saveArray.Length);

            for (int i = 0; i < EmailAddressArray.Length; i++)
            {
                EmailAddressArray[i].Address = strSaveArray.Substring(j, 32); j += 32;
                EmailAddressArray[i].State = strSaveArray.Substring(j, 3); j += 3;

                dGridMailbox.SetCellData(0, i+1, EmailAddressArray[i].State);
                dGridMailbox.SetCellData(1, i+1, EmailAddressArray[i].Address);

                if (EmailAddressArray[i].State == "ON ")
                    emailDevice.setEmailAddressee(EmailAddressArray[i].Address.Trim(), i);
                else
                    emailDevice.setEmailAddressee(String.Empty, i);
            }

            //code to query connectOneDevice for mailbox setup
            dGridMailbox.SetCellData(1, 9,  emailDevice.getSMTPUsername());
            dGridMailbox.SetCellData(1, 10, emailDevice.getSMTPServer());
            dGridMailbox.SetCellData(1, 11, emailDevice.getSMTPPort());
            dGridMailbox.SetCellData(1, 13, emailDevice.getSMTPAuthent());
            dGridMailbox.SetCellData(1, 14, emailDevice.getPOP3server());
            dGridMailbox.SetCellData(1, 15, emailDevice.getPOP3mailbox());

            dGridMailbox.Invalidate();

            return true;
        }

        /// <summary>
        /// Recover's window's data from SD card.
        /// </summary>
        public bool recoverWindowLines()
        {
            int i = 0;
            string[] saveArray = new string[dGridMailbox.NumItems];

            saveArray = Program.SDCard.ReadFileLines(settingsFilename, saveArray.Length);

            if (saveArray == null) return false;

            for (i = 0; i < EmailAddressArray.Length; i++)
            {
                EmailAddressArray[i].Address = saveArray[i].Substring(0,saveArray[i].LastIndexOf(","));
                EmailAddressArray[i].State = saveArray[i].Substring(saveArray[i].LastIndexOf(",")+1);

                dGridMailbox.SetCellData(0, i + 1, EmailAddressArray[i].State);
                dGridMailbox.SetCellData(1, i + 1, EmailAddressArray[i].Address);
            }

            UserMailbox.Username = saveArray[i++];
            UserMailbox.Password = saveArray[i++];
            UserMailbox.SmtpServer = saveArray[i++];
            UserMailbox.SmtpPort = Convert.ToInt16(saveArray[i++]);
            UserMailbox.Pop3Server = saveArray[i++];
            UserMailbox.Pop3Port = Convert.ToInt16(saveArray[i++]);
            
            //code to populate grid
            dGridMailbox.SetCellData(1,  9, UserMailbox.Username);
            dGridMailbox.SetCellData(1, 10, "*******");//UserMailbox.Password
            dGridMailbox.SetCellData(1, 11, UserMailbox.SmtpServer);
            dGridMailbox.SetCellData(1, 12, UserMailbox.SmtpPort.ToString());
            dGridMailbox.SetCellData(1, 13, UserMailbox.Pop3Server);
            dGridMailbox.SetCellData(1, 14, UserMailbox.Pop3Port.ToString());

            dGridMailbox.Invalidate();

            return true;
        }

        /// <summary>
        /// Updates textbox on screen.
        /// </summary>
        /// <param name="txtBoxChildName">The name of the text box instance.</param>
        private void updateTxtBox(string txtBoxChildName)
        {
            TextBox txtBox = (TextBox)emailWindow.GetChildByName(txtBoxChildName);
            emailWindow.FillRect(txtBox.Rect);
            txtBox.Invalidate();
        }

        /// <summary>
        /// Initialize keyboard.
        /// </summary>
        private Keyboard InitKeyboard()
        {
            Keyboard keyboard = new Keyboard(320, 128, 3, 32, 0);

            // Each view with keys in a up position.
            keyboard.BitmapUp = new Bitmap[4]
            {
                Resources.GetBitmap(Resources.BitmapResources.Keyboard_320x128_Up_Uppercase),
                Resources.GetBitmap(Resources.BitmapResources.Keyboard_320x128_Up_Lowercase),
                Resources.GetBitmap(Resources.BitmapResources.Keyboard_320x128_Up_Numbers),
                Resources.GetBitmap(Resources.BitmapResources.Keyboard_320x128_Up_Symbols)
            };

            //// Each view with keys in a down position.
            //keyboard.BitmapDown seems to have been removed from latest Glide libraries I DID THIS
            //keyboard.BitmapDown = new Bitmap[4]
            //{
            //    Resources.GetBitmap(Resources.BitmapResources.Keyboard_320x128_Down_Uppercase),
            //    Resources.GetBitmap(Resources.BitmapResources.Keyboard_320x128_Down_Lowercase),
            //    Resources.GetBitmap(Resources.BitmapResources.Keyboard_320x128_Down_Numbers),
            //    Resources.GetBitmap(Resources.BitmapResources.Keyboard_320x128_Down_Symbols)
            //};

            // We must set the default key content.

            string[][] keyContent = new string[4][];

            // Letters
            keyContent[0] = new string[10] { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p" };
            keyContent[1] = new string[9] { "a", "s", "d", "f", "g", "h", "j", "k", "l" };
            keyContent[2] = new string[9] { Keyboard.ActionKey.ToggleCase, "z", "x", "c", "v", "b", "n", "m", Keyboard.ActionKey.Backspace };
            keyContent[3] = new string[5] { Keyboard.ActionKey.ToNumbers, ",", Keyboard.ActionKey.Space, ".", Keyboard.ActionKey.Return };
            keyboard.SetViewKeyContent(Keyboard.View.Letters, keyContent);

            // Numbers
            keyContent[0] = new string[10] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            keyContent[1] = new string[10] { "@", "#", "$", "%", "&", "*", "-", "+", "(", ")" };
            keyContent[2] = new string[9] { Keyboard.ActionKey.ToSymbols, "!", "\"", "'", ":", ";", "/", "?", Keyboard.ActionKey.Backspace };
            keyContent[3] = new string[5] { Keyboard.ActionKey.ToLetters, ",", Keyboard.ActionKey.Space, ".", Keyboard.ActionKey.Return };
            keyboard.SetViewKeyContent(Keyboard.View.Numbers, keyContent);

            // Symbols
            keyContent[0] = new string[10] { "~", "`", "|", "•", "√", "π", "÷", "×", "{", "}" };
            keyContent[1] = new string[10] { Keyboard.ActionKey.Tab, "£", "¢", "€", "º", "^", "_", "=", "[", "]" };
            keyContent[2] = new string[9] { Keyboard.ActionKey.ToNumbers, "™", "®", "©", "¶", "\\", "<", ">", Keyboard.ActionKey.Backspace };
            keyContent[3] = new string[5] { Keyboard.ActionKey.ToLetters, ",", Keyboard.ActionKey.Space, ".", Keyboard.ActionKey.Return };
            keyboard.SetViewKeyContent(Keyboard.View.Symbols, keyContent);

            // or we could just call this:
            // keyboard.DefaultKeyContent();

            int[][] keyWidth = new int[4][];

            // Each array entry represents a row of keys on the keyboard top-down (0-3)
            // Each array within these entries contains the widths of the keys for that row.
            // For example: keyWidth[0] = new int[10] { 48, 48, 48, 48, 48, 48, 48, 48, 48, 48 }
            // represents the first row (0) which contains the keys Q, W, E, R, T, Y, U, I, O, P

            // Letters
            keyWidth[0] = new int[10] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 };
            keyWidth[1] = new int[9] { 32, 32, 32, 32, 32, 32, 32, 32, 32 };
            keyWidth[2] = new int[9] { 48, 32, 32, 32, 32, 32, 32, 32, 48 };
            keyWidth[3] = new int[5] { 48, 32, 160, 32, 48 };

            keyboard.SetViewKeyWidth(Keyboard.View.Letters, keyWidth);

            // Numbers
            keyWidth[0] = new int[10] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 };
            keyWidth[1] = new int[10] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 };
            keyWidth[2] = new int[9] { 48, 32, 32, 32, 32, 32, 32, 32, 48 };
            keyWidth[3] = new int[5] { 48, 32, 160, 32, 48 };

            keyboard.SetViewKeyWidth(Keyboard.View.Numbers, keyWidth);

            // Symbols
            keyWidth[0] = new int[10] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 };
            keyWidth[1] = new int[10] { 32, 32, 32, 32, 32, 32, 32, 32, 32, 32 };
            keyWidth[2] = new int[9] { 48, 32, 32, 32, 32, 32, 32, 32, 48 };
            keyWidth[3] = new int[5] { 48, 32, 160, 32, 48 };

            keyboard.SetViewKeyWidth(Keyboard.View.Symbols, keyWidth);

            keyboard.CalculateKeys();
            return keyboard;
        }

    }
}
