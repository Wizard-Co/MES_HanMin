using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
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
using WPF.MDI;

namespace WizMes_DeokWoo
{
    /// <summary>
    /// Win_prd_PlanInput_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_prd_PlanInput_U_New : UserControl
    {
        Win_prd_PlanInput_U_CodeView WinPlan = new Win_prd_PlanInput_U_CodeView();
        Win_prd_PlanArticleOne_CodeView WinPlanArticleOne = new Win_prd_PlanArticleOne_CodeView();

        List<DataRow> lstDataRow = new List<DataRow>();

        int tempSelectedIndex = 0;
        int numSelect = 0;

        public Win_prd_PlanInput_U_New()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Lib.Instance.UiLoading(sender);

            chkOrderDay.IsChecked = true;
            dtpSDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];

            dtpInstDate.SelectedDate = DateTime.Today; // 지시일자
            dtpInstCompleteDate.SelectedDate = DateTime.Today; // 작업완료일

            setAllPrint(); // 테스트
        }

        #region 공정패턴 콤보박스 세팅

        private void setCboPattern(string ArticleGrpID)
        {
            if (cboProcessPattern.ItemsSource != null)
            {
                cboProcessPattern.ItemsSource = null;
            }

            ObservableCollection<CodeView> cboPattern = new ObservableCollection<CodeView>();

            List<string> CbView = new List<string>();
            List<string> PatternID = new List<string>();

            string strCompare1 = string.Empty;
            string strCompare2 = string.Empty;
            string TheView = string.Empty;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("sArticleGrpID", ArticleGrpID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_PlanInput_sPatternByArticleGrpID", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;
                        int i = 0;

                        foreach (DataRow item in drc)
                        {
                            CodeView codeView = new CodeView();

                            strCompare1 = item["PatternID"].ToString().Trim();
                            strCompare2 = item["Pattern"].ToString().Trim();

                            TheView = strCompare1 + "." + strCompare2 + " : ";

                            foreach (DataRow items in drc)
                            {
                                if (items["PatternID"].ToString().Equals(strCompare1))
                                {
                                    TheView += " [" + items["Process"].ToString() + "] →";
                                }
                            }
                            if (TheView != null && !TheView.Equals(""))
                            {
                                TheView = TheView.Substring(0, TheView.Length - 1);
                            }

                            if (CbView.Count > 0)
                            {
                                if (!CbView[i].Substring(0, 2).Equals(strCompare1))
                                {
                                    codeView.code_id = strCompare1;
                                    codeView.code_name = TheView;

                                    CbView.Add(TheView);
                                    cboPattern.Add(codeView);
                                    i++;
                                }
                            }
                            else
                            {
                                codeView.code_id = strCompare1;
                                codeView.code_name = TheView;

                                CbView.Add(TheView);
                                cboPattern.Add(codeView);
                            }
                        }
                        // foreach 끝

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }

            cboProcessPattern.ItemsSource = cboPattern;
            cboProcessPattern.DisplayMemberPath = "code_name";
            cboProcessPattern.SelectedValuePath = "code_id";
        }
        #endregion

        #region 일자
        //수주 일자
        private void lblOrderDay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkOrderDay.IsChecked == true)
            {
                chkOrderDay.IsChecked = false;
            }
            else
            {
                chkOrderDay.IsChecked = true;
            }
        }
        //수주 일자
        private void chkOrderDay_Checked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = true;
            dtpEDate.IsEnabled = true;
        }
        //수주 일자
        private void chkOrderDay_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
        }

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
        #endregion

        #region 상단 조건 모음
        //거래처
        private void lblCustom_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

        //거래처
        private void chkCustom_Checked(object sender, RoutedEventArgs e)
        {
            txtCustom.IsEnabled = true;
            btnPfCustom.IsEnabled = true;
            txtCustom.Focus();
        }

        //거래처
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
                MainWindow.pf.ReturnCode(txtCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
            }
        }

        //거래처
        private void btnPfCustom_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //품명
        private void lblArticle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticle.IsChecked == true) { chkArticle.IsChecked = false; }
            else { chkArticle.IsChecked = true; }
        }

        //품명
        private void chkArticle_Checked(object sender, RoutedEventArgs e)
        {
            txtArticle.IsEnabled = true;
            btnPfArticle.IsEnabled = true;
            txtArticle.Focus();
        }

        //품명
        private void chkArticle_Unchecked(object sender, RoutedEventArgs e)
        {
            txtArticle.IsEnabled = false;
            btnPfArticle.IsEnabled = false;
        }

        //품명
        private void txtArticle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticle, (int)Defind_CodeFind.DCF_Article, "");
            }
        }

        //품명
        private void btnPfArticle_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticle, (int)Defind_CodeFind.DCF_Article, "");
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
            txtOrder.Focus();
        }

        //관리번호
        private void chkOrder_Unchecked(object sender, RoutedEventArgs e)
        {
            txtOrder.IsEnabled = false;
        }

        //해상도가 낮아지면 체크박스 클릭이 어려워지므로 라벨 클릭으로 대체할수 있게 한다.
        private void lblCloseClss_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkClosClss.IsChecked == true) { chkClosClss.IsChecked = false; }
            else { chkClosClss.IsChecked = true; }
        }

        //해상도가 낮아지면 체크박스 클릭이 어려워지므로 라벨 클릭으로 대체할수 있게 한다.
        private void lblCompleteOrder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkCompleteOrder.IsChecked == true) { chkCompleteOrder.IsChecked = false; }
            else { chkCompleteOrder.IsChecked = true; }
        }

        //OrderNo
        private void rbnOrderNo_Click(object sender, RoutedEventArgs e)
        {
            Check_bdrOrder();
        }

        //관리번호
        private void rbnOrderID_Click(object sender, RoutedEventArgs e)
        {
            Check_bdrOrder();
        }

        private void Check_bdrOrder()
        {
            if (rbnOrderID.IsChecked == true)
            {
                tbkOrder.Text = "관리번호";
                dgdtxtBuyerModel.Visibility = Visibility.Visible;
                dgdtxtBuyerArticleNo.Visibility = Visibility.Hidden;
            }
            else if (rbnOrderNo.IsChecked == true)
            {
                tbkOrder.Text = "Order No.";
                dgdtxtBuyerModel.Visibility = Visibility.Hidden;
                dgdtxtBuyerArticleNo.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region 우측 상단 버튼
        //검색
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            re_Search(0);
        }

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Lib.Instance.ChildMenuClose(this.ToString());
        }


        #endregion

        #region 조회

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

                sqlParameter.Add("ChkDate", chkOrderDay.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SDate", chkOrderDay.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("EDate", chkOrderDay.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("ChkCustomID", chkCustom.IsChecked == true ? 1 : 0);
                sqlParameter.Add("CustomID", chkCustom.IsChecked == true && txtCustom.Tag != null ? txtCustom.Tag.ToString() : "");

                sqlParameter.Add("ChkArticleID", chkArticle.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ArticleID", chkArticle.IsChecked == true && txtArticle.Tag != null ? txtArticle.Tag.ToString() : "");
                sqlParameter.Add("ChkOrder", chkOrder.IsChecked == true ? (rbnOrderID.IsChecked == true ? 1 : 2) : 0);
                sqlParameter.Add("Order", chkOrder.IsChecked == true ? txtOrder.Text : "");
                sqlParameter.Add("ChkCloseClss", chkClosClss.IsChecked == true ? 1 : 0);

                sqlParameter.Add("ChkIncPlComplete", chkCompleteOrder.IsChecked == true ? 1 : 0);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_PlanInput_sOrder", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinPlanOrder = new Win_prd_PlanInput_U_CodeView()
                            {
                                Num = i + 1,
                                KCustom = dr["KCustom"].ToString(),
                                Article = dr["Article"].ToString(),
                                OrderID = dr["OrderID"].ToString(),
                                OrderNo = dr["OrderNo"].ToString(),
                                OrderQty = stringFormatN0(dr["OrderQty"]),
                                notOrderInstQty = stringFormatN0(dr["notOrderInstQty"]),
                                OrderInstQy = stringFormatN0(dr["OrderInstQy"]),
                                p1WorkQty = stringFormatN0(dr["p1WorkQty"]),
                                p1ProcessID = dr["p1ProcessID"].ToString(),
                                p1ProcessName = dr["p1ProcessName"].ToString(),
                                InspectQty = stringFormatN0(dr["InspectQty"]),
                                OutQty = stringFormatN0(dr["OutQty"]),
                                PatternID = dr["PatternID"].ToString(),
                                ArticleGrpID = dr["ArticleGrpID"].ToString(),
                                ArticleGrpName = dr["ArticleGrpName"].ToString(),
                                BuyerModel = dr["BuyerModel"].ToString(),
                                BuyerModelID = dr["BuyerModelID"].ToString(),
                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                Remark = dr["Remark"].ToString(),
                                ArticleID = dr["ArticleID"].ToString(),
                                CloseClss = dr["CloseClss"].ToString(),
                                PlanComplete = dr["PlanComplete"].ToString()
                            };

                            dgdMain.Items.Add(WinPlanOrder);
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
        #endregion

        #region 하단 그리드 조회
        private void FillGridPlanInput(string strPatternID, string strArticleID)
        {
            if (dgdPlanInput.Items.Count > 0)
            {
                dgdPlanInput.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("sPatternID", strPatternID);
                sqlParameter.Add("sArticleID", strArticleID);
                sqlParameter.Add("sOutMessage", "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_PlanInput_sPatternArticleOne", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count > 0)
                    {
                        if (!dt.Columns[0].ColumnName.Equals("PatternSeq"))
                        {
                            MessageBox.Show(dt.Rows[0].ItemArray[0].ToString());
                        }
                        else
                        {
                            DataRowCollection drc = dt.Rows;

                            foreach (DataRow dr in drc)
                            {
                                var WinPlanArticle = new Win_prd_PlanArticleOne_CodeView()
                                {
                                    Num = i + 1,
                                    PatternSeq = dr["PatternSeq"].ToString(),
                                    ProcessID = dr["ProcessID"].ToString(),
                                    Process = dr["Process"].ToString(),
                                    Qty = stringFormatN0(dr["Qty"]),
                                    Article = dr["Article"].ToString(),
                                    ArticleID = dr["ArticleID"].ToString(),
                                    LVL = dr["LVL"].ToString(),
                                    InstQty = stringFormatN0(txtQty.Text),
                                    StartDate = dtpInstDate.SelectedDate.Value.ToString("yyyyMMdd"),
                                    EndDate = dtpInstCompleteDate.SelectedDate.Value.ToString("yyyyMMdd")
                                };

                                dgdPlanInput.Items.Add(WinPlanArticle);
                                i++;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("해당 품명의 공정을 확인해보시기 바랍니다.");
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
        #endregion

        #region 재조회
        private void re_Search(int selectedIndex)
        {
            FillGrid();

            if (dgdMain.Items.Count > 0)
            {
                dgdMain.SelectedItem = selectedIndex;
            }
        }

        #endregion

        #region 메인그리드 선택 시!!

        private void dgdMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Plan = dgdMain.SelectedItem as Win_prd_PlanInput_U_CodeView;

            if (Plan != null
                && Plan.ArticleGrpID != null)
            {
                this.DataContext = null;

                setCboPattern(Plan.ArticleGrpID); // 공정패턴 콤보박스 세팅!!
                cboProcessPattern.SelectedValue = Plan.PatternID;

                dtpInstDate.SelectedDate = DateTime.Today;
                dtpInstCompleteDate.SelectedDate = DateTime.Today;
                this.DataContext = Plan;

                // 현재고 구하기 → 그냥 재고 화면으로 넘기는 거였어..
                //FillGrid_StockQty(Plan.ArticleID);
            }
        }

        #endregion

        #region 메인그리드 선택 시 - 해당 품명의 현 재고 구하기 [안씀]

        private void FillGrid_StockQty(string ArticleID)
        {
            string StockQty = "0";

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("ArticleID", ArticleID);
                
                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_PlanInput_sStockForPlanInput", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            if (dr["StockQty"] != null)
                            {
                                StockQty = stringFormatN0(dr["StockQty"]);
                            }
                        }

                        txtStockQty.Text = StockQty;
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
        #endregion

        #region Content - 지시수량 (숫자만 입력되도록), 패턴바뀌면!

        // 수량을 숫자만 입력 가능 하도록
        private void txtQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Lib.Instance.CheckIsNumeric((TextBox)sender, e);
        }

     
        // 패턴이 바뀌면!
        private void cboProcessPattern_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (btnAdd.IsEnabled == false
                && cboProcessPattern.SelectedValue != null)
            {
                WinPlan = dgdMain.SelectedItem as Win_prd_PlanInput_U_CodeView;

                if (WinPlan != null)
                {
                    dgdPlanInput.Visibility = Visibility.Visible;
                    FillGridPlanInput(cboProcessPattern.SelectedValue.ToString(), WinPlan.ArticleID);
                    //dgdMain.IsEnabled = false;
                    dgdMain.IsHitTestVisible = false;
                }
                else
                {
                    return;
                }
            }
        }
        #endregion

        #region Content - 투입완료, 원자재 투입 예외관리 등등 라벨 체크박스 이벤트

        private void chkStuffClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkStuffClose.IsChecked == true)
            {
                chkStuffClose.IsChecked = false;
            }
            else
            {
                chkStuffClose.IsChecked = true;
            }
        }

        private void chkMtrExceptYN_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkMtrExceptYN.IsChecked == true)
            {
                chkMtrExceptYN.IsChecked = false;
            }
            else
            {
                chkMtrExceptYN.IsChecked = true;
            }
        }

        private void chkOutwareExceptYN_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkOutwareExceptYN.IsChecked == true)
            {
                chkOutwareExceptYN.IsChecked = false;
            }
            else
            {
                chkOutwareExceptYN.IsChecked = true;
            }
        }

        private void chkRemainData_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkRemainData.IsChecked == true)
            {
                chkRemainData.IsChecked = false;
            }
            else
            {
                chkRemainData.IsChecked = true;
            }
        }

        private void chkAutoPrint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkAutoPrint.IsChecked == true)
            {
                chkAutoPrint.IsChecked = false;
            }
            else
            {
                chkAutoPrint.IsChecked = true;
            }
        }

        #endregion

        #region Content - 버튼 모음 (추가, 작업지시, 취소)
        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            WinPlan = dgdMain.SelectedItem as Win_prd_PlanInput_U_CodeView;

            if (WinPlan != null)
            {
                // 공정패턴이 없다면
                if (cboProcessPattern.SelectedValue == null)
                {
                    MessageBox.Show("공정패턴을 선택해주세요.");
                    return;
                }
                
                // 품명이 없다면
                if (WinPlan.ArticleID == null)
                {
                    MessageBox.Show("해당 수주에 품명 정보가 없습니다.");
                    return;
                }

                numSelect = dgdMain.SelectedIndex;
                dgdPlanInput.Visibility = Visibility.Visible;
                FillGridPlanInput(cboProcessPattern.SelectedValue.ToString(), WinPlan.ArticleID);
                //dgdMain.IsEnabled = false;
                dgdMain.IsHitTestVisible = false;

                btnAdd.IsEnabled = false;
                btnCancel.IsEnabled = true;
                btnSave.IsEnabled = true;
            }
        }

        //취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (btnAdd.IsEnabled == false)
            {
                if (MessageBox.Show("선택하신 항목을 취소하시겠습니까?", "취소 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (dgdPlanInput.Items.Count > 0)
                    {
                        dgdPlanInput.Items.Clear();
                    }

                    dgdPlanInput.Visibility = Visibility.Hidden;
                    //dgdMain.IsEnabled = true;
                    dgdMain.IsHitTestVisible = true;
                    dgdMain.SelectedIndex = numSelect;
                    btnAdd.IsEnabled = true;
                    btnCancel.IsEnabled = false;
                    btnSave.IsEnabled = false;
                    numSelect = 0;
                }
            }
        }

        //작업지시
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveData())
            {
                if (dgdPlanInput.Items.Count > 0)
                {
                    dgdPlanInput.Items.Clear();
                    lstDataRow.Clear();
                }

                re_Search(0);

                dgdPlanInput.Visibility = Visibility.Hidden;
                //dgdMain.IsEnabled = true;
                dgdMain.IsHitTestVisible = true;
                btnAdd.IsEnabled = true;
                btnCancel.IsEnabled = false;
                btnSave.IsEnabled = false;
            }
        }

        #endregion

        #region 저장
        private bool SaveData()
        {
            bool flag = false;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                if (CheckData())
                {
                    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Clear();
                    sqlParameter.Add("InstID", "");
                    sqlParameter.Add("InstDate", dtpInstDate.SelectedDate.Value.ToString("yyyyMMdd"));
                    sqlParameter.Add("OrderID", WinPlan.OrderID);
                    sqlParameter.Add("OrderSeq", "1");
                    sqlParameter.Add("InstRoll", 0);

                    sqlParameter.Add("InstQty", txtQty.Text.Replace(",", ""));
                    sqlParameter.Add("ExpectDate", dtpInstCompleteDate.SelectedDate.Value.ToString("yyyyMMdd"));
                    sqlParameter.Add("PersonID", MainWindow.CurrentPersonID);
                    sqlParameter.Add("Remark", txtRemark.Text);
                    sqlParameter.Add("MtrExceptYN", chkMtrExceptYN.IsChecked == true ? "Y" : "N");
                    sqlParameter.Add("OutwareExceptYN", chkOutwareExceptYN.IsChecked == true ? "Y" : "N");     //단위 선택
                    sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                    Procedure pro1 = new Procedure();
                    pro1.Name = "xp_PlanInput_iPlanInput";
                    pro1.OutputUseYN = "Y";
                    pro1.OutputName = "InstID";
                    pro1.OutputLength = "12";

                    Prolist.Add(pro1);
                    ListParameter.Add(sqlParameter);

                    for (int i = 0; i < dgdPlanInput.Items.Count; i++)
                    {
                        WinPlanArticleOne = dgdPlanInput.Items[i] as Win_prd_PlanArticleOne_CodeView;
                        sqlParameter = new Dictionary<string, object>();
                        sqlParameter.Add("InstID", "");
                        sqlParameter.Add("InstDate", txtQty.Text);
                        sqlParameter.Add("ProcSeq", WinPlanArticleOne.PatternSeq);
                        sqlParameter.Add("ArticleID", WinPlanArticleOne.ArticleID);
                        sqlParameter.Add("ProcessID", WinPlanArticleOne.ProcessID);

                        sqlParameter.Add("InstRemark", WinPlanArticleOne.InstRemark == null ? "" : WinPlanArticleOne.InstRemark);
                        sqlParameter.Add("InstQty", WinPlanArticleOne.InstQty.Replace(",", ""));
                        sqlParameter.Add("StartDate", WinPlanArticleOne.StartDate);
                        sqlParameter.Add("EndDate", WinPlanArticleOne.EndDate);
                        sqlParameter.Add("Remark", WinPlanArticleOne.Remark == null ? "" : WinPlanArticleOne.Remark);

                        sqlParameter.Add("MachineID", WinPlanArticleOne.MachineID == null ? "" : WinPlanArticleOne.MachineID);
                        sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                        Procedure pro2 = new Procedure();
                        pro2.Name = "xp_PlanInput_iPlanInputSub";
                        pro2.OutputUseYN = "N";
                        pro2.OutputName = "InstID";
                        pro2.OutputLength = "12";

                        Prolist.Add(pro2);
                        ListParameter.Add(sqlParameter);
                    }

                    List<KeyValue> list_Result = new List<KeyValue>();
                    list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS(Prolist, ListParameter);
                    string sGetID = string.Empty;

                    if (list_Result[0].key.ToLower() == "success")
                    {
                        list_Result.RemoveAt(0);
                        for (int i = 0; i < list_Result.Count; i++)
                        {
                            KeyValue kv = list_Result[i];
                            if (kv.key == "InstID")
                            {
                                sGetID = kv.value;
                                flag = true;
                            }
                        }

                        if (flag)
                        {
                            UpdatePattern(WinPlan.OrderID);
                        }
                    }
                    else
                    {
                        MessageBox.Show("[저장실패]\r\n" + list_Result[0].value.ToString());
                        flag = false;
                        //return false;
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

            return flag;
        }
        #endregion

        #region 유효성 검사 CheckData()

        private bool CheckData()
        {
            bool flag = true;

            // pl_inputdet 에 데이터가 무조건 들어가야 됨!!!
            // == dgdPlanInput 에 데이터가 들어가 있어야 저장이 되야됨.
            if (dgdPlanInput.Items.Count == 0)
            {
                MessageBox.Show("하단의 공정별 작업지시 데이터가 없습니다.\r(공정패턴을 변경해주시거나, 취소 후 다시 작업지시를 내려주세요.)");
                flag = false;
                return flag;
            }

            return flag;
        }

        #endregion // 유효성 검사

        #region UpdatePattern
        //
        private bool UpdatePattern(string strOrderID)
        {
            bool flag = false;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("OrderID", strOrderID);
                sqlParameter.Add("PatternID", cboProcessPattern.SelectedValue.ToString());
                sqlParameter.Add("StuffCloseClss", chkStuffClose.IsChecked == true ? "*" : "");
                sqlParameter.Add("LastUpdateUserID", MainWindow.CurrentUser);

                Procedure pro1 = new Procedure();
                pro1.Name = "xp_PlanInput_uOrderPatternID";
                pro1.OutputUseYN = "N";
                pro1.OutputName = "OrderID";
                pro1.OutputLength = "10";

                Prolist.Add(pro1);
                ListParameter.Add(sqlParameter);

                string[] Confirm = new string[2];
                Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew(Prolist, ListParameter);
                if (Confirm[0] != "success")
                {
                    MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
                    flag = false;
                    //return false;
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

            return flag;
        }
        #endregion

        #region 데이터 그리드 이동관련
        //
        private void DataGridCell_KeyDown(object sender, KeyEventArgs e)
        {
            WinPlanArticleOne = dgdPlanInput.CurrentItem as Win_prd_PlanArticleOne_CodeView;
            int startColCount = dgdPlanInput.Columns.IndexOf(dgdtpeOrderQty);
            int colCount = dgdPlanInput.Columns.IndexOf(dgdPlanInput.CurrentCell.Column);
            int rowCount = dgdPlanInput.Items.IndexOf(dgdPlanInput.CurrentItem);

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (dgdPlanInput.Columns.Count - 1 == colCount && dgdPlanInput.Items.Count - 1 > rowCount)
                {
                    dgdPlanInput.CurrentCell = new DataGridCellInfo(dgdPlanInput.Items[rowCount + 1], dgdPlanInput.Columns[startColCount]);
                }
                else if (dgdPlanInput.Columns.Count - 1 > colCount && dgdPlanInput.Items.Count - 1 > rowCount)
                {
                    dgdPlanInput.CurrentCell = new DataGridCellInfo(dgdPlanInput.Items[rowCount], dgdPlanInput.Columns[colCount + 1]);
                }
                else if (dgdPlanInput.Columns.Count - 1 == colCount && dgdPlanInput.Items.Count - 1 == rowCount)
                {
                    //btnSave.Focus();
                }
                else if (dgdPlanInput.Columns.Count - 1 > colCount && dgdPlanInput.Items.Count - 1 == rowCount)
                {
                    dgdPlanInput.CurrentCell = new DataGridCellInfo(dgdPlanInput.Items[rowCount], dgdPlanInput.Columns[colCount + 1]);
                }
                else
                {
                    MessageBox.Show("있으면 찾아보자...");
                }
            }
        }

        //
        private void TextBoxFocusInDataGrid(object sender, KeyEventArgs e)
        {
            Lib.Instance.DataGridINControlFocus(sender, e);
        }

        //
        private void TextBoxFocusInDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Lib.Instance.DataGridINBothByMouseUP(sender, e);
        }

        //
        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            //if (btnSave.IsEnabled == true)
            //{
            //    DataGridCell cell = sender as DataGridCell;
            //    cell.IsEditing = true;
            //}
        }
        #endregion

        #region 데이터 그리드 값 변동시
        //
        private void dgdtpetxtOrderQty_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (btnSave.IsEnabled == true)
            //{
            //    WinPlanArticleOne = dgdPlanInput.CurrentItem as Win_prd_PlanArticleOne_CodeView;

            //    if (WinPlanArticleOne != null)
            //    {
            //        TextBox tb1 = sender as TextBox;

            //        if (tb1 != null)
            //        {
            //            WinPlanArticleOne.InstQty = Lib.Instance.returnNumStringZero(tb1.Text);
            //            tb1.SelectionStart = tb1.Text.Length;
            //        }

            //        sender = tb1;
            //    }
            //}
        }

        private void dgdtpetxtSDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (btnSave.IsEnabled == true)
            //{
            //    WinPlanArticleOne = dgdPlanInput.CurrentItem as Win_prd_PlanArticleOne_CodeView;

            //    if (WinPlanArticleOne != null)
            //    {
            //        TextBox tb1 = sender as TextBox;

            //        if (tb1 != null)
            //        {
            //            WinPlanArticleOne.StartDate = tb1.Text;
            //        }

            //        sender = tb1;
            //    }
            //}
        }

        private void dgdtpetxtEDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (btnSave.IsEnabled == true)
            //{
            //    WinPlanArticleOne = dgdPlanInput.CurrentItem as Win_prd_PlanArticleOne_CodeView;

            //    if (WinPlanArticleOne != null)
            //    {
            //        TextBox tb1 = sender as TextBox;

            //        if (tb1 != null)
            //        {
            //            WinPlanArticleOne.EndDate = tb1.Text;
            //        }

            //        sender = tb1;
            //    }
            //}
        }
        #endregion

        #region 시작일, 종료일 값 변동시
        private void dgdtpetxtOrderQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Lib.Instance.CheckIsNumeric((TextBox)sender, e);
        }

        //시작일
        private void dgdtpedtpSDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            WinPlanArticleOne = dgdPlanInput.CurrentItem as Win_prd_PlanArticleOne_CodeView;
            DatePicker dtpSDate = (DatePicker)sender;

            if (WinPlanArticleOne == null)
            {
                MessageBox.Show("행 없다!");
                return;
            }

            if (dtpSDate.SelectedDate != null)
            {
                WinPlanArticleOne.StartDate = dtpSDate.SelectedDate.Value.ToString("yyyyMMdd");
                sender = dtpSDate;
            }
        }

        //시작일
        private void dgdtpedtpSDate_CalendarClosed(object sender, RoutedEventArgs e)
        {
            WinPlanArticleOne = dgdPlanInput.CurrentItem as Win_prd_PlanArticleOne_CodeView;
            int rowCount = dgdPlanInput.Items.IndexOf(dgdPlanInput.CurrentItem);
            int colCount = dgdPlanInput.Columns.IndexOf(dgdPlanInput.CurrentCell.Column);
            DatePicker dtpSDate = (DatePicker)sender;

            if (WinPlanArticleOne == null)
            {
                MessageBox.Show("행 없다!");
                return;
            }

            if (dtpSDate.SelectedDate != null)
            {
                WinPlanArticleOne.StartDate = dtpSDate.SelectedDate.Value.ToString("yyyyMMdd");
                sender = dtpSDate;
            }
            DataGridCell cell = Lib.Instance.GetParent<DataGridCell>(sender as DatePicker);
            if (cell != null)
            {
                cell.IsEditing = false;
                dgdPlanInput.CurrentCell = new DataGridCellInfo(dgdPlanInput.Items[rowCount], dgdPlanInput.Columns[colCount + 1]);
            }
        }

        //종료일
        private void dgdtpedtpEDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            WinPlanArticleOne = dgdPlanInput.CurrentItem as Win_prd_PlanArticleOne_CodeView;
            DatePicker dtpEDate = (DatePicker)sender;

            if (WinPlanArticleOne == null)
            {
                MessageBox.Show("행 없다!");
                return;
            }

            if (dtpEDate.SelectedDate != null)
            {
                WinPlanArticleOne.EndDate = dtpEDate.SelectedDate.Value.ToString("yyyyMMdd");
                sender = dtpEDate;
            }
        }

        //종료일
        private void dgdtpedtpEDate_CalendarClosed(object sender, RoutedEventArgs e)
        {
            WinPlanArticleOne = dgdPlanInput.CurrentItem as Win_prd_PlanArticleOne_CodeView;
            int rowCount = dgdPlanInput.Items.IndexOf(dgdPlanInput.CurrentItem);
            int colCount = dgdPlanInput.Columns.IndexOf(dgdPlanInput.CurrentCell.Column);
            DatePicker dtpEDate = (DatePicker)sender;

            if (WinPlanArticleOne == null)
            {
                MessageBox.Show("행 없다!");
                return;
            }

            if (dtpEDate.SelectedDate != null)
            {
                WinPlanArticleOne.EndDate = dtpEDate.SelectedDate.Value.ToString("yyyyMMdd");
                sender = dtpEDate;
            }
            DataGridCell cell = Lib.Instance.GetParent<DataGridCell>(sender as DatePicker);
            if (cell != null)
            {
                cell.IsEditing = false;
                dgdPlanInput.CurrentCell = new DataGridCellInfo(dgdPlanInput.Items[rowCount], dgdPlanInput.Columns[colCount + 1]);
            }
        }
        #endregion

        #region 지시사항, 특이사항 값 변동시
        //지시사항
        private void dgdtpetxtInsRemark_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (btnSave.IsEnabled == true)
            //{
            //    WinPlanArticleOne = dgdPlanInput.CurrentItem as Win_prd_PlanArticleOne_CodeView;

            //    if (WinPlanArticleOne != null)
            //    {
            //        TextBox tb1 = sender as TextBox;

            //        if (tb1 != null)
            //        {
            //            WinPlanArticleOne.InstRemark = tb1.Text;
            //        }

            //        sender = tb1;
            //    }
            //}
        }

        //특이사항
        private void dgdtpetxtRemark_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (btnSave.IsEnabled == true)
            //{
            //    WinPlanArticleOne = dgdPlanInput.CurrentItem as Win_prd_PlanArticleOne_CodeView;

            //    if (WinPlanArticleOne != null)
            //    {
            //        TextBox tb1 = sender as TextBox;

            //        if (tb1 != null)
            //        {
            //            WinPlanArticleOne.Remark = tb1.Text;
            //        }

            //        sender = tb1;
            //    }
            //}
        }
        #endregion

        #region 호기에서 키, 마우스 이벤트
        //호기
        private void dgdtpetxtMachine_KeyDown(object sender, KeyEventArgs e)
        {
            WinPlanArticleOne = dgdPlanInput.CurrentItem as Win_prd_PlanArticleOne_CodeView;

            if (e.Key == Key.Enter)
            {
                TextBox tb1 = sender as TextBox;
                MainWindow.pf.ReturnCode(tb1, 66, WinPlanArticleOne.ProcessID);

                if (tb1.Tag != null)
                {
                    WinPlanArticleOne.Machine = tb1.Text;
                    WinPlanArticleOne.MachineID = tb1.Tag.ToString();
                }

                sender = tb1;
            }
        }

        //호기
        private void dgdtpetxtMachine_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WinPlanArticleOne = dgdPlanInput.CurrentItem as Win_prd_PlanArticleOne_CodeView;

            TextBox tb1 = sender as TextBox;
            MainWindow.pf.ReturnCode(tb1, 66, WinPlanArticleOne.ProcessID);

            if (tb1.Tag != null)
            {
                WinPlanArticleOne.Machine = tb1.Text;
                WinPlanArticleOne.MachineID = tb1.Tag.ToString();
            }

            sender = tb1;
        }

        #endregion

        #region 기타 메서드 모음

        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }

        // 천마리 콤마, 소수점 두자리
        private string stringFormatN2(object obj)
        {
            return string.Format("{0:N2}", obj);
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


        #endregion

        #region 프린터, 스샷 테스트

        #region 프린터 테스트

        private void setAllPrint()
        {
            PrinterSettings settings = new PrinterSettings();

            int i = 0;
            foreach(string printer in PrinterSettings.InstalledPrinters)
            {
                i++;

                var Printer = new PCodeView()
                {
                    Num = i.ToString(),
                    PrintName = printer.ToString(),
                };

                

                dgdPrint.Items.Add(Printer);
            }

            //if (settings.IsDefaultPrinter && (printer.Contains("TSC"))) //기본 프린트일때
            //{
            //    return printer;
            //}
            ////if ((printer.Contains("TSC")))// && printer.Contains("Pro")))// || printer.Contains("복사"))
            ////{
            ////    return printer;
            ////    //ini에 프린트 on/off 상태체크할지 설정창 추가
            ////    if (PrintCheckOnOff(printer))
            ////    {
            ////        return printer;
            ////    }
            ////}
        }

        #endregion

        private void btnSavePrinter_Click(object sender, RoutedEventArgs e)
        {
            //PrinterSettings settings = new PrinterSettings();

            

            var Printer = dgdPrint.SelectedItem as PCodeView;
            if (Printer != null)
            {
                //settings.DefaultPageSettings.PrinterSettings.PrinterName = Printer.PrintName;
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            ScreenCapture();
        }

        public void ScreenCapture()
        {
            //화면의 크기 정보 
            // 1. ConvertInt(txtCustom.Text) : 왼쪽 메뉴 폭
            int width = (int)SystemParameters.PrimaryScreenWidth - ConvertInt(txtCustom.Text);
            // 2. ConvertInt(txtArticle.Text) : 위 메뉴 높이
            // ConvertInt(txtArticle.Text) : 밑의 시작메뉴 높이
            int height = (int)SystemParameters.PrimaryScreenHeight - ConvertInt(txtArticle.Text) - ConvertInt(txtOrder.Text);  

            //화면의 크기만큼 bitmap생성
            using (Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                //bitmap 이미지 변경을 위해 Grapics 객체 생성
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    // 화면을 그대로 카피해서 Bitmap 메모리에 저장 
                    // 1. ConvertInt(txtCustom.Text) : 왼쪽 메뉴 폭
                    // 2. ConvertInt(txtArticle.Text) : 위 메뉴 높이
                    gr.CopyFromScreen(ConvertInt(txtCustom.Text), ConvertInt(txtArticle.Text), 0, 0, bmp.Size);
                }

                //Bitmap 데이터를 파일로(저장 경로를 지정해서??)
                bmp.Save(@"c:\temp\test.png", ImageFormat.Png);

                using (MemoryStream memory = new MemoryStream())
                {
                    bmp.Save(memory, ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    //ImgImage.Source = bitmapImage;

                    // 저장한 이미지 실행시키기
                    try
                    {
                        ProcessStartInfo proc = new ProcessStartInfo(@"c:\temp\test.png");
                        proc.UseShellExecute = true;
                        Process.Start(proc);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        #endregion // 프린터, 스샷 테스트
    }

    class PCodeView
    {
        public string Num { get; set; }
        public string PrintName { get; set; }
    }
}
