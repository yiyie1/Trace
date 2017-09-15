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
    /// SelPatternWin.xaml 的交互逻辑
    /// </summary>
    public partial class SelPatternWindow : Window
    {
        public static String MEASURE_MODE = "测量模式";
        public static String ANALYZE_MODE = "分析模式";

        private String mode = String.Empty;
        public String Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
            }
        }

        public SelPatternWindow()
        {
            InitializeComponent();

            BtnMeasure.Click += new RoutedEventHandler(BtnMeasure_Click);
            BtnAnalyze.Click += new RoutedEventHandler(BtnAnalyze_Click);
        }

        void BtnMeasure_Click(object sender, RoutedEventArgs e)
        {
            mode = MEASURE_MODE;
            this.Hide();
            SysStateWindow sysStateWin = new SysStateWindow();
            sysStateWin.ShowDialog();
        }

        void BtnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            mode = ANALYZE_MODE;
            this.Hide();
            SearchWindow seWindow = new SearchWindow();
            seWindow.Closed += SeWindow_Closed;
            seWindow.ShowDialog();
        }

        private void SeWindow_Closed(object sender, EventArgs e)
        {
            Show();
        }

        protected override void OnClosed(EventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
