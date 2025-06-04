using System;
using System.Collections.Generic; // Dictionary için gerekli
using System.Windows.Forms;
using MongoDB.Bson.Serialization;
namespace StokTakipSistemi;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
            
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}