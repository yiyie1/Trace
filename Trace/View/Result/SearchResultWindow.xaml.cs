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
using Trace.Model;
using System.ComponentModel;
using System.Windows.Controls.DataVisualization.Charting;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;
using DataBase;

namespace Trace.View.Result
{
    /// <summary>
    /// SearchResultWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SearchResultWindow : Window
    {
        int _groupValue = 6;
        bool _reverseSelectionDone = true;
        public List<TestResult> SearchResults = new List<TestResult>();
        private List<TestResult> _calculaionResults = new List<TestResult>();

        private List<TestResult> _selection = new List<TestResult>();

        private TestResult _avgResult;
        private TestResult _varianceSquareResult;
        private TestResult _maxResult;
        private TestResult _minResult;

        delegate void UpdateCalListviewDelegate();
        private UpdateCalListviewDelegate updateCalListviewDelegate;

        delegate void UpdateGuassDelegate(ObservableCollection<KeyValuePair<double, double>> guasslist);
        private UpdateGuassDelegate updateGuassDelegate;

        delegate void UpdateColumnChartDelegate(ObservableCollection<KeyValuePair<double, double>> columnlist);
        private UpdateColumnChartDelegate updateColumnChartDelegate;

        delegate void UpdatePicDelegate(List<double> results);
        private UpdatePicDelegate updatePicDelegate;
        private string _comboBoxValue;

        public SearchResultWindow()
        {
            InitializeComponent();
            updateCalListviewDelegate = new UpdateCalListviewDelegate(updateCalListview);
            updateColumnChartDelegate = new UpdateColumnChartDelegate(updateColumnChart);
            updateGuassDelegate = new UpdateGuassDelegate(updateGuassChart);
            updatePicDelegate = new UpdatePicDelegate(UpdatePic);
        }

        private void SearchResultWindow_Loaded(object sender, RoutedEventArgs e)
        {
            listViewData.ItemsSource = SearchResults;
            calculationListview.ItemsSource = _calculaionResults;
            _comboBoxValue = dataComboBox.Text.Trim();
        }

        private void dataComboBox_Closed(object sender, EventArgs e)
        {
            _comboBoxValue = dataComboBox.Text.Trim();

            if (_selection.Count > 0)
            {
                calculateColumnDataInThread();
                calculateGaussInThread();
            }
        }

        private void returnBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Delegate function
        private void updateColumnChart(ObservableCollection<KeyValuePair<double, double>> columnlist)
        {
            ColumnSeries cs = (ColumnSeries)resultChart.Series[0];
            cs.ItemsSource = columnlist;
        }

        private void updateGuassChart(ObservableCollection<KeyValuePair<double, double>> guasslist)
        {
            LineSeries cs = (LineSeries)resultChart.Series[1];
            cs.ItemsSource = guasslist;
        }

        private void updateCalListview()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(calculationListview.ItemsSource);
            view.Refresh();
        }

        #endregion

        #region Create, start thread
        private void startCalculationThread()
        {
            Thread threadCal = new Thread(() => calculateInThread());
            threadCal.IsBackground = true;
            threadCal.Start();
        }

        private void startColumnDataThread()
        {
            string field = dataComboBox.Text;
            Thread columnThread = new Thread(() => calculateColumnDataInThread());
            columnThread.IsBackground = true;
            columnThread.Start();
        }

        private void startGuassDataThread()
        {
            Thread guassThread = new Thread(() => calculateGaussInThread());
            guassThread.IsBackground = true;
            guassThread.Start();
        }
        #endregion

