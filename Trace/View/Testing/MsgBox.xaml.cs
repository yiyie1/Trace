using System;
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

namespace Trace
{
    /// <summary>
    /// Interaction logic for MissingWnd.xaml
    /// </summary>
    public partial class MsgBox : Window
    {
        public static String msgBoxTitle = String.Empty;
        public static String msgBoxTip = String.Empty;
        public static MessageBoxButton msgBoxBtn;
        public static bool isNoControl = false;

        public MsgBox(String caption, String msg, MessageBoxButton btns, bool IsNoControl = false)
        {
            this.Owner = Application.Current.MainWindow;
            InitializeComponent();

            this.MouseLeftButtonDown += new MouseButtonEventHandler(MsgBox_MouseLeftButtonDown);
            txtTitle.Text = caption;
            txtMsg.Text = msg;
            btnOK.Click += new RoutedEventHandler(btnOK_Click);
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnOK_OK.Click += new RoutedEventHandler(btnOK_Click);
            btnYes.Click += new RoutedEventHandler(btnOK_Click);
            btnNo.Click += new RoutedEventHandler(btnCancel_Click);
            btnClose.Click += new RoutedEventHandler(btnCancel_Click);

            spOKCancel.Visibility = (btns == MessageBoxButton.OKCancel) ? Visibility.Visible : Visibility.Collapsed;
            spOK.Visibility = (btns == MessageBoxButton.OK) ? Visibility.Visible : Visibility.Collapsed;
            spYesNo.Visibility = (btns == MessageBoxButton.YesNo) ? Visibility.Visible : Visibility.Collapsed;

            if (IsNoControl)
            {
                spOKCancel.Visibility = Visibility.Collapsed;
                spOK.Visibility = Visibility.Collapsed;
                spYesNo.Visibility = Visibility.Collapsed;
                btnClose.Visibility = Visibility.Collapsed;
            }
        }

        void MsgBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
