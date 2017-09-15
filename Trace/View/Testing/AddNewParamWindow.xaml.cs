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

namespace Trace
{
    /// <summary>
    /// Interaction logic for AddNewParamWin.xaml
    /// </summary>
    public partial class AddNewParamWindow : Window
    {
        public static string valueX { get; set; }
        public static string valueY { get; set; }
        public static string Angle { get; set; }
        public static string Stop { get; set; }
        public static int selectedStopIndex { get; set; }
        public bool IsOK { get; set; }

        public AddNewParamWindow()
        {
            InitializeComponent();

            xBox.Text = valueX;
            yBox.Text = valueY;
            AngleBox.Text = Angle;

            StopBox.SelectedIndex = selectedStopIndex;

            this.Loaded += AddNewParamWindow_Loaded;
        }

        void AddNewParamWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.lbXScope.Content = "输入范围为" + Constants.MIN_X + "~" + Constants.MAX_X;
            this.lbYScope.Content = "输入范围为" + Constants.MIN_Y + "~" + Constants.MAX_Y;
            this.lbAngleScope.Content = "输入范围为" + Constants.MIN_ANGLE + "~" + Constants.MAX_ANGLE;
            xBox.Focus();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            //判断是否为空
            if (String.IsNullOrEmpty(xBox.Text))
            {
                MsgBox msgBox = new MsgBox(Constants.WRONG, Constants.EMPTY_PARAM, MessageBoxButton.OK);
                msgBox.ShowDialog();
                xBox.Focus();
                return;
            }
            else if (String.IsNullOrEmpty(yBox.Text))
            {
                MsgBox msgBox = new MsgBox(Constants.WRONG, Constants.EMPTY_PARAM, MessageBoxButton.OK);
                msgBox.ShowDialog();
                yBox.Focus();
                return;
            }
            else if (String.IsNullOrEmpty(AngleBox.Text))
            {
                MsgBox msgBox = new MsgBox(Constants.WRONG, Constants.EMPTY_PARAM, MessageBoxButton.OK);
                msgBox.ShowDialog();
                AngleBox.Focus();
                return;
            }
            else if (!IsXPosValid(xBox.Text))
            {
                MsgBox msgBox = new MsgBox(Constants.WRONG, Constants.X_OVER_RANGE, MessageBoxButton.OK);
                msgBox.ShowDialog();
                xBox.Focus();
                return;
            }
            else if (!IsYPosValid(yBox.Text))
            {
                MsgBox msgBox = new MsgBox(Constants.WRONG, Constants.Y_OVER_RANGE, MessageBoxButton.OK);
                msgBox.ShowDialog();
                yBox.Focus();
                return;
            }
            else if (!IsAngleValid(AngleBox.Text))
            {
                MsgBox msgBox = new MsgBox(Constants.WRONG, Constants.ANGLLE_OVER_RANGE, MessageBoxButton.OK);
                msgBox.ShowDialog();
                AngleBox.Focus();
                return;
            }

            valueX = xBox.Text;
            valueY = yBox.Text;
            Angle = AngleBox.Text;
            Stop = StopBox.Text;
            selectedStopIndex = StopBox.SelectedIndex;

            IsOK = true;
            Close();
        }

        public static bool IsXPosValid(String str)
        {
            try
            {
                return int.Parse(str) <= Constants.MAX_X && int.Parse(str) >= Constants.MIN_X;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsYPosValid(String str)
        {
            try
            {
                return int.Parse(str) <= Constants.MAX_Y && int.Parse(str) >= Constants.MIN_Y;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsAngleValid(String str)
        {
            try
            {
                return int.Parse(str) <= Constants.MAX_ANGLE && int.Parse(str) >= Constants.MIN_ANGLE;
            }
            catch
            {
                return false;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region 限定TextBox输入数字

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!isNumberic(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        //isDigit是否是数字
        public static bool isNumberic(string _string)
        {
            if (string.IsNullOrEmpty(_string))
            {
                return false;
            }

            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                {
                    //if(c<'0' c="">'9')//最好的方法,在下面测试数据中再加一个0，然后这种方法效率会搞10毫秒左右
                    return false;
                }
            }
            return true;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!isNumberic(e.Text))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        #endregion
    }
}
