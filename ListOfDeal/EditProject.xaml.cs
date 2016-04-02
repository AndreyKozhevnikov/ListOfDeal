﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using System.Windows.Threading;


namespace ListOfDeal {
    /// <summary>
    /// Interaction logic for EditProject.xaml
    /// </summary>
    public partial class EditProject :DXWindow {
        public EditProject() {
            InitializeComponent();
        }

        private void GridDragDropManager_Drop(object sender, DevExpress.Xpf.Grid.DragDrop.GridDropEventArgs e) {
            Dispatcher.BeginInvoke((System.Action)(() => {
                GridControl gc = e.GridControl;
                var count = gc.VisibleRowCount;
                for (int i = 0; i < count; i++) {
                    var rh = gc.GetRowHandleByVisibleIndex(i);
                    var act = gc.GetRow(rh) as MyAction;
                    act.OrderNumber = rh;
                }
            }), DispatcherPriority.Input);



        }
    }
}


