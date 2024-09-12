using ExcelDataReader;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WizMes_HanMin.PopUP;
using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;


/**************************************************************************************************
'** 프로그램명 : ExcelToDB
'** 설명       : 수주등록
'** 작성일자   : 2024-09-11
'** 작성자     : 최대현
'**------------------------------------------------------------------------------------------------
'**************************************************************************************************
' 변경일자  , 변경자, 요청자    , 요구사항ID      , 요청 및 작업내용
'**************************************************************************************************
' 2024.09.11, 최대현, 한민     품번을 선택하고 형상측정기 csv파일을 선택하면 검사기준 등록 여부를
                               확인 후에 없으면 자동 등록하고 검사값을 업로드 함
'**************************************************************************************************/

namespace WizMes_HanMin.Quality.PopUp
{
    /// <summary>
    /// ExcelToDB.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ExcelToDB : Window
    {
        string InspectBasisID_global = string.Empty;
        string strPoint = string.Empty;            
        string CreateUserID = MainWindow.CurrentUser;
        public bool _err = true;                        

        public event EventHandler<OperationCompletedEventArgs> OperationCompleted;

        public ExcelToDB(string insPoint)
        {
            InitializeComponent();
            strPoint = insPoint;
        }

        #region 시작시

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            chkBuyerArticleNo.IsEnabled = false;
            chkBuyerArticleNo.IsChecked = true;
            btnUploadToDB.IsEnabled = false;
            txtBuyerArticleNo.Focus();

            this.Title = SetTitle(strPoint);
            
        }

        

        //자주, 공정순회 타이틀 설정
        private string SetTitle(string strPoint)
        {
            string title = string.Empty;

            switch (strPoint)
            {
                case "3": title = "형성측정값 업로드(공정순회)"; break;
                case "9": title = "형성측정값 업로드(자주검사)"; break;

            }

            return title;
        }

        #endregion

        #region 등록 전&후 데이터 확인/검사기준 자동 셋팅/수정 메서드
        
