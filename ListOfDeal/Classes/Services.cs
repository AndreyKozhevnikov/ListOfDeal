using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ListOfDeal {
    public interface IExportToExcelService {
        void Export();
    }

    public class ExportToExcelService :ServiceBase, IExportToExcelService {


        public DataViewBase ExportTable {
            get { return (DataViewBase)GetValue(ExportTableProperty); }
            set { SetValue(ExportTableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExportTable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExportTableProperty =
            DependencyProperty.Register("ExportTable", typeof(DataViewBase), typeof(ExportToExcelService), new PropertyMetadata(null));

        
        public void Export() {
            (ExportTable as TableView).ExportToXlsx(@"f:\dropbox\common\Deals.xlsx");
        }
    }


    public interface IGridControlManagerService {
        void ExpandMasterRow(object obj);
    }

    public class GridControlManagerService : ServiceBase, IGridControlManagerService {
        GridControl Control;
        public void ExpandMasterRow(object obj) {
            var rh = Control.DataController.FindRowByRowValue(obj);
            Control.ExpandMasterRow(rh);
        }

        protected override void OnAttached() {
            var pc = this.AssociatedObject as ProjectsView;
            this.Control= pc.grid1;
            base.OnAttached();
        }
    }
}
