using DevExpress.Data.Filtering;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;


namespace ListOfDeal.Views {
    /// <summary>
    /// Interaction logic for EnterNewProjectView.xaml
    /// </summary>
    public partial class EnterNewProjectView : UserControl {
        public EnterNewProjectView() {
            InitializeComponent();
            CriteriaOperator.RegisterCustomFunction(new GetActiveActionsFunction());
            CriteriaOperator.RegisterCustomFunction(new GetScheduledActions());
        }
        private void Button_Click(object sender, RoutedEventArgs e) {
            MainViewModel vm = this.DataContext as MainViewModel;
            vm.Test();
        }

        private void GridControl_Loaded(object sender, RoutedEventArgs e) {
            var gc = sender as GridControl;
            gc.RefreshData();
#if DEBUG
        //    gc.UngroupBy("TypeId");
#endif
            var descr = DependencyPropertyDescriptor.FromProperty(TableView.SearchStringProperty, typeof(TableView));
            EventHandler myE = new EventHandler(SearchChanged);
            descr.AddValueChanged(gc.View, myE);

        }

        private void SearchChanged(object sender, EventArgs e) {
            var tv = sender as TableView;
            var gc = tv.DataControl as GridControl;
            if (!string.IsNullOrEmpty(tv.SearchString)) {
                gc.ExpandAllGroups();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            for (int i = 0; i < grid1.VisibleRowCount; i++) {
                int rowHandle = grid1.GetRowHandleByVisibleIndex(i);
                grid1.CollapseMasterRow(rowHandle);
            }
            grid1.CollapseAllGroups();
        }

        private void TableView_ColumnHeaderClick(object sender, ColumnHeaderClickEventArgs e) {
            e.AllowSorting = false;
            e.Handled = true;
        }
    }

    public class GetActiveActionsFunction : ICustomFunctionOperator {

        public string Name {
            get { return "GetActiveActions"; }
        }

        public Type ResultType(params Type[] operands) {
            return typeof(bool);
        }

        public object Evaluate(params object[] operands) {
            var actions = operands[0] as ObservableCollection<MyAction>;
            return actions.Where(x => x.Status == ActionsStatusEnum.InWork).Count();
        }
    }
 
    public enum GetScheduledActionsResult {
        HasScheduledActions,
        HasComingScheduledActons,
        HasOutDatedScheduledActons
    }
    public class GetScheduledActions : ICustomFunctionOperator {
        public string Name {
            get { return "GetScheduledActions"; }
        }

        public object Evaluate(params object[] operands) {
            var actions = operands[0] as IList<MyAction>;
            var scheduledActions = actions.Where(x => x.Status == ActionsStatusEnum.InWork && x.ScheduledTime != null);
            if (scheduledActions.Count() > 0) {
                if (scheduledActions.Where(x => x.ScheduledTime < DateTime.Today).Count() > 0)
                    return GetScheduledActionsResult.HasOutDatedScheduledActons;
                if (scheduledActions.Where(x => x.ScheduledTime <= DateTime.Today.AddDays(8)).Count() > 0)
                    return GetScheduledActionsResult.HasComingScheduledActons;
                return GetScheduledActionsResult.HasScheduledActions;
            }
            return null;

        }

        public Type ResultType(params Type[] operands) {
            return typeof(string);
        }
    }
}