        #region Thread function
        private int[] groupingData(double[] groupArray, List<double> dataList)
        {
            int[] frequencyArray = new int[_groupValue];
            if (_groupValue == 6)
            {
                foreach (double d in dataList)
                {
                    if (d >= groupArray[0] && d < groupArray[1])
                    {
                        frequencyArray[0]++;
                    }
                    else if (d >= groupArray[1] && d < groupArray[2])
                    {
                        frequencyArray[1]++;
                    }
                    else if (d >= groupArray[2] && d < groupArray[3])
                    {
                        frequencyArray[2]++;
                    }
                    else if (d >= groupArray[3] && d < groupArray[4])
                    {
                        frequencyArray[3]++;
                    }
                    else if (d >= groupArray[4] && d < groupArray[5])
                    {
                        frequencyArray[4]++;
                    }
                    else if(d >= groupArray[5])
                    {
                        frequencyArray[5]++;
                    }
                }
            }
            else if(_groupValue == 12)
            {
                foreach (double d in dataList)
                {
                    if (d >= groupArray[0] && d < groupArray[1])
                    {
                        frequencyArray[0]++;
                    }
                    else if (d >= groupArray[1] && d < groupArray[2])
                    {
                        frequencyArray[1]++;
                    }
                    else if (d >= groupArray[2] && d < groupArray[3])
                    {
                        frequencyArray[2]++;
                    }
                    else if (d >= groupArray[3] && d < groupArray[4])
                    {
                        frequencyArray[3]++;
                    }
                    else if (d >= groupArray[4] && d < groupArray[5])
                    {
                        frequencyArray[4]++;
                    }
                    else if (d >= groupArray[5] && d < groupArray[6])
                    {
                        frequencyArray[5]++;
                    }
                    else if (d >= groupArray[6] && d < groupArray[7])
                    {
                        frequencyArray[6]++;
                    }
                    else if (d >= groupArray[7] && d < groupArray[8])
                    {
                        frequencyArray[7]++;
                    }
                    else if (d >= groupArray[8] && d < groupArray[9])
                    {
                        frequencyArray[8]++;
                    }
                    else if (d >= groupArray[9] && d < groupArray[10])
                    {
                        frequencyArray[9]++;
                    }
                    else if (d >= groupArray[10] && d < groupArray[11])
                    {
                        frequencyArray[10]++;
                    }
                    else if(d >= groupArray[11])
                    {
                        frequencyArray[11]++;
                    }
                }
            }

            return frequencyArray;
        }

        private void calculateColumnDataInThread()
        {
            List<double> dataList = getDataListFromField(_selection);
            double maxValue = getDataFromField(_maxResult);
            double minValue = getDataFromField(_minResult);
            double uintValue = (maxValue - minValue) / _groupValue;

            double[] groupArray = new double[_groupValue];

            for (int i = 0; i < _groupValue; i++)
            {
                groupArray[i] = minValue + i * uintValue;
            }

            int[] group = groupingData(groupArray, dataList);

            ObservableCollection<KeyValuePair<double, double>> columnlist = new ObservableCollection<KeyValuePair<double, double>>();

            int index = 0;
            foreach (int d in group)
            {
                columnlist.Add(new KeyValuePair<double, double>(Math.Round(groupArray[index++], 1), d));
                //columnlist.Add(new KeyValuePair<double, double>(index++, d));
            }

            Dispatcher.Invoke(updateColumnChartDelegate, columnlist);
        }

        
        private void calculateGaussInThread()
        {

            TestResult variance = TestResult.Pow(_varianceSquareResult, 0.5);
            TestResult val = TestResult.Pow((variance * Math.PI * 2), -0.5);
            double eVal = Math.Log(10.0);

            //List<double> guassValues = new List<double>();

            ObservableCollection<KeyValuePair<double, double>> guasslist = new ObservableCollection<KeyValuePair<double, double>>();
            foreach (TestResult x in _selection)
            {
                TestResult upVal = ((x - _avgResult) * (x - _avgResult)) / (_varianceSquareResult * 2);
                TestResult t = TestResult.Pow(eVal, upVal);
                TestResult minusT = TestResult.Pow(t, -1);
                TestResult y = val * minusT;

                double key = getDataFromField(x);
                double value = getDataFromField(y);
                //guassValues.Add(value);
                guasslist.Add(new KeyValuePair<double, double>(key, value));
            }

            /*double maxValue = guassValues.Max();
            double minValue = guassValues.Min();
            double uintValue = (maxValue - minValue) / _groupValue;

            double[] groupArray = new double[_groupValue];

            for (int i = 0; i < _groupValue; i++)
            {
                groupArray[i] = minValue + i * uintValue;
            }

            int[] group = groupingData(groupArray, guassValues);*/

            

            //int index = 0;
            //foreach (int d in group)
            //{
                //_guasslist.Add(new KeyValuePair<double, double>(Math.Round(groupArray[index++], 1), d));
                //guasslist.Add(new KeyValuePair<int, double>(index++, d));
            //}

            Dispatcher.Invoke(updateGuassDelegate, guasslist);
        }

