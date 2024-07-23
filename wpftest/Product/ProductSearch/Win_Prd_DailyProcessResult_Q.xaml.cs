﻿using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;
using WizMes_HanMin.PopUp;
using WizMes_HanMin.PopUP;
using WPF.MDI;

namespace WizMes_HanMin
{
    /// <summary>
    /// Win_Prd_ProcessResult_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_Prd_DailyProcessResult_Q : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        int rowNum = 0;

        public Win_Prd_DailyProcessResult_Q()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            chkDateSrh.IsChecked = true;

            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }

        #region Header 부분 - 검색조건

        // 일자
        private void lblDateSrh_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkDateSrh.IsChecked == true)
            {
                chkDateSrh.IsChecked = false;
            }
            else
            {
                chkDateSrh.IsChecked = true;
            }
        }
        private void chkDateSrh_Checked(object sender, RoutedEventArgs e)
        {
            chkDateSrh.IsChecked = true;
            dtpSDate.IsEnabled = true;
            dtpEDate.IsEnabled = true;

            btnYesterDay.IsEnabled = true;
            btnToday.IsEnabled = true;
            btnLastMonth.IsEnabled = true;
            btnThisMonth.IsEnabled = true;
        }
        private void chkDateSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            chkDateSrh.IsChecked = false;
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;

            btnYesterDay.IsEnabled = false;
            btnToday.IsEnabled = false;
            btnLastMonth.IsEnabled = false;
            btnThisMonth.IsEnabled = false;
        }

        // 전일 금일 전월 금월 버튼
        //금일
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }
        //금월
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];
        }
        //전월
        private void btnLastMonth_Click(object sender, RoutedEventArgs e)
        {
            //dtpSDate.SelectedDate = Lib.Instance.BringLastMonthDatetimeList()[0];
            //dtpEDate.SelectedDate = Lib.Instance.BringLastMonthDatetimeList()[1];

            if (dtpSDate.SelectedDate != null)
            {
                DateTime ThatMonth1 = dtpSDate.SelectedDate.Value.AddDays(-(dtpSDate.SelectedDate.Value.Day - 1)); // 선택한 일자 달의 1일!

                DateTime LastMonth1 = ThatMonth1.AddMonths(-1); // 저번달 1일
                DateTime LastMonth31 = ThatMonth1.AddDays(-1); // 저번달 말일

                dtpSDate.SelectedDate = LastMonth1;
                dtpEDate.SelectedDate = LastMonth31;
            }
            else
            {
                DateTime ThisMonth1 = DateTime.Today.AddDays(-(DateTime.Today.Day - 1)); // 이번달 1일

                DateTime LastMonth1 = ThisMonth1.AddMonths(-1); // 저번달 1일
                DateTime LastMonth31 = ThisMonth1.AddDays(-1); // 저번달 말일

                dtpSDate.SelectedDate = LastMonth1;
                dtpEDate.SelectedDate = LastMonth31;
            }
        }
        //전일
        private void btnYesterday_Click(object sender, RoutedEventArgs e)
        {
            //dtpSDate.SelectedDate = DateTime.Today.AddDays(-1);
            //dtpEDate.SelectedDate = DateTime.Today.AddDays(-1);

            if (dtpSDate.SelectedDate != null)
            {
                dtpSDate.SelectedDate = dtpSDate.SelectedDate.Value.AddDays(-1);
                dtpEDate.SelectedDate = dtpSDate.SelectedDate;
            }
            else
            {
                dtpSDate.SelectedDate = DateTime.Today.AddDays(-1);
                dtpEDate.SelectedDate = DateTime.Today.AddDays(-1);
            }
        }

        // 공정 검색
        private void lblProcess_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkProcess.IsChecked == true)
            {
                chkProcess.IsChecked = false;
            }
            else
            {
                chkProcess.IsChecked = true;
            }
        }
        private void chkProcess_Checked(object sender, RoutedEventArgs e)
        {
            chkProcess.IsChecked = true;
            txtProcess.IsEnabled = true;
            btnPfProcess.IsEnabled = true;
        }
        private void chkProcess_Unchecked(object sender, RoutedEventArgs e)
        {
            chkProcess.IsChecked = false;
            txtProcess.IsEnabled = false;
            btnPfProcess.IsEnabled = false;
        }
        private void txtProcess_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtProcess, (int)Defind_CodeFind.DCF_PROCESS, "");
            }
        }
        private void btnPfProcess_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtProcess, (int)Defind_CodeFind.DCF_PROCESS, "");
        }

        // 작업자 검색
        private void lblPerson_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkPerson.IsChecked == true)
            {
                chkPerson.IsChecked = false;
            }
            else
            {
                chkPerson.IsChecked = true;
            }
        }
        private void chkPerson_Checked(object sender, RoutedEventArgs e)
        {
            chkPerson.IsChecked = true;
            txtPerson.IsEnabled = true;
            btnPfPerson.IsEnabled = true;
        }
        private void chkPerson_Unchecked(object sender, RoutedEventArgs e)
        {
            chkPerson.IsChecked = false;
            txtPerson.IsEnabled = false;
            btnPfPerson.IsEnabled = false;
        }
        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtPerson, (int)Defind_CodeFind.DCF_PERSON, "");
            }
        }
        private void btnPfPerson_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtPerson, (int)Defind_CodeFind.DCF_PERSON, "");
        }

        // 품명 검색
        private void lblArticle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkArticle.IsChecked == true)
            {
                chkArticle.IsChecked = false;
            }
            else
            {
                chkArticle.IsChecked = true;
            }
        }
        private void chkArticle_Checked(object sender, RoutedEventArgs e)
        {
            chkArticle.IsChecked = true;
            txtArticle.IsEnabled = true;
            btnPfArticle.IsEnabled = true;
        }
        private void chkArticle_Unchecked(object sender, RoutedEventArgs e)
        {
            chkArticle.IsChecked = false;
            txtArticle.IsEnabled = false;
            btnPfArticle.IsEnabled = false;
        }
        private void txtArticle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticle, 76, "");
            }
        }
        private void btnPfArticle_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticle, 76, "");
        }

        // 품번
        private void lblBuyerArticleNo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkBuyerArticleNo.IsChecked == true)
            {
                chkBuyerArticleNo.IsChecked = false;
            }
            else
            {
                chkBuyerArticleNo.IsChecked = true;
            }
        }
        private void chkBuyerArticleNo_Checked(object sender, RoutedEventArgs e)
        {
            chkBuyerArticleNo.IsChecked = true;
            txtBuyerArticleNo.IsEnabled = true;
            //btnPfBuyerArticleNo.IsEnabled = true;
        }
        private void chkBuyerArticleNo_Unchecked(object sender, RoutedEventArgs e)
        {
            chkBuyerArticleNo.IsChecked = false;
            txtBuyerArticleNo.IsEnabled = false;
            //btnPfBuyerArticleNo.IsEnabled = false;
        }
        private void txtBuyerArticleNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                rowNum = 0;
                re_search();
            }
        }

        // 공정 검색
        private void lblCustom_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkCustom.IsChecked == true)
            {
                chkCustom.IsChecked = false;
            }
            else
            {
                chkCustom.IsChecked = true;
            }
        }
        private void chkCustom_Checked(object sender, RoutedEventArgs e)
        {
            chkCustom.IsChecked = true;
            txtCustom.IsEnabled = true;
            btnPfCustom.IsEnabled = true;
        }
        private void chkCustom_Unchecked(object sender, RoutedEventArgs e)
        {
            chkCustom.IsChecked = false;
            txtCustom.IsEnabled = false;
            btnPfCustom.IsEnabled = false;
        }
        private void txtCustom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
            }
        }
        private void btnPfCustom_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        // 작업 날짜보기
        private void lblView_WorkStartEndDate_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkView_WorkStartEndDate.IsChecked == true)
            {
                chkView_WorkStartEndDate.IsChecked = false;
            }
            else
            {
                chkView_WorkStartEndDate.IsChecked = true;
            }
        }

        private void chkView_WorkStartEndDate_Checked(object sender, RoutedEventArgs e)
        {
            chkView_WorkStartEndDate.IsChecked = true;

            dgtcWorkStartTime.Visibility = Visibility.Hidden;
            dgtcWorkEndTime.Visibility = Visibility.Hidden;
            dgtcWorkStartDateTime.Visibility = Visibility.Visible;
            dgtcWorkEndDateTime.Visibility = Visibility.Visible;
        }

        private void chkView_WorkStartEndDate_Unchecked(object sender, RoutedEventArgs e)
        {
            chkView_WorkStartEndDate.IsChecked = false;

            dgtcWorkStartTime.Visibility = Visibility.Visible;
            dgtcWorkEndTime.Visibility = Visibility.Visible;
            dgtcWorkStartDateTime.Visibility = Visibility.Hidden;
            dgtcWorkEndDateTime.Visibility = Visibility.Hidden;
        }

        #endregion // Header 부분 - 검색조건

        #region Header 부분 - 오른쪽 버튼 모음 (검색, 닫기, 엑셀)

        // 검색버튼
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSearch.IsEnabled = false;

                using (Loading lw = new Loading(re_search))
                {
                    lw.ShowDialog();
                }
            }
            catch(Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
            finally
            {
                btnSearch.IsEnabled = true;
            }
            
        }


        // 닫기버튼
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        // 엑셀버튼
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[2];
            lst[0] = "일 생산 집계";
            lst[1] = dgdMain.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);

            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdMain.Name))
                {
                    DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdMain);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdMain);

                    Name = dgdMain.Name;

                    if (Lib.Instance.GenerateExcel(dt, Name))
                        Lib.Instance.excel.Visible = true;
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
            }
        }

        #endregion // Header 부분 - 오른쪽 버튼 모음 (검색, 닫기, 엑셀)

        void re_search()
        {
            FillGrid();

            if (dgdMain.Items.Count > 1)
            {
                dgdMain.SelectedIndex = rowNum;
            }
            else
            {
                MessageBox.Show("조회된 데이터가 없습니다.");
                return;
            }
        }

        #region 조회 메서드

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

                sqlParameter.Add("chkDate", chkDateSrh.IsChecked == true? 1: 0);
                sqlParameter.Add("FromDate", dtpSDate.SelectedDate != null ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("ToDate", dtpEDate.SelectedDate != null ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("chkProcessID", chkProcess.IsChecked == true ? 1: 0);
                sqlParameter.Add("ProcessID", txtProcess.Tag != null ? txtProcess.Tag.ToString() : "");

                sqlParameter.Add("chkWorkPersonID", chkPerson.IsChecked == true ? 1 : 0);
                sqlParameter.Add("WorkPersonID", txtPerson.Tag != null ? txtPerson.Tag.ToString() : "");
                sqlParameter.Add("chkArticleID", chkArticle.IsChecked == true ? 1: 0);
                sqlParameter.Add("ArticleID", txtArticle.Tag != null ? txtArticle.Tag.ToString() : "");
                sqlParameter.Add("chkBuyerArticleNo", chkBuyerArticleNo.IsChecked == true ? 1 : 0);

                sqlParameter.Add("BuyerArticleNo", !txtBuyerArticleNo.Text.Trim().Equals("") ? txtBuyerArticleNo.Text : "");
                sqlParameter.Add("chkCustomID", chkCustom.IsChecked == true ? 1 : 0);
                sqlParameter.Add("CustomID", txtCustom.Tag != null ? txtCustom.Tag.ToString() : "");

                //DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_ProdSumPersonArticle_s_20200911", sqlParameter, false);
                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_prd_ProdSumPersonArticle_s_20210517", sqlParameter, true, "R"); //2021-05-19
                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;

                            var WinR = new Win_Prd_DailyProcessResult_Q_CodeView()
                            {
                                Num = i.ToString(),

                                cls = dr["cls"].ToString(),
                                WorkYYYY = dr["WorkYYYY"].ToString(),
                                WorkMM = dr["WorkMM"].ToString(),
                                WorkDD = dr["WorkDD"].ToString(),
                                DayNight = dr["DayNight"].ToString().Trim().Equals("N") ? "야간" : "주간",

                                ProcessID = dr["ProcessID"].ToString(),
                                Process = dr["Process"].ToString(),
                                AutoMCYN = dr["AutoMCYN"].ToString(),
                                MachineNo = dr["MachineNo"].ToString(),
                                WorkPersonName = dr["WorkPersonName"].ToString(),

                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                Article = dr["Article"].ToString(),
                                WorkStartTime = DateTimeFormat(dr["WorkStartTime"].ToString()),
                                WorkEndTime = DateTimeFormat(dr["WorkEndTime"].ToString()),
                              
                                WorkTime = DateTimeMinToTime(dr["WorkTime"].ToString()), // 근무시간
                                IdleTime = DateTimeMinToTime(dr["IdleTime"].ToString()), // 휴식시간
                                RealWorkTime = DateTimeMinToTime(dr["RealWorkTime"].ToString()), // 실제 근무시간
                                RealWorkTime_Num = stringFormatN2(ConvertDouble(dr["RealWorkTime"].ToString()) / 60), // 실제 근무시간

                                CycleTime = stringFormatN1(dr["CycleTime"]),
                                StandardWorkQty = stringFormatNDigit(dr["StandardWorkQty"], 1),

                                GoalQty = stringFormatN0(dr["GoalQty"]),
                                WorkQty = stringFormatN0(dr["WorkQty"]),
                                ProcessRate = stringFormatN0(dr["ProcessRate"]) + "%",
                                OutUnitPrice = stringFormatN0(dr["OutUnitPrice"]),
                                ProcessAmount = stringFormatN0(dr["ProcessAmount"]),

                                NoWorkTime = DateTimeMinToTime(dr["NoWorkTime"].ToString()),

                                WorkStartDateTime = DateTimeFormat2(dr["WorkStartDateTime"].ToString()),
                                WorkEndDateTime = DateTimeFormat2(dr["WorkEndDateTime"].ToString()),
                            };

                            WinR.F_WorkTime = ConvertDouble(dr["WorkTime"].ToString()) + 1000;
                            WinR.F_IdleTime = ConvertDouble(dr["IdleTime"].ToString()) + 1000;
                            WinR.F_CycleTime = ConvertDouble(dr["CycleTime"].ToString()) + 1000;
                            WinR.F_RealWorkTime = ConvertDouble(dr["RealWorkTime"].ToString()) + 1000;
                            WinR.F_StandardWorkQty = ConvertDouble(dr["StandardWorkQty"].ToString()) + 1000;
                            WinR.F_GoalQty = ConvertDouble(dr["GoalQty"].ToString()) + 1000;
                            WinR.F_WorkQty = ConvertDouble(dr["WorkQty"].ToString()) + 1000;
                            WinR.F_ProcessRate = ConvertDouble(dr["ProcessRate"].ToString()) + 1000;
                            WinR.F_OutUnitPrice = ConvertDouble(dr["OutUnitPrice"].ToString()) + 1000;
                            WinR.F_ProcessAmount = ConvertDouble(dr["ProcessAmount"].ToString()) + 1000;

                            if (WinR.cls.Trim().Equals("9"))
                            {
                                WinR.WorkYYYY = "총계";
                                WinR.Total_Color = true;

                                // 근무시간, 휴식시간, 실제근무시간, 시간, CT, 표준수량은 빈값으로
                                WinR.WorkTime = "";
                                WinR.IdleTime = "";
                                WinR.RealWorkTime = "";
                                WinR.RealWorkTime_Num = "";
                                WinR.CycleTime = "";
                                WinR.StandardWorkQty = "";
                                WinR.OutUnitPrice = "";
                                WinR.NoWorkTime = "";

                                WinR.F_WorkTime = 99999999999;
                                WinR.F_IdleTime = 99999999999;
                                WinR.F_CycleTime = 99999999999;
                                WinR.F_RealWorkTime = 99999999999;
                                WinR.F_StandardWorkQty = 99999999999;
                                WinR.F_GoalQty = 99999999999;
                                WinR.F_WorkQty = 99999999999;
                                WinR.F_ProcessRate = 99999999999;
                                WinR.F_OutUnitPrice = 99999999999;
                                WinR.F_ProcessAmount = 99999999999;
                            }

                            dgdMain.Items.Add(WinR);
                        }
                    }

                    tblCnt.Text = " ▶ 검색 결과 : " + (dt.Rows.Count - 1) + " 건";
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

        #endregion // 조회 메서드

        #region 기타 메서드 모음

        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }

        // 천마리 콤마, 소수점 버리기
        private string stringFormatN1(object obj)
        {
            return string.Format("{0:N0}", obj);
        }

        // 천마리 콤마, 소수점 두자리
        private string stringFormatN2(object obj)
        {
            return string.Format("{0:N2}", obj);
        }

        // 천마리 콤마, 소수점 두자리
        private string stringFormatNDigit(object obj, int digit)
        {
            return string.Format("{0:N" + digit + "}", obj);
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

            return result;
        }

        // 시간 형식 6글자라면! 11:11:11
        private string DateTimeFormat(string str)
        {
            str = str.Replace(":", "").Trim();

            if (str.Length == 6)
            {
                string Hour = str.Substring(0, 2);
                string Min = str.Substring(2, 2);
                string Sec = str.Substring(4, 2);

                str = Hour + ":" + Min + ":" + Sec;
            }

            return str;
        }

        // 시간 분 → 11:12 형식으로 변환
        private string DateTimeMinToTime(string str)
        {
            str = str.Replace(":", "").Trim();

            int num = 0;
            if (int.TryParse(str, out num) == true)
            {
                string hour = (num / 60).ToString();
                string min = (num % 60).ToString();

                if (min.Length == 1)
                {
                    min = "0" + min;
                }

                str = hour + ":" + min;
            }

            return str;
        }

        private string DateTimeFormat2(string str)
        {
            if (str == null) { return ""; }

            string result = str;

            str = str.Replace(":", "").Replace("/", "").Replace("-", "").Trim();

            if (str.Length == 14)
            {
                string Date = DatePickerFormat(str.Substring(0, 8));
                string Time = DateTimeFormat(str.Substring(8, 6));

                result = Date + " " + Time;
            }

            return result;
        }

        // Int로 변환
        private int ConvertInt(string str)
        {
            int result = 0;
            int chkInt = 0;

            if (!str.Trim().Equals(""))
            {
                str = str.Replace(",", "");

                if (Int32.TryParse(str, out chkInt) == true)
                {
                    result = Int32.Parse(str);
                }
            }

            return result;
        }

        // 소수로 변환 가능한지 체크 이벤트
        private bool CheckConvertDouble(string str)
        {
            bool flag = false;
            double chkDouble = 0;

            if (!str.Trim().Equals(""))
            {
                if (Double.TryParse(str, out chkDouble) == true)
                {
                    flag = true;
                }
            }

            return flag;
        }

        // 숫자로 변환 가능한지 체크 이벤트
        private bool CheckConvertInt(string str)
        {
            bool flag = false;
            int chkInt = 0;

            if (!str.Trim().Equals(""))
            {
                str = str.Trim().Replace(",", "");

                if (Int32.TryParse(str, out chkInt) == true)
                {
                    flag = true;
                }
            }

            return flag;
        }

        // 소수로 변환
        private double ConvertDouble(string str)
        {
            double result = 0;
            double chkDouble = 0;

            if (!str.Trim().Equals(""))
            {
                str = str.Replace(",", "").Replace(":", "");

                if (Double.TryParse(str, out chkDouble) == true)
                {
                    result = Double.Parse(str);
                }
            }

            return result;
        }


        #endregion

        private void DgdMain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var NoWork = dgdMain.SelectedItem as Win_Prd_DailyProcessResult_Q_CodeView;

                NoWorkInfo NoWorking = null;

                if (NoWork != null)
                {
                    if (ConvertDouble(NoWork.NoWorkTime) == 0)
                        MessageBox.Show("선택된 자료의 비가동 시간을 확인해보세요.");
                    else
                        NoWorking = new NoWorkInfo(NoWork.WorkStartDateTime, NoWork.WorkEndDateTime, NoWork.ProcessID, NoWork.MachineNo, NoWork.WorkPersonName);
                }

                if (NoWorking != null)
                {
                    NoWorking.Topmost = true;
                    NoWorking.Show();
                }
                    
            }
        }

        private void dtpEDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            if (datePicker != null && dtpSDate.SelectedDate > datePicker.SelectedDate)
            {
                MessageBox.Show("종료일자는 시작일 이후로 설정해주세요.");
                dtpEDate.SelectedDate = Convert.ToDateTime(e.RemovedItems[0].ToString());
            }
        }
    }

    #region 메인그리드 코드뷰

    class Win_Prd_DailyProcessResult_Q_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }
        public string Num { get; set; }

        public string cls { get; set; }
        public string WorkYYYY { get; set; }
        public string WorkMM { get; set; }
        public string WorkDD { get; set; }
        public string DayNight { get; set; }

        public string ProcessID { get; set; }
        public string Process { get; set; }
        public string AutoMCYN { get; set; }
        public string MachineNo { get; set; }
        public string WorkPersonName { get; set; } // 작업자

        public string BuyerArticleNo { get; set; }
        public string Article { get; set; }               // 품명  
        public string WorkStartTime { get; set; }   // 작업시작 시간 
        public string WorkEndTime { get; set; }     // 작업종료 시간 
        public string WorkStartTimeDate { get; set; }

        public string WorkEndTimeDate { get; set; }
        public string WorkTime { get; set; }
        public string IdleTime { get; set; } // 휴식시간인듯

        public string RealWorkTime { get; set; } // 실제근무시간
        public string RealWorkTime_Num { get; set; } // 실제근무시간을 숫자로! 10:30 → 10.5

        public string CycleTime { get; set; } // CT
        public string StandardWorkQty { get; set; } // 표준수량  =  시간당 생산가능량  *  시간  

        public string GoalQty { get; set; } // 목표수량  
        public string WorkQty { get; set; }         // 생산수량 
        public string ProcessRate { get; set; }    // 달성률 = 생산수량 / 목표량 * 100
        public string OutUnitPrice { get; set; }    // 가공단가
        public string ProcessAmount { get; set; }   // 금액

        public string NoWorkTime { get; set; }


        public string WorkStartDateTime { get; set; }
        public string WorkEndDateTime { get; set; }

        public bool Total_Color { get; set; }

        public double F_WorkTime { get; set; }
        public double F_IdleTime { get; set; } // 휴식시간인듯
        public double F_CycleTime { get; set; } // CT
        public double F_StandardWorkQty { get; set; } // CT
        public double F_RealWorkTime { get; set; } // 실제근무시간
        public double F_GoalQty { get; set; } // 목표수량  
        public double F_WorkQty { get; set; }         // 생산수량 
        public double F_ProcessRate { get; set; }    // 달성률 = 생산수량 / 목표량 * 100
        public double F_OutUnitPrice { get; set; }    // 가공단가
        public double F_ProcessAmount { get; set; }   // 금액
    }

    #endregion
}