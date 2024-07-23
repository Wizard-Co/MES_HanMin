using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WizMes_HanMin;
using WizMes_HanMin.PopUP;

namespace WizMes_HanMin
{
    /// <summary>
    /// Win_ord_EnergyUse_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_ord_EnergyUse_U : UserControl
    {
        Lib lib = new Lib();
        string AASS = string.Empty;
        PlusFinder pf = new PlusFinder();
        WizMes_HanMin.PopUp.NoticeMessage msg = new PopUp.NoticeMessage();
        private Microsoft.Office.Interop.Excel.Application excelapp;
        private Microsoft.Office.Interop.Excel.Workbook workbook;
        private Microsoft.Office.Interop.Excel.Worksheet worksheet;
        private Microsoft.Office.Interop.Excel.Range workrange;
        private Microsoft.Office.Interop.Excel.Worksheet copysheet;
        private Microsoft.Office.Interop.Excel.Worksheet pastesheet;

        string stDate = string.Empty;
        string stTime = string.Empty;

        public Win_ord_EnergyUse_U()
        {
            InitializeComponent();
        }

        // 폼 로드 됬을때
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            // 콤보박스 세팅
            comboBoxSetting();
            SetCombobox();

            // 일자 체크박스 IsChecked = true
            chkInOutDate.IsChecked = true;
            chkEnergySearch.IsChecked = true;
            chkEnergyUnit.IsChecked = true;
            dtpFromDate.SelectedDate = dtpFromDate.SelectedDate.Value.AddMonths(-1);
        }

        ObservableCollection<CodeView> ovcProductFormIDView = null;
        ObservableCollection<CodeView> UnitView = null;

        // 콤보박스 세팅
        private void comboBoxSetting()
        {

            //그리드안에 에너지콤보박스
            List<String[]> energycbo = new List<string[]>();
            string[] Type1 = new string[] { "00", "전기" };
            string[] Type2 = new string[] { "01", "가스" };
            energycbo.Add(Type1);
            energycbo.Add(Type2);

            //그리드안에 내용 : 에너지 단위 콤보박스 임의지정
            List<String[]> unitcbo = new List<string[]>();
            string[] Type3 = new string[] { "00", "kWh" };
            string[] Type4 = new string[] { "01", "MJ" };
            unitcbo.Add(Type3);
            unitcbo.Add(Type4);

            ovcProductFormIDView = ComboBoxUtil.Instance.Direct_SetComboBox(energycbo);
            UnitView = ComboBoxUtil.Instance.Direct_SetComboBox(unitcbo);

            //ObservableCollection<CodeView> cboEnergy = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ENERGY", "Y", "", "");
            //cboEnergySearch.ItemsSource = cboEnergy;
            //cboEnergySearch.DisplayMemberPath = "code_name";
            //cboEnergySearch.SelectedValuePath = "code_id";
        }

        private void SetCombobox()
        {
            //에너지구분 
            ObservableCollection<CodeView> cboEnergy = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ENERGY", "Y", "", "");
            cboEnergySearch.ItemsSource = cboEnergy;
            cboEnergySearch.DisplayMemberPath = "code_name";
            cboEnergySearch.SelectedValuePath = "code_id";
            cboEnergySearch.SelectedIndex = 0;

            //에너지단위
            ObservableCollection<CodeView> cboEnergyUnit = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ENERGYUnit", "Y", "", "");
            this.cboEnergyUnit.ItemsSource = cboEnergyUnit;
            this.cboEnergyUnit.DisplayMemberPath = "code_name";
            this.cboEnergyUnit.SelectedValuePath = "code_id";
            this.cboEnergyUnit.SelectedIndex = 0;
        }


