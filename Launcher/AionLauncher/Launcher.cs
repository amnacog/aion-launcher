using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Drawing;
using System.Text;
using System.Resources;
using System.Drawing.Text;
using System.Windows.Forms;
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
            //Load Languages
            LoadLang();
            
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
        public static string lang { get; set; }
        public static string setupurl { get; set; }
        public CultureInfo ci { get; set; }

        public ResourceManager resman = new ResourceManager("AionLauncher.Launcher", Assembly.GetExecutingAssembly());

        //call lang
        private void LoadLang()
        {
            string cl = lang;
            if (System.IO.File.Exists("launcher.ini"))
            {
                IConfigSource getlang = new IniConfigSource("launcher.ini");
                lang = getlang.Configs["Misc"].Get("LaunchLanguage");
                lang = lang.Replace("fr", "fr-FR");
                lang = lang.Replace("de", "de-DE");
                lang = lang.Replace("en", "en-US");
            }
            if (lang == null){lang = "en-US";}

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            this.ci = new CultureInfo(lang);
            //load controllers
            this.InitializeComponent();
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
            //delete current setup
            string setupfile = "AionLauncher-" + Application.ProductVersion + " Setup.exe";
            if (System.IO.File.Exists(setupfile))
            {
                File.Delete(setupfile);
            }
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
            string LAUNLANG = miscSection.Get("LaunchLanguage");

            //set the lang selectbox
            LANG = LANG.Replace("fr", "FR");
            LANG = LANG.Replace("de", "DE");
            LANG = LANG.Replace("en", "EN");
            LANG = LANG.Replace("es", "ES");
            LANG = LANG.Replace("ru", "RU");

            }
            catch (Exception)
            {
                MessageBox.Show(resman.GetString("MsgLaunchError",ci), resman.GetString("MsgLaunchError1",ci), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            if (!System.IO.File.Exists("version.ini"))
            {
                var NewIni = new NewIni();
                MessageBox.Show(resman.GetString("MsgVersionCreate", ci), resman.GetString("MsgVersionCreate1,ci"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show(resman.GetString("MsgVersionClient", ci), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    //Update
                    btnLaunch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(149)))), ((int)(((byte)(0)))));
                    btnLaunch.GlowColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(122)))), ((int)(((byte)(0)))));
                    label1.Text = resman.GetString("label1.Text.ready", ci);
                    btnLaunch.Text = resman.GetString("btnLaunch.Text.up", ci);
                    } else {
                        this.Opacity = 0f;
                        Application.DoEvents();

                    // Update Progress Bars etc.
                    progressBar1.Value = 100;
                    progressBar2.Value = 100;
                    label1.Text = resman.GetString("label1.Text.latest", ci);

                    Thread AutoStartThread = new Thread(new ThreadStart(this.AutoStart));
                    AutoStartThread.Start();
                }
            }
            catch (Exception)
            {
                MessageBox.Show(resman.GetString("MsgVersionError", ci), resman.GetString("MsgVersionError1", ci), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        {            IniConfigSource launcher = new IniConfigSource("launcher.ini");
            IConfig miscSection = launcher.Configs["Misc"];
            bool AUTOL = miscSection.GetBoolean("AutoStart");
            if (AUTOL == true)
            {
                AutoStartTimer.Start();
                int number = 5;
                while (btnLaunch.Text != resman.GetString("btnLaunchTimeout", ci))
                {
                    if (AutoStartTimer.Enabled == false) { break; }
                    SetControlPropertyThreadSafe(btnLaunch, "Text", resman.GetString("btnLaunch.Text", ci) + number--);
                    Thread.Sleep(1000);
                }
                if (btnLaunch.Text == resman.GetString("btnLaunchTimeout", ci))
                {
                    SetControlPropertyThreadSafe(btnLaunch, "Text", resman.GetString("btnLaunch.Text", ci));
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
                            MessageBox.Show(resman.GetString("MsgInvalidHost", ci), resman.GetString("MsgInvalidHost1", ci), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Application.Exit();
                        } //end if

                        socket.Connect(HOST, PORT);
                        socket.Close();

                        SetControlPropertyThreadSafe(lblServerStatus, "Text", resman.GetString("lblServerStatusOn", ci));
                        SetControlPropertyThreadSafe(lblServerStatus, "ForeColor", Color.Green);

                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                        {
                            SetControlPropertyThreadSafe(lblServerStatus, "Text", resman.GetString("lblServerStatusOff", ci));
                            SetControlPropertyThreadSafe(lblServerStatus, "ForeColor", Color.Red);
                        } //end if
                    } //end try/catch
                } //end using
                //only check every 5 seconds while the program is up.
                Thread.Sleep(5000);
            } //end while
        }

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

            if (btnLaunch.Text == resman.GetString("btnLaunch.Text.up", ci))
            {
                timer1.Start();
                btnLaunch.Text = resman.GetString("btnLaunch.Text.ccl", ci);
            }
            else if (btnLaunch.Text == resman.GetString("btnLaunch.Text.ccl", ci)){cclupdate();}
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
                        MessageBox.Show(resman.GetString("MsgErrorLaunch.Text.up", ci) + ex.Message);
                    } //end try/catch
                }
                else
                {
                    MessageBox.Show(resman.GetString("MsgErrorAion.bin", ci), resman.GetString("MsgErrorAion.bin1", ci), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                //end if
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
            
                if (NEWSFEEDURL == "" | NEWSFEEDURL == null | NEWSFEEDURL == "http://")
                {
                    string news = "";
                    this.news_panel.BackgroundImage = global::AionLauncher.Properties.Resources.u3jsplashblank;
                    this.lblNews.Text = news;
                    this.BannerBrowser.Visible = false; 

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
                this.BannerBrowser.Visible = true;
                this.BannerBrowser.Refresh();
            }
            else if (BannerCode == 0) {

            }
            else
            {
                string news = "";
                this.news_panel.BackgroundImage = global::AionLauncher.Properties.Resources.u3jsplashblank;
                this.lblNews.Text = news;
                this.BannerBrowser.Visible = false;
            }
        }
        private void Fcheck()
        {
            if (ForceCheck == true)
            {
                //Update
                btnLaunch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(149)))), ((int)(((byte)(0)))));
                btnLaunch.GlowColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(122)))), ((int)(((byte)(0)))));
                label1.Text = resman.GetString("label1.Text.ready", ci);
                btnLaunch.Text = resman.GetString("btnLaunch.Text.up", ci);
            }
            else
            {   
                //normal state
                btnLaunch.BackColor = System.Drawing.Color.MediumBlue;
                btnLaunch.GlowColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(61)))), ((int)(((byte)(245)))));
                label1.Text = resman.GetString("label1.Text", ci);
                btnLaunch.Text = resman.GetString("btnLaunch.Text", ci);
            }
        }

        //Write New launcher.ini conf
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
                + "IP = xxx.xxx.xxx.xxx" + NL
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
                + "BannerUrl = http://cmsstatic.aionfreetoplay.com/launcher-pts_en.html" + NL
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
        private void cclupdate()
        {
            if (btnLaunch.Text == resman.GetString("btnLaunch.Text.ccl", ci))
            {
                btnLaunch.Text = resman.GetString("btnLaunch.Text", ci);
                btnLaunch.BackColor = System.Drawing.Color.MediumBlue;
                btnLaunch.GlowColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(61)))), ((int)(((byte)(245)))));
                label1.Text = "Update Canceled..";
                Launcher.ForceCheck = false;
                if (System.IO.File.Exists("bin32.zip"))
                {
                    File.Delete("bin32.zip");
                }

            }
            timer1.Stop();
            timer2.Stop();
            timer3.Stop();
            timer4.Stop();
            timer5.Stop();
            progressBar1.Value = 0;
            progressBar2.Value = 0;
            label1.Text = "Ready to update..";
        }
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
                HttpStatusCode status = response.StatusCode;
                if (status == HttpStatusCode.OK | status == HttpStatusCode.Accepted)
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
                else
                {
                    label1.Text = "Connection Error";
                    timer1.Stop();
                    MessageBox.Show("Unable to connect to the game updater, please try again or contact the support", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    label1.Text = "Ready to Update..";
                    cclupdate();
                }
            }
            catch(WebException wc)
            {
                using (WebResponse response = wc.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    if (httpResponse.StatusCode != HttpStatusCode.Forbidden)
                    {
                        label1.Text = "Connection Error";
                        timer1.Stop();
                        MessageBox.Show("Unable to connect to the game updater, please try again or contact the support", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        label1.Text = "Ready to Update..";
                        cclupdate();
                    }
                    else
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
            label1.Text = "Deleting Old Package...";

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
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    //Catch Or download
                    webClient.DownloadFile(PATCH, System.Environment.CurrentDirectory + "/" + "bin32.zip");
                }
                // Progress bar update
                progressBar1.Value = 0;
                progressBar1.Value = 100;
                progressBar2.Value = 85;

                // Pause Timer
                timer4.Stop();
                timer5.Start();
            }
            catch (WebException ea)
            {
                    var response = ea.Response as HttpWebResponse;
                    label1.Text = "Connection Error";
                    timer4.Stop();
                    if (ea.Status == WebExceptionStatus.ProtocolError)
                    {
                        MessageBox.Show("Unable to download the file (" + (int)response.StatusCode +" "+ ((HttpWebResponse)ea.Response).StatusDescription + ")", "Connection Error: (" + (int)response.StatusCode + ")", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    label1.Text = "Ready to Update..";
                    cclupdate();
            }

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
            btnLaunch.BackColor = System.Drawing.Color.MediumBlue;
            btnLaunch.GlowColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(61)))), ((int)(((byte)(245)))));
            btnLaunch.Text = resman.GetString("btnLaunch.Text", ci);
            Launcher.ForceCheck = false;

            // Pause Timer
            timer5.Stop();

        }

        //Timeout Launcher
        private void AutoStartTimer_Tick(object sender, EventArgs e)
        {
            AutoStartTimer.Stop();
            LaunchGame();
        }

        //CheckVersion (PLEASE DO NOT MODIFY THESE LINES) --------------------------------///

        //get informations
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
        public class JsonParser
        {
            public string version { get; set; }
            public string changelog { get; set; }
            public string gameclient { get; set; }
            public string download { get; set; }
        }
        private void CheckVersions()
        {
            if (Environment.OSVersion.Version >= new Version(6, 0))
            {
                try
                {
                    WebClient getJson = new WebClient();
                    string json = getJson.DownloadString("http://aion-launcher-beta.googlecode.com/svn/trunk/Launcher/Parser/index.php");
                    JsonParser LauncherJson = JsonHelper.JsonDeserialize<JsonParser>(json);

                    string currentVersion = Application.ProductVersion;
                    string getVersion = LauncherJson.version;
                    string getDownload = LauncherJson.download;
                    string getgameclient = LauncherJson.gameclient;
                    string getChangelog = LauncherJson.changelog;
                    Launcher.setupurl = getDownload;


                    if (currentVersion != getVersion)
                    {

                        this.Loading.Image = global::AionLauncher.Properties.Resources.update;
                        if (lang == "fr-FR")
                        {SetControlPropertyThreadSafe(CheckVersionLbl, "Location", new System.Drawing.Point(460, 393));}
                        else if (lang == "de-DE") { SetControlPropertyThreadSafe(CheckVersionLbl, "Location", new System.Drawing.Point(480, 393)); }
                        else { SetControlPropertyThreadSafe(CheckVersionLbl, "Location", new System.Drawing.Point(510, 393)); }
                        SetControlPropertyThreadSafe(CheckVersionLbl, "Text", resman.GetString("CheckVersionNewlbl", ci));
                        SetControlPropertyThreadSafe(CheckVersionLbl, "LinkBehavior", System.Windows.Forms.LinkBehavior.SystemDefault);
                        SetControlPropertyThreadSafe(CheckVersionLbl, "LinkColor", System.Drawing.SystemColors.MenuHighlight);
                        MessageBox.Show(resman.GetString("MsgCheckVrsNew", ci) + "\r\n\r\n" + getChangelog, resman.GetString("MsgCheckVrsNew1", ci) + getVersion, MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                    else
                    {
                        this.Loading.Image = global::AionLauncher.Properties.Resources.check;
                        SetControlPropertyThreadSafe(CheckVersionLbl, "Location", new System.Drawing.Point(575, 393));
                        SetControlPropertyThreadSafe(CheckVersionLbl, "Size", new System.Drawing.Size(40, 13));
                        SetControlPropertyThreadSafe(CheckVersionLbl, "Text", currentVersion);

                    }
                }
                catch(Exception)
                {
                    this.Loading.Image = null;
                    SetControlPropertyThreadSafe(CheckVersionLbl, "Location", new System.Drawing.Point(535, 393));
                    SetControlPropertyThreadSafe(CheckVersionLbl, "LinkBehavior", System.Windows.Forms.LinkBehavior.SystemDefault);
                    SetControlPropertyThreadSafe(CheckVersionLbl, "Text", resman.GetString("CheckVrersionErrorlbl", ci));
                    this.CheckVersionLbl.LinkColor = System.Drawing.Color.Red;
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
        //--------------------------------End Updater----------------------------------///

        //call events
        private void drag_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void btnLaunch_Click(object sender, EventArgs e)
        {
            if (AutoStartTimer.Enabled == true)
            {
                AutoStartTimer.Stop();
                btnLaunch.Text = resman.GetString("btnLaunch.Text", ci);
            }  
            LaunchGame();
        }
        private void CheckVersionLbl_Click(object sender, EventArgs e)
        {
            WebClient getJson = new WebClient();
            string json = getJson.DownloadString("http://aion-launcher-beta.googlecode.com/svn/trunk/Launcher/Parser/index.php");
            JsonParser LauncherJson = JsonHelper.JsonDeserialize<JsonParser>(json);

            if (CheckVersionLbl.Text == resman.GetString("CheckVersionNewlbl", ci))
            {
                DialogResult update = MessageBox.Show(resman.GetString("MsgCheckVrsNew", ci) + "\r\n\r\n" + LauncherJson.changelog + "\r\n\r\n" + resman.GetString("MsgCheckVrsNewDl", ci), resman.GetString("MsgCheckVrsNew1", ci) + LauncherJson.version, MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (update == DialogResult.Yes)
                {
                    this.Hide();
                    var UpdaterWindow = new Autoupdate();
                    UpdaterWindow.ShowDialog(this);
                }
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
                btnLaunch.Text = resman.GetString("btnLaunch.Text", ci);
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
