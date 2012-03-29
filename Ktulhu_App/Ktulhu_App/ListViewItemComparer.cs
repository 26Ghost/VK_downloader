using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Globalization;

namespace Ktulhu_App
{
    class ListViewItemComparer : IComparer
    {
        private int col, por;
        public ListViewItemComparer()
        {
            col = 0;
            por = 1;
        }
        public ListViewItemComparer(int column, int sort_type)
        {
            col = column;
            por = sort_type;
        }
        public int Compare(object x, object y)
        {
            if(col==0)
                return por*String.Compare(((ListViewItem)x).SubItems[col].Text.PadLeft(5,'0'), ((ListViewItem)y).SubItems[col].Text.PadLeft(5,'0'), StringComparison.OrdinalIgnoreCase);
            if (col == 3)
                return por * DateTime.Compare(DateTime.Parse(((ListViewItem)x).SubItems[col].Text+" AM"), DateTime.Parse(((ListViewItem)y).SubItems[col].Text+" AM"));

            return por*String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text,StringComparison.OrdinalIgnoreCase);
        }
    }
   
}