        //ArticleID에 등록된 검사기준이 있는지 확인
        private bool CheckInsBasis(DataTable dt, string ArticleID, string strPoint)
        {
            bool flag = true;

            List<string> list_insItemName = new List<string>();
            List<string> list_InsRASpec = new List<string>();
            List<string> list_InsRASpecMin = new List<string>();
            List<string> list_InsRASpecMax = new List<string>();

            double double_InsRASpecMin = 0;
            double double_InsRASpecMax = 0;

            string insItemName = string.Empty;
            string InsRASpec = string.Empty;
            string InsRASpecMin = string.Empty;
            string InsRASpecMax = string.Empty;

            try
            { 
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    double_InsRASpecMin = Convert.ToDouble(dr["Column6"].ToString()) + Convert.ToDouble(dr["Column9"].ToString());
                    double_InsRASpecMax = Convert.ToDouble(dr["Column6"].ToString()) + Convert.ToDouble(dr["Column8"].ToString());

                    list_insItemName.Add(dr["Column0"].ToString() + " " + dr["Column1"].ToString());
                    list_InsRASpec.Add(dr["Column6"].ToString());
                    list_InsRASpecMin.Add(double_InsRASpecMin.ToString());
                    list_InsRASpecMax.Add(double_InsRASpecMax.ToString());

                }

                insItemName = string.Join("|", list_insItemName);
                InsRASpec = string.Join("|", list_InsRASpec);
                InsRASpecMin = string.Join("|", list_InsRASpecMin);
                InsRASpecMax = string.Join("|", list_InsRASpecMax);


                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("ArticleID", ArticleID);
                sqlParameter.Add("InspectPoint", strPoint);
                sqlParameter.Add("insItemName", insItemName);
                sqlParameter.Add("InsRASpec", InsRASpec);
                sqlParameter.Add("InsRASpecMin", InsRASpecMin);
                sqlParameter.Add("InsRASpecMax", InsRASpecMax);

                //시트에서 얻은 검사항목명/검사기준값/상한/하한을 구분자로 묶어 전달, 프로시저내에서 비교하여
                //내용과 일치하는 검사기준번호를 가지고 온다.
                DataSet dataSet = DataStore.Instance.ProcedureToDataSet("xp_Inspect_chkInspectAutoBasis", sqlParameter, false);

                if (dataSet != null && dataSet.Tables.Count > 0)
                {
                    DataTable dataTable = dataSet.Tables[0];

                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dataRow = dataTable.Rows[0];
                        if (dataRow[0].ToString() == "NO") flag = false; //NO이면 검사기준을 이후에 자동으로 새로 만듬
                        else InspectBasisID_global = dataRow[0].ToString();  //검사기준번호를 반환하고 이걸 전역변수에 할당함
                    }
                }
            }
            catch (Exception e)
            {

                if (e.Message.Contains("열은") && e.Message.Contains("테이블에 속하지 않습니다"))
                {
                    MessageBox.Show("값을 읽어오는 도중 오류가 발생했습니다." +
                                   "\n형상측정결과 파일을 엑셀 또는 메모장으로 직접 열어 수정한\n경우 발생 할 수 있습니다." +
                                   "\n가능한 원본 파일을 사용하여 주세요. 업로드를 중지합니다.", "파일 손상됨", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                else
                {
                    MessageBox.Show("오류 발생 : " + e.ToString());
                }

                flag = false;
                _err = false;
            }


            return flag;
        }

        //읽은 파일의 경로를 돌려주기
        private string SelectFiles()
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "CSV Files (*.csv)|*.csv" };
            bool? result = ofd.ShowDialog();

            if (result == true)
            {

                return ofd.FileName;
            }

            return null;
        }

        //검사기준 미등록일때 자동 등록
        private bool AutoRegisterBasis(DataTable dt, string ArticleID, string strPoint)
        {
            bool flag = true;
            string InspectBasisID = string.Empty;
            InspectBasisID_global = string.Empty; //검사기준에 부합하는게 없기 때문에 여기서 초기화

            try
                {
                //mt_InspectAutoBasis 등록
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("ArticleID", ArticleID);
                sqlParameter.Add("InspectPoint", strPoint);
                sqlParameter.Add("CreateUserID", CreateUserID);

                string[] result = DataStore.Instance.ExecuteProcedure("xp_Inspect_iInspectAutoBasis_AutoRegister", sqlParameter, true);
                if (result[0].Equals("success") && result[1].Length == 10)
                {
                    InspectBasisID = result[1].ToString();
                }
                else
                {
                    MessageBox.Show("오류 :" + result[1]);
                    flag = false;
                }

                //mt_InspectAutoBasisSub등록
                if (flag == true)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string insItemName = string.Empty;
                        string insRASpec = string.Empty;
                        double InsRASpecMin = 0;
                        double InsRASpecMax = 0;


                        DataRow dr = dt.Rows[i];
                        var UploadData = new UploadData()
                        {
                            //CSV파일 기준(2024-09-11)
                            //형상 측정기에서 내보낼때 Z1 지시정도.CSV 이런 파일 형식으로 나옴

                            Column0 = dr["Column0"].ToString(),     //단차(평균)
                            Column1 = dr["Column1"].ToString(),     //구별 셀, 컬럼0번 1번 합쳐서 검사항목명
                            Column2 = dr["Column2"].ToString(),     //측정내용
                            Column3 = dr["Column3"].ToString(),     //EmptyCell
                            Column4 = dr["Column4"].ToString(),     //측정값
                            Column5 = dr["Column5"].ToString(),     //EmptyCell
                            Column6 = dr["Column6"].ToString(),     //설계값
                            Column7 = dr["Column7"].ToString(),     //오차
                            Column8 = dr["Column8"].ToString(),     //상한 허용
                            Column9 = dr["Column9"].ToString(),     //하한 허용
                            Column10 = dr["Column10"].ToString(),   //공차외값
                            Column11 = dr["Column11"].ToString(),   //조합결과
                            Column12 = dr["Column12"].ToString(),   //판정
                            Column13 = dr["Column13"].ToString(),   //단위
                            Column14 = dr["Column14"].ToString(),   //검사일,getDate()형식으로 나옴
                            Column15 = dr["Column15"].ToString(),   //EmptyCell
                        };


                        insItemName = UploadData.Column0 + " " + UploadData.Column1;
                        insRASpec = UploadData.Column6;
                        InsRASpecMin = Convert.ToDouble(UploadData.Column9) + Convert.ToDouble(UploadData.Column6);
                        InsRASpecMax = Convert.ToDouble(UploadData.Column8) + Convert.ToDouble(UploadData.Column6);

                        sqlParameter = new Dictionary<string, object>();
                        sqlParameter.Add("InspectBasisID", InspectBasisID);
                        sqlParameter.Add("SubSeq", i + 1);
                        sqlParameter.Add("insItemName", insItemName);
                        sqlParameter.Add("InsRASpec", insRASpec);
                        sqlParameter.Add("InsRASpecMin", InsRASpecMin);
                        sqlParameter.Add("InsRASpecMax", InsRASpecMax);
                        sqlParameter.Add("CreateUserID", CreateUserID);

                        result = DataStore.Instance.ExecuteProcedure("xp_Inspect_iInspectAutoBasis_AutoRegisterSub", sqlParameter, true);
                        if (result[0].Equals("success"))
                        {
                            //MessageBox.Show($"총 {dt.Rows.Count} 중 {i + 1} 건 삽입됨");
                        }
                        else
                        {
                            MessageBox.Show("오류 :" + result[1]);
                            flag = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if(e.Message.Contains("열은") && e.Message.Contains("테이블에 속하지 않습니다"))
                {
                    MessageBox.Show("값을 읽어오는 도중 오류가 발생했습니다." +
                                    "\n형상측정결과 파일을 엑셀 또는 메모장으로 직접 열어 수정한\n경우 발생 할 수 있습니다." +
                                    "\n가능한 원본 파일을 사용하여 주세요. 업로드를 중지합니다.", "파일 손상됨",MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("오류 발생 : " + e.ToString());              
                }
                flag = false;
                _err = false;
            }
            finally
            {
                //서브에 넣다가 오류 발생시                 
                if (flag == false)
                {
                    DeleteRecentInsBasis(); //검사기준 등록한 것 바로 삭제
                }
            }

            return flag;

        }


        //데이터 업로드 이후 Ins_InspectAuto테이블 (합/불)&(합격/불량 수량) 업데이트
        private void SetDefectYN(string InspectID, string ArticleID)
        {

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("InspectID", InspectID);
                sqlParameter.Add("ArticleID", ArticleID);

                string[] result = DataStore.Instance.ExecuteProcedure("xp_Inspect_uAutoInspect_AutoRegister_Defect", sqlParameter, true);
                if (result[0].Equals("success"))
                {

                }
                else
                {
                    MessageBox.Show("오류 SetDefectYN():" + result[1]);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("합/불 조정 중 오류 : \n위치 : SetDefectYN()" + e.ToString());
            }
        }

        //Ins_InspectAutoSub에 데이터 넣을 때 만약 '판정' 컬럼으로 합불을 정하지 않는다면
        //상하한과 검사값 비교하기
        private bool CheckDefectYN(double InspectValue, double InsRASpecMin, double InsRASpecMax)
        {
            bool flag = true;

            if (!(InspectValue > InsRASpecMin && InspectValue < InsRASpecMax))
                flag = false;

            return flag;
        }


        #endregion

        #region 예외처리

        //최근 등록 검사기준 삭제
        private void DeleteRecentInsBasis()
        {
            string _InspectBasisID = string.Empty;

            string sql = "Select top 1 InspectBasisID from mt_InspectAutoBasis Where Comments like '%자동 업로드%' order by InspectBasisID desc";
            _InspectBasisID = DataStore.Instance.ExecSQLgetString(sql, false);

            sql = "DELETE mt_InspectAutoBasis WHERE InspectBasisID = " + _InspectBasisID;
            DataStore.Instance.ExecuteNonQuery(sql, false);
        }

        #endregion

        #region 업로드 관련

        //엑셀을 읽을 때
        public DataSet ReadExcelFile(string filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    var readerConfig = new ExcelReaderConfiguration()
                    {
                        FallbackEncoding = Encoding.GetEncoding("euc-kr")
                    };

                    using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream, readerConfig))
                    {
                        //DataSet 기본 설정 공간
                        var config = new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = false
                            }
                        };
                        var result = reader.AsDataSet(config);

                        //행 건너뛰어 테이블에 저장
                        var table = result.Tables[0];
                        int rowsToSkip = 10;
                        for (int i = 0; i < rowsToSkip; i++)
                        {
                            if (table.Rows.Count > 0)
                            {
                                table.Rows.RemoveAt(0);
                            }
                        }

                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("사용 중"))
                { MessageBox.Show("업로드 하려는 파일이 열려 있습니다.\n먼저 종료한 후 진행하여 주십시오.", "확인", MessageBoxButton.OK, MessageBoxImage.Error); }
                else { MessageBox.Show("오류 :" + e.ToString(), "확인"); }

                return new DataSet();
            }
        }

        //Csv를 읽을 때
        public DataSet ReadCsvFile(string filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    var readerConfig = new ExcelReaderConfiguration()
                    {
                        FallbackEncoding = Encoding.GetEncoding("euc-kr")
                    };

                    using (IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream, readerConfig))
                    {
                        //DataSet 기본 설정 공간
                        var config = new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = false,
                            }
                        };
                        var result = reader.AsDataSet(config);

                        //행 건너뛰어 테이블에 저장
                        var table = result.Tables[0];
                        int rowsToSkip = 10;
                        for (int i = 0; i < rowsToSkip; i++)
                        {
                            if (table.Rows.Count > 0)
                            {
                                table.Rows.RemoveAt(0);
                            }
                        }

                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("사용 중"))
                { MessageBox.Show("업로드 하려는 파일이 열려 있습니다.\n먼저 종료한 후 진행하여 주십시오.", "확인", MessageBoxButton.OK, MessageBoxImage.Error); }
                else { MessageBox.Show("오류 :" + e.ToString(), "확인"); }

                return new DataSet();
            }

        }

        //잘못된 파일을 읽었을 때 또는 오류 발생시에 빈 DataSet돌려주기
        public DataSet EmptyDataSet()
        {
            return new DataSet();
        }


        //읽은 데이터 DB에 업로드
        private bool UploadDataToDB(DataTable dt, string ArticleID, string strPoint)
        {
            bool flag = true;
            string InspectID = string.Empty;
            string InspectBasisID = string.Empty;

            try
            {
                //Ins_InspectAuto 등록
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("ArticleID", ArticleID);
                sqlParameter.Add("InspectPoint", strPoint);
                sqlParameter.Add("CreateUserID", CreateUserID);

                string[] result = DataStore.Instance.ExecuteProcedure("xp_Inspect_iAutoInspect_AutoRegister", sqlParameter, true);
                if (result[0].Equals("success"))
                {
                    string[] arr = result[1].Split('|'); //구분자로 문자열 내용 '검사기준번호|검사번호'를 반환
                    //검사기준과 일치하는 것이 있었으면 그 검사기준번호(InspectBasisID_global)를
                    //가지고 오고 그렇지 않으면 새로 만든걸(arr[0]) 갖다 쓴다.
                    InspectBasisID = InspectBasisID_global == string.Empty ? arr[0] : InspectBasisID_global;
                    InspectID = arr[1];
                }
                else
                {
                    MessageBox.Show("오류 :" + result[1]);
                    flag = false;
                }

                if (flag == true)
                {
                    //Ins_InspectAutoSub 등록
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        double InspectValue = 0;
                        string DefectYN = string.Empty;

                        //만약 판정으로 불량여부 결정 안한다고 하면                       
                        double InsRASpecMin = 0;
                        double InsRASpecMax = 0;

                        DataRow dr = dt.Rows[i];
                        var UploadData = new UploadData()
                        {
                            //CSV파일 기준(2024-09-11)
                            //형상 측정기에서 내보낼때 Z1 지시정도.CSV 이런 파일 형식으로 나옴
                            //판정이 '확인' = 합격

                            //////Column0 = dr["Column0"].ToString(),     //단차(평균)
                            //////Column1 = dr["Column1"].ToString(),     //구별 셀, 컬럼0번 1번 합쳐서 검사항목명
                            //////Column2 = dr["Column2"].ToString(),     //측정내용
                            //////Column3 = dr["Column3"].ToString(),     //EmptyCell
                            Column4 = dr["Column4"].ToString(),           //측정값
                            //////Column5 = dr["Column5"].ToString(),     //EmptyCell
                            Column6 = dr["Column6"].ToString(),           //설계값
                            //////Column7 = dr["Column7"].ToString(),     //오차
                            Column8 = dr["Column8"].ToString(),           //상한 허용
                            Column9 = dr["Column9"].ToString(),           //하한 허용
                            //////Column10 = dr["Column10"].ToString(),   //공차외값
                            //////Column11 = dr["Column11"].ToString(),   //조합결과
                            Column12 = dr["Column12"].ToString(),         //판정
                            //////Column13 = dr["Column13"].ToString(),   //단위
                            //////Column14 = dr["Column14"].ToString(),   //검사일,getDate()형식으로 나옴
                            //////Column15 = dr["Column15"].ToString(),   //EmptyCell
                        };

                        InspectValue = Convert.ToDouble(UploadData.Column4);

                        InsRASpecMin = Convert.ToDouble(UploadData.Column9) + Convert.ToDouble(UploadData.Column6);
                        InsRASpecMax = Convert.ToDouble(UploadData.Column8) + Convert.ToDouble(UploadData.Column6);

                        DefectYN = CheckDefectYN(InspectValue, InsRASpecMin, InsRASpecMax) == true ? "N" : "Y";
                        //DefectYN = UploadData.Column12 == "확인" ? "N" : "Y";

                        sqlParameter = new Dictionary<string, object>();
                        sqlParameter.Add("InspectID", InspectID);
                        sqlParameter.Add("Seq", i + 1);
                        sqlParameter.Add("InspectBasisID", InspectBasisID);
                        sqlParameter.Add("InspectBasisSubSeq", i + 1);
                        sqlParameter.Add("InspectValue", InspectValue);
                        sqlParameter.Add("DefectYN", DefectYN);
                        sqlParameter.Add("CreateUserID", CreateUserID);

                        result = DataStore.Instance.ExecuteProcedure("xp_Inspect_iAutoInspectSub_AutoRegister", sqlParameter, true);
                        if (result[0].Equals("success"))
                        {
                            //MessageBox.Show($"총 {dt.Rows.Count} 중 {i + 1} 건 삽입됨");
                        }
                        else
                        {
                            MessageBox.Show("오류 :" + result[1]);
                            flag = false;
                        }
                    }
                }

                SetDefectYN(InspectID, ArticleID);
            }
            catch(Exception e)
            {
                MessageBox.Show("오류 발생 : " + e.ToString());
            }
         

            return flag;
        }

        #endregion

        #region 버튼/텍스트박스 이벤트       
        //★     ★ ★★★★   ★         ★★★   ★★★    ★★★★ 
        //★     ★ ★     ★  ★        ★    ★  ★    ★  ★     
        //★     ★ ★★★★   ★        ★    ★  ★    ★  ★★★★
        //★     ★ ★         ★        ★    ★  ★    ★  ★     
        // ★★★   ★         ★★★★   ★★★   ★★★    ★★★★
        

        //업로드 클릭 시
        //검색한 품번이 있어야 버튼 활성화 됨
        private void btnUploadToDB_Click(object sender, RoutedEventArgs e)
        {
            string fileName = string.Empty;
            string extenstion = string.Empty;
            string strArticleID = txtBuyerArticleNo.Tag.ToString();
            string strBuyerArticleNo = txtBuyerArticleNo.Text;
            string strPoint = this.strPoint;
            bool flag = true;

            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            fileName = SelectFiles();
            if (fileName == null) return;

            var loadingDialog = new LoadingDialog("파일 업로드 준비 중...", "파일 업로드", "pack://application:,,,/Resources/excel.png", "Y");
            loadingDialog.Owner = Window.GetWindow(this);

            //여기서부터 업로드 시작
            //BackGroundWorker를 통해 업로드를 하면서 UI를 업데이트 할 수 있도록 함         
            loadingDialog.StartProcess(worker =>
            {
                string extension = System.IO.Path.GetExtension(fileName); 

                loadingDialog.UpdateProgressAndWait("파일 읽는 중...", 0, 15);

                //시트를 읽고 값이 있을때만 진행, 없으면 중단                
                if (extension.Contains("CSV"))
                {
                    ds = ReadCsvFile(fileName);
                    if (ds.Tables.Count > 0) dt = ds.Tables[0];
                    else { _err = false; return; }

                }
                //else if (extension.Contains("xls"))
                //{
                //    ds = ReadExcelFile(fileName);
                //    if (ds.Tables.Count > 0) dt = ds.Tables[0];
                //    else { _err = false; return; }
                //}
                else
                {
                    ds = EmptyDataSet();
                }

                loadingDialog.UpdateProgressAndWait("검사기준 등록 확인 중...", 15, 30);

                if (!CheckInsBasis(dt, strArticleID, strPoint)) //검사기준 없으면 자동 등록하고 업로드
                {
                    flag = AutoRegisterBasis(dt, strArticleID, strPoint); //검사 기준 자동 등록
                    if(flag == false)
                    {
                        ErrCloseDialog(loadingDialog);
                        return;
                    }
                    loadingDialog.UpdateProgressAndWait("미등록 대상 자동 등록 중...", 30, 60);
                    if (flag == true)
                    {
                        UploadDataToDB(dt, strArticleID, strPoint);
                        loadingDialog.UpdateProgressAndWait("업로드를 하고 있습니다...", 60, 90);
                    }

                }
                else //검사기준 있으면 바로 업로드
                {
                    loadingDialog.UpdateProgressAndWait("측정값 업로드 준비 중...", 30, 60);

                    if (dt.Rows.Count > 0)
                    {
                        flag = UploadDataToDB(dt, strArticleID, strPoint);
                        loadingDialog.UpdateProgressAndWait("업로드를 하고 있습니다...", 60, 90);
                    }

                }

                if (flag == true)
                {
                    loadingDialog.UpdateProgressAndWait("업로드 완료", 90, 100);
                    _err = true;

                }

            });

        }


        private void ErrCloseDialog(LoadingDialog loadingDialog)
        {
            _err = false;          
            Application.Current.Dispatcher.Invoke(new Action(delegate ()
            {
                if (loadingDialog != null)
                {
                    loadingDialog.Close();
                }
            }));
        }


        //품번 텍스트박스
        private void txtBuyerArticleNo_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    MainWindow.pf.ReturnCode(txtBuyerArticleNo, 84, txtBuyerArticleNo.Text);
                    //MessageBox.Show($"Text : {txtBuyerArticleNo.Text}\n Tag : {txtBuyerArticleNo.Tag}\n strPoint : {strPoint}");

                }
                ButtonEnable();
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

        //품번 버튼
        private void btnBuyerArticleNo_Click(object sender, RoutedEventArgs e)
        {
            var keyEventArgs = new KeyEventArgs(
                                  Keyboard.PrimaryDevice, //이벤트 발생시키는 키보드는?(사용자 주 키보드)
                                  Keyboard.PrimaryDevice.ActiveSource, //이벤트 소스
                                  0, //이벤트 발생시각 특별한 경우 아니면 0
                                  Key.Enter //누른 키
                                  );
            txtBuyerArticleNo_KeyDown(null, keyEventArgs);

            ButtonEnable();

        }


        #endregion

        #region 기타

        //품번 삭제시 버튼 비활성화/Tag값 초기화
        private void txtBuyerArticleNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBuyerArticleNo.Text.Length == 0)
            {
                txtBuyerArticleNo.Tag = null;
                btnUploadToDB.IsEnabled = false;
            }
        }

        //팝업창 열린 상태에서 검사실적등록 fillgrid()를 하기 위함
        public void OnOperationCompleted(bool success)
        {
            OperationCompleted?.Invoke(this, new OperationCompletedEventArgs(success));
        }

        //품번을 먼저 검색해야 버튼 활성화 됨
        private void ButtonEnable()
        {
            if (txtBuyerArticleNo.Tag != null)
            {
                btnUploadToDB.IsEnabled = true;
            }
        }

        #endregion
    }


    #region 진행바 메세지 박스 Class
    //커스텀 진행바 메세지 박스
    //오래걸리는 작업의 상태를 사용자가 확인함으로써 사용자 경험 개선
    //필수 사용은 아닙니다. 
    public class LoadingDialog : Window
    {
        private ProgressBar _progressBar;
        private TextBlock textBlock;
        private BackgroundWorker _worker;

        public LoadingDialog(string Text, string Title, string sourcePath, string UseProgressBar)
        {
            this.Title = Title;
            Width = 400;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.SingleBorderWindow;   //알트탭했을때 보이게
            ShowInTaskbar = true;                           //작업표시줄 여부

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var iconImage = new System.Windows.Controls.Image
            {
                //Source = new ImageSourceConverter().ConvertFromString("pack://application:,,,/Resources/verification.png") as ImageSource,
                Source = new ImageSourceConverter().ConvertFromString(sourcePath) as ImageSource,
                Width = 50,
                Height = 50,
                Margin = new Thickness(20, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(iconImage, 0);
            Grid.SetRowSpan(iconImage, 2);
            Grid.SetColumn(iconImage, 0);

            textBlock = new System.Windows.Controls.TextBlock
            {
                Text = Text,
                FontSize = 14,
                Margin = new Thickness(20, 0, 20, 0),
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(textBlock, 0);
            Grid.SetColumn(textBlock, 1);

            //하단 진행바 사용 여부
            if (UseProgressBar == "Y")
            {
                _progressBar = new ProgressBar
                {
                    Minimum = 0,
                    Maximum = 100,
                    Value = 0,
                    Height = 20,
                    Margin = new Thickness(20, 10, 20, 20),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                Grid.SetRow(_progressBar, 2);
                Grid.SetColumn(_progressBar, 0);
                Grid.SetColumnSpan(_progressBar, 2);
                grid.Children.Add(_progressBar);
            }

            grid.Children.Add(iconImage);
            grid.Children.Add(textBlock);

            Content = grid;

            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
        }

        //표시한 지점까지 한번에 차오르게 함
        //worker.ReportProgress(50, "검사기준 등록 확인 중...");
        public void StartProcess(Action<BackgroundWorker> workAction)
        {
            //핸들러 등록
            _worker.DoWork += (s, args) => workAction(_worker); 

            //진행률이 바뀌면 호출됨
            _worker.ProgressChanged += (s, args) =>
            {
                UpdateTextAndProgress(args.UserState.ToString(), args.ProgressPercentage);
            };

            //더 이상 백그라운드 실행할게 없으면 자동 실행됨
            _worker.RunWorkerCompleted += (s, args) =>
            {
                if (args.Error != null)
                {
                    MessageBox.Show("오류 발생: " + args.Error.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if(((ExcelToDB)this.Owner)._err == true)
                    {
                        MessageBox.Show("데이터 업로드가 완료되었습니다.", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;

                        // 성공적으로 완료된 경우에만 작업완료 알림
                        if (this.Owner is ExcelToDB excelToDB)
                        {
                            excelToDB.OnOperationCompleted(true);
                        }
                    }                  

                }
                this.Close();
               
            };

            _worker.RunWorkerAsync(); //백그라운드작업을 실행하도록 함 DoWork이벤트 호출
            this.ShowDialog(); //작업 중 메인 창을 건드리면 안되므로 모달 처리
        }

        //부드럽게 차오르는 버전
        public void UpdateProgressAndWait(string newText, int start, int end, int steps = 30, int delay = 10)
        {
            UpdateTextAndStartProgress(newText, start, end, steps, delay);
            WaitForProgressComplete();
        }

        //진행률 셋팅
        public void SetProgress(double value)
        {
            Dispatcher.Invoke(new System.Action(() =>
            {
                if (_progressBar != null)
                {
                    _progressBar.Value = value;
                }
            }));
        }

        //텍스트 업데이트
        public void UpdateText(string newText)
        {
            Dispatcher.Invoke(new System.Action(() =>
            {
                textBlock.Text = newText;
            }));
        }

        //텍스트와 진행률 같이 업데이트
        public void UpdateTextAndProgress(string newText, double progressValue)
        {
            Dispatcher.Invoke(new System.Action(() =>
            {
                textBlock.Text = newText;
                if (_progressBar != null)
                {
                    _progressBar.Value = progressValue;
                }
            }));
        }

        //진행율이 동기적으로 차오르도록 하기 위함
        private ManualResetEvent _progressComplete = new ManualResetEvent(false);

        //진행율을 시작 - 지정한 끝 부분까지 부드럽게 차오르도록
        public void UpdateTextAndStartProgress(string newText, int start, int end, int steps = 30, int delay = 10)
        {
            _progressComplete.Reset();

            Dispatcher.Invoke(new Action(() =>
            {
                UpdateText(newText);
                SetProgress(start);
            }));

            var progressWorker = new BackgroundWorker();
            progressWorker.WorkerReportsProgress = true;
            progressWorker.DoWork += (sender, args) =>
            {
                for (int i = 0; i <= steps; i++)
                {
                    int progress = start + (end - start) * i / steps;
                    progressWorker.ReportProgress(progress);
                    Thread.Sleep(delay);
                }
            };
            progressWorker.ProgressChanged += (sender, args) =>
            {
                Dispatcher.Invoke(new Action(() => SetProgress(args.ProgressPercentage)));
            };
            progressWorker.RunWorkerCompleted += (sender, args) =>
            {
                _progressComplete.Set();
            };
            progressWorker.RunWorkerAsync();
        }

        //백그라운드 작업과 동기적으로 수행할 수 있도록 함
        public void WaitForProgressComplete()
        {
            _progressComplete.WaitOne();
        }       
    }

    #endregion

    public class OperationCompletedEventArgs : EventArgs
    {
        public bool Success { get; }

        public OperationCompletedEventArgs(bool success)
        {
            Success = success;
        }
    }

    class UploadData : BaseView
    {
        public string BuyerArticleNo { get; set; }
        public string ArticleID { get; set; }

        //컬럼
        public string Column0 { get; set; }
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }
        public string Column4 { get; set; }
        public string Column5 { get; set; }
        public string Column6 { get; set; }
        public string Column7 { get; set; }
        public string Column8 { get; set; }
        public string Column9 { get; set; }
        public string Column10 { get; set; }
        public string Column11 { get; set; }
        public string Column12 { get; set; }
        public string Column13 { get; set; }
        public string Column14 { get; set; }
        public string Column15 { get; set; }
    }
}