        //DataGrid내부에 콤보박스 생성
        private void cboProductFormID_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            comboBox.ItemsSource = ovcProductFormIDView;
            comboBox.DisplayMemberPath = "code_name";
            comboBox.SelectedValuePath = "code_id";
            comboBox.IsDropDownOpen = true;
            sender = (object)comboBox;
        }

        string strPlanDateGet = string.Empty;
        string strFromDateGet = string.Empty;
        string strToDateGet = string.Empty;
        string strFlag = string.Empty;
        int rowNum = 0;

        Win_ord_EnergyUse_U_CodeView2 Energy = new Win_ord_EnergyUse_U_CodeView2();


        #region 기간

        //전월
        private void BtnLastMonth_Click(object sender, RoutedEventArgs e)
        {
            //dtpFromDate.SelectedDate = Lib.Instance.BringLastMonthDatetimeList()[0];
            //dtpToDate.SelectedDate = Lib.Instance.BringLastMonthDatetimeList()[1];

            if (dtpFromDate.SelectedDate != null)
            {
                DateTime ThatMonth1 = dtpFromDate.SelectedDate.Value.AddDays(-(dtpFromDate.SelectedDate.Value.Day - 1)); // 선택한 일자 달의 1일!

                DateTime LastMonth1 = ThatMonth1.AddMonths(-1); // 저번달 1일
                DateTime LastMonth31 = ThatMonth1.AddDays(-1); // 저번달 말일

                dtpFromDate.SelectedDate = LastMonth1;
                dtpToDate.SelectedDate = LastMonth31;
            }
            else
            {
                DateTime ThisMonth1 = DateTime.Today.AddDays(-(DateTime.Today.Day - 1)); // 이번달 1일

                DateTime LastMonth1 = ThisMonth1.AddMonths(-1); // 저번달 1일
                DateTime LastMonth31 = ThisMonth1.AddDays(-1); // 저번달 말일

                dtpFromDate.SelectedDate = LastMonth1;
                dtpToDate.SelectedDate = LastMonth31;
            }
        }
        //금월
        private void BtnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpFromDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpToDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];
        }
        //전일
        private void BtnYesterday_Click(object sender, RoutedEventArgs e)
        {
            //dtpFromDate.SelectedDate = DateTime.Today.AddDays(-1);
            //dtpToDate.SelectedDate = DateTime.Today.AddDays(-1);

            if (dtpFromDate.SelectedDate != null)
            {
                dtpFromDate.SelectedDate = dtpFromDate.SelectedDate.Value.AddDays(-1);
                dtpToDate.SelectedDate = dtpFromDate.SelectedDate;
            }
            else
            {
                dtpFromDate.SelectedDate = DateTime.Today.AddDays(-1);
                dtpToDate.SelectedDate = DateTime.Today.AddDays(-1);
            }
        }
        //금일
        private void BtnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpFromDate.SelectedDate = DateTime.Today;
            dtpToDate.SelectedDate = DateTime.Today;
        }
        //날짜 라벨 이벤트
        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkInOutDate.IsChecked == true)
            {
                chkInOutDate.IsChecked = false;
            }
            else
            {
                chkInOutDate.IsChecked = true;
            }
        }
        //날짜 체크박스 이벤트
        private void ChkInOutDate_Checked(object sender, RoutedEventArgs e)
        {
            chkInOutDate.IsChecked = true;
            dtpFromDate.IsEnabled = true;
            dtpToDate.IsEnabled = true;

            //btnYesterday.IsEnabled = true;
            //btnToday.IsEnabled = true;
            btnLastMonth.IsEnabled = true;
            btnThisMonth.IsEnabled = true;

            // 오늘날짜 세팅하기
            dtpFromDate.SelectedDate = DateTime.Today;
            dtpToDate.SelectedDate = DateTime.Today;
        }
        //날짜 체크박스 이벤트
        private void ChkInOutDate_Unchecked(object sender, RoutedEventArgs e)
        {
            chkInOutDate.IsChecked = false;
            dtpFromDate.IsEnabled = false;
            dtpToDate.IsEnabled = false;

            //btnYesterday.IsEnabled = false;
            //btnToday.IsEnabled = false;
            btnLastMonth.IsEnabled = false;
            btnThisMonth.IsEnabled = false;
        }
        #endregion

        //추가/수정 완료 후
        private void CanBtnControl()
        {
            Lib.Instance.UiButtonEnableChange_IUControl(this);

            SubAdd.Visibility = Visibility.Hidden;
            SubDel.Visibility = Visibility.Hidden;

        }

        //추가/수정 진행 중
        private void CantBtnControl()
        {
            Lib.Instance.UiButtonEnableChange_SCControl(this);

        }

        #region 상단 조회 조건

        private void lblTotSearch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkTotSearch.IsChecked == true) { chkTotSearch.IsChecked = false; }
            else { chkTotSearch.IsChecked = true; }
        }

        private void chkTotSearch_Checked(object sender, RoutedEventArgs e)
        {
            cboTotSearch.IsEnabled = true;
            txtTotSearch.IsEnabled = true;
        }

        private void chkTotSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            cboTotSearch.IsEnabled = false;
            txtTotSearch.IsEnabled = false;
        }

        private void txtBox_EnterAndSearch(object sender, KeyEventArgs e)
        {

        }



        #endregion

        //추가 클릭
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMain.Items.Count > 0)
            {
                dgdMain.Items.Clear();
            }

           //Energy = dgdMain.SelectedItem as Win_ord_EnergyUse_U_CodeView2;

            strFlag = "I";
            this.DataContext = null;
            CantBtnControl();

           
            SubAdd.Visibility = Visibility.Visible;
            SubDel.Visibility = Visibility.Visible;

            tbkMsg.Text = "자료 입력 중";
            rowNum = 0;
            try
            {
                int i = 1;

                if (dgdMain.Items.Count > 0)
                    i = dgdMain.Items.Count + 1;

                var selectedName = ovcProductFormIDView[0].code_name;

                var Energy = new Win_ord_EnergyUse_U_CodeView2()
                {
                    Num = i++,
                    CstYYYYMM = dtpToDate.SelectedDate.Value.ToString("yyyyMM"),
                    CstYYYYMM_CV = dtpToDate.SelectedDate.Value.ToString("yyyy-MM"),
                    gbnEnergy = selectedName,
                    UnitEnergy = "kWh",
                    cstElectQty = "",
                    cstElectAmount = "",
                    CreateDate = "",
                    CreateUserID = "",
                    LastUpdateDate = "",
                    LastUpdateUserID = "",
                    Comments = ""
                };

                dgdMain.Items.Add(Energy);

            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 지점 -" + ex.ToString());
            }
        }


        //수정 클릭
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            Energy = dgdMain.SelectedItem as Win_ord_EnergyUse_U_CodeView2;
            
            if (Energy != null)
            {
                //// 삭제 보관 리스트 초기화
                //lstDeleteRow.Clear();

                SubAdd.Visibility = Visibility.Visible;
                SubDel.Visibility = Visibility.Visible;

                rowNum = dgdMain.SelectedIndex;
                tbkMsg.Text = "자료 수정 중";
                strFlag = "U";
                CantBtnControl();
               
            }
            else
            {
                MessageBox.Show("수정할 데이터를 선택해주세요.");
                return;
            }
        }

        //삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var Energy = dgdMain.SelectedItem as Win_ord_EnergyUse_U_CodeView2;

            if (Energy != null)
            {
                if (MessageBox.Show("선택한 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {

                    if (DeleteData(Energy.cstID))
                    {
                        rowNum = 0;
                        re_Search(rowNum);
                    }
                }
            }
            else
            {
                MessageBox.Show("삭제할 데이터를 선택해주세요.");
            }
        }
        //취소
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            Lib.Instance.ChildMenuClose(this.ToString());

        }
        //검색
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            rowNum = 0;
            re_Search(rowNum);
        }
        //저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Energy = dgdMain.SelectedItem as Win_ord_EnergyUse_U_CodeView2;

            if (SaveData(strFlag))
            {
                if (strFlag.Equals("I"))
                {
                    rowNum = 0;
                }

                CanBtnControl();
                strFlag = "";
                re_Search(rowNum);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CanBtnControl();

            if (!strFlag.Equals(string.Empty))
            {
                if (!strFlag.Trim().Equals("U"))
                {
                    rowNum = 0;
                }

                strFlag = string.Empty;
                re_Search(rowNum);
            }

            strFlag = "";

            //dgdMain.IsHitTestVisible = true;
        }
        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            System.Data.DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[2];
            lst[0] = "에너지사용량";
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

        //삭제
        private bool DeleteData(string cstID)
        {
            bool flag = false;

            Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
            sqlParameter.Clear();
            sqlParameter.Add("cstID", cstID);

            try
            {
                //string[] result = DataStore.Instance.ExecuteProcedure("xp_Ord_Energy_dEnergy", sqlParameter, true);
                string[] result = DataStore.Instance.ExecuteProcedure_NewLog("xp_Ord_Energy_dEnergy", sqlParameter, "D");
                if (!result[0].Equals("success"))
                {
                    MessageBox.Show("삭제 실패");
                    flag = false;
                }
                else
                {
                    //MessageBox.Show("성공적으로 삭제되었습니다.");
                    flag = true;
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

            return flag;
        }

        //재조회
        private void re_Search(int selectedIndex)
        {
            dgdMain.Items.Clear();

            FillGrid();

            if (dgdMain.Items.Count > 0)
            {
                dgdMain.SelectedIndex = selectedIndex;
            }
            else
            {
                this.DataContext = null;
            }
        }

        //조회
        private void FillGrid()
        {
            if (dgdMain.Items.Count > 0)
            {
                dgdMain.Items.Clear();
            }
            if (dgdSum.Items.Count > 0)
            {
                dgdSum.Items.Clear();
            }

            try
            {
                var EnergySum = new Win_ord_EnergyUse_U_EnergySum();

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("nChkDate", chkInOutDate.IsChecked == true ? 1 : 0);
                sqlParameter.Add("sSDate", dtpFromDate.SelectedDate != null ? dtpFromDate.SelectedDate.Value.ToString("yyyyMM") : "");
                sqlParameter.Add("sEDate", dtpToDate.SelectedDate != null ? dtpToDate.SelectedDate.Value.ToString("yyyyMM") : "");
                sqlParameter.Add("chkCodeID", chkEnergySearch.IsChecked == true ? 1 : 0);
                sqlParameter.Add("sCodeID", cboEnergySearch.SelectedValue);

                //DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Ord_Energy_sEnergy", sqlParameter, false);
                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_Ord_Energy_sEnergy", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    System.Data.DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("조회된 데이터가 없습니다.");
                        Win_ord_EnergyUse_U_CodeView2 Empty = new Win_ord_EnergyUse_U_CodeView2();
                        this.DataContext = Empty;
                    }
                    else
                    {
                        int i = 0;
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;
                            var Energy = new Win_ord_EnergyUse_U_CodeView2
                            {
                                Num = i,
                                cstID = dr["cstID"].ToString(), //cstID가져오기
                                CstYYYYMM = dr["cstYYYYMM"].ToString(),
                                CstYYYYMM_CV = DatePickerFormat(dr["cstYYYYMM"].ToString()),
                                gbnEnergy = dr["gbnEnergy"].ToString(),
                                UnitEnergy = dr["UnitEnergy"].ToString(),
                                cstElectQty = stringFormatN0(dr["cstElectQty"]),
                                cstElectAmount = stringFormatN0(dr["cstElectAmount"]),
                                CreateDate = dr["CreateDate"].ToString(),
                                CreateUserID = dr["CreateUserID"].ToString(),
                                LastUpdateDate = dr["LastUpdateDate"].ToString(),
                                LastUpdateUserID = dr["LastUpdateUserID"].ToString(),
                                Comments = dr["Comments"].ToString()
                            };

                            //Energy.cstElectQty = lib.returnNumStringZero(Energy.cstElectQty);

                            dgdMain.Items.Add(Energy);

                            EnergySum.gbnEnergy = Energy.gbnEnergy;
                            EnergySum.UnitEnergy = Energy.UnitEnergy;
                            EnergySum.cstElectQtySum += ConvertDouble(Energy.cstElectQty);
                            EnergySum.cstElectAmountSum += ConvertDouble(Energy.cstElectAmount);    
              
                        }
                        dgdSum.Items.Add(EnergySum);
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


        private List<Win_ord_EnergyUse_U_CodeView2> lstDeleteRow = new List<Win_ord_EnergyUse_U_CodeView2>();

        //저장
        private bool SaveData(string strFlag)
        {
            bool flag = false;

            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                if (CheckData())
                {
                    #region 추가

                    if (strFlag.Equals("I"))
                    {

                        for (int i = 0; i < dgdMain.Items.Count; i++)
                        {
                            
                            Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();

                            var Energy = dgdMain.Items[i] as Win_ord_EnergyUse_U_CodeView2;
                            var selectedID = ovcProductFormIDView[0].code_id;

                            sqlParameter.Add("cstID", "");
                            sqlParameter.Add("cstYYYYMM", Energy.CstYYYYMM);
                            sqlParameter.Add("cstElectQty", ConvertDouble(Energy.cstElectQty));
                            sqlParameter.Add("cstElectAmount", ConvertDouble(Energy.cstElectAmount));
                            sqlParameter.Add("gbnEnergy", Energy.ProductFormID == null ? selectedID : Energy.ProductFormID); //클릭하지 않아도 에너지 코드id 등록 될수 있게 함.

                            sqlParameter.Add("UnitEnergy", Energy.UnitEnergy);
                            //sqlParameter.Add("CreateDate", "");
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);
                            //sqlParameter.Add("LastUpdateDate", "");
                            sqlParameter.Add("LastUpdateUserID", "");

                            sqlParameter.Add("Comments", Energy.Comments);
                            Procedure pro1 = new Procedure();
                            pro1.Name = "xp_Ord_Energy_iEnergy";
                            pro1.OutputUseYN = "N";
                            pro1.OutputName = "cstID";
                            pro1.OutputLength = "12";

                            Prolist.Add(pro1);
                            ListParameter.Add(sqlParameter);

                        }
       

                    }
                    else if (strFlag.Equals("U"))
                    {
                        Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                     
                        for (int i = 0; i < dgdMain.Items.Count; i++)
                        {
                            var Energy = dgdMain.Items[i] as Win_ord_EnergyUse_U_CodeView2;
                            var selectedID = ovcProductFormIDView[0].code_id;

                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();

                            sqlParameter.Add("cstID", Energy.cstID);
                            sqlParameter.Add("cstYYYYMM", Energy.CstYYYYMM);
                            sqlParameter.Add("cstElectQty", ConvertDouble(Energy.cstElectQty));
                            sqlParameter.Add("cstElectAmount", ConvertDouble(Energy.cstElectAmount));
                            sqlParameter.Add("gbnEnergy", Energy.ProductFormID == null ? selectedID : Energy.ProductFormID);

                            sqlParameter.Add("UnitEnergy", Energy.UnitEnergy);
                            sqlParameter.Add("Comments", Energy.Comments);
                            //sqlParameter.Add("CreateDate", DateTime.Today);
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);
                            sqlParameter.Add("LastUpdateDate", "");
                            sqlParameter.Add("LastUpdateUserID", "");

                            Procedure pro2 = new Procedure();
                            pro2.Name = "xp_Ord_Energy_uEnergy";
                            pro2.OutputUseYN = "N";
                            pro2.OutputName = "cstID";
                            pro2.OutputLength = "12";

                            Prolist.Add(pro2);
                            ListParameter.Add(sqlParameter);
                        }


                    }
                    #endregion

                    string[] Confirm = new string[2];
                    //Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew(Prolist, ListParameter);

                    Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew_NewLog(Prolist, ListParameter, "C");


                    if (Confirm[0] != "success")
                    {
                        MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
                        flag = false;
                        //return false;
                    }
                    else
                    {
                        flag = true;
                    }
                    Prolist.Clear();
                    ListParameter.Clear();
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

            return flag;
        }



        private bool CheckData()
        {
            bool flag = true;

            for (int i = 0; i < dgdMain.Items.Count; i++)
            {
                var dgdMainItems = dgdMain.Items[i] as Win_ord_EnergyUse_U_CodeView2;


                if (dgdMainItems.CstYYYYMM == string.Empty)
                {
                    MessageBox.Show("기준월을 입력 하여 주세요", "주의");
                    flag = false;
                    return flag;
                }
                else if (dgdMainItems.cstElectQty == string.Empty || dgdMainItems.cstElectQty == null)
                {
                    MessageBox.Show("이용량을 입력 하여 주세요", "주의");
                    flag = false;
                    return flag;
                }
                else if (dgdMainItems.cstElectAmount == string.Empty)
                {
                    MessageBox.Show("이용금액을 입력 하여 주세요", "주의");
                    flag = false;
                    return flag;
                }
            }
            return flag;
        }
        #region DataGrid 포커스 이동 및 편집모드

        private void DataGird_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
                {
                    DataGird_KeyDown(sender, e);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("포커스 이동관련 오류입니다.내용 : " + ex.ToString());
            }
        }

        private void DataGird_KeyDown(object sender, KeyEventArgs e)
        {
            var MainItem = dgdMain.CurrentItem as Win_ord_EnergyUse_U_CodeView2;
            int currRow = dgdMain.Items.IndexOf(dgdMain.CurrentItem);
            int currCol = dgdMain.Columns.IndexOf(dgdMain.CurrentCell.Column);
            int startCol = 1;
            int endCol = 10;

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (endCol == currCol && dgdMain.Items.Count - 1 > currRow)
                {
                    dgdMain.SelectedIndex = currRow + 1;
                    dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[currRow + 1], dgdMain.Columns[startCol]);
                }
                else if (endCol > currCol && dgdMain.Items.Count - 1 > currRow)
                {
                    btnSave.Focus();
                    //dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[currRow], dgdMain.Columns[currCol + 1]);
                }
                else if (endCol == currCol && dgdMain.Items.Count - 1 == currRow)
                {
                    btnSave.Focus();
                }
                else if (endCol > currCol && dgdMain.Items.Count - 1 == currRow)
                {
                    btnSave.Focus();
                    //dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[currRow], dgdMain.Columns[currCol + 1]);
                }
                else
                {
                    MessageBox.Show("");
                }
            }
            else if (e.Key == Key.Down)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                // 마지막 행 아님
                if (dgdMain.Items.Count - 1 > currRow)
                {
                    dgdMain.SelectedIndex = currRow + 1;
                    dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[currRow + 1], dgdMain.Columns[currCol]);
                } // 마지막 행일때
                else if (dgdMain.Items.Count - 1 == currRow)
                {
                    if (endCol > currCol) // 마지막 열이 아닌 경우, 열을 오른쪽으로 이동
                    {
                        //dgdProcess.SelectedIndex = 0;
                        dgdMain.SelectedIndex = 0;
                        dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[currRow], dgdMain.Columns[currCol + 1]);
                    }
                    else
                    {
                        //btnSave.Focus();
                    }
                }
            }
            else if (e.Key == Key.Up)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                // 첫행 아님
                if (currRow > 0)
                {
                    dgdMain.SelectedIndex = currRow - 1;
                    dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[currRow - 1], dgdMain.Columns[currCol]);
                } // 첫 행
                //else if (dgdMain.Items.Count - 1 == currRow)
                //{
                //    if (0 < currCol) // 첫 열이 아닌 경우, 열을 왼쪽으로 이동
                //    {
                //        //dgdProcess.SelectedIndex = 0;
                //        dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[currRow], dgdMain.Columns[currCol - 1]);
                //    }
                //    else
                //    {
                //        //btnSave.Focus();
                //    }
                //}
            }
            else if (e.Key == Key.Left)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                //if (startCol < currCol)
                //{
                //    dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[currRow], dgdMain.Columns[currCol - 1]);
                //}
                //else if (startCol == currCol)
                //{
                //    if (0 < currRow)
                //    {
                //        dgdMain.SelectedIndex = currRow - 1;
                //        dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[currRow - 1], dgdMain.Columns[endCol]);
                //    }
                //    else
                //    {
                //        //btnSave.Focus();
                //    }
                //}

                if (currRow > 0)
                {
                    dgdMain.SelectedIndex = currRow - 1;
                    dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[currRow - 1], dgdMain.Columns[currCol]);
                }
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (endCol > currCol)
                {

                    dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[currRow], dgdMain.Columns[currCol + 1]);
                }
                else if (endCol == currCol)
                {
                    if (dgdMain.Items.Count - 1 > currRow)
                    {
                        dgdMain.SelectedIndex = currRow + 1;
                        dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[currRow + 1], dgdMain.Columns[startCol]);
                    }
                    else
                    {
                        //btnSave.Focus();
                    }
                }
            }
        }

        private void DatagridIn_TextFocus(object sender, KeyEventArgs e)
        {
            //Lib.Instance.DataGridINTextBoxFocus(sender, e);
            //Lib.Instance.DataGridINControlContainFocus(sender, e);
            Lib.Instance.DataGridINControlFocus(sender, e);
        }

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                DataGridCell cell = sender as DataGridCell;
                cell.IsEditing = true;
            }
        }

        private void DataGridCell_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell.Column.DisplayIndex != 7)
            {
              Lib.Instance.DataGridINBothByMouseUP(sender, e);
            }
            //Lib.Instance.DataGridINControlContainByMouseUP(sender, e);
        }



        #endregion


        

        #region 기타 메서드 모음

        // 천 단위 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }

        // 천 단위 콤마, 소수점 두자리
        private string stringFormatN2(object obj)
        {
            return string.Format("{0:N2}", obj);
        }

        private string stringFormatcst(object obj)
        {
            return string.Format("{%s}", obj);
        }

        // 데이터피커 포맷으로 변경
        private string DatePickerFormat(string str)
        {
            string result = "";

            if (str.Length == 6)
            {
                if (!str.Trim().Equals(""))
                {
                    result = str.Substring(0, 4) +"-"+ str.Substring(4, 2);
                        //+ "-" + str.Substring(6, 2);
                }
            }
            if (str.Length == 22)
            {
                if (!str.Trim().Equals(""))
                {
                    result = str.Substring(0, 4) + "-" + str.Substring(4, 2);
                    //+ "-" + str.Substring(6, 2);
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


        //숫자 외에 다른 문자열 못들어오도록
        public bool IsNumeric(string source)
        {

            Regex regex = new Regex("[^0-9.-]+");
            return !regex.IsMatch(source);
        }

        //단가 숫자 이외 문자 못들어가게
        private void UnitPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Lib.Instance.CheckIsNumeric((System.Windows.Controls.TextBox)sender, e);
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - txtOrderID_PreviewTextInput : " + ee.ToString());
            }
        }
        #endregion


        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var grdMain = dgdMain.CurrentItem as Win_ord_EnergyUse_U_CodeView2;

            if (e.Key == Key.Enter)
            {
                System.Windows.Controls.TextBox OrderNo = sender as System.Windows.Controls.TextBox;

                MainWindow.pf.ReturnCode(OrderNo, 5006, "");

            }
        }

        // ArticleID 로 Article 정보 가져오기
        private ArticleInform getOrder(string OrderNo)
        {
            var getArticleInform = new ArticleInform();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("OrderID", OrderNo);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_ord_DailyPlan_OrderID", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    System.Data.DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr = dt.Rows[0];

                        getArticleInform = new ArticleInform
                        {
                            Customname = dr["Customname"].ToString(),
                            Article = dr["Article"].ToString(),
                            OrderQty = dr["OrderQty"].ToString(),
                            DvlyDate = dr["DvlyDate"].ToString(),
                            ArticleID = dr["ArticleID"].ToString(),

                        };
                    }
                }

                return getArticleInform;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private void SubAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int i = 1;

                if (dgdMain.Items.Count > 0)
                    i = dgdMain.Items.Count + 1;

                var selectedName = ovcProductFormIDView[0].code_name;

                var Energy = new Win_ord_EnergyUse_U_CodeView2()
                {
                    Num = i++,
                    CstYYYYMM = DateTime.Today.ToString("yyyyMM"),
                    CstYYYYMM_CV = DateTime.Today.ToString("yyyy-MM"),
                    //Ddate_CV = dtpDueDate.SelectedDate != null ? dtpDueDate.SelectedDate.Value.ToString("yyyy-MM-dd") : "",
                    //cstYYYYMM = dtpToDate.SelectedDate.Value.ToString("yyyyMM"),
                    gbnEnergy = selectedName,
                    UnitEnergy = "kWH",
                    cstElectQty = "",
                    cstElectAmount = "",
                    CreateDate = "",
                    CreateUserID = "",
                    LastUpdateDate = "",
                    LastUpdateUserID = "",
                    Comments = ""

                };

                dgdMain.Items.Add(Energy);

            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 지점 -" + ex.ToString());
            }
        }

        private void SubDel_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (dgdMain.Items.Count > 0)
                {
                    if (dgdMain.SelectedItem != null)
                    {

                        Win_ord_EnergyUse_U_CodeView2 deleteRow = null;

                        if (dgdMain.CurrentItem != null)
                        {
                            //dgdMain.Items.Remove(dgdMain.CurrentItem as Win_ord_EnergyUse_U_CodeView2);
                            deleteRow = dgdMain.CurrentItem as Win_ord_EnergyUse_U_CodeView2;
                        }
                        else
                        {
                            //dgdMain.Items.Remove((dgdMain.Items[dgdMain.SelectedIndex]) as Win_ord_EnergyUse_U_CodeView2);
                            deleteRow = (dgdMain.Items[dgdMain.SelectedIndex]) as Win_ord_EnergyUse_U_CodeView2;
                        }

                        // 삭제 행 보관
                        // 1. 수정 상태일때
                        // 2. 기본키값이 널값이 아닐때
                        if (strFlag.Trim().Equals("U")
                            && deleteRow.cstID != null
                            && !deleteRow.cstID.Trim().Equals(""))
                        {
                            lstDeleteRow.Add(deleteRow);
                        }

                        dgdMain.Items.Remove(deleteRow);

                        dgdMain.Refresh();
                    }
                }

            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContextMenu menu = btnPrint.ContextMenu;
                menu.StaysOpen = true;
                menu.IsOpen = true;
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        #region 인쇄관련 메서드
        //인쇄-미리보기
        //private void menuSeeAhead_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (dgdMain.Items.Count < 1)
        //        {
        //            MessageBox.Show("먼저 검색해 주세요.");
        //            return;
        //        }

        //        msg.Show();
        //        msg.Topmost = true;
        //        msg.Refresh();

        //        PrintWork(true);
        //    }
        //    catch (Exception ee)
        //    {
        //        MessageBox.Show("오류지점 - " + ee.ToString());
        //    }
        //}

        ////인쇄-바로인쇄
        //private void menuRightPrint_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (dgdMain.Items.Count < 1)
        //        {
        //            MessageBox.Show("먼저 검색해 주세요.");
        //            return;
        //        }

        //        msg.Show();
        //        msg.Topmost = true;
        //        msg.Refresh();

        //        PrintWork(false);
        //    }
        //    catch (Exception ee)
        //    {
        //        MessageBox.Show("오류지점 - " + ee.ToString());
        //    }
        //}

        ////인쇄-닫기
        //private void menuClose_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        ContextMenu menu = btnPrint.ContextMenu;
        //        menu.StaysOpen = false;
        //        menu.IsOpen = false;
        //    }
        //    catch (Exception ee)
        //    {
        //        MessageBox.Show("오류지점 - " + ee.ToString());
        //    }
        //}
        //private void PrintWork(bool preview_click)
        //{
        //    try
        //    {
        //        excelapp = new Microsoft.Office.Interop.Excel.Application();

        //        string MyBookPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Report\\생산 및 출고진행 관리.xls";
        //        workbook = excelapp.Workbooks.Add(MyBookPath);
        //        worksheet = workbook.Sheets["Form"];

        //        //일자 조건
        //        if (chkInOutDate.IsChecked == true)
        //        {
        //            //StartDate
        //            workrange = worksheet.get_Range("D3", "I3");
        //            workrange.Value2 = dtpFromDate.Text;
        //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
        //            //workrange.Font.Size = 10;

        //            //EndDate
        //            workrange = worksheet.get_Range("K3", "P3");
        //            workrange.Value2 = dtpToDate.Text;
        //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
        //            //workrange.Font.Size = 10;
        //        }
        //        else
        //        {
        //            workrange = worksheet.get_Range("D3", "I3");
        //            workrange.Value2 = "전체";
        //            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
        //            //workrange.Font.Size = 10;
        //        }

        //        /////////////////////////
        //        int Page = 0;
        //        int DataCount = 0;
        //        int copyLine = 0;

        //        copysheet = workbook.Sheets["Form"];
        //        pastesheet = workbook.Sheets["Print"];

        //        Lib lib = new Lib();
        //        System.Data.DataTable DT = lib.DataGirdToDataTable(dgdMain);

        //        string str_PlanDate = string.Empty;
        //        string str_PlanDate_CV = string.Empty;
        //        string str_OrderNO = string.Empty;
        //        string str_Kcustom = string.Empty;
        //        string str_Article = string.Empty;
        //        string str_OrderQty = string.Empty;
        //        string str_DvlyDate_CV = string.Empty;
        //        string str_PlanProdQty = string.Empty;
        //        string str_ProdComments = string.Empty;
        //        string str_PlanOutwareQty = string.Empty;
        //        string str_OutwareComments = string.Empty;
        //        string str_Comments = string.Empty;

        //        int ToralCnt = dgdMain.Items.Count;
        //        int canInsert = 27; //데이터가 입력되는 행 수 27개

        //        int PageCount = (int)Math.Ceiling(1.0 * ToralCnt / canInsert);

        //        for (int k = 0; k < PageCount; k++)
        //        {
        //            Page++;
        //            if (Page != 1) { DataCount++; } //+1
        //            copyLine = (Page - 1) * 38;
        //            copysheet.Select();
        //            copysheet.UsedRange.Copy();
        //            pastesheet.Select();
        //            workrange = pastesheet.Cells[copyLine + 1, 1];
        //            workrange.Select();
        //            pastesheet.Paste();

        //            int j = 0;
        //            for (int i = DataCount; i < dgdMain.Items.Count; i++)
        //            {
        //                if (j == 27) { break; }
        //                int insertline = copyLine + 7 + j;

        //                var gridData = dgdMain.Items[i] as Win_ord_EnergyUse_U_CodeView2;

        //                //일자
        //                workrange = pastesheet.get_Range("A" + insertline, "D" + insertline);
        //                workrange.Value2 = gridData.cstYYYYMM;
        //                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
        //                workrange.Font.Size = 9;

        //                //오더번호
        //                workrange = pastesheet.get_Range("E" + insertline, "I" + insertline);
        //                workrange.Value2 = gridData.OrderNO;
        //                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
        //                workrange.Font.Size = 9;

        //                //거래처
        //                workrange = pastesheet.get_Range("J" + insertline, "O" + insertline);
        //                workrange.Value2 = gridData.Kcustom;
        //                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
        //                workrange.Font.Size = 9;

        //                //품명
        //                workrange = pastesheet.get_Range("P" + insertline, "U" + insertline);
        //                workrange.Value2 = gridData.Article;
        //                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
        //                workrange.Font.Size = 9;

        //                //오더량
        //                workrange = pastesheet.get_Range("V" + insertline, "X" + insertline);
        //                workrange.Value2 = gridData.cstElectQty;
        //                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
        //                workrange.Font.Size = 9;

        //                //납기 일자
        //                workrange = pastesheet.get_Range("Y" + insertline, "AB" + insertline);
        //                workrange.Value2 = gridData.DvlyDate_CV;
        //                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
        //                workrange.Font.Size = 9;

        //                //예정 생산량
        //                workrange = pastesheet.get_Range("AC" + insertline, "AE" + insertline);
        //                workrange.Value2 = gridData.PlanProdQty;
        //                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
        //                workrange.Font.Size = 9;

        //                //생산주의
        //                workrange = pastesheet.get_Range("AF" + insertline, "AH" + insertline);
        //                workrange.Value2 = gridData.ProdComments;
        //                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
        //                workrange.Font.Size = 9;

        //                //예정 출하량
        //                workrange = pastesheet.get_Range("AI" + insertline, "AK" + insertline);
        //                workrange.Value2 = gridData.PlanOutwareQty;
        //                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignRight;
        //                workrange.Font.Size = 9;

        //                //출하 주의
        //                workrange = pastesheet.get_Range("AL" + insertline, "AN" + insertline);
        //                workrange.Value2 = gridData.OutwareComments;
        //                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
        //                workrange.Font.Size = 9;

        //                //비고
        //                workrange = pastesheet.get_Range("AO" + insertline, "AR" + insertline);
        //                workrange.Value2 = gridData.Comments;
        //                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
        //                workrange.Font.Size = 9;

        //                DataCount = i;
        //                j++;
        //            }

        //            // 합계 출력
        //            int totalLine = 34 + ((Page - 1) * 38);
        //        }

        //        pastesheet.PageSetup.TopMargin = 0;
        //        pastesheet.PageSetup.BottomMargin = 0;
        //        //pastesheet.PageSetup.Zoom = 43;

        //        msg.Hide();

        //        if (preview_click == true)
        //        {
        //            excelapp.Visible = true;
        //            pastesheet.PrintPreview();
        //        }
        //        else
        //        {
        //            excelapp.Visible = true;
        //            pastesheet.PrintOutEx();
        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
        //    }
        //    finally
        //    {
        //        DataStore.Instance.CloseConnection();
        //    }
        //}
        #endregion

        private void chkEnergySearch_Checked(object sender, RoutedEventArgs e)
        {
            cboEnergySearch.IsEnabled = true;
        }

        private void chkEnergySearch_Unchecked(object sender, RoutedEventArgs e)
        {
            cboEnergySearch.IsEnabled = false;
        }

        private void EnergySearch_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            if (chkEnergySearch.IsChecked == false)
            {
                chkEnergySearch.IsChecked = true;
            }
            else
            {
                chkEnergySearch.IsChecked = false;
            }
        }

        private void EnergyUnit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(chkEnergyUnit.IsChecked == false)
            {
                chkEnergyUnit.IsChecked = true;
                
            }
            else
                chkEnergyUnit.IsChecked = false;

        }

        private void chkEnergyUnit_Checked(object sender, RoutedEventArgs e)
        {
            cboEnergyUnit.IsEnabled = true;
        }

        private void chkEnergyUnit_Unchecked(object sender, RoutedEventArgs e)
        {
            cboEnergyUnit.IsEnabled = false;
        }

        //추가할때 에너지구분을 선택하면 단위에다가 넣을 수 있게. 
        private void cboProductFormID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Energy = dgdMain.CurrentItem as Win_ord_EnergyUse_U_CodeView2;
            try
            {
                if (Energy.ProductFormID == "00")
                {
                    Energy.UnitEnergy = UnitView[0].code_name;
                }
                else
                    Energy.UnitEnergy = UnitView[1].code_name;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void tblcstElectQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            e.Handled = regex.IsMatch(e.Text);
            
        }

        private void tblElectAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void dgdMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
          
        }

        private void dpcstYYYYMM_CalendarClosed(object sender, RoutedEventArgs e)
        {
            DatePicker dtpSender = sender as DatePicker;

            var Energy = dtpSender.DataContext as Win_ord_EnergyUse_U_CodeView2;

            try
            {

                if (Energy != null
                    || dtpSender != null)
                {
                    Energy.CstYYYYMM_CV = dtpSender.SelectedDate.Value.ToString("yyyy-MM");
                    Energy.CstYYYYMM = dtpSender.SelectedDate.Value.ToString("yyyyMM");
                }

                if (Energy == null
                || dtpSender == null)
                {
                    MessageBox.Show("날짜를 선택해주세요");
                    Energy.CstYYYYMM_CV = DateTime.Today.ToString("yyyy-MM");
                    Energy.CstYYYYMM = DateTime.Today.ToString("yyyyMM");

                }

            }
            catch(Exception ex)
            {
                MessageBox.Show("날짜를 선택해주세요");
            }
               
        }

        private void DtpYYYYMM_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker dtpSender = sender as DatePicker;

            if (dtpSender == null
                || dtpSender.SelectedDate == null
                )
            {
                return;
            }
            //dtpSender.SelectedDate = dtpSender.SelectedDate;

        }
    }

    class Win_ord_EnergyUse_U_CodeView2 : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }
        public string cstID { get; set; }
        public string CstYYYYMM { get; set; }
        public string CstYYYYMM_CV { get; set; } //datepicker로 형변환한 변수
        public string gbnEnergy { get; set; }
        public string UnitEnergy { get; set; }
        public string ProductFormID { get; set; }
        public string cstElectQty { get; set; }
        public string cstElectAmount { get; set; }
        public string CreateDate { get; set; }
        public string CreateUserID { get; set; }
        public string LastUpdateDate { get; set; }
        public string LastUpdateUserID { get; set; }
        public string Comments { get; set; }


    }

    class Win_ord_EnergyUse_U_EnergySum  : Win_ord_EnergyUse_U_CodeView2
    { 
        public double cstElectQtySum { get; set; }
        public double cstElectAmountSum { get; set; }

    }

    class ArticleInform : BaseView
    {
        public string CustomID { get; set; }
        public string DvlyDate { get; set; }
        public string ArticleID { get; set; }
        public string OrderQty { get; set; }
        public string Customname { get; set; }
        public string Article { get; set; }

    }
}


