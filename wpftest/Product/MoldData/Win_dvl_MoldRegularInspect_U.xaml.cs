using WizMes_HanMin.PopUP;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF.MDI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Net;
using System.Drawing;

namespace WizMes_HanMin
{
    /// <summary>
    /// Win_dvl_MoldRegularInspect_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_dvl_MoldRegularInspect_U : UserControl
    {
        #region 변수선언 및 로드
        //string FTP_ADDRESS = "ftp://192.168.0.28/MoldBasis";
        string FTP_ADDRESS = "ftp://" + LoadINI.FileSvr + ":"
            + LoadINI.FTPPort + LoadINI.FtpImagePath + "/MoldBasis";

        //FTP 활용모음
        string strImagePath = string.Empty;
        string strFullPath = string.Empty;
        string strDelFileName = string.Empty;

        private const string FTP_ID = "wizuser";
        private const string FTP_PASS = "wiz9999";
        private const string LOCAL_DOWN_PATH = "C:\\Temp";

        private FTP_EX _ftp = null;
        List<string[]> listFtpFile = new List<string[]>();
        List<string[]> deleteListFtpFile = new List<string[]>();

        bool FTP_Trigger = false;

        // 복사 추가용 변수
        // 이미지 이름 : 폴더이름
        Dictionary<string, string> lstFtpFilePath = new Dictionary<string, string>();

        Lib lib = new Lib();
        PlusFinder pf = new PlusFinder();
        string strFlag = string.Empty;
        int numRowCount = 0;
        string strBasisID = string.Empty;
        Win_dvl_MoldRegularInspect_U_Sub_CodeView WinRInsSub1 = new Win_dvl_MoldRegularInspect_U_Sub_CodeView();
        Win_dvl_MoldRegularInspect_U_Sub_CodeView WinRInsSub2 = new Win_dvl_MoldRegularInspect_U_Sub_CodeView();

        Dictionary<string, object> dicCompare = new Dictionary<string, object>();
        List<string> lstCompareValue = new List<string>();

        public Win_dvl_MoldRegularInspect_U()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
            SetComboBox();

            lib.UiLoading(sender);
        }

        #endregion

        #region 콤보박스세팅
        private void SetComboBox()
        {
            ObservableCollection<CodeView> ovcRegularGbn = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ReGualGbn", "Y", "");
            this.cboRegularGbn.ItemsSource = ovcRegularGbn;
            this.cboRegularGbn.DisplayMemberPath = "code_name";
            this.cboRegularGbn.SelectedValuePath = "code_id";

            this.cboRegularGbnSrh.ItemsSource = ovcRegularGbn;
            this.cboRegularGbnSrh.DisplayMemberPath = "code_name";
            this.cboRegularGbnSrh.SelectedValuePath = "code_id";
        }

        #endregion 콤보박스세팅

        #region 상단 중간 이벤트

