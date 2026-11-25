using System;
using System.Threading;
using System.Windows.Forms;
using UcScaleTool.UI;

namespace UcScaleTool
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnThreadException;
            Application.Run(new MainForm());
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("Onverwachte fout:\n\n" + e.Exception.Message, "UC Scale Tool",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
