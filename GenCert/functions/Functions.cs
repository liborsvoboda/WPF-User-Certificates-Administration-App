using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.Script.Serialization;
using System.Collections.Specialized;
using System.Diagnostics;
using Dragablz;

namespace CertAdmin.functions
{
    class Functions
    {

        public static void fn_LoadSettings()
        {
            try
            {

                string json = File.ReadAllText(Path.Combine(MainWindow.setting_folder, MainWindow.settingFile), fn_file_detect_encoding(Path.Combine(MainWindow.setting_folder, MainWindow.settingFile)));
                ListDictionary SettingData = new JavaScriptSerializer().Deserialize<ListDictionary>(json);

                MainWindow.Setting.WSDLServer1 = SettingData["WSDLServer1"].ToString();
                MainWindow.Setting.CertPassword = SettingData["CertPassword"].ToString();
                MainWindow.Setting.MySQLDbName = SettingData["MySQLDbName"].ToString();
                MainWindow.Setting.MySQLServer = SettingData["MySQLServer"].ToString();
                MainWindow.Setting.MySQLPort = SettingData["MySQLPort"].ToString();
                MainWindow.Setting.MySQLLoginName = SettingData["MySQLLoginName"].ToString();
                MainWindow.Setting.MySQLLoginPassword = SettingData["MySQLLoginPassword"].ToString();
                MainWindow.Setting.WriteToLog = SettingData["WriteToLog"].ToString();
                MainWindow.Setting.CertificateSavingPath = SettingData["CertificateSavingPath"].ToString();
                MainWindow.Setting.openSslVersion = SettingData["openSslVersion"].ToString();


                MainWindow.dbConnectioString = "Server=" + MainWindow.Setting.MySQLServer + ";Port=" + MainWindow.Setting.MySQLPort + ";Database=" + MainWindow.Setting.MySQLDbName + ";Uid=" + MainWindow.Setting.MySQLLoginName + ";Pwd =" + MainWindow.Setting.MySQLLoginPassword + ";Character Set=utf8; ";

                if (!fn_check_directory(MainWindow.Setting.CertificateSavingPath))
                {
                    fn_create_directory(MainWindow.Setting.CertificateSavingPath);
                }
            }
            catch (Exception ex)
            {
                fn_WriteToFile(Path.Combine(MainWindow.setting_folder, MainWindow.logFile), "configuration load error: " + ex.Message);
            }
        }

        public static bool fn_create_file(string file)
        {
            if (!System.IO.File.Exists(file))
                System.IO.File.Create(file).Close();

            if (fn_check_file(file))
                return true;
            else
                return false;

        }

        public static System.Text.Encoding fn_file_detect_encoding(string FileName)
        {
            string enc = "";
            if (System.IO.File.Exists(FileName))
            {
                System.IO.FileStream filein = new System.IO.FileStream(FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                if ((filein.CanSeek))
                {
                    byte[] bom = new byte[5];
                    filein.Read(bom, 0, 4);
                    // EF BB BF       = utf-8
                    // FF FE          = ucs-2le, ucs-4le, and ucs-16le
                    // FE FF          = utf-16 and ucs-2
                    // 00 00 FE FF    = ucs-4
                    if ((((bom[0] == 0xEF) && (bom[1] == 0xBB) && (bom[2] == 0xBF)) || ((bom[0] == 0xFF) && (bom[1] == 0xFE)) || ((bom[0] == 0xFE) && (bom[1] == 0xFF)) || ((bom[0] == 0x0) && (bom[1] == 0x0) && (bom[2] == 0xFE) && (bom[3] == 0xFF))))
                        enc = "Unicode";
                    else
                        enc = "ASCII";
                    // Position the file cursor back to the start of the file
                    filein.Seek(0, System.IO.SeekOrigin.Begin);
                }
                filein.Close();
            }
            if (enc == "Unicode")
                return System.Text.Encoding.UTF8;
            else
                return System.Text.Encoding.Default;
        }

        public static void fn_delete_directory(string directory)
        {
            if (System.IO.Directory.Exists(directory))
                System.IO.Directory.Delete(directory, true);
        }

        public static bool fn_check_directory(string directory)
        {
            return System.IO.Directory.Exists(directory);
        }

        public static bool fn_check_file(string file)
        {
            return System.IO.File.Exists(file);
        }

        public static void fn_create_directory(string directory)
        {
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);
        }

        public static bool fn_delete_file(string file)
        {
            System.IO.File.Delete(file);

            if (!fn_check_file(file))
                return true;
            else
                return false;
        }

        public static void fn_WriteToFile(string file, string Message)
        {
            fn_create_file(file);

            System.IO.StreamWriter objWriter = new System.IO.StreamWriter(file, true);
            objWriter.WriteLine(Message);
            objWriter.Close();
        }

        public static void SendMailWithMailTo(string address, string subject, string body, string attach)
        {
            string mailto =
                string.Format(
                    "mailto:{0}?Subject={1}&Body={2}&Attach={3}",
                    address, subject, body, attach);
            System.Diagnostics.Process.Start(mailto);
        }

        public static bool fn_generatePfx(string filename)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = (MainWindow.Setting.openSslVersion == "64") ? Path.Combine(MainWindow.setting_folder, "Data", "64", "openssl.exe") : Path.Combine(MainWindow.setting_folder, "Data", "32", "openssl.exe");
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.Arguments = " pkcs12 -export -out " + Path.Combine(MainWindow.setting_folder, "Data", "Temp", filename + ".pfx") + " -inkey " + Path.Combine(MainWindow.setting_folder, "Data", "Temp", filename + ".key") + " -in " + Path.Combine(MainWindow.setting_folder, "Data", "Temp", filename + ".cer") + " -passout pass:";

            Process proc = Process.Start(psi);
            proc.WaitForExit();
            string errorOutput = proc.StandardError.ReadToEnd();
            string standardOutput = proc.StandardOutput.ReadToEnd();
            if (proc.ExitCode != 0)
            {
                Functions.fn_WriteToFile(Path.Combine(MainWindow.setting_folder, MainWindow.logFile), "GeneratePFX by OpenSSh Error: " + proc.ExitCode.ToString() + " " + (!string.IsNullOrEmpty(errorOutput) ? " " + errorOutput : "") + " " + (!string.IsNullOrEmpty(standardOutput) ? " " + standardOutput : ""));
                return false;
            }
            else { return true; }
        }

        public static void resetSelection()
        {
            MainWindow.dataGridSelectedId = 0;
            MainWindow.DataGridSelected = false;
            MainWindow.TMSelected = false;
            MainWindow.PrivKeySelected = false;
            MainWindow.PFXSelected = false;
        }
   
    }
}
