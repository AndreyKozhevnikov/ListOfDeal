using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ListOfDeal {
    public interface IExportToExcelService {
        void Export();
    }

    public class ExportToExcelService :ServiceBase, IExportToExcelService {


        public TableView ExportTable {
            get { return (TableView)GetValue(ExportTableProperty); }
            set { SetValue(ExportTableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExportTable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExportTableProperty =
            DependencyProperty.Register("ExportTable", typeof(TableView), typeof(ExportToExcelService), new PropertyMetadata(null));

        
        public void Export() {
            ExportTable.ExportToXlsx(@"f:\dropbox\comon\Deals.xlsx");
        }
    }
}
