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
using Trace.View.Result;
using DataBase;
using Trace.Model;
using System.Threading;

namespace Trace.View.Result
{
    /// <summary>
    /// SearchFilterUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class SearchFilterUserControl : UserControl
    {
        public List<TestResult> searchResults = new List<TestResult>();

        public SearchFilterUserControl()
        {
            InitializeComponent();

            SearchUserControl searchControl = new SearchUserControl();
            searchPanel.Children.Add(searchControl);

            startDate.SelectedDate = DateTime.Now;
            endDate.SelectedDate = DateTime.Now;

            searchControl.StartDate = startDate.SelectedDate.Value;
            searchControl.EndDate = endDate.SelectedDate.Value;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            search();

            var p = this.Parent;
            if (searchResults != null && searchResults.Count != 0)
            {
                //searchResultsPanel.Children.Clear();

                //SearchResultUserControl searchControl = new SearchResultUserControl() { SearchResults = searchResults };
                //searchResultsPanel.Children.Add(searchControl);
            }

            /*SearchResultWindow resWindow = new SearchResultWindow() { SearchResults = searchResults };
            resWindow.Closed += resWindowClosed;
            resWindow.ShowDialog();*/

        }

        public bool search()
        {
            List<SearchParam> searchParamsResult = new List<SearchParam>();

            foreach (SearchUserControl se in searchPanel.Children)
            {
                string field = (se.searchComboBox.SelectedItem as ComboBoxItem).Tag.ToString().Trim();

                //Check if field is duplicated
                if (searchParamsResult.Exists(x => x.Field == field))
                {
                    MessageBox.Show(Constants.DUP_SEARCH_FIELD);
                    return false;
                }

                string compare = se.compareComboBox.Text.ToString().Trim();
                string value = "";
                if (field == "TaskName")
                {
                    value = se.taskComboBox.Text.ToString().Trim();
                }
                else
                {
                    value = se.searchText.Text.ToString().Trim();
                }
                string logicOperation = se.logicComboBox.Text.ToString().Trim();

                if (value == "")
                {
                    MessageBox.Show(Constants.EMPTY_SEARCH_VALUE);
                    return false;
                }

                SearchParam param = new SearchParam
                {
                    Field = field,
                    Compare = compare,
                    Value = value,
                    LogicOperation = logicOperation
                };
                Console.WriteLine("Searching: {0} {1} {2}, {3}", param.Field, param.Compare, param.Value, param.LogicOperation);

                searchParamsResult.Add(param);
            }

            searchResults.Clear();

            bool bDate = datePickerCheckBox.IsChecked == true ? true : false;
            searchResults = DatabaseConnection.QueryFromResult(searchParamsResult, bDate, startDate.SelectedDate.Value, endDate.SelectedDate.Value);

            return true;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchUserControl searchControl = new SearchUserControl();
            searchPanel.Children.Add(searchControl);

            DelBtn.IsEnabled = (searchPanel.Children.Count >= 1);
        }

        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            int count = searchPanel.Children.Count;
            if (count > 1)
            {
                searchPanel.Children.RemoveAt(count - 1);

                DelBtn.IsEnabled = (searchPanel.Children.Count != 1);
            }
        }

        private void startDate_changed(object sender, RoutedEventArgs e)
        {
            if (startDate.SelectedDate.HasValue)
            {
                foreach (SearchUserControl se in searchPanel.Children)
                {
                    se.StartDate = startDate.SelectedDate.Value;
                    se.UpdateVisible();
                }
            }
        }

        private void endDate_changed(object sender, RoutedEventArgs e)
        {
            if (endDate.SelectedDate.HasValue)
            {
                foreach (SearchUserControl se in searchPanel.Children)
                {
                    se.EndDate = endDate.SelectedDate.Value;
                    se.UpdateVisible();
                }
            }
        }

        private void datePickerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (SearchUserControl se in searchPanel.Children)
            {
                se.IsDateEnabled = true;
                se.StartDate = startDate.SelectedDate.Value;
                se.EndDate = endDate.SelectedDate.Value;
                se.UpdateVisible();
            }
        }

        private void datePickerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (SearchUserControl se in searchPanel.Children)
            {
                se.IsDateEnabled = false;
                se.UpdateVisible();
            }
        }
    }
}
