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
using System.Data;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using CsvFile;
using Trace.Model;

namespace Trace
{
    /// <summary>
    /// ProjectParam.xaml 的交互逻辑
    /// </summary>
    public partial class ProjectParamWindow : Window
    {
        string m_csvFile;

        public static ObservableCollection<ProjectParam> ParametersList = new ObservableCollection<ProjectParam>();

        private const int listViewColumnCount = 5;

        public ProjectParamWindow()
        {
            InitializeComponent();

            listViewData.SelectionChanged += listViewData_SelectionChanged;

            listViewData.ItemsSource = ParametersList;
        }

        void listViewData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewData.SelectedIndex >= 0)
            {
                BtnDel.IsEnabled = true;
            }
            else
            {
                BtnDel.IsEnabled = false; ;
            }
        }

        private void HideAndShowAdjustWnd()
        {
            this.Hide();
            AjustWindow ajustWin = new AjustWindow();
            ajustWin.Closed += adjustWinClosed;
            ajustWin.ShowDialog();
        }

        private void BtnStartAjust_Click(object sender, RoutedEventArgs e)
        {
            if (ParametersList.Count <= 0)
            {
                MsgBox msgBox = new MsgBox(Constants.WRONG, Constants.NO_TEST_PARAMS, MessageBoxButton.OK);
                msgBox.Owner = this;
                msgBox.ShowDialog();
                return;
            }

            HideAndShowAdjustWnd();
        }

        private void saveListViewDataToCsv(string csvFile)
        {
            try
            {
                using (var writer = new CsvFileWriter(csvFile))
                {
                    foreach (var item in listViewData.Items)
                    {
                        Model.ProjectParam param = (Model.ProjectParam)item;
                        List<string> columns = new List<string>();

                        columns.Add(param.OperID.ToString());
                        columns.Add(param.PositionX.ToString());
                        columns.Add(param.PositionY.ToString());
                        columns.Add(param.MeasureAngle.ToString());
                        columns.Add(param.Stop.ToString());

                        writer.WriteRow(columns);
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveProjectParamsToCsv();
        }

        private void SaveProjectParamsToCsv()
        {
            if (listViewData.Items.Count > 0)
            {
                SaveFileDialog saveDlg = new SaveFileDialog()
                {
                    CheckFileExists = false,
                    CheckPathExists = true,
                    Filter = "CSV 文件 (*.csv)|*.csv"
                };

                if (saveDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    m_csvFile = saveDlg.FileName;
                    saveListViewDataToCsv(m_csvFile);
                }
            }
        }

        private void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            int index = listViewData.SelectedIndex;
            if (index == -1)
            {
                System.Windows.MessageBox.Show("请选择需要删除的数据");
            }
            else
            {
                if(System.Windows.MessageBox.Show(Constants.CONFIRMED_DELETE_DATA, "警告", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }
                ParametersList.RemoveAt(index);

                RefreshIdInParametersList();
                listViewData.Items.Refresh();
            }
        }

        static void RefreshIdInParametersList()
        {
            for (int i = 0; i < ParametersList.Count; i++)
            {
                ParametersList[i].OperID = i + 1;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddNewParamWindow newParamWin = new AddNewParamWindow();
            newParamWin.Closed += newParamWinClosed;
            newParamWin.ShowDialog();
        }

        private void adjustWinClosed(object sender, EventArgs e)
        {
            this.Show();
        }

        private void newParamWinClosed(object sender, EventArgs e)
        {
            AddNewParamWindow newParamWin = (AddNewParamWindow)sender;
            if (newParamWin.IsOK)
            {
                int operID = listViewData.SelectedIndex;
                if (operID < 0)
                {
                    operID = 0;
                }
                
                ProjectParam p = new ProjectParam(operID, int.Parse(AddNewParamWindow.valueX), int.Parse(AddNewParamWindow.valueY), Convert.ToInt32(AddNewParamWindow.Angle),
                    Convert.ToInt32(AddNewParamWindow.Stop));

                ParametersList.Insert(operID, p);

                RefreshIdInParametersList();
                listViewData.Items.Refresh();

                listViewData.SelectedIndex = operID;
            }
        }

        private void loadListViewDataFromCsv(string csvFile)
        {
            List<string> list = new List<string>();
            try
            {
                using (var reader = new CsvFileReader(csvFile))
                {
                    while (reader.ReadRow(list))
                    {
                        if (list.Count != listViewColumnCount || !AddNewParamWindow.IsXPosValid(list[1])
                            || !AddNewParamWindow.IsYPosValid(list[2]) || !AddNewParamWindow.IsAngleValid(list[3]))
                        {
                            MsgBox msgBox = new MsgBox(Constants.WRONG, Constants.IMPORT_PARAMS_WRONG, MessageBoxButton.OK);
                            msgBox.ShowDialog();
                            return;
                        }

                        ProjectParam p = new ProjectParam(Convert.ToInt32(list[0]), int.Parse(list[1]), int.Parse(list[2]), Convert.ToInt32(list[3]), Convert.ToInt32(list[4]));

                        ParametersList.Add(p);
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
        }

        private void BtnLoadPlan_Click(object sender, RoutedEventArgs e)
        {
            //先询问用户是否需要保存当前的参数
            if (listViewData.Items.Count > 0)
            {
                MsgBox msgBox = new MsgBox(Constants.WARNING, Constants.WHETHER_SAVE_PARAMS, MessageBoxButton.YesNo);
                msgBox.Owner = this;
                if (msgBox.ShowDialog() == false)
                {
                    return;
                }
            }

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
                ParametersList.Clear();
                m_csvFile = filePicker.FileName;
                loadListViewDataFromCsv(m_csvFile);
            }
        }
    }
}
