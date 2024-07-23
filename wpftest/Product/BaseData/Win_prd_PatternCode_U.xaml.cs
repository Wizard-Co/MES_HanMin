/**
 * 
 * @details     공정패턴 코드 등록
 * @author      정승학
 * @date        2019-07-29
 * @version     1.0
 * 
 * @section MODIFYINFO 수정정보
 * - 수정일        - 수정자       : 수정내역
 * - 2019-00-00    - 정승학       : -----
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
    /// Win_prd_PatternCode_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_prd_PatternCode_U : UserControl
    {
        Lib lib = new Lib();
        PlusFinder pf = MainWindow.pf;

        string strFlag = string.Empty;
        int rowNum = 0;
        Win_prd_PatternCode_U_CodeView winPattern = new Win_prd_PatternCode_U_CodeView();
        

        public Win_prd_PatternCode_U()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Lib.Instance.UiLoading(sender);
            SetComboBox();
        }

        #region 콤보박스
        private void SetComboBox()
        {
            ObservableCollection<CodeView> ovcArticleGrp = ComboBoxUtil.Instance.GetArticleCode_SetComboBox("", 0);
            this.cboArticleGrp.ItemsSource = ovcArticleGrp;
            this.cboArticleGrp.DisplayMemberPath = "code_name";
            this.cboArticleGrp.SelectedValuePath = "code_id";
        }
        #endregion

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
            grdInput.IsEnabled = false;
            //lblMsg.Visibility = Visibility.Hidden;
            Lib.Instance.UiButtonEnableChange_IUControl(this);
        }

        //수정, 추가 진행 중
        private void CantBtnControl()
        {
            //btnAdd.IsEnabled = false;
            //btnUpdate.IsEnabled = false;
            //btnDelete.IsEnabled = false;
            //btnSearch.IsEnabled = false;
            //btnSave.Visibility = Visibility.Visible;
            //btnCancel.Visibility = Visibility.Visible;
            //btnExcel.Visibility = Visibility.Hidden;
            grdInput.IsEnabled = true;
            //lblMsg.Visibility = Visibility.Visible;
            Lib.Instance.UiButtonEnableChange_SCControl(this);
        }

        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CantBtnControl();
            strFlag = "I";

            lblMsg.Visibility = Visibility.Visible;
            tbkMsg.Text = "자료 입력 중";
            rowNum = dgdPattern.SelectedIndex;
            this.DataContext = null;

            if (dgdPatternProcess.Items.Count > 0)
            {
                dgdPatternProcess.Items.Clear();
            }

            //if (dgdAllProcess.Items.Count == 0)
            //{
                FillGridlAllProcess();
            //}

            cboArticleGrp.IsDropDownOpen = true;
            cboArticleGrp.Focus();

        }

        //수정
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            winPattern = dgdPattern.SelectedItem as Win_prd_PatternCode_U_CodeView;

            if (winPattern != null)
            {
                rowNum = dgdPattern.SelectedIndex;
                //dgdPattern.IsEnabled = false;
                dgdPattern.IsHitTestVisible = false;
                tbkMsg.Text = "자료 수정 중";
                lblMsg.Visibility = Visibility.Visible;
                CantBtnControl();
                strFlag = "U";

                //if (dgdAllProcess.Items.Count == 0)
                //{
                    FillGridlAllProcess();
                //}
            }
        }

        //삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            winPattern = dgdPattern.SelectedItem as Win_prd_PatternCode_U_CodeView;

            if (winPattern == null)
            {
                MessageBox.Show("삭제할 데이터가 지정되지 않았습니다. 삭제데이터를 지정하고 눌러주세요");
                return;
            }
            else
            {
                if(PlanCheck_PatternCode(winPattern.PatternID) == true)
                {
                    MessageBox.Show("해당 패턴코드가 포함된 작업지시서가 존재합니다.");
                    return;
                }
                
            }

            if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (dgdPattern.Items.Count > 0 && dgdPattern.SelectedItem != null)
                {
                    rowNum = dgdPattern.SelectedIndex;
                }

                if (DeleteData(winPattern.PatternID))
                {
                    rowNum -= 1;
                    re_Search();
                }
            }
        }

        //닫기
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

        //검색
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSearch.IsEnabled = false;

                rowNum = 0;
                using (Loading lw = new Loading(beSearch))
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

        private void beSearch()
        {
            re_Search();
            grdInput.IsEnabled = false;

            FillGridlAllProcess();
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
            if (SaveData(strFlag, txtCode.Text))
            {
                CanBtnControl();
                lblMsg.Visibility = Visibility.Hidden;
                //rowNum = 0;
                //dgdPattern.IsEnabled = true;
                dgdPattern.IsHitTestVisible = true;
                re_Search();
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
            //dgdPattern.IsEnabled = true;
            dgdPattern.IsHitTestVisible = true;
            re_Search();
        }

        #region 엑셀 버튼
        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[6];
            lst[0] = "패턴";
            lst[1] = "전체 공정";
            lst[2] = "선택된 공정(패턴 공정)";
            lst[3] = dgdPattern.Name;
            lst[4] = dgdAllProcess.Name;
            lst[5] = dgdPatternProcess.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);
            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdPattern.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdPattern);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdPattern);

                    Name = dgdPattern.Name;
                    Lib.Instance.GenerateExcel(dt, Name);
                    Lib.Instance.excel.Visible = true;
                    lib.ReleaseExcelObject(lib.excel);
                }
                else if (ExpExc.choice.Equals(dgdAllProcess.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdAllProcess);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdAllProcess);

                    Name = dgdAllProcess.Name;
                    Lib.Instance.GenerateExcel(dt, Name);
                    Lib.Instance.excel.Visible = true;
                    lib.ReleaseExcelObject(lib.excel);
                }
                else if (ExpExc.choice.Equals(dgdPatternProcess.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdPatternProcess);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdPatternProcess);

                    Name = dgdPatternProcess.Name;
                    Lib.Instance.GenerateExcel(dt, Name);
                    Lib.Instance.excel.Visible = true;
                    lib.ReleaseExcelObject(lib.excel);
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
        #endregion // 엑셀 버튼

        #endregion

        #region Content - 위아래 버튼

        #region 패턴 공정 순서 위아래 변경 메서드

        private void btnStepUpDown_Click(object sender, RoutedEventArgs e)
        {
            Button senderBtn = sender as Button;

            Win_prd_PatternCode_U_Process_CodeView AppStepE = new Win_prd_PatternCode_U_Process_CodeView(); // 임시 객체

            // 아래 버튼 클릭시
            if (senderBtn.Tag.ToString().Equals("Down"))
            {
                var StepFrom = dgdPatternProcess.SelectedItem as Win_prd_PatternCode_U_Process_CodeView;

                if (StepFrom != null)
                {
                    int currRow = dgdPatternProcess.SelectedIndex;

                    int goalRow = currRow + 1;
                    int maxRow = dgdPatternProcess.Items.Count - 1;

                    if (goalRow <= maxRow)
                    {
                        var StepTo = dgdPatternProcess.Items[goalRow] as Win_prd_PatternCode_U_Process_CodeView;

                        if (StepTo != null)
                        {
                            dgdPatternProcess.Items.RemoveAt(currRow); // 선택한 행 지우고
                            dgdPatternProcess.Items.RemoveAt(currRow); // 바로 밑의 행 지우고

                            StepTo.Num = currRow + 1;
                            dgdPatternProcess.Items.Insert(currRow, StepTo);

                            StepFrom.Num = goalRow + 1;
                            dgdPatternProcess.Items.Insert(goalRow, StepFrom);

                            dgdPatternProcess.SelectedIndex = goalRow;
                        }
                    }
                }
            }
            else // 위 버튼 클릭시
            {
                var StepFrom = dgdPatternProcess.SelectedItem as Win_prd_PatternCode_U_Process_CodeView;

                if (StepFrom != null)
                {
                    int currRow = dgdPatternProcess.SelectedIndex;

                    int goalRow = currRow - 1;

                    if (goalRow >= 0)
                    {
                        var StepTo = dgdPatternProcess.Items[goalRow] as Win_prd_PatternCode_U_Process_CodeView;

                        if (StepTo != null)
                        {
                            dgdPatternProcess.Items.RemoveAt(goalRow); // 선택한 행 지우고
                            dgdPatternProcess.Items.RemoveAt(goalRow); // 바로 밑의 행 지우고

                            StepTo.Num = currRow + 1;
                            dgdPatternProcess.Items.Insert(goalRow, StepTo);

                            StepFrom.Num = goalRow + 1;
                            dgdPatternProcess.Items.Insert(goalRow, StepFrom);

                            dgdPatternProcess.SelectedIndex = goalRow;
                        }
                    }
                }
            }
        }

        #endregion // 패턴 공정 순서 위아래 변경 메서드

        #endregion 

        #region 조회
        private void FillGrid()
        {
            if (dgdPattern.Items.Count > 0)
            {
                dgdPattern.Items.Clear();
            }

            try
            {
                DataSet ds = null;

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("nChkWorkID", 0);
                sqlParameter.Add("sWorkID", "");

                ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sPattern", sqlParameter, false);

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
                            var WinPattern = new Win_prd_PatternCode_U_CodeView()
                            {
                                Num = i + 1,
                                Pattern = dr["Pattern"].ToString(),
                                PatternID = dr["PatternID"].ToString(),
                                ArticleGrpID = dr["ArticleGrpID"].ToString(),
                                WorkID = dr["WorkID"].ToString(),
                                WorkName = dr["WorkName"].ToString(),
                                ArticleGrp = dr["ArticleGrp"].ToString(),
                            };

                            dgdPattern.Items.Add(WinPattern);

                            i++;
                        }
                    }

                    tbkCount.Text = " ▶ 검색 결과 : " + i + " 건";
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

            if (dgdPattern.Items.Count > 0)
            {
                dgdPattern.SelectedIndex = rowNum;
            }
        }

        #endregion

        #region 삭제
        private bool DeleteData(string strPatternId)
        {
            bool flag = false;

            Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
            sqlParameter.Clear();
            sqlParameter.Add("sPatternID", strPatternId);

            string[] result = DataStore.Instance.ExecuteProcedure("xp_Pattern_dPattern", sqlParameter, false);
            DataStore.Instance.CloseConnection();

            if (result[0].Equals("success"))
            {
                //MessageBox.Show("성공 *^^*");
                flag = true;
            }

            return flag;
        }

        #endregion

        #region 저장
        private bool SaveData(string strFlag, string strPatternID)
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
                    sqlParameter.Add("sPatternID", strPatternID);
                    sqlParameter.Add("sPattern", txtName.Text);
                    //0001 => 고무가공 18.10.02 현재 유일 workID
                    sqlParameter.Add("sWorkID", "0001");
                    sqlParameter.Add("sArticleGrpID", cboArticleGrp.SelectedValue.ToString());

                    #region 추가

                    if (strFlag.Equals("I"))
                    {
                        sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_Pattern_iPattern";
                        pro1.OutputUseYN = "Y";
                        pro1.OutputName = "sPatternID";
                        pro1.OutputLength = "2";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        for (int i = 0; i < dgdPatternProcess.Items.Count; i++)
                        {
                            DataGridRow dgr = Lib.Instance.GetRow(i, dgdPatternProcess);
                            var winPatternProcess = dgr.Item as Win_prd_PatternCode_U_Process_CodeView;

                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("sPatternID", strPatternID);
                            sqlParameter.Add("nPatternSeq", i + 1);
                            sqlParameter.Add("sProcessID", winPatternProcess.ProcessID);
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro2 = new Procedure();
                            pro2.Name = "xp_Pattern_iPatternSub";
                            pro2.OutputUseYN = "N";
                            pro2.OutputName = "sPatternID";
                            pro2.OutputLength = "2";

                            Prolist.Add(pro2);
                            ListParameter.Add(sqlParameter);
                        }

                        List<KeyValue> list_Result = new List<KeyValue>();
                        list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS(Prolist, ListParameter);
                        string sGetPatternID = string.Empty;

                        if (list_Result[0].key.ToLower() == "success")
                        {
                            list_Result.RemoveAt(0);
                            for (int i = 0; i < list_Result.Count; i++)
                            {
                                KeyValue kv = list_Result[i];
                                if (kv.key == "sPatternID")
                                {
                                    sGetPatternID = kv.value;
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

                    #endregion

                    #region 수정

                    else if (strFlag.Equals("U"))
                    {
                        sqlParameter.Add("LastUpdateUserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_Pattern_uPattern";
                        pro1.OutputUseYN = "N";
                        pro1.OutputName = "sPatternID";
                        pro1.OutputLength = "2";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        for (int i = 0; i < dgdPatternProcess.Items.Count; i++)
                        {
                            DataGridRow dgr = Lib.Instance.GetRow(i, dgdPatternProcess);
                            var winPatternProcess = dgr.Item as Win_prd_PatternCode_U_Process_CodeView;

                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("sPatternID", strPatternID);
                            sqlParameter.Add("nPatternSeq", i + 1);
                            sqlParameter.Add("sProcessID", winPatternProcess.ProcessID);
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro2 = new Procedure();
                            pro2.Name = "xp_Pattern_iPatternSub";
                            pro2.OutputUseYN = "N";
                            pro2.OutputName = "sPatternID";
                            pro2.OutputLength = "2";

                            Prolist.Add(pro2);
                            ListParameter.Add(sqlParameter);
                        }

                        string[] Confirm = new string[2];
                        Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew(Prolist, ListParameter);
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
                MessageBox.Show(ex.ToString());
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

            if (cboArticleGrp.SelectedIndex == -1 || cboArticleGrp.SelectedValue == null)
            {
                MessageBox.Show("제품그룹이 선택되지 않았습니다.");
                flag = false;
                return flag;
            }

            if (txtName.Text.Length <= 0 || txtName.Text.Equals(""))
            {
                MessageBox.Show("패턴 설명이 입력되지 않았습니다.");
                flag = false;
                return flag;
            }


            if (dgdPatternProcess.Items.Count <= 0)
            {
                MessageBox.Show("선택된 공정이 없습니다.");
            }

            if (txtName.Text.Length > 20)
            {
                MessageBox.Show("패턴명은 최대 20글자 까지 가능합니다.");
                flag = false;
                return flag;
            }

            return flag;
        }
        #endregion

        #region 공정 좌우 이동
        //오른쪽 버튼 - 전체->패턴
        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            var winPatternAllProcess = dgdAllProcess.SelectedItem as Win_prd_PatternCode_U_Process_CodeView;
            bool flag = true;

            if (winPatternAllProcess != null)
            {
                for (int i = 0; i < dgdPatternProcess.Items.Count; i++)
                {
                    var WinPP = dgdPatternProcess.Items[i] as Win_prd_PatternCode_U_Process_CodeView;

                    if (WinPP.Process == winPatternAllProcess.Process)
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    if (dgdPatternProcess.Items.Count > 0)
                    {
                        winPatternAllProcess.Num = dgdPatternProcess.Items.Count + 1;
                    }

                    dgdPatternProcess.Items.Add(winPatternAllProcess);
                }
                else
                {
                    MessageBox.Show("같은 이름의 공정이 추가되어있습니다.");
                }
            }
            else
            {
                MessageBox.Show("패턴에 추가할 공정이 선택되지 않았습니다.");
            }
        }

        //왼쪽버튼 패턴->전체
        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            var winPatternProcess = dgdPatternProcess.SelectedItem as Win_prd_PatternCode_U_Process_CodeView;

            if (winPatternProcess != null)
            {
                dgdPatternProcess.Items.Remove(winPatternProcess);
            }
            else
            {
                MessageBox.Show("패턴에서 제외할 공정이 선택되지 않았습니다.");
            }
        }

        #endregion

        #region dgdPattern_SelectionChanged 
        private void dgdPattern_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            winPattern = dgdPattern.SelectedItem as Win_prd_PatternCode_U_CodeView;

            if (winPattern != null)
            {
                FillGrid_OrderAndProcess(winPattern.PatternID);
                this.DataContext = winPattern;
            }
        }
        #endregion

        #region 모든 공정 보기
        private void FillGridlAllProcess()
        {
            if (dgdAllProcess.Items.Count > 0)
            {
                dgdAllProcess.Items.Clear();
            }

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("ProcessID", "");
                ds = DataStore.Instance.ProcedureToDataSet("xp_Code_sProcess", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinPatternAllProcess = new Win_prd_PatternCode_U_Process_CodeView()
                            {
                                Num = i + 1,
                                Process = dr["Process"].ToString(),
                                ProcessID = dr["ProcessID"].ToString()
                            };

                            dgdAllProcess.Items.Add(WinPatternAllProcess);
                            i++;
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

        #region PatternID로 패턴의 공정과 공정순서 가져오기
        private void FillGrid_OrderAndProcess(string strPatternID)
        {
            if (dgdPatternProcess.Items.Count > 0)
            {
                dgdPatternProcess.Items.Clear();
            }

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("sPatternID", strPatternID);

                ds = DataStore.Instance.ProcedureToDataSet("xp_Pattern_sPatternSub", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;
                    string strProcessPattern = string.Empty;

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinPatternProcess = new Win_prd_PatternCode_U_Process_CodeView()
                            {
                                Num = i + 1,
                                Process = dr["Process"].ToString(),
                                ProcessID = dr["ProcessID"].ToString(),
                                PatternSeq = dr["PatternSeq"].ToString()
                            };

                            dgdPatternProcess.Items.Add(WinPatternProcess);
                            i++;

                            if (i == drc.Count)
                            {
                                strProcessPattern += WinPatternProcess.Process;
                            }
                            else
                            {
                                strProcessPattern += WinPatternProcess.Process + "→";
                            }
                        }

                        if (dgdProcessOrder.Items.Count > 0)
                        {
                            dgdProcessOrder.Items.Clear();
                        }

                        var WinProcessOrder = new Win_prd_PatternCode_U_Order_CodeView() { ProcessOrder = strProcessPattern };
                        dgdProcessOrder.Items.Add(WinProcessOrder);
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

        // 더블 클릭시 오른쪽 그리드로 넘기기
        private void dgdAllProcess_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                TextBlock tblSender = sender as TextBlock;

                var Process = tblSender.DataContext as Win_prd_PatternCode_U_Process_CodeView;

                if (Process != null)
                {
                    for (int i = 0; i < dgdPatternProcess.Items.Count; i++)
                    {
                        var Compare = dgdPatternProcess.Items[i] as Win_prd_PatternCode_U_Process_CodeView;

                        if (Compare != null)
                        {
                            if (Compare.ProcessID.Trim().Equals(Process.ProcessID))
                            {
                                return;
                            }
                        }
                    }

                    var newP = new Win_prd_PatternCode_U_Process_CodeView()
                    {
                        Num = dgdPatternProcess.Items.Count + 1,
                        ProcessID = Process.ProcessID,
                        Process = Process.Process
                    };

                    //Process.Num = dgdPatternProcess.Items.Count + 1;
                    dgdPatternProcess.Items.Add(newP);
                    //(dgdPatternProcess.Items[dgdPatternProcess.Items.Count - 1] as Win_prd_PatternCode_U_Process_CodeView).Num = dgdPatternProcess.Items.Count;
                }
            }
        }
        // 오른쪽 그리드 더블클릭 → 빼기
        private void dgdPatternProcess_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                TextBlock tblSender = sender as TextBlock;

                var Process = tblSender.DataContext as Win_prd_PatternCode_U_Process_CodeView;

                if (Process != null)
                {
                    dgdPatternProcess.Items.Remove(Process);
                    SettingNum();
                }
            }
        }
        // 번호 세팅하기
        private void SettingNum()
        {
            for (int i = 0; i < dgdPatternProcess.Items.Count; i++)
            {
                var Process = dgdPatternProcess.Items[i] as Win_prd_PatternCode_U_Process_CodeView;
                if (Process != null)
                {
                    Process.Num = i + 1;
                }
            }
        }

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

        private bool PlanCheck_PatternCode(string patternID)
        {
            bool result = false;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("PatternID", patternID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_PatternCode_PlanCheck", sqlParameter, false);

                if(ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if(dt.Rows.Count > 0)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
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

            return result;
        }

    }


    #region CodeView
    class Win_prd_PatternCode_U_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }
        public string PatternID { get; set; }
        public string Pattern { get; set; }
        public string WorkID { get; set; }
        public string WorkName { get; set; }
        public string ArticleGrpID { get; set; }
        public string ArticleGrp { get; set; }
    }

    class Win_prd_PatternCode_U_Process_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }
        public string ProcessID { get; set; }
        public string Process { get; set; }
        public string DetailProcessYN { get; set; }
        public string PatternSeq { get; set; }
    }

    class Win_prd_PatternCode_U_Order_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public string ProcessOrder { get; set; }
    }

    #endregion
}
