﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WizMes_HanMin.PopUp;
using WizMes_HanMin.PopUP;
using WPF.MDI;

namespace WizMes_HanMin
{
    /// <summary>
    /// Win_Prd_ProcessResultSum_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_Prd_ProcessResultSum_Q : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        Lib lib = new Lib();
        PlusFinder pf = MainWindow.pf;

        public Win_Prd_ProcessResultSum_Q()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            Lib.Instance.UiLoading(sender);
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;

            SetComboBox();

            cboProcess.SelectedIndex = 0;
            cboMachine.SelectedIndex = 0;

            rbnOrderID.IsChecked = true;
        }

        private void SetComboBox()
        {
            ObservableCollection<CodeView> cbWork = ComboBoxUtil.Instance.GetWorkProcess(0, "");

            this.cboProcess.ItemsSource = cbWork;
            this.cboProcess.DisplayMemberPath = "code_name";
            this.cboProcess.SelectedValuePath = "code_id";

            ObservableCollection<CodeView> ovcMachine = GetMachineByProcessID("");
            this.cboMachine.ItemsSource = ovcMachine;
            this.cboMachine.DisplayMemberPath = "code_name";
            this.cboMachine.SelectedValuePath = "code_id";
        }

        #region mt_Machine - 호기 세팅

        /// <summary>
        /// 호기ID 가져오기
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ObservableCollection<CodeView> GetMachineByProcessID(string value)
        {
            ObservableCollection<CodeView> ovcMachine = new ObservableCollection<CodeView>();

            Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
            sqlParameter.Add("sProcessID", value);

            DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sMachineForComboBox", sqlParameter, false);

            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    CodeView CV = new CodeView();
                    CV.code_id = "";
                    CV.code_name = "전체";
                    ovcMachine.Add(CV);

                    DataRowCollection drc = dt.Rows;

                    foreach (DataRow dr in drc)
                    {
                        CodeView mCodeView = new CodeView()
                        {
                            code_id = dr["Code"].ToString().Trim(),
                            code_name = dr["Name"].ToString().Trim()
                        };

                        ovcMachine.Add(mCodeView);
                    }
                }
            }

            return ovcMachine;
        }

        #endregion // mt_Machine - 호기 세팅

        private void cboProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboProcess.SelectedValue != null)
            {
                ObservableCollection<CodeView> ovcMachine = GetMachineByProcessID(cboProcess.SelectedValue.ToString());
                this.cboMachine.ItemsSource = ovcMachine;
                this.cboMachine.DisplayMemberPath = "code_name";
                this.cboMachine.SelectedValuePath = "code_id";

                cboMachine.SelectedIndex = 0;
            }
        }

        #region 날짜 관련 이벤트

        //전월
        private void btnLastMonth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - btnLastMonth_Click : " + ee.ToString());
            }
        }

        //금월
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];
        }

        //전일
        private void btnYesterday_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - btnYesterday_Click : " + ee.ToString());
            }
        }

        //금일
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }

        #endregion

        #region 체크 등 이벤트

        //작업자
        private void lblPerson_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkPerson.IsChecked == true) { chkPerson.IsChecked = false; }
            else { chkPerson.IsChecked = true; }
        }

        //작업자
        private void chkPerson_Checked(object sender, RoutedEventArgs e)
        {
            txtPerson.IsEnabled = true;
            txtPerson.Focus();
        }

        //작업자
        private void chkPerson_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPerson.IsEnabled = false;
        }

        //관리번호
        private void rbnOrderNo_Click(object sender, RoutedEventArgs e)
        {
            if (rbnOrderNo.IsChecked == true)
            {
                tbkOrder.Text = " Order No.";
            }
        }

        //관리번호
        private void rbnOrderID_Click(object sender, RoutedEventArgs e)
        {
            if (rbnOrderID.IsChecked == true)
            {
                tbkOrder.Text = " 관리번호";
            }
        }

        //관리번호
        private void lblOrder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkOrder.IsChecked == true) { chkOrder.IsChecked = false; }
            else { chkOrder.IsChecked = true; }
        }

        //관리번호
        private void chkOrder_Checked(object sender, RoutedEventArgs e)
        {
            txtOrder.IsEnabled = true;
            btnPfOrder.IsEnabled = true;
            txtOrder.Focus();
        }

        //관리번호
        private void chkOrder_Unchecked(object sender, RoutedEventArgs e)
        {
            txtOrder.IsEnabled = false;
            btnPfOrder.IsEnabled = false;
        }

        //관리번호
        private void txtOrder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtOrder, (int)Defind_CodeFind.DCF_ORDER, "");
            }
        }

        //관리번호
        private void btnPfOrder_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtOrder, (int)Defind_CodeFind.DCF_ORDER, "");
        }

        //거래처
        private void lblCustom_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCustom.IsChecked == true) { chkCustom.IsChecked = false; }
            else { chkCustom.IsChecked = true; }
        }

        //거래처
        private void chkCustom_Checked(object sender, RoutedEventArgs e)
        {
            txtCustom.IsEnabled = true;
            btnPfCustom.IsEnabled = true;
            txtCustom.Focus();
        }

        private void chkCustom_Unchecked(object sender, RoutedEventArgs e)
        {
            txtCustom.IsEnabled = false;
            btnPfCustom.IsEnabled = false;
        }

        //거래처
        private void txtCustom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //pf.ReturnCode(txtCustom, 0, "");
                MainWindow.pf.ReturnCode(txtCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
            }
        }

        //거래처
        private void btnPfCustom_Click(object sender, RoutedEventArgs e)
        {
            //pf.ReturnCode(txtCustom, 0, "");
            MainWindow.pf.ReturnCode(txtCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //품번
        private void lblArticle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CheckBoxBuyerArticleNoSearch.IsChecked == true) { CheckBoxBuyerArticleNoSearch.IsChecked = false; }
            else { CheckBoxBuyerArticleNoSearch.IsChecked = true; }
        }

        //품번
        private void chkArticle_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxBuyerArticleNoSearch.IsEnabled = true;
            ButtonBuyerArticleNoSearch.IsEnabled = true;
            TextBoxBuyerArticleNoSearch.Focus();
        }

        //품번
        private void chkArticle_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxBuyerArticleNoSearch.IsEnabled = false;
            ButtonBuyerArticleNoSearch.IsEnabled = false;
        }

        //품번
        private void txtArticle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(TextBoxBuyerArticleNoSearch, 76, "");
            }
        }

        //품번
        private void btnPfArticle_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(TextBoxBuyerArticleNoSearch, 76, "");
        }

        #endregion

        //조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataStore.Instance.InsertLogByForm(this.GetType().Name, "R");
                btnSearch.IsEnabled = false;

                using (Loading lw = new Loading(re_Search))
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

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        #region 엑셀 버튼 이벤트
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            TabItem nowTab = tabconGrid.SelectedItem as TabItem;

            if (nowTab.Header.ToString().Equals("공정별 호기별 집계"))
            {
                string[] lst = new string[2];
                lst[0] = "공정별 호기별 집계";
                lst[1] = dgdByProcess.Name;

                ExportExcelxaml ExpExc = new ExportExcelxaml(lst);

                ExpExc.ShowDialog();

                if (ExpExc.DialogResult.HasValue)
                {
                    if (ExpExc.choice.Equals(dgdByProcess.Name))
                    {
                        DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");
                        if (ExpExc.Check.Equals("Y"))
                            dt = Lib.Instance.DataGridToDTinHidden(dgdByProcess);
                        else
                            dt = Lib.Instance.DataGirdToDataTable(dgdByProcess);

                        Name = dgdByProcess.Name;

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
            else if (nowTab.Header.ToString().Equals("품번별 집계"))
            {
                string[] lst = new string[2];
                lst[0] = "품번별 집계";
                lst[1] = dgdByArticle.Name;

                ExportExcelxaml ExpExc = new ExportExcelxaml(lst);

                ExpExc.ShowDialog();

                if (ExpExc.DialogResult.HasValue)
                {
                    if (ExpExc.choice.Equals(dgdByArticle.Name))
                    {
                        if (ExpExc.Check.Equals("Y"))
                            dt = Lib.Instance.DataGridToDTinHidden(dgdByArticle);
                        else
                            dt = Lib.Instance.DataGirdToDataTable(dgdByArticle);

                        Name = dgdByArticle.Name;

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
            else if (nowTab.Header.ToString().Equals("작업자별 집계"))
            {
                string[] lst = new string[2];
                lst[0] = "작업자별 집계";
                lst[1] = dgdByWorker.Name;

                ExportExcelxaml ExpExc = new ExportExcelxaml(lst);

                ExpExc.ShowDialog();

                if (ExpExc.DialogResult.HasValue)
                {
                    if (ExpExc.choice.Equals(dgdByWorker.Name))
                    {
                        if (ExpExc.Check.Equals("Y"))
                            dt = Lib.Instance.DataGridToDTinHidden(dgdByWorker);
                        else
                            dt = Lib.Instance.DataGirdToDataTable(dgdByWorker);

                        Name = dgdByWorker.Name;

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
            else if(nowTab.Header.ToString().Equals("일별 집계"))
            {
                string[] lst = new string[2];
                lst[0] = "일별 집계";
                lst[1] = DataGridThisMonth.Name;

                ExportExcelxaml ExpExc = new ExportExcelxaml(lst);

                ExpExc.ShowDialog();

                if (ExpExc.DialogResult.HasValue)
                {
                    if (ExpExc.choice.Equals(DataGridThisMonth.Name))
                    {
                        if (ExpExc.Check.Equals("Y"))
                            dt = Lib.Instance.DataGridToDTinHidden(DataGridThisMonth);
                        else
                            dt = Lib.Instance.DataGirdToDataTable(DataGridThisMonth);

                        Name = DataGridThisMonth.Name;

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

            lib.ReleaseExcelObject(lib.excel);

        }
        #endregion // 엑셀 버튼 이벤트

        private bool CheckData()
        {
            bool flag = true;

            if (cboProcess.SelectedValue == null)
            {
                MessageBox.Show("공정이 선택되지 않았습니다. 선택해주세요");
                flag = false;
                return flag;
            }

            if (cboMachine.SelectedValue == null)
            {
                MessageBox.Show("호기가 선택되지 않았습니다. 선택해주세요");
                flag = false;
                return flag;
            }

            return flag;
        }

        private void re_Search()
        {
            if (CheckData())
            {
                TabItem nowTab = tabconGrid.SelectedItem as TabItem;

                if (nowTab.Header.ToString().Equals("공정별 호기별 집계"))
                {
                    FillGridProcessMachine();

                    if (dgdByProcess.Items.Count == 1)
                    {
                        MessageBox.Show("조회된 데이터가 없습니다.");
                        return;
                    }
                }
                else if (nowTab.Header.ToString().Equals("품번별 집계")) //2021-06-10 HanMin는 품명을 품번으로 변경하여 사용
                {
                    FillGridArticle();

                    if (dgdByArticle.Items.Count == 1)
                    {
                        MessageBox.Show("조회된 데이터가 없습니다.");
                        return;
                    }
                }
                else if (nowTab.Header.ToString().Equals("작업자별 집계"))
                {
                    FillGridWorker();

                    if (dgdByWorker.Items.Count == 1)
                    {
                        MessageBox.Show("조회된 데이터가 없습니다.");
                        return;
                    }
                }
                else if(nowTab.Header.ToString().Equals("일별 집계"))
                {
                    FillGrid_ThisMonth();

                    //if(DataGridThisMonth.Items.Count == 1)
                    //{

                    //}
                }
            }
        }

        #region 주요 메서드 - 공정별 호기별 집계 조회 FillGridProcessMachine
        private void FillGridProcessMachine()
        {
            dgdByProcessTotal.Items.Clear();

            if (dgdByProcess.Items.Count > 0)
            {
                dgdByProcess.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("sFromDate", dtpSDate.SelectedDate != null ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("sToDate", dtpEDate.SelectedDate != null ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("sProcessIDS", cboProcess.SelectedValue != null ? cboProcess.SelectedValue.ToString() : "");
                sqlParameter.Add("sMachineIDS", cboMachine.SelectedValue != null ? cboMachine.SelectedValue.ToString() : "");
                sqlParameter.Add("ArticleID", CheckBoxArticleSearch.IsChecked == true && TextBoxArticleSearch.Tag != null ? TextBoxArticleSearch.Tag.ToString() : "");
                sqlParameter.Add("CustomID", chkCustom.IsChecked == true && txtCustom.Tag != null ? txtCustom.Tag.ToString() : "");
                sqlParameter.Add("nOrderID", chkOrder.IsChecked == true ? (rbnOrderID.IsChecked == true ? 1 : 2) : 0);
                sqlParameter.Add("sOrderID", chkOrder.IsChecked == true ? (rbnOrderID.IsChecked == true ? txtOrder.Tag.ToString() : txtOrder.Text) : "");
                sqlParameter.Add("sWorker", chkPerson.IsChecked == true ? txtPerson.Text : "");
                sqlParameter.Add("nBuySaleMainYN", chkMainItem.IsChecked == true ? 1 : 0);
                sqlParameter.Add("BuyerArticleNoID", CheckBoxBuyerArticleNoSearch.IsChecked == true && TextBoxBuyerArticleNoSearch.Tag != null ? TextBoxBuyerArticleNoSearch.Tag.ToString() : "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sWKResultByProcessMachine", sqlParameter, false);

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
                            var WinM = new Win_Prd_ProcessResultSum_Q_ByProcessMachine()
                            {
                                cls = dr["cls"].ToString(),
                                ProcessID = dr["ProcessID"].ToString(),
                                Process = dr["Process"].ToString(),
                                BuyerModel = dr["BuyerModel"].ToString(),
                                MachineID = dr["MachineID"].ToString(),
                                MachineNo = dr["MachineNo"].ToString(),
                                Article = dr["Article"].ToString(),
                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                KCustom = dr["KCustom"].ToString(),
                                QtyPerBox = stringFormatN0(dr["QtyPerBox"]),
                                WorkQty = stringFormatN0(dr["WorkQty"]),
                                UnitPrice = stringFormatN0(dr["UnitPrice"]),
                                Amount = stringFormatN0(dr["Amount"]),
                                WorkTime = stringFormatN0(dr["WorkTime"]),
                                Num = i
                            };

                            if (WinM.cls.Equals("2")) // 호기계
                            {
                                WinM.BuyerModel = "호기계";

                                WinM.QtyPerBox = "";
                            }
                            else if (WinM.cls.Equals("3")) // 공정계
                            {
                                WinM.MachineNo = "공정계";

                                WinM.QtyPerBox = "";
                            }
                            else if (WinM.cls.Equals("9")) // 총계
                            {
                                WinM.Process = "총계";

                                WinM.QtyPerBox = "";

                                dgdByProcessTotal.Items.Add(WinM);
                            }

                            dgdByProcess.Items.Add(WinM);
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

        #endregion // 공정별 호기별 집계

        #region 주요 메서드 - 품명별 집계 조회 FillGridArticle
        private void FillGridArticle()  //2021-06-10 HanMin는 품번별로 변경
        {
            dgdByArticleTotal.Items.Clear();

            if (dgdByArticle.Items.Count > 0)
            {
                dgdByArticle.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("sFromDate", dtpSDate.SelectedDate != null ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("sToDate", dtpEDate.SelectedDate != null ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("sProcessIDS", cboProcess.SelectedValue != null ? cboProcess.SelectedValue.ToString() : "");
                sqlParameter.Add("sMachineIDS", cboMachine.SelectedValue != null ? cboMachine.SelectedValue.ToString() : "");
                sqlParameter.Add("ArticleID", CheckBoxArticleSearch.IsChecked == true && TextBoxArticleSearch.Tag != null ? TextBoxArticleSearch.Tag.ToString() : "");
                sqlParameter.Add("CustomID", chkCustom.IsChecked == true && txtCustom.Tag != null ? txtCustom.Tag.ToString() : "");
                sqlParameter.Add("nOrderID", chkOrder.IsChecked == true ? (rbnOrderID.IsChecked == true ? 1 : 2) : 0);
                sqlParameter.Add("sOrderID", chkOrder.IsChecked == true ? (rbnOrderID.IsChecked == true ? txtOrder.Tag.ToString() : txtOrder.Text) : "");
                sqlParameter.Add("sWorker", chkPerson.IsChecked == true ? txtPerson.Text : "");
                sqlParameter.Add("nBuySaleMainYN", chkMainItem.IsChecked == true ? 1 : 0);
                sqlParameter.Add("BuyerArticleNoID", CheckBoxBuyerArticleNoSearch.IsChecked == true && TextBoxBuyerArticleNoSearch.Tag != null ? TextBoxBuyerArticleNoSearch.Tag.ToString() : "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sWKResultByArticle", sqlParameter, false);

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
                            var WinA = new Win_Prd_ProcessResultSum_Q_ByArticle()
                            {
                                Num = i,
                                cls = dr["cls"].ToString(),
                                CustomID = dr["CustomID"].ToString(),
                                KCustom = dr["KCustom"].ToString(),
                                ArticleID = dr["ArticleID"].ToString(),
                                Article = dr["Article"].ToString(),
                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                ProcessID = dr["ProcessID"].ToString(),
                                Process = dr["Process"].ToString(),
                                BuyerModelID = dr["BuyerModelID"].ToString(),
                                Model = dr["Model"].ToString(),
                                WorkQty = stringFormatN0(dr["WorkQty"]),
                                ProdQtyPerBox = stringFormatN0(dr["ProdQtyPerBox"]),
                                
                            };

                            if (WinA.cls.Trim().Equals("3")) // 공정계
                            {
                                WinA.BuyerArticleNo = "공정계";
                            }
                            else if (WinA.cls.Trim().Equals("9")) // 총계
                            {
                                WinA.Process = "총계";
                                WinA.BuyerArticleNo  = "";
                                dgdByArticleTotal.Items.Add(WinA);
                            }

                            dgdByArticle.Items.Add(WinA);
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
        #endregion // 품명별 집계

        #region 주요 메서드 - 작업자별 집계 조회 FillGridWorker

        private void FillGridWorker()
        {
            dgdByWorkerTotal.Items.Clear();

            if (dgdByWorker.Items.Count > 0)
            {
                dgdByWorker.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("sFromDate", dtpSDate.SelectedDate != null ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("sToDate", dtpEDate.SelectedDate != null ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("sProcessIDS", cboProcess.SelectedValue != null ? cboProcess.SelectedValue.ToString() : "");
                sqlParameter.Add("sMachineIDS", cboMachine.SelectedValue != null ? cboMachine.SelectedValue.ToString() : "");
                sqlParameter.Add("ArticleID", CheckBoxArticleSearch.IsChecked == true && TextBoxArticleSearch.Tag != null ? TextBoxArticleSearch.Tag.ToString() : "");
                sqlParameter.Add("CustomID", chkCustom.IsChecked == true && txtCustom.Tag != null ? txtCustom.Tag.ToString() : "");
                sqlParameter.Add("nOrderID", chkOrder.IsChecked == true ? (rbnOrderID.IsChecked == true ? 1 : 2) : 0);
                sqlParameter.Add("sOrderID", chkOrder.IsChecked == true ? (rbnOrderID.IsChecked == true ? txtOrder.Tag.ToString() : txtOrder.Text) : "");
                sqlParameter.Add("sWorker", chkPerson.IsChecked == true ? txtPerson.Text : "");
                sqlParameter.Add("nBuySaleMainYN", chkMainItem.IsChecked == true ? 1 : 0);
                sqlParameter.Add("BuyerArticleNoID", CheckBoxBuyerArticleNoSearch.IsChecked == true && TextBoxBuyerArticleNoSearch.Tag != null ? TextBoxBuyerArticleNoSearch.Tag.ToString() : "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sWKResultByWorker", sqlParameter, false);

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
                            var WinW = new Win_Prd_ProcessResultSum_Q_ByWorker()
                            {
                                Num = i,
                                cls = dr["cls"].ToString().Trim(),

                                WorkPersonID = dr["WorkPersonID"].ToString(),
                                Name = dr["Name"].ToString(),
                                ProcessID = dr["ProcessID"].ToString(),
                                Process = dr["Process"].ToString(),
                                MachineID = dr["MachineID"].ToString(),

                                Machine = dr["Machine"].ToString(),
                                MachineNo = dr["MachineNo"].ToString(),
                                BuyerModelID = dr["BuyerModelID"].ToString(),
                                Model = dr["Model"].ToString(),
                                ArticleID = dr["ArticleID"].ToString(),

                                Article = dr["Article"].ToString(),
                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                CustomID = dr["CustomID"].ToString(),
                                KCustom = dr["KCustom"].ToString(),
                                WorkQty = stringFormatN0(dr["WorkQty"]),

                                ProdQtyPerBox = stringFormatN0(dr["ProdQtyPerBox"]),
                                
                            };

                            if (WinW.cls.Trim().Equals("3")) // 작업자계
                            {
                                WinW.Process = "작업자계";
                            }
                            else if (WinW.cls.Trim().Equals("9")) // 총계
                            {
                                WinW.Process = "총계";
                                WinW.Name = "";

                                dgdByWorkerTotal.Items.Add(WinW);
                            }

                            dgdByWorker.Items.Add(WinW);
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

        #endregion // 주요 메서드 - 작업자별 집계 조회 FillGridWorker

        #region 조회 일자별 집계
        private void FillGrid_ThisMonth()
        {
            if(DataGridThisMonth.Items.Count > 0)
            {
                DataGridThisMonth.Items.Clear();
            }

            //int chkDate = 0;
            //string sFromDate = string.Empty;
            //string sToDate = string.Empty;
            //int chkProcessID = 0;
            //string sProcessID = string.Empty;
            //int chkMachineID = 0;
            //string sMachineID = string.Empty;
            //int chkWorker = 0;
            //string sWorker = string.Empty;
            //int chkOrderID = 0;
            //string sOrderID = string.Empty;
            //int chkCustomID = 0;
            //string sCustomID = string.Empty;
            //int chkArticleID = 0;
            //string sArticleID = string.Empty;
            //int chkBuySaleMainYN = 0;
            //int chkBuyerArticleNo = 0;
            //string buyerArticleNo = string.Empty;


            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("ChkDate", chkDateSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("sFromDate", chkDateSrh.IsChecked == true ? (dtpSDate.SelectedDate == null ? "" : dtpSDate.SelectedDate.Value.ToString("yyyyMMdd")) : "");
                sqlParameter.Add("sToDate", chkDateSrh.IsChecked == true ? (dtpEDate.SelectedDate == null ? "" : dtpEDate.SelectedDate.Value.ToString("yyyyMMdd")) : "");
                sqlParameter.Add("ChkProcessID",  CheckBoxProcessSearch.IsChecked == true && cboProcess.SelectedValue.ToString() != "" ? 1 : 0);
                sqlParameter.Add("sProcessID", CheckBoxProcessSearch.IsChecked == true ? (cboProcess.SelectedValue == null ? "" : cboProcess.SelectedValue.ToString()) : "");
                sqlParameter.Add("ChkMachineID", CheckBoxMachineSearch.IsChecked == true && cboMachine.SelectedValue.ToString() != "" ? 1 : 0);
                sqlParameter.Add("sMachineID", CheckBoxMachineSearch.IsChecked == true ? (cboMachine.SelectedValue == null ? "" : cboMachine.SelectedValue.ToString()) : "");
                sqlParameter.Add("ChkWorker", chkPerson.IsChecked == true ? 1 : 0);
                sqlParameter.Add("sWorker", chkPerson.IsChecked == true ? (txtPerson.Text == string.Empty ? "" : txtPerson.Text) : "");
                sqlParameter.Add("ChkOrderID", chkOrder.IsChecked == true ? 1 : 0);
                sqlParameter.Add("sOrderID", chkOrder.IsChecked == true ? (txtOrder.Tag == null ? "" : txtOrder.Tag.ToString()) : "");
                sqlParameter.Add("ChkCustomID", chkCustom.IsChecked == true ? 1 : 0);
                sqlParameter.Add("sCustomID", chkCustom.IsChecked == true ? (txtCustom.Tag == null ? "" : txtCustom.Tag.ToString()) : "");
                sqlParameter.Add("ChkArticleID", CheckBoxArticleSearch.IsChecked == true ? 1 : 0); 
                sqlParameter.Add("sArticleID", CheckBoxArticleSearch.IsChecked == true && TextBoxArticleSearch.Tag != null ? TextBoxArticleSearch.Tag.ToString() : ""); 
                sqlParameter.Add("ChkBuySaleMainYN", chkMainItem.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ChkBuyerArticleNo", CheckBoxBuyerArticleNoSearch.IsChecked == true ? 1 : 0);
                sqlParameter.Add("BuyerArticleNo", (CheckBoxBuyerArticleNoSearch.IsChecked == true && TextBoxBuyerArticleNoSearch.Tag != null) ? TextBoxBuyerArticleNoSearch.Tag.ToString() : "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sWKResult_Article_ThisMonth", sqlParameter, false);

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
                            var WPPQCT = new Win_Prd_ProcessResultSum_Q_CodeView_ThisMonth()
                            {
                                Num = i,

                                Article = dr["Article"].ToString(),
                                ArticleID = dr["ArticleID"].ToString(),
                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),

                                SDay01 = dr["SDay01"].ToString(),
                                SDay02 = dr["SDay02"].ToString(),
                                SDay03 = dr["SDay03"].ToString(),
                                SDay04 = dr["SDay04"].ToString(),
                                SDay05 = dr["SDay05"].ToString(),
                                SDay06 = dr["SDay06"].ToString(),
                                SDay07 = dr["SDay07"].ToString(),
                                SDay08 = dr["SDay08"].ToString(),
                                SDay09 = dr["SDay09"].ToString(),
                                SDay10 = dr["SDay10"].ToString(),
                                SDay11 = dr["SDay11"].ToString(),
                                SDay12 = dr["SDay12"].ToString(),
                                SDay13 = dr["SDay13"].ToString(),
                                SDay14 = dr["SDay14"].ToString(),
                                SDay15 = dr["SDay15"].ToString(),
                                SDay16 = dr["SDay16"].ToString(),
                                SDay17 = dr["SDay17"].ToString(),
                                SDay18 = dr["SDay18"].ToString(),
                                SDay19 = dr["SDay19"].ToString(),
                                SDay20 = dr["SDay20"].ToString(),
                                SDay21 = dr["SDay21"].ToString(),
                                SDay22 = dr["SDay22"].ToString(),
                                SDay23 = dr["SDay23"].ToString(),
                                SDay24 = dr["SDay24"].ToString(),
                                SDay25 = dr["SDay25"].ToString(),
                                SDay26 = dr["SDay26"].ToString(),
                                SDay27 = dr["SDay27"].ToString(),
                                SDay28 = dr["SDay28"].ToString(),
                                SDay29 = dr["SDay29"].ToString(),
                                SDay30 = dr["SDay30"].ToString(),
                                SDay31 = dr["SDay31"].ToString(),
                            };

                            int sum = Convert.ToInt32(WPPQCT.SDay01) + Convert.ToInt32(WPPQCT.SDay02) + Convert.ToInt32(WPPQCT.SDay03)
                                    + Convert.ToInt32(WPPQCT.SDay04) + Convert.ToInt32(WPPQCT.SDay05) + Convert.ToInt32(WPPQCT.SDay06)
                                    + Convert.ToInt32(WPPQCT.SDay07) + Convert.ToInt32(WPPQCT.SDay08) + Convert.ToInt32(WPPQCT.SDay09)
                                    + Convert.ToInt32(WPPQCT.SDay10) + Convert.ToInt32(WPPQCT.SDay11) + Convert.ToInt32(WPPQCT.SDay12)
                                    + Convert.ToInt32(WPPQCT.SDay13) + Convert.ToInt32(WPPQCT.SDay14) + Convert.ToInt32(WPPQCT.SDay15)
                                    + Convert.ToInt32(WPPQCT.SDay16) + Convert.ToInt32(WPPQCT.SDay17) + Convert.ToInt32(WPPQCT.SDay18)
                                    + Convert.ToInt32(WPPQCT.SDay19) + Convert.ToInt32(WPPQCT.SDay20) + Convert.ToInt32(WPPQCT.SDay21)
                                    + Convert.ToInt32(WPPQCT.SDay22) + Convert.ToInt32(WPPQCT.SDay23) + Convert.ToInt32(WPPQCT.SDay24)
                                    + Convert.ToInt32(WPPQCT.SDay25) + Convert.ToInt32(WPPQCT.SDay26) + Convert.ToInt32(WPPQCT.SDay27)
                                    + Convert.ToInt32(WPPQCT.SDay28) + Convert.ToInt32(WPPQCT.SDay29) + Convert.ToInt32(WPPQCT.SDay30)
                                    + Convert.ToInt32(WPPQCT.SDay31);
                            WPPQCT.TotalQty = lib.returnNumStringZero(Convert.ToString(sum));


                            WPPQCT.SDay01 = lib.returnNumStringZero(WPPQCT.SDay01);
                            WPPQCT.SDay02 = lib.returnNumStringZero(WPPQCT.SDay02);
                            WPPQCT.SDay03 = lib.returnNumStringZero(WPPQCT.SDay03);
                            WPPQCT.SDay04 = lib.returnNumStringZero(WPPQCT.SDay04);
                            WPPQCT.SDay05 = lib.returnNumStringZero(WPPQCT.SDay05);
                            WPPQCT.SDay06 = lib.returnNumStringZero(WPPQCT.SDay06);
                            WPPQCT.SDay07 = lib.returnNumStringZero(WPPQCT.SDay07);
                            WPPQCT.SDay08 = lib.returnNumStringZero(WPPQCT.SDay08);
                            WPPQCT.SDay09 = lib.returnNumStringZero(WPPQCT.SDay09);
                            WPPQCT.SDay10 = lib.returnNumStringZero(WPPQCT.SDay10);
                            WPPQCT.SDay11 = lib.returnNumStringZero(WPPQCT.SDay11);
                            WPPQCT.SDay12 = lib.returnNumStringZero(WPPQCT.SDay12);
                            WPPQCT.SDay13 = lib.returnNumStringZero(WPPQCT.SDay13);
                            WPPQCT.SDay14 = lib.returnNumStringZero(WPPQCT.SDay14);
                            WPPQCT.SDay15 = lib.returnNumStringZero(WPPQCT.SDay15);
                            WPPQCT.SDay16 = lib.returnNumStringZero(WPPQCT.SDay16);
                            WPPQCT.SDay17 = lib.returnNumStringZero(WPPQCT.SDay17);
                            WPPQCT.SDay18 = lib.returnNumStringZero(WPPQCT.SDay18);
                            WPPQCT.SDay19 = lib.returnNumStringZero(WPPQCT.SDay19);
                            WPPQCT.SDay20 = lib.returnNumStringZero(WPPQCT.SDay20);
                            WPPQCT.SDay21 = lib.returnNumStringZero(WPPQCT.SDay21);
                            WPPQCT.SDay22 = lib.returnNumStringZero(WPPQCT.SDay22);
                            WPPQCT.SDay23 = lib.returnNumStringZero(WPPQCT.SDay23);
                            WPPQCT.SDay24 = lib.returnNumStringZero(WPPQCT.SDay24);
                            WPPQCT.SDay25 = lib.returnNumStringZero(WPPQCT.SDay25);
                            WPPQCT.SDay26 = lib.returnNumStringZero(WPPQCT.SDay26);
                            WPPQCT.SDay27 = lib.returnNumStringZero(WPPQCT.SDay27);
                            WPPQCT.SDay28 = lib.returnNumStringZero(WPPQCT.SDay28);
                            WPPQCT.SDay29 = lib.returnNumStringZero(WPPQCT.SDay29);
                            WPPQCT.SDay30 = lib.returnNumStringZero(WPPQCT.SDay30);
                            WPPQCT.SDay31 = lib.returnNumStringZero(WPPQCT.SDay31);

                            DataGridThisMonth.Items.Add(WPPQCT);
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

        #region 기타 메서드 모음

        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }

        private string stringFormatN1(object obj)
        {
            return string.Format("{0:N1}", obj);
        }

        private string stringFormatN2(object obj)
        {
            return string.Format("{0:N2}", obj);
        }

        private string stringFormatNN(object obj, int length)
        {
            return string.Format("{0:N" + length + "}", obj);
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
                str = str.Replace(",", "");

                if (Double.TryParse(str, out chkDouble) == true)
                {
                    result = Double.Parse(str);
                }
            }

            return result;
        }

        // 시간 : 분 으로 변환
        private string ConvertTimeFormat(string str)
        {
            string result = "";

            str = str.Trim().Replace(":", "");
            if (str.Length > 3 && str.Length < 7)
            {
                string hour = str.Substring(0, 2);
                string min = str.Substring(2, 2);

                result = hour + ":" + min;
            }

            return result;
        }

        #endregion

        #region 텍스트박스 공용 키다운 이벤트
        private void txtBox_KeyDown_Search(object sender, KeyEventArgs e)
        {
            using (Loading lw = new Loading(re_Search))
            {
                lw.ShowDialog();
            }
        }
        #endregion

        //품명
        private void LabelArticleSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
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
            catch(Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
        }

        //품명
        private void CheckBoxArticleSearch_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxArticleSearch.IsEnabled = true;
                ButtonArticleSearch.IsEnabled = true;
                TextBoxArticleSearch.Focus();

            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
        }

        //품명
        private void CheckBoxArticleSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxArticleSearch.IsEnabled = false;
                ButtonArticleSearch.IsEnabled = false;
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
        }

        //품명
        private void TextBoxArticleSearch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if(e.Key == Key.Enter)
                {
                    pf.ReturnCode(TextBoxArticleSearch, 77, "");
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
        }

        //품명
        private void ButtonArticleSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pf.ReturnCode(TextBoxArticleSearch, 77, "");
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
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

    class Win_Prd_ProcessResultSum_Q_ByProcessMachine : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }
        public string cls { get; set; }
        public string ProcessID { get; set; }
        public string Process { get; set; }
        public string BuyerModel { get; set; }
        public string MachineID { get; set; }
        public string MachineNo { get; set; }
        public string Article { get; set; }
        //public string ArticleID { get; set; }
        public string KCustom { get; set; }
        //public string ProdQtyPerBox { get; set; }
        public string WorkQty { get; set; }
        public string UnitPrice { get; set; }
        public string Amount { get; set; }
        public string WorkTime { get; set; }
        //public string OutQtyPerBox { get; set; }
        public string QtyPerBox { get; set; }
        public string BuyerArticleNo { get; set; }
    }

    class Win_Prd_ProcessResultSum_Q_ByArticle : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public string cls { get; set; }

        public string CustomID { get; set; }
        public string KCustom { get; set; }
        public string ArticleID { get; set; }
        public string Article { get; set; }

        public string BuyerArticleNo { get; set; }
        public string ProcessID { get; set; }
        public string Process { get; set; }
        public string BuyerModelID { get; set; }
        public string Model { get; set; }

        public string WorkQty { get; set; }
        public string ProdQtyPerBox { get; set; }

        public int Num { get; set; }
    }

    class Win_Prd_ProcessResultSum_Q_ByWorker : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public string cls { get; set; }
        public string WorkPersonID { get; set; }
        public string Name { get; set; }
        public string ProcessID { get; set; }
        public string Process { get; set; }

        public string MachineID { get; set; }
        public string Machine { get; set; }
        public string MachineNo { get; set; }
        public string BuyerModelID { get; set; }        
        public string Model { get; set; }

        public string ArticleID { get; set; }
        public string Article { get; set; }
        public string BuyerArticleNo { get; set; }
        public string CustomID { get; set; }
        public string KCustom { get; set; }

        public string WorkQty { get; set; }
        public string ProdQtyPerBox { get; set; }
        public int Num { get; set; }
    }

    class Win_Prd_ProcessResultSum_Q_CodeView_ThisMonth : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }

        public string Article { get; set; }
        public string ArticleID { get; set; }
        public string BuyerArticleNo { get; set; }
        public string TotalQty { get; set; }
        public string SDay01 { get; set; }
        public string SDay02 { get; set; }
        public string SDay03 { get; set; }
        public string SDay04 { get; set; }
        public string SDay05 { get; set; }
        public string SDay06 { get; set; }
        public string SDay07 { get; set; }
        public string SDay08 { get; set; }
        public string SDay09 { get; set; }
        public string SDay10 { get; set; }
        public string SDay11 { get; set; }
        public string SDay12 { get; set; }
        public string SDay13 { get; set; }
        public string SDay14 { get; set; }
        public string SDay15 { get; set; }
        public string SDay16 { get; set; }
        public string SDay17 { get; set; }
        public string SDay18 { get; set; }
        public string SDay19 { get; set; }
        public string SDay20 { get; set; }
        public string SDay21 { get; set; }
        public string SDay22 { get; set; }
        public string SDay23 { get; set; }
        public string SDay24 { get; set; }
        public string SDay25 { get; set; }
        public string SDay26 { get; set; }
        public string SDay27 { get; set; }
        public string SDay28 { get; set; }
        public string SDay29 { get; set; }
        public string SDay30 { get; set; }
        public string SDay31 { get; set; }
    }
}