        private void calculateInThread()
        {
            calculateAverageValue();
            calculateVarianceValue();

            _maxResult = findMaxValue();
            _minResult = findMinValue();

            _calculaionResults.Add(_avgResult);
            _calculaionResults.Add(_maxResult);
            _calculaionResults.Add(_minResult);
            _calculaionResults.Add(_varianceSquareResult);

            Dispatcher.Invoke(updateCalListviewDelegate);

            calculateColumnDataInThread();
            calculateGaussInThread();
        }


        private void calculateAverageValue()
        {
            //Average
            TestResult temp = new TestResult();
            foreach (TestResult sel in _selection)
            {
                temp += sel;
            }

            _avgResult = temp / _selection.Count;
            _avgResult.Type = "平均值";
        }

        private void calculateVarianceValue()
        {
            //Variance
            TestResult t = new TestResult();
            foreach (TestResult sel in _selection)
            {
                TestResult tmp = sel - _avgResult;
                tmp *= tmp;
                t += tmp;
            }

            _varianceSquareResult = t / _selection.Count;
            _varianceSquareResult.Type = "方差值";
        }

        private TestResult findMaxValue()
        {
            TestResult tmp = _selection[0];
            TestResult maxResult = new TestResult
            {
                Type = "极大值",
                TaskName = tmp.TaskName,
                GroupName = tmp.GroupName,
                ReelNumber = tmp.ReelNumber,
                Illuminant = tmp.Illuminant,
                ObserveAngle = tmp.ObserveAngle,
                ID = tmp.ID,
                TestID = tmp.TestID,
                PositionX = tmp.PositionX,
                PositionY = tmp.PositionY,
                MeasureAngle = tmp.MeasureAngle,
                Stop = tmp.Stop,
                IntegrationTime_ms = tmp.IntegrationTime_ms,
                Average = tmp.Average,
                Smooth = tmp.Smooth,
                UpperL = tmp.UpperL,
                UpperC = tmp.UpperC,
                UpperH = tmp.UpperH,
                LowerA = tmp.LowerA,
                LowerB = tmp.LowerB,
                UpperY = tmp.UpperY,
                LowerX = tmp.LowerX,
                LowerY = tmp.LowerY,
                UpperR = tmp.UpperR,
                UpperG = tmp.UpperG,
                UpperB = tmp.UpperB,
            };

            foreach (TestResult sel in _selection)
            {
                if (sel.PositionX > maxResult.PositionX)
                {
                    maxResult.PositionX = sel.PositionX;
                }
                if (sel.PositionY > maxResult.PositionY)
                {
                    maxResult.PositionY = sel.PositionY;
                }
                if (sel.MeasureAngle > maxResult.MeasureAngle)
                {
                    maxResult.MeasureAngle = sel.MeasureAngle;
                }
                if (sel.Stop > maxResult.Stop)
                {
                    maxResult.Stop = sel.Stop;
                }
                if (sel.IntegrationTime_ms > maxResult.IntegrationTime_ms)
                {
                    maxResult.IntegrationTime_ms = sel.IntegrationTime_ms;
                }
                if (sel.Average > maxResult.Average)
                {
                    maxResult.Average = sel.Average;
                }
                if (sel.Smooth > maxResult.Smooth)
                {
                    maxResult.Smooth = sel.Smooth;
                }
                if (sel.UpperL > maxResult.UpperL)
                {
                    maxResult.UpperL = sel.UpperL;
                }

                if (sel.UpperC > maxResult.UpperC)
                {
                    maxResult.UpperC = sel.UpperC;
                }
                if (sel.UpperH > maxResult.UpperH)
                {
                    maxResult.UpperH = sel.UpperH;
                }
                if (sel.LowerA > maxResult.LowerA)
                {
                    maxResult.LowerA = sel.LowerA;
                }
                if (sel.LowerB > maxResult.LowerB)
                {
                    maxResult.LowerB = sel.LowerB;
                }
                if (sel.UpperY > maxResult.UpperY)
                {
                    maxResult.UpperY = sel.UpperY;
                }
                if (sel.LowerX > maxResult.LowerX)
                {
                    maxResult.LowerX = sel.LowerX;
                }
                if (sel.LowerY > maxResult.LowerY)
                {
                    maxResult.LowerY = sel.LowerY;
                }
                if (sel.UpperR > maxResult.UpperR)
                {
                    maxResult.UpperR = sel.UpperR;
                }
                if (sel.UpperG > maxResult.UpperG)
                {
                    maxResult.UpperG = sel.UpperG;
                }
                if (sel.UpperB > maxResult.UpperB)
                {
                    maxResult.UpperB = sel.UpperB;
                }
            }

            return maxResult;
        }

