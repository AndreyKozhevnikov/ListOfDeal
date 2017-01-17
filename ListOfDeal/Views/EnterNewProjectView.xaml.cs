using DevExpress.Data.Filtering;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ListOfDeal.Views {
    /// <summary>
    /// Interaction logic for EnterNewProjectView.xaml
    /// </summary>
    public partial class EnterNewProjectView :UserControl {
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
            (sender as GridControl).RefreshData();
#if DEBUG
            (sender as GridControl).UngroupBy("TypeId");
#endif
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
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
            return actions.Where(x => x.IsActive == true).Count();
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
            var outdated = actions.Where(x => x.ScheduledTime < DateTime.Today);
            return outdated.Count() > 0;
        }
    }
}
