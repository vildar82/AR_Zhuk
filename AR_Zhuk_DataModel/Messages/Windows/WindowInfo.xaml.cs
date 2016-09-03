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
using System.Windows.Shapes;

namespace AR_Zhuk_DataModel.Messages.Windows
{
    /// <summary>
    /// Логика взаимодействия для WindowInfo.xaml
    /// </summary>
    internal partial class WindowInfo : Window
    {
        public WindowInfo (MessagesViewModel model)
        {
            InitializeComponent();
            Height =Properties.Settings.Default.Height;
            Width =Properties.Settings.Default.Width;
            Top=Properties.Settings.Default.Top;
            Left=Properties.Settings.Default.Left;
            DataContext = model;
        }

        private void Window_Closing (object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Height = Height;
            Properties.Settings.Default.Width = Width;
            Properties.Settings.Default.Top = Top;
            Properties.Settings.Default.Left = Left;
            Properties.Settings.Default.Save();            
        }
    }
}
