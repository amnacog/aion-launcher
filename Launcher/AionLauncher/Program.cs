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
            //check to see if the DLL dependency is missing -- HtmlRendered.dll
            if (!System.IO.File.Exists("HtmlRenderer.dll"))
            {
                MessageBox.Show("You must copy HtmlRenderer.dll into your aion folder.", "Error loading DLL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            } //end if

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Launcher());
        } //end Main
    } //end class
} //end namespace
