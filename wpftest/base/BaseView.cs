using PropertyChanged;
using System.Collections.Generic;
using System.ComponentModel;

namespace WizMes_HanMin
{
    [ImplementPropertyChanged]
    public class BaseView :INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public override string ToString()
        {
            return this.ReportAllProperties();
        }

        public List<int> GetValue()
        {
            List<int> returnValue = new List<int>();



            return returnValue;
        }
    }
}
