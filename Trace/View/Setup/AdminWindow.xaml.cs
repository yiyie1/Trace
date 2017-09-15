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
using System.Windows.Forms;
using CsvFile;
using DataBase;
using System.Collections.ObjectModel;
using Trace.Model;

namespace Trace
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        
        ObservableCollection<User> userList = new ObservableCollection<User>();

        public AdminWindow()
        {
            InitializeComponent();
            listViewUser.ItemsSource = userList;
        }

        private void selectPatternBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            SelPatternWindow selPatternWin = new SelPatternWindow();
            selPatternWin.ShowDialog();
        }

        private void BtnImportData_Click(object sender, RoutedEventArgs e)
        {
            ImportCompensateData();
        }

        public static void ImportCompensateData()
        {
            OpenFileDialog filePicker = new OpenFileDialog()
                    {
                        CheckFileExists = true,
                        CheckPathExists = true,
                        Multiselect = false,
                        Filter = "CSV 文件 (*.csv)|*.csv"
                    };

            // Process open file dialog box results
            if (filePicker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImportCsvToDatabase(filePicker.FileName);
            }
        }

        private static void ImportCsvToDatabase(string csvFile)
        {
            List<string> listToInsert = new List<string>();

            List<string> rowList = new List<string>();
            try
            {
                //读取CSV中的数据
                using (var reader = new CsvFileReader(csvFile))
                {
                    while (reader.ReadRow(rowList))
                    {
                        listToInsert.Add(rowList[0]);
                    }
                }

                //删除表格内容
                DatabaseConnection.DeleteDataFromTable(DatabaseConnection.COMPENSATE_TABLE_NAME);
                
                //更新表格内容
                if (DatabaseConnection.SaveCompenParamsToDatabase(listToInsert))
                {
                    MsgBox msgBox = new MsgBox(Constants.TIP, Constants.SAVE_COMPENSATE_PARAMS_TO_DATABASE_SUCCESS, MessageBoxButton.OK);
                    msgBox.ShowDialog();
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
        }

        private void BtnExportData_Click(object sender, RoutedEventArgs e)
        {
            ExportCompensateData();
        }

        public static void ExportCompensateData()
        {
            SaveFileDialog saveDlg = new SaveFileDialog()
                    {
                        CheckFileExists = false,
                        CheckPathExists = true,
                        Filter = "CSV 文件 (*.csv)|*.csv"
                    };

            if (saveDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                exportCompensateDataToCsv(saveDlg.FileName);
            }
        }

        private static void exportCompensateDataToCsv(string csvFile)
        {
            try
            {
                using (var writer = new CsvFileWriter(csvFile))
                {
                    List<string> compensateParams = new List<string>();

                    DatabaseConnection.GetCompensateParamFromDatabase(ref compensateParams);

                    foreach (String param in compensateParams)
                    {
                        List<String> listToWrite = new List<String>();
                        listToWrite.Add(param);
                        writer.WriteRow(listToWrite);
                    }

                    MsgBox msgBox = new MsgBox(Constants.TIP, Constants.SAVE_COMPENSATE_PARAMS_CSV_SUCCESS, MessageBoxButton.OK);
                    msgBox.ShowDialog();
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
        }

        private void newChangeUserBtn_Click(object sender, RoutedEventArgs e)
        {
            NewUserWindow newUserWindow = new NewUserWindow();
            newUserWindow.Closed += newUserWindowClosed;
            newUserWindow.ShowDialog();
        }

        private void newUserWindowClosed(object sender, EventArgs e)
        {
            updateUserList();
        }

        private void delUserBtn_Click(object sender, RoutedEventArgs e)
        {
            int index = listViewUser.SelectedIndex;
            if (index == -1)
            {
                System.Windows.MessageBox.Show("请选择需要删除的数据");
            }
            else
            {
                User user = listViewUser.SelectedItem as User;
                if (App.UserName == user.UserName)
                {
                    System.Windows.MessageBox.Show(Constants.CANNOT_DEL_CURRENT_USER);
                    return;
                }

                bool ret = DatabaseConnection.DeleteUser(user.UserName);
                if (ret)
                {
                    updateUserList();
                    System.Windows.MessageBox.Show(Constants.SUCCESS_DEL_USER);
                }
                else
                {
                    System.Windows.MessageBox.Show(Constants.FAILURE_DEL_USER);
                }
            }
        }

        private void updateUserList()
        {
            List<User> users = new List<User>();
            DatabaseConnection.QueryUserTable(ref users);

            userList.Clear();

            foreach (var u in users)
            {
                userList.Add(u);
            }
        }

        private void AdminWindow_Loaded(object sender, RoutedEventArgs e)
        {
            updateUserList();
        }

        private void listViewUser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (listViewUser.SelectedIndex >= 0)
            {
                delUserBtn.IsEnabled = true;
            }
            else
            {
                delUserBtn.IsEnabled = false; ;
            }
        }
    }
}
