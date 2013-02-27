using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AionLauncher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //check to see if one of the DLL dependency is missing 
                if (!System.IO.File.Exists("Ionic.Zip.dll"))
                {
                    MessageBox.Show("You must copy Ionic.Zip.dll into your aion folder.", "Error loading DLL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!System.IO.File.Exists("Nini.dll"))
                {
                    MessageBox.Show("You must copy Ionic.Zip.dll into your aion folder.", "Error loading DLL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Launcher());
        } //end Main
    } //end class
} //end namespace