        private TestResult findMinValue()
        {
            TestResult tmp = _selection[0];
            TestResult minResult = new TestResult
            {
                Type = "极小值",
                ID = tmp.ID,
                TestID = tmp.TestID,
                TaskName = tmp.TaskName,
                GroupName = tmp.GroupName,
                ReelNumber = tmp.ReelNumber,
                Illuminant = tmp.Illuminant,
                ObserveAngle = tmp.ObserveAngle,
                PositionX = tmp.PositionX,
                PositionY = tmp.PositionY,
                MeasureAngle = tmp.MeasureAngle,
                Stop = tmp.Stop,
                IntegrationTime_ms = tmp.IntegrationTime_ms,
                Average = tmp.Average,
                Smooth = tmp.Smooth,
                UpperL = tmp.UpperL,
                UpperC = tmp.UpperC,
                UpperH = tmp.UpperH,
                LowerA = tmp.LowerA,
                LowerB = tmp.LowerB,
                UpperY = tmp.UpperY,
                LowerX = tmp.LowerX,
                LowerY = tmp.LowerY,
                UpperR = tmp.UpperR,
                UpperG = tmp.UpperG,
                UpperB = tmp.UpperB,
            };

            foreach (TestResult sel in _selection)
            {
                if (sel.PositionX < minResult.PositionX)
                {
                    minResult.PositionX = sel.PositionX;
                }
                if (sel.PositionY < minResult.PositionY)
                {
                    minResult.PositionY = sel.PositionY;
                }
                if (sel.MeasureAngle < minResult.MeasureAngle)
                {
                    minResult.MeasureAngle = sel.MeasureAngle;
                }
                if (sel.Stop < minResult.Stop)
                {
                    minResult.Stop = sel.Stop;
                }
                if (sel.IntegrationTime_ms < minResult.IntegrationTime_ms)
                {
                    minResult.IntegrationTime_ms = sel.IntegrationTime_ms;
                }
                if (sel.Average < minResult.Average)
                {
                    minResult.Average = sel.Average;
                }
                if (sel.Smooth < minResult.Smooth)
                {
                    minResult.Smooth = sel.Smooth;
                }
                if (sel.UpperL < minResult.UpperL)
                {
                    minResult.UpperL = sel.UpperL;
                }

                if (sel.UpperC < minResult.UpperC)
                {
                    minResult.UpperC = sel.UpperC;
                }
                if (sel.UpperH < minResult.UpperH)
                {
                    minResult.UpperH = sel.UpperH;
                }
                if (sel.LowerA < minResult.LowerA)
                {
                    minResult.LowerA = sel.LowerA;
                }
                if (sel.LowerB < minResult.LowerB)
                {
                    minResult.LowerB = sel.LowerB;
                }
                if (sel.UpperY < minResult.UpperY)
                {
                    minResult.UpperY = sel.UpperY;
                }
                if (sel.LowerX < minResult.LowerX)
                {
                    minResult.LowerX = sel.LowerX;
                }
                if (sel.LowerY < minResult.LowerY)
                {
                    minResult.LowerY = sel.LowerY;
                }
                if (sel.UpperR < minResult.UpperR)
                {
                    minResult.UpperR = sel.UpperR;
                }
                if (sel.UpperG < minResult.UpperG)
                {
                    minResult.UpperG = sel.UpperG;
                }
                if (sel.UpperB < minResult.UpperB)
                {
                    minResult.UpperB = sel.UpperB;
                }
            }

            return minResult;
        }
        #endregion

