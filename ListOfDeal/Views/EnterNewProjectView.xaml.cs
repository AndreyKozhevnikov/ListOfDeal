using DevExpress.Data.Filtering;
using DevExpress.Xpf.Grid;
using System;
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
            CriteriaOperator.RegisterCustomFunction(new GetOutdatedActionsFunction());
        }
        private void Button_Click(object sender, RoutedEventArgs e) {
            MainViewModel vm = this.DataContext as MainViewModel;
            vm.Test();
        }

        private void GridControl_Loaded(object sender, RoutedEventArgs e) {
            var gc = sender as GridControl;
            gc.RefreshData();
#if DEBUG
            gc.UngroupBy("TypeId");
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
            return actions.Where(x => x.IsActive2).Count();
        }
    }
    public class GetOutdatedActionsFunction : ICustomFunctionOperator {

        public string Name {
            get { return "GetOutdatedActions"; }
        }

        public Type ResultType(params Type[] operands) {
            return typeof(bool);
        }

        public object Evaluate(params object[] operands) {
            var actions = operands[0] as ObservableCollection<MyAction>;
            var outdated = actions.Where(x => x.ScheduledTime < DateTime.Today && x.IsActive2);
            return outdated.Count() > 0;
        }
    }
}
