using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FSX.Windows.Controls;
using System.Collections.Generic;

namespace FSX.IntelliBox
{
    public class ViewModel : INotifyPropertyChanged
    {       
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ViewModel()
        {
            var intellisense1 = new IntellisenseItem("Pakistan")
            {
                Children = new Intellisense()
                {
                    new IntellisenseItem("Punjab")
                    {
                        Children = new Intellisense()
                        {
                            new IntellisenseItem("Lahore"),
                            new IntellisenseItem("Islamabad"),
                            new IntellisenseItem("Multan"),
                        }
                    },
                    new IntellisenseItem("Sindh")
                    {
                        Children = new Intellisense()
                        {
                            new IntellisenseItem("Karachi"),
                            new IntellisenseItem("Hyderabad"),
                            new IntellisenseItem("Jamshoro"),
                            new IntellisenseItem("Jacobabad"),
                        }
                    },
                    new IntellisenseItem("KPK")
                    {
                        Children = new Intellisense()
                        {
                            new IntellisenseItem("Peshawar"),
                            new IntellisenseItem("Abbottabad"),
                            new IntellisenseItem("Chitral"),
                            new IntellisenseItem("Swat"),
                        }
                    },
                    new IntellisenseItem("Balochistan")
                    {
                        Children = new Intellisense()
                        {
                            new IntellisenseItem("Quetta"),
                            new IntellisenseItem("Gwadar"),
                            new IntellisenseItem("Chagai"),
                        }
                    }
                }
            };

            var intellisense2 = new IntellisenseItem("USA")
            {
                Children = new Intellisense()
                {
                    new IntellisenseItem("Alabama"),                   
                    new IntellisenseItem("California"),
                    new IntellisenseItem("Hawaii"),
                    new IntellisenseItem("Georgia")
                }
            };

            IntellisenseList = new Intellisense();
            IntellisenseList.Add(intellisense1);
            IntellisenseList.Add(intellisense2);
        }

        #region Properties

        private Intellisense _intellisenseList;
        public Intellisense IntellisenseList
        {
            get { return _intellisenseList; }
            set { _intellisenseList = value; OnPropertyChanged(); }
        }

        #endregion
    }
}
