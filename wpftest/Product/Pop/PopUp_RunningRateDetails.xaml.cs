using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WizMes_HanMin.Product.Pop
{
    /// <summary>
    /// PopUp_RunningRateDetails.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PopUp_RunningRateDetails : Window
    {
        string sFromDate = string.Empty;
        string sToDate = string.Empty;
        string sProcessid = string.Empty;
        string sProcessName = string.Empty;
        string sMachineID = string.Empty;
        string sMachineName = string.Empty;


        public PopUp_RunningRateDetails()
        {
            InitializeComponent();
        }

        public PopUp_RunningRateDetails(string strFromDate, string strToDate, string strProcessID, string strProcessName, string strMachineID, string strMachineName)
        {
            InitializeComponent();
            sFromDate = strFromDate;
            sToDate = strToDate;
            sProcessid = strProcessID;
            sProcessName = strProcessName;
            sMachineID = strMachineID;
            sMachineName = strMachineName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            btnSearch_Click(null, null);
            TextBoxDateSearch.Text = sFromDate + "~" + sToDate;
            TextBoxProcessSearch.Text = sProcessName;
            TextBoxMachineSearch.Text = sMachineName;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            FillGrid();

            //FillGrid_MachineRunning();
            //FillGrid_WorkPerson();
            //FillGrid_McInspect();
            //FillGrid_Defect();
            //FillGrid_NoRework();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        #region 조회
        private void FillGrid()
        {
            DataGridMachineRunning.Items.Clear();
            DataGridWorkPerson.Items.Clear();
            DataGridDefect.Items.Clear();
            DataGridNoRework.Items.Clear();
            DataGridMcInspect.Items.Clear();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("sFromDate", sFromDate.Replace("-", ""));
                sqlParameter.Add("sToDate", sToDate.Replace("-", ""));
                sqlParameter.Add("nProcessid", sProcessid == string.Empty ? 0 : 1);
                sqlParameter.Add("sProcessid", sProcessid);
                sqlParameter.Add("nMachineid", sMachineID == string.Empty ? 0 : 1);
                sqlParameter.Add("sMachineID", sMachineID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sAnalProd", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt_MachineRunning = ds.Tables[0];
                    DataTable dt_WorkPerson = ds.Tables[1];
                    DataTable dt_WorkQty = ds.Tables[2];
                    DataTable dt_Defect = ds.Tables[3];
                    DataTable dt_McInspect = ds.Tables[4];
                    DataTable dt_NoRework = ds.Tables[5];

                    //MachineRunning
                    if (dt_MachineRunning.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt_MachineRunning.Rows;
                        int i = 0;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var PRMC = new PopUp_RunningRateDetails_MachineRunning_CodeView()
                            {
                                MachineRunningRate = Convert.ToDouble(dr["MachineRunningRate"]),
                                GoalRunRate = Convert.ToDouble(dr["GoalRunRate"]),
                            };

                            DataGridMachineRunning.Items.Add(PRMC);
                        }
                    }

                    //WorkPerson
                    if (dt_WorkPerson.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt_WorkPerson.Rows;
                        int i = 0;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var PRWC = new PopUp_RunningRateDetails_WorkPerson_CodeView()
                            {
                                WorkPersonCount = Convert.ToDouble(dr["WorkPersonCount"]),
                                WorkPersonEndCount = Convert.ToDouble(dr["WorkPersonEduCount"]),
                            };

                            DataGridWorkPerson.Items.Add(PRWC);
                        }
                    }

                    //WorkQty

                    //Defect
                    if (dt_Defect.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt_Defect.Rows;
                        int i = 0;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var PRDC = new PopUp_RunningRateDetails_Defect_CodeView()
                            {
                                DefectID = dr["DefectID"].ToString(),
                                KDefect = dr["kDefect"].ToString(),
                                DefectQty = Convert.ToDouble(dr["DefectQty"]),
                            };

                            DataGridDefect.Items.Add(PRDC);
                        }
                    }

                    //McInspect
                    if (dt_McInspect.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt_McInspect.Rows;
                        int i = 0;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var PRMC = new PopUp_RunningRateDetails_McInspect_CodeView()
                            {
                                McInspectBasisID = dr["McInspectBasisID"].ToString(),
                                MCID = dr["MCID"].ToString(),
                                McName = dr["McName"].ToString(),
                                Managerid = dr["Managerid"].ToString(),
                                McInsBasisDate = dr["McInsBasisDate"].ToString(),
                                McInsContent = dr["McInsContent"].ToString(),
                                BasisComments = dr["BasisComments"].ToString(),
                                McRInspectID = dr["McRInspectID"].ToString(),
                                McRInspectDate = dr["McRInspectDate"].ToString(),
                                McInsCycleGbn = dr["McInsCycleGbn"].ToString(),
                                McInsCycle = dr["McInsCycle"].ToString(),
                                McRInspectPersonID = dr["McRInspectPersonID"].ToString(),
                                DefectContents = dr["DefectContents"].ToString(),
                                DefectReason = dr["DefectReason"].ToString(),
                                DefectRespectContents = dr["DefectRespectContents"].ToString(),
                                Comments = dr["Comments"].ToString(),
                            };

                            DataGridMcInspect.Items.Add(PRMC);
                        }
                    }

                    //NoRework
                    if (dt_NoRework.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt_NoRework.Rows;
                        int i = 0;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var PRNC = new PopUp_RunningRateDetails_NoRework_CodeView()
                            {
                                NoReworkName = dr["noReworkName"].ToString(),
                                NoReworkTime = dr["noReworkTime"].ToString(),
                            };

                            DataGridNoRework.Items.Add(PRNC);
                        }
                    }

                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        #endregion

        #region 개별조회 - MachineRunning
        private void FillGrid_MachineRunning()
        {
            if(DataGridMachineRunning.Items.Count > 0)
            {
                DataGridMachineRunning.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("sFromDate", "20220101");
                sqlParameter.Add("sToDate", "20220101");
                sqlParameter.Add("nProcessid", 0);
                sqlParameter.Add("sProcessid", "");
                sqlParameter.Add("nMachineid", 0);
                sqlParameter.Add("sMachineID", "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sAnalProd_MachineRunning", sqlParameter, false);

                if(ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if(dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;
                        int i = 0;

                        foreach(DataRow dr in drc)
                        {
                            i++;

                            var PRMC = new PopUp_RunningRateDetails_MachineRunning_CodeView()
                            {
                                MachineRunningRate = Convert.ToDouble(dr["MachineRunningRate"]),
                                GoalRunRate = Convert.ToDouble(dr["GoalRunRate"]),
                            };

                            DataGridMachineRunning.Items.Add(PRMC);
                        }
                    }
                }
            }
            catch(Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        #endregion

        #region 개별조회 - WorkPerson
        private void FillGrid_WorkPerson()
        {
            if (DataGridWorkPerson.Items.Count > 0)
            {
                DataGridWorkPerson.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("sFromDate", "20220101");
                sqlParameter.Add("sToDate", "20220101");
                sqlParameter.Add("nProcessid", 0);
                sqlParameter.Add("sProcessid", "");
                sqlParameter.Add("nMachineid", 0);
                sqlParameter.Add("sMachineID", "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sAnalProd_WorkPerson", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;
                        int i = 0;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var PRWC = new PopUp_RunningRateDetails_WorkPerson_CodeView()
                            {
                                WorkPersonCount = Convert.ToDouble(dr["WorkPersonCount"]),
                                WorkPersonEndCount = Convert.ToDouble(dr["WorkPersonEduCount"]),
                            };

                            DataGridWorkPerson.Items.Add(PRWC);
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }
        #endregion

        #region 개별조회 - Defect
        private void FillGrid_Defect()
        {
            if (DataGridDefect.Items.Count > 0)
            {
                DataGridDefect.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("sFromDate", "20220101");
                sqlParameter.Add("sToDate", "20220101");
                sqlParameter.Add("nProcessid", 0);
                sqlParameter.Add("sProcessid", "");
                sqlParameter.Add("nMachineid", 0);
                sqlParameter.Add("sMachineID", "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sAnalProd_Defect", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;
                        int i = 0;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var PRDC = new PopUp_RunningRateDetails_Defect_CodeView()
                            {
                                DefectID = dr["DefectID"].ToString(),
                                KDefect = dr["kDefect"].ToString(),
                                DefectQty = Convert.ToDouble(dr["DefectQty"]),
                            };

                            DataGridDefect.Items.Add(PRDC);
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }
        #endregion

        #region 개별조회 - NoRework
        private void FillGrid_NoRework()
        {
            if (DataGridNoRework.Items.Count > 0)
            {
                DataGridNoRework.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("sFromDate", "20220101");
                sqlParameter.Add("sToDate", "20220101");
                sqlParameter.Add("nProcessid", 0);
                sqlParameter.Add("sProcessid", "");
                sqlParameter.Add("nMachineid", 0);
                sqlParameter.Add("sMachineID", "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sAnalProd_NoRework", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;
                        int i = 0;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var PRNC = new PopUp_RunningRateDetails_NoRework_CodeView()
                            {
                                NoReworkName = dr["noReworkName"].ToString(),
                                NoReworkTime = dr["noReworkTime"].ToString(),
                            };

                            DataGridNoRework.Items.Add(PRNC);
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }
        #endregion

        #region 개별조회 - McInspect
        private void FillGrid_McInspect()
        {
            if (DataGridMcInspect.Items.Count > 0)
            {
                DataGridMcInspect.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("sFromDate", "20220101");
                sqlParameter.Add("sToDate", "20220101");
                sqlParameter.Add("nProcessid", 0);
                sqlParameter.Add("sProcessid", "");
                sqlParameter.Add("nMachineid", 0);
                sqlParameter.Add("sMachineID", "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sAnalProd_McInspect", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;
                        int i = 0;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var PRMC = new PopUp_RunningRateDetails_McInspect_CodeView()
                            {
                                McInspectBasisID = dr["McInspectBasisID"].ToString(),
                                MCID = dr["MCID"].ToString(),
                                McName = dr["McName"].ToString(),
                                Managerid = dr["Managerid"].ToString(),
                                McInsBasisDate = dr["McInsBasisDate"].ToString(),
                                McInsContent = dr["McInsContent"].ToString(),
                                BasisComments = dr["BasisComments"].ToString(),
                                McRInspectID = dr["McRInspectID"].ToString(),
                                McRInspectDate = dr["McRInspectDate"].ToString(),
                                McInsCycleGbn = dr["McInsCycleGbn"].ToString(),
                                McInsCycle = dr["McInsCycle"].ToString(),
                                McRInspectPersonID = dr["McRInspectPersonID"].ToString(),
                                DefectContents = dr["DefectContents"].ToString(),
                                DefectReason = dr["DefectReason"].ToString(),
                                DefectRespectContents = dr["DefectRespectContents"].ToString(),
                                Comments = dr["Comments"].ToString(),
                            };

                            DataGridMcInspect.Items.Add(PRMC);
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }
        #endregion

    }

    #region CodeView
    class PopUp_RunningRateDetails_MachineRunning_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public double MachineRunningRate { get; set; }
        public double GoalRunRate { get; set; }
    }

    class PopUp_RunningRateDetails_WorkPerson_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public double WorkPersonCount { get; set; }
        public double WorkPersonEndCount { get; set; }
    }

    class PopUp_RunningRateDetails_WorkQty_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public double WorkQty { get; set; }
    }

    class PopUp_RunningRateDetails_Defect_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public string DefectID { get; set; }
        public string KDefect { get; set; }
        public double DefectQty { get; set; }
    }

    class PopUp_RunningRateDetails_NoRework_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }
        public string NoReworkName { get; set; }
        public string NoReworkTime { get; set; }
    }

    class PopUp_RunningRateDetails_McInspect_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }
        public string MC { get; set; }
        public string McInspectBasisID { get; set; }
        public string MCID { get; set; }
        public string McName { get; set; }
        public string Managerid { get; set; }
        public string McInsBasisDate { get; set; }
        public string McInsContent { get; set; }
        public string BasisComments { get; set; }
        public string McRInspectID { get; set; }
        public string McRInspectDate { get; set; }
        public string McInsCycleGbn { get; set; }
        public string McInsCycle { get; set; }
        public string McRInspectPersonID { get; set; }
        public string DefectContents { get; set; }
        public string DefectReason { get; set; }
        public string DefectRespectContents { get; set; }
        public string Comments { get; set; }
    }

    #endregion
}
