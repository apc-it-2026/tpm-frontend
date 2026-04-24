using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F_EPM_Maintennace_Plan
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
          //  Client.APIURL = "http://10.3.0.24:8082/api/CommonCall";
            Client.UserToken = "59c477d1-b48e-4eb9-9cf5-e5713ba6709c";//
            Client.Language = "en";
            Application.Run(new Maintenance_PlanMainForm());
        }

        public static SJeMES_Framework.Class.ClientClass Client;
    }
}
