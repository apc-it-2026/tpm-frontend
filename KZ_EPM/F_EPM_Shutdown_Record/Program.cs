using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F_EPM_Shutdown_Record
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Client = new SJeMES_Framework.Class.ClientClass();
            Client.APIURL = "http://localhost:60626/api/CommonCall";
            Client.UserToken = "528b30ae-5e4a-48ef-943b-c1c680b03ab3";//
            Client.Language = "en";
            Application.Run(new Shutdown_RecordMainForm());
        }

        public static SJeMES_Framework.Class.ClientClass Client;
    }
}
