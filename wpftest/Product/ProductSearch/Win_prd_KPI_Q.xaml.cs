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

namespace WizMes_HanMin
{
    /// <summary>
    /// Win_prd_KPI_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_prd_KPI_Q : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        Lib lib = new Lib();
        PlusFinder pf = new PlusFinder();

        int rowNum = 0;

        public Win_prd_KPI_Q()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            lib.UiLoading(sender);
            DatePickerStartDateSearch.SelectedDate = lib.BringThisMonthDatetimeList()[0];
            DatePickerEndDateSearch.SelectedDate = lib.BringThisMonthDatetimeList()[1];
        }

        #region 상단 검색조건
        //전년
        private void ButtonLastYear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DatePickerStartDateSearch.SelectedDate != null)
                {
                    DatePickerStartDateSearch.SelectedDate = DatePickerStartDateSearch.SelectedDate.Value.AddYears(-1);
                }
                else
                {
                    DatePickerStartDateSearch.SelectedDate = DateTime.Today.AddDays(-1);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //전월
        private void ButtonLastMonth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DatePickerStartDateSearch.SelectedDate != null)
                {
                    DateTime FirstDayOfMonth = DatePickerStartDateSearch.SelectedDate.Value.AddDays(-(DatePickerStartDateSearch.SelectedDate.Value.Day - 1));
                    DateTime FirstDayOfLastMonth = FirstDayOfMonth.AddMonths(-1);

                    DatePickerStartDateSearch.SelectedDate = FirstDayOfLastMonth;
                }
                else
                {
                    DateTime FirstDayOfMonth = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));

                    DatePickerStartDateSearch.SelectedDate = FirstDayOfMonth;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //금년
        private void ButtonThisYear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DatePickerStartDateSearch.SelectedDate != null)
                {
                    DatePickerStartDateSearch.SelectedDate = lib.BringThisYearDatetimeFormat()[0];
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //금월
        private void ButtonThisMonth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DatePickerStartDateSearch.SelectedDate = lib.BringThisMonthDatetimeList()[0];
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        #endregion

        #region Re_Search
        private void re_Search(int selectedIndex)
        {
            try
            {
                if (dgdOut.Items.Count > 0)
                {
                    dgdOut.Items.Clear();
                }

                if (dgdGonsu.Items.Count > 0)
                {
                    dgdGonsu.Items.Clear();
                }

                if (dgdQ.Items.Count > 0)
                {
                    dgdQ.Items.Clear();
                }

                FillGrid();

            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        #endregion

        #region 공수조회
        private void FillGrid()
        {
            try
            {
                if (dgdOut.Items.Count > 0)
                {
                    dgdOut.Items.Clear();
                }
                if (dgdGonsu.Items.Count > 0)
                {
                    dgdGonsu.Items.Clear();
                }
                if (dgdQ.Items.Count > 0)
                {
                    dgdQ.Items.Clear();
                }

                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("sFromDate", DatePickerStartDateSearch.SelectedDate == null ? "" : DatePickerStartDateSearch.SelectedDate.Value.ToString().Replace("-", ""));
                sqlParameter.Add("sToDate", DatePickerEndDateSearch.SelectedDate == null ? "" : DatePickerEndDateSearch.SelectedDate.Value.ToString().Replace("-", ""));
                sqlParameter.Add("ArticleID", chkArticleNo.IsChecked == true && txtArticleNoSearch.Tag != null ? txtArticleNoSearch.Tag.ToString() : ""); 
                ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_prd_sKPI_KPI", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0]; 
                    int i = 0;

                    if (dt.Rows.Count == 0)
                    {

                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WPKQC = new Win_prd_KPI_Q_CodeView()
                            {
                                Num = i + 1,

                                KPIKey = dr["KPIKey"].ToString(),
                                MonthWorkHour = dr["MonthWorkHour"].ToString(),
                                MonthWorkQty = dr["MonthWorkQty"].ToString(),

                                MonthUPH = dr["MonthUPH"].ToString(),
                                WorkQty = dr["WorkQty"].ToString(),
                                DefectQty = dr["DefectQty"].ToString(),
                                DefectRate = dr["DefectRate"].ToString(),
                                McSettingHour = dr["McSettingHour"].ToString(),
                                MtrYongGiHour = dr["MtrYongGiHour"].ToString(),
                                PackingHour = dr["PackingHour"].ToString(),
                                WorkHour = dr["WorkHour"].ToString(),
                                WorkDate = dr["WorkDate"].ToString(),

                            };

                            //WPKQC.Gonsu = lib.returnNumStringZero(WPKQC.Gonsu);
                            //WPKQC.OrderQty = lib.returnNumStringZero(WPKQC.OrderQty);
                            //WPKQC.DiffOutDayPerQty = lib.returnNumStringZero(WPKQC.DiffOutDayPerQty);
                            
                            WPKQC.MonthWorkHour = lib.returnNumStringOne(WPKQC.MonthWorkHour);
                            WPKQC.MonthWorkQty = lib.returnNumStringZero(WPKQC.MonthWorkQty);
                            WPKQC.MonthUPH = lib.returnNumStringZero(WPKQC.MonthUPH);
                            WPKQC.WorkQty = lib.returnNumStringZero(WPKQC.WorkQty);
                            WPKQC.DefectQty = lib.returnNumStringZero(WPKQC.DefectQty);
                            WPKQC.DefectRate = lib.returnNumStringZero(WPKQC.DefectRate);
                            WPKQC.McSettingHour = lib.returnNumStringZero(WPKQC.McSettingHour);
                            WPKQC.MtrYongGiHour = lib.returnNumStringZero(WPKQC.MtrYongGiHour);
                            WPKQC.PackingHour = lib.returnNumStringZero(WPKQC.PackingHour);
                            WPKQC.WorkHour = lib.returnNumStringZero(WPKQC.WorkHour);
                            WPKQC.WorkDate = DatePickerFormat(WPKQC.WorkDate);

                            if (WPKQC.KPIKey == "P")
                            {
                                dgdGonsu.Items.Add(WPKQC);
                            }

                            if (WPKQC.KPIKey == "Q")
                            {
                                dgdQ.Items.Add(WPKQC);
                            }

                            if (WPKQC.KPIKey == "D")
                            {
                                dgdOut.Items.Add(WPKQC);
                            }

                            

                            i++;
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }
        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                rowNum = 0;
                re_Search(rowNum);

            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        private void btiClose_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            try
            {
                lib.ChildMenuClose(this.ToString());
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        private void btiExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if(dgdOut.Items.Count == 0 && dgdGonsu.Items.Count == 0)
                //{
                //    MessageBox.Show("먼저 검색해 주세요.");
                //    return;
                //}

                DataTable dt = null;
                string Name = string.Empty;

                string[] lst = new string[4];
                lst[0] = "KPI작업공수";
                lst[1] = "KPI납기";
                lst[2] = dgdGonsu.Name;
                lst[3] = dgdOut.Name;

                ExportExcelxaml ExpExc = new ExportExcelxaml(lst);
                ExpExc.ShowDialog();

                if (ExpExc.DialogResult.HasValue)
                {
                    if (ExpExc.choice.Equals(dgdGonsu.Name))
                    {
                        DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");
                        if (ExpExc.Check.Equals("Y"))
                            dt = Lib.Instance.DataGridToDTinHidden(dgdGonsu);
                        else
                            dt = Lib.Instance.DataGirdToDataTable(dgdGonsu);

                        Name = dgdGonsu.Name;
                        Lib.Instance.GenerateExcel(dt, Name);
                        Lib.Instance.excel.Visible = true;
                    }
                    else if (ExpExc.choice.Equals(dgdOut.Name))
                    {
                        if (ExpExc.Check.Equals("Y"))
                            dt = Lib.Instance.DataGridToDTinHidden(dgdOut);
                        else
                            dt = Lib.Instance.DataGirdToDataTable(dgdOut);

                        Name = dgdOut.Name;
                        Lib.Instance.GenerateExcel(dt, Name);
                        Lib.Instance.excel.Visible = true;
                    }
                    else
                    {
                        if (dt != null)
                        {
                            dt.Clear();
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        private void lblArticleNo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticleNo.IsChecked == true)
            {
                chkArticleNo.IsChecked = false;
            }
            else
            {
                chkArticleNo.IsChecked = true;
            }
        }
        // 거래처 체크박스 이벤트
        private void chkArticleNo_Checked(object sender, RoutedEventArgs e)
        {
            chkArticleNo.IsChecked = true;
            txtArticleNoSearch.IsEnabled = true;
            btnArticleNoSearch.IsEnabled = true;
        }
        private void chkArticleNo_UnChecked(object sender, RoutedEventArgs e)
        {
            chkArticleNo.IsChecked = false;
            txtArticleNoSearch.IsEnabled = false;
            btnArticleNoSearch.IsEnabled = false;
        }
        // 거래처 텍스트박스 엔터 → 플러스파인더
        private void txtArticleNoSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleNoSearch, 76, "");
            }
        }
        // 거래처 플러스파인더 이벤트
        private void btnArticleNoSearch_Click(object sender, RoutedEventArgs e)
        {
            // 거래처 : 0
            MainWindow.pf.ReturnCode(txtArticleNoSearch, 76, "");
        }

        //품명 라벨 클릭
        private void LabelArticleSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(CheckBoxArticleSearch.IsChecked == true)
            {
                CheckBoxArticleSearch.IsChecked = false;
            }
            else
            {
                CheckBoxArticleSearch.IsChecked = true;
            }
        }

        private void CheckBoxArticleSearch_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxArticleSearch.IsEnabled = true;
            ButtonArticleSearch.IsEnabled = true;
        }

        private void CheckBoxArticleSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxArticleSearch.IsEnabled = false;
            ButtonArticleSearch.IsEnabled = false;
        }

        private void TextBoxArticleSearch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if(e.Key == Key.Enter)
                {
                    pf.ReturnCode(TextBoxArticleSearch, 77, TextBoxArticleSearch.Text);
                }
            }
            catch(Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
        }

        private void ButtonArticleSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pf.ReturnCode(TextBoxArticleSearch, 77, TextBoxArticleSearch.Text);
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
        }

        // 데이터피커 포맷으로 변경
        private string DatePickerFormat(string str)
        {
            string result = "";

            if (str.Length == 8)
            {
                if (!str.Trim().Equals(""))
                {
                    result = str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2);
                }
            }
            else
            {
                result = "합     계";
            }
            return result;
        }

    }

    #region CodeView
    class Win_prd_KPI_Q_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }

        public string KPIKey { get; set; }
        public string MonthWorkHour { get; set; }
        public string MonthWorkQty { get; set; }
        public string MonthUPH { get; set; }
        public string WorkQty { get; set; }
        public string DefectQty { get; set; }
        public string DefectRate { get; set; }
        public string McSettingHour { get; set; }
        public string MtrYongGiHour { get; set; }
        public string PackingHour { get; set; }
        public string WorkHour { get; set; }
        public string WorkDate { get; set; }
        public string WorkDate_CV { get; set; }
        
    }

    #endregion

}