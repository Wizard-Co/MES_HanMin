using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WizMes_HanMin.PopUP;
using WPF.MDI;

namespace WizMes_HanMin
{
    /**************************************************************************************************
   '** System 명 : WizMes_HanMin
   '** Author    : Wizard
   '** 작성자    : 최준호
   '** 내용      : 금형 타발수 조회(AFT 최규환 과장 요청으로 생성)
   '** 생성일자  : 2018.10~2019.2 월 사이
   '** 변경일자  : 
   '**------------------------------------------------------------------------------------------------
   ''*************************************************************************************************
   ' 변경일자  , 변경자, 요청자    , 요구사항ID  , 요청 및 작업내용
   '**************************************************************************************************
   ' ex) 2015.11.09, 박진성, 오영      ,S_201510_AFT_03 , 월별집계(가로) 순서 변경 : 합계/10월/9월/8월 순으로
   ' 2019.06.25  최준호 , 최규환     ,품명 다중선택 가능하게 해달라 => 프로시저 아닌 프로그램상에서 하는걸로 수정
   '**************************************************************************************************/

    /// <summary>
    /// Win_dvl_Molding_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_dvl_Molding_U : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        string strFlag = string.Empty;
        int rowNum = 0;
        bool MultiArticle = false;

        string ArticleSrh1 = string.Empty;
        string ArticleSrh2 = string.Empty;
        string ArticleSrh3 = string.Empty;
        string ArticleSrh4 = string.Empty;
        string ArticleSrh5 = string.Empty;

        Win_dvl_Molding_U_CodeView WinMold = new Win_dvl_Molding_U_CodeView();
        Win_dvl_Molding_U_Parts_CodeView WinMoldParts = new Win_dvl_Molding_U_Parts_CodeView();
        
        // FTP 활용모음.
        bool ftpDelete1 = false;
        bool ftpDelete2 = false;
        bool ftpDelete3 = false;
        string strImagePath = string.Empty;
        string strFullPath = string.Empty;

        string strAttPath1 = string.Empty;
        string strAttPath2 = string.Empty;
        string strAttPath3 = string.Empty;

        string FullPath1 = string.Empty;
        string FullPath2 = string.Empty;
        string FullPath3 = string.Empty;

        private FTP_EX _ftp = null;
        private List<UploadFileInfo> _listFileInfo = new List<UploadFileInfo>();

        internal struct UploadFileInfo          //FTP.
        {
            public string Filename { get; set; }
            public FtpFileType Type { get; set; }
            public DateTime LastModifiedTime { get; set; }
            public long Size { get; set; }
            public string Filepath { get; set; }
        }
        internal enum FtpFileType
        {
            None,
            DIR,
            File
        }

        //string FTP_ADDRESS = "ftp://wizis.iptime.org/ImageData/Mold";
        //string FTP_ADDRESS = "ftp://wizis.iptime.org/ImageData/Mold";
        string FTP_ADDRESS = "ftp://" + LoadINI.FileSvr + ":"
            + LoadINI.FTPPort + LoadINI.FtpImagePath + "/Mold";
        //string FTP_ADDRESS = "ftp://222.104.222.145:25000/ImageData/Mold";
        //string FTP_ADDRESS = "ftp://192.168.0.95/ImageData/Mold";
        private const string FTP_ID = "wizuser";
        private const string FTP_PASS = "wiz9999";
        private const string LOCAL_DOWN_PATH = "C:\\Temp";

