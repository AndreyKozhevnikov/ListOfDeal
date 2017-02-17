using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Editors.Popups.Calendar;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Globalization;

namespace ListOfDeal {
    class CustomDisplayTextConverter : MarkupExtension, IValueConverter {
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null)
                return value;
            string st = value.ToString();
            if (st == "Trigger Id: ")
                return "Trigger Id: Empty trigger";
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
    public class FocusedRowEventArgsConverter : EventArgsConverterBase<FocusedRowHandleChangedEventArgs> {
        protected override object Convert(object sender, FocusedRowHandleChangedEventArgs args) {
            var rh = args.RowData.RowHandle.Value;
            var tv = sender as TableView;
            var gc = tv.DataControl as GridControl;
            var isGroup = gc.IsGroupRowHandle(rh);
            if (isGroup) {
                var chRH = gc.GetChildRowHandle(rh, 0);
                var ch = gc.GetRow(chRH) as MyProject;
                return ch.TypeId;
            }
            return -1;
        }
    }

    public class custConverter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return "test";
            //return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class SpecialDateBorderStyleConverter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            CalendarCellButton currentCellButton = (CalendarCellButton)value;
            DateTime dt = (DateTime)DateEditCalendar.GetDateTime((DependencyObject)currentCellButton);
            dt = dt.Date;
            var today = DateTime.Today;
            var diff = today.DayOfWeek - DayOfWeek.Monday;
            if (diff < 0)
                diff += 7;
            var stDt = today.AddDays(-1 * diff).AddDays(7).Date;
            var fnDt = stDt.AddDays(6);
            if (dt >= stDt && dt <= fnDt)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }

}