        private List<double> getDataListFromField(List<TestResult> testResult)
        {
            List<double> dataList = new List<double>();
            foreach (TestResult tr in testResult)
            {
                switch (_comboBoxValue)
                {
                    case "L":
                        dataList.Add(tr.UpperL);
                        break;
                    case "C":
                        dataList.Add(tr.UpperC);
                        break;
                    case "H":
                        dataList.Add(tr.UpperH);
                        break;
                    case "a":
                        dataList.Add(tr.LowerA);
                        break;
                    case "b":
                        dataList.Add(tr.LowerB);
                        break;
                    case "Y":
                        dataList.Add(tr.UpperY);
                        break;
                    case "x":
                        dataList.Add(tr.LowerX);
                        break;
                    case "y":
                        dataList.Add(tr.LowerY);
                        break;
                    case "R":
                        dataList.Add(tr.UpperR);
                        break;
                    case "G":
                        dataList.Add(tr.UpperG);
                        break;
                    case "B":
                        dataList.Add(tr.UpperB);
                        break;
                    default:
                        break;
                }
            }

            return dataList;
        }

        private double getDataFromField(TestResult testResult)
        {
            switch (_comboBoxValue)
            {
                case "L":
                    return testResult.UpperL;
                    
                case "C":
                    return testResult.UpperC;
                    
                case "H":
                    return testResult.UpperH;
                    
                case "a":
                    return testResult.LowerA;
                    
                case "b":
                    return testResult.LowerB;
                    
                case "Y":
                    return testResult.UpperY;
                    
                case "x":
                    return testResult.LowerX;
                    
                case "y":
                    return testResult.LowerY;
                    
                case "R":
                    return testResult.UpperR;
                    
                case "G":
                    return testResult.UpperG;
                    
                case "B":
                    return testResult.UpperB;
                    
                default:
                    break;
            }
            

            return 0;
        }

        #region Save data and chart
        private void saveToExcelBtn_Click(object sender, RoutedEventArgs e)
        {
            if(listViewData.Items.Count == 0)
            {
                MessageBox.Show(Constants.NO_SELECTED_DATA);
                return;
            }

            bool ret = ExcelUtil.ExcelUtil.ExportToExcel(listViewData);
            if (ret)
            {
                MessageBox.Show(Constants.SAVE_DATA_TO_EXCEL_PASS);
            }
        }

        private void saveImage()
        {
            System.Windows.Forms.SaveFileDialog saveDlg = new System.Windows.Forms.SaveFileDialog()
            {
                CheckFileExists = false,
                CheckPathExists = true,
                Filter = "JPG 文件 (*.jpg)|*.jpg"
            };

            if (saveDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                renderChartToImage(saveDlg.FileName);
            }
        }

        private void saveColumnToJpgBtn_Click(object sender, RoutedEventArgs e)
        {
            saveImage();
        }