        public Win_dvl_Molding_U()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            Lib.Instance.UiLoading(this);
            SetComboBox();
            btnToday_Click(null, null);
        }

        private void SetComboBox()
        {
            //ObservableCollection<CodeView> ovcMoldCycle = ComboBoxUtil.Instance.GetCMCode_SetComboBox("MLDCYCLEGBN", "");
            //this.cboRegularGubun.ItemsSource = ovcMoldCycle;
            //this.cboRegularGubun.DisplayMemberPath = "code_name";
            //this.cboRegularGubun.SelectedValuePath = "code_id";

            List<string[]> lstDvlYN = new List<string[]>();
            string[] strDvl_1 = { "Y", "개발" };
            string[] strDvl_2 = { "N", "양산" };
            lstDvlYN.Add(strDvl_1);
            lstDvlYN.Add(strDvl_2);

            ObservableCollection<CodeView> ovcDvlYN = ComboBoxUtil.Instance.Direct_SetComboBox(lstDvlYN);
            this.cboDevYN.ItemsSource = ovcDvlYN;
            this.cboDevYN.DisplayMemberPath = "code_name";
            this.cboDevYN.SelectedValuePath = "code_id";

            this.cboDevYNSrh.ItemsSource = ovcDvlYN;
            this.cboDevYNSrh.DisplayMemberPath = "code_name";
            this.cboDevYNSrh.SelectedValuePath = "code_id";

            ObservableCollection<CodeView> ovMoldPlace = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "MOLDPLACE", "Y", "");
            this.cboStorgeLocation.ItemsSource = ovMoldPlace;
            this.cboStorgeLocation.DisplayMemberPath = "code_name";
            this.cboStorgeLocation.SelectedValuePath = "code_id";

            List<string[]> lstMD = new List<string[]>();
            string[] strMD_M = { "0", "월" };
            string[] strMD_D = { "1", "일" };
            lstMD.Add(strMD_M);
            lstMD.Add(strMD_D);

            ObservableCollection<CodeView> ovcMD = ComboBoxUtil.Instance.Direct_SetComboBox(lstMD);
            this.cboMD.ItemsSource = ovcMD;
            this.cboMD.DisplayMemberPath = "code_name";
            this.cboMD.SelectedValuePath = "code_id";
            cboMD.SelectedIndex = 0;
        }

        #region 라벨 클릭 및 체크박스 이벤트

        //금형발주일
        private void lblDate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkDate.IsChecked == true) { chkDate.IsChecked = false; }
            else { chkDate.IsChecked = true; }
        }

        //금형발주일
        private void chkDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = true;
            dtpEDate.IsEnabled = true;
        }

        //금형발주일
        private void chkDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
        }

        //금월
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];
        }

        //금일
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }

        //폐기건 포함
        private void lblDisCardSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkDisCardSrh.IsChecked == true) { chkDisCardSrh.IsChecked = false; }
            else { chkDisCardSrh.IsChecked = true; }
        }

        //폐기건 포함
        private void chkDisCardSrh_Checked(object sender, RoutedEventArgs e)
        {

        }

        //폐기건 포함
        private void chkDisCardSrh_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        //타발 수 점검필요
        private void lblCheckNeedMoldSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCheckNeedMoldSrh.IsChecked == true) { chkCheckNeedMoldSrh.IsChecked = false; }
            else { chkCheckNeedMoldSrh.IsChecked = true; }
        }

        //타발 수 점검필요
        private void chkCheckNeedMoldSrh_Checked(object sender, RoutedEventArgs e)
        {

        }

        //타발 수 점검필요
        private void chkCheckNeedMoldSrh_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        //금형LotNo(%)
        private void lblMoldNoSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkMoldNoSrh.IsChecked == true) { chkMoldNoSrh.IsChecked = false; }
            else { chkMoldNoSrh.IsChecked = true; }
        }

        //금형LotNo(%)
        private void chkMoldNoSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtMoldNoSrh.IsEnabled = true;
        }

        //금형LotNo(%)
        private void chkMoldNoSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtMoldNoSrh.IsEnabled = false;
        }

        //품명
        private void lblArticleSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticleSrh.IsChecked == true) { chkArticleSrh.IsChecked = false; }
            else { chkArticleSrh.IsChecked = true; }
        }

        //품명
        private void chkArticleSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtArticleSrh.IsEnabled = true;
            btnPfArticleSrh.IsEnabled = true;
        }

        //품명
        private void chkArticleSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtArticleSrh.IsEnabled = false;
            btnPfArticleSrh.IsEnabled = false;
        }

        //품명
        private void txtArticleSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh, 68, "");
            }
        }

        //품명
        private void btnPfArticleSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh, 68, "");
        }

        //거래처
        private void lblCustomSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCustomSrh.IsChecked == true) { chkCustomSrh.IsChecked = false; }
            else { chkCustomSrh.IsChecked = true; }
        }

        //거래처
        private void chkCustomSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtCustomSrh.IsEnabled = true;
            btnPfCustomSrh.IsEnabled = true;
        }

        //거래처
        private void chkCustomSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtCustomSrh.IsEnabled = false;
            btnPfCustomSrh.IsEnabled = false;
        }

        //거래처
        private void txtCustomSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
            }
        }

        //거래처
        private void btnPfCustomSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //개발/양산
        private void lblDevYNSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkDevYNSrh.IsChecked == true) { chkDevYNSrh.IsChecked = false; }
            else { chkDevYNSrh.IsChecked = true; }
        }

        //개발/양산
        private void chkDevYNSrh_Checked(object sender, RoutedEventArgs e)
        {
            cboDevYNSrh.IsEnabled = true;
        }

        //개발/양산
        private void chkDevYNSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            cboDevYNSrh.IsEnabled = false;
        }

        #endregion

        /// <summary>
        /// 수정,추가 저장 후
        /// </summary>
        private void CanBtnControl()
        {
            Lib.Instance.UiButtonEnableChange_IUControl(this);
            grdInput1.IsEnabled = false;
            gbxInput.IsEnabled = false;
            grxInput.IsEnabled = false;
            //dgdMain.IsEnabled = true;
            dgdMain.IsHitTestVisible = true;
        }

        /// <summary>
        /// 수정,추가 진행 중
        /// </summary>
        private void CantBtnControl()
        {
            Lib.Instance.UiButtonEnableChange_SCControl(this);
            grdInput1.IsEnabled = true;
            gbxInput.IsEnabled = true;
            grxInput.IsEnabled = true;
            //dgdMain.IsEnabled = false;
            dgdMain.IsHitTestVisible = false;
        }

        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CantBtnControl();
            strFlag = "I";

            tbkMsg.Text = "자료 입력 중";
            rowNum = dgdMain.SelectedIndex;
            this.DataContext = null;

            if (dgdPartsCode.Items.Count > 0)
            {
                dgdPartsCode.Items.Clear();
                dgdPartsCode.Refresh();
            }

            dtpEvalDate.SelectedDate = DateTime.Today;
            dtpProdCompDate.SelectedDate = DateTime.Today;
            dtpProdDueDate.SelectedDate = DateTime.Today;
            dtpProdOrderDate.SelectedDate = DateTime.Today;
            dtpSetDateM.SelectedDate = DateTime.Today;
            dtpSetDateD.SelectedDate = DateTime.Today;
            dtpSetInitHitCountDate.SelectedDate = DateTime.Today;

            cboStorgeLocation.SelectedIndex = 0;
            cboDevYN.SelectedIndex = 0;
            cboMD.SelectedIndex = 0;
        }

        //수정
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            WinMold = dgdMain.SelectedItem as Win_dvl_Molding_U_CodeView;

            if (WinMold != null)
            {
                rowNum = dgdMain.SelectedIndex;
                dgdMain.IsHitTestVisible = false;
                tbkMsg.Text = "자료 수정 중";
                lblMsg.Visibility = Visibility.Visible;
                CantBtnControl();
                strFlag = "U";
            }
        }

        //삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            WinMold = dgdMain.SelectedItem as Win_dvl_Molding_U_CodeView;

            if (WinMold == null)
            {
                MessageBox.Show("삭제할 데이터가 지정되지 않았습니다. 삭제데이터를 지정하고 눌러주세요");
                return;
            }
            else
            {
                if (dgdMain.SelectedIndex == 0)
                    rowNum = 0;
                else
                    rowNum = dgdMain.SelectedIndex - 1;

                if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (DeleteData(WinMold.MoldID))
                    {
                        re_Search(rowNum);
                    }
                }
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
            rowNum = 0;
            re_Search(rowNum);
        }

        //저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveData(strFlag, txtMoldID.Text))
            {
                CanBtnControl();
                if (dgdPartsCode.Items.Count > 0)
                {
                    dgdPartsCode.Items.Clear();
                }

                re_Search(rowNum);
                strFlag = string.Empty;
                dgdMain.IsHitTestVisible = true;
                ftpDelete1 = false;
                ftpDelete2 = false;
                ftpDelete3 = false;

                

            }
            else
            {
                MessageBox.Show("저장실패");
            }
        }

        //취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CanBtnControl();
            if (dgdPartsCode.Items.Count > 0)
            {
                dgdPartsCode.Items.Clear();
            }

            if (!strFlag.Equals(string.Empty))
            {
                re_Search(rowNum);
            }

            strFlag = string.Empty;
            dgdMain.IsHitTestVisible = true;
        }

        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[4];
            lst[0] = "금형현황";
            lst[1] = "사용 부품";
            lst[2] = dgdMain.Name;
            lst[3] = dgdPartsCode.Name;

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
                    Lib.Instance.GenerateExcel(dt, Name);
                    Lib.Instance.excel.Visible = true;
                }
                else if (ExpExc.choice.Equals(dgdPartsCode.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdPartsCode);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdPartsCode);

                    Name = dgdPartsCode.Name;
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
               

        /// <summary>
        /// 재검색(수정,삭제,추가 저장후에 자동 재검색)
        /// </summary>
        /// <param name="selectedIndex"></param>
        private void re_Search(int selectedIndex)
        {
            FillGrid();

            if (dgdMain.Items.Count > 0)
            {
                dgdMain.SelectedIndex = selectedIndex;
            }
        }

        /// <summary>
        /// 실조회
        /// </summary>
        private void FillGrid()
        {
            if (dgdMain.Items.Count > 0)
            {
                dgdMain.Items.Clear();
            }

            try
            {
                string sql = string.Empty;

                if (ArticleSrh1 != string.Empty)
                {
                    sql = "ProductionArticleID = " + ArticleSrh1+" ";
                }

                if (ArticleSrh2 != string.Empty)
                {
                    if(sql == string.Empty)
                        sql = "ProductionArticleID = " + ArticleSrh2 + " ";
                    else
                        sql += "or ProductionArticleID = " + ArticleSrh2 + " ";
                }

                if (ArticleSrh3 != string.Empty)
                {
                    if (sql == string.Empty)
                        sql = "ProductionArticleID = " + ArticleSrh3 + " ";
                    else
                        sql += "or ProductionArticleID = " + ArticleSrh3 + " ";
                }

                if (ArticleSrh4 != string.Empty)
                {
                    if (sql == string.Empty)
                        sql = "ProductionArticleID = " + ArticleSrh4 + " ";
                    else
                        sql += "or ProductionArticleID = " + ArticleSrh4 + " ";
                }

                if (ArticleSrh5 != string.Empty)
                {
                    if (sql == string.Empty)
                        sql = "ProductionArticleID = " + ArticleSrh5 + " ";
                    else
                        sql += "or ProductionArticleID = " + ArticleSrh5 + " ";
                }


                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("chkDate", chkDate.IsChecked == true ? 1 : 0);
                sqlParameter.Add("FromDate", chkDate.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("ToDate", chkDate.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("nchkArticle", chkArticleSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ArticleID", chkArticleSrh.IsChecked == true ? 
                    (txtArticleSrh.Tag != null ? txtArticleSrh.Tag.ToString() : "") : "");

                sqlParameter.Add("nchkMold", chkMoldNoSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("MoldNo", chkMoldNoSrh.IsChecked == true ? txtMoldNoSrh.Text : "");
                sqlParameter.Add("ndvlYN", chkDevYNSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("dvlYN", chkDevYNSrh.IsChecked == true ? 
                    (cboDevYNSrh.SelectedValue != null ? cboDevYNSrh.SelectedValue.ToString() : "") : "");
                sqlParameter.Add("nChkCustom", chkCustomSrh.IsChecked == true ? 1 : 0);

                sqlParameter.Add("CustomID", chkCustomSrh.IsChecked == true ? 
                    (txtCustomSrh.Tag !=null ? txtCustomSrh.Tag.ToString() : "") : "");
                sqlParameter.Add("nChkDisCardYN", chkDisCardSrh.IsChecked == true ? "Y" : "N");
                sqlParameter.Add("nNeedCheckMold", chkCheckNeedMoldSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("nChkBuyerArticleNo", CheckBoxBuyerArticleSearch.IsChecked == true ? 1 : 0);
                sqlParameter.Add("BuyerArticleNoID", CheckBoxBuyerArticleSearch.IsChecked == true ? 
                    (TextBoxBuyerArticleNoSearch.Tag == null ? "" : TextBoxBuyerArticleNoSearch.Tag.ToString()) : "");

                ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_dvlMold_sMold", sqlParameter, true, "R");

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
                        if (!MultiArticle)
                        {
                            DataRowCollection drc = dt.Rows;

                            foreach (DataRow dr in drc)
                            {
                                var WinMolding = new Win_dvl_Molding_U_CodeView()
                                {
                                    Num = i + 1,
                                    MoldNo = dr["MoldNo"].ToString(),
                                    Article = dr["Article"].ToString(),
                                    MoldID = dr["MoldID"].ToString(),
                                    Comments = dr["Comments"].ToString(),
                                    AfterRepairHitcount = dr["AfterRepairHitcount"].ToString(),
                                    AfterWashHitcount = dr["AfterWashHitcount"].ToString(),
                                    AttFile1 = dr["AttFile1"].ToString(),
                                    AttFile2 = dr["AttFile2"].ToString(),
                                    AttFile3 = dr["AttFile3"].ToString(),
                                    AttPath1 = dr["AttPath1"].ToString(),
                                    AttPath2 = dr["AttPath2"].ToString(),
                                    AttPath3 = dr["AttPath3"].ToString(),
                                    BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                    BuyerModelID = dr["BuyerModelID"].ToString(),
                                    BuyerModelName = dr["BuyerModelName"].ToString(),
                                    ProductionArticleID = dr["ProductionArticleID"].ToString(),
                                    Cavity = dr["Cavity"].ToString(),
                                    DisCardYN = dr["DisCardYN"].ToString(),
                                    dvlYN = dr["dvlYN"].ToString(),
                                    EvalDate = dr["EvalDate"].ToString(),
                                    EvalGrade = dr["EvalGrade"].ToString(),
                                    Evalscore = dr["Evalscore"].ToString(),
                                    Hitcount = dr["Hitcount"].ToString(),
                                    MoldKind = dr["MoldKind"].ToString(),
                                    MoldName = dr["MoldName"].ToString(),
                                    MoldPerson = dr["MoldPerson"].ToString(),
                                    MoldQuality = dr["MoldQuality"].ToString(),
                                    ProdCompDate = dr["ProdCompDate"].ToString(),
                                    ProdCustomName = dr["ProdCustomName"].ToString(),
                                    ProdDueDate = dr["ProdDueDate"].ToString(),
                                    ProdOrderDate = dr["ProdOrderDate"].ToString(),
                                    RealCavity = dr["RealCavity"].ToString(),
                                    SetCheckProdQty = dr["SetCheckProdQty"].ToString(),
                                    SetDate = dr["SetDate"].ToString(),
                                    SetinitHitCount = dr["SetinitHitCount"].ToString(),
                                    SetInitHitCountDate = dr["SetInitHitCountDate"].ToString(),
                                    SetProdQty = dr["SetProdQty"].ToString(),
                                    SetWashingProdQty = dr["SetWashingProdQty"].ToString(),
                                    Spec = dr["Spec"].ToString(),
                                    StorgeLocation = dr["StorgeLocation"].ToString(),
                                    StorgeLocationName = dr["StorgeLocationName"].ToString(),
                                    Weight = dr["Weight"].ToString(),
                                    MoldKindName = dr["MoldKindName"].ToString(),
                                    RegularCheckCount = dr["RegularCheckCount"].ToString(),
                                };

                                if (WinMolding.ProdCompDate.Trim().Length > 0)
                                {
                                    WinMolding.ProdCompDate_CV = Lib.Instance.StrDateTimeBar(WinMolding.ProdCompDate);
                                    WinMolding.flagProdCompDate = true;
                                }
                                else
                                {
                                    WinMolding.flagProdCompDate = false;
                                }

                                if (WinMolding.ProdDueDate.Trim().Length > 0)
                                {
                                    WinMolding.ProdDueDate_CV = Lib.Instance.StrDateTimeBar(WinMolding.ProdDueDate);
                                    WinMolding.flagProdDueDate = true;
                                }
                                else
                                {
                                    WinMolding.flagProdDueDate = false;
                                }

                                if (WinMolding.ProdOrderDate.Trim().Length > 0)
                                {
                                    WinMolding.ProdOrderDate_CV = Lib.Instance.StrDateTimeBar(WinMolding.ProdOrderDate);
                                    WinMolding.flagProdOrderDate = true;
                                }
                                else
                                {
                                    WinMolding.flagProdOrderDate = false;
                                }

                                if (WinMolding.SetDate.Length > 0)
                                {
                                    if (WinMolding.SetDate.Replace(" ", "").Length == 6)
                                    {
                                        WinMolding.SetMD = "0";
                                        WinMolding.SetDate_CV = (WinMolding.SetDate.Substring(0, 4) + "-" + WinMolding.SetDate.Substring(4, 2));
                                    }
                                    else if (WinMolding.SetDate.Replace(" ", "").Length == 8)
                                    {
                                        WinMolding.SetMD = "1";
                                        WinMolding.SetDate_CV = Lib.Instance.StrDateTimeBar(WinMolding.SetDate);
                                    }
                                    else
                                    {
                                    }
                                }

                                if (WinMolding.EvalDate.Length > 0)
                                {
                                    WinMolding.EvalDate_CV = Lib.Instance.StrDateTimeBar(WinMolding.EvalDate);
                                }

                                if (WinMolding.SetInitHitCountDate.Trim().Length > 0)
                                {
                                    WinMolding.SetInitHitCountDate_CV = Lib.Instance.StrDateTimeBar(WinMolding.SetInitHitCountDate);
                                    WinMolding.flagSetInitHitCountDate = true;
                                }
                                else
                                {
                                    WinMolding.flagSetInitHitCountDate = false;
                                }

                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetCheckProdQty))
                                {
                                    WinMolding.SetCheckProdQty = Lib.Instance.returnNumStringZero(WinMolding.SetCheckProdQty);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.AfterRepairHitcount))
                                {
                                    WinMolding.AfterRepairHitcount = Lib.Instance.returnNumStringZero(WinMolding.AfterRepairHitcount);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.Hitcount))
                                {
                                    WinMolding.Hitcount = Lib.Instance.returnNumStringZero(WinMolding.Hitcount);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetProdQty))
                                {
                                    WinMolding.SetProdQty = Lib.Instance.returnNumStringZero(WinMolding.SetProdQty);
                                }

                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetWashingProdQty))
                                {
                                    WinMolding.SetWashingProdQty = Lib.Instance.returnNumStringZero(WinMolding.SetWashingProdQty);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetinitHitCount))
                                {
                                    WinMolding.SetinitHitCount = Lib.Instance.returnNumStringZero(WinMolding.SetinitHitCount);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.Weight))
                                {
                                    WinMolding.Weight = Lib.Instance.returnNumStringZero(WinMolding.Weight);
                                }

                                if (WinMolding.StorgeLocation.Trim().Equals(string.Empty))
                                {
                                    WinMolding.StorgeLocationName = "";
                                }
                                 
                                dgdMain.Items.Add(WinMolding);
                                i++;
                            }
                        }
                        else
                        {
                            foreach (DataRow dr in dt.Select(sql))
                            {
                                var WinMolding = new Win_dvl_Molding_U_CodeView()
                                {
                                    Num = i + 1,
                                    MoldNo = dr["MoldNo"].ToString(),
                                    Article = dr["Article"].ToString(),
                                    MoldID = dr["MoldID"].ToString(),
                                    Comments = dr["Comments"].ToString(),
                                    AfterRepairHitcount = dr["AfterRepairHitcount"].ToString(),
                                    AfterWashHitcount = dr["AfterWashHitcount"].ToString(),
                                    AttFile1 = dr["AttFile1"].ToString(),
                                    AttFile2 = dr["AttFile2"].ToString(),
                                    AttFile3 = dr["AttFile3"].ToString(),
                                    AttPath1 = dr["AttPath1"].ToString(),
                                    AttPath2 = dr["AttPath2"].ToString(),
                                    AttPath3 = dr["AttPath3"].ToString(),
                                    BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                    BuyerModelID = dr["BuyerModelID"].ToString(),
                                    BuyerModelName = dr["BuyerModelName"].ToString(),
                                    ProductionArticleID = dr["ProductionArticleID"].ToString(),
                                    Cavity = dr["Cavity"].ToString(),
                                    DisCardYN = dr["DisCardYN"].ToString(),
                                    dvlYN = dr["dvlYN"].ToString(),
                                    EvalDate = dr["EvalDate"].ToString(),
                                    EvalGrade = dr["EvalGrade"].ToString(),
                                    Evalscore = dr["Evalscore"].ToString(),
                                    Hitcount = dr["Hitcount"].ToString(),
                                    MoldKind = dr["MoldKind"].ToString(),
                                    MoldName = dr["MoldName"].ToString(),
                                    MoldPerson = dr["MoldPerson"].ToString(),
                                    MoldQuality = dr["MoldQuality"].ToString(),
                                    ProdCompDate = dr["ProdCompDate"].ToString(),
                                    ProdCustomName = dr["ProdCustomName"].ToString(),
                                    ProdDueDate = dr["ProdDueDate"].ToString(),
                                    ProdOrderDate = dr["ProdOrderDate"].ToString(),
                                    RealCavity = dr["RealCavity"].ToString(),
                                    SetCheckProdQty = dr["SetCheckProdQty"].ToString(),
                                    SetDate = dr["SetDate"].ToString(),
                                    SetinitHitCount = dr["SetinitHitCount"].ToString(),
                                    SetInitHitCountDate = dr["SetInitHitCountDate"].ToString(),
                                    SetProdQty = dr["SetProdQty"].ToString(),
                                    SetWashingProdQty = dr["SetWashingProdQty"].ToString(),
                                    Spec = dr["Spec"].ToString(),
                                    StorgeLocation = dr["StorgeLocation"].ToString(),
                                    StorgeLocationName = dr["StorgeLocationName"].ToString(),
                                    Weight = dr["Weight"].ToString(),
                                    MoldKindName = dr["MoldKindName"].ToString(),
                                    RegularCheckCount = dr["RegularCheckCount"].ToString(),
                                };

                                if (WinMolding.ProdCompDate.Trim().Length > 0)
                                {
                                    WinMolding.ProdCompDate_CV = Lib.Instance.StrDateTimeBar(WinMolding.ProdCompDate);
                                    WinMolding.flagProdCompDate = true;
                                }
                                else
                                {
                                    WinMolding.flagProdCompDate = false;
                                }

                                if (WinMolding.ProdDueDate.Trim().Length > 0)
                                {
                                    WinMolding.ProdDueDate_CV = Lib.Instance.StrDateTimeBar(WinMolding.ProdDueDate);
                                    WinMolding.flagProdDueDate = true;
                                }
                                else
                                {
                                    WinMolding.flagProdDueDate = false;
                                }

                                if (WinMolding.ProdOrderDate.Trim().Length > 0)
                                {
                                    WinMolding.ProdOrderDate_CV = Lib.Instance.StrDateTimeBar(WinMolding.ProdOrderDate);
                                    WinMolding.flagProdOrderDate = true;
                                }
                                else
                                {
                                    WinMolding.flagProdOrderDate = false;
                                }

                                if (WinMolding.SetDate.Length > 0)
                                {
                                    if (WinMolding.SetDate.Replace(" ", "").Length == 6)
                                    {
                                        WinMolding.SetMD = "0";
                                        WinMolding.SetDate_CV = (WinMolding.SetDate.Substring(0, 4) + "-" + WinMolding.SetDate.Substring(4, 2));
                                    }
                                    else if (WinMolding.SetDate.Replace(" ", "").Length == 8)
                                    {
                                        WinMolding.SetMD = "1";
                                        WinMolding.SetDate_CV = Lib.Instance.StrDateTimeBar(WinMolding.SetDate);
                                    }
                                    else
                                    {
                                    }
                                }

                                if (WinMolding.EvalDate.Length > 0)
                                {
                                    WinMolding.EvalDate_CV = Lib.Instance.StrDateTimeBar(WinMolding.EvalDate);
                                }

                                if (WinMolding.SetInitHitCountDate.Trim().Length > 0)
                                {
                                    WinMolding.SetInitHitCountDate_CV = Lib.Instance.StrDateTimeBar(WinMolding.SetInitHitCountDate);
                                    WinMolding.flagSetInitHitCountDate = true;
                                }
                                else
                                {
                                    WinMolding.flagSetInitHitCountDate = false;
                                }

                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetCheckProdQty))
                                {
                                    WinMolding.SetCheckProdQty = Lib.Instance.returnNumStringZero(WinMolding.SetCheckProdQty);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.AfterRepairHitcount))
                                {
                                    WinMolding.AfterRepairHitcount = Lib.Instance.returnNumStringZero(WinMolding.AfterRepairHitcount);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.Hitcount))
                                {
                                    WinMolding.Hitcount = Lib.Instance.returnNumStringZero(WinMolding.Hitcount);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetProdQty))
                                {
                                    WinMolding.SetProdQty = Lib.Instance.returnNumStringZero(WinMolding.SetProdQty);
                                }

                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetWashingProdQty))
                                {
                                    WinMolding.SetWashingProdQty = Lib.Instance.returnNumStringZero(WinMolding.SetWashingProdQty);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetinitHitCount))
                                {
                                    WinMolding.SetinitHitCount = Lib.Instance.returnNumStringZero(WinMolding.SetinitHitCount);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.Weight))
                                {
                                    WinMolding.Weight = Lib.Instance.returnNumStringZero(WinMolding.Weight);
                                }

                                if (WinMolding.StorgeLocation.Trim().Equals(string.Empty))
                                {
                                    WinMolding.StorgeLocationName = "";
                                }

                                dgdMain.Items.Add(WinMolding);
                                i++;
                            }
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

        //셀렉션item, selectedItem 시 이벤트
        private void dgdMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WinMold = dgdMain.SelectedItem as Win_dvl_Molding_U_CodeView;

            if (WinMold != null)
            {
                //this.DataContext = WinMold;
                FillGridPasts(WinMold.MoldID);
                ShowData(WinMold.MoldID); //오른쪽 데이터 보일놈들
            }
        }

        private void FillGridPasts(string strMoldID)
        {
            if (dgdPartsCode.Items.Count > 0)
            {
                dgdPartsCode.Items.Clear();
            }

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("MoldID", strMoldID);
                sqlParameter.Add("McPartID", "");
                sqlParameter.Add("ChangeCheckGbn", "");
                ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sMoldChangeProd", sqlParameter, false);

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
                            i++;
                            var WinMoldParts = new Win_dvl_Molding_U_Parts_CodeView()
                            {
                                MoldID = dr["MoldID"].ToString(),
                                Num = i,
                                //ChangeCheckGbn = dr["ChangeCheckGbn"].ToString(),
                                //CycleProdQty = dr["CycleProdQty"].ToString(),
                                McPartID = dr["McPartID"].ToString(),
                                MCPartName = dr["MCPartName"].ToString(),
                                //StartSetDate = dr["StartSetDate"].ToString(),
                                //StartSetProdQty = dr["StartSetProdQty"].ToString()
                            };

                            //if (Lib.Instance.IsNumOrAnother(WinMoldParts.CycleProdQty))
                            //{
                            //    WinMoldParts.CycleProdQty = Lib.Instance.returnNumStringZero(WinMoldParts.CycleProdQty);
                            //}
                            //if (Lib.Instance.IsNumOrAnother(WinMoldParts.StartSetProdQty))
                            //{
                            //    WinMoldParts.StartSetProdQty = Lib.Instance.returnNumStringZero(WinMoldParts.StartSetProdQty);
                            //}

                            dgdPartsCode.Items.Add(WinMoldParts);
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


        // 메인 그리드 행 선택시 오른쪽에 데이터 출력
        private void ShowData(string MoldID)
        {

            try
            {
                this.DataContext = null;

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("sMoldID", MoldID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Mold_sMoldONE", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        int i = 0;
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;
                            var MoldingSub = new Win_dvl_Molding_U_CodeView_One()
                            {
                                MoldID = dr["MoldID"].ToString(),
                                MoldNo = dr["MoldNo"].ToString(),
                                ProductionArticleID = dr["ProductionArticleID"].ToString(),
                                Article = dr["Article"].ToString(),
                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                

                                BuyerModelName = dr["BuyerModelName"].ToString(),
                                BuyerModelID = dr["BuyerModelID"].ToString(),
                                DvlYN = dr["DvlYN"].ToString(),
                                StorgeLocation = dr["StorgeLocation"].ToString(),
                                Cavity = dr["Cavity"].ToString(),

                                RealCavity = dr["RealCavity"].ToString(),
                                Weight = dr["Weight"].ToString(),
                                Comments = dr["Comments"].ToString(),
                                Spec = dr["Spec"].ToString(),
                                ProdCustomName = dr["ProdCustomName"].ToString(),

                                SetDate = dr["SetDate"].ToString(),
                                ProdOrderDate = dr["ProdOrderDate"].ToString(),
                                ProdDueDate = dr["ProdDueDate"].ToString(),
                                ProdCompDate = dr["ProdCompDate"].ToString(),
                                MoldPerson = dr["MoldPerson"].ToString(),


                                StorgeLocationName = dr["StorgeLocationName"].ToString(),
                                SetinitHitCount = dr["SetinitHitCount"].ToString(),
                                Hitcount = dr["Hitcount"].ToString(),
                                EvalGrade = dr["EvalGrade"].ToString(),

                                AttFile1 = dr["AttFile1"].ToString(),
                                AttFile2 = dr["AttFile2"].ToString(),
                                AttFile3 = dr["AttFile3"].ToString(),
                                AttPath1 = dr["AttPath1"].ToString(),
                                AttPath2 = dr["AttPath2"].ToString(),
                                AttPath3 = dr["AttPath3"].ToString(),
                                RegularCheckCount = dr["RegularCheckCount"].ToString(),

                            };


                            //MoldingSub.SetDate_CV = DatePickerFormat(MoldingSub.SetDate);
                            MoldingSub.ProdOrderDate_CV = DatePickerFormat(MoldingSub.ProdOrderDate);
                            MoldingSub.ProdDueDate_CV = DatePickerFormat(MoldingSub.ProdDueDate);
                            MoldingSub.ProdCompDate_CV = DatePickerFormat(MoldingSub.ProdCompDate);
                            if (MoldingSub.SetDate.Length > 0)
                            {
                                if (MoldingSub.SetDate.Replace(" ", "").Length == 6)
                                {
                                    MoldingSub.SetMD = "0";
                                    MoldingSub.SetDate_CV = (MoldingSub.SetDate.Substring(0, 4) + "-" + MoldingSub.SetDate.Substring(4, 2));
                                }
                                else if (MoldingSub.SetDate.Replace(" ", "").Length == 8)
                                {
                                    MoldingSub.SetMD = "1";
                                    MoldingSub.SetDate_CV = Lib.Instance.StrDateTimeBar(MoldingSub.SetDate);
                                }
                                else
                                {
                                }
                            }

                            txtArticle.Tag = MoldingSub.ProductionArticleID;

                            this.DataContext = MoldingSub;


                        }


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
        }


        /// <summary>
        /// 저장
        /// </summary>
        /// <param name="strFlag"></param>
        /// <param name="strYYYY"></param>
        /// <returns></returns>
        private bool SaveData(string strFlag, string strMoldID)
        {
            bool flag = false;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                if (CheckData())
                {
                    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Add("MoldID", strMoldID);
                    sqlParameter.Add("MoldNo", txtMoldNo.Text); //LotNo입니다..
                    sqlParameter.Add("CustomID", "");
                    sqlParameter.Add("BuyerModelID", txtBuyerModel.Tag != null ?  txtBuyerModel.Tag.ToString() : "");
                    sqlParameter.Add("MoldKind", "");
                    
                    sqlParameter.Add("MoldQuality", txtMoldQuality.Text);
                    sqlParameter.Add("Weight", txtWeight.Text != string.Empty ? txtWeight.Text.Replace(",","") : "0");
                    sqlParameter.Add("Spec", txtSpec.Text);
                    sqlParameter.Add("ProdCustomName", txtProdCustomName.Text);
                    sqlParameter.Add("ProdOrderDate", chkProdOrderDate.IsChecked==true ? 
                        dtpProdOrderDate.SelectedDate.Value.ToString("yyyyMMdd") : "");

                    sqlParameter.Add("ProdDueDate", chkProdDueDate.IsChecked == true ? 
                        dtpProdDueDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                    sqlParameter.Add("ProdCompDate", chkProdCompDate.IsChecked == true ? 
                        dtpProdCompDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                    sqlParameter.Add("Comments", txtComments.Text);
                    sqlParameter.Add("AttFile1", txtAttFile1.Text);
                    sqlParameter.Add("AttPath1", "");

                    sqlParameter.Add("AttFile2", txtAttFile2.Text);
                    sqlParameter.Add("AttFile3", txtAttFile3.Text);
                    sqlParameter.Add("AttPath2", "");
                    sqlParameter.Add("AttPath3", "");
                    sqlParameter.Add("dvlYN", cboDevYN.SelectedValue.ToString());

                    sqlParameter.Add("EvalScore", txtEvalscore.Text.Equals(string.Empty)!=true ? txtEvalscore.Text : "0");
                    sqlParameter.Add("EvalGrade", txtEvalGrade.Text);
                    sqlParameter.Add("EvalDate", dtpEvalDate.SelectedDate != null ?
                        dtpEvalDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                    sqlParameter.Add("SetDate", cboMD.SelectedValue.ToString() == "0" ? 
                        dtpSetDateM.SelectedDate.Value.ToString("yyyyMM") : dtpSetDateD.SelectedDate.Value.ToString("yyyyMMdd"));
                    sqlParameter.Add("StorgeLocation", cboStorgeLocation.SelectedValue.ToString());
                    
                    sqlParameter.Add("MoldPerson", txtMoldPerson.Text);
                    sqlParameter.Add("DisCardYN", "N");
                    sqlParameter.Add("Cavity", txtCavity.Text != string.Empty ? txtCavity.Text.Replace(",", "") : "0" );
                    sqlParameter.Add("RealCavity", txtRealCavity.Text != string.Empty ? txtRealCavity.Text.Replace(",", "") : "0");
                    sqlParameter.Add("SetProdQty", txtSetProdQty.Text != string.Empty ? txtSetProdQty.Text.Replace(",", "") : "0");

                    sqlParameter.Add("SetCheckProdQty", txtSetCheckProdQty.Text != string.Empty ? 
                        txtSetCheckProdQty.Text.Replace(",", "") : "0");
                    //sqlParameter.Add("SetWashingProdQty", txtSetWashingProdQty.Text != string.Empty ?
                    //    txtSetWashingProdQty.Text.Replace(",", "") : "0" );
                    sqlParameter.Add("SetWashingProdQty", 0);   // 그냥 항목을 빼기로 했다..=> 2019.02.13  최준호
                    sqlParameter.Add("nSetHitCount", txtSetinitHitCount.Text != string.Empty ?
                        txtSetinitHitCount.Text.Replace(",", "") : "0" );
                    sqlParameter.Add("sSetHitCountDate", chkSetInitHitCountDate.IsChecked == true ? 
                        dtpSetInitHitCountDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                    sqlParameter.Add("sProductionArticleID", txtArticle.Tag.ToString());

                    sqlParameter.Add("sRegularCheckCount", ConvertDouble(txtRegulerCk.Text)); // 정기점검시점
                    
                    
                    #region 추가

                    if (strFlag.Equals("I"))
                    {
                        sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_dvlMold_iMold";
                        pro1.OutputUseYN = "Y";
                        pro1.OutputName = "MoldID";
                        pro1.OutputLength = "5";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        for (int i = 0; i < dgdPartsCode.Items.Count; i++)
                        {
                            WinMoldParts = dgdPartsCode.Items[i] as Win_dvl_Molding_U_Parts_CodeView;
                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldID", strMoldID);
                            sqlParameter.Add("McPartID", WinMoldParts.McPartID);
                            sqlParameter.Add("ChangeCheckGbn", 1);
                            sqlParameter.Add("CycleProdQty", 0);
                            sqlParameter.Add("StartSetProdQty", 0);
                            sqlParameter.Add("StartSetDate", DateTime.Today.ToString("yyyyMMdd"));
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro2 = new Procedure();
                            pro2.Name = "xp_dvlMold_iMoldChangeProd";
                            pro2.OutputUseYN = "N";
                            pro2.OutputName = "MoldID";
                            pro2.OutputLength = "5";

                            Prolist.Add(pro2);
                            ListParameter.Add(sqlParameter);
                        }

                        List<KeyValue> list_Result = new List<KeyValue>();
                        list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS_NewLog(Prolist, ListParameter,"C");
                        string sGetID = string.Empty;

                        if (list_Result[0].key.ToLower() == "success")
                        {
                            list_Result.RemoveAt(0);
                            for (int i = 0; i < list_Result.Count; i++)
                            {
                                KeyValue kv = list_Result[i];
                                if (kv.key == "MoldID")
                                {
                                    sGetID = kv.value;
                                    flag = true;
                                }
                            }

                            if (flag)
                            {
                                bool AttachYesNo = false;
                                if (txtAttFile1.Text != string.Empty)       //첨부파일 1
                                {
                                    AttachYesNo = true;
                                    FTP_Save_File(sGetID, txtAttFile1.Text, FullPath1);
                                }
                                if (txtAttFile2.Text != string.Empty)       //첨부파일 2
                                {
                                    AttachYesNo = true;
                                    FTP_Save_File(sGetID, txtAttFile2.Text, FullPath2);
                                }
                                if (txtAttFile3.Text != string.Empty)       //첨부파일 3
                                {
                                    AttachYesNo = true;
                                    FTP_Save_File(sGetID, txtAttFile3.Text, FullPath3);
                                }
                                if (AttachYesNo == true) { AttachFileUpdate(sGetID); }
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
                        sqlParameter.Add("UserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_dvlMold_uMold";
                        pro1.OutputUseYN = "N";
                        pro1.OutputName = "MoldID";
                        pro1.OutputLength = "5";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        for (int i = 0; i < dgdPartsCode.Items.Count; i++)
                        {
                            WinMoldParts = dgdPartsCode.Items[i] as Win_dvl_Molding_U_Parts_CodeView;
                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldID", strMoldID);
                            sqlParameter.Add("McPartID", WinMoldParts.McPartID);
                            sqlParameter.Add("ChangeCheckGbn", 1);
                            sqlParameter.Add("CycleProdQty", 0);
                            sqlParameter.Add("StartSetProdQty", 0);
                            sqlParameter.Add("StartSetDate", DateTime.Today.ToString("yyyyMMdd"));
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro2 = new Procedure();
                            pro2.Name = "xp_dvlMold_iMoldChangeProd";
                            pro2.OutputUseYN = "N";
                            pro2.OutputName = "MoldID";
                            pro2.OutputLength = "5";

                            Prolist.Add(pro2);
                            ListParameter.Add(sqlParameter);
                        }

                        string[] Confirm = new string[2];
                        Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew_NewLog(Prolist, ListParameter,"U");
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

                        if (flag)
                        {
                            bool AttachYesNo = false;
                            if (txtAttFile1.Text != string.Empty && ftpDelete1)       //첨부파일 1
                            {
                                AttachYesNo = true;
                                FTP_Save_File(strMoldID, txtAttFile1.Text, FullPath1);
                            }
                            if (txtAttFile2.Text != string.Empty && ftpDelete2)       //첨부파일 2
                            {
                                AttachYesNo = true;
                                FTP_Save_File(strMoldID, txtAttFile2.Text, FullPath2);
                            }
                            if (txtAttFile3.Text != string.Empty && ftpDelete3)       //첨부파일 3
                            {
                                AttachYesNo = true;
                                FTP_Save_File(strMoldID, txtAttFile3.Text, FullPath3);
                            }
                            if (AttachYesNo == true) { AttachFileUpdate(strMoldID); }
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

        /// <summary>
        /// 실삭제
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>
        private bool DeleteData(string strID)
        {
            bool flag = false;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("MoldID", strID);

                string[] result = DataStore.Instance.ExecuteProcedure_NewLog("xp_dvlMold_dMold", sqlParameter, "D");

                if (result[0].Equals("success"))
                {
                    //MessageBox.Show("성공 *^^*");
                    flag = true;
                }
                else
                {
                    MessageBox.Show("삭제 실패, 실패 이유 : "+result[1]);
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

        /// <summary>
        /// 입력사항 체크
        /// </summary>
        /// <returns></returns>
        private bool CheckData()
        {
            bool flag = true;

            //if (txtMoldKind.Text.Length <= 0 || txtMoldKind.Text.Equals(""))
            //{
            //    MessageBox.Show("설비종류가 입력되지 않았습니다.");
            //    flag = false;
            //    return flag;
            //}

            if (txtArticle.Text.Length <= 0 || txtArticle.Text.Equals(""))
            {
                MessageBox.Show("품명이 입력되지 않았습니다.");
                flag = false;
                return flag;
            }

            if (txtMoldNo.Text.Length <= 0 || txtMoldNo.Text.Equals(""))
            {
                MessageBox.Show("금형LotNo이 입력되지 않았습니다.");
                flag = false;
                return flag;
            }

            //if (txtBuyerModel.Text.Length <= 0 || txtBuyerModel.Text.Equals(""))
            //{
            //    MessageBox.Show("차종이 입력되지 않았습니다.");
            //    flag = false;
            //    return flag;
            //}

            ////if (txtArticle.Text.Length <= 0 || txtArticle.Text.Equals(""))
            ////{
            ////    MessageBox.Show("품명이 입력되지 않았습니다.");
            ////    flag = false;
            ////    return flag;
            ////}

            if (cboDevYN.SelectedValue == null)
            {
                MessageBox.Show("개발/양산구분이 선택되지 않았습니다.");
                flag = false;
                return flag;
            }

            if (cboStorgeLocation.SelectedValue == null)
            {
                MessageBox.Show("보관위치가 선택되지 않았습니다.");
                flag = false;
                return flag;
            }

            return flag;
        }

        private void btnSubAdd_Click(object sender, RoutedEventArgs e)
        {
            Win_dvl_Molding_U_Parts_CodeView PartsMold = new Win_dvl_Molding_U_Parts_CodeView()
            {
                Num = dgdPartsCode.Items.Count+1,
                McPartID= "",
                MCPartName="",
                MoldID=""
            };

            dgdPartsCode.Items.Add(PartsMold);
        }

        private void btnSubDel_Click(object sender, RoutedEventArgs e)
        {
            WinMoldParts = dgdPartsCode.CurrentItem as Win_dvl_Molding_U_Parts_CodeView;

            if (WinMoldParts != null)
            {
                dgdPartsCode.Items.Remove(WinMoldParts);
            }
            else
            {
                if (dgdPartsCode.Items.Count > 0)
                {
                    dgdPartsCode.Items.RemoveAt(dgdPartsCode.Items.Count - 1);
                }
            }
        }

        //
        private void DataGridCell_KeyDown(object sender, KeyEventArgs e)
        {
            WinMoldParts = dgdPartsCode.CurrentItem as Win_dvl_Molding_U_Parts_CodeView;
            int rowCount = dgdPartsCode.Items.IndexOf(dgdPartsCode.CurrentItem);
            int colCountOne = dgdPartsCode.Columns.IndexOf(dgdtpePartsName);
            int colCountTwo = dgdPartsCode.Columns.IndexOf(dgdtpePartsCode);
            int colCount = dgdPartsCode.Columns.IndexOf(dgdPartsCode.CurrentCell.Column);

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (dgdPartsCode.Items.Count - 1 > rowCount && colCount == colCountTwo)
                {
                    dgdPartsCode.SelectedIndex = rowCount + 1;
                    dgdPartsCode.CurrentCell = 
                        new DataGridCellInfo(dgdPartsCode.Items[rowCount + 1], dgdPartsCode.Columns[colCountOne]);
                }
                else if (dgdPartsCode.Items.Count - 1 >= rowCount && colCount == colCountOne)
                {
                    dgdPartsCode.CurrentCell = 
                        new DataGridCellInfo(dgdPartsCode.Items[rowCount], dgdPartsCode.Columns[colCountTwo]);
                }
                else if (dgdPartsCode.Items.Count - 1 == rowCount && colCount == colCountTwo)
                {
                    if (MessageBox.Show("부품을 추가하시겠습니까?", "추가 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        btnSubAdd_Click(null, null);
                        dgdPartsCode.SelectedIndex = rowCount + 1;
                        dgdPartsCode.CurrentCell =
                            new DataGridCellInfo(dgdPartsCode.Items[rowCount + 1], dgdPartsCode.Columns[colCountOne]);
                    }
                    else
                    {
                        btnSave.Focus();
                    }
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
            if (lblMsg.Visibility == Visibility.Visible)
            {
                DataGridCell cell = sender as DataGridCell;
                cell.IsEditing = true;
            }
        }

        private void dgdtxtMCPartName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinMoldParts = dgdPartsCode.CurrentItem as Win_dvl_Molding_U_Parts_CodeView;

                if (WinMoldParts != null)
                {
                    TextBox tb1 = sender as TextBox;

                    if (tb1 != null)
                    {
                        WinMoldParts.MCPartName = tb1.Text;
                    }

                    sender = tb1;
                }
            }
        }

        private void dgdtxtMCPartName_KeyDown(object sender, KeyEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinMoldParts = dgdPartsCode.CurrentItem as Win_dvl_Molding_U_Parts_CodeView;

                if (WinMoldParts != null)
                {
                    TextBox tb1 = sender as TextBox;
                    MainWindow.pf.ReturnCode(tb1, (int)Defind_CodeFind.DCF_PART, "");

                    if (tb1 != null)
                    {
                        WinMoldParts.McPartID = tb1.Tag.ToString();
                        WinMoldParts.MCPartName = tb1.Text;
                    }

                    sender = tb1;
                }
            }
        }

        private void dgdtxtMCPartID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinMoldParts = dgdPartsCode.CurrentItem as Win_dvl_Molding_U_Parts_CodeView;

                if (WinMoldParts != null)
                {
                    TextBox tb1 = sender as TextBox;

                    if (tb1 != null)
                    {
                        WinMoldParts.McPartID = tb1.Text;
                    }

                    sender = tb1;
                }
            }
        }

        private void dgdtxtMCPartID_KeyDown(object sender, KeyEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinMoldParts = dgdPartsCode.CurrentItem as Win_dvl_Molding_U_Parts_CodeView;

                if (WinMoldParts != null)
                {
                    TextBox tb1 = sender as TextBox;
                    MainWindow.pf.ReturnCode(tb1, (int)Defind_CodeFind.DCF_PART, "");

                    if (tb1 != null)
                    {
                        WinMoldParts.McPartID = tb1.Tag.ToString();
                        WinMoldParts.MCPartName = tb1.Text;
                    }

                    sender = tb1;
                }
            }
        }

        private void chkProdOrderDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpProdOrderDate.IsEnabled = true;
        }

        private void chkProdOrderDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpProdOrderDate.IsEnabled = false;
        }

        private void chkProdDueDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpProdDueDate.IsEnabled = true;
        }

        private void chkProdDueDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpProdDueDate.IsEnabled = false;
        }

        private void chkProdCompDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpProdCompDate.IsEnabled = true;
        }

        private void chkProdCompDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpProdCompDate.IsEnabled = false;
        }

        // 파일 저장하기.
        private void FTP_Save_File(string Defect_ID, string FileName, string FullPath)
        {
            UploadFileInfo fileInfo_up = new UploadFileInfo();
            fileInfo_up.Filename = FileName;
            fileInfo_up.Type = FtpFileType.File;

            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

            string[] fileListSimple;
            string[] fileListDetail;

            fileListSimple = _ftp.directoryListSimple("", Encoding.Default);
            fileListDetail = _ftp.directoryListDetailed("", Encoding.Default);

            // 기존 폴더 확인작업.
            bool MakeFolder = false;
            for (int i = 0; i < fileListSimple.Length; i++)
            {
                if (fileListSimple[i] == Defect_ID)
                {
                    MakeFolder = true;
                    break;
                }
            }
            if (MakeFolder == false)        // 같은 아이를 찾지 못한경우,
            {
                //MIL 폴더에 InspectionID로 저장
                if (_ftp.createDirectory(Defect_ID) == false)
                {
                    MessageBox.Show("업로드를 위한 폴더를 생성할 수 없습니다.");
                    return;
                }
            }
            // 폴더 생성 후 생성한 폴더에 파일을 업로드
            string str_remotepath = Defect_ID + "/";
            fileInfo_up.Filepath = str_remotepath;
            str_remotepath += FileName;
            if (_ftp.upload(str_remotepath, FullPath) == false)
            {
                MessageBox.Show("파일업로드에 실패하였습니다.");
                return;
            }

            //if (FullPath == FullPath1) { txtAttPath1.Text = "/ImageData/Draw/" + str_remotepath; }
            //if (FullPath == FullPath2) { txtAttPath2.Text = "/ImageData/Draw/" + str_remotepath; }
            //if (FullPath == FullPath3) { txtAttPath3.Text = "/ImageData/Draw/" + str_remotepath; }

            if (FullPath == FullPath1) { txtAttFile1.Tag = "/ImageData/Mold/" + fileInfo_up.Filepath; }
            if (FullPath == FullPath2) { txtAttFile2.Tag = "/ImageData/Mold/" + fileInfo_up.Filepath; }
            if (FullPath == FullPath3) { txtAttFile3.Tag = "/ImageData/Mold/" + fileInfo_up.Filepath; }
        }

        //파일 삭제(FTP상에서)_폴더 삭제는 X
        private void FTP_UploadFile_File_Delete(string strSaveName, string FileName)
        {
            //if (!_ftp.delete(strSaveName + "/" + FileName))
            //{
            //    MessageBox.Show("파일이 삭제되지 않았습니다.");
            //}
            if (_ftp.DeleteFileOnFtpServer(new Uri(FTP_ADDRESS+"/"+strSaveName + "/" + FileName)) == true)
            {
            }
            else
            {
                MessageBox.Show("파일이 삭제되지 않았습니다."); 
            }
        }

        /// <summary>
        /// FTP 업로드 폴더 삭제(안의 파일을 삭제해야 삭제가 된다.)
        /// </summary>
        /// <param name="strSaveName"></param>
        /// <param name="FileName"></param>
        /// <returns></returns>
        private bool FTP_UploadFile_Path_Delete(string strSaveName)
        {
            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

            string[] fileListSimple;
            string[] fileListDetail;

            fileListSimple = _ftp.directoryListSimple("", Encoding.Default);
            fileListDetail = _ftp.directoryListDetailed("", Encoding.Default);

            bool tf_ExistInspectionID = MakeFileInfoList(fileListSimple, fileListDetail, strSaveName);

            if (tf_ExistInspectionID == true)
            {
                if (_ftp.removeDir(strSaveName) == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                //if (_ftp.DeleteFileOnFtpServer(new Uri(strSaveName)) == true)
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }
            return true;
        }

        private void btnInsPic_Click(object sender, RoutedEventArgs e)
        {
            // (버튼)sender 마다 tag를 달자.
            string ClickPoint = ((Button)sender).Tag.ToString();

            Microsoft.Win32.OpenFileDialog OFdlg = new Microsoft.Win32.OpenFileDialog();

            OFdlg.DefaultExt = ".jpg";
            OFdlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png | All Files|*.*";

            Nullable<bool> result = OFdlg.ShowDialog();
            if (result == true)
            {
                if (ClickPoint == "1") { FullPath1 = OFdlg.FileName; }  //긴 경로(FULL 사이즈)
                if (ClickPoint == "2") { FullPath2 = OFdlg.FileName; }
                if (ClickPoint == "3") { FullPath3 = OFdlg.FileName; }

                string AttachFileName = OFdlg.SafeFileName;  //명.
                string AttachFilePath = string.Empty;       // 경로

                if (ClickPoint == "1") { AttachFilePath = FullPath1.Replace(AttachFileName, ""); }
                if (ClickPoint == "2") { AttachFilePath = FullPath2.Replace(AttachFileName, ""); }
                if (ClickPoint == "3") { AttachFilePath = FullPath3.Replace(AttachFileName, ""); }

                StreamReader sr = new StreamReader(OFdlg.FileName);
                long File_size = sr.BaseStream.Length;
                if (sr.BaseStream.Length > (2048 * 1000))
                {
                    // 업로드 파일 사이즈범위 초과
                    MessageBox.Show("이미지의 파일사이즈가 2M byte를 초과하였습니다.");
                    sr.Close();
                    return;
                }
                if (ClickPoint == "1")
                {
                    txtAttFile1.Text = AttachFileName;
                    txtAttFile1.Tag = AttachFilePath.ToString();
                }
                else if (ClickPoint == "2")
                {
                    txtAttFile2.Text = AttachFileName;
                    txtAttFile2.Tag = AttachFilePath.ToString();
                }
                else if (ClickPoint == "3")
                {
                    txtAttFile3.Text = AttachFileName;
                    txtAttFile3.Tag = AttachFilePath.ToString();
                }
            }
        }

        private void btnDelPic_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgresult = MessageBox.Show("파일을 삭제 하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo);
            if (msgresult == MessageBoxResult.Yes)
            {
                string ClickPoint = ((Button)sender).Tag.ToString();

                if ((ClickPoint == "1") && (txtAttFile1.Tag.ToString() != string.Empty))
                {
                    if (strFlag.Equals("U"))
                    {
                        if (DetectFtpFile(txtMoldID.Text) && !txtAttFile1.Text.Equals(string.Empty))
                        {
                            FTP_UploadFile_File_Delete(txtMoldID.Text, txtAttFile1.Text);
                        }
                        ftpDelete1 = true;
                    }

                    txtAttFile1.Text = string.Empty;
                    txtAttFile1.Tag = string.Empty;
                    
                }
                if ((ClickPoint == "2") && (txtAttFile2.Tag.ToString() != string.Empty))
                {
                    if (strFlag.Equals("U"))
                    {
                        if (DetectFtpFile(txtMoldID.Text) && !txtAttFile2.Text.Equals(string.Empty))
                        {
                            FTP_UploadFile_File_Delete(txtMoldID.Text, txtAttFile2.Text);
                        }
                        ftpDelete2 = true;
                    }

                    txtAttFile2.Text = string.Empty;
                    txtAttFile2.Tag = string.Empty;                    
                }
                if ((ClickPoint == "3") && (txtAttFile3.Tag.ToString() != string.Empty))
                {
                    if (strFlag.Equals("U"))
                    {
                        if (DetectFtpFile(txtMoldID.Text) && !txtAttFile3.Text.Equals(string.Empty))
                        {
                            FTP_UploadFile_File_Delete(txtMoldID.Text, txtAttFile3.Text);
                        }
                        ftpDelete3 = true;
                    }

                    txtAttFile3.Text = string.Empty;
                    txtAttFile3.Tag = string.Empty;
                }
            }
        }

        private void btnPreView_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgresult = MessageBox.Show("파일을 보시겠습니까?", "보기 확인", MessageBoxButton.YesNo);
            if (msgresult == MessageBoxResult.Yes)
            {
                //버튼 태그값.
                string ClickPoint = ((Button)sender).Tag.ToString();

                if ((ClickPoint == "1") && (txtAttFile1.Tag.ToString() == string.Empty))
                {
                    MessageBox.Show("파일이 없습니다.");
                    return;
                }
                if ((ClickPoint == "2") && (txtAttFile2.Tag.ToString() == string.Empty))
                {
                    MessageBox.Show("파일이 없습니다.");
                    return;
                }
                if ((ClickPoint == "3") && (txtAttFile3.Tag.ToString() == string.Empty))
                {
                    MessageBox.Show("파일이 없습니다.");
                    return;
                }

                // 접속 경로
                _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

                string[] fileListSimple;
                string[] fileListDetail;

                fileListSimple = _ftp.directoryListSimple("", Encoding.Default);
                fileListDetail = _ftp.directoryListDetailed("", Encoding.Default);

                bool ExistFile = false;

                if (ClickPoint == "1")
                {
                    // 경로에 '\\'가 잘못 들어간 경우 오류가 나 멈춤, 이를 방지하기 위한 조건 추가
                    if (txtAttFile1.Tag.ToString().Contains("\\"))
                    {
                        ExistFile = MakeFileInfoList(fileListSimple, fileListDetail, txtAttFile1.Tag.ToString().Split('\\')[3].Trim());
                    }
                    else
                    {
                        ExistFile = MakeFileInfoList(fileListSimple, fileListDetail, txtAttFile1.Tag.ToString().Split('/')[3].Trim());
                    }

                }  //(폴더경로 찾기.)
                if (ClickPoint == "2")
                {
                    if (txtAttFile2.Tag.ToString().Contains("\\"))
                    {
                        ExistFile = MakeFileInfoList(fileListSimple, fileListDetail, txtAttFile2.Tag.ToString().Split('\\')[3].Trim());
                    }
                    else
                    {
                        ExistFile = MakeFileInfoList(fileListSimple, fileListDetail, txtAttFile2.Tag.ToString().Split('/')[3].Trim());
                    }
                }
                if (ClickPoint == "3")
                {
                    if (txtAttFile3.Tag.ToString().Contains("\\"))
                    {
                        ExistFile = MakeFileInfoList(fileListSimple, fileListDetail, txtAttFile3.Tag.ToString().Split('\\')[3].Trim());
                    }
                    else
                    {
                        ExistFile = MakeFileInfoList(fileListSimple, fileListDetail, txtAttFile3.Tag.ToString().Split('/')[3].Trim());
                    }
                }

                int totalCount = _listFileInfo.Count;

                if (ExistFile == true)
                {
                    ExistFile = false;
                    // 접속 경로
                    string str_path = string.Empty;
                    if (ClickPoint == "1") { str_path = FTP_ADDRESS + '/' + txtAttFile1.Tag.ToString().Split('/')[3].Trim(); }
                    if (ClickPoint == "2") { str_path = FTP_ADDRESS + '/' + txtAttFile2.Tag.ToString().Split('/')[3].Trim(); }
                    if (ClickPoint == "3") { str_path = FTP_ADDRESS + '/' + txtAttFile3.Tag.ToString().Split('/')[3].Trim(); }

                    _ftp = new FTP_EX(str_path, FTP_ID, FTP_PASS);

                    if (ClickPoint == "1") { ExistFile = MakeFileInfoList(fileListSimple, fileListDetail, txtAttFile1.Tag.ToString().Split('/')[3].Trim()); }
                    if (ClickPoint == "2") { ExistFile = MakeFileInfoList(fileListSimple, fileListDetail, txtAttFile2.Tag.ToString().Split('/')[3].Trim()); }
                    if (ClickPoint == "3") { ExistFile = MakeFileInfoList(fileListSimple, fileListDetail, txtAttFile3.Tag.ToString().Split('/')[3].Trim()); }

                    totalCount = _listFileInfo.Count;

                    if (ExistFile == true)
                    {
                        string str_remotepath = string.Empty;
                        string str_localpath = string.Empty;

                        if (ClickPoint == "1") { str_remotepath = txtAttFile1.Text.ToString(); }
                        if (ClickPoint == "1") { str_localpath = LOCAL_DOWN_PATH + "\\" + txtAttFile1.Text.ToString(); }
                        if (ClickPoint == "2") { str_remotepath = txtAttFile2.Text.ToString(); }
                        if (ClickPoint == "2") { str_localpath = LOCAL_DOWN_PATH + "\\" + txtAttFile2.Text.ToString(); }
                        if (ClickPoint == "3") { str_remotepath = txtAttFile3.Text.ToString(); }
                        if (ClickPoint == "3") { str_localpath = LOCAL_DOWN_PATH + "\\" + txtAttFile3.Text.ToString(); }

                        DirectoryInfo DI = new DirectoryInfo(LOCAL_DOWN_PATH);      // Temp 폴더가 없는 컴터라면, 만들어 줘야지.
                        if (DI.Exists == false)
                        {
                            DI.Create();
                        }

                        FileInfo file = new FileInfo(str_localpath);
                        if (file.Exists)
                        {
                            //if (MessageBox.Show("같은 이름의 파일이 존재하여" +
                            //    "진행합니다. 계속 하시겠습니까?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            //{
                            //    file.Delete();
                            //}
                            //else
                            //{
                            //    MessageBox.Show("C:Temp 폴더를 확인하세요.");
                            //    return;
                            //}

                            file.Delete();
                            MessageBox.Show("C:Temp 폴더를 확인하세요.");
                            return;
                        }

                        _ftp.download(str_remotepath, str_localpath);
                        //MessageBox.Show("C:Temp 폴더를 확인하세요.");

                        ProcessStartInfo proc = new ProcessStartInfo(str_localpath);
                        proc.UseShellExecute = true;
                        Process.Start(proc);
                    }
                }
                else
                {
                    MessageBox.Show("파일을 찾을 수 없습니다.");
                }
            }
        }

        private bool MakeFileInfoList(string[] simple, string[] detail, string str_ID)
        {
            bool tf_return = false;
            foreach (string filename in simple)
            {
                foreach (string info in detail)
                {
                    if (info.Contains(filename) == true)
                    {

                        if (MakeFileInfoList(filename, info, str_ID) == true)
                        {
                            tf_return = true;
                        }
                    }
                }
            }
            return tf_return;
        }

        private bool MakeFileInfoList(string simple, string detail, string strCompare)
        {
            UploadFileInfo info = new UploadFileInfo();
            info.Filename = simple;
            info.Filepath = detail;

            if (simple.Length > 0)
            {
                string[] tokens = detail.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                string name = tokens[3].ToString();         // 2017.03.16  허윤구.  토근 배열이 8자리로 되어 있었는데 에러가 나길래 확인해 보니 4자리 배열로 나오길래 바꾸었습니다.
                string permissions = tokens[2].ToString();      // premission도 배열 0번이 아니라 배열 2번인데...;;


                if (permissions.Contains("D") == true)          // 대문자 D로 표시해야 합니다.
                {
                    info.Type = FtpFileType.DIR;
                }
                else
                {
                    info.Type = FtpFileType.File;
                }

                if (info.Type == FtpFileType.File)
                {
                    info.Size = Convert.ToInt64(detail.Substring(17, detail.LastIndexOf(simple) - 17).Trim());      // 사이즈가 중요한가?
                }

                _listFileInfo.Add(info);

                if (string.Compare(simple, strCompare, false) == 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 삭제할 파일이 존재하는지 확인, strSaveName = FullPath(파일이름 포함)
        /// </summary>
        /// <param name="strSaveName"></param>
        /// <returns></returns>
        private bool DetectFtpFile(string strSaveName)
        {
            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

            string[] fileListSimple;
            string[] fileListDetail;

            fileListSimple = _ftp.directoryListSimple("", Encoding.Default);
            fileListDetail = _ftp.directoryListDetailed("", Encoding.Default);

            bool tf_ExistInspectionID = MakeFileInfoList(fileListSimple, fileListDetail, strSaveName);

            return tf_ExistInspectionID;
        }

        // 1) 첨부문서가 있을경우, 2) FTP에 정상적으로 업로드가 완료된 경우.  >> DB에 정보 업데이트 
        private void AttachFileUpdate(string ID)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("MoldID", ID);

                sqlParameter.Add("AttPath1", !txtAttFile1.Text.Trim().Equals("") ? txtAttFile1.Tag.ToString() : "");
                //sqlParameter.Add("AttPath1", txtAttFile1.Text.Equals(string.Empty) ? "" : txtAttFile1.Tag.ToString());
                sqlParameter.Add("AttFile1", txtAttFile1.Text);
                sqlParameter.Add("AttPath2", txtAttFile2.Text.Equals(string.Empty) ? "" : txtAttFile2.Tag.ToString());
                sqlParameter.Add("AttFile2", txtAttFile2.Text);
                sqlParameter.Add("AttPath3", txtAttFile3.Text.Equals(string.Empty) ? "" : txtAttFile3.Tag.ToString());
                sqlParameter.Add("AttFile3", txtAttFile3.Text);

                string[] result = DataStore.Instance.ExecuteProcedure("xp_dvlMold_uMolde_Ftp", sqlParameter, true);
                if (!result[0].Equals("success"))
                {
                    MessageBox.Show("이상발생, 관리자에게 문의하세요");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //private void txtMoldKind_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        MainWindow.pf.ReturnCode(txtMoldKind, 12, "");
        //    }
        //}

        //private void btnPfMoldKind_Click(object sender, RoutedEventArgs e)
        //{
        //    MainWindow.pf.ReturnCode(txtMoldKind, 12, "");
        //}

        private void txtArticle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticle, 68, "");
                SetBuyerArticleNo(txtArticle.Tag);
            }
        }

        private void btnPfArticle_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticle, 68, "");
            SetBuyerArticleNo(txtArticle.Tag);
        }

        //buyerArticleNo 세팅..
        private void SetBuyerArticleNo(object obj)
        {
            try
            {
                string strArticleID = string.Empty;

                if (obj != null)
                {
                    string sql = "select ma.BuyerArticleNo from mt_Article ma ";
                    sql += "where ma.ArticleID = '"+ obj.ToString() + "'   ";

                    DataSet ds = DataStore.Instance.QueryToDataSet(sql);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        DataTable dt = ds.Tables[0];
                        if (dt.Rows.Count > 0)
                        {
                            txtBuyerArticleNo.Text = dt.Rows[0].ItemArray[0].ToString();
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

        private void txtBuyerModel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtBuyerModel, (int)Defind_CodeFind.DCF_BUYERMODEL, "");
            }
        }

        private void btnPfBuyerModel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtBuyerModel, (int)Defind_CodeFind.DCF_BUYERMODEL, "");
        }

        private void lblSetInitHitCountDate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkSetInitHitCountDate.IsChecked == true) { chkSetInitHitCountDate.IsChecked = false; }
            else { chkSetInitHitCountDate.IsChecked = true; }
        }

        private void lblProdOrderDate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkProdOrderDate.IsChecked == true) { chkProdOrderDate.IsChecked = false; }
            else { chkProdOrderDate.IsChecked = true; }
        }

        private void lblProdDueDate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkProdDueDate.IsChecked == true) { chkProdDueDate.IsChecked = false; }
            else { chkProdDueDate.IsChecked = true; }
        }

        private void lblProdCompDate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkProdCompDate.IsChecked == true) { chkProdCompDate.IsChecked = false; }
            else { chkProdCompDate.IsChecked = true; }
        }

        private void chkSetInitHitCountDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpSetInitHitCountDate.IsEnabled = true;
        }

        private void chkSetInitHitCountDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSetInitHitCountDate.IsEnabled = false;
        }

        private void cboMD_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboMD.SelectedIndex == 0)
            {
                dtpSetDateM.Visibility = Visibility.Visible;
                dtpSetDateD.Visibility = Visibility.Hidden;
            }
            else
            {
                dtpSetDateM.Visibility = Visibility.Hidden;
                dtpSetDateD.Visibility = Visibility.Visible;
            }
        }

        private void BtnMultiArticle_Click(object sender, RoutedEventArgs e)
        {
            MultiArticle = true;

            if (popMultiArticle.IsOpen == false)
                popMultiArticle.IsOpen = true;
        }

        private void TxtArticleSrh1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh1, 68, "");
            }
        }

        private void TxtArticleSrh2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh2, 68, "");
            }
        }

        private void TxtArticleSrh3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh3, 68, "");
            }
        }

        private void TxtArticleSrh4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh4, 68, "");
            }
        }

        private void TxtArticleSrh5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh5, 68, "");
            }
        }

        private void BtnPfArticleSrh1_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh1, 68, "");
        }

        private void BtnPfArticleSrh2_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh2, 68, "");
        }

        private void BtnPfArticleSrh3_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh3, 68, "");
        }

        private void BtnPfArticleSrh4_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh4, 68, "");
        }

        private void BtnPfArticleSrh5_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh5, 68, "");
        }

        private void BtnMultiArticleOK_Click(object sender, RoutedEventArgs e)
        {
            if (txtArticleSrh1.Tag != null && !txtArticleSrh1.Text.Equals(string.Empty))
                ArticleSrh1 = txtArticleSrh1.Tag.ToString().Trim();
            else
                ArticleSrh1 = string.Empty;

            if (txtArticleSrh2.Tag != null && !txtArticleSrh2.Text.Equals(string.Empty))
                ArticleSrh2 = txtArticleSrh2.Tag.ToString().Trim();
            else
                ArticleSrh2 = string.Empty;

            if (txtArticleSrh3.Tag != null && !txtArticleSrh3.Text.Equals(string.Empty))
                ArticleSrh3 = txtArticleSrh3.Tag.ToString().Trim();
            else
                ArticleSrh3 = string.Empty;

            if (txtArticleSrh4.Tag != null && !txtArticleSrh4.Text.Equals(string.Empty))
                ArticleSrh4 = txtArticleSrh4.Tag.ToString().Trim();
            else
                ArticleSrh4 = string.Empty;

            if (txtArticleSrh5.Tag != null && !txtArticleSrh5.Text.Equals(string.Empty))
                ArticleSrh5 = txtArticleSrh5.Tag.ToString().Trim();
            else
                ArticleSrh5 = string.Empty;

            if (popMultiArticle.IsOpen == true)
                popMultiArticle.IsOpen = false;
        }

        private void BtnMultiArticleCC_Click(object sender, RoutedEventArgs e)
        {
            if ((txtArticleSrh1.Tag != null && !txtArticleSrh1.Text.Equals(string.Empty))||
                (txtArticleSrh2.Tag != null && !txtArticleSrh2.Text.Equals(string.Empty))||
                (txtArticleSrh3.Tag != null && !txtArticleSrh3.Text.Equals(string.Empty))||
                (txtArticleSrh4.Tag != null && !txtArticleSrh4.Text.Equals(string.Empty))||
                (txtArticleSrh5.Tag != null && !txtArticleSrh5.Text.Equals(string.Empty)))
                MultiArticle = true;
            else
                MultiArticle = false;

            if (popMultiArticle.IsOpen == true)
                popMultiArticle.IsOpen = false;
        }

        private void BtnMultiArticleClear_Click(object sender, RoutedEventArgs e)
        {
            MultiArticle = false;
            txtArticleSrh1.Clear();
            txtArticleSrh2.Clear();
            txtArticleSrh3.Clear();
            txtArticleSrh4.Clear();
            txtArticleSrh5.Clear();

            ArticleSrh1 = string.Empty;
            ArticleSrh2 = string.Empty;
            ArticleSrh3 = string.Empty;
            ArticleSrh4 = string.Empty;
            ArticleSrh5 = string.Empty;
        }

        private void BtnPfArticleSrh1Clear_Click(object sender, RoutedEventArgs e)
        {
            txtArticleSrh1.Clear();
        }

        private void BtnPfArticleSrh2Clear_Click(object sender, RoutedEventArgs e)
        {
            txtArticleSrh2.Clear();
        }

        private void BtnPfArticleSrh3Clear_Click(object sender, RoutedEventArgs e)
        {
            txtArticleSrh3.Clear();
        }

        private void BtnPfArticleSrh4Clear_Click(object sender, RoutedEventArgs e)
        {
            txtArticleSrh4.Clear();
        }

        private void BtnPfArticleSrh5Clear_Click(object sender, RoutedEventArgs e)
        {
            txtArticleSrh5.Clear();
        }

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

        private void btnPfBuyerArticleNo_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtBuyerArticleNo, 76, txtBuyerArticleNo.Text);
        }

        private void LabelBuyerArticleNoSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(CheckBoxBuyerArticleSearch.IsChecked == true)
            {
                CheckBoxBuyerArticleSearch.IsChecked = false;
            }
            else
            {
                CheckBoxBuyerArticleSearch.IsChecked = true;
            }
        }

        private void CheckBoxBuyerArticleSearch_Checked(object sender, RoutedEventArgs e)
        {
            TextBoxBuyerArticleNoSearch.IsEnabled = true;
            ButtonBuyerArticleNoSearch.IsEnabled = true;
            TextBoxBuyerArticleNoSearch.Focus();
        }

        private void CheckBoxBuyerArticleSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            TextBoxBuyerArticleNoSearch.IsEnabled = false;
            ButtonBuyerArticleNoSearch.IsEnabled = false;
        }

        private void TextBoxBuyerArticleNoSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(TextBoxBuyerArticleNoSearch, 76, TextBoxBuyerArticleNoSearch.Text);
            }
        }

        private void ButtonBuyerArticleNoSearch_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(TextBoxBuyerArticleNoSearch, 76, TextBoxBuyerArticleNoSearch.Text);
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

    }

    class Win_dvl_Molding_U_CodeView : BaseView
    {
        public int Num { get; set; }
        public string MoldID { get; set; }
        public string MoldKind { get; set; }
        public string MoldName { get; set; }
        public string ProductionArticleID { get; set; }

        public string Article { get; set; }
        public string MoldNo { get; set; }
        public string BuyerModelID { get; set; }
        public string BuyerModelName { get; set; }
        public string BuyerArticleNo { get; set; }

        public string dvlYN { get; set; }
        public string StorgeLocation { get; set; }
        public string StorgeLocationName { get; set; }
        public string MoldQuality { get; set; }
        public string Cavity { get; set; }

        public string RealCavity { get; set; }
        public string Weight { get; set; }
        public string Spec { get; set; }
        public string ProdCustomName { get; set; }
        public string SetDate { get; set; }

        public string ProdOrderDate { get; set; }
        public string ProdDueDate { get; set; }
        public string ProdCompDate { get; set; }
        public string MoldPerson { get; set; }
        public string Comments { get; set; }

        public string SetCheckProdQty { get; set; }
        public string AfterRepairHitcount { get; set; }
        public string SetWashingProdQty { get; set; }
        public string AfterWashHitcount { get; set; }
        public string SetProdQty { get; set; }

        public string Hitcount { get; set; }
        public string Evalscore { get; set; }
        public string EvalGrade { get; set; }
        public string EvalDate { get; set; }
        public string SetinitHitCount { get; set; }

        public string SetInitHitCountDate { get; set; }
        public string AttPath1 { get; set; }
        public string AttFile1 { get; set; }
        public string AttPath2 { get; set; }
        public string AttFile2 { get; set; }

        public string AttPath3 { get; set; }
        public string AttFile3 { get; set; }
        public string DisCardYN { get; set; }
        public string MoldKindName { get; set; }
        public string RegularCheckCount { get; set; }

        public string SetDate_CV { get; set; }
        public string ProdOrderDate_CV { get; set; }
        public string ProdDueDate_CV { get; set; }
        public string ProdCompDate_CV { get; set; }
        public string EvalDate_CV { get; set; }
        public string SetInitHitCountDate_CV { get; set; }

        public string PeriodHitCount { get; set; }
        public string AfterinitHitCount { get; set; }

        public bool flagProdOrderDate { get; set; }
        public bool flagProdDueDate { get; set; }
        public bool flagProdCompDate { get; set; }
        public bool flagSetInitHitCountDate { get; set; }
        public string SetMD { get; set; }
    }

    class Win_dvl_Molding_U_CodeView_One :  BaseView
    {
        public int Num { get; set; }
        public string ProductionArticleID { get; set; }
        public string Article { get; set; }
        public string BuyerArticleNo { get; set; }
        public string MoldNo { get; set; }
        public string MoldID { get; set; }

        public string BuyerModelName { get; set; }
        public string BuyerModelID { get; set; }
        public string DvlYN { get; set; }
        public string StorgeLocation { get; set; }
        public string Cavity { get; set; }
        public string RealCavity { get; set; }

        public string Weight { get; set; }
        public string Comments { get; set; }
        public string Spec { get; set; }
        public string ProdCustomName { get; set; }

        public string SetDate { get; set; }
        public string ProdOrderDate { get; set; }
        public string ProdDueDate { get; set; }
        public string ProdCompDate { get; set; }
        public string MoldPerson { get; set; }

        public string SetDate_CV { get; set; }
        public string ProdOrderDate_CV { get; set; }
        public string ProdDueDate_CV { get; set; }
        public string ProdCompDate_CV { get; set; }
        public string StorgeLocationName { get; set; }
        public string SetMD { get; set; }

        public string SetinitHitCount { get; set; }
        public string Hitcount { get; set; }
        public string EvalGrade { get; set; }


        public string AttPath1 { get; set; }
        public string AttFile1 { get; set; }
        public string AttPath2 { get; set; }
        public string AttFile2 { get; set; }

        public string AttPath3 { get; set; }
        public string AttFile3 { get; set; }
        public string RegularCheckCount { get; set; }
        



    }
    class Win_dvl_Molding_U_Sub_CodeView : BaseView
    {
        public int Num { get; set; }

        public string MoldEvalID { get; set; }
        public string EvalDate { get; set; }
        public string MoldID { get; set; }
        public string MoldNo { get; set; }
        public string Article { get; set; }

        public string BuyerArticleNo { get; set; }
        public string AvgWorkHourScore { get; set; }
        public string HitCount { get; set; }
        public string QualPartEasyChangeRateScore { get; set; }
        public string QualOccurRate { get; set; }

        public string QualAvgRepairHour { get; set; }
        public string Score { get; set; }
        public string EvalGrade { get; set; }
        public string EvalPersonName { get; set; }
        public string Comments { get; set; }
    }

    class Win_dvl_Molding_U_Parts_CodeView : BaseView
    {
        public int Num { get; set; }

        public string MoldID { get; set; }
        public string McPartID { get; set; }
        public string MCPartName { get; set; }
        public string ChangeCheckGbn { get; set; }
        public string CycleProdQty { get; set; }

        public string StartSetProdQty { get; set; }
        public string StartSetDate { get; set; }
    }
}
