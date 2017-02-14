using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ListOfDeal {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DXWindow {
        public MainWindow() {
            MainViewModel.DataProvider = new MainViewModelDataProvider();
            this.DataContext = new MainViewModel();
            InitializeComponent();
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            var st = this.Title + " - " + v;
#if DEBUG
           st  = st + " Debug mode";
#endif
            this.Title = st;
        }


    }


}
