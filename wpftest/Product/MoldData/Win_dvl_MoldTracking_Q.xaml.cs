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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WizMes_HanMin.PopUP;
using WPF.MDI;

namespace WizMes_HanMin
{
    /// <summary>
    /// Win_dvl_MoldTracking_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_dvl_MoldTracking_Q : UserControl
    {

        int rowNum = 0;
        Win_dvl_MoldTracking_Q_CodeView WindvlMoldTracking = new Win_dvl_MoldTracking_Q_CodeView();


        public Win_dvl_MoldTracking_Q()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Lib.Instance.UiLoading(sender);
            //chkInOutDate.IsChecked = true;
            //dtpSDate.IsEnabled = true;
            //dtpEDate.IsEnabled = true;


            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }


        private void lblInOutDate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(chkInOutDate.IsChecked == false)
            {
                chkInOutDate.IsChecked = true;
                dtpSDate.IsEnabled = true;
                dtpEDate.IsEnabled = true;
            }
            else
            {
                chkInOutDate.IsChecked = false;
                dtpSDate.IsEnabled = false;
                dtpEDate.IsEnabled = false;
            }
        }

        private void chkInOutDate_Click(object sender, RoutedEventArgs e)
        {
            if (chkInOutDate.IsChecked == false)
            {
                chkInOutDate.IsChecked = true;
                dtpSDate.IsEnabled = true;
                dtpEDate.IsEnabled = true;
            }
            else
            {
                chkInOutDate.IsChecked = false;
                dtpSDate.IsEnabled = false;
                dtpEDate.IsEnabled = false;
            }
        }

        private void btnYesterday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today.AddDays(-1);
            dtpEDate.SelectedDate = DateTime.Today.AddDays(-1);
        }

        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }

        private void btnLastMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = Lib.Instance.BringLastMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringLastMonthDatetimeList()[1];
        }

        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];
        }

        private void lblProductName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(chkProductName.IsChecked == false)
            {
                chkProductName.IsChecked = true;
                txtProductName.IsEnabled = true;
                btnPfProdName.IsEnabled = true;
            }
            else
            {
                chkProductName.IsChecked = false;
                txtProductName.IsEnabled = false;
                btnPfProdName.IsEnabled = false;
            }
        }

        private void chkProductName_Click(object sender, RoutedEventArgs e)
        {
            if (chkProductName.IsChecked == false)
            {
                chkProductName.IsChecked = true;
                txtProductName.IsEnabled = true;
                btnPfProdName.IsEnabled = true;
            }
            else
            {
                chkProductName.IsChecked = false;
                txtProductName.IsEnabled = false;
                btnPfProdName.IsEnabled = false;
            }
        }

        private void btnPfProdName_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtProductName, (int)Defind_CodeFind.DCF_Article, "");
        }

        private void lblMoldLotNo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(chkMoldLotNo.IsChecked == false)
            {
                chkMoldLotNo.IsChecked = true;
                txtMoldLotNo.IsEnabled = true;
                btnPfMoldLotNo.IsEnabled = true;
            }
            else
            {
                chkMoldLotNo.IsChecked = false;
                txtMoldLotNo.IsEnabled = false;
                btnPfMoldLotNo.IsEnabled = false;
            }
        }

        private void chkMoldLotNo_Click(object sender, RoutedEventArgs e)
        {
            if (chkMoldLotNo.IsChecked == false)
            {
                chkMoldLotNo.IsChecked = true;
                txtMoldLotNo.IsEnabled = true;
                btnPfMoldLotNo.IsEnabled = true;
            }
            else
            {
                chkMoldLotNo.IsChecked = false;
                txtMoldLotNo.IsEnabled = false;
                btnPfMoldLotNo.IsEnabled = false;
            }
        }

        private void txtMoldLotNo_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtMoldLotNo, (int)Defind_CodeFind.DCF_MOLD, "");
            }
        }

        private void btnPfMoldLotNo_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtMoldLotNo, (int)Defind_CodeFind.DCF_MOLD, "");
        }

        private void txtProductName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtProductName, (int)Defind_CodeFind.DCF_Article, "");
            }
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            rowNum = 0;
            re_search(rowNum);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            foreach (MenuViewModel mvm in MainWindow.mMenulist)
            {
                if (mvm.subProgramID.ToString().Contains("MDI"))
                {
                    if (this.ToString().Equals((mvm.subProgramID as MdiChild).Content.ToString()))
                    {
                        (MainWindow.mMenulist[i].subProgramID as MdiChild).Close();
                        break;
                    }
                }
                i++;
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] dgdStr = new string[6];
            dgdStr[0] = "금형정보";
            dgdStr[1] = "금형생산수량";
            dgdStr[2] = "금형수리내역";
            dgdStr[3] = dgdMain.Name;
            dgdStr[4] = dgdSub1.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(dgdStr);
            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdMain.Name))
                {
                    if (ExpExc.Check.Equals("Y")) { dt = Lib.Instance.DataGridToDTinHidden(dgdMain); }
                    else { dt = Lib.Instance.DataGirdToDataTable(dgdMain); }

                    Name = dgdMain.Name;
                    if (Lib.Instance.GenerateExcel(dt, Name)) { Lib.Instance.excel.Visible = true; }
                    else return;
                }
                else if (ExpExc.choice.Equals(dgdSub1.Name))
                {
                    if (ExpExc.Check.Equals("Y")) { dt = Lib.Instance.DataGridToDTinHidden(dgdSub1); }
                    else { dt = Lib.Instance.DataGirdToDataTable(dgdSub1); }

                    Name = dgdMain.Name;
                    if (Lib.Instance.GenerateExcel(dt, Name)) { Lib.Instance.excel.Visible = true; }
                    else return;
                }
  
                else { if (dt != null) dt.Clear(); }
            }
        }

        private void re_search(int idx)
        {
            if(dgdMain.Items.Count > 0)
            {
                dgdMain.Items.Clear();
            }

            FillGrid();

            dgdMain.SelectedIndex = idx;
        }

        private void FillGrid()
        {
            if (dgdMain.Items.Count > 0)
            {
                dgdMain.Items.Clear();
                dgdSub1.Items.Clear();
            }
            

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("chkDate", chkInOutDate.IsChecked == true ? 1 : 0);
                sqlParameter.Add("FromDate", dtpSDate.SelectedDate != null ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("ToDate", dtpEDate.SelectedDate != null ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");

                
                sqlParameter.Add("nchkMold", chkMoldLotNo.IsChecked == true ? 1 : 0);
                sqlParameter.Add("MoldNo", txtMoldLotNo.Tag != null && !txtMoldLotNo.Text.Trim().Equals("") ? txtMoldLotNo.Tag.ToString() : "");
                sqlParameter.Add("nchkArticle", chkProductName.IsChecked == true ? 1 : 0);
                sqlParameter.Add("sArticleID", txtProductName.Tag != null && !txtProductName.Text.Trim().Equals("") ? txtProductName.Tag.ToString() : "");


                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sMoldTrack_Main", sqlParameter, false);

                if(ds!=null && ds.Tables.Count > 0)
                {
                    DataTable dt = null;
                    dt = ds.Tables[0];
                    int i = 0;

                    if(dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = null;
                        drc = dt.Rows;

                        foreach(DataRow dr in drc)
                        {
                            var WinMoldTrack = new Win_dvl_MoldTracking_Q_CodeView()
                            {
                                Num = i+1,
                                MoldID = dr["MoldID"].ToString().Trim(), //금형번호
                                MoldHitLimitCount = dr["MoldHitLimitCount"].ToString().Trim(),
                                Article = dr["Article"].ToString().Trim(),
                                ArticleID = dr["ArticleID"].ToString().Trim(),
                                BuyerArticleNo = dr["BuyerArticleNo"].ToString().Trim(),
                                Spec = dr["Spec"].ToString().Trim(),
                                MoldName = dr["MoldName"].ToString().Trim(),
                                HitCount = dr["HitCount"].ToString().Trim(),
                                
        
                            };

                            dgdMain.Items.Add(WinMoldTrack);
                            i++;
                        }
                    }
                    else
                    {
                        MessageBox.Show("조회 결과가 없습니다.", "알림");
                    }
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show("오류발생!, 오류 원인 : " + ex.ToString(), "경고");
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }  
        }

        private void dgdMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WindvlMoldTracking = dgdMain.SelectedItem as Win_dvl_MoldTracking_Q_CodeView;
             
            if(WindvlMoldTracking != null)
            {
                this.DataContext = WindvlMoldTracking;
                FillGridSub1(WindvlMoldTracking.MoldID);
                
            }
        }

        private void FillGridSub1(string MoldID)
        {
            if(dgdSub1.Items.Count > 0)
            {
                dgdSub1.Items.Clear();
            }


            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("sMoldID", MoldID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sMoldTrack_Sub", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;
                        int j = 0;

                        for (int i = 0; i < drc.Count; i++)
                        {
                            j = i;
                            DataRow dr = drc[i];

                            var MoldSub = new Win_dvl_MoldTracking_Q_SubOne_CodeView()
                            {

                                SetDate = dr["SetDate"].ToString(),
                                EvalDate = dr["EvalDate"].ToString(),
                                MachineID = dr["MachineID"].ToString(),
                                Process = dr["Process"].ToString(),
                                HitCount = dr["HitCount"].ToString(),
                                
                            };


                            dgdSub1.Items.Add(MoldSub);

                        }
                    } // for문 끝
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show("오류발생!, 오류 원인 : " + ex.ToString(), "경고");
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }
       

    }

    class Win_dvl_MoldTracking_Q_CodeView : BaseView
    {
        public int Num { get; set; }
        public string MoldID { get; set; }
        public string MoldName { get; set; }
        public string Spec { get; set; }
        public string ArticleID { get; set; }
        public string Article { get; set; }
        public string BuyerArticleNo { get; set; }
        public string MoldHitLimitCount { get; set; }
        public string ProcessID { get; set; }
        public string Process { get; set; }
        public string SetDate { get; set; }         //사용일자 ?
        public string EvalDate { get; set; }            //사용기간?
        public string HitCount { get; set; }            //사용기간?

    }

    class Win_dvl_MoldTracking_Q_SubOne_CodeView : BaseView
    {
        //public string MachineName { get; set; }

        public string MachineID { get; set; }
        public string Process { get; set; }
        public string SetDate { get; set; } //사용일자?
        public string EvalDate { get; set; } //사용기간?
        public string HitCount { get; set; } //사용기간?

                                
    }
    class Win_dvl_MoldTracking_Q_SubTwo_CodeView : BaseView
    {
        public int Num { get; set; }
        public string RepairID { get; set; }
        public string repairdate { get; set; }
        public string repairTime { get; set; }
        public string RepairGubun { get; set; }
        public string RepairGubunName { get; set; }
        public string MoldID { get; set; }
        public string RepairCustom { get; set; }
        public string RepairRemark { get; set; }
        public string MoldKind { get; set; }
        public string MoldQuality { get; set; }
        public string Weight { get; set; }
        public string Spec { get; set; }
        public string MoldNo { get; set; }
        public string ProdCustomName { get; set; }
        public string Article { get; set; }
    }
}
