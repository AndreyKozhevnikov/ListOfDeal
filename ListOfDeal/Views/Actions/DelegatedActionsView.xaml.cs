using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for DelegatedActionsView.xaml
    /// </summary>
    public partial class DelegatedActionsView :UserControl {
        public DelegatedActionsView() {
            InitializeComponent();
        }
        public GridControl Grid {
            get {
                return this.InnerGrid;
            }
        }
    }
}
