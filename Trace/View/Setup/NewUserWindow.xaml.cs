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
using DataBase;

namespace Trace
{
    /// <summary>
    /// Interaction logic for NewUserWindow.xaml
    /// </summary>
    public partial class NewUserWindow : Window
    {
        private string _userName;
        private string _password;

        public NewUserWindow()
        {
            InitializeComponent();
        }

        bool checkUserInput()
        {
            _userName = userNameTextBox.Text.Trim();
            _password = userPasswordBox.Password.Trim();

            if (_userName == "")
            {
                System.Windows.MessageBox.Show(Constants.EMPTY_USER_NAME);
                return false;
            }
            if (_password.Length < 6)
            {
                System.Windows.MessageBox.Show(Constants.SHORT_PASSWORD);
                return false;
            }

            return true;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            bool bValid = checkUserInput();
            if (!bValid)
                return;

            bool? isChecked = adminCheckBox.IsChecked;
            //admin role is 0
            int userRole = isChecked == true ? 0 : 1;

            bool ret = DatabaseConnection.SaveUserPwdToDatabase(_userName, _password, userRole);
            if (ret)
            {
                System.Windows.MessageBox.Show(Constants.SUCCESS_ADD_USER);
            }
            else
            {
                System.Windows.MessageBox.Show(Constants.FAILURE_ADD_USER);
            }

            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
