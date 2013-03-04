using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text.RegularExpressions;
using Nini.Config;
using Ionic.Zip;
using System.IO;
using System.Runtime.Serialization.Json;

namespace AionLauncher
{
    public partial class Launcher : Form
    {
        public Launcher()
        {
            //Load Embed Dlls
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                string resourceName = new AssemblyName(args.Name).Name + ".dll";
                string resource = Array.Find(this.GetType().Assembly.GetManifestResourceNames(), element => element.EndsWith(resourceName));

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

            //Init Controls
            InitializeComponent();

            //Load Fonts
            LoadFont();
            btnLaunch.Font = new Font(private_fonts.Families[0], 9.5F);
            btnLaunch.UseCompatibleTextRendering = true;
        
        } //end constructor

        //corner
        public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
		//drag zone
        [DllImportAttribute ("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		
		[DllImportAttribute ("user32.dll")]
		public static extern bool ReleaseCapture();

        [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true)]
        public static extern int AddFontResource([In][MarshalAs(UnmanagedType.LPWStr)]
        string lpFileName);

        public static bool ForceCheck { get; set; }
        public static int BannerCode { get; set; }
        public static string ChangeBanner { get; set; }

		//call functions to move the form
        private void drag_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        
        //Create NewPrivate Font
        PrivateFontCollection private_fonts = new PrivateFontCollection();
        private void LoadFont()
        {
            string ressource = "AionLauncher.TCCEB.TTF";
            Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ressource);
            System.IntPtr data = Marshal.AllocCoTaskMem((int)fontStream.Length);
            byte[] fontdata = new byte[fontStream.Length];
            fontStream.Read(fontdata, 0, (int)fontStream.Length);
            Marshal.Copy(fontdata, 0, data, (int)fontStream.Length);
            private_fonts.AddMemoryFont(data, (int)fontStream.Length);
            fontStream.Close();
            Marshal.FreeCoTaskMem(data);
        }

