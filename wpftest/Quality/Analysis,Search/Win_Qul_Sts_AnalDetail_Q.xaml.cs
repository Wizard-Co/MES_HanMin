using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace WizMes_HanMin
{
    /// <summary>
    /// 
    /// 
    /// </summary>
    public partial class Win_Qul_Sts_AnalDetail_Q : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        int rowNum = 0;
        Lib lib = new Lib();
        public string flag = "";

        public Win_Qul_Sts_AnalDetail_Q()
        {
            InitializeComponent();
        }

        //public Win_Qul_Sts_AnalDetail_Q(String InspectDate, String LOTNO, String ArticleID, String BuyerArticleNo, String DefectGubn)
        //{
        //    InitializeComponent();

        //    chkArticle.IsChecked = true;
        //    DateTime EndDt = DateTime.ParseExact(InspectDate, "yyyyMMdd", null);
        //    dtpSDate.SelectedDate = EndDt.AddDays(-30);
        //    dtpEDate.SelectedDate = EndDt;

        //    txtArticle.Text = BuyerArticleNo;
        //    txtArticle.Tag = ArticleID;

        //    lblLotNo.Text = LOTNO;
        //    lblDefectGubn.Text = DefectGubn;

        //    flag = "Popup";
        //}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            Lib.Instance.UiLoading(sender);

            if (MainWindow.tempContent != null
                && MainWindow.tempContent.Count > 0)
            {
                string InspectDate = MainWindow.tempContent[0];
                string LOTNO = MainWindow.tempContent[1];
                string ArticleID = MainWindow.tempContent[2];
                string BuyerArticleNo = MainWindow.tempContent[3];
                string DefectGubn = MainWindow.tempContent[4];
                
                //입고일자 체크
                chkDate.IsChecked = true;
                txtArticle.IsEnabled = true;
                btnPfArticle.IsEnabled = true;

                DateTime EndDt = DateTime.ParseExact(InspectDate, "yyyyMMdd", null);
                dtpSDate.SelectedDate = EndDt.AddDays(-30);
                dtpEDate.SelectedDate = EndDt;

                txtArticle.Text = BuyerArticleNo;
                txtArticle.Tag = ArticleID;

                lblLotNo.Text = LOTNO;
                lblDefectGubn.Text = DefectGubn;

                FillGrid();
                FillGrid2();
            }
            else
            {
                //입고일자 체크
                chkDate.IsChecked = true;

                //품명 체크 해제
                chkArticle.IsChecked = true;

                //데이트피커 오늘 날짜
                dtpSDate.SelectedDate = DateTime.Today.AddDays(-30);
                dtpEDate.SelectedDate = DateTime.Today;

                txtArticle.Text = "";
                txtArticle.Tag = "";

                lblLotNo.Text = "";
                lblDefectGubn.Text = "";

                //조건 박스 true
                txtArticle.IsEnabled = true;
                btnPfArticle.IsEnabled = true;
            }
        }



        #region 클릭 이벤트

        //입고일자 라벨 클릭 이벤트
        private void LblchkDay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkDate.IsChecked == true)
            {
                chkDate.IsChecked = false;
                dtpSDate.IsEnabled = false;
                dtpEDate.IsEnabled = false;
            }
            else
            {
                chkDate.IsChecked = true;
                dtpSDate.IsEnabled = true;
                dtpEDate.IsEnabled = true;
            }
        }

        //입고일자 체크 이벤트
        private void ChkDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = true;
            dtpEDate.IsEnabled = true;
        }

        //입고일자 체크해제 이벤트
        private void ChkDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
        }

        //전일
        private void btnYesterDay_Click(object sender, RoutedEventArgs e)
        {
            DateTime[] SearchDate = lib.BringLastDayDateTimeContinue(dtpEDate.SelectedDate.Value);

            dtpSDate.SelectedDate = SearchDate[0];
            dtpEDate.SelectedDate = SearchDate[1];
        }

        //금일
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }

        // 전월 버튼 클릭 이벤트
        private void btnLastMonth_Click(object sender, RoutedEventArgs e)
        {
            DateTime[] SearchDate = lib.BringLastMonthContinue(dtpSDate.SelectedDate.Value);

            dtpSDate.SelectedDate = SearchDate[0];
            dtpEDate.SelectedDate = SearchDate[1];
        }

        // 금월 버튼 클릭 이벤트
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = lib.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = lib.BringThisMonthDatetimeList()[1];
        }





        //품명 라벨 클릭 이벤트
        private void LblArticle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticle.IsChecked == true)
            {
                chkArticle.IsChecked = false;
                txtArticle.IsEnabled = false;
                btnPfArticle.IsEnabled = false;
            }
            else
            {
                chkArticle.IsChecked = true;
                txtArticle.IsEnabled = true;
                btnPfArticle.IsEnabled = true;
            }
        }

        //품명 텍스트박스 키다운
        private void txtArticle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticle, 83, txtArticle.Text);
            }
        }

        //품명 플러스파인더
        private void BtnPfArticle_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticle, 83, txtArticle.Text);
        }
        #endregion 클릭이벤트, 날짜

        #region CRUD 버튼

        //검색(조회)
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //검색버튼 비활성화
            btnSearch.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>

            {
                Thread.Sleep(2000);

                //로직
                if (CheckData())
                {
                    re_Search(rowNum);
                }

            }), System.Windows.Threading.DispatcherPriority.Background);



            Dispatcher.BeginInvoke(new Action(() =>

            {
                btnSearch.IsEnabled = true;

            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;
            Lib lib = new Lib();

            string[] lst = new string[4];
            lst[0] = "과거 동일품번 품질추이";
            lst[1] = dgdMain.Name;
            lst[2] = "설비별 [반]제품 품질추이";
            lst[3] = dgdMain2.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);

            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
                if (ExpExc.Check.Equals("Y"))
                    dt = lib.DataGridToDTinHidden(dgdMain);
                else
                    dt = lib.DataGirdToDataTable(dgdMain);

                Name = dgdMain.Name;

                if (lib.GenerateExcel(dt, Name))
                {
                    lib.excel.Visible = true;
                    lib.ReleaseExcelObject(lib.excel);
                }
                else
                    return;
            }
            else
            {
                if (dt != null)
                {
                    dt.Clear();
                }
            }
            lib = null;
        }

        #endregion CRUD 버튼


        #region 데이터그리드 이벤트

        //데이터그리드 셀렉션체인지드
        private void dgdMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //조회만 하는 화면이라 이 친구는 필요가 없지요.
        }

        #endregion 데이터그리드 이벤트

        #region 조회관련(Fillgrid)

        //재조회
        private void re_Search(int selectedIndex)
        {
            lblLotNo.Text = "";
            lblDefectGubn.Text = "";

            FillGrid();

            FillGrid2();
            

            if (dgdMain.Items.Count > 0)
            {
                dgdMain.SelectedIndex = selectedIndex;
            }

        }

        //조회
        private void FillGrid()
        {
            if (dgdMain.Items.Count > 0)
            {
                dgdMain.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("nchkDate", chkDate.IsChecked == true ? 1 : 0);
                sqlParameter.Add("StartDate", chkDate.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("EndDate", chkDate.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");

                sqlParameter.Add("nchkArticleID", 1);
                sqlParameter.Add("ArticleID", txtArticle.Text.Trim() == "" ? "" : txtArticle.Tag.ToString());
                //sqlParameter.Add("BuyerArticleNo", chkArticle.IsChecked == true && !txtArticle.Text.Trim().Equals("") ? @Escape(txtArticle.Text) : "");


                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_Qul_StsAnalDetail_s", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        ClearHeader();
                        //MessageBox.Show("조회결과가 없습니다.");
                        return;
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                        int i = 0;
                        foreach (DataRow dr in drc)
                        {
                            i++;
                            var DefectInfo = new Win_Qul_Sts_AnalDetail_Q_CodeView()
                            {
                                examdate = dr["examdate"].ToString(),
                                DefectName1 = dr["DefectName1"].ToString(),
                                DefectQty1 = dr["DefectQty1"].ToString(),
                                DefectName2 = dr["DefectName2"].ToString(),
                                DefectQty2 = dr["DefectQty2"].ToString(),
                                DefectName3 = dr["DefectName3"].ToString(),
                                DefectQty3 = dr["DefectQty3"].ToString(),
                                DefectName4 = dr["DefectName4"].ToString(),
                                DefectQty4 = dr["DefectQty4"].ToString(),
                                DefectName5 = dr["DefectName5"].ToString(),
                                DefectQty5 = dr["DefectQty5"].ToString(),
                                DefectName6 = dr["DefectName6"].ToString(),
                                DefectQty6 = dr["DefectQty6"].ToString(),
                                DefectName7 = dr["DefectName7"].ToString(),
                                DefectQty7 = dr["DefectQty7"].ToString(),
                                DefectName8 = dr["DefectName8"].ToString(),
                                DefectQty8 = dr["DefectQty8"].ToString(),
                                DefectName9 = dr["DefectName9"].ToString(),
                                DefectQty9 = dr["DefectQty9"].ToString(),
                                DefectName10 = dr["DefectName10"].ToString(),
                                DefectQty10 = dr["DefectQty10"].ToString(),
                                DefectName11 = dr["DefectName11"].ToString(),
                                DefectQty11 = dr["DefectQty11"].ToString()
                            };

                            if ((DefectInfo.examdate != "" && DefectInfo.examdate != null))
                            {
                                DefectInfo.examdate = DefectInfo.examdate.ToString().Substring(0, 4) + "-"
                              + DefectInfo.examdate.ToString().Substring(4, 2) + "-"
                              + DefectInfo.examdate.ToString().Substring(6, 2);
                            }

                            //불량 유형은 헤더
                            if (!DefectInfo.DefectName1.Equals(""))
                            {
                                dgdMain.Columns[1].Header = DefectInfo.DefectName1;
                            }
                            if (!DefectInfo.DefectName2.Equals(""))
                            {
                                dgdMain.Columns[2].Header = DefectInfo.DefectName2;
                            }
                            if (!DefectInfo.DefectName3.Equals(""))
                            {
                                dgdMain.Columns[3].Header = DefectInfo.DefectName3;
                            }
                            if (!DefectInfo.DefectName4.Equals(""))
                            {
                                dgdMain.Columns[4].Header = DefectInfo.DefectName4;
                            }
                            if (!DefectInfo.DefectName5.Equals(""))
                            {
                                dgdMain.Columns[5].Header = DefectInfo.DefectName5;
                            }
                            if (!DefectInfo.DefectName6.Equals(""))
                            {
                                dgdMain.Columns[6].Header = DefectInfo.DefectName6;
                            }
                            if (!DefectInfo.DefectName7.Equals(""))
                            {
                                dgdMain.Columns[7].Header = DefectInfo.DefectName7;
                            }
                            if (!DefectInfo.DefectName8.Equals(""))
                            {
                                dgdMain.Columns[8].Header = DefectInfo.DefectName8;
                            }
                            if (!DefectInfo.DefectName9.Equals(""))
                            {
                                dgdMain.Columns[9].Header = DefectInfo.DefectName9;
                            }

                            //수량은 그리드 내용
                            dgdMain.Items.Add(DefectInfo);


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        private void ClearHeader()
        {
            for (int i = 1; i < 10; i++)
            {
                dgdMain.Columns[i].Header = "";
            }
        }

        private void FillGrid2()
        {
            if (dgdMain2.Items.Count > 0)
            {
                dgdMain2.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("nchkDate", chkDate.IsChecked == true ? 1 : 0);
                sqlParameter.Add("StartDate", chkDate.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("EndDate", chkDate.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");

                sqlParameter.Add("nchkArticleID", 1);
                sqlParameter.Add("ArticleID", txtArticle.Text.Trim() == "" ? "" : txtArticle.Tag.ToString());


                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Qul_StsAnalDetailMachineWork_s", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회결과가 없습니다.");
                        return;
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                        int i = 0;
                        foreach (DataRow dr in drc)
                        {
                            i++;
                            var DefectInfo = new Win_Qul_Sts_AnalDetail_Q_CodeView()
                            {
                                Machine = dr["Machine"].ToString(),
                                Machineno = dr["Machineno"].ToString(),
                                WorkPersonID = dr["WorkPersonID"].ToString(),
                                WorkPersoName = dr["WorkPersoName"].ToString(),
                                Kdefect = dr["Kdefect"].ToString(),
                                DefectQty = dr["DefectQty"].ToString()
                            };

                            dgdMain2.Items.Add(DefectInfo);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }
        //검색 조건 Check
        private bool CheckData()
        {
            bool flag = true;

            if (chkArticle.IsChecked == true)
            {
                if (txtArticle.Text == "")
                {
                    MessageBox.Show("품번이 입력 되지 않았습니다. 품번을 선택하고 검색해 주세요.");
                    flag = false;
                    return flag;
                }
            }

            return flag;
        }


        #endregion 조회관련(Fillgrid)

        #region 기타 메소드 
        //특수문자 포함 검색
        private string Escape(string str)
        {
            string result = "";

            for (int i = 0; i < str.Length; i++)
            {
                string txt = str.Substring(i, 1);

                bool isSpecial = Regex.IsMatch(txt, @"[^a-zA-Z0-9가-힣]");

                if (isSpecial == true)
                {
                    result += (@"/" + txt);
                }
                else
                {
                    result += txt;
                }
            }
            return result;
        }

        // 천단위 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }
        #endregion

        private void DataGrid_SizeChange(object sender, SizeChangedEventArgs e)
        {
            DataGrid dgs = sender as DataGrid;
            if (dgs.ColumnHeaderHeight == 0)
            {
                dgs.ColumnHeaderHeight = 1;
            }
            double a = e.NewSize.Height / 100;
            double b = e.PreviousSize.Height / 100;
            double c = a / b;

            if (c != double.PositiveInfinity && c != 0 && double.IsNaN(c) == false)
            {
                dgs.ColumnHeaderHeight = dgs.ColumnHeaderHeight * c;
                dgs.FontSize = dgs.FontSize * c;
            }
        }

        private void chkArticle_Unchecked(object sender, RoutedEventArgs e)
        {
            //txtArticle.Text = "";
            //txtArticle.Tag = "";
            //txtArticle.IsEnabled = false;
            //btnPfArticle.IsEnabled = false;
        }

        //품명 체크 이벤트
        private void ChkArticle_Checked(object sender, RoutedEventArgs e)
        {
            //txtArticle.Text = "";
            //txtArticle.Tag = "";
            //txtArticle.IsEnabled = true;
            //btnPfArticle.IsEnabled = true;
        }

        //날짜 선택시 밸리데이션체크
        private void dtpSDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpSDate.SelectedDate > dtpEDate.SelectedDate)
            {
                MessageBox.Show("종료일자는 시작일 이후로 설정해주세요.");
                dtpSDate.SelectedDate = Convert.ToDateTime(e.RemovedItems[0].ToString());
            }

        }
        //날짜 선택시 밸리데이션체크
        private void dtpEDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpSDate.SelectedDate > dtpEDate.SelectedDate)
            {
                MessageBox.Show("종료일자는 시작일 이후로 설정해주세요.");
                dtpEDate.SelectedDate = Convert.ToDateTime(e.RemovedItems[0].ToString());
            }
        }

    }

    #region 생성자들(CodeView)

    class Win_Qul_Sts_AnalDetail_Q_CodeView : BaseView
    {
        public string examdate { get; set; }
        public string DefectName1 { get; set; }
        public string DefectQty1 { get; set; }
        public string DefectName2 { get; set; }
        public string DefectQty2 { get; set; }
        public string DefectName3 { get; set; }
        public string DefectQty3 { get; set; }
        public string DefectName4 { get; set; }
        public string DefectQty4 { get; set; }
        public string DefectName5 { get; set; }
        public string DefectQty5 { get; set; }
        public string DefectName6 { get; set; }
        public string DefectQty6 { get; set; }
        public string DefectName7 { get; set; }
        public string DefectQty7 { get; set; }
        public string DefectName8 { get; set; }
        public string DefectQty8 { get; set; }
        public string DefectName9 { get; set; }
        public string DefectQty9 { get; set; }
        public string DefectName10 { get; set; }
        public string DefectQty10 { get; set; }
        public string DefectName11 { get; set; }
        public string DefectQty11 { get; set; }
        public string Machine       { get; set; }
        public string Machineno     { get; set; }
        public string WorkPersonID  { get; set; }
        public string WorkPersoName { get; set; }
        public string Kdefect       { get; set; }
        public string DefectQty     { get; set; }
    }

    #endregion 생성자들(CodeView)
}