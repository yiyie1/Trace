using System;
using System.Collections.Generic;
using System.Collections;
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
using System.Collections.ObjectModel;
using System.Windows.Controls.DataVisualization.Charting;
using Trace.Model;
using System.ComponentModel;
using System.Windows.Controls.DataVisualization.Charting;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;
using DataBase;
using Trace.View.Result;
using Trace.View.Setup;
using System.Windows.Threading;

namespace Trace
{
    /// <summary>
    /// MainWindowNew.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindowNew : Window
    {

        bool isSpectrographConnected = true;
        bool isPlcStateOk = true;

        Model.ProjectParam currentMeasureParam = new Model.ProjectParam();

        //是否已经点击过确认按钮
        bool isMeasureParamConfirmed = false;

        SPAM.CCoSpectralMath spectralMath;
        SPAM.CCoCIEConstants cieConstants;
        SPAM.CCoCIEObserver cieObserver;
        SPAM.CCoIlluminant illuminant;

        //当前测量光谱
        private double[] pixels;

        //补偿后的光谱
        private double[] pixels_Compensated = new double[Constants.PIXELS_COUNT];

        //测量参数
        private int currentIntegrationTime = Constants.DEFAULT_INTEGRATION_TIME;
        private int currentAverage = Constants.DEFAULT_AVRAGE_TIMES;
        private int currentBoxcarWidth = Constants.DEFAULT_BOXCAR_WIDTH;

        //弹出的提示窗口
        public static MsgBox msgBox;

        //打开提示窗口的委托
        private delegate void OpenMsgBox();
        private OpenMsgBox openMsgBox;

        //关闭提示窗口的委托
        private delegate void CloseMsgBox();
        private CloseMsgBox closeMsgBox;

        //更新标准光谱图的委托
        delegate void UpdateNormalPicDelegate();
        private UpdateNormalPicDelegate updateNormalChartDelegate;

        //设置单次扫描按钮无法点击的委托
        delegate void SetScanOnceBtnEnableDelegate();
        private SetScanOnceBtnEnableDelegate setScanOnceBtnEnableDelegate;

        //设置测试按钮无法点击的委托
        delegate void SetMeasureBtnEnableDelegate();
        private SetMeasureBtnEnableDelegate setMeasureBtnEnableDelegate;

        //光谱仪状态lable不可见
        delegate void SetLableStateInVisibleDelegate();
        private SetLableStateInVisibleDelegate setLableStateInVisibleDelegate;

        //更新图片的委托
        delegate void UpdateRelChartDelegate();
        private UpdateRelChartDelegate updateRelChartDelegate;

        //校准的参数
        public static List<AjustParam> ajustParamList = new List<AjustParam>();

        //参考光谱
        private double[] refPixels;

        //是否已经获取参考光谱
        bool isRefPixelsExist = false;

        //暗光谱
        private double[] darkPixels;

        //是否存在暗光谱
        bool isDarkPixelsExist = false;

        //反射光谱
        private double[] energyArray;

        //补偿后的反射光谱
        private double[] energyArray_Compensated = new double[Constants.PIXELS_COUNT];

        //观察角度
        String strAngle = String.Empty;
        
        //照明光源
        String strLight = String.Empty;

        //是否使用补偿参数进行补偿
        private bool isCompensated = false;

        //测试结果
        public static ObservableCollection<TestResult> testResultList = new ObservableCollection<TestResult>();

        //刷新表格内容
        private delegate void UpdateListViewDelegate();
        private UpdateListViewDelegate updateListViewDelegate;

        public List<TestResult> searchResults;

        int minXScope = Constants.DEFAULT_MIN_X_AXIS_VALUE;
        int maxXScope = Constants.DEFAULT_MAX_X_AXIS_VALUE;

        //是否在校准位置
        private bool isInAjustPos = false;

        //测量方式
        public enum TestMode
        {
            TEST_WITH_PLC,      //使用下位机硬件移动位置进行测量
            TEST_WITHOUT_PLC,   //手动移动位置进行测量
            NO_TEST             //无法进行测量
        }

        //判断是否获取并使用了之前测试所使用到的参考光谱、暗光谱、积分时间、平均次数、平滑度
        bool isGetMeasuredParams = false;

        public static TestMode testMode;

        //积分时间按住按钮递增Timer
        DispatcherTimer timer2IncreaseTime = new DispatcherTimer();
        DispatcherTimer timerOfIncreasingTime = new DispatcherTimer();

        DispatcherTimer timer2DecreaseTime = new DispatcherTimer();
        DispatcherTimer timerOfDecreasingTime = new DispatcherTimer();

        //平均次数按住按钮递增Timer
        DispatcherTimer timer2IncreaseAverage = new DispatcherTimer();
        DispatcherTimer timerOfIncreasingAverage = new DispatcherTimer();

        DispatcherTimer timer2DecreaseAverage = new DispatcherTimer();
        DispatcherTimer timerOfDecreasingAverage = new DispatcherTimer();

        //平滑度按住按钮递增Timer
        DispatcherTimer timer2IncreaseBoxcarWidth = new DispatcherTimer();
        DispatcherTimer timerOfIncreasingBoxcarWidth = new DispatcherTimer();

        DispatcherTimer timer2DecreaseBoxcarWidth = new DispatcherTimer();
        DispatcherTimer timerOfDecreasingBoxcarWidth = new DispatcherTimer();

        //已保存的测试结果Index的List
        List<int> savedTestIdList = new List<int>();

        public MainWindowNew()
        {
            testMode = TestMode.TEST_WITH_PLC;

            InitializeComponent();

            spectralMath = new SPAM.CCoSpectralMath();
            cieConstants = spectralMath.createCIEConstantsObject();

            this.Loaded += MainWindowNew_Loaded;

            //新建普通测量
            mINewNormalMeasure.Click += mINewNormalMeasure_Click;

            //退出账户登入
            mIExitUser.Click += mIExitUser_Click;

            //退出软件
            mIExit.Click += mIExit_Click;

            //存储参考光谱
            mISaveRefPixels.Click += mISaveRefPixels_Click;

            //存储暗光谱
            mISaveDarkPixels.Click += mISaveDarkPixels_Click;

            //显示标准光谱
            mINormalChart.Click += BtnNormalChart_Click;

            //显示反射光谱
            mIRelectivityChart.Click += BtnReflectChart_Click;

            //自检
            mIHome.Click += mIHome_Click;

            //管理员功能
            mIAdminFuction.Click += mIAdminFuction_Click;

            //导入补偿参数
            mIInputCompensate.Click += mIInputCompensate_Click;

            //导出补偿参数
            mIOutputCompensate.Click += mIOutputCompensate_Click;

            //积分时间
            btnIncreaseTime.PreviewMouseDown += btnIncreaseTime_PreviewMouseDown;
            btnIncreaseTime.PreviewMouseUp += btnIncreaseTime_PreviewMouseUp;
            btnDecreaseTime.PreviewMouseDown += btnDecreaseTime_PreviewMouseDown;
            btnDecreaseTime.PreviewMouseUp += btnDecreaseTime_PreviewMouseUp;

            //平均次数
            btnIncreaseAverage.PreviewMouseDown += btnIncreaseAverage_PreviewMouseDown;
            btnIncreaseAverage.PreviewMouseUp += btnIncreaseAverage_PreviewMouseUp;
            btnDecreaseAverage.PreviewMouseDown += btnDecreaseAverage_PreviewMouseDown;
            btnDecreaseAverage.PreviewMouseUp += btnDecreaseAverage_PreviewMouseUp;

            //平滑度
            btnIncreaseBoxcarWidth.PreviewMouseDown += btnIncreaseBoxcarWidth_PreviewMouseDown;
            btnIncreaseBoxcarWidth.PreviewMouseUp += btnIncreaseBoxcarWidth_PreviewMouseUp;
            btnDecreaseBoxcarWidth.PreviewMouseDown += btnDecreaseBoxcarWidth_PreviewMouseDown;
            btnDecreaseBoxcarWidth.PreviewMouseUp += btnDecreaseBoxcarWidth_PreviewMouseUp;

            CbUnit.SelectionChanged += CbUnit_SelectionChanged;

            cBIsOpenLight.Click += cBIsOpenLight_Click;

            //确认测量参数
            BtnConfirm.Click += BtnConfirm_Click;

            //移动到校准位置
            BtnMoveToAjustPos.Click += BtnMoveToAjustPos_Click;

            //移动到测量位置
            BtnMoveToMeasurePos.Click += BtnMoveToMeasurePos_Click;

            //单次扫描
            BtnScanOnce.Click += BtnScanOnce_Click;

            //保存参考光谱
            BtnSaveRef.Click += BtnSaveRef_Click;

            //保存暗光谱
            BtnSaveDark.Click += BtnSaveDark_Click;

            //显示标准光谱
            BtnNormalChart.Click += BtnNormalChart_Click;

            //显示反射光谱
            BtnReflectChart.Click += BtnReflectChart_Click;

            //点击补偿系数checkbox
            cBIsCompensated.Click += cBIsCompensated_Click;

            //测量
            BtnMeasure.Click += BtnMeasure_Click;
            BtnMeasure2.Click += BtnMeasure_Click;

            //全选
            checkBoxSelAll.Click += checkBoxSelAll_Click;

            //保存到excel
            BtnData2Excel.Click += BtnData2Excel_Click;

            //保存到数据库
            BtnData2Database.Click += BtnData2Database_Click;

            //清空测量数据
            BtnClearData.Click += BtnClearData_Click;

            this.Closed += MainWindowNew_Closed;

            openMsgBox = new OpenMsgBox(OpenMsgBoxMethod);
            closeMsgBox = new CloseMsgBox(CloseMsgWin);

            updateNormalChartDelegate = new UpdateNormalPicDelegate(UpdateNormalChartMethod);
            updateRelChartDelegate = new UpdateRelChartDelegate(UpdateRelChartMethod);
            setScanOnceBtnEnableDelegate = new SetScanOnceBtnEnableDelegate(SetScanOnceBtnEnableMethod);
            setLableStateInVisibleDelegate = new SetLableStateInVisibleDelegate(SetLableStateInVisibleMethod);
            setMeasureBtnEnableDelegate = new SetMeasureBtnEnableDelegate(SetMeasureBtnEnableMethod);

            listViewData.ItemsSource = testResultList;

            updateListViewDelegate = new UpdateListViewDelegate(UpdateListView);

            //积分时间
            timer2IncreaseTime.Tick += timer2IncreaseTime_Tick;
            timer2IncreaseTime.Interval = new TimeSpan(Constants.INTERVEL_TO_CHANGE_100NS);

            timerOfIncreasingTime.Tick += timerOfIncreasingTime_Tick;
            timerOfIncreasingTime.Interval = new TimeSpan(Constants.INVERVAL_OF_CHANGING_100NS);

            timer2DecreaseTime.Tick += timer2DecreaseTime_Tick;
            timer2DecreaseTime.Interval = new TimeSpan(Constants.INTERVEL_TO_CHANGE_100NS);

            timerOfDecreasingTime.Tick += timerOfDecreasingTime_Tick;
            timerOfDecreasingTime.Interval = new TimeSpan(Constants.INVERVAL_OF_CHANGING_100NS);

            //平均次数
            timer2IncreaseAverage.Tick += timer2IncreaseAverage_Tick;
            timer2IncreaseAverage.Interval = new TimeSpan(Constants.INTERVEL_TO_CHANGE_100NS);

            timerOfIncreasingAverage.Tick += timerOfIncreasingAverage_Tick;
            timerOfIncreasingAverage.Interval = new TimeSpan(Constants.INVERVAL_OF_CHANGING_100NS);

            timer2DecreaseAverage.Tick += timer2DecreaseAverage_Tick;
            timer2DecreaseAverage.Interval = new TimeSpan(Constants.INTERVEL_TO_CHANGE_100NS);

            timerOfDecreasingAverage.Tick += timerOfDecreasingAverage_Tick;
            timerOfDecreasingAverage.Interval = new TimeSpan(Constants.INVERVAL_OF_CHANGING_100NS);

            //平滑度
            timer2IncreaseBoxcarWidth.Tick += timer2IncreaseBoxcarWidth_Tick;
            timer2IncreaseBoxcarWidth.Interval = new TimeSpan(Constants.INTERVEL_TO_CHANGE_100NS);

            timerOfIncreasingBoxcarWidth.Tick += timerOfIncreasingBoxcarWidth_Tick;
            timerOfIncreasingBoxcarWidth.Interval = new TimeSpan(Constants.INVERVAL_OF_CHANGING_100NS);

            timer2DecreaseBoxcarWidth.Tick += timer2DecreaseBoxcarWidth_Tick;
            timer2DecreaseBoxcarWidth.Interval = new TimeSpan(Constants.INTERVEL_TO_CHANGE_100NS);

            timerOfDecreasingBoxcarWidth.Tick += timerOfDecreasingBoxcarWidth_Tick;
            timerOfDecreasingBoxcarWidth.Interval = new TimeSpan(Constants.INVERVAL_OF_CHANGING_100NS);
        }

        
        void MainWindowNew_Loaded(object sender, RoutedEventArgs e)
        {
            //初始化照明光源和观察角
            InitObserver();
            InitIlluminant();
            cieConstants.Dispose();
            spectralMath.Dispose();

            tBMinWaveLength.Text = Constants.DEFAULT_MIN_X_AXIS_VALUE.ToString();
            tBMaxWaveLength.Text = Constants.DEFAULT_MAX_X_AXIS_VALUE.ToString();

            //检查光谱仪和plc状态
            CheckState();

            SetIfEnblebToMeasure(false);

            UpdateStateToolBar();

            //设置是否开灯
            Spectrograph.Spectrograph.SetStrobeEnable((bool)cBIsOpenLight.IsChecked);

            //初始化图坐标轴的显示范围
            normalChartXAxis.Minimum = Constants.DEFAULT_MIN_X_AXIS_VALUE;
            normalChartXAxis.Maximum = Constants.DEFAULT_MAX_X_AXIS_VALUE;

            normalChartYAxis.Minimum = Constants.DEFAULT_NORMAL_CHART_MIN_Y_AXIS_VALUE;
            normalChartYAxis.Maximum = Constants.DEFAULT_NORMAL_CHART_MAX_Y_AXIS_VALUE;

            RelChartXAxis.Minimum = Constants.DEFAULT_MIN_X_AXIS_VALUE;
            RelChartXAxis.Maximum = Constants.DEFAULT_MAX_X_AXIS_VALUE;

            RelChartYAxis.Minimum = Constants.DEFAULT_RELECTIVITY_CHART_MIN_Y_AXIS_VALUE;
            RelChartYAxis.Maximum = Constants.DEFAULT_RELECTIVITY_CHART_MAX_Y_AXIS_VALUE;

            //管理员功能按钮只有管理员才能使用
            /*if (App.UserRole == 0)
            {
                mIAdminFuction.Visibility = Visibility.Visible;
                mICompensate.Visibility = Visibility.Visible;
            }
            else
            {
                mIAdminFuction.Visibility = Visibility.Collapsed;
                mICompensate.Visibility = Visibility.Collapsed;
            }*/

            isCompensated = cBIsCompensated.IsChecked.Value;
        }

        #region 提示窗口控制

        private void ShowMsgBox(String msgBoxTitle, String msgBoxTip, MessageBoxButton msgBoxBtn, bool isNoControl)
        {
            MsgBox.msgBoxTitle = msgBoxTitle;
            MsgBox.msgBoxTip = msgBoxTip;
            MsgBox.msgBoxBtn = msgBoxBtn;
            MsgBox.isNoControl = isNoControl;

            this.Dispatcher.BeginInvoke(openMsgBox);
        }

        //打开提示窗口
        public void OpenMsgBoxMethod()
        {
            msgBox = new MsgBox(MsgBox.msgBoxTitle, MsgBox.msgBoxTip, MsgBox.msgBoxBtn, MsgBox.isNoControl);
            msgBox.Owner = this;
            msgBox.ShowDialog();
        }

        //关闭当前打开的提示窗口
        public void CloseMsgWin()
        {
            if (null != msgBox && msgBox.Visibility == Visibility.Visible)
            {
                msgBox.Close();
            }
        }

        #endregion

        #region 初始化

        //初始化照明光源
        private void InitIlluminant()
        {
            illuminant = new SPAM.CCoIlluminant();

            int illuminantCount = cieConstants.getNumberOfIlluminants();
            int defaultLightIndex = -1;
            for (int i = 0; i < illuminantCount; i++)
            {
                illuminant = cieConstants.getIlluminantByIndex(i);
                cBLight.Items.Add(illuminant.toString());
                if (String.Compare(illuminant.toString(), Constants.DEFAULT_LIGHT, true) == 0)
                {
                    defaultLightIndex = i;
                }
            }

            if (defaultLightIndex >= 0)
            {
                cBLight.SelectedIndex = defaultLightIndex;
            }
            else
            {
                cBLight.SelectedIndex = 0;
            }
        }

        //初始化观察角
        private void InitObserver()
        {
            cieObserver = new SPAM.CCoCIEObserver();

            int observerCount = cieConstants.getNumberOfObservers();
            int defaultAngleIndex = -1;
            for (int i = 0; i < observerCount; i++)
            {
                cieObserver = cieConstants.getCIEObserverByIndex(i);

                if (String.Compare(cieObserver.toString(), Constants.DEFAULT_ANGLE, true) == 0)
                {
                    defaultAngleIndex = i;
                }
                cBAngle.Items.Add(cieObserver.toString());
            }

            if (defaultAngleIndex >= 0)
            {
                cBAngle.SelectedIndex = defaultAngleIndex;
            }
            else
            {
                cBAngle.SelectedIndex = 0;
            }
        }

        private void CheckState()
        {
            isSpectrographConnected = IsSpectrographConnected();
            if (!isSpectrographConnected)
            {
                MessageBox.Show("光谱仪通讯失败，请重新拔插光谱仪的USB接口进行排查");
                testMode = TestMode.NO_TEST;
            }

            isPlcStateOk = IsPlcStateOk();

            if (isPlcStateOk)
            {
                testMode = TestMode.TEST_WITH_PLC;
            }
            else
            {
                testMode = TestMode.TEST_WITHOUT_PLC;
            }
        }

        private void UpdateUI()
        {
            UpdateStateToolBar();

            if (testMode == TestMode.TEST_WITH_PLC)
            {
                SetIfEnblebToMeasure(isSpectrographConnected && isPlcStateOk);
                BtnSaveRef.IsEnabled = isMeasureParamConfirmed && isInAjustPos && isSpectrographConnected && isPlcStateOk;
                BtnSaveDark.IsEnabled = isMeasureParamConfirmed && isInAjustPos && isSpectrographConnected && isPlcStateOk;

                BtnMoveToAjustPos.IsEnabled = isMeasureParamConfirmed;
                BtnMoveToMeasurePos.IsEnabled = isMeasureParamConfirmed;
                sPNormalMeasureShortCutKeys2.IsEnabled = isMeasureParamConfirmed;

                BtnMeasure.IsEnabled = isRefPixelsExist && isDarkPixelsExist;
                BtnMeasure2.IsEnabled = isRefPixelsExist && isDarkPixelsExist;
            }
            else if (testMode == TestMode.TEST_WITHOUT_PLC)
            {
                sPNormalMeasureShortCutKeys1_1.IsEnabled = false;
                sPNormalMeasureShortCutKeys1_2.IsEnabled = true;
                sPNormalMeasureShortCutKeys1_3.IsEnabled = false;
                sPNormalMeasureShortCutKeys2.IsEnabled = false;
                this.sPShortCutKeys.IsEnabled = true;
                this.sPShortCutKey.IsEnabled = true;
                sPMeasureResultShortCutKeys.IsEnabled = true;
                BtnSaveRef.IsEnabled = true;
                BtnSaveDark.IsEnabled = true;
                listViewData.IsEnabled = true;
                NormalChart.IsEnabled = true;
                RelectivityChart.IsEnabled = true;
                listViewData.IsEnabled = true;
                
                StopBox.IsEnabled = true;
                lbStopDiameter.IsEnabled = true;

                BtnMoveToAjustPos.IsEnabled = false;
                BtnMoveToMeasurePos.IsEnabled = false;
                sPNormalMeasureShortCutKeys2.IsEnabled = true;

                BtnMeasure.IsEnabled = isRefPixelsExist && isDarkPixelsExist;
                BtnMeasure2.IsEnabled = isRefPixelsExist && isDarkPixelsExist;
            }
        }

        //更新下方状态栏的显示
        private void UpdateStateToolBar()
        {
            if (isSpectrographConnected)
            {
                lBSpecState.Background = Brushes.Green;
            }
            else
            {
                lBSpecState.Background = Brushes.Red;
            }

            if (isPlcStateOk)
            {
                lBPlcState.Background = Brushes.Green;
            }
            else
            {
                lBPlcState.Background = Brushes.Red;
            }

            if (isDarkPixelsExist)
            {
                lBDarkPixels.Background = Brushes.Green;
            }
            else
            {
                lBDarkPixels.Background = Brushes.Red;
            }

            if (isRefPixelsExist)
            {
                lBRefPixels.Background = Brushes.Green;
            }
            else
            {
                lBRefPixels.Background = Brushes.Red;
            }
        }

        //设置各快捷键栏是否可用
        private void SetIfEnblebToMeasure(bool isEnable)
        {
            this.sPShortCutKeys.IsEnabled = isEnable;
            this.sPShortCutKey.IsEnabled = isEnable;
            sPNormalMeasureShortCutKeys1_1.IsEnabled = isEnable;
            sPNormalMeasureShortCutKeys1_2.IsEnabled = isEnable;
            sPNormalMeasureShortCutKeys1_3.IsEnabled = isEnable;
            sPNormalMeasureShortCutKeys2.IsEnabled = isEnable;
            sPMeasureResultShortCutKeys.IsEnabled = isEnable;
            NormalChart.IsEnabled = isEnable;
            RelectivityChart.IsEnabled = isEnable;
            listViewData.IsEnabled = isEnable;
        }

        private bool IsSpectrographConnected()
        {
            if (App.isDebugWithoutPlc)
            {
                return true;
            }

            int spectroGraphCount = Spectrograph.Spectrograph.GetSpectrographCount();
            return spectroGraphCount >= 1;
        }

        private bool IsPlcConnected()
        {
            return Util.TcpThreadMgr.dictConn.Count > 0;
        }

        private bool IsPlcStateOk()
        {
            if (App.isDebugWithoutPlc)
            {
                return true;
            }

            // 检查通信状态
            if (!IsPlcConnected())
            {
                MessageBox.Show("控制器通信失败，请查看控制器电源是否开启");
                return false;
            }

            //检查各轴的状态
            Plc.Plc.STATESTRUCT state = Plc.Plc.CheckState();

            bool isXok = (state.X_STAT != (UInt16)Plc.Plc.PLCState.AXIS_ERROR)
                && (state.X_STAT != (UInt16)Plc.Plc.PLCState.AXIS_OVERTIME);
            bool isYok = (state.Y_STAT != (UInt16)Plc.Plc.PLCState.AXIS_ERROR)
                && (state.Y_STAT != (UInt16)Plc.Plc.PLCState.AXIS_OVERTIME);
            bool isMok = (state.M_STAT != (UInt16)Plc.Plc.PLCState.AXIS_ERROR)
                && (state.M_STAT != (UInt16)Plc.Plc.PLCState.AXIS_OVERTIME);

            bool isPlcOk = isXok && isYok && isMok;
            if (!isPlcOk)
            {
                MessageBox.Show("控制器存在故障，请查看下位机状态");
                return false;
            }

            return true;
        }

        #endregion

        #region 菜单键

        //新建普通测量
        private void mINewNormalMeasure_Click(object sender, RoutedEventArgs e)
        {

            var newTask = new NewTaskWindow();
            newTask.ShowDialog();
            /*if (App.isDebugWithoutPlc)
            {
                isSpectrographConnected = true;
            }

            if (!isSpectrographConnected)
            {
                MessageBox.Show("光谱仪通讯失败，无法新建普通测量任务");
                return;
            }

            if (App.isDebugWithoutPlc)
            {
                isPlcStateOk = true;
            }

            if (!isPlcStateOk)
            {
                MessageBox.Show("下位机无法使用，请手动调整测量");
            }

            currentMeasureParam.OperID = 0;
            isRefPixelsExist = false;
            isDarkPixelsExist = false;
            StopBox.SelectedIndex = 0;

            //如果使用PLC进行测量
            if (testMode == TestMode.TEST_WITH_PLC)
            {
                isMeasureParamConfirmed = false;

                tBXPos.Text = "0";
                tBYPos.Text = "0";
                tBAngle.Text = "80";
            }
            else if (testMode == TestMode.TEST_WITHOUT_PLC)
            {
                isMeasureParamConfirmed = true;

                tBXPos.Text = "0";
                tBYPos.Text = "0";
                tBAngle.Text = "0";
            }

            UpdateUI();

            testResultList.Clear();

            NormalChart.Series.Clear();
            RelectivityChart.Series.Clear();

            savedTestIdList.Clear();*/
        }

        private void mIExitUser_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mIExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void mISaveRefPixels_Click(object sender, RoutedEventArgs e)
        {
            BtnSaveRef_Click(null, null);
        }

        private void mISaveDarkPixels_Click(object sender, RoutedEventArgs e)
        {
            BtnSaveDark_Click(null, null);
        }

        #endregion

        #region 快捷键

        void CbUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbUnit.SelectedIndex == 0)
            {
                tBIntegral.Text = "100";
            }
            else if (CbUnit.SelectedIndex == 1)
            {
                tBIntegral.Text = "1";
            }
        }

        //扫描一次
        private void BtnScanOnce_Click(object sender, RoutedEventArgs e)
        {
            BtnScanOnce.IsEnabled = false;
            lBState.Visibility = Visibility.Visible;
            currentIntegrationTime = AjustWindow.GetIntegrationTime(CbUnit, tBIntegral);
            currentAverage = AjustWindow.GetAverage(tBAverTimes);
            currentBoxcarWidth = AjustWindow.GetBoxcarWidth(tBSmooth);

            NormalChart.Visibility = Visibility.Visible;
            RelectivityChart.Visibility = Visibility.Collapsed;

            minXScope = int.Parse(tBMinWaveLength.Text);
            maxXScope = int.Parse(tBMaxWaveLength.Text);

            Thread th = new Thread(InitPixelsAndUpdateNormalChart);
            th.IsBackground = true;
            th.Start();

            UpdateChartXAxisScope();
        }

        private void InitPixelsAndUpdateNormalChart()
        {
            pixels = Spectrograph.Spectrograph.GetPixels(currentIntegrationTime, currentAverage, currentBoxcarWidth);
            if (pixels == null || pixels.Count() <= 0)
            {
                return;
            }

            for (int i = 0; i < pixels.Count(); i++)
            {
                pixels_Compensated[i] = pixels[i];
            }
            Trace.Util.CalculateClass.Compensate(ref pixels_Compensated);

            this.Dispatcher.Invoke(updateNormalChartDelegate);

            this.Dispatcher.Invoke(setScanOnceBtnEnableDelegate);
            this.Dispatcher.Invoke(setLableStateInVisibleDelegate);
        }

        void UpdateNormalChartMethod()
        {
            if (pixels_Compensated == null || pixels == null)
            {
                return;
            }

            ObservableCollection<KeyValuePair<double, double>> lineList = new ObservableCollection<KeyValuePair<double, double>>();

            double[] localPixels;
            if (isCompensated)
            {
                localPixels = pixels_Compensated;
            }
            else
            {
                localPixels = pixels;
            }

            double maxPixel = 0;
            for (int i = 0; i < localPixels.Count(); i++)
            {
                lineList.Add(new KeyValuePair<double, double>(Spectrograph.Spectrograph.WavelengthArray[i], localPixels[i]));

                if (Spectrograph.Spectrograph.WavelengthArray[i] > minXScope && Spectrograph.Spectrograph.WavelengthArray[i] < maxXScope)
                {
                    if (localPixels[i] > maxPixel)
                    {
                        maxPixel = localPixels[i];
                    }
                }
            }

            normalChartYAxis.Maximum = maxPixel * (Constants.Y_AXIS_SCOPE_FACTOR);

            //刷新光谱图
            NormalChart.Series.Clear();

            LineSeries ls = new LineSeries();
            ls.IndependentValueBinding = new System.Windows.Data.Binding("Key");//X轴
            ls.DependentValueBinding = new System.Windows.Data.Binding("Value");//Y轴
            ls.DataPointStyle = Application.Current.Resources["MyLineDataPointStyle"] as Style;
            ls.ItemsSource = lineList;

            NormalChart.Series.Add(ls);
        }

        #region 增大减小按键

        //增加积分时间按钮
        private void btnIncreaseTime_PreviewMouseDown(object sender, RoutedEventArgs e)
        {
            tBIntegral.Text = increaseClick(tBIntegral.Text);
            timer2IncreaseTime.Start();
        }

        void timer2IncreaseTime_Tick(object sender, EventArgs e)
        {
            timer2IncreaseTime.Stop();
            timerOfIncreasingTime.Start();
        }

        void timerOfIncreasingTime_Tick(object sender, EventArgs e)
        {
            tBIntegral.Text = increaseClick(tBIntegral.Text);
        }

        void btnIncreaseTime_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            timer2IncreaseTime.Stop();
            timerOfIncreasingTime.Stop();
        }

        //减少积分时间按钮
        private void btnDecreaseTime_PreviewMouseDown(object sender, RoutedEventArgs e)
        {
            tBIntegral.Text = decreaseClick(tBIntegral.Text);
            timer2DecreaseTime.Start();
        }

        void timer2DecreaseTime_Tick(object sender, EventArgs e)
        {
            timer2DecreaseTime.Stop();
            timerOfDecreasingTime.Start();
        }

        void timerOfDecreasingTime_Tick(object sender, EventArgs e)
        {
            tBIntegral.Text = decreaseClick(tBIntegral.Text);
        }

        void btnDecreaseTime_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            timer2DecreaseTime.Stop();
            timerOfDecreasingTime.Stop();
        }

        //增加平均次数按钮
        private void btnIncreaseAverage_PreviewMouseDown(object sender, RoutedEventArgs e)
        {
            tBAverTimes.Text = increaseClick(tBAverTimes.Text);
            timer2IncreaseAverage.Start();
        }

        void timer2IncreaseAverage_Tick(object sender, EventArgs e)
        {
            timer2IncreaseAverage.Stop();
            timerOfIncreasingAverage.Start();
        }

        void timerOfIncreasingAverage_Tick(object sender, EventArgs e)
        {
            tBAverTimes.Text = increaseClick(tBAverTimes.Text);
        }

        void btnIncreaseAverage_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            timer2IncreaseAverage.Stop();
            timerOfIncreasingAverage.Stop();
        }

        //减少平均次数按钮
        private void btnDecreaseAverage_PreviewMouseDown(object sender, RoutedEventArgs e)
        {
            tBAverTimes.Text = decreaseClick(tBAverTimes.Text);
            timer2DecreaseAverage.Start();
        }

        void timer2DecreaseAverage_Tick(object sender, EventArgs e)
        {
            timer2DecreaseAverage.Stop();
            timerOfDecreasingAverage.Start();
        }

        void timerOfDecreasingAverage_Tick(object sender, EventArgs e)
        {
            tBAverTimes.Text = decreaseClick(tBAverTimes.Text);
        }

        void btnDecreaseAverage_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            timer2DecreaseAverage.Stop();
            timerOfDecreasingAverage.Stop();
        }

        //增加平滑度按钮
        private void btnIncreaseBoxcarWidth_PreviewMouseDown(object sender, RoutedEventArgs e)
        {
            tBSmooth.Text = increaseClick(tBSmooth.Text);
            timer2IncreaseBoxcarWidth.Start();
        }

        void timer2IncreaseBoxcarWidth_Tick(object sender, EventArgs e)
        {
            timer2IncreaseBoxcarWidth.Stop();
            timerOfIncreasingBoxcarWidth.Start();
        }

        void timerOfIncreasingBoxcarWidth_Tick(object sender, EventArgs e)
        {
            tBSmooth.Text = increaseClick(tBSmooth.Text);
        }

        void btnIncreaseBoxcarWidth_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            timer2IncreaseBoxcarWidth.Stop();
            timerOfIncreasingBoxcarWidth.Stop();
        }

        //减少平滑度按钮
        private void btnDecreaseBoxcarWidth_PreviewMouseDown(object sender, RoutedEventArgs e)
        {
            tBSmooth.Text = decreaseClick(tBSmooth.Text);
            timer2DecreaseBoxcarWidth.Start();
        }

        void timer2DecreaseBoxcarWidth_Tick(object sender, EventArgs e)
        {
            timer2DecreaseBoxcarWidth.Stop();
            timerOfDecreasingBoxcarWidth.Start();
        }

        void timerOfDecreasingBoxcarWidth_Tick(object sender, EventArgs e)
        {
            tBSmooth.Text = decreaseClick(tBSmooth.Text);
        }

        void btnDecreaseBoxcarWidth_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            timer2DecreaseBoxcarWidth.Stop();
            timerOfDecreasingBoxcarWidth.Stop();
        }

        private string increaseClick(string val)
        {
            int intVal = Convert.ToInt32(val);
            return (intVal + 1).ToString();
        }

        private string decreaseClick(string val)
        {
            int intVal = Convert.ToInt32(val);
            if (intVal > 1)
            {
                return (intVal - 1).ToString();
            }
            else
            {
                return val;
            }
        }

        #endregion

        private void cBIsOpenLight_Click(object sender, RoutedEventArgs e)
        {
            Spectrograph.Spectrograph.SetStrobeEnable((bool)cBIsOpenLight.IsChecked);
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

        #endregion

        #region 普通测量页签

        //确认测量参数
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!AddNewParamWindow.IsXPosValid(tBXPos.Text))
            {
                MessageBox.Show("X位置超出范围，范围是  " + Constants.MIN_X + "~" + Constants.MAX_X);
                return;
            }

            if (!AddNewParamWindow.IsYPosValid(tBYPos.Text))
            {
                MessageBox.Show("Y位置超出范围，范围是" + Constants.MIN_Y + "~" + Constants.MAX_Y);
                return;
            }

            if (!AddNewParamWindow.IsAngleValid(tBAngle.Text))
            {
                MessageBox.Show("角度超出范围，范围是" + Constants.MIN_ANGLE + "~" + Constants.MAX_ANGLE);
                return;
            }

            isMeasureParamConfirmed = true;

            //判断是否改变了测量角度和光阑值
            if (currentMeasureParam.MeasureAngle != Int32.Parse(tBAngle.Text)
                || currentMeasureParam.Stop != Int32.Parse(StopBox.Text))
            {
                isRefPixelsExist = false;
                isDarkPixelsExist = false;
            }            

            currentMeasureParam.PositionX = Int32.Parse(tBXPos.Text);
            currentMeasureParam.PositionY = Int32.Parse(tBYPos.Text);
            currentMeasureParam.MeasureAngle = Int32.Parse(tBAngle.Text);
            currentMeasureParam.Stop = Int32.Parse(StopBox.Text);

            //获取之前保存的参考光谱、暗光谱、积分时间、平均次数、平滑度
            GetLastSavedMeasureParams();

            UpdateUI();

            if (isGetMeasuredParams)
            {
                sPShortCutKey.IsEnabled = false;
            }

            NormalChart.Series.Clear();
            RelectivityChart.Series.Clear();
        }

        //获取之前保存的参考光谱、暗光谱、积分时间、平均次数、平滑度
        private void GetLastSavedMeasureParams()
        {
            isGetMeasuredParams = false;

            foreach (AjustParam ajustParam in ajustParamList)
            {
                if (ajustParam.Angle == Int32.Parse(tBAngle.Text) && ajustParam.Stop == Int32.Parse(StopBox.Text))
                {
                    //询问是否使用之前的参考光谱、暗光谱等参数
                    MsgBox msgBox = new MsgBox(Constants.TIP, "是否应用已保存的参考光谱、暗光谱、积分时间、平均次数、平滑宽度？", MessageBoxButton.YesNo);

                    //如果是，则应用之前的积分时间、平均次数、平滑宽度、参考光谱、暗光谱，并把测试按钮设为可点击
                    if (msgBox.ShowDialog() == true)
                    {
                        currentIntegrationTime = ajustParam.IntegrationTime_us;
                        currentAverage = ajustParam.Average;
                        currentBoxcarWidth = ajustParam.Smooth;

                        SetIntegralControl(currentIntegrationTime);
                        tBAverTimes.Text = currentAverage.ToString();
                        tBSmooth.Text = currentBoxcarWidth.ToString();

                        refPixels = ajustParam.Refrence;
                        darkPixels = ajustParam.Dark;

                        isRefPixelsExist = true;
                        isDarkPixelsExist = true;

                        BtnMeasure.IsEnabled = true;
                        BtnMeasure2.IsEnabled = true;

                        isGetMeasuredParams = true;
                    }
                    //如果不是，则不改变积分时间等参数，测试按钮设为不可点击
                    else
                    {
                        isRefPixelsExist = false;
                        isDarkPixelsExist = false;

                        BtnMeasure.IsEnabled = false;
                        BtnMeasure2.IsEnabled = false;
                    }

                    break;
                }
            }
        }

        private void SetIntegralControl(int usIntegrationTime)
        {
            //大于1秒
            if (usIntegrationTime >= 1000000)
            {
                //如果没有毫秒数
                if (usIntegrationTime % 1000000 == 0)
                {
                    tBIntegral.Text = (usIntegrationTime / 1000000).ToString();
                    CbUnit.SelectedIndex = 1;
                }
                else
                {
                    tBIntegral.Text = (usIntegrationTime / 1000).ToString();
                    CbUnit.SelectedIndex = 0;
                }
            }
            else
            {
                tBIntegral.Text = (usIntegrationTime / 1000).ToString();
                CbUnit.SelectedIndex = 0;
            }
        }

        //移动到校准位置
        private void BtnMoveToAjustPos_Click(object sender, RoutedEventArgs e)
        {
            //校准流程
            Thread ajustThread = new Thread(MoveToAjustPosThread);
            ajustThread.IsBackground = true;
            ajustThread.Start();

            isInAjustPos = true;
            UpdateUI();
        }

        //光谱仪移动到校准位置，需要提示用户，等移动到了后自动关闭
        private void MoveToAjustPosThread()
        {
            ShowMsgBox(Constants.WARNING, Constants.MOVING_TO_AJUST_POS, MessageBoxButton.OK, true);
            Plc.Plc.ControlPlc(Plc.Plc.AjustPosX, Plc.Plc.AjustPosY, (UInt32)currentMeasureParam.MeasureAngle);
            this.Dispatcher.Invoke(closeMsgBox);
        }

        //移动到测量位置
        private void BtnMoveToMeasurePos_Click(object sender, RoutedEventArgs e)
        {
            //校准流程
            Thread ajustThread = new Thread(MoveToMeasurePosPosThread);
            ajustThread.IsBackground = true;
            ajustThread.Start();

            isInAjustPos = false;
            UpdateUI();
        }

        //光谱仪移动到测量位置，需要提示用户，等移动到了后自动关闭
        private void MoveToMeasurePosPosThread()
        {
            ShowMsgBox(Constants.WARNING, "光谱仪正移动到(" + currentMeasureParam.PositionX + "," + currentMeasureParam.PositionY
                + ")，" + "角度" + currentMeasureParam.MeasureAngle + "度" + "，请勿触碰"
                , MessageBoxButton.OK, true);
            Plc.Plc.ControlPlc(currentMeasureParam.PositionX, currentMeasureParam.PositionY, (UInt32)currentMeasureParam.MeasureAngle);
            this.Dispatcher.Invoke(closeMsgBox);
        }

        private void BtnSaveRef_Click(object sender, RoutedEventArgs e)
        {
            this.lBState.Visibility = Visibility.Visible;

            //保存参数
            currentIntegrationTime = AjustWindow.GetIntegrationTime(CbUnit, tBIntegral);
            currentAverage = AjustWindow.GetAverage(tBAverTimes);
            currentBoxcarWidth = AjustWindow.GetBoxcarWidth(tBSmooth);

            //此时快捷键（积分时间、平均次数、平滑度）不能使用
            if (testMode == TestMode.TEST_WITHOUT_PLC)
            {
                sPShortCutKey.IsEnabled = true;
            }
            else if (testMode == TestMode.TEST_WITH_PLC)
            {
                sPShortCutKey.IsEnabled = false;
            }

            //显示标准光谱
            NormalChart.Visibility = Visibility.Visible;
            RelectivityChart.Visibility = Visibility.Collapsed;

            isRefPixelsExist = true;
            UpdateStateToolBar();

            Thread th = new Thread(InitRefPixelsAndUpdateNormalChart);
            th.IsBackground = true;
            th.Start();

            if (isRefPixelsExist && isDarkPixelsExist)
            {
                BtnMeasure.IsEnabled = true;
                BtnMeasure2.IsEnabled = true;
            }

            UpdateChartXAxisScope();
        }

        private void InitRefPixelsAndUpdateNormalChart()
        {
            pixels = Spectrograph.Spectrograph.GetPixels(currentIntegrationTime, currentAverage, currentBoxcarWidth);
            refPixels = pixels;

            for (int i = 0; i < pixels.Count(); i++)
            {
                pixels_Compensated[i] = pixels[i];
            }
            Trace.Util.CalculateClass.Compensate(ref pixels_Compensated);

            if (pixels == null || pixels.Count() <= 0)
            {
                return;
            }

            this.Dispatcher.Invoke(updateNormalChartDelegate);

            this.Dispatcher.Invoke(setLableStateInVisibleDelegate);
        }

        private void BtnSaveDark_Click(object sender, RoutedEventArgs e)
        {
            this.lBState.Visibility = Visibility.Visible;

            //保存参数
            currentIntegrationTime = AjustWindow.GetIntegrationTime(CbUnit, tBIntegral);
            currentAverage = AjustWindow.GetAverage(tBAverTimes);
            currentBoxcarWidth = AjustWindow.GetBoxcarWidth(tBSmooth);

            //此时快捷键（积分时间、平均次数、平滑度）不能使用
            if (testMode == TestMode.TEST_WITHOUT_PLC)
            {
                sPShortCutKey.IsEnabled = true;
            }
            else if (testMode == TestMode.TEST_WITH_PLC)
            {
                sPShortCutKey.IsEnabled = false;
            }

            //显示标准光谱
            NormalChart.Visibility = Visibility.Visible;
            RelectivityChart.Visibility = Visibility.Collapsed;

            isDarkPixelsExist = true;
            UpdateStateToolBar();

            Thread th = new Thread(InitDarkPixelsAndUpdateNormalChart);
            th.IsBackground = true;
            th.Start();

            if (isRefPixelsExist && isDarkPixelsExist)
            {
                BtnMeasure.IsEnabled = true;
                BtnMeasure2.IsEnabled = true;
            }

            UpdateChartXAxisScope();
        }

        private void BtnNormalChart_Click(object sender, RoutedEventArgs e)
        {
            NormalChart.Visibility = Visibility.Visible;
            RelectivityChart.Visibility = Visibility.Collapsed;
        }

        private void BtnReflectChart_Click(object sender, RoutedEventArgs e)
        {
            NormalChart.Visibility = Visibility.Collapsed;
            RelectivityChart.Visibility = Visibility.Visible;
        }

        private void mIHome_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(Plc.Plc.Home);
            thread.IsBackground = true;
            thread.Start();
        }

        private void mIAdminFuction_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow adminWnd = new AdminWindow();
            adminWnd.ShowDialog();
        }

        void mIInputCompensate_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow.ImportCompensateData();
        }

        void mIOutputCompensate_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow.ExportCompensateData();
        }

        private void InitDarkPixelsAndUpdateNormalChart()
        {
            pixels = Spectrograph.Spectrograph.GetPixels(currentIntegrationTime, currentAverage, currentBoxcarWidth);
            darkPixels = pixels;
            if (pixels == null || pixels.Count() <= 0)
            {
                return;
            }

            for (int i = 0; i < pixels.Count(); i++)
            {
                pixels_Compensated[i] = pixels[i];
            }
            Trace.Util.CalculateClass.Compensate(ref pixels_Compensated);

            this.Dispatcher.Invoke(updateNormalChartDelegate);
            this.Dispatcher.Invoke(setLableStateInVisibleDelegate);
        }

        //补偿参数checkbox
        private void cBIsCompensated_Click(object sender, RoutedEventArgs e)
        {
            isCompensated = cBIsCompensated.IsChecked.Value;

            //刷新不标准光谱
            this.Dispatcher.Invoke(updateNormalChartDelegate);

            //刷新反射率光谱
            this.Dispatcher.Invoke(updateRelChartDelegate);
        }

        private void BtnMeasure_Click(object sender, RoutedEventArgs e)
        {
            this.lBState.Visibility = Visibility.Visible;

            this.BtnMeasure.IsEnabled = false;
            this.BtnMeasure2.IsEnabled = false;

            //默认显示反射光谱
            NormalChart.Visibility = Visibility.Collapsed;
            RelectivityChart.Visibility = Visibility.Visible;

            strAngle = cBAngle.Text;
            strLight = cBLight.Text;

            testResult = new TestResult();
            testResultList.Insert(0, testResult);

            if (TestMode.TEST_WITHOUT_PLC == testMode)
            {
                currentMeasureParam.Stop = Int32.Parse(StopBox.Text);
            }

            //刷新光谱图和结果表
            Thread th = new Thread(GetPixelsAndRefreshChartsAndTable);
            th.IsBackground = true;
            th.Start();

            UpdateChartXAxisScope();

            SaveRefAndDarkPixels();
        }

        //保存参考光谱、暗光谱、积分时间、平均次数、平滑宽度参数
        private void SaveRefAndDarkPixels()
        {
            bool isFindSameAngleAndStop = false;
            foreach (AjustParam ajustParam in ajustParamList)
            {
                if (Int32.Parse(tBAngle.Text) == ajustParam.Angle
                    && Int32.Parse(StopBox.Text) == ajustParam.Stop)
                {
                    ajustParam.IntegrationTime_us = currentIntegrationTime;
                    ajustParam.Average = currentAverage;
                    ajustParam.Smooth = currentBoxcarWidth;

                    ajustParam.Refrence = refPixels;
                    ajustParam.Dark = darkPixels;

                    isFindSameAngleAndStop = true;
                    break;
                }
            }

            if (!isFindSameAngleAndStop)
            {
                AjustParam ajustParam = new AjustParam(Int32.Parse(tBAngle.Text), Int32.Parse(StopBox.Text));

                ajustParam.IntegrationTime_us = currentIntegrationTime;
                ajustParam.Average = currentAverage;
                ajustParam.Smooth = currentBoxcarWidth;

                ajustParam.Refrence = refPixels;
                ajustParam.Dark = darkPixels;

                ajustParamList.Add(ajustParam);
            }
        }

        TestResult testResult = new TestResult();

        //刷新光谱图和结果表
        private void GetPixelsAndRefreshChartsAndTable()
        {
            //读取并显示标准光谱
            pixels = Spectrograph.Spectrograph.GetPixels(currentIntegrationTime, currentAverage, currentBoxcarWidth);
            if (pixels == null || pixels.Count() <= 0)
            {
                return;
            }

            for (int i = 0; i < pixels.Count(); i++)
            {
                pixels_Compensated[i] = pixels[i];
            }
            Trace.Util.CalculateClass.Compensate(ref pixels_Compensated);

            this.Dispatcher.Invoke(updateNormalChartDelegate);

            //计算反射光谱
            Trace.Util.CalculateClass calculateObject = new Util.CalculateClass(refPixels, darkPixels, pixels
                            , strAngle, strLight);
            energyArray = calculateObject.GetReflection();

            for (int i = 0; i < energyArray.Count(); i++)
            {
                energyArray_Compensated[i] = energyArray[i];
            }
            Trace.Util.CalculateClass.Compensate(ref energyArray_Compensated);

            //显示反射光谱
            this.Dispatcher.Invoke(updateRelChartDelegate);

            //计算测量的结果显示到测试结果页签的表格中
            testResult.ID = currentMeasureParam.OperID;
            currentMeasureParam.OperID++;
            testResult.desc = "test" + testResult.ID.ToString().PadLeft(3, '0');
            testResult.PositionX = currentMeasureParam.PositionX;
            testResult.PositionY = currentMeasureParam.PositionY;
            testResult.MeasureAngle = currentMeasureParam.MeasureAngle;
            testResult.Stop = currentMeasureParam.Stop;

            testResult.ObserveAngle = strAngle;
            testResult.Illuminant = strLight;

            testResult.IntegrationTime_ms = currentIntegrationTime / 1000;
            testResult.Average = currentAverage;
            testResult.Smooth = currentBoxcarWidth;

            //计算LCH等值
            if (isCompensated)
            {
                calculateObject.ComputeParams(Spectrograph.Spectrograph.WavelengthArray, energyArray_Compensated);
            }
            else
            {
                calculateObject.ComputeParams(Spectrograph.Spectrograph.WavelengthArray, energyArray);
            }
            
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
            testResult.EnergyArray = energyArray;

            this.Dispatcher.Invoke(updateListViewDelegate);
            this.Dispatcher.Invoke(setLableStateInVisibleDelegate);
            this.Dispatcher.Invoke(setMeasureBtnEnableDelegate);
        }

        private void UpdateRelChartMethod()
        {
            if (energyArray == null || energyArray_Compensated == null)
            {
                return;
            }

            ObservableCollection<KeyValuePair<double, double>> lineList = new ObservableCollection<KeyValuePair<double, double>>();

            double[] localEnergyArray;
            if (isCompensated)
            {
                localEnergyArray = energyArray_Compensated;
            }
            else
            {
                localEnergyArray = energyArray;
            }

            double maxEnergyArray = 0;
            double minEnergyArray = 0;
            bool scopeOut = false;
            for (int i = 0; i < localEnergyArray.Count(); i++)
            {
                //生成显示结果
                lineList.Add(new KeyValuePair<double, double>(Spectrograph.Spectrograph.WavelengthArray[i], localEnergyArray[i]));

                //获取最大值，对Y轴范围进行自适应
                if (scopeOut)
                {
                    continue;
                }

                if (Spectrograph.Spectrograph.WavelengthArray[i] > 400 && Spectrograph.Spectrograph.WavelengthArray[i] < 760)
                {
                    if (localEnergyArray[i] > maxEnergyArray)
                    {
                        maxEnergyArray = localEnergyArray[i];
                    }

                    if (localEnergyArray[i] < minEnergyArray)
                    {
                        minEnergyArray = localEnergyArray[i];
                    }
                }
                else if (Spectrograph.Spectrograph.WavelengthArray[i] >= 760)
                {
                    scopeOut = true;
                }
            }

            RelChartYAxis.Maximum = maxEnergyArray * (Constants.Y_AXIS_SCOPE_FACTOR);
            RelChartYAxis.Minimum = minEnergyArray > 0 ? 0 : (minEnergyArray * Constants.Y_AXIS_SCOPE_FACTOR);

            //刷新光谱图
            RelectivityChart.Series.Clear();

            LineSeries ls = new LineSeries();
            ls.IndependentValueBinding = new System.Windows.Data.Binding("Key");//X轴
            ls.DependentValueBinding = new System.Windows.Data.Binding("Value");//Y轴
            ls.DataPointStyle = Application.Current.Resources["MyLineDataPointStyle"] as Style;
            ls.ItemsSource = lineList;

            RelectivityChart.Series.Add(ls);
        }

        private void SetScanOnceBtnEnableMethod()
        {
            this.BtnScanOnce.IsEnabled = true;
        }

        private void SetMeasureBtnEnableMethod()
        {
            this.BtnMeasure.IsEnabled = true;
            this.BtnMeasure2.IsEnabled = true;
        }

        private void SetLableStateInVisibleMethod()
        {
            this.lBState.Visibility = Visibility.Collapsed;
        }

        private void UpdateChartXAxisScope()
        {
            normalChartXAxis.Minimum = int.Parse(tBMinWaveLength.Text);
            normalChartXAxis.Maximum = int.Parse(tBMaxWaveLength.Text);

            RelChartXAxis.Minimum = int.Parse(tBMinWaveLength.Text);
            RelChartXAxis.Maximum = int.Parse(tBMaxWaveLength.Text);
        }



        private void checkBoxSelAll_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxSelAll.IsChecked.HasValue && checkBoxSelAll.IsChecked.Value)
            {
                foreach (TestResult testResult in listViewData.Items)
                {
                    testResult.IsSelected = true;
                }
            }
            else
            {
                foreach (TestResult testResult in listViewData.Items)
                {
                    testResult.IsSelected = false;
                }
            }
        }

        private void BtnData2Excel_Click(object sender, RoutedEventArgs e)
        {
            //判断是否有已勾选的测试结果
            bool isSelectedExist = false;
            foreach (TestResult testResult in listViewData.Items)
            {
                if (testResult.IsSelected)
                {
                    isSelectedExist = true;
                    break;
                }
            }

            if (!isSelectedExist)
            {
                MessageBox.Show("没有选择需要保存的测量数据，请选择测量数据");
                return;
            }

            bool ret = ExcelUtil.ExcelUtil.ExportToExcel_MainWindow(listViewData, true);
            if (ret)
            {
                MessageBox.Show(Constants.SAVE_DATA_TO_EXCEL_PASS);
            }
        }

        private void BtnData2Database_Click(object sender, RoutedEventArgs e)
        {
            //判断是否有已勾选的测试结果
            bool isSelectedExist = false;
            foreach (TestResult testResult in listViewData.Items)
            {
                if (testResult.IsSelected)
                {
                    isSelectedExist = true;
                    break;
                }
            }

            if (!isSelectedExist)
            {
                MessageBox.Show("没有选择需要保存的测量数据，请选择测量数据");
                return;
            }

            //获取选择的且没有保存过的数据
            ObservableCollection<TestResult> selectedAndNotSavedResultList = GetSelectedAndNotSavedResultList(testResultList);

            if (selectedAndNotSavedResultList.Count <= 0)
            {
                MessageBox.Show("所选择的数据之前已经保存，无需再次保存");
                return;
            }

            bool res1 = DatabaseConnection.SaveResultToDatabase(selectedAndNotSavedResultList, 0);
            if (!res1)
            {
                MessageBox.Show("任务结果数据保存失败，停止保存任务结果数据");
                return;
            }
            
            bool res2 = DatabaseConnection.SaveRefsToDataBase(selectedAndNotSavedResultList, 0);
            if (!res2)
            {
                MessageBox.Show("反射率数据保存失败，停止保存任务结果数据");
                return;
            }

            SaveSavedResultTestId();
            MessageBox.Show("数据保存成功");
        }

        //保存已经保存过的测试结果，避免多次保存
        private void SaveSavedResultTestId()
        {
            foreach (TestResult testResult in listViewData.Items)
            {
                if (testResult.IsSelected)
                {
                    if (!savedTestIdList.Contains(testResult.ID))
                    {
                        savedTestIdList.Add(testResult.ID);
                    }
                }
            }
        }

        private ObservableCollection<TestResult> GetSelectedAndNotSavedResultList(ObservableCollection<TestResult> testResultList)
        {
            ObservableCollection<TestResult> resultList = new ObservableCollection<TestResult>();

            foreach (TestResult testResult in testResultList)
            {
                if (testResult.IsSelected && !savedTestIdList.Contains(testResult.ID))
                {
                    resultList.Add(testResult);
                }
            }
            return resultList;
        }

        private void BtnRestartSpec_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnClearData_Click(object sender, RoutedEventArgs e)
        {
            MsgBox msgBox = new MsgBox(Constants.WARNING, "确定要清空所有测量结果吗？", MessageBoxButton.YesNo);
            if (msgBox.ShowDialog() == false)
            {
                return;
            }

            testResultList.Clear();
        }
        #endregion

        private void MainWindowNew_Closed(object sender, EventArgs e)
        {
            Spectrograph.Spectrograph.SetStrobeEnable(false);
        }

        #region 测试结果页签

        private void UpdateListView()
        {
            this.listViewData.Items.Refresh();
        }

        private void SearchFileBtn_Click(object sender, RoutedEventArgs e)
        {
            searchFilterControl.search();
            searchResults = searchFilterControl.searchResults;
            if (searchResults != null && searchResults.Count != 0)
            {
                searchResultsPanel.Children.Clear();

                SearchResultUserControl searchControl = new SearchResultUserControl() { SearchResults = searchResults };
                searchControl.Height = 640;//TODO，暂时按照交付设备的分辨率将该高度设为固定值，应该是自适应的
                searchResultsPanel.Children.Add(searchControl);
            }
            else
            {
                searchResultsPanel.Children.Clear();
            }
        }

        private void BtnCloseSpec_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCalibrateNonLinear_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCloseCalibrate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mOpenLog_Click(object sender, RoutedEventArgs e)
        {

        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            statusLabel.Background = Brushes.Blue;

            startButton.IsEnabled = false;
            pauseButton.IsEnabled = true;
            stopButton.IsEnabled = true;
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            statusLabel.Background = Brushes.Yellow;

            startButton.IsEnabled = true;
            pauseButton.IsEnabled = false;
            stopButton.IsEnabled = true;
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            statusLabel.Background = Brushes.Red;

            startButton.IsEnabled = true;
            pauseButton.IsEnabled = false;
            stopButton.IsEnabled = false;
        }

        #endregion

        /*private void SearchFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchWindow se = new SearchWindow();
            se.Closed += SeWindow_Closed;
            se.ShowDialog();
        }

        private void SeWindow_Closed(object sender, EventArgs e)
        {
            SearchWindow se = (SearchWindow)sender;
            if(se.bIsOk)
            {
                searchResults = se.searchResults;
                if (searchResults != null && searchResults.Count != 0)
                {
                    searchResultsPanel.Children.Clear();

                    SearchResultUserControl searchControl = new SearchResultUserControl() { SearchResults = searchResults };
                    searchResultsPanel.Children.Add(searchControl);
                }
            }
        }*/
    }
}