        private void Launcher_Load(object sender, EventArgs e)
        {

            //Test launcher.ini (if all filled and set lang selectbox)
            if (!System.IO.File.Exists("launcher.ini"))
            {
                var NewIniConf = new NewIniConf();
                MessageBox.Show("This is the first time you launch the Launcher, Generating config files..", "First Time (step 1)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Thread.Sleep(500);
                MessageBox.Show("Before using this Launcher you must modificate the configuration.", "First Time (step 2)", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnSettings_Click(null, null);
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
            bool AUTOL = miscSection.GetBoolean("AutoStart");

            //set the lang selectbox
            LANG = LANG.Replace("fr", "FR");
            LANG = LANG.Replace("de", "DE");
            LANG = LANG.Replace("en", "EN");
            LANG = LANG.Replace("es", "ES");
            LANG = LANG.Replace("ru", "RU");
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
                    this.Opacity = 0f;
                    Application.DoEvents();
                IniConfigSource version = new IniConfigSource("version.ini");
                string current = version.Configs["Settings"].Get("Version");
                // Check if we need to download files
                if (current != "3.0.0.8" | ForceCheck == true)
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
                        this.Opacity = 0f;
                        Application.DoEvents();

                    // Update Progress Bars etc.
                    progressBar1.Value = 100;
                    progressBar2.Value = 100;
                    label1.Text = "You Have the latest version";

                    Thread AutoStartThread = new Thread(new ThreadStart(this.AutoStart));
                    AutoStartThread.Start();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid version.ini..", "Error: invalid version.ini", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }

            //Rounded corners
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width - 0, Height - 0, 6, 6));
                this.Opacity = 100.0f;
                this.ShowInTaskbar = true;

            Thread StatusThread = new Thread(new ThreadStart(this.CheckServerStatus));
            StatusThread.IsBackground = true;
            StatusThread.Start();
            Thread CheckVersionThread = new Thread(new ThreadStart(this.CheckVersions));
            CheckVersionThread.IsBackground = true;
            CheckVersionThread.Start();

        } //end Launcher_Load

        private void AutoStart()
        {
            IniConfigSource launcher = new IniConfigSource("launcher.ini");
            IConfig miscSection = launcher.Configs["Misc"];
            bool AUTOL = miscSection.GetBoolean("AutoStart");
            if (AUTOL == true)
            {
                AutoStartTimer.Start();
                int number = 5;
                while (btnLaunch.Text != "RUN\r\nGAME\r\n0")
                {
                    if (AutoStartTimer.Enabled == false) { break; }
                    SetControlPropertyThreadSafe(btnLaunch, "Text", "RUN\r\nGAME\r\n" + number--);
                    Thread.Sleep(1000);
                }
                if (btnLaunch.Text == "RUN\r\nGAME\r\n0")
                {
                    SetControlPropertyThreadSafe(btnLaunch, "Text", "RUN\r\nGAME");
                }
            }
        }

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

        private void LaunchGame()
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

            if (btnLaunch.Text == "UPDATE")
            {
                timer1.Start();
                btnLaunch.UseWaitCursor = true;
                btnLaunch.Enabled = false;
            }
            else
            {
                //first, check to see if aion.bin can be found
                if (System.IO.File.Exists("bin32\\aion.bin"))
                {
                    System.Diagnostics.ProcessStartInfo aionLauncher =
                    new System.Diagnostics.ProcessStartInfo(
                    "cmd.exe",
                     "/c" + "\"bin32\\aion.bin\" -ip:" + ValidateIP(HOST) + " -ng -port:" + PORT + " -cc:" + CC + " -lang:" + LANG + OPTIONS);

                    aionLauncher.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    aionLauncher.CreateNoWindow = true;
                    try
                    {
                        System.Diagnostics.Process.Start(aionLauncher);

                        //wait 0.5 seconds, then close the launcher
                        Thread.Sleep(500);
                        while (this.Opacity != 0)
                        {
                            this.Opacity -= 0.05;
                            Thread.Sleep(10);
                        }
                        Application.Exit();
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
            } //end btnLaunch_Click
        }

        //Events

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
        //Refresh Banner if changed
        private void ChangedBanner()
        {
            string NEWSFEEDURL = ChangeBanner;

            if (BannerCode == 200)
            {
                this.BannerBrowser.Navigate(NEWSFEEDURL);
            }
            else if (BannerCode == 0)
            {

            }
            else if(BannerCode != 200)
            {
                string news = "";
                this.news_panel.BackgroundImage = global::AionLauncher.Properties.Resources.u3jsplashblank;
                this.lblNews.Text = news;
            }
        }
        private void Fcheck()
        {
            if (ForceCheck == true)
            {
                //Update
                btnLaunch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(149)))), ((int)(((byte)(0)))));
                btnLaunch.GlowColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(122)))), ((int)(((byte)(0)))));
                label1.Text = "Ready to update...";
                btnLaunch.Text = "UPDATE";
            }
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
                + "BannerUrl = http://cmsstatic.aionfreetoplay.com/launcher_en.html" + NL
                + "; (bool)Game Start" + NL
                + "AutoStart = false" + NL
                + "; (2string)" + NL
                + "LaunchLanguage = en";

                System.IO.File.WriteAllText(@"launcher.ini", toWrite);
            }
        }
        //Validate Ip/Dns
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
                }

            }

            return returnValue;
        }

        //Launch Nav
        private void BannerBrowser_NewWindow(object sender, CancelEventArgs e)
        {
            var webbrowser = (WebBrowser)sender;
            e.Cancel = true;
            OpenWebsite(webbrowser.StatusText.ToString());
            webbrowser = null;
        }
        public static void OpenWebsite(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        //Proceed to update
        private void timer1_Tick(object sender, EventArgs e)
        {
            IniConfigSource launcher = new IniConfigSource("launcher.ini");
            IConfig patchSection = launcher.Configs["Patch"];
            string PATCH = patchSection.Get("Bin");
            string strRegex = @"bin32.zip";
            RegexOptions myRegexOptions = RegexOptions.None;
            Regex myRegex = new Regex(strRegex, myRegexOptions);
            string strReplace = @"";
            string URL = myRegex.Replace(PATCH, strReplace);

            // Label change
            label1.Text = "Checking Server...";

            try
            {
                HttpWebRequest request = WebRequest.Create(URL) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK | response.StatusCode == HttpStatusCode.Accepted | response.StatusCode == HttpStatusCode.Forbidden | response.StatusCode == HttpStatusCode.NoContent | response.StatusCode == HttpStatusCode.NotFound)
                {
                    label1.Text = "Connection OK";
                    // Progress bar update
                    progressBar1.Value = 100;
                    progressBar2.Value = 25;
                    // Pause Timer
                    timer1.Stop();
                    timer2.Start();
                    // Timer 2 for updates
                }
            }
            catch
            {
                label1.Text = "Connection Error";
                timer1.Stop();
                MessageBox.Show("Unable to connect to the game updater, please try again or contact the support", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label1.Text = "Ready to Update..";
                btnLaunch.Enabled = true;
                btnLaunch.UseWaitCursor = false;
            }

        }
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

            label1.Text = "Updating ini...";
            // Get configs
            IniConfigSource version = new IniConfigSource("version.ini");
            version.Configs["Settings"].Set("Version", "3.0.0.8");
            version.Save("version.ini");

            // Progress bar update
            progressBar1.Value = 100;
            progressBar2.Value = 100;

            label1.Text = "Update Complete...";
            btnLaunch.Enabled = true;
            btnLaunch.BackColor = System.Drawing.Color.MediumBlue;
            btnLaunch.GlowColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(61)))), ((int)(((byte)(245)))));
            btnLaunch.Text = "RUN\r\nGAME";
            btnLaunch.UseWaitCursor = false;

            // Pause Timer
            timer5.Stop();

        }

        //Timeout Launcher
        private void AutoStartTimer_Tick(object sender, EventArgs e)
        {
            AutoStartTimer.Stop();
            LaunchGame();
        }
        //get uodate version
        public class JsonHelper
        {
            public static T JsonDeserialize<T>(string jsonString)
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                T obj = (T)ser.ReadObject(ms);
                return obj;
            }
        }
        public class JsonParser{public string version { get; set; }}

        //CheckVersion (Please do not modify these lines)
        private void CheckVersions()
        {
            if (Environment.OSVersion.Version >= new Version(6, 0))
            {
                WebClient getJson = new WebClient();
                string json = getJson.DownloadString("http://aion-launcher-beta.googlecode.com/svn/trunk/Launcher/Parser/index.php");
                JsonParser LauncherJson = JsonHelper.JsonDeserialize<JsonParser>(json);

                string currentVersion = Application.ProductVersion;
                string getVersion = LauncherJson.version;


                if (currentVersion != getVersion)
                {

                    this.Loading.Image = global::AionLauncher.Properties.Resources.update;
                    SetControlPropertyThreadSafe(CheckVersionLbl, "Location", new System.Drawing.Point(514, 393));
                    SetControlPropertyThreadSafe(CheckVersionLbl, "Text", "Update available");
                    SetControlPropertyThreadSafe(CheckVersionLbl, "LinkBehavior", System.Windows.Forms.LinkBehavior.SystemDefault);
                    SetControlPropertyThreadSafe(CheckVersionLbl, "LinkColor", System.Drawing.SystemColors.MenuHighlight);
                    MessageBox.Show("A new version of the launcher is available", "New Version " + getVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                else
                {
                    this.Loading.Image = global::AionLauncher.Properties.Resources.check;
                    SetControlPropertyThreadSafe(CheckVersionLbl, "Location", new System.Drawing.Point(560, 393));
                    SetControlPropertyThreadSafe(CheckVersionLbl, "Size", new System.Drawing.Size(40, 13));
                    SetControlPropertyThreadSafe(CheckVersionLbl, "Text", currentVersion);

                }
            }
            else
            {
                this.Loading.Image = null;
                SetControlPropertyThreadSafe(CheckVersionLbl, "Location", new System.Drawing.Point(535, 393));
                SetControlPropertyThreadSafe(CheckVersionLbl, "LinkBehavior", System.Windows.Forms.LinkBehavior.SystemDefault);
                SetControlPropertyThreadSafe(CheckVersionLbl, "Text", "Incompatible");
                this.CheckVersionLbl.LinkColor = System.Drawing.Color.Red;
            }
        }

        private void btnLaunch_Click(object sender, EventArgs e)
        {
          LaunchGame();
        }
        private void CheckVersionLbl_Click(object sender, EventArgs e)
        {
            if (CheckVersionLbl.Text == "Update available")
            {
                System.Diagnostics.Process.Start("https://code.google.com/p/aion-launcher-beta/");
            }
            if (CheckVersionLbl.Text == "Incompatible")
            {
                MessageBox.Show("The Version checker is incompatible with Windows XP, Please update your Os aged over 10 years", "Incompatible OS", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }      
        private void btnSettings_Click(object sender, EventArgs e)
        {
            if (AutoStartTimer.Enabled == true)
            {
                AutoStartTimer.Stop();
                btnLaunch.Text = "RUN\r\nGAME";
            }
            var SettingsWindow = new Settings();
            SettingsWindow.ShowDialog(this);
            Fcheck();
            ChangedBanner();
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
    } //end class
} //end namespace