        //점검기간 라벨
        private void lblInspectDaySrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkInspectDaySrh.IsChecked == true) { chkInspectDaySrh.IsChecked = false; }
            else { chkInspectDaySrh.IsChecked = true; }
        }

        //점검기간 체크박스
        private void chkInspectDaySrh_Checked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = true;
            dtpEDate.IsEnabled = true;
        }

        //점검기간 체크박스
        private void chkInspectDaySrh_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
        }

        //금월
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = lib.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = lib.BringThisMonthDatetimeList()[1];
        }

        //금일
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }

        //금형 라벨
        private void lblMoldSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkMoldSrh.IsChecked == true) { chkMoldSrh.IsChecked = false; }
            else { chkMoldSrh.IsChecked = true; }
        }

        //금형 체크박스
        private void chkMoldSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtMoldSrh.IsEnabled = true;
            btnPfMoldSrh.IsEnabled = true;
            txtMoldSrh.Focus();
        }

        //금형 체크박스
        private void chkMoldSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtMoldSrh.IsEnabled = false;
            btnPfMoldSrh.IsEnabled = false;
        }

        //금형 플러스파인더 이벤트(텍스트박스)
        private void txtMoldSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                pf.ReturnCode(txtMoldSrh, 51, "");
            }
        }

        //금형 플러스파인더 이벤트(버튼)
        private void btnPfMoldSrh_Click(object sender, RoutedEventArgs e)
        {
            pf.ReturnCode(txtMoldSrh, 51, "");
        }
        #endregion

        #region 상단 우측 버튼 이벤트

        //추가,수정 시 동작 모음
        private void ControlVisibleAndEnable_AU()
        {
            lib.UiButtonEnableChange_SCControl(this);
            dgdMoldInspect.IsEnabled = false;
            grbMold.IsEnabled = true;
        }

        //저장,취소 시 동작 모음
        private void ControlVisibleAndEnable_SC()
        {
            lib.UiButtonEnableChange_IUControl(this);
            dgdMoldInspect.IsEnabled = true;
            grbMold.IsEnabled = false;
        }

        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMoldInspect.Items.Count > 0 && dgdMoldInspect.SelectedItem != null)
            {
                numRowCount = dgdMoldInspect.SelectedIndex; //취소 시 대비
            }

            ControlVisibleAndEnable_AU();            
            strFlag = "I";
            tbkMsg.Text = "자료 입력(추가) 중";

            this.DataContext = null;
            dtpMoldInspectDate.SelectedDate = DateTime.Today;

            if (dgdMold_InspectSub1.Items.Count > 0)
            {
                dgdMold_InspectSub1.Items.Clear();
            }
            if (dgdMold_InspectSub2.Items.Count > 0)
            {
                dgdMold_InspectSub2.Items.Clear();
            }

            cboRegularGbn.SelectedIndex = 0;
        }

        //수정
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMoldInspect.SelectedItem == null)
            {
                MessageBox.Show("수정할 자료를 선택하고 눌러주십시오.");
            }
            else
            {
                numRowCount = dgdMoldInspect.SelectedIndex;
                ControlVisibleAndEnable_AU();
                tbkMsg.Text = "자료 입력(수정) 중";
                strFlag = "U";
                txtMoldID.Focus();
            }
        }

        //삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var winMoldInspect = dgdMoldInspect.SelectedItem as Win_dvl_MoldRegularInspect_U_CodeView;

            if (winMoldInspect == null)
            {
                MessageBox.Show("삭제할 데이터가 지정되지 않았습니다. 삭제데이터를 지정하고 눌러주세요");
                return;
            }
            else
            {
                if (dgdMoldInspect.SelectedIndex == dgdMoldInspect.Items.Count - 1)
                {
                    numRowCount = dgdMoldInspect.SelectedIndex - 1;
                }
                else
                {
                    numRowCount = dgdMoldInspect.SelectedIndex;
                }

                if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (DeleteData(winMoldInspect.MoldRInspectID))
                    {
                        re_Search(numRowCount);
                    }
                }
            }
        }

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            string stDate = DateTime.Now.ToString("yyyyMMdd");
            string stTime = DateTime.Now.ToString("HHmm");
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        //조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            numRowCount = 0;
            re_Search(numRowCount);
        }

        //저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (strFlag.Equals("I"))
            {
                if (SaveData("", strFlag))
                {
                    ControlVisibleAndEnable_SC();
                    numRowCount = 0;
                    re_Search(numRowCount);
                }
            }
            else
            {
                if (SaveData(txtMoldRInspectID.Text, strFlag))
                {
                    ControlVisibleAndEnable_SC();
                    re_Search(numRowCount);
                }
            }
        }

        //취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            InputClear();
            ControlVisibleAndEnable_SC();
            re_Search(numRowCount);
        }

        //입력 데이터 클리어
        private void InputClear()
        {
            try
            {
                //foreach (Control child in this.grdInput.Children)
                //{
                //    if (child.GetType() == typeof(TextBox))
                //        ((TextBox)child).Clear();
                //}
                if (this.dgdMold_InspectSub1.Items.Count > 0)
                    this.dgdMold_InspectSub1.Items.Clear();
                if (this.dgdMold_InspectSub2.Items.Count <= 0)
                    return;
                this.dgdMold_InspectSub2.Items.Clear();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[6];
            lst[0] = "금형 일상점검 메인";
            lst[1] = "금형 일상점검_범례";
            lst[2] = "금형 일상점검_수치";
            lst[3] = dgdMoldInspect.Name;
            lst[4] = dgdMold_InspectSub1.Name;
            lst[5] = dgdMold_InspectSub2.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);

            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdMoldInspect.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = lib.DataGridToDTinHidden(dgdMoldInspect);
                    else
                        dt = lib.DataGirdToDataTable(dgdMoldInspect);

                    Name = dgdMoldInspect.Name;

                    if (lib.GenerateExcel(dt, Name))
                        lib.excel.Visible = true;
                    else
                        return;
                }
                else if (ExpExc.choice.Equals(dgdMold_InspectSub1.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = lib.DataGridToDTinHidden(dgdMold_InspectSub1);
                    else
                        dt = lib.DataGirdToDataTable(dgdMold_InspectSub1);

                    Name = dgdMold_InspectSub1.Name;

                    if (lib.GenerateExcel(dt, Name))
                        lib.excel.Visible = true;
                    else
                        return;
                }
                else if (ExpExc.choice.Equals(dgdMold_InspectSub2.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = lib.DataGridToDTinHidden(dgdMold_InspectSub2);
                    else
                        dt = lib.DataGirdToDataTable(dgdMold_InspectSub2);

                    Name = dgdMold_InspectSub2.Name;

                    if (lib.GenerateExcel(dt, Name))
                        lib.excel.Visible = true;
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

        //수정,추가,삭제 후 조회 등
        private void re_Search(int index)
        {
            if (dgdMoldInspect.Items.Count > 0)
            {
                dgdMoldInspect.Items.Clear();
            }

            FillGrid();

            if (dgdMoldInspect.Items.Count > 0)
            {
                if (lstCompareValue.Count > 0)
                {
                    dgdMoldInspect.SelectedIndex = lib.reTrunIndex(dgdMoldInspect, lstCompareValue[0]);
                }
                else
                {
                    dgdMoldInspect.SelectedIndex = index; ;
                }
            }
            else
            {
                InputClear();
            }

            dicCompare.Clear();
            lstCompareValue.Clear();
        }

        #endregion

        #region CRUD
        private void FillGrid() //일단 추가해주나 수정 꼭 해야함
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("nChkDate", chkInspectDaySrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SDate", chkInspectDaySrh.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("EDate", chkInspectDaySrh.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("nChkMold", chkMoldSrh.IsChecked == true ? (txtMoldSrh.Tag != null ? 1 : 2) : 0);
                sqlParameter.Add("MoldID", chkMoldSrh.IsChecked == true ? (txtMoldSrh.Tag != null ? txtMoldSrh.Tag.ToString() : txtMoldSrh.Text) : "");

                //sqlParameter.Add("ntotSearch", ChkntotSearch.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("ntotSearchGbn", ChkntotSearch.IsChecked == true ? (ntotSearchGbn.SelectedValue == null ? 0 : ntotSearchGbn.SelectedIndex + 1) : 0);
                //sqlParameter.Add("stotSearch", txttotSearch.Text.ToString());
                sqlParameter.Add("nChkArticleID", chkArticleSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("sArticleID", txtArticleSrh.Tag != null && !txtArticleSrh.Text.Trim().Equals("") ? txtArticleSrh.Tag.ToString() : "");

                sqlParameter.Add("nChkRegularGbn", chkRegularGbnSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("sChkRegularGbn", chkRegularGbnSrh.IsChecked == true ? cboRegularGbnSrh.SelectedValue.ToString() : "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMoldIns_sRegularInspect_FAC", sqlParameter, false);

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
                            var WinMoldInspect = new Win_dvl_MoldRegularInspect_U_CodeView()
                            {
                                Num = i.ToString(),
                                MoldRInspectID = dr["MoldRInspectID"].ToString(),
                                MoldInspectBasisID = dr["MoldInspectBasisID"].ToString(),
                                MoldRInspectUserID = dr["MoldRInspectUserID"].ToString(),
                                MoldID = dr["MoldID"].ToString(),
                                HitCount = dr["HitCount"].ToString(),
                                MoldRInspectDate = dr["MoldRInspectDate"].ToString(),
                                Comments = dr["Comments"].ToString(),
                                Person = dr["Person"].ToString(),
                                MoldNo = dr["MoldNo"].ToString(),
                                ChkRegularGbn = dr["ChkRegularGbn"].ToString(),
                            };
                            if (WinMoldInspect.MoldRInspectDate != null && !WinMoldInspect.MoldRInspectDate.Equals(""))
                            {
                                WinMoldInspect.MoldRInspectDate_CV = lib.StrDateTimeBar(WinMoldInspect.MoldRInspectDate);
                            }

                            if (dicCompare.Count > 0)
                            {
                                if (WinMoldInspect.MoldRInspectID.Equals(dicCompare["MoldRInspectID"].ToString()))
                                {
                                    lstCompareValue.Add(WinMoldInspect.ToString());
                                }
                            }

                            dgdMoldInspect.Items.Add(WinMoldInspect);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생,오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        private void FillGridSub(string strMoldID, string strMoldInspectID)
        {
            FTP_Trigger = true;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("MoldID", strMoldID);
                sqlParameter.Add("MoldRInspectID", strMoldInspectID);
             
                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMoldIns_sRegularInspectSub", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinMoldSub = new Win_dvl_MoldRegularInspect_U_Sub_CodeView()
                            {
                                MoldRInspectID = dr["MoldRInspectID"].ToString(),
                                MoldInspectBasisID = dr["MoldInspectBasisID"].ToString(),
                                MoldID = dr["MoldID"].ToString(),
                                InspectSubSeq = dr["MoldInspectSeq"].ToString(),
                                MoldInspectItemName = dr["MoldInspectItemName"].ToString(),
                                MoldInspectContent = dr["MoldInspectContent"].ToString(),
                                MoldInspectCheckGbn = dr["MoldInspectCheckGbn"].ToString(),
                                MoldInspectCheckName = dr["MoldInspectCheckName"].ToString(),
                                MoldInspectCycleGbn = dr["MoldInspectCycleGbn"].ToString(),
                                MoldInspectCycleName = dr["MoldInspectCycleName"].ToString(),
                                MoldInspectCycleDate = dr["MoldInspectCycleDate"].ToString(),
                                MldRInspectLegend = dr["MldRInspectLegend"].ToString(),
                                MoldInspectRecordGbn = dr["MoldInspectRecordGbn"].ToString(),
                                MoldInspectRecordName = dr["MoldInspectRecordName"].ToString(),
                                MoldInspectImageFile = dr["MoldInspectImageFile"].ToString(),
                                MldRValue = dr["MldRValue"].ToString(),
                                Comments = dr["Comments"].ToString()
                            };


                            if (!WinMoldSub.MoldInspectImageFile.Trim().Equals(""))
                            {
                                if (FTP_Trigger == true)
                                {
                                    WinMoldSub.ImageByte = SetImage(WinMoldSub.MoldInspectImageFile, WinMoldSub.MoldInspectBasisID);
                                }
                            }

                            if (WinMoldSub != null)
                            {
                                if (WinMoldSub.MoldInspectRecordGbn.Equals("01"))
                                {
                                    
                                    dgdMold_InspectSub1.Items.Add(WinMoldSub);
                                }
                                else if (WinMoldSub.MoldInspectRecordGbn.Equals("02"))
                                {

                                    dgdMold_InspectSub2.Items.Add(WinMoldSub);
                                }
                            }

                            



                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생,오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        private void dgdMoldInspect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgdMold_InspectSub1.Items.Count > 0)
            {
                dgdMold_InspectSub1.Items.Clear();
            }

            if (dgdMold_InspectSub2.Items.Count > 0)
            {
                dgdMold_InspectSub2.Items.Clear();
            }

            var WinMold = dgdMoldInspect.SelectedItem as Win_dvl_MoldRegularInspect_U_CodeView;

            if (WinMold != null)
            {
                this.DataContext = WinMold;
                FillGridSub(WinMold.MoldID, WinMold.MoldRInspectID);
            }
        }

        //삭제
        private bool DeleteData(string strMoldInspectID)
        {
            bool flag = true;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("MoldInspectID", strMoldInspectID);
                
                string[] result = DataStore.Instance.ExecuteProcedure_NewLog("xp_dvlMoldIns_dRegularInspect", sqlParameter, "D");
                //string[] result = DataStore.Instance.ExecuteProcedure("xp_dvlMoldIns_dRegularInspect", sqlParameter, true);

                if (!result[0].Equals("success"))
                {
                    //MessageBox.Show("실패 ㅠㅠ");
                }
                else
                {
                    //MessageBox.Show("성공 *^^*");
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생,오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }

            return flag;
        }

        //추가, 수정
        private bool SaveData(string strMoldRInspectID, string strFlag)
        {
            bool flag = true;
            string resultAdd = string.Empty;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            if (CheckData())
            {
                try
                {
                    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Add("MoldRInspectID", strMoldRInspectID);
                    sqlParameter.Add("MoldInspectBasisID", txtMoldBasisID.Text);
                    sqlParameter.Add("MoldRInspectDate", dtpMoldInspectDate.SelectedDate.Value.ToString("yyyyMMdd"));
                    sqlParameter.Add("MoldRInspectUserID", txtPerson.Tag.ToString());

                    sqlParameter.Add("ChkRegularGbn", cboRegularGbn.SelectedValue != null ? cboRegularGbn.SelectedValue.ToString() : "");

                    sqlParameter.Add("Comments", txtComments.Text);
                    sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                    if (strFlag.Equals("I"))
                    {
                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_dvlMoldIns_iRegularInspect";
                        pro1.OutputUseYN = "Y";
                        pro1.OutputName = "MoldRInspectID";
                        pro1.OutputLength = "10";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        for (int i = 0; i < dgdMold_InspectSub1.Items.Count; i++)
                        {
                            DataGridRow dgr = lib.GetRow(i, dgdMold_InspectSub1);
                            var inspectSub1 = dgr.Item as Win_dvl_MoldRegularInspect_U_Sub_CodeView;

                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldRInspectID", "");
                            sqlParameter.Add("MoldRInspectSubSeq", inspectSub1.InspectSubSeq);
                            sqlParameter.Add("MoldInsBasisID", inspectSub1.MoldInspectBasisID);
                            sqlParameter.Add("MoldInsSeq", inspectSub1.InspectSubSeq);
                            //sqlParameter.Add("MldRValue", inspectSub1.MldRValue);
                            //sqlParameter.Add("MldRValue", 0.0);
                            sqlParameter.Add("MldRValue", inspectSub1.MldRValue != null ? (lib.IsNumOrAnother(inspectSub1.MldRValue) ? double.Parse(inspectSub1.MldRValue) : 0.0) : 0.0);
                            sqlParameter.Add("MldRInspectLegend", inspectSub1.MldRInspectLegend);
                            sqlParameter.Add("Comments", inspectSub1.Comments != null ? inspectSub1.Comments : "");
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro2 = new Procedure();
                            pro2.Name = "xp_dvlMoldIns_iRegularInspectSub";
                            pro2.OutputUseYN = "N";
                            pro2.OutputName = "MoldRInspectID";
                            pro2.OutputLength = "10";

                            Prolist.Add(pro2);
                            ListParameter.Add(sqlParameter);
                        }

                        for (int i = 0; i < dgdMold_InspectSub2.Items.Count; i++)
                        {
                            DataGridRow dgr = lib.GetRow(i, dgdMold_InspectSub2);
                            var inspectSub2 = dgr.Item as Win_dvl_MoldRegularInspect_U_Sub_CodeView;

                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldRInspectID", "");
                            sqlParameter.Add("MoldRInspectSubSeq", inspectSub2.InspectSubSeq);
                            sqlParameter.Add("MoldInsBasisID", inspectSub2.MoldInspectBasisID);
                            sqlParameter.Add("MoldInsSeq", inspectSub2.InspectSubSeq);
                            sqlParameter.Add("MldRValue", inspectSub2.MldRValue != null ? (lib.IsNumOrAnother(inspectSub2.MldRValue) ? double.Parse(inspectSub2.MldRValue) : 0.0) : 0.0);
                            //sqlParameter.Add("MldRValue", inspectSub2.MldRValue);
                            sqlParameter.Add("MldRInspectLegend", inspectSub2.MldRInspectLegend);
                            sqlParameter.Add("Comments", inspectSub2.Comments != null ? inspectSub2.Comments : "");
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro3 = new Procedure();
                            pro3.Name = "xp_dvlMoldIns_iRegularInspectSub";
                            pro3.OutputUseYN = "N";
                            pro3.OutputName = "MoldRInspectID";
                            pro3.OutputLength = "10";

                            Prolist.Add(pro3);
                            ListParameter.Add(sqlParameter);
                        }

                        List<KeyValue> list_Result = new List<KeyValue>();
                        list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS(Prolist, ListParameter);
                        string sGetMoldRInspectID = string.Empty;

                        if (list_Result[0].key.ToLower() == "success")
                        {
                            list_Result.RemoveAt(0);
                            for (int i = 0; i < list_Result.Count; i++)
                            {
                                KeyValue kv = list_Result[i];
                                if (kv.key == "MoldRInspectID")
                                {
                                    sGetMoldRInspectID = kv.value;
                                    dicCompare.Add("MoldRInspectID", sGetMoldRInspectID);
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
                    else
                    {
                        dicCompare.Add("MoldRInspectID", strMoldRInspectID);
                        //sqlParameter.Add("MoldInspectBasisDate", dtpMoldInspectDate.SelectedDate.Value.ToString("yyyyMMdd"));
          

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_dvlMoldIns_uRegularInspect";
                        pro1.OutputUseYN = "N";
                        pro1.OutputName = "MoldRInspectID";
                        pro1.OutputLength = "10";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        for (int i = 0; i < dgdMold_InspectSub1.Items.Count; i++)
                        {
                            DataGridRow dgr = lib.GetRow(i, dgdMold_InspectSub1);
                            var inspectSub1 = dgr.Item as Win_dvl_MoldRegularInspect_U_Sub_CodeView;

                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldRInspectID", txtMoldRInspectID.Text);
                            sqlParameter.Add("MoldRInspectSubSeq", inspectSub1.InspectSubSeq);
                            sqlParameter.Add("MoldInsBasisID", inspectSub1.MoldInspectBasisID);
                            sqlParameter.Add("MoldInsSeq", inspectSub1.InspectSubSeq);
                            sqlParameter.Add("MldRValue", inspectSub1.MldRValue);
                            sqlParameter.Add("MldRInspectLegend", inspectSub1.MldRInspectLegend);
                            sqlParameter.Add("Comments", inspectSub1.Comments);
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro2 = new Procedure();
                            pro2.Name = "xp_dvlMoldIns_iRegularInspectSub";
                            pro2.OutputUseYN = "N";
                            pro2.OutputName = "MoldRInspectID";
                            pro2.OutputLength = "10";

                            Prolist.Add(pro2);
                            ListParameter.Add(sqlParameter);
                        }

                        for (int i = 0; i < dgdMold_InspectSub2.Items.Count; i++)
                        {
                            DataGridRow dgr = lib.GetRow(i, dgdMold_InspectSub2);
                            var inspectSub2 = dgr.Item as Win_dvl_MoldRegularInspect_U_Sub_CodeView;

                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldRInspectID", txtMoldRInspectID.Text);
                            sqlParameter.Add("MoldRInspectSubSeq", inspectSub2.InspectSubSeq);
                            sqlParameter.Add("MoldInsBasisID", inspectSub2.MoldInspectBasisID);
                            sqlParameter.Add("MoldInsSeq", inspectSub2.InspectSubSeq);
                            sqlParameter.Add("MldRValue", inspectSub2.MldRValue);
                            sqlParameter.Add("MldRInspectLegend", inspectSub2.MldRInspectLegend);
                            sqlParameter.Add("Comments", inspectSub2.Comments);
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro3 = new Procedure();
                            pro3.Name = "xp_dvlMoldIns_iRegularInspectSub";
                            pro3.OutputUseYN = "N";
                            pro3.OutputName = "MoldRInspectID";
                            pro3.OutputLength = "10";

                            Prolist.Add(pro3);
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
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("오류 발생,오류 내용 : " + ex.ToString());
                }
                finally
                {
                    DataStore.Instance.CloseConnection();
                }
            }
            else { flag = false; }

            return flag;
        }

        //수정되어 필요없지만 일단 주석처리 보관(2018.05.31)
        private bool AddSubData(Win_dvl_MoldRegularInspect_U_Sub_CodeView WinMoldInspect)
        {
            bool flag = true;

            //Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
            //sqlParameter.Add("MoldRInspectID", WinMoldInspect.MoldRInspectID);
            //sqlParameter.Add("MoldRInspectSubSeq", WinMoldInspect.InspectSubSeq);
            //sqlParameter.Add("MoldInsBasisID", WinMoldInspect.MoldInspectBasisID);
            //sqlParameter.Add("MoldInsSeq", WinMoldInspect.InspectSubSeq);
            //sqlParameter.Add("MldRValue", WinMoldInspect.MldRValue);
            //sqlParameter.Add("MldRInspectLegend", WinMoldInspect.MldRInspectLegend);
            //sqlParameter.Add("Comments", WinMoldInspect.Comments);
            //sqlParameter.Add("CreateUserID", "");

            //string[] resultSub = DataStore.Instance.ExecuteProcedure("xp_dvlMoldIns_iRegularInspectSub", sqlParameter, false);

            //if (!resultSub[0].Equals("success"))
            //{
            //    flag = false;
            //    MessageBox.Show("실패 ㅠㅠ컥");
            //}

            return flag;
        }

        //추가, 수정 시 필수 입력 체크
        private bool CheckData()
        {
            bool flag = true;

            if (txtMoldID.Tag == null || txtMoldID.Tag.ToString().Equals(""))
            {
                MessageBox.Show("금형 선택이 잘못되었습니다. enter키 또는 품명 옆의 버튼을 이용하여 다시 입력해주세요");
                flag = false;
                return flag;
            }

            if (dtpMoldInspectDate.SelectedDate == null)
            {
                MessageBox.Show("점검일자가 선택되지 않았습니다. 점검일자를 선택해주세요");
                flag = false;
                return flag;
            }

            if (txtPerson.Tag == null || txtPerson.Tag.ToString().Equals(""))
            {
                MessageBox.Show("점검자 선택이 잘못되었습니다. enter키 또는 품명 옆의 버튼을 이용하여 다시 입력해주세요");
                flag = false;
                return flag;
            }

            return flag;
        }

        #endregion

        #region 플러스 파인더 및 enter focus move

        //금형번호(textbox)
        private void txtMoldID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //pf.ReturnCode(txtMoldID, 51, txtMoldID.Text);
                pf.ReturnCode(txtMoldID, 51, "");

                if (txtMoldID.Tag != null)
                {
                    GetMoldInspectInfo(txtMoldID.Tag.ToString());
                }

                dtpMoldInspectDate.Focus();
            }
        }

        //금형번호(button)
        private void btnPfMoldID_Click(object sender, RoutedEventArgs e)
        {
            //pf.ReturnCode(txtMoldID, 51, txtMoldID.Text);
            pf.ReturnCode(txtMoldID, 51, "");

            if (txtMoldID.Tag != null)
            {
                GetMoldInspectInfo(txtMoldID.Tag.ToString());
            }

            dtpMoldInspectDate.Focus();
        }

        //금형번호 선택시, 선택된 금형의 정보를 가져온다.
        private void GetMoldInspectInfo(string strMoldID)
        {
            try
            {
                if (dgdMold_InspectSub1.Items.Count > 0)
                {
                    dgdMold_InspectSub1.Items.Clear();
                }

                if (dgdMold_InspectSub2.Items.Count > 0)
                {
                    dgdMold_InspectSub2.Items.Clear();
                }

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("MoldID", strMoldID);
                //sqlParameter.Add("ntotSearch", ChkntotSearch.IsChecked == true ? 1 : 0);
                //sqlParameter.Add("ntotSearchGbn", ChkntotSearch.IsChecked == true ? (ntotSearchGbn.SelectedValue == null ? 0 : ntotSearchGbn.SelectedIndex + 1) : 0);
                //sqlParameter.Add("stotSearch", txttotSearch.Text.ToString());
                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMoldIns_sRegularInspectSubByMoldID", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinMoldRIns = new Win_dvl_MoldRegularInspect_U_Sub_CodeView()
                            {
                                MoldID = dr["MoldID"].ToString(),
                                MoldInspectBasisID = dr["MoldInspectBasisID"].ToString(),
                                InspectSubSeq = dr["MoldInspectSeq"].ToString(),
                                MoldInspectItemName = dr["MoldInspectItemName"].ToString(),
                                MoldInspectContent = dr["MoldInspectContent"].ToString(),
                                MoldInspectCheckGbn = dr["MoldInspectCheckGbn"].ToString(),
                                MoldInspectCheckName = dr["MoldInspectCheckName"].ToString(),
                                MoldInspectCycleGbn = dr["MoldInspectCycleGbn"].ToString(),
                                MoldInspectCycleName = dr["MoldInspectCycleName"].ToString(),
                                MoldInspectCycleDate = dr["MoldInspectCycleDate"].ToString(),
                                MoldInspectRecordGbn = dr["MoldInspectRecordGbn"].ToString()
                            };

                            WinMoldRIns.flagLegend = false;
                            WinMoldRIns.flagComments = false;

                            if (WinMoldRIns.MoldInspectRecordGbn.Equals("01"))
                            {
                                dgdMold_InspectSub1.Items.Add(WinMoldRIns);
                            }
                            else
                            {
                                dgdMold_InspectSub2.Items.Add(WinMoldRIns);
                            }
                        }

                        txtMoldBasisID.Text = drc[0]["MoldInspectBasisID"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생,오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        //점검일자
        private void dtpMoldInspectDate_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                dtpMoldInspectDate.IsDropDownOpen = true;
            }
        }

        //점검일자
        private void dtpMoldInspectDate_CalendarClosed(object sender, RoutedEventArgs e)
        {
            txtPerson.Focus();
        }

        //점검자
        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                pf.ReturnCode(txtPerson, 2, "");
                txtComments.Focus();
            }
        }

        //점검자
        private void btnPfPerson_Click(object sender, RoutedEventArgs e)
        {
            pf.ReturnCode(txtPerson, 2, "");
            txtComments.Focus();
        }

        #endregion

        #region 서브그리드 이벤트

        //Sub1(범례), Sub2(수치)
        private void DataGridSub_EnableChanged(object sender, KeyEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                DataGridCell data = sender as DataGridCell;
                string strName = lib.GetParent<DataGrid>(data).Name;

                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    data.IsEditing = false;
                    int rowCount = 0;
                    int colCount = 0;

                    if (strName.Equals(dgdMold_InspectSub1.Name))
                    {
                        WinRInsSub1 = dgdMold_InspectSub1.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;
                        rowCount = dgdMold_InspectSub1.Items.IndexOf(lib.GetParent<DataGrid>(data).CurrentItem);

                        if (data.Column.Header.ToString().Equals("점검결과"))
                        {
                            colCount = dgdMold_InspectSub1.Columns.IndexOf(dgdtpeComments);
                            dgdMold_InspectSub1.CurrentCell = new DataGridCellInfo(dgdMold_InspectSub1.Items[rowCount], dgdMold_InspectSub1.Columns[colCount]);
                        }
                        else if (data.Column.Header.ToString().Equals("비고"))
                        {
                            colCount = dgdMold_InspectSub1.Columns.IndexOf(dgdtpeMldRInspectLegend);
                            int colCountSub2 = 0;

                            if (dgdMold_InspectSub2 != null && dgdMold_InspectSub2.Items.Count > 0)
                            {
                                colCountSub2 = dgdMold_InspectSub2.Columns.IndexOf(dgdtpeMldRValue);
                            }

                            if (dgdMold_InspectSub1.Items.Count - 1 == rowCount)
                            {
                                if (colCountSub2 > 0)
                                {
                                    dgdMold_InspectSub2.Focus();
                                    dgdMold_InspectSub2.CurrentCell = new DataGridCellInfo(dgdMold_InspectSub2.Items[0], dgdMold_InspectSub2.Columns[colCountSub2]);
                                }
                                else
                                {
                                    btnSave.Focus();
                                }
                            }
                            else
                            {
                                dgdMold_InspectSub1.CurrentCell = new DataGridCellInfo(dgdMold_InspectSub1.Items[rowCount + 1], dgdMold_InspectSub1.Columns[colCount]);
                            }
                        }
                    }
                    else if (strName.Equals(dgdMold_InspectSub2.Name))
                    {
                        WinRInsSub2 = dgdMold_InspectSub2.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;
                        rowCount = dgdMold_InspectSub2.Items.IndexOf(lib.GetParent<DataGrid>(data).CurrentItem);

                        if (data.Column.Header.ToString().Equals("점검결과"))
                        {
                            colCount = dgdMold_InspectSub2.Columns.IndexOf(dgdtpeComments);
                            dgdMold_InspectSub2.CurrentCell = new DataGridCellInfo(dgdMold_InspectSub2.Items[rowCount], dgdMold_InspectSub2.Columns[colCount]);
                        }
                        else if (data.Column.Header.ToString().Equals("비고"))
                        {
                            colCount = dgdMold_InspectSub2.Columns.IndexOf(dgdtpeMldRValue);

                            if (dgdMold_InspectSub2.Items.Count - 1 == rowCount)
                            {
                                btnSave.Focus();
                            }
                            else
                            {
                                dgdMold_InspectSub2.CurrentCell = new DataGridCellInfo(dgdMold_InspectSub2.Items[rowCount + 1], dgdMold_InspectSub2.Columns[colCount]);                                
                            }
                        }
                    }
                }
            }
        }

        //Sub1(범례), Sub2(수치)
        private void DataGridSub_GotFocus(object sender, RoutedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                DataGridCell cell = sender as DataGridCell;

                if (cell.IsEditing == false)
                {
                    cell.IsEditing = true;
                }
            }
        }

        //Sub1(범례), Sub2(수치)
        private void DataGridCell_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lib.DataGridINTextBoxFocusByMouseUP(sender, e);
        }

        //서브 그리드 인 포커스
        private void dgdSub_TextFocus(object sender, KeyEventArgs e)
        {
            lib.DataGridINTextBoxFocus(sender, e);
        }

        //서브1 점검결과_enter key 없이도 값이 대입되도록
        private void dgdtpetxtMldRInspectLegend_LostFocus(object sender, RoutedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinRInsSub1 = dgdMold_InspectSub1.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;

                if (WinRInsSub1 != null)
                {
                    TextBox tb1 = sender as TextBox;
                    WinRInsSub1.MldRInspectLegend = tb1.Text;
                    sender = tb1;
                }
                else
                {
                    MessageBox.Show("현재 줄의 정보가 확인되지 않습니다.");
                }
            }
        }

        //서브1 점검결과
        private void dgdtpetxtMldRInspectLegend_KeyDown(object sender, KeyEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinRInsSub1 = dgdMold_InspectSub1.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;

                if (e.Key == Key.Enter)
                {
                    if (WinRInsSub1 != null)
                    {
                        TextBox tb1 = sender as TextBox;
                        WinRInsSub1.MldRInspectLegend = tb1.Text;
                        sender = tb1;
                    }
                    else
                    {
                        MessageBox.Show("현재 줄의 정보가 확인되지 않습니다.");
                    }
                }
            }
        }

        //서브1 비고_enter key 없이도 값이 대입되도록
        private void dgdtpetxtComments_LostFocus(object sender, RoutedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinRInsSub1 = dgdMold_InspectSub1.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;

                if (WinRInsSub1 != null)
                {
                    TextBox tb1 = sender as TextBox;
                    WinRInsSub1.Comments = tb1.Text;
                    sender = tb1;
                }
            }
        }

        //서브1 비고
        private void dgdtpetxtComments_KeyDown(object sender, KeyEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                if (e.Key == Key.Enter)
                {
                    WinRInsSub1 = dgdMold_InspectSub1.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;

                    if (WinRInsSub1 != null)
                    {
                        TextBox tb1 = sender as TextBox;
                        WinRInsSub1.Comments = tb1.Text;
                        sender = tb1;
                    }
                }
            }
        }

        //서브2 점검결과
        private void dgdtxtMldRValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                if (e.Key == Key.Enter)
                {
                    WinRInsSub2 = dgdMold_InspectSub2.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;

                    if (WinRInsSub2 != null)
                    {
                        TextBox tb1 = sender as TextBox;
                        WinRInsSub2.MldRInspectLegend = tb1.Text;
                        sender = tb1;
                    }
                }
            }
        }

        //서브2 점검결과_enter key 없이도 값이 대입되도록
        private void dgdtxtMldRValue_LostFocus(object sender, RoutedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinRInsSub2 = dgdMold_InspectSub2.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;

                if (WinRInsSub2 != null)
                {
                    TextBox tb1 = sender as TextBox;
                    WinRInsSub2.MldRInspectLegend = tb1.Text;
                    sender = tb1;
                }
            }
        }

        //서브2 점검결과(숫자만 입력)
        private void dgdtxtMldRValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            lib.CheckIsNumericbyThree((TextBox)sender, e);
        }

        //서브2 비고
        private void dgdtxtComments2_KeyDown(object sender, KeyEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                if (e.Key == Key.Enter)
                {
                    WinRInsSub2 = dgdMold_InspectSub2.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;

                    if (WinRInsSub2 != null)
                    {
                        TextBox tb1 = sender as TextBox;
                        WinRInsSub2.Comments = tb1.Text;
                        sender = tb1;
                    }
                }
            }
        }

        //서브2 비고_enter key 없이도 값이 대입되도록
        private void dgdtxtComments2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinRInsSub2 = dgdMold_InspectSub2.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;

                if (WinRInsSub2 != null)
                {
                    TextBox tb1 = sender as TextBox;
                    WinRInsSub2.Comments = tb1.Text;
                    sender = tb1;
                }
            }
        }


        #endregion

        private void ChkntotSearch_Checked(object sender, RoutedEventArgs e)
        {
            ChkntotSearch.IsChecked = true;
            ntotSearchGbn.IsEnabled = true;

            System.Diagnostics.Debug.WriteLine("인덱스 : " + ntotSearchGbn.SelectedIndex);
            System.Diagnostics.Debug.WriteLine("아이템 : " + ntotSearchGbn.SelectedItem);
            System.Diagnostics.Debug.WriteLine("밸류 : " + ntotSearchGbn.SelectedValue);

        }

        //출고요청상태 체크박스 체크해제
        private void ChkntotSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            ChkntotSearch.IsChecked = false;
            ntotSearchGbn.IsEnabled = false;
        }

        private void ntotSearchGbn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void txttotSearch_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void txttotSearch_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        //정기점검대상조회 버튼클릭 정기점검일때만 활성화
        private void BtnRegualrSrh_Click(object sender, RoutedEventArgs e)
        {
            pf.ReturnCode(txtMoldID, 51, "");

            if (txtMoldID.Tag != null)
            {
                GetMoldInspectInfo(txtMoldID.Tag.ToString());
            }
        }

        private void CboRegularGbn_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                if (cboRegularGbn.SelectedValue.Equals("01"))
                {
                    btnRegualrSrh.IsEnabled = true;
                }
                else
                {
                    btnRegualrSrh.IsEnabled = false;
                }
            
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            


        }
        // 비트맵을 비트맵 이미지로 형태변환시키기.<0823 허윤구> 
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private BitmapImage SetImage(string ImageName, string FolderName)
        {
            //bool ExistFile = false;
            BitmapImage bit = null;
            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);
            if (_ftp == null) { return null; }

            //string[] fileListDetail;
            //fileListDetail = _ftp.directoryListSimple(FolderName, Encoding.Default);

            //ExistFile = FileInfoAndFlag(fileListDetail, ImageName);
            //if (ExistFile)
            //{
            bit = DrawingImageByByte2(FTP_ADDRESS + '/' + FolderName + '/' + ImageName + "");
            //}

            return bit;
        }

        private BitmapImage DrawingImageByByte2(string ftpFilePath)
        {
            BitmapImage image = new BitmapImage();

            try
            {
                WebClient ftpClient = new WebClient();
                ftpClient.Credentials = new NetworkCredential(FTP_ID, FTP_PASS);
                byte[] imageByte = ftpClient.DownloadData(ftpFilePath);

                //MemoryStream mStream = new MemoryStream();
                //mStream.Write(imageByte, 0, Convert.ToInt32(imageByte.Length));

                using (MemoryStream stream = new MemoryStream(imageByte))
                {
                    image.BeginInit();
                    image.StreamSource = stream;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.Freeze();
                }

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("1" + ex.Message + " / " + ex.Source);
                FTP_Trigger = false;
                //throw ex;
            }

            return image;
        }


        private void img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount == 2)
                {

                    System.Windows.Controls.Image senderImg = sender as System.Windows.Controls.Image;
                    var MoldSub = senderImg.DataContext as Win_dvl_MoldRegularInspect_U_Sub_CodeView;

                    if (MoldSub != null
                        && MoldSub.MoldInspectImageFile != null
                        && !MoldSub.MoldInspectImageFile.Trim().Equals(""))
                    {

                        string str_path = string.Empty;
                        str_path = FTP_ADDRESS + '/' + MoldSub.MoldInspectBasisID;
                        _ftp = new FTP_EX(str_path, FTP_ID, FTP_PASS);

                        string str_remotepath = string.Empty;
                        string str_localpath = string.Empty;

                        str_remotepath = "/" + MoldSub.MoldInspectImageFile;
                        str_localpath = LOCAL_DOWN_PATH + "\\" + MoldSub.MoldInspectImageFile;

                        DirectoryInfo DI = new DirectoryInfo(LOCAL_DOWN_PATH);      // Temp 폴더가 없는 컴터라면, 만들어 줘야지.
                        if (DI.Exists == false)
                        {
                            DI.Create();
                        }

                        FileInfo file = new FileInfo(str_localpath);
                        if (file.Exists)
                        {
                            file.Delete();
                        }

                        _ftp.download(str_remotepath, str_localpath);

                        ProcessStartInfo proc = new ProcessStartInfo(str_localpath);
                        proc.UseShellExecute = true;
                        Process.Start(proc);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        //라벨
        private void LblRegularGbnSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkRegularGbnSrh.IsChecked == true)
            {
                chkRegularGbnSrh.IsChecked = false;
            }
            else
            {
                chkRegularGbnSrh.IsChecked = true;
            }
        }
        //점검주기구분 체크 ㅇ
        private void ChkRegularGbnSrh_Checked(object sender, RoutedEventArgs e)
        {
            cboRegularGbnSrh.IsEnabled = true;
        }
        //점검주기구분  체크 ㄴ 
        private void ChkRegularGbnSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            cboRegularGbnSrh.IsEnabled = false;
        }

        // 품명
        // 품명 검색 라벨 왼쪽 클릭 이벤트
        private void lblArticleSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticleSrh.IsChecked == true)
            {
                chkArticleSrh.IsChecked = false;
            }
            else
            {
                chkArticleSrh.IsChecked = true;
            }
        }
        // 품명 검색 체크박스 이벤트
        private void chkArticleSrh_Checked(object sender, RoutedEventArgs e)
        {
            chkArticleSrh.IsChecked = true;

            txtArticleSrh.IsEnabled = true;
            btnPfArticleSrh.IsEnabled = true;
        }
        private void chkArticleSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            chkArticleSrh.IsChecked = false;

            txtArticleSrh.IsEnabled = false;
            btnPfArticleSrh.IsEnabled = false;
        }
        // 품명 검색 엔터 → 플러스 파인더 이벤트
        private void txtArticleSrh_KeyDown(object sender, KeyEventArgs e)
        {

            //if (e.Key == Key.Enter)
            //{
            //    rowNum = 0;
            //    re_Search();
            //}

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                MainWindow.pf.ReturnCode(txtArticleSrh, 76, "");
            }
        }
        // 품명 검색 플러스파인더 이벤트
        private void btnPfArticleSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh, 76, "");
        }
    }
}
