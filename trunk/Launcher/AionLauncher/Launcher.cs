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

namespace AionLauncher
{
    public partial class Launcher : Form
    {
        public Launcher()
        {
            InitializeComponent();
        } //end constructor

        private void Launcher_Load(object sender, EventArgs e)
        {
            if (!File.Exists("version.ini"))
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
                    label1.Text = "Ready to update...";
                    btnLaunch.Text = "UPDATE";
                }
                else
                {
                    // Update Progress Bars etc.
                    progressBar1.Value = 100;
                    progressBar2.Value = 100;
                    label1.Text = "Your Have the latest version";
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid version.ini..", "Error: invalid version.ini", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }
                string news = "";
                if (Config.NEWSFEEDURL != "")
                {
                    try
                    {
                        WebClient client = new WebClient();
                        news = client.DownloadString(Config.NEWSFEEDURL);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("An error occurred while attempting to access the news feed.", "Error Accessing News", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    } //end try/catch
                } //end if

                if (news == "")
                {
                    news = Config.DEFAULTNEWS;
                } //end if

                lblNews.Text = news;

                Thread StatusThread = new Thread(new ThreadStart(this.CheckServerStatus));
                StatusThread.IsBackground = true;

                StatusThread.Start();
            } //end Launcher_Load
        //this pings the HOST and PORT specified in the Config class every 5 seconds as long as the program is running
        private void CheckServerStatus()
        {
            while (true)
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    try
                    {
                        if ((Config.HOST == string.Empty || Config.HOST == null))
                        {
                            MessageBox.Show("Please make sure your HOST is set in Config.cs", "Invalid HOST", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Application.Exit();
                        } //end if

                        socket.Connect(Config.HOST, Config.PORT);
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
            } //end if
        } //end SetControlPropertyThreadSafe

        //loads up the WEBSITE specified in the config class in the users default browser
        private void btnWebsite_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Config.WEBSITE);
        } //end btnWebsite_Click

        //this launches the actual game
        private void btnLaunch_Click(object sender, EventArgs e)
        {
            if (lstLang.SelectedItem == "EN") { lstLang.SelectedItem = "enu"; }
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
                     "/c" + "\"bin32\\aion.bin\" -ip:" + ValidateIP(Config.HOST) + " -ng -port:" + Config.PORT + " -cc:1 -noauthgg -lang:" + lstLang.SelectedItem + " -noweb -nowebshop -ingameshop");

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
            // Label change
            label1.Text = "Downloading bin32.zip...";

            // Download Files
            WebClient webClient = new WebClient();
            webClient.DownloadFile(Config.PATCH, System.Environment.CurrentDirectory + "/" + "bin32.zip");

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
            btnLaunch.Text = "       RUN         GAME";
            btnLaunch.UseWaitCursor = false;

            // Pause Timer
            timer5.Stop();

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

        public class NewIni
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
        //useless

        private void pctLogo_Click(object sender, EventArgs e)
        {

        }

        private void news_panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void labelLang_Click(object sender, EventArgs e)
        {

        }

        private void lstLang_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }//end ValidateIP
    } //end class
} //end namespace
