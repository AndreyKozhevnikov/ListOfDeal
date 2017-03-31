using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace ListOfDeal.Views {
    /// <summary>
    /// Interaction logic for WunderListView.xaml
    /// </summary>
    public partial class WunderListView : UserControl {
        public WunderListView() {
            InitializeComponent();
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e) {
            var coll = listBox1.ItemsSource as INotifyCollectionChanged;
            coll.CollectionChanged += Coll_CollectionChanged;
        }

        private void Coll_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {

            Dispatcher.BeginInvoke((System.Action)(() => {
                listBox1.ScrollIntoView(e.NewItems[0]);
            }), DispatcherPriority.Background);

        }
    }
}
