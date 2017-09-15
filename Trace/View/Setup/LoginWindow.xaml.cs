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
using System.Data.SqlClient;
using System.Data;
using Trace.Model;

namespace Trace
{
    /// <summary>
    /// Interaction logic for LoginWin.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            this.Loaded += LoginWindow_Loaded;
            this.MouseLeftButtonDown += LoginWindow_MouseLeftButtonDown;
        }

        void LoginWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UserTextBox.Focus();
        }

        private bool CheckUserPwd(string userName, string password, ref Int32 userRole)
        {
            List<User> users = new List<User>();
            bool ret = DatabaseConnection.QueryUserTable(ref users);

            foreach (var u in users)
            {
                if (u.UserName == userName && u.Password == password)
                {
                    userRole = u.UserRole == "管理员" ? 0 : 1;
                    break;
                }
            }
            return ret;
        }


        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (UserTextBox.Text == "" || PwdTextBox.Password == "")
            {
                MessageBox.Show("请输入用户名和密码");
                return;
            }
            else
            {
                Int32 userRole = -1;
                bool bValidUser = CheckUserPwd(UserTextBox.Text, PwdTextBox.Password ,ref userRole);
                App.UserRole = userRole;

                if (bValidUser)
                {
                    App.UserName = UserTextBox.Text;

                    if (userRole == 0)
                    {
                        MessageBox.Show("管理员登陆");

                        this.Hide();
                        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

                        MainWindowTrace mainWindowNew = new MainWindowTrace();
                        App.Current.MainWindow = mainWindowNew; 
                        mainWindowNew.Show();
                    }
                    else if (userRole == 1)
                    {
                        MessageBox.Show("操作员登陆");

                        this.Hide();
                        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

                        MainWindowTrace mainWindowNew = new MainWindowTrace();
                        App.Current.MainWindow = mainWindowNew;
                        mainWindowNew.Show();
                    }

                }
                else
                {
                    MessageBox.Show("验证用户名密码失败，请重新输入");
                }
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();         
        }

        protected override void OnClosed(EventArgs e)
        {
            App.Current.Shutdown();
        }
        
    }
}
