using Microsoft.VisualBasic;
using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Nini.Config;
using Ionic.Zip;
using System.IO;
using System.Runtime.InteropServices;

namespace AionLauncher
{
    public partial class Launcher : Form
    {

        public Launcher()
        {
            InitializeComponent();
        } //end constructor
        //corner
        public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
		//drag zone
        [DllImportAttribute ("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, 
			int Msg, int wParam, int lParam);
		
		[DllImportAttribute ("user32.dll")]
		public static extern bool ReleaseCapture();

		//call functions to move the form
        private void drag_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            //Test launcher.ini (if all filled and set lang selectbox)
            if (!System.IO.File.Exists("launcher.ini"))
            {
                var NewIniConf = new NewIniConf();
                MessageBox.Show("This is the first time you launch the Launcher, Generating config files..", "First Time (step 1)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Thread.Sleep(500);
                MessageBox.Show("Before using this Launcher you must modificate your config file (launcher.ini)", "First Time (step 2)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start(@"launcher.ini");
                Application.Exit();
            }
            try
            {
            //new instance of nini
            IniConfigSource launcher = new IniConfigSource("launcher.ini");
            IConfig connectionSection = launcher.Configs["Connection"];
            IConfig gameSection = launcher.Configs["Game"];
            IConfig patchSection = launcher.Configs["Patch"];
            IConfig miscSection = launcher.Configs["Misc"];
            string HOST = connectionSection.Get("IP"); //can be DNS or IP.
            int PORT = connectionSection.GetInt("LoginPort");
            string OPTIONS = gameSection.Get("Options");
            string LANG = gameSection.Get("Language");
            int CC = gameSection.GetInt("CountryCode");
            string PATCH = patchSection.Get("Bin");
            string NEWSFEEDURL = miscSection.Get("BannerUrl");

            //set the lang selectbox
            LANG = LANG.Replace("fr", "FR");
            LANG = LANG.Replace("de", "DE");
            LANG = LANG.Replace("en", "EN");
            LANG = LANG.Replace("es", "ES");
            LANG = LANG.Replace("jp", "JP");
            this.lstLang.Text = LANG;
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid launcher.ini..", "Error: invalid version.ini", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }

            if (!System.IO.File.Exists("version.ini"))
            {
                var NewIni = new NewIni();
                MessageBox.Show("Error While trying to access version.ini.. Create it for you", "Error Accessing version.ini", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            try
            {
                IniConfigSource version = new IniConfigSource("version.ini");
                string current = version.Configs["Settings"].Get("Version");
                // Check if we need to download files
                if (current != "3.0.0.8")
                {
                    //Notify the user
                    MessageBox.Show("Your Client is Outaded or too recent. We will update it for you.");
                    //Update
                    btnLaunch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(149)))), ((int)(((byte)(0)))));
                    btnLaunch.GlowColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(122)))), ((int)(((byte)(0)))));
                    label1.Text = "Ready to update...";
                    btnLaunch.Text = "UPDATE";
                }
                else
                {
                    // Update Progress Bars etc.
                    progressBar1.Value = 100;
                    progressBar2.Value = 100;
                    label1.Text = "You Have the latest version";
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid version.ini..", "Error: invalid version.ini", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }

            //Rounded corners
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width - 0, Height - 0, 6, 6));
            Thread StatusThread = new Thread(new ThreadStart(this.CheckServerStatus));
            StatusThread.IsBackground = true;
            StatusThread.Start();
        } //end Launcher_Load

        //this pings the HOST and PORT specified in the Config class every 5 seconds as long as the program is running
        private void CheckServerStatus()
        {
            //Begin new instance nini
            IniConfigSource launcher = new IniConfigSource("launcher.ini");
            IConfig connectionSection = launcher.Configs["Connection"];

            //Get string-int
            string HOST = connectionSection.Get("IP");
            int PORT = connectionSection.GetInt("LoginPort");
            while (true)
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    try
                    {
                        if ((HOST == string.Empty || HOST == null))
                        {
                            MessageBox.Show("Please make sure your HOST is set in Version.ini", "Invalid HOST", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Application.Exit();
                        } //end if

                        socket.Connect(HOST, PORT);
                        socket.Close();

                        SetControlPropertyThreadSafe(lblServerStatus, "Text", "Online");
                        SetControlPropertyThreadSafe(lblServerStatus, "ForeColor", Color.Green);

                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                        {
                            SetControlPropertyThreadSafe(lblServerStatus, "Text", "Offline");
                            SetControlPropertyThreadSafe(lblServerStatus, "ForeColor", Color.Red);
                        } //end if
                    } //end try/catch
                } //end using
                //only check every 5 seconds while the program is up.
                Thread.Sleep(5000);
            } //end while
        } //end CheckServerStatus

        //this is a delegate used to access the UI from another thread
        private delegate void SetControlPropertyThreadSafeDelegate(Control control, string propertyName, object propertyValue);
        public static void SetControlPropertyThreadSafe(Control control, string propertyName, object propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new SetControlPropertyThreadSafeDelegate(SetControlPropertyThreadSafe), new object[] { control, propertyName, propertyValue });
            }
            else
            {
                control.GetType().InvokeMember(propertyName, System.Reflection.BindingFlags.SetProperty, null, control, new object[] { propertyValue });
            }

        }//end SetControlPropertyThreadSafe
        ////this launches the actual Play
        private void btnLaunch_Click(object sender, EventArgs e)
        {
            //Begin new instance nini
            IniConfigSource launcher = new IniConfigSource("launcher.ini");
            IConfig connectionSection = launcher.Configs["Connection"];
            IConfig PlaySection = launcher.Configs["Game"];
            IConfig patchSection = launcher.Configs["Patch"];

            //Get string-int
            string HOST = connectionSection.Get("IP");
            int PORT = connectionSection.GetInt("LoginPort");
            string OPTIONS = PlaySection.Get("Options");
            string LANG = PlaySection.Get("Language");
            int CC = PlaySection.GetInt("CountryCode");

            ChangeConf(null, null);

            if (btnLaunch.Text == "UPDATE")
            {
                timer1.Start();
                btnLaunch.UseWaitCursor = true;
                btnLaunch.Enabled = false;
            }
            else
            {
                var langEn = "EN";
              if("".Equals(lstLang.SelectedItem) || lstLang.SelectedItem == null) 
              { 
                  MessageBox.Show("Language not specified","Language Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
              }
              else 
              {
                  if (lstLang.SelectedItem.Equals(langEn)) 
                  {
                      lstLang.SelectedItem = "enu"; 
                  }
                //first, check to see if aion.bin can be found
                if (System.IO.File.Exists("bin32\\aion.bin"))
                {
                    System.Diagnostics.ProcessStartInfo aionLauncher =
                    new System.Diagnostics.ProcessStartInfo(
                    "cmd.exe",
                     "/c" + "\"bin32\\aion.bin\" -ip:" + ValidateIP(HOST) + " -ng -port:" + PORT + " -cc:" + CC + " -lang:" + lstLang.SelectedItem + OPTIONS);

                    aionLauncher.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    aionLauncher.CreateNoWindow = true;
                    try
                    {
                        System.Diagnostics.Process.Start(aionLauncher);

                        //wait 5 seconds, then close the launcher
                        Thread.Sleep(1000);
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An unknown error occurred while attempting to run aion.bin: " + ex.Message);
                    } //end try/catch
                }
                else
                {
                    MessageBox.Show("This program must be run under the Aion folder.", "aion.bin not found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //end if
            }
          } //end btnLaunch_Click
        }

        //news
        private void news_Tick(object sender, EventArgs e)
        {
            //new instance nini
            IniConfigSource launcher = new IniConfigSource("launcher.ini");
            IConfig miscSection = launcher.Configs["Misc"];

            //Get string
            string NEWSFEEDURL = miscSection.Get("BannerUrl");

            if (NEWSFEEDURL == null || NEWSFEEDURL == "")
            {
                    string news = "";
                    this.news_panel.BackgroundImage = global::AionLauncher.Properties.Resources.u3jsplashblank;
                    this.lblNews.Text = news;

            }else{
                this.BannerBrowser.Url = new System.Uri(NEWSFEEDURL, System.UriKind.Absolute);
                this.BannerBrowser.Visible = true;
            }
          NewsTimer.Stop();
        }
        //Proceed to update
        private void timer1_Tick(object sender, EventArgs e)
        {
            // New

            // Label change
            label1.Text = "Updating INI...";

            // Get configs
            IniConfigSource version = new IniConfigSource("version.ini");
            version.Configs["Settings"].Set("Version", "3.0.0.8");
            version.Save("version.ini");
      

            // Instead of downloading a new file, we just rewrite the INI

            // Progress bar update
            progressBar1.Value = 100;
            progressBar2.Value = 25;

            // Pause Timer
            timer1.Stop();
            timer2.Start();

        }

        // Timer 2 for updates

        private void timer2_Tick(object sender, EventArgs e)
        {
            // New

            // Since there is no need for this timer, just update the progressbar etc.

            // Label change
            label1.Text = "Re-Configuring Files...";

            // Progress bar update
            progressBar1.Value = 0;
            progressBar1.Value = 100;
            progressBar2.Value = 45;

            // Pause Timer
            timer2.Stop();
            timer3.Start();
        }

        // Timer 3 for updates

        private void timer3_Tick(object sender, EventArgs e)
        {
            // Label change
            label1.Text = "Deleting Old Files...";

            // Download update
            if (System.IO.File.Exists(System.Environment.CurrentDirectory + "/" + "bin32.zip") == true)
            {
                System.IO.File.Delete(System.Environment.CurrentDirectory + "/" + "bin32.zip");
            }

            // Progress bar update
            progressBar1.Value = 0;
            progressBar1.Value = 100;
            progressBar2.Value = 55;

            // Pause Timer
            timer3.Stop();
            timer4.Start();

        }

        // Timer 4 for updates

        private void timer4_Tick(object sender, EventArgs e)
        {
            IniConfigSource launcher = new IniConfigSource("launcher.ini");
            IConfig patchSection = launcher.Configs["Patch"];
            string PATCH = patchSection.Get("Bin");

            // Label change
            label1.Text = "Downloading bin32.zip...";

            // Download Files
            WebClient webClient = new WebClient();
            webClient.DownloadFile(PATCH, System.Environment.CurrentDirectory + "/" + "bin32.zip");

            // Progress bar update
            progressBar1.Value = 0;
            progressBar1.Value = 100;
            progressBar2.Value = 85;

            // Pause Timer
            timer4.Stop();
            timer5.Start();

        }

        // Timer 5 for updates

        private void timer5_Tick(object sender, EventArgs e)
        {
            // Label change
            label1.Text = "Unzipping bin32.zip...";

            // Unzipping
            string ZipToUnpack = "bin32.zip";
            string TargetDir = System.Environment.CurrentDirectory;
            Console.WriteLine("Extracting file {0} to {1}", ZipToUnpack, TargetDir);
            using (ZipFile zip = ZipFile.Read("bin32.zip"))
            {
                foreach (ZipEntry d in zip)
                {
                    d.Extract(TargetDir, ExtractExistingFileAction.OverwriteSilently);
                }
            }

            // Progress bar update
            progressBar1.Value = 100;
            progressBar2.Value = 100;

            label1.Text = "Update Complete...";
            btnLaunch.Enabled = true;
            btnLaunch.BackColor = System.Drawing.Color.MediumBlue;
            btnLaunch.GlowColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(61)))), ((int)(((byte)(245)))));
            btnLaunch.Text = "       RUN         GAME";
            btnLaunch.UseWaitCursor = false;

            // Pause Timer
            timer5.Stop();

        }

		 private class NewIni
        {
            string NL = Environment.NewLine;

            public NewIni()
            {
                try
                {
                    IniConfigSource config = new IniConfigSource("version.ini");
                }
                catch (Exception)
                {
                    fill_new_ini();
                }
            }

            private void fill_new_ini()
            {
                string toWrite = ";version.ini" + NL
                + "[Settings]" + NL
                + "Version = 3.0.0.0" + NL;
                System.IO.File.WriteAllText(@"version.ini", toWrite);
            }
        }
         //Write New launcher.ini conf
         private class NewIniConf
         {
             string NL = Environment.NewLine;

             public NewIniConf()
             {
                 try
                 {
                     IniConfigSource config = new IniConfigSource("launcher.ini");
                 }
                 catch (Exception)
                 {
                     fill_new_iniConf();
                 }
             }

             private void fill_new_iniConf()
             {
                 string toWrite = ";Configuration file for the launcher" + NL
                 + ";This file has been generated by the launcher, for any problem please report to the about section" + NL
                 + "[Connection]" + NL
                 + ";Your login ip/dns server (can be game server ip/dns too)" + NL
                 + "IP = vzoneserver.dyndns.org" + NL
                 + ";Your login port server (2106 by default)" + NL
                 + "LoginPort = 2106" + NL
                 + "" + NL
                 + "[Game]" + NL
                 + ";(string) Place here your game options" + NL
                 + "Options = -noauthgg -ls -noweb -nowebshop -ingameshop" + NL
                 + ";(2string) Place here the launguage of your client, one of these : en, de, fr, es, jp" + NL
                 + "Language = fr" + NL
                 + ";(2int) place here the country code of the server" + NL
                 + "CountryCode = 2" + NL
                 + "" + NL
                 + "[Patch]" + NL
                 + ";(url/bin32.zip) Place here your patch for the client, must be a zip archive" + NL
                 + "Bin = http://vzoneserver.dyndns.org/aion/bin32.zip" + NL
                 + "" + NL
                 + "[Misc]" + NL
                 + ";(url) Place here the webpage for the banner" + NL
                 + "BannerUrl = http://cmsstatic.aionfreetoplay.com/launcher_en.html" + NL;
                System.IO.File.WriteAllText(@"launcher.ini", toWrite);
             }
         }

        private string ValidateIP(string ip)
        {
            string returnValue = ip;

            if (!System.Text.RegularExpressions.Regex.Match(ip, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}").Success)
            {
                try
                {
                    returnValue = Dns.GetHostAddresses(ip)[0].ToString();
                }
                catch (Exception)
                {
                    returnValue = ip;
                } //end try/catch

            } //end if

            return returnValue;
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Working...", "Not In this version", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            while(this.Opacity != 0)
        {
        this.Opacity -= 0.05;
        Thread.Sleep(10);
        }
            Application.Exit();
        }
        private void btnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        //Launch Nav

        //future update
        //private void BannerBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        //{ 
        //    IniConfigSource launcher = new IniConfigSource("launcher.ini");
        //    IConfig miscSection = launcher.Configs["Misc"];
        //    string NEWSFEEDURL = miscSection.Get("BannerUrl");
        //    int i;
        //    for (i = 0; i < BannerBrowser.Document.Links.Count; i++)
        //    {
        //        if (!BannerBrowser.Document.Links.Equals(NEWSFEEDURL))
        //        {
        //          var navigateUrl = e.Url.ToString();
        //          e.Cancel = true;
        //           System.Diagnostics.Process.Start(navigateUrl);

        //        }
        //    }
        //}
        private void Getlink(object sender, WebBrowserNavigatingEventArgs e)
        {

            MessageBox.Show("Captured URL :" + e.Url.ToString());
        }

        //Change lang in conf
        private void ChangeConf(object sender, EventArgs e) {
            IniConfigSource launcher = new IniConfigSource("launcher.ini");
            IConfig gameSection = launcher.Configs["Game"];
            string LANG = gameSection.Get("Language");
            //set the lang selectbox
            LANG = LANG.Replace("fr", "FR");
            LANG = LANG.Replace("de", "DE");
            LANG = LANG.Replace("en", "EN");
            LANG = LANG.Replace("es", "ES");
            LANG = LANG.Replace("jp", "JP");
            if (!lstLang.SelectedItem.Equals(LANG)) 
                  {
                    gameSection.Set("Language", lstLang.SelectedItem);
                    launcher.Save();
                  }
        }
        //useless
        private void pctLogo_Click(object sender, EventArgs e){}
        private void news_panel_Paint(object sender, PaintEventArgs e){}
        private void labelLang_Click(object sender, EventArgs e){}
        private void label1_Click(object sender, EventArgs e){}
        private void progressBar1_Click(object sender, EventArgs e){}
        private void lblNews_Click(object sender, EventArgs e){}
        private void timer6_Tick(object sender, EventArgs e)
        {

        }
    } //end class
} //end namespace
