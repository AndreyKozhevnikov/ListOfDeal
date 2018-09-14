using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;
using ListOfDeal.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WlConnectionLibrary.Classes;

namespace ListOfDeal {
    public interface IExportToExcelService {
        void Export();
    }

    public class ExportToExcelService : ServiceBase, IExportToExcelService {


        public GridControl ScheduledGrid {
            get { return (GridControl)GetValue(SchedGridProperty); }
            set { SetValue(SchedGridProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SchedGrid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SchedGridProperty =
            DependencyProperty.Register("ScheduledGrid", typeof(GridControl), typeof(ExportToExcelService), new PropertyMetadata(null));


        public GridControl WaitedGrid {
            get { return (GridControl)GetValue(WaitedGridProperty); }
            set { SetValue(WaitedGridProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WaitedGrid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WaitedGridProperty =
            DependencyProperty.Register("WaitedGrid", typeof(GridControl), typeof(ExportToExcelService), new PropertyMetadata(null));





        public void Export() {
            var dropBoxPath = SettingsStore.GetPropertyValue("Dropbox");
            (WaitedGrid.View as TableView).ExportToXlsx(dropBoxPath + @"\common\Deals.xlsx");
            (ScheduledGrid.View as TableView).ExportToXlsx(dropBoxPath + @"\common\DealsSched.xlsx");
        }
    }


    public interface IGridControlManagerService {
        void ExpandFocusedMasterRow();
        void ExpandMasterRow(object obj);
        void ScrollToSeveralRows();
        void ClearFilterAndSearchString();
    }

    public class GridControlManagerService : ServiceBase, IGridControlManagerService {
        GridControl Control;

        public void ClearFilterAndSearchString() {
            // Control.FilterString = null;
            Control.View.SearchString = null;
        }

        public void ExpandFocusedMasterRow() {
            Control.ExpandMasterRow(Control.View.FocusedRowHandle);
        }

        public void ExpandMasterRow(object obj) {
            var rh = Control.DataController.FindRowByRowValue(obj);
            Control.ExpandMasterRow(rh);

        }

        public void ScrollToSeveralRows() {
            var rh = Control.View.FocusedRowHandle;
            Control.View.ScrollIntoView(rh + 10);
        }

        protected override void OnAttached() {
            var pc = this.AssociatedObject as GridControl;
            this.Control = pc;
            base.OnAttached();
        }
    }
}
