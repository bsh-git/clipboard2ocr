using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace clipboard2ocr
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        const string key = "ApiKey";

        public static string ReadApiKey()
		{

			try {
				var appSettings = ConfigurationManager.AppSettings;
				if (appSettings[key] != null && appSettings[key] != "")
					return appSettings[key];
			}
			catch (ConfigurationErrorsException e) {
				Console.WriteLine("Error reading app setings: {0}", e.Message);
			}
			return "no_api_key";
		}

		public static void UpdateApiKey(string newkey)
		{
			try {
				var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				var appSettings = ConfigurationManager.AppSettings;
				if (appSettings[key] == null)
					appSettings.Add(key, newkey);
				else
					appSettings[key] = newkey;
				configFile.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
			}
			catch (ConfigurationErrorsException e) {
				Console.WriteLine("Error updating app setings: {0}", e.Message);
			}
		}
    }
}
