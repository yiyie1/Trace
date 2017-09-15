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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using DataBase;
using Trace.Model;
using System.ComponentModel;

namespace Trace.View.Result
{
    /// <summary>
    /// SearchUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class SearchUserControl : UserControl
    {
        ViewModel viewMode = new ViewModel();
        public DateTime StartDate = DateTime.Now;
        public DateTime EndDate = DateTime.Now;
        public bool IsDateEnabled = false;

        public SearchUserControl()
        {
            InitializeComponent();
            this.DataContext = viewMode;
        }

        public void UpdateVisible()
        {
            if (searchComboBox.SelectedItem != null)
            {
                if ((searchComboBox.SelectedItem as ComboBoxItem).Tag.ToString().Trim() == "TaskName")
                {
                    List<string> names = DatabaseConnection.QueryFromTask("TaskName", IsDateEnabled, StartDate, EndDate);
                    taskComboBox.ItemsSource = names;
                    taskComboBox.SelectedIndex = 0;

                    (this.DataContext as ViewModel).SearchTextVis = false;
                    (this.DataContext as ViewModel).TaskComboBoxVis = true;
                }
                else
                {
                    (this.DataContext as ViewModel).SearchTextVis = true;
                    (this.DataContext as ViewModel).TaskComboBoxVis = false;
                }
            }
        }

        private void searchComboBox_Closed(object sender, EventArgs e)
        {
            UpdateVisible();
        }

        private void searchUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateVisible();
        }

    }

    public class ViewModel : INotifyPropertyChanged
    {
        private bool _searchTextVis = false;
        public bool SearchTextVis
        {
            get { return _searchTextVis; }
            set
            {
                if (_searchTextVis != value)
                {
                    _searchTextVis = value;
                    NotifyPropertyChanged("SearchTextVis");  // To notify when the property is changed
                }
            }
        }

        private bool _taskComboBoxVis = true;
        public bool TaskComboBoxVis
        {
            get { return _taskComboBoxVis; }
            set
            {
                if (_taskComboBoxVis != value)
                {
                    _taskComboBoxVis = value;
                    NotifyPropertyChanged("TaskComboBoxVis");  // To notify when the property is changed
                }
            }
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