        private void renderChartToImage(string fileName)
        {
            try
            {
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                                                      (int)resultChart.ActualWidth,
                                                      (int)resultChart.ActualHeight,
                                                      96d,
                                                      96d,
                                                      PixelFormats.Pbgra32);
                renderBitmap.Render(resultChart);

                // Create a file stream for saving image
                using (FileStream outStream = new FileStream(fileName, FileMode.Create))
                {
                    // Use png encoder for our data
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    // push the rendered bitmap to it
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    // save the data to the stream
                    encoder.Save(outStream);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        #endregion

        private void selectAllCheckBox_click(object sender, RoutedEventArgs e)
        {
            if (selectAllCheckBox.IsChecked.HasValue && selectAllCheckBox.IsChecked.Value)
            {
                listViewData.SelectAll();
            }
            else
            {
                listViewData.UnselectAll();
            }
        }

        private void selectReverseCheckBox_click(object sender, RoutedEventArgs e)
        {
            int itemCount = listViewData.Items.Count;
            int index = 0;
            foreach (var item in listViewData.Items)
            {
                if(index == itemCount - 1)
                {
                    _reverseSelectionDone = true;
                }
                else
                {
                    _reverseSelectionDone = false;
                }

                if (listViewData.SelectedItems.Contains(item))
                {
                    listViewData.SelectedItems.Remove(item);
                }
                else
                {
                    listViewData.SelectedItems.Add(item);
                }

                index++;
            }
        }

        private void highPreciousComboBox_Closed(object sender, EventArgs e)
        {
            if((highPreciousComboBox.SelectedItem as ComboBoxItem).Tag.ToString().Trim() == "NORMAL")
            {
                _groupValue = 6;
                calculateGaussInThread();
                calculateColumnDataInThread();
            }
            else
            {
                _groupValue = 12;
                calculateGaussInThread();
                calculateColumnDataInThread();
            }
        }

        private bool showHideGroupBox(int selectedCount)
        {
            if (selectedCount == 0)
            {
                calculationGroupBox.Visibility = Visibility.Hidden;
                chartGroupBox.Visibility = Visibility.Hidden;
                dataGroupBox.Visibility = Visibility.Hidden;

                return false;
            }
            else
            {
                dataGroupBox.Visibility = Visibility.Visible;
                chartGroupBox.Visibility = Visibility.Visible;
                calculationGroupBox.Visibility = Visibility.Visible;

                return true;
            }
        }

        private void listViewData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_reverseSelectionDone == false)
            {
                return;
            }

            int selectedCount = listViewData.SelectedItems.Count;

            if (showHideGroupBox(selectedCount) == false)
            {
                return;
            }

            _selection.Clear();

            _selection = new List<TestResult>();
            foreach (TestResult tr in listViewData.SelectedItems)
            {
                _selection.Add(tr);
            }

            _calculaionResults.Clear();

            startCalculationThread();
            
            TestResult lastTr = listViewData.SelectedItems[selectedCount - 1] as TestResult;
            if (lastTr != null)
            {
                Thread queryThread = new Thread(() => queryDataInThread(lastTr));
                queryThread.IsBackground = true;
                queryThread.Start();
            }
        }

        private void queryDataInThread(TestResult tr)
        {
            //List<SearchParam> sp = new List<SearchParam>();
            //sp.Add(new SearchParam { Field = "TaskID", Compare = "=", Value = tr.ID.ToString() });
            //List<TestTask> tasks = DatabaseConnection.QueryFromTask(sp);
            //Dispatcher.Invoke(updateTaskListviewDelegate, tasks);

            List<double> refResults = new List<double>();
            bool ret = DatabaseConnection.QueryRefResultTables(tr.ID, tr.TestID, ref refResults);
            if (ret)
            {
                Dispatcher.Invoke(updatePicDelegate, refResults);
            }
        }

        private void UpdatePic(List<double> energyArray)
        {
            ObservableCollection<KeyValuePair<double, double>> lineList = new ObservableCollection<KeyValuePair<double, double>>();

            double maxEnergy = -1.0;
            for (int i = 0; i < energyArray.Count(); i++)
            {
                //FIXME
                //lineList.Add(new KeyValuePair<double, double>(i, energyArray[i]));//TODO
                lineList.Add(new KeyValuePair<double, double>(Spectrograph.Spectrograph.WavelengthArray[i], energyArray[i]));
                if(Spectrograph.Spectrograph.WavelengthArray[i] >= 400 && Spectrograph.Spectrograph.WavelengthArray[i] <= 760)
                { 
                    if(energyArray[i] > maxEnergy)
                    {
                        maxEnergy = energyArray[i];
                    }
                }
            }

            //查询结果界面光谱图的Y轴最大值根据横坐标400-700对应的所有Y值的最大值*1..05
            energyAxis.Maximum = maxEnergy * Constants.Y_AXIS_SCOPE_FACTOR;

            //刷新光谱图
            TestingChart.Series.Clear();

            LineSeries ls = new LineSeries();
            ls.IndependentValueBinding = new System.Windows.Data.Binding("Key");//X轴
            ls.DependentValueBinding = new System.Windows.Data.Binding("Value");//Y轴
            ls.DataPointStyle = Application.Current.Resources["MyLineDataPointStyle"] as Style;
            ls.ItemsSource = lineList;

            TestingChart.Series.Add(ls);

            
        }
    }
}
