using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
          
            this.DataContext = new MainViewModel();
            InitializeComponent();
#if DEBUG
            this.Title = this.Title + " Debug mode";
#endif
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            MainViewModel vm = this.DataContext as MainViewModel;
            vm.Test();
        }
    }

  
}
