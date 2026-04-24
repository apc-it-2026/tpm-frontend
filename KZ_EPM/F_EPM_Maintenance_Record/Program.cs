using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F_EPM_Maintenance_Record
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
         //   Client.APIURL = "http://localhost:60626/api/CommonCall";
            Client.APIURL = "http://10.3.0.24:8082/api/CommonCall";
            Client.UserToken = "080895fb-ebff-423f-945d-a1af07702be2";//
            Client.Language = "en";
            Application.Run(new Maintenance_RecordMainForm());
        }

        public static SJeMES_Framework.Class.ClientClass Client;
    }
}
