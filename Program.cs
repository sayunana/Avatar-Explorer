using Avatar_Explorer.Classes;
using Avatar_Explorer.Forms;

namespace Avatar_Explorer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

            if (!(File.Exists("./Datas/CopyIcon.png") && File.Exists("./Datas/TrashIcon.png") &&
                File.Exists("./Datas/EditIcon.png")))
            {
                MessageBox.Show("Missing required image files. Please reinstall the application.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists("./Datas/NotoSansJP-Regular.ttf") || !File.Exists("./Datas/NotoSans-Regular.ttf") || !File.Exists("./Datas/NotoSansKR-Regular.ttf"))
            {
                MessageBox.Show("Missing required font files. Please reinstall the application.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new Main());
        }
    }
}