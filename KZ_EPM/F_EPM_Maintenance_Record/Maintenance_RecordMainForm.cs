using AutocompleteMenuNS;
using F_EPM_Maintenance_Record;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using SJeMES_Control_Library;
using SJeMES_Framework.WebAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace F_EPM_Maintenance_Record
{
    public partial class Maintenance_RecordMainForm : MaterialForm
    {

        AutoCompleteStringCollection autoComplete_factory = new AutoCompleteStringCollection();
        AutoCompleteStringCollection autoComplete_device = new AutoCompleteStringCollection();

        private List<String> postionInfo = new List<string>();
        private List<String> deviceInfo = new List<string>();
        private DataTable _maintenanceData;
        private int _currentPage = 1;
        private int _pageSize = 3000;   // or 1000
        private int _totalPages = 0;  // optional if backend returns count


        public Maintenance_RecordMainForm()
        {
            InitializeComponent();
            SJeMES_Framework.Common.UIHelper.UIUpdate(this.Name, this, Program.Client, "", Program.Client.Language);
            label9.Visible = false;
            btnPrev.Visible = false;
            btnNext.Visible = false;
            lblPageInfo.Visible = false;
            //CenterLoadingLabel();
        }

        private void Equipment_Maintenance_Record_Form_Load(object sender, EventArgs e)
        {
            LoadDept();
            LoadOrg();
            DefaultComBoxSelect();
        }

        private void DefaultComBoxSelect()
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
            comboBox7.SelectedIndex = 0;
            comboBox8.SelectedIndex = 0;
        }

        private void LoadDept()
        {
            List<AutocompleteItem> items3 = new List<AutocompleteItem>();
            Dictionary<string, object> parm = new Dictionary<string, object>();
            string ret = WebAPIHelper.Post(Program.Client.APIURL, "KZ_SFCAPI_WorkOrder", "KZ_SFCAPI_WorkOrder.Controllers.GeneralServer", "GetAllDepts", Program.Client.UserToken, JsonConvert.SerializeObject(parm));
            if (Convert.ToBoolean(JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["IsSuccess"]))
            {
                items3.Add(new MulticolumnAutocompleteItem(new[] { "" }, "All"));
                string json = JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["RetData"].ToString();
                DataTable dtdept = JsonConvert.DeserializeObject<DataTable>(json);
                if (dtdept != null && dtdept.Rows.Count > 0)
                {
                    for (int i = 0; i < dtdept.Rows.Count; i++)
                    {
                        items3.Add(new MulticolumnAutocompleteItem(new[] { dtdept.Rows[i]["department_code"].ToString() }, dtdept.Rows[i]["department_code"].ToString()));
                    }
                }

                comboBox3.DataSource = items3;
            }
            else
            {
                MessageHelper.ShowErr(this, JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["ErrMsg"].ToString());
            }
        }

        private void LoadOrg()
        {
            //工厂
            List<AutocompleteItem> items3 = new List<AutocompleteItem>();
            List<string> stringList = new List<string>();
            AutoCompleteStringCollection autoComplete1 = new AutoCompleteStringCollection();

            string ret = SJeMES_Framework.WebAPI.WebAPIHelper.Post(Program.Client.APIURL, "KZ_WMSAPI", "KZ_WMSAPI.Controllers.F_WMS_Miscellaneous_Server", "LoadOrg", Program.Client.UserToken, Newtonsoft.Json.JsonConvert.SerializeObject("noParam"));
            if (Convert.ToBoolean(Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["IsSuccess"]))
            {
                string Data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["RetData"].ToString();
                DataTable dtJson = SJeMES_Framework.Common.JsonHelper.GetDataTableByJson(Data);
                //dtJson.Rows.Add("全部","");

                items3.Add(new MulticolumnAutocompleteItem(new[] { "", "" }, "All"));
                stringList.Add("All");

                for (int i = 0; i < dtJson.Rows.Count; i++)
                {
                    stringList.Add(dtJson.Rows[i]["org_name"].ToString() + "|" + dtJson.Rows[i]["org_code"].ToString());
                    items3.Add(new MulticolumnAutocompleteItem(new[] { dtJson.Rows[i]["org_code"].ToString(), dtJson.Rows[i]["org_name"].ToString() }, dtJson.Rows[i]["org_name"].ToString() + "|" + dtJson.Rows[i]["org_code"].ToString()));
                }
                comboBox1.DataSource = items3;
                autoComplete1.AddRange(stringList.ToArray());
                autoComplete_factory.AddRange(stringList.ToArray());
                comboBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;
                comboBox1.AutoCompleteCustomSource = autoComplete1;
            }
            else
            {
                MessageHelper.ShowErr(this, JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["ErrMsg"].ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            QueryMaintenanceRecord();
        }
        //private void CenterLoadingLabel()
        //{
        //    label9.Left = dataGridView1.Left +
        //                  (dataGridView1.Width - label9.Width) / 2;

        //    label9.Top = dataGridView1.Top +
        //                 (dataGridView1.Height - label9.Height) / 2;
        //}

        private void SetRowColor(int rowIndex, Color color)
        {
            dataGridView1.Rows[rowIndex].Cells["jc_result"].Style.ForeColor = color;
            dataGridView1.Rows[rowIndex].Cells["by_status"].Style.ForeColor = color;
            dataGridView1.Rows[rowIndex].Cells["jc_description"].Style.ForeColor = color;
        }

        private void BindMaintenancePage()
        {
            dataGridView1.Rows.Clear();

            if (_maintenanceData == null || _maintenanceData.Rows.Count == 0)
                return;

            int startIndex = (_currentPage - 1) * _pageSize;
            int endIndex = Math.Min(startIndex + _pageSize, _maintenanceData.Rows.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                DataRow r = _maintenanceData.Rows[i];
                int rowIndex = dataGridView1.Rows.Add();

                dataGridView1.Rows[rowIndex].Cells["by_status"].Value =
                    r["by_status"].ToString() == "0" ? "Unmaintained" : "Maintained";

                int jcResult = Convert.ToInt32(r["jc_result"]);
                if (jcResult == 0) dataGridView1.Rows[rowIndex].Cells["jc_result"].Value = "Not_Audited";
                else if (jcResult == 1) { dataGridView1.Rows[rowIndex].Cells["jc_result"].Value = "OK"; SetRowColor(rowIndex, Color.Green); }
                else if (jcResult == 2) { dataGridView1.Rows[rowIndex].Cells["jc_result"].Value = "NG"; SetRowColor(rowIndex, Color.Red); }

                dataGridView1.Rows[rowIndex].Cells["jc_description"].Value = r["jc_description"];
                dataGridView1.Rows[rowIndex].Cells["org_name"].Value = r["org_name"];
                dataGridView1.Rows[rowIndex].Cells["department_name"].Value = r["address_name"];
                dataGridView1.Rows[rowIndex].Cells["device_name"].Value = r["device_name"];
                dataGridView1.Rows[rowIndex].Cells["snid"].Value = r["snid"];
                dataGridView1.Rows[rowIndex].Cells["type"].Value = r["type"];
                dataGridView1.Rows[rowIndex].Cells["body_part"].Value = r["body_part"];
                dataGridView1.Rows[rowIndex].Cells["item"].Value = r["item"];
                dataGridView1.Rows[rowIndex].Cells["level_name"].Value = r["level_name"];
                dataGridView1.Rows[rowIndex].Cells["frequency"].Value = r["frequency"];
                dataGridView1.Rows[rowIndex].Cells["plan_finishdate"].Value = Convert.ToDateTime(r["plan_finishdate"]).ToString("yyyy/MM/dd");
                dataGridView1.Rows[rowIndex].Cells["by_date"].Value = r["by_date"];
                dataGridView1.Rows[rowIndex].Cells["by_user"].Value = r["by_user"];
                dataGridView1.Rows[rowIndex].Cells["jc_date"].Value = r["jc_date"];
                dataGridView1.Rows[rowIndex].Cells["jc_user"].Value = r["jc_user"];
                dataGridView1.Rows[rowIndex].Cells["plan_begindate"].Value = Convert.ToDateTime(r["plan_begindate"]).ToString("yyyy/MM/dd");
                dataGridView1.Rows[rowIndex].Cells["plan_enddate"].Value = Convert.ToDateTime(r["plan_enddate"]).ToString("yyyy/MM/dd");
                dataGridView1.Rows[rowIndex].Cells["plan_id"].Value = r["plan_id"];
                dataGridView1.Rows[rowIndex].Cells["plan_name"].Value = r["plan_name"];
            }

            lblPageInfo.Text = $"Page {_currentPage} / {_totalPages}";
            btnPrev.Enabled = _currentPage > 1;
            btnNext.Enabled = _currentPage < _totalPages;
        }



        private void QueryMaintenanceRecordAll()
        {

            ///
            string factoryName = comboBox1.Text == "All" ? "" : comboBox1.Text.Split('|')[0].Trim();
            string deviceName = comboBox2.Text == "All" ? "" : comboBox2.Text;
            string departmentName = comboBox3.Text == "All" ? "" : comboBox3.Text;
            string maintainStatusText = comboBox4.Text;
            string filterDateType = comboBox5.Text;
            string startDate = Convert.ToDateTime(dateTimePicker2.Text).ToString("yyyy/MM/dd");
            string endDate = Convert.ToDateTime(dateTimePicker1.Text).ToString("yyyy/MM/dd");
            string level = comboBox7.Text == "All" ? "" : comboBox7.Text;
            string checkResult = comboBox8.Text;

            Dictionary<string, object> p = new Dictionary<string, object>();
            p["factory_name"] = factoryName;
            p["device_name"] = deviceName;
            p["department_name"] = departmentName;

            p["maintain_status"] =
                maintainStatusText == "All" ? -1 :
                maintainStatusText == "Unmaintained" ? 0 : 1;

            p["filter_date_type"] =
                filterDateType == "Planned_Maintenance_Date" ? 0 :
                filterDateType == "Actual_Maintenance_Date" ? 1 : 2;

            p["start_date"] = startDate;
            p["end_date"] = endDate;
            p["level"] = level;

            p["check_result"] =
                checkResult == "All" ? -1 :
                checkResult == "Not_Audited" ? 0 :
                checkResult == "OK" ? 1 : 2;

            // ✅ pagination params
            p["page_no"] = _currentPage;
            p["page_size"] = _pageSize;

            Cursor.Current = Cursors.WaitCursor;
            label9.ForeColor = Color.Red;
            label9.Text = "Please wait data is loading..";
            label9.Visible = true;

            string ret = WebAPIHelper.Post(
                Program.Client.APIURL,
                "KZ_EPMAPI",
                "KZ_EPMAPI.Controllers.MaintenanceRecordService",
                "QueryMaintenanceRecordAll",
                Program.Client.UserToken,
                JsonConvert.SerializeObject(p)
            );



            if (!Convert.ToBoolean(JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["IsSuccess"]))
            {
                MessageHelper.ShowErr(this,
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["ErrMsg"].ToString());
                return;
            }

            string json = JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["RetData"].ToString();
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(json);

            if (dt != null && dt.Rows.Count > 0)
            {

                // Rename columns AFTER deserialization
                if (dt.Columns.Contains("BY_STATUS"))
                {
                    // Make column string-capable
                    dt.Columns["BY_STATUS"].DataType = typeof(string);

                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["BY_STATUS"] != DBNull.Value)
                        {
                            row["BY_STATUS"] =
                                row["BY_STATUS"].ToString() == "0" ? "Unmaintained" :
                                row["BY_STATUS"].ToString() == "1" ? "Maintained" :
                                row["BY_STATUS"].ToString();
                        }
                    }

                    // 🔥 Rename AFTER data conversion
                    dt.Columns["BY_STATUS"].ColumnName = "Maintenance status";
                }


                if (dt.Columns.Contains("JC_RESULT"))
                {
                    // Allow text values
                    dt.Columns["JC_RESULT"].DataType = typeof(string);

                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["JC_RESULT"] != DBNull.Value)
                        {
                            row["JC_RESULT"] =
                                row["JC_RESULT"].ToString() == "0" ? "Not Audited" :
                                row["JC_RESULT"].ToString() == "1" ? "OK" :
                                row["JC_RESULT"].ToString() == "2" ? "NG" :
                                row["JC_RESULT"].ToString();
                        }
                    }

                    // 🔥 Rename AFTER converting values
                    dt.Columns["JC_RESULT"].ColumnName = "Audit results";
                }

                dt.Columns["JC_DESCRIPTION"].ColumnName = "Audit description";
                dt.Columns["ORG_NAME"].ColumnName = "Factory";
                dt.Columns["ADDRESS_NAME"].ColumnName = "Location";
                dt.Columns["DEVICE_NAME"].ColumnName = "Device name";

                dt.Columns["SNID"].ColumnName = "SN code";
                dt.Columns["TYPE"].ColumnName = "Device model";
                dt.Columns["BODY_PART"].ColumnName = "Parts";
                dt.Columns["ITEM"].ColumnName = "Preservation matters";
                dt.Columns["LEVEL_NAME"].ColumnName = "Maintenance level";
                dt.Columns["FREQUENCY"].ColumnName = "Frequency";

                dt.Columns["PLAN_FINISHDATE"].ColumnName = "Plan completion time";
                dt.Columns["BY_DATE"].ColumnName = "Actual maintenance time";
                dt.Columns["BY_USER"].ColumnName = "Maintenance staff";
                dt.Columns["JC_DATE"].ColumnName = "Actual audit time";
                dt.Columns["JC_USER"].ColumnName = "Inspector";

                dt.Columns["PLAN_BEGINDATE"].ColumnName = "Program start time";
                dt.Columns["PLAN_ENDDATE"].ColumnName = "Program end time";
                dt.Columns["PLAN_ID"].ColumnName = "Maintenance plan number";
                dt.Columns["PLAN_NAME"].ColumnName = "Maintenance plan name";



                NewExportExcels.ExportExcels.Export(
                    "Maintenance History (details)",
                    dt
                );
                Cursor.Current = Cursors.Default;
                label9.Visible = false;

            }
            else
            {
                MessageHelper.ShowErr(this, "No data to export.");
            }


            //dataGridView1.Rows.Clear();

            //if (dt.Rows.Count == 0)
            //{
            //    MessageBox.Show("No Data!");
            //    return;
            //}

            //// ✅ bind ONLY current page data
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    int row = dataGridView1.Rows.Add();
            //    DataRow r = dt.Rows[i];

            //    dataGridView1.Rows[row].Cells["by_status"].Value =
            //        r["by_status"].ToString() == "0" ? "Unmaintained" : "Maintained";

            //    int jc = Convert.ToInt32(r["jc_result"]);
            //    dataGridView1.Rows[row].Cells["jc_result"].Value =
            //        jc == 0 ? "Not_Audited" : jc == 1 ? "OK" : "NG";

            //    if (jc == 1) SetRowColor(row, Color.Green);
            //    if (jc == 2) SetRowColor(row, Color.Red);

            //    dataGridView1.Rows[row].Cells["jc_description"].Value = r["jc_description"];
            //    dataGridView1.Rows[row].Cells["org_name"].Value = r["org_name"];
            //    dataGridView1.Rows[row].Cells["department_name"].Value = r["address_name"];
            //    dataGridView1.Rows[row].Cells["device_name"].Value = r["device_name"];
            //    dataGridView1.Rows[row].Cells["snid"].Value = r["snid"];
            //    dataGridView1.Rows[row].Cells["type"].Value = r["type"];
            //    dataGridView1.Rows[row].Cells["body_part"].Value = r["body_part"];
            //    dataGridView1.Rows[row].Cells["item"].Value = r["item"];
            //    dataGridView1.Rows[row].Cells["level_name"].Value = r["level_name"];
            //    dataGridView1.Rows[row].Cells["frequency"].Value = r["frequency"];
            //    dataGridView1.Rows[row].Cells["plan_finishdate"].Value =
            //        Convert.ToDateTime(r["plan_finishdate"]).ToString("yyyy/MM/dd");

            //    //dataGridView1.Rows[row].Cells["by_date"].Value = r["by_date"];
            //    //            dataGridView1.Rows[row].Cells["by_date"].Value =
            //    //Convert.ToDateTime(r["by_date"]).ToString("yyyy/MM/dd HH:mm:ss");
            //    dataGridView1.Rows[row].Cells["by_date"].Value =
            //    r["by_date"] == DBNull.Value
            //        ? ""
            //        : Convert.ToDateTime(r["by_date"])
            //              .ToString("yyyy/MM/dd HH:mm:ss");


            //    //dataGridView1.Rows[row].Cells["by_date"].Value = r["by_date"];
            //    dataGridView1.Rows[row].Cells["by_user"].Value = r["by_user"];
            //    dataGridView1.Rows[row].Cells["jc_date"].Value = r["jc_date"];
            //    dataGridView1.Rows[row].Cells["jc_user"].Value = r["jc_user"];
            //    dataGridView1.Rows[row].Cells["plan_begindate"].Value =
            //        Convert.ToDateTime(r["plan_begindate"]).ToString("yyyy/MM/dd");
            //    dataGridView1.Rows[row].Cells["plan_enddate"].Value =
            //        Convert.ToDateTime(r["plan_enddate"]).ToString("yyyy/MM/dd");
            //    dataGridView1.Rows[row].Cells["plan_id"].Value = r["plan_id"];
            //    dataGridView1.Rows[row].Cells["plan_name"].Value = r["plan_name"];

            //}

            //lblPageInfo.Text = $"Page {_currentPage}";


        }

        private void QueryMaintenanceRecord()
        {
            string factoryName = comboBox1.Text == "All" ? "" : comboBox1.Text.Split('|')[0].Trim();
            string deviceName = comboBox2.Text == "All" ? "" : comboBox2.Text;
            string departmentName = comboBox3.Text == "All" ? "" : comboBox3.Text;
            string maintainStatusText = comboBox4.Text;
            string filterDateType = comboBox5.Text;
            string startDate = Convert.ToDateTime(dateTimePicker2.Text).ToString("yyyy/MM/dd");
            string endDate = Convert.ToDateTime(dateTimePicker1.Text).ToString("yyyy/MM/dd");
            string level = comboBox7.Text == "All" ? "" : comboBox7.Text;
            string checkResult = comboBox8.Text;

            Dictionary<string, object> p = new Dictionary<string, object>();
            p["factory_name"] = factoryName;
            p["device_name"] = deviceName;
            p["department_name"] = departmentName;

            p["maintain_status"] =
                maintainStatusText == "All" ? -1 :
                maintainStatusText == "Unmaintained" ? 0 : 1;

            p["filter_date_type"] =
                filterDateType == "Planned_Maintenance_Date" ? 0 :
                filterDateType == "Actual_Maintenance_Date" ? 1 : 2;

            p["start_date"] = startDate;
            p["end_date"] = endDate;
            p["level"] = level;

            p["check_result"] =
                checkResult == "All" ? -1 :
                checkResult == "Not_Audited" ? 0 :
                checkResult == "OK" ? 1 : 2;

            // ✅ pagination params
            p["page_no"] = _currentPage;
            p["page_size"] = _pageSize;

            Cursor.Current = Cursors.WaitCursor;
            label9.ForeColor = Color.Red;
            label9.Text = "Please wait data is loading..";
            label9.Visible = true;

            string ret = WebAPIHelper.Post(
                Program.Client.APIURL,
                "KZ_EPMAPI",
                "KZ_EPMAPI.Controllers.MaintenanceRecordService",
                "QueryMaintenanceRecord",
                Program.Client.UserToken,
                JsonConvert.SerializeObject(p)
            );



            if (!Convert.ToBoolean(JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["IsSuccess"]))
            {
                MessageHelper.ShowErr(this,
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["ErrMsg"].ToString());
                return;
            }

            string json = JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["RetData"].ToString();
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(json);

            dataGridView1.Rows.Clear();

            if (dt.Rows.Count == 0)
            {
                label9.Visible = false;
                MessageBox.Show("No Data!");
                return;
            }

            // ✅ bind ONLY current page data
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int row = dataGridView1.Rows.Add();
                DataRow r = dt.Rows[i];

                dataGridView1.Rows[row].Cells["by_status"].Value =
                    r["by_status"].ToString() == "0" ? "Unmaintained" : "Maintained";

                int jc = Convert.ToInt32(r["jc_result"]);
                dataGridView1.Rows[row].Cells["jc_result"].Value =
                    jc == 0 ? "Not_Audited" : jc == 1 ? "OK" : "NG";

                if (jc == 1) SetRowColor(row, Color.Green);
                if (jc == 2) SetRowColor(row, Color.Red);

                dataGridView1.Rows[row].Cells["jc_description"].Value = r["jc_description"];
                dataGridView1.Rows[row].Cells["org_name"].Value = r["org_name"];
                dataGridView1.Rows[row].Cells["department_name"].Value = r["address_name"];
                dataGridView1.Rows[row].Cells["device_name"].Value = r["device_name"];
                dataGridView1.Rows[row].Cells["snid"].Value = r["snid"];
                dataGridView1.Rows[row].Cells["type"].Value = r["type"];
                dataGridView1.Rows[row].Cells["body_part"].Value = r["body_part"];
                dataGridView1.Rows[row].Cells["item"].Value = r["item"];
                dataGridView1.Rows[row].Cells["level_name"].Value = r["level_name"];
                dataGridView1.Rows[row].Cells["frequency"].Value = r["frequency"];
                dataGridView1.Rows[row].Cells["plan_finishdate"].Value =
                    Convert.ToDateTime(r["plan_finishdate"]).ToString("yyyy/MM/dd");

                //dataGridView1.Rows[row].Cells["by_date"].Value = r["by_date"];
                //            dataGridView1.Rows[row].Cells["by_date"].Value =
                //Convert.ToDateTime(r["by_date"]).ToString("yyyy/MM/dd HH:mm:ss");
                dataGridView1.Rows[row].Cells["by_date"].Value =
                r["by_date"] == DBNull.Value
                    ? ""
                    : Convert.ToDateTime(r["by_date"])
                          .ToString("yyyy/MM/dd HH:mm:ss");


                //dataGridView1.Rows[row].Cells["by_date"].Value = r["by_date"];
                dataGridView1.Rows[row].Cells["by_user"].Value = r["by_user"];
                dataGridView1.Rows[row].Cells["jc_date"].Value = r["jc_date"];
                dataGridView1.Rows[row].Cells["jc_user"].Value = r["jc_user"];
                dataGridView1.Rows[row].Cells["plan_begindate"].Value =
                    Convert.ToDateTime(r["plan_begindate"]).ToString("yyyy/MM/dd");
                dataGridView1.Rows[row].Cells["plan_enddate"].Value =
                    Convert.ToDateTime(r["plan_enddate"]).ToString("yyyy/MM/dd");
                dataGridView1.Rows[row].Cells["plan_id"].Value = r["plan_id"];
                dataGridView1.Rows[row].Cells["plan_name"].Value = r["plan_name"];

            }

            lblPageInfo.Text = $"Page {_currentPage}";
            Cursor.Current = Cursors.Default;
            label9.Visible = false;
            btnPrev.Visible = true;
            btnNext.Visible = true;
            lblPageInfo.Visible = true;
        }


        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            LoadDeviceInfo();
            GetPositionInfoByOrgId();
        }

        private void LoadDeviceInfo()
        {
            deviceInfo.Clear();

            List<AutocompleteItem> items3 = new List<AutocompleteItem>();
            List<string> stringList = new List<string>();
            AutoCompleteStringCollection autoComplete1 = new AutoCompleteStringCollection();

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("orgId", FindOrgCode());
            string ret = SJeMES_Framework.WebAPI.WebAPIHelper.Post(Program.Client.APIURL, "KZ_EPMAPI", "KZ_EPMAPI.Controllers.MaintenanceRecordService", "GetDeviceInfoByOrgId", Program.Client.UserToken, JsonConvert.SerializeObject(p));
            if (Convert.ToBoolean(Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["IsSuccess"]))
            {
                string Data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["RetData"].ToString();
                DataTable dtJson = SJeMES_Framework.Common.JsonHelper.GetDataTableByJson(Data);
                //dtJson.Rows.Add("全部","");

                items3.Add(new MulticolumnAutocompleteItem(new[] { "", "" }, "All"));
                stringList.Add("All");

                for (int i = 0; i < dtJson.Rows.Count; i++)
                {
                    deviceInfo.Add(dtJson.Rows[i]["device_no"].ToString() + "|" + dtJson.Rows[i]["device_name"].ToString());
                    autoComplete1.Add(dtJson.Rows[i]["device_name"].ToString());
                    items3.Add(new MulticolumnAutocompleteItem(new[] { dtJson.Rows[i]["device_no"].ToString(), dtJson.Rows[i]["device_name"].ToString() }, dtJson.Rows[i]["device_name"].ToString()));
                }
                comboBox2.DataSource = items3;
                autoComplete1.AddRange(stringList.ToArray());
                autoComplete_device.AddRange(stringList.ToArray());
                comboBox2.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox2.AutoCompleteSource = AutoCompleteSource.CustomSource;
                comboBox2.AutoCompleteCustomSource = autoComplete1;
            }
            else
            {
                MessageHelper.ShowErr(this, JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["ErrMsg"].ToString());
            }

        }

        private void GetPositionInfoByOrgId()
        {
            postionInfo.Clear();

            List<AutocompleteItem> items3 = new List<AutocompleteItem>();

            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("orgId", FindOrgCode());
            string ret = SJeMES_Framework.WebAPI.WebAPIHelper.Post(Program.Client.APIURL, "KZ_EPMAPI", "KZ_EPMAPI.Controllers.MaintenanceRecordService", "GetPositionInfoByOrgId", Program.Client.UserToken, JsonConvert.SerializeObject(p));
            if (Convert.ToBoolean(Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["IsSuccess"]))
            {
                string Data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["RetData"].ToString();
                DataTable dtJson = SJeMES_Framework.Common.JsonHelper.GetDataTableByJson(Data);
                //dtJson.Rows.Add("全部","");

                items3.Add(new MulticolumnAutocompleteItem(new[] { "", "" }, "All"));


                for (int i = 0; i < dtJson.Rows.Count; i++)
                {
                    postionInfo.Add(dtJson.Rows[i]["address_code"].ToString() + "|" + dtJson.Rows[i]["address_name"].ToString());
                    items3.Add(new MulticolumnAutocompleteItem(new[] { dtJson.Rows[i]["address_code"].ToString(), dtJson.Rows[i]["address_name"].ToString() }, dtJson.Rows[i]["address_name"].ToString()));
                }
                comboBox3.DataSource = items3;
            }
            else
            {
                MessageHelper.ShowErr(this, JsonConvert.DeserializeObject<Dictionary<string, object>>(ret)["ErrMsg"].ToString());
            }

        }

        /*工厂下拉框选择后获取org_code*/
        private string FindOrgCode()
        {
            string text = comboBox1.Text;
            if (text.Equals("All"))
            {
                return "";
            }
            string orgId = text.Split('|')[1];
            return orgId;
        }

        private string GetCodeFromList(List<string> list, string text)
        {
            if (text.Equals("All"))
            {
                return "";
            }
            for (int i = 0; i < list.Count; i++)
            {
                string data = list[i];
                string[] st = data.Split('|');
                if (st[1].Equals(text))
                {
                    return st[0];
                }
            }
            return "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Boolean isExcute = false;
            DataTable dt = new DataTable();
            for (int i = 1; i < dataGridView1.Columns.Count; i++)
            {
                dt.Columns.Add(dataGridView1.Columns[i].HeaderText.ToString());
            }

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                DataGridViewCell o = dataGridView1.Rows[i].Cells["column_cb"];
                if (o != null && o.Value != null && bool.Parse(o.Value.ToString()))
                {
                    isExcute = true;
                    DataRow dr = dt.NewRow();
                    for (int j = 1; j < dataGridView1.Rows[i].Cells.Count; j++)
                    {
                        dr[j - 1] = dataGridView1.Rows[i].Cells[j].Value.ToString();
                    }
                    dt.Rows.Add(dr);
                }
            }
            if (isExcute)
            {
                NewExportExcels.ExportExcels.Export("Maintenance History (details)", dt);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells["column_cb"].Value = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Cells["column_cb"].Value = false;
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                QueryMaintenanceRecord();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            _currentPage++;
            QueryMaintenanceRecord();

        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            QueryMaintenanceRecordAll();
        }
    }
}
