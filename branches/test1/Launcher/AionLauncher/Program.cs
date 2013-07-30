using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AionLauncher
{
     class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
       static void Main()
        {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Launcher());
        } //end Main
    } //end class
} //end namespace
