using System;
using System.Collections.Generic;

namespace FSX.Windows.Controls
{
    public class Intellisense : List<IntellisenseItem> {  }

    public class IntellisenseItem
    {
        public IntellisenseItem(string name)
        {
            Name = name;
            Children = new Intellisense();
        }

        #region Properties

        public string Name { get; set; }
        public Intellisense Children { get; set; }

        #endregion
    }
}
