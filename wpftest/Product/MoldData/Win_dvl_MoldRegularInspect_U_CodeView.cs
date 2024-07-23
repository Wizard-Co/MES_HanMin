using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WizMes_HanMin
{
    class Win_dvl_MoldRegularInspect_U_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        /*-----------------------  dvl_MoldRegularInspect 의 Columns  -----------------    */
        public string MoldRInspectID { get; set; }
        public string MoldInspectBasisID { get; set; }
        public string MoldRInspectDate { get; set; }
        public string MoldRInspectUserID { get; set; }
        public string Comments { get; set; }

        public string HitCount { get; set; }
        public string MoldID { get; set; }
        public string MoldNo { get; set; }
        public string MoldRInspectDate_CV { get; set; }
        public string Num { get; set; }
        public string Person { get; set; }

        public string Article_Sabun { get; set; }
        public string ChkRegularGbn { get; set; }
    }

    class Win_dvl_MoldRegularInspect_U_Sub_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        /*-----------------------  dvl_MoldRegularInspectSub 의 Columns  -----------------    */
        public string MoldRInspectID { get; set; }
        //public string MldRInspectSubSeq { get; set; }
        //public string MoldInsBasisID { get; set; }
        public string MoldInsSeq { get; set; }
        public string MldRValue { get; set; }   //수치 확인 결과
        public string MldRInspectLegend { get; set; }   //범례 확인 결과
        public string Comments { get; set; }

        /*-----------------------  dvl_MoldRegularInspectBasisSub 의 Columns  -----------------    */
        //public string MoldInspectBasisID { get; set; }   //
        //public string MoldInspectSeq { get; set; }      //점검순서
        public string MoldInspectItemName { get; set; }   //
        public string MoldInspectContent { get; set; }   //
        public string MoldInspectCheckGbn { get; set; }   //
        public string MoldInspectCycleGbn { get; set; }   //
        public string MoldInspectCycleDate { get; set; }   //
        public string MoldInspectRecordGbn { get; set; }   //

        //public string MoldInspectID { get; set; }   //
        public string MoldID { get; set; }
        public string InspectSubSeq { get; set; }   //BasisSub,InspectSub 둘다 쓰기위해
        public string MoldInspectBasisID { get; set; } //BasisID 이거로 통합
        public string MoldInspectCheckName { get; set; }   //
        public string MoldInspectCycleName { get; set; }   //
        public string MoldInspectRecordName { get; set; }   //
        public string MoldInspectImageFile { get; set; }   //
        public BitmapImage ImageByte { get; set; }
        //
        public bool flagLegend { get; set; }
        public bool flagComments { get; set; }
    }
}
