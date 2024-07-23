/**
 * 
 * @details 공정별 호기코드 등록
 * @author 정승학
 * @date 2019-07-29
 * @version 1.0
 * 
 * @section MODIFYINFO 수정정보
 * - 수정일        - 수정자       : 수정내역
 * - 2000-01-01    - 정승학       : -----
 * 
 * 
 * */

using System;
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
    /// Win_prd_MachineCode_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_prd_MachineCode_U : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        Lib lib = new Lib();
        PlusFinder pf = MainWindow.pf;

        string strFlag = string.Empty;
        int rowNum = 0;
        int rowNumProcessMachine = 0;

        //네임
        Win_prd_MachineCode_U_CodeView ProcessName = new Win_prd_MachineCode_U_CodeView();
        Win_prd_MachineCode_ProcessMachine_CodeView ProcessMachine = new Win_prd_MachineCode_ProcessMachine_CodeView();

        public Win_prd_MachineCode_U()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            Lib.Instance.UiLoading(sender);
            SetComboBox();
        }

        #region 콤보박스
        private void SetComboBox()
        {
            //실적창고
            ObservableCollection<CodeView> ovcLOC = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "LOC", "Y", "", "");
            this.cboLoc.ItemsSource = ovcLOC;
            this.cboLoc.DisplayMemberPath = "code_name";
            this.cboLoc.SelectedValuePath = "code_id";

            //CommCollectionYN
            ComboBoxCommCollectionYN.Items.Clear();
            ComboBoxCommCollectionYN.Items.Add("Y");
            ComboBoxCommCollectionYN.Items.Add("N");
        }

        #endregion

        #region 검색조건

        // 공정명 검색
        private void lblProcessSrh_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chkProcessSrh.IsChecked == true)
            {
                chkProcessSrh.IsChecked = false;
            }
            else
            {
                chkProcessSrh.IsChecked = true;
            }
        }
        private void chkProcessSrh_Checked(object sender, RoutedEventArgs e)
        {
            chkProcessSrh.IsChecked = true;
            txtProcessSrh.IsEnabled = true;
            txtProcessSrh.Focus();
        }
        private void chkProcessSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            chkProcessSrh.IsChecked = false;
            txtProcessSrh.IsEnabled = false;
        }

        #endregion // 검색조건

        #region 우측 상단 버튼
        //수정, 추가 저장 후
        private void CanBtnControl()
        {
            //btnAdd.IsEnabled = true;
            //btnUpdate.IsEnabled = true;
            //btnDelete.IsEnabled = true;
            //btnSearch.IsEnabled = true;
            //btnSave.Visibility = Visibility.Hidden;
            //btnCancel.Visibility = Visibility.Hidden;
            //btnExcel.Visibility = Visibility.Visible;
            //gbxInput.IsEnabled = false;
            //lblMsg.Visibility = Visibility.Hidden;
            //Lib.Instance.UiButtonEnableChange_IUControl(this);


            btnAdd.IsEnabled = true;
            btnUpdate.IsEnabled = true;
            btnDelete.IsEnabled = true;
            btnSearch.IsEnabled = true;
            btnExcel.IsEnabled = true;
            btnSave.Visibility = Visibility.Hidden;
            btnCancel.Visibility = Visibility.Hidden;

            dgdProcessName.IsHitTestVisible = true;
            dgdProcessMachine.IsHitTestVisible = true;

            gbxInput.IsHitTestVisible = false;
            lblMsg.Visibility = Visibility.Hidden;

            //Lib.Instance.UiButtonEnableChange_SCControl(this);
        }

        //수정, 추가 진행 중
        private void CantBtnControl()
        {
            btnAdd.IsEnabled = false;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnSearch.IsEnabled = false;
            btnExcel.IsEnabled = false;
            btnSave.Visibility = Visibility.Visible;
            btnCancel.Visibility = Visibility.Visible;

            dgdProcessName.IsHitTestVisible = false;
            dgdProcessMachine.IsHitTestVisible = false;

            gbxInput.IsHitTestVisible = true;
            lblMsg.Visibility = Visibility.Visible;

            //Lib.Instance.UiButtonEnableChange_SCControl(this);
        }

        //TAG(BARCODE) 일단 빈값
        private void btnTag_Click(object sender, RoutedEventArgs e)
        {

        }

        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var Process = dgdProcessName.SelectedItem as Win_prd_MachineCode_U_CodeView;

            if (Process != null)
            {
                CantBtnControl();
                strFlag = "I";
                //dgdProcessName.IsEnabled = false;
                //dgdProcessMachine.IsEnabled = false;
                dgdProcessName.IsHitTestVisible = false;
                dgdProcessMachine.IsHitTestVisible = false;

                txtCode.IsReadOnly = false;
                txtMachineWorkStationName.IsReadOnly = false;
                txtMachineWorkStationNumName.IsReadOnly = false;
                txtSetHitCount.IsReadOnly = false;
                cboLoc.IsEnabled = true;
                TextBoxCommStationNo.IsReadOnly = false;
                TextBoxCommIP.IsReadOnly = false;
                ComboBoxCommCollectionYN.IsEnabled = true;

                lblMsg.Visibility = Visibility.Visible;
                tbkMsg.Text = "자료 입력 중";
                rowNum = dgdProcessName.SelectedIndex;
                rowNumProcessMachine = dgdProcessMachine.SelectedIndex;
                this.DataContext = null;

                txtCode.Focus();
            }
            else
            {
                MessageBox.Show("공정을 먼저 선택해 주세요.");
                return;
            }
        }

        //수정
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            ProcessName = dgdProcessName.SelectedItem as Win_prd_MachineCode_U_CodeView;

            if (ProcessName != null)
            {
                ProcessMachine = dgdProcessMachine.SelectedItem as Win_prd_MachineCode_ProcessMachine_CodeView;

                if (ProcessMachine != null)
                {
                    rowNum = dgdProcessName.SelectedIndex;
                    rowNumProcessMachine = dgdProcessMachine.SelectedIndex;

                    tbkMsg.Text = "자료 수정 중";
                    CantBtnControl();
                    strFlag = "U";
                }
                else
                {
                    MessageBox.Show("수정할 Machine(설비)정보를 선택해주세요");
                }
            }
            else
            {
                MessageBox.Show("먼저 공정을 선택한 후 수정할 Machine(설비)정보를 선택해주세요");
                return;
            }
        }

        //삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ProcessName = dgdProcessName.SelectedItem as Win_prd_MachineCode_U_CodeView;

            if (ProcessName != null)
            {
                rowNum = dgdProcessName.SelectedIndex;
                ProcessMachine = dgdProcessMachine.SelectedItem as Win_prd_MachineCode_ProcessMachine_CodeView;

                if (ProcessMachine == null)
                {
                    MessageBox.Show("삭제할 데이터가 지정되지 않았습니다. 삭제데이터를 지정하고 눌러주세요");
                }
                else
                {
                    if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (dgdProcessMachine.Items.Count > 0 && dgdProcessMachine.SelectedItem != null)
                        {
                            rowNumProcessMachine = dgdProcessMachine.SelectedIndex;
                        }

                        if (DeleteData(ProcessName.ProcessID, ProcessMachine.MachineID))
                        {
                            rowNumProcessMachine -= 1;
                            re_Search();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("먼저 공정을 선택한 후 삭제할 Machine(기계)정보를 선택해주세요");
                return;
            }
        }

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        //검색
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSearch.IsEnabled = false;

                rowNum = 0;
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

        //저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            using (Loading lw = new Loading(beSave))
            {
                lw.ShowDialog();
            }
        }

        private void beSave()
        {
            if (SaveData(strFlag, ProcessName.ProcessID))
            {
                rowNumProcessMachine = 0;

                re_Search();
                CanBtnControl();
                strFlag = string.Empty;
                lblMsg.Visibility = Visibility.Hidden;
            }
        }

        //취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            using (Loading lw = new Loading(beCancel))
            {
                lw.ShowDialog();
            }
        }

        private void beCancel()
        {
            CanBtnControl();
            strFlag = string.Empty;

            re_Search();
        }

        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[4];
            lst[0] = "공정";
            lst[1] = "공정별 Machine";
            lst[2] = dgdProcessName.Name;
            lst[3] = dgdProcessMachine.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);
            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdProcessName.Name))
                {
                    DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdProcessName);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdProcessName);

                    Name = dgdProcessName.Name;

                    if(Lib.Instance.GenerateExcel(dt, Name))
                    {
                        Lib.Instance.GenerateExcel(dt, Name);
                        Lib.Instance.excel.Visible = true;
                        lib.ReleaseExcelObject(lib.excel);
                    }
                    else
                    {
                        return;
                    }
                }
                else if (ExpExc.choice.Equals(dgdProcessMachine.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdProcessMachine);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdProcessMachine);

                    Name = dgdProcessMachine.Name;

                    if(Lib.Instance.GenerateExcel(dt, Name))
                    {
                        Lib.Instance.GenerateExcel(dt, Name);
                        Lib.Instance.excel.Visible = true;
                        lib.ReleaseExcelObject(lib.excel);
                    }
                    else
                    {
                        return;
                    }
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


        #endregion

        #region Content 입력 → 코드 숫자 2자리만 입력 되도록 → 안씀

        // 코드는 숫자만 입력 가능하도록, + 백스페이스
        private void txtCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (
                (!Char.IsDigit((char)KeyInterop.VirtualKeyFromKey(e.Key))
                && e.Key != Key.NumPad0
                && e.Key != Key.NumPad1
                && e.Key != Key.NumPad2
                && e.Key != Key.NumPad3
                && e.Key != Key.NumPad4
                && e.Key != Key.NumPad5
                && e.Key != Key.NumPad6
                && e.Key != Key.NumPad7
                && e.Key != Key.NumPad8
                && e.Key != Key.NumPad9
                ) && e.Key != Key.Back
                ) //  || e.Key == Key.Space
            {
                e.Handled = true;
            }
        }

        #endregion

        #region 조회
        private void FillGrid()
        {
            if (dgdProcessName.Items.Count > 0)
            {
                dgdProcessName.Items.Clear();
            }

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("Process", chkProcessSrh.IsChecked == true && txtProcessSrh.Text.Trim().Equals("") == false ? txtProcessSrh.Text : "");

                ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_Code_sProcess", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinProcessName = new Win_prd_MachineCode_U_CodeView()
                            {
                                Num = i + 1,
                                ProcessID = dr["ProcessID"].ToString(),
                                Process = dr["Process"].ToString()
                            };

                            dgdProcessName.Items.Add(WinProcessName);

                            i++;
                        }
                    }
                    TextBlockCountMain.Text = " ▶ 검색 결과 : " + i + " 건";
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

        #region 공정별 설비 조회
        private void FillGridMachine(string strProcessID)
        {
            if (dgdProcessMachine.Items.Count > 0)
            {
                dgdProcessMachine.Items.Clear();
            }

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("sProcessID", strProcessID);

                ds = DataStore.Instance.ProcedureToDataSet("xp_Process_sMachine", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count > 0)
                    { 
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinProcessMachine = new Win_prd_MachineCode_ProcessMachine_CodeView()
                            {
                                Num = i + 1,
                                MachineID = dr["MachineID"].ToString(),
                                Machine = dr["Machine"].ToString(),
                                MachineNO = dr["MachineNO"].ToString(),
                                SetHitCount = dr["SetHitCount"].ToString(),
                                ProductLocID = dr["ProductLocID"].ToString(),
                                ProcessID = dr["ProcessID"].ToString(),
                                Process = dr["Process"].ToString(),
                                CommStationNo = dr["CommStationNo"].ToString(),
                                CommIP = dr["CommIP"].ToString(),
                                CommCollectionYN = dr["CommCollectionYN"].ToString()
                                
                                //TdGbn = dr["TdGbn"].ToString(),
                                //TdCycle = dr["TdCycle"].ToString(),
                                //TdDate = dr["TdDate"].ToString(),
                                //TdExchange = dr["TdExchange"].ToString(),
                                //TdTime = dr["TdTime"].ToString()
                            };

                            dgdProcessMachine.Items.Add(WinProcessMachine);
                            i++;
                        }
                    }
                    TextBlockCountSub.Text = " ▶ 검색 결과 : " + i + " 건";
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

        #region 재검색
        private void re_Search()
        {
            FillGrid();

            if (dgdProcessName.Items.Count > 0)
            {
                dgdProcessName.SelectedIndex = rowNum;
            }
            else
            {
                dgdProcessMachine.Items.Clear();
                this.DataContext = null;
            }
        }
        #endregion

        #region 공정 선택시
        private void dgdProcessName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessName = dgdProcessName.SelectedItem as Win_prd_MachineCode_U_CodeView;

            if (ProcessName != null)
            {
                FillGridMachine(ProcessName.ProcessID);

                if (dgdProcessMachine.Items.Count > 0)
                {
                    dgdProcessMachine.SelectedIndex = rowNumProcessMachine;
                }
                else if(dgdProcessMachine.Items.Count == 0)
                {
                    ClearData();
                }
            }
        }
        #endregion

        #region 설비 선택시
        private void dgdProcessMachine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgdProcessMachine.Items.Count > 0)
            {
                ProcessMachine = dgdProcessMachine.SelectedItem as Win_prd_MachineCode_ProcessMachine_CodeView;
            }
            else
            {
                ProcessMachine = null;
            }

            if (ProcessMachine != null)
            {
                this.DataContext = ProcessMachine;
            }
        }
        #endregion

        #region 삭제
        private bool DeleteData(string strProcessID, string strMachineID)
        {
            bool flag = false;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("sProcessID", strProcessID);
                sqlParameter.Add("sMachineID", strMachineID);

                string[] result = DataStore.Instance.ExecuteProcedure_NewLog("xp_Process_dMachine", sqlParameter, "D");
                DataStore.Instance.CloseConnection();

                if (result[0].Equals("success"))
                {
                    //MessageBox.Show("성공 *^^*");
                    flag = true;
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

        #region 저장
        private bool SaveData(string strFlag, string strProcessID)
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
                    sqlParameter.Add("sProcessID", strProcessID);
                    sqlParameter.Add("sMachineID", txtCode.Text);
                    sqlParameter.Add("sMachine", txtMachineWorkStationName.Text);
                    sqlParameter.Add("sMachineNO", txtMachineWorkStationNumName.Text);
                    sqlParameter.Add("SetHitCount", txtSetHitCount.Text.Length > 0 ? Convert.ToInt64(txtSetHitCount.Text) : 0);
                    sqlParameter.Add("sProdLocID", cboLoc.SelectedValue == null ? "" : cboLoc.SelectedValue.ToString());

                    sqlParameter.Add("CommStationNo", TextBoxCommStationNo.Text);
                    sqlParameter.Add("CommIP", TextBoxCommIP.Text);
                    sqlParameter.Add("CommCollectionYN", ComboBoxCommCollectionYN.Text);
                    

                    #region 추가

                    if (strFlag.Equals("I"))
                    {
                        sqlParameter.Add("sCreateuserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_Process_iMachine";
                        pro1.OutputUseYN = "N";
                        pro1.OutputName = "sMachineID";
                        pro1.OutputLength = "2";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        string[] Confirm = new string[2];
                        Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew_NewLog(Prolist, ListParameter,"C");
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
                    }

                    #endregion

                    #region 수정

                    else if (strFlag.Equals("U"))
                    {
                        sqlParameter.Add("sUpdateUserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_Process_uMachine";
                        pro1.OutputUseYN = "N";
                        pro1.OutputName = "sMachineID";
                        pro1.OutputLength = "2";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        string[] Confirm = new string[2];
                        Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew_NewLog(Prolist, ListParameter,"R");
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
                    }

                    #endregion
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

        #region 데이터 체크
        private bool CheckData()
        {
            bool flag = true;

            if (txtMachineWorkStationName.Text.Length <= 0 || txtMachineWorkStationName.Text.Equals(""))
            {
                MessageBox.Show("기계명/작업장이 입력되지 않았습니다.");
                flag = false;
                return flag;
            }

            // 공백을 제외한 숫자 2자리를 입력해주세요.
            if (txtCode.Text.Trim().Length < 2)
            {
                MessageBox.Show("공백을 제외한 숫자 2자리를 입력해주세요.");
                flag = false;
                return flag;
            }

            // 코드는 숫자만 입력
            if (CheckConvertInt(txtCode.Text) == false)
            {
                MessageBox.Show("코드는 숫자만 입력 가능합니다.");
                flag = false;
                return flag;
            }

            // 코드 중복
            var Process = dgdProcessName.SelectedItem as Win_prd_MachineCode_U_CodeView;
            if (Process != null)
            {
                if (strFlag.Trim().Equals("I")
                    && ChkMachineID(Process.ProcessID, txtCode.Text.Trim()) == false)
                {
                    MessageBox.Show("해당 코드는 이미 존재합니다.");
                    flag = false;
                    return flag;
                }
            }

            // 실적 창고도 필수 입력!
            if (cboLoc.SelectedValue == null)
            {
                MessageBox.Show("실적 창고를 선택해주세요.");
                flag = false;
                return flag;
            }

            // 설정 타점수는 숫자만 입력 가능하도록
            if (CheckConvertInt(txtSetHitCount.Text) == false)
            {
                MessageBox.Show("설정 타점수는 숫자만 입력 가능합니다.");
                flag = false;
                return flag;
            }

            return flag;
        }

        #region 호기 코드 중복 검사 
        private bool ChkMachineID(string ProcessID, string MachineID)
        {
            bool flag = true;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("ProcessID", ProcessID);
                sqlParameter.Add("MachineID", MachineID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Machine_sChkMachineID", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count != 0)
                    {
                        DataRowCollection drc = dt.Rows;
                        DataRow dr = drc[0];
                        int Cnt = ConvertInt(dr["Cnt"].ToString());

                        if (Cnt > 0)
                        {
                            return false;
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

            return flag;
        }

        #endregion

        #endregion

        #region 텍스트 박스 공통 키다운 이벤트

        // 검색조건 - 텍스트 박스 엔터 → 조회
        private void txtBox_EnterAndSearch(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                rowNum = 0;
                using (Loading lw = new Loading(re_Search))
                {
                    lw.ShowDialog();
                }
            }
        }


        // 텍스트박스 숫자만 입력 가능하도록
        private void txtBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            CheckIsNumericOnly((TextBox)sender, e);
        }

        #endregion

        #region 기타 메서드 모음

        // 텍스트 박스 숫자만 입력 되도록
        public void CheckIsNumericOnly(TextBox sender, TextCompositionEventArgs e)
        {
            decimal result;
            if (!(Decimal.TryParse(e.Text, out result)))
            {
                e.Handled = true;
            }
        }

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

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            string processID = txtProcessSrh.Text.Trim();

            CopyData(processID);
        }

        #region 복사 저장

        private bool CopyData(string strProcessID)
        {
            bool flag = false;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                for (int i = 1; i < dgdProcessMachine.Items.Count; i++)
                {
                    var Machine = dgdProcessMachine.Items[i] as Win_prd_MachineCode_ProcessMachine_CodeView;

                    if (Machine != null)
                    {
                        Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                        sqlParameter.Clear();

                        sqlParameter.Add("sProcessID", strProcessID);
                        sqlParameter.Add("sMachineID", Machine.MachineID);
                        sqlParameter.Add("sMachine", Machine.Machine);
                        sqlParameter.Add("sMachineNO", Machine.MachineNO);
                        sqlParameter.Add("SetHitCount", ConvertInt(Machine.SetHitCount));
                        sqlParameter.Add("sProdLocID", "A0002");

                        sqlParameter.Add("CommStationNo", Machine.CommStationNo);
                        sqlParameter.Add("CommIP", Machine.CommIP);
                        sqlParameter.Add("CommCollectionYN", Machine.CommCollectionYN);
                        sqlParameter.Add("sCreateuserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_Process_iMachine";
                        pro1.OutputUseYN = "N";
                        pro1.OutputName = "sMachineID";
                        pro1.OutputLength = "2";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);
                    }
                }

                List<KeyValue> list_Result = new List<KeyValue>();
                list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS(Prolist, ListParameter);
                string sGetMachineID = string.Empty;

                if (list_Result[0].key.ToLower() == "success")
                {
                    list_Result.RemoveAt(0);
                    for (int i = 0; i < list_Result.Count; i++)
                    {
                        KeyValue kv = list_Result[i];
                        if (kv.key == "sMachineID")
                        {
                            sGetMachineID = kv.value;
                            flag = true;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("[저장실패]\r\n" + list_Result[0].value.ToString());
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

        #region 입력창 초기화
        private void ClearData()
        {
            this.DataContext = null;
        }

        #endregion

        #region 입력창 이동 이벤트
        //텍스트박스
        private void EnterMoveTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    lib.SendK(Key.Tab, this);

                    
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
        }

        //콤보박스일때
        private void EnterMoveComboBox_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                lib.SendK(Key.Tab, this);
            }
            catch (Exception ee)
            {
                MessageBox.Show("예외처리 - " + ee.ToString());
            }
        }

        #endregion

        private void txtSetHitCount_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                cboLoc.IsDropDownOpen = true;
                cboLoc.Focus();
            }
        }

        private void TextBoxCommIP_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                ComboBoxCommCollectionYN.Focus();
                ComboBoxCommCollectionYN.IsDropDownOpen = true;
                
            }
        }
    }

    #region CodeView
    class Win_prd_MachineCode_U_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }
        public string ProcessID { get; set; }
        public string Process { get; set; }
        public string DetailProcessYN { get; set; }
    }

    class Win_prd_MachineCode_ProcessMachine_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }
        public string MachineID { get; set; }
        public string Machine { get; set; }
        public string MachineNO { get; set; }
        public string SetHitCount { get; set; }
        public string ProductLocID { get; set; }
        public string ProcessID { get; set; }
        public string Process { get; set; }
        public string CommStationNo { get; set; }
        public string CommIP { get; set; }
        public string CommCollectionYN { get; set; }

        //public string TdGbn { get; set; }
        //public string TdCycle { get; set; }
        //public string TdDate { get; set; }
        //public string TdTime { get; set; }
        //public string TdExchange { get; set; }
    }

    #endregion
}
