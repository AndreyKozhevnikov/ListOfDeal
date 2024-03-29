﻿using DevExpress.Mvvm.UI;
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
using DevExpress.Xpf.Editors;

namespace ListOfDeal {

    public class FocusedRowEventArgsConverter : EventArgsConverterBase<FocusedRowHandleChangedEventArgs> {
        protected override object Convert(object sender, FocusedRowHandleChangedEventArgs args) {
            var rh = args.RowData.RowHandle.Value;
            var tv = sender as TableView;
            var gc = tv.DataControl as GridControl;
            var isGroup = gc.IsGroupRowHandle(rh);
            if(isGroup) {
                var chRH = gc.GetChildRowHandle(rh, 0);
                var ch = gc.GetRow(chRH) as MyProject;
                return ch.ProjectType;
            }
            return null;
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
#if DebugTest
        public DateTime ToDayToTest;
#endif
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            DateTime dt = (DateTime)value;
            dt = dt.Date;
            var today = DateTime.Today;
#if DebugTest
            today = ToDayToTest;
#endif
            var diff = today.DayOfWeek - DayOfWeek.Wednesday;
            if(diff < 0)
                diff += 7;
            var stDt = today.AddDays(-1 * diff).AddDays(5).Date;
            var fnDt = stDt.AddDays(6);
            if(dt >= stDt && dt <= fnDt)
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

    public class DateToWeekDayConverter : MarkupExtension, IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            DateTime dt;
            DateTime.TryParse(value.ToString(), out dt);
            if(dt != null) {
                return dt.ToString("yyyy-MM-dd dddd");
            } else {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }

}
