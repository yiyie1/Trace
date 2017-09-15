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
using System.Threading;
using ExcelUtil;
using Trace.Model;
using DataBase;
using System.Windows.Controls.DataVisualization.Charting;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Trace
{
    /// <summary>
    /// Interaction logic for ResultWindow.xaml
    /// </summary>
    public partial class ResultWindow : Window
    {
        public static ObservableCollection<TestResult> testResultList = new ObservableCollection<TestResult>();

        private static int BatchCount = AjustWindow.ajustParamList.Count;

        //提示用户调整光阑的委托
        private delegate void ImformUserModifyStop(int stop);
        private ImformUserModifyStop imformUserModifyStop;

        //填写日志信息
        private delegate void LogDelegate(String log, bool isBold);
        private LogDelegate logDelegate;

        //刷新表格内容
        private delegate void UpdateListViewDelegate();
        private UpdateListViewDelegate updateListViewDelegate;

        //弹出的提示窗口
        public static MsgBox msgBox;

        //反射光谱数据
        private double[] energyArray;

        //更新图片的委托
        delegate void UpdatePicDelegate();
        private UpdatePicDelegate updatePicDelegate;

        //刷新二维地图的委托
        delegate void RefreshMapDelegate();
        private RefreshMapDelegate refreshMapDelegate;

        private bool isBtnSaveDataBaseEnable = false;
        private bool isBtnExportExcelEnable = false;

        delegate void RefreshBtnEnableDelegate();
        private RefreshBtnEnableDelegate refreshBtnEnableDelegate;

        public ResultWindow()
        {
            InitializeComponent();

            imformUserModifyStop = new ImformUserModifyStop(ImformUserToModifyStopMethod);
            logDelegate = new LogDelegate(WriteLog);
            updatePicDelegate = new UpdatePicDelegate(UpdatePicMethod);
            updateListViewDelegate = new UpdateListViewDelegate(UpdateListView);
            refreshMapDelegate = new RefreshMapDelegate(RefreshMap);
            refreshBtnEnableDelegate = new RefreshBtnEnableDelegate(RefreshBtnEnable);

            this.Closed += ResultWindow_Closed;
        }

        private void ResultWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //初始化测试结果信息
            InitTestResultList();

            //初始化二维地图
            InitChartMap();

            //启动后台线程开始测量和刷新界面
            Thread thread = new Thread(StartTestInThread);
            thread.IsBackground = true;
            thread.Start();
        }

        private void StartTestInThread()
        {
            int stop = -1;

            //按批次测试
            for (int batchIndex = 0; batchIndex < BatchCount; batchIndex++)
            {
                foreach (TestResult testResult in testResultList)
                {
                    if (testResult.BatchIndex == batchIndex)
                    {
                        //提示用户调整光阑
                        if (testResult.Stop != stop)
                        {
                            this.Dispatcher.Invoke(imformUserModifyStop, testResult.Stop);
                        }
                        stop = testResult.Stop;

                        //需要刷新二维地图信息
                        testResult.TestState = TestStateEnum.TESTING;
                        this.Dispatcher.BeginInvoke(refreshMapDelegate);

                        //给PLC发送移动命令，并移动到测量位置
                        MovePlc(testResult);
                        
                        //测量该点
                        double[] samplePixels = Spectrograph.Spectrograph.GetRefPixels(testResult.IntegrationTime_ms * 1000, testResult.Average, testResult.Smooth);
                        if (samplePixels == null || samplePixels.Count() <= 0)
                        {
                            return;
                        }

                        //计算反射光谱
                        Trace.Util.CalculateClass calculateObject = new Util.CalculateClass(testResult.Refrence, testResult.Dark, samplePixels
                            , testResult.ObserveAngle, testResult.Illuminant);
                        energyArray = calculateObject.GetReflection();

                        for (int i = 0; i < energyArray.Count(); i++)
                        {
                            testResult.EnergyArray[i] = energyArray[i];
                        }

                        //刷新至光谱图
                        this.Dispatcher.Invoke(updatePicDelegate);

                        //计算LCH等值
                        calculateObject.ComputeParams(Spectrograph.Spectrograph.WavelengthArray, energyArray);
                        testResult.UpperL = calculateObject.UpperL;
                        testResult.UpperC = calculateObject.UpperC;
                        testResult.UpperH = calculateObject.UpperH;
                        testResult.LowerA = calculateObject.LowerA;
                        testResult.LowerB = calculateObject.LowerB;
                        testResult.UpperY = calculateObject.UpperY;
                        testResult.LowerX = calculateObject.LowerX;
                        testResult.LowerY = calculateObject.LowerY;
                        testResult.UpperR = calculateObject.UpperR;
                        testResult.UpperG = calculateObject.UpperG;
                        testResult.UpperB = calculateObject.UpperB;

                        //刷新二维地图
                        testResult.TestState = TestStateEnum.TESTED;
                        this.Dispatcher.BeginInvoke(refreshMapDelegate);

                        this.Dispatcher.Invoke(updateListViewDelegate);

                        this.Dispatcher.Invoke(logDelegate, "坐标：" + "(" + testResult.PositionX
                                    + "," + testResult.PositionY + ") 角度：" + testResult.MeasureAngle + "  测试完成", false);
                    }
                }
            }

            listViewData.SelectionChanged += listViewData_SelectionChanged;

            this.Dispatcher.Invoke(logDelegate, "全部测试完成", true);

            //设置按键是否可用
            isBtnSaveDataBaseEnable = true;
            isBtnExportExcelEnable = true;
            this.Dispatcher.Invoke(refreshBtnEnableDelegate);
        }

        private void RefreshBtnEnable()
        {
            this.BtnSaveDatabase.IsEnabled = isBtnSaveDataBaseEnable;
            this.BtnExportExcel.IsEnabled = isBtnExportExcelEnable;
        }

        void listViewData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            energyArray = testResultList[listViewData.SelectedIndex].EnergyArray;

            //刷新反射光谱图
            this.Dispatcher.Invoke(updatePicDelegate);
        }

        private void UpdateListView()
        {
            this.listViewData.Items.Refresh();
        }

        void UpdatePicMethod()
        {
            UpdatePic(energyArray);
        }

        private void UpdatePic(double[] energyArray)
        {
            ObservableCollection<KeyValuePair<double, double>> lineList = new ObservableCollection<KeyValuePair<double, double>>();

            double maxEnergyArray = 0;
            bool scopeOut = false;
            for (int i = 0; i < energyArray.Count(); i++)
            {
                //生成显示结果
                lineList.Add(new KeyValuePair<double, double>(Spectrograph.Spectrograph.WavelengthArray[i], energyArray[i]));

                //获取最大值，对Y轴范围进行自适应
                if (scopeOut)
                {
                    continue;
                }

                if (Spectrograph.Spectrograph.WavelengthArray[i] > 400 && Spectrograph.Spectrograph.WavelengthArray[i] < 760)
                {
                    if (energyArray[i] > maxEnergyArray)
                    {
                        maxEnergyArray = energyArray[i];
                    }
                }
                else if (Spectrograph.Spectrograph.WavelengthArray[i] >= 760)
                {
                    scopeOut = true;
                }
            }

            YAxis.Maximum = maxEnergyArray * (Constants.Y_AXIS_SCOPE_FACTOR);

            //刷新光谱图
            TestingChart.Series.Clear();

            LineSeries ls = new LineSeries();
            ls.IndependentValueBinding = new System.Windows.Data.Binding("Key");//X轴
            ls.DependentValueBinding = new System.Windows.Data.Binding("Value");//Y轴
            ls.DataPointStyle = Application.Current.Resources["MyLineDataPointStyle"] as Style;
            ls.ItemsSource = lineList;

            TestingChart.Series.Add(ls);
        }

        //控制PLC进行移动并
        private void MovePlc(TestResult testResult)
        {
            this.Dispatcher.Invoke(logDelegate, "正在调整测量位置至坐标：" + "(" + testResult.PositionX
                                    + "," + testResult.PositionY + ") 角度：" + testResult.MeasureAngle, false);

            Plc.Plc.ControlPlc(testResult.PositionX, testResult.PositionY, (UInt32)testResult.MeasureAngle);
            this.Dispatcher.Invoke(logDelegate, "调整完成", false);
        }

        private void WriteLog(String log, bool isBold)
        {
            var p = new Paragraph();
            var r = new Run(log);
            p.Inlines.Add(r);

            if (isBold)
            {
                p.FontWeight = FontWeights.Bold;
            }
            
            p.LineHeight = 1;
            LogDoc.Blocks.Add(p);

            rtbLog.ScrollToEnd();
        }

        //提示用户去修改光阑
        private void ImformUserToModifyStopMethod(int stop)
        {
            msgBox = new MsgBox("提示", Constants.MODIFY_STOP1 + stop + Constants.MODIFY_STOP2, MessageBoxButton.OK);
            msgBox.Owner = this;
            msgBox.ShowDialog();
        }

        private void InitChartMap()
        {
            ChartMap.Series.Clear();
            
            ObservableCollection<KeyValuePair<int, int>> pointList = new ObservableCollection<KeyValuePair<int, int>>();
            foreach (ProjectParam projectParam in ProjectParamWindow.ParametersList)
            {
                pointList.Add(new KeyValuePair<int, int>(projectParam.PositionX, projectParam.PositionY));
            }

            ScatterSeries ss = new ScatterSeries();

            ss.IndependentValueBinding = new System.Windows.Data.Binding("Key");//X轴
            ss.DependentValueBinding = new System.Windows.Data.Binding("Value");//Y轴
            ss.ItemsSource = pointList;
            ss.DataPointStyle = Application.Current.Resources["ScatterGrayPointStyle"] as Style; 

            ChartMap.Series.Add(ss);
        }

        private void RefreshMap()
        {
            ChartMap.Series.Clear();

            ObservableCollection<KeyValuePair<int, int>> noTestPointList = new ObservableCollection<KeyValuePair<int, int>>();
            ObservableCollection<KeyValuePair<int, int>> testingPointList = new ObservableCollection<KeyValuePair<int, int>>();
            ObservableCollection<KeyValuePair<int, int>> testedPointList = new ObservableCollection<KeyValuePair<int, int>>();

            foreach (TestResult testResult in testResultList)
            {
                if (testResult.TestState == TestStateEnum.NO_TEST)
                {
                    noTestPointList.Add(new KeyValuePair<int, int>(testResult.PositionX, testResult.PositionY));
                }
                else if (testResult.TestState == TestStateEnum.TESTING)
                {
                    testingPointList.Add(new KeyValuePair<int, int>(testResult.PositionX, testResult.PositionY));
                }
                else if (testResult.TestState == TestStateEnum.TESTED)
                {
                    testedPointList.Add(new KeyValuePair<int, int>(testResult.PositionX, testResult.PositionY));
                }
            }

            //未测试点
            ScatterSeries noTestScatterSeries = new ScatterSeries();
            noTestScatterSeries.IndependentValueBinding = new System.Windows.Data.Binding("Key");//X轴
            noTestScatterSeries.DependentValueBinding = new System.Windows.Data.Binding("Value");//Y轴
            noTestScatterSeries.ItemsSource = noTestPointList;
            noTestScatterSeries.DataPointStyle = Application.Current.Resources["ScatterGrayPointStyle"] as Style;
            ChartMap.Series.Add(noTestScatterSeries);

            //测试中的点
            ScatterSeries testingScatterSeries = new ScatterSeries();
            testingScatterSeries.IndependentValueBinding = new System.Windows.Data.Binding("Key");//X轴
            testingScatterSeries.DependentValueBinding = new System.Windows.Data.Binding("Value");//Y轴
            testingScatterSeries.ItemsSource = testingPointList;
            testingScatterSeries.DataPointStyle = Application.Current.Resources["ScatterYellowPointStyle"] as Style;
            ChartMap.Series.Add(testingScatterSeries);

            //测试完的点
            ScatterSeries testedScatterSeries = new ScatterSeries();
            testedScatterSeries.IndependentValueBinding = new System.Windows.Data.Binding("Key");//X轴
            testedScatterSeries.DependentValueBinding = new System.Windows.Data.Binding("Value");//Y轴
            testedScatterSeries.ItemsSource = testedPointList;
            testedScatterSeries.DataPointStyle = Application.Current.Resources["ScatterGreenPointStyle"] as Style;
            ChartMap.Series.Add(testedScatterSeries);
        }

        private void InitTestResultList()
        {
            foreach (ProjectParam projectParam in ProjectParamWindow.ParametersList)
            {
                TestResult testResult = new TestResult();

                testResult.TestState = TestStateEnum.NO_TEST;

                testResult.ID = projectParam.OperID;
                testResult.PositionX = projectParam.PositionX;
                testResult.PositionY = projectParam.PositionY;
                testResult.MeasureAngle = projectParam.MeasureAngle;
                testResult.Stop = projectParam.Stop;

                testResult.ObserveAngle = NewProjectWindow.TestTask.ObserveAngle;
                testResult.Illuminant = NewProjectWindow.TestTask.LightSource;

                int batchIndex = 0;
                foreach (AjustParam ajustParam in AjustWindow.ajustParamList)
                {
                    testResult.BatchIndex = batchIndex;
                    if (ajustParam.Angle == projectParam.MeasureAngle && ajustParam.Stop == projectParam.Stop)
                    {
                        testResult.IntegrationTime_ms = ajustParam.IntegrationTime_us / 1000;
                        testResult.Average = ajustParam.Average;
                        testResult.Smooth = ajustParam.Smooth;

                        for (int i = 0; i < testResult.Refrence.Count(); i++)
                        {
                            testResult.Refrence[i] = ajustParam.Refrence[i];
                        }

                        for (int i = 0; i < testResult.Dark.Count(); i++)
                        {
                            testResult.Dark[i] = ajustParam.Dark[i];
                        }

                        break;
                    }

                    batchIndex++;
                }

                testResultList.Add(testResult);
            }

            this.listViewData.ItemsSource = testResultList;
        }

        void ResultWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void SaveExcelBtn_Click(object sender, RoutedEventArgs e)
        {
            ExcelUtil.ExcelUtil.ExportToExcel(listViewData);
        }

        private void SaveDatabaseBtn_Click(object sender, RoutedEventArgs e)
        {
            //保存任务数据
            int taskID = DatabaseConnection.SaveTaskToDatabase(NewProjectWindow.TestTask);
            if (taskID == -1)
            {
                MessageBox.Show("任务信息数据保存失败，停止保存任务结果数据");
                return;
            }
            
            //保存运算结果数据
            bool res1 = DatabaseConnection.SaveResultToDatabase(testResultList, taskID);
            if (!res1)
            {
                MessageBox.Show("任务结果数据保存失败，停止保存任务结果数据");
                return;
            }

            //保存反射率数据
            bool res2 = DatabaseConnection.SaveRefsToDataBase(testResultList, taskID);
            if (!res2)
            {
                MessageBox.Show("反射率数据保存失败，停止保存任务结果数据");
                return;
            }

            MsgBox msgBox = new MsgBox(Constants.TIP, "测试结果存入数据库成功", MessageBoxButton.OK);
            msgBox.Owner = this;
            msgBox.ShowDialog();
        }
    }

    public partial class StylePalette : Collection<Style>
    {
        public StylePalette() { }
    }
}
