using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WizMes_DeokWoo.PopUp;
using WizMes_DeokWoo.PopUP;

namespace WizMes_DeokWoo
{
    /// <summary>
    /// Win_com_Process_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_prd_Test : UserControl
    {
        int rowNum = 0;

        public Win_prd_Test()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Lib.Instance.UiLoading(sender);            
        }


        #region 주요 메서드


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

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_prd_sTest", sqlParameter, false);

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

                            var Test = new prd_Test_CodeView()
                            {
                                num = dr["num"].ToString(),
                                value = dr["value"].ToString(),
                            };

                            dgdMain.Items.Add(Test);

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

        #endregion // 대분류 조회

        #region 저장

        /// <summary>
        /// 저장
        /// </summary>
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
                    sqlParameter.Add("num", @txtNum.Text);
                    sqlParameter.Add("value", txtValue.Text);
                  
                    Procedure pro1 = new Procedure();
                    pro1.Name = "xp_prd_Test";
                    pro1.OutputUseYN = "N";
                    pro1.OutputName = "ProcessID";
                    pro1.OutputLength = "4";

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
                    else
                    {
                        flag = true;
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

        #endregion // 저장

        #region 유효성 검사

        private bool CheckData()
        {
            bool flag = true;

            // 

          

            return flag;
        }

        #region 공정ID 
        private bool ChkProcessID(string ProcessID)
        {
            bool flag = true;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();

                sqlParameter.Add("ProcessID", ProcessID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Process_sChkProcessID", sqlParameter, false);

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

        #endregion // 유효성 검사

        #endregion // 주요 메서드

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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //if (SaveData())
            //{

            //}
            //FillGrid();

            //Loading loading = new Loading();
            //loading.ShowDialog();
        }
    }

    // 코드뷰
    class prd_Test_CodeView : BaseView
    {
        public string num { get; set; }
        public string value { get; set; }
    }
}
