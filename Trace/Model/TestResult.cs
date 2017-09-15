using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Trace.Model
{
    public class TestResult : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        bool isSelected = false;
        /// <summary>
        /// 是否被选中
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }

        //平均值，方差值
        public string Type { get; set; }

        //用户定义的描述，如test001
        public string desc { get; set; }

        public int IdInDatabase { get; set; }

        //测试任务编号
        public int TestID { get; set; }

        //测试编号
        public int ID { get; set; }
        
        public string TaskName { get; set; }
        public string GroupName { get; set; }
        public string ReelNumber { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int MeasureAngle { get; set; }
        public int Stop { get; set; }
        public string ObserveAngle { get; set; }
        public string Illuminant { get; set; }
        public int IntegrationTime_ms { get; set; }
        public int Average { get; set; }
        public int Smooth { get; set; }
        public double UpperL { get; set; }
        public double UpperC { get; set; }
        public double UpperH { get; set; }
        public double LowerA { get; set; }
        public double LowerB { get; set; }
        public double UpperY { get; set; }
        public double LowerX { get; set; }
        public double LowerY { get; set; }
        public double UpperR { get; set; }
        public double UpperG { get; set; }
        public double UpperB { get; set; }

        public double[] Refrence = new double[Constants.PIXELS_COUNT];
        public double[] Dark = new double[Constants.PIXELS_COUNT];
        public double[] EnergyArray = new double[Constants.PIXELS_COUNT];

        public int BatchIndex { get; set; }

        public TestStateEnum TestState { get; set; }

        public static TestResult operator +(TestResult user1, TestResult user2)
        {
            TestResult retUser = new TestResult
            {
                PositionX = user1.PositionX + user2.PositionX,
                PositionY = user1.PositionY + user2.PositionY,
                MeasureAngle = user1.MeasureAngle + user2.MeasureAngle,
                Stop = user1.Stop + user2.Stop,
                IntegrationTime_ms = user1.IntegrationTime_ms + user2.IntegrationTime_ms,
                Average = user1.Average + user2.Average,
                Smooth = user1.Smooth + user2.Smooth,
                UpperL = user1.UpperL + user2.UpperL,
                UpperC = user1.UpperC + user2.UpperC,
                UpperH = user1.UpperH + user2.UpperH,
                LowerA = user1.LowerA + user2.LowerA,
                LowerB = user1.LowerB + user2.LowerB,
                UpperY = user1.UpperY + user2.UpperY,
                LowerX = user1.LowerX + user2.LowerX,
                LowerY = user1.LowerY + user2.LowerY,
                UpperR = user1.UpperR + user2.UpperR,
                UpperG = user1.UpperG + user2.UpperG,
                UpperB = user1.UpperB + user2.UpperB,
            };

            return retUser;
        }

        public static TestResult operator -(TestResult user1, TestResult user2)
        {
            TestResult retUser = new TestResult
            {
                PositionX = user1.PositionX - user2.PositionX,
                PositionY = user1.PositionY - user2.PositionY,
                MeasureAngle = user1.MeasureAngle - user2.MeasureAngle,
                Stop = user1.Stop - user2.Stop,
                IntegrationTime_ms = user1.IntegrationTime_ms - user2.IntegrationTime_ms,
                Average = user1.Average - user2.Average,
                Smooth = user1.Smooth - user2.Smooth,
                UpperL = user1.UpperL - user2.UpperL,
                UpperC = user1.UpperC - user2.UpperC,
                UpperH = user1.UpperH - user2.UpperH,
                LowerA = user1.LowerA - user2.LowerA,
                LowerB = user1.LowerB - user2.LowerB,
                UpperY = user1.UpperY - user2.UpperY,
                LowerX = user1.LowerX - user2.LowerX,
                LowerY = user1.LowerY - user2.LowerY,
                UpperR = user1.UpperR - user2.UpperR,
                UpperG = user1.UpperG - user2.UpperG,
                UpperB = user1.UpperB - user2.UpperB,
            };

            return retUser;
        }

        public static TestResult operator *(TestResult user1, TestResult user2)
        {
            TestResult retUser = new TestResult
            {
                PositionX = user1.PositionX * user2.PositionX,
                PositionY = user1.PositionY * user2.PositionY,
                MeasureAngle = user1.MeasureAngle * user2.MeasureAngle,
                Stop = user1.Stop * user2.Stop,
                IntegrationTime_ms = user1.IntegrationTime_ms * user2.IntegrationTime_ms,
                Average = user1.Average * user2.Average,
                Smooth = user1.Smooth * user2.Smooth,
                UpperL = user1.UpperL * user2.UpperL,
                UpperC = user1.UpperC * user2.UpperC,
                UpperH = user1.UpperH * user2.UpperH,
                LowerA = user1.LowerA * user2.LowerA,
                LowerB = user1.LowerB * user2.LowerB,
                UpperY = user1.UpperY * user2.UpperY,
                LowerX = user1.LowerX * user2.LowerX,
                LowerY = user1.LowerY * user2.LowerY,
                UpperR = user1.UpperR * user2.UpperR,
                UpperG = user1.UpperG * user2.UpperG,
                UpperB = user1.UpperB * user2.UpperB,
            };

            return retUser;
        }

        public static TestResult operator /(TestResult user1, TestResult user2)
        {
            TestResult retUser = new TestResult
            {
                PositionX = user2.PositionX.Equals(0) ? 0 : user1.PositionX / user2.PositionX,
                PositionY = user2.PositionY.Equals(0) ? 0 : user1.PositionY / user2.PositionY,
                MeasureAngle = user2.MeasureAngle.Equals(0) ? 0 : user1.MeasureAngle / user2.MeasureAngle,
                Stop = user2.Stop.Equals(0) ? 0 : user1.Stop / user2.Stop,
                IntegrationTime_ms = user2.IntegrationTime_ms.Equals(0) ? 0 : user1.IntegrationTime_ms / user2.IntegrationTime_ms,
                Average = user2.Average.Equals(0) ? 0 : user1.Average / user2.Average,
                Smooth = user2.Smooth.Equals(0) ? 0 : user1.Smooth / user2.Smooth,
                UpperL = user2.UpperL.Equals(0) ? 0 : user1.UpperL / user2.UpperL,
                UpperC = user2.UpperC.Equals(0) ? 0 : user1.UpperC / user2.UpperC,
                UpperH = user2.UpperH.Equals(0) ? 0 : user1.UpperH / user2.UpperH,
                LowerA = user2.LowerA.Equals(0) ? 0 : user1.LowerA / user2.LowerA,
                LowerB = user2.LowerB.Equals(0) ? 0 : user1.LowerB / user2.LowerB,
                UpperY = user2.UpperY.Equals(0) ? 0 : user1.UpperY / user2.UpperY,
                LowerX = user2.LowerX.Equals(0) ? 0 : user1.LowerX / user2.LowerX,
                LowerY = user2.LowerY.Equals(0) ? 0 : user1.LowerY / user2.LowerY,
                UpperR = user2.UpperR.Equals(0) ? 0 : user1.UpperR / user2.UpperR,
                UpperG = user2.UpperG.Equals(0) ? 0 : user1.UpperG / user2.UpperG,
                UpperB = user2.UpperB.Equals(0) ? 0 : user1.UpperB / user2.UpperB,
            };

            return retUser;
        }

        public static TestResult operator *(TestResult user1, double mul)
        {
            TestResult retUser = new TestResult
            {
                PositionX = (int)(user1.PositionX * mul),
                PositionY = (int)(user1.PositionY * mul),
                MeasureAngle = (int)(user1.MeasureAngle * mul),
                Stop = (int)(user1.Stop * mul),
                IntegrationTime_ms = (int)(user1.IntegrationTime_ms * mul),
                Average = (int)(user1.Average * mul),
                Smooth = (int)(user1.Smooth * mul),
                UpperL = Math.Round(user1.UpperL * mul, 1),
                UpperC = Math.Round(user1.UpperC * mul, 1),
                UpperH = Math.Round(user1.UpperH * mul, 1),
                LowerA = Math.Round(user1.LowerA * mul, 1),
                LowerB = Math.Round(user1.LowerB * mul, 1),
                UpperY = Math.Round(user1.UpperY * mul, 1),
                LowerX = Math.Round(user1.LowerX * mul, 1),
                LowerY = Math.Round(user1.LowerY * mul, 1),
                UpperR = Math.Round(user1.UpperR * mul, 1),
                UpperG = Math.Round(user1.UpperG * mul, 1),
                UpperB = Math.Round(user1.UpperB * mul, 1),
            };

            return retUser;
        }

        public static TestResult operator /(TestResult user1, int div)
        {
            TestResult retUser = new TestResult
            {
                PositionX = user1.PositionX / div,
                PositionY = user1.PositionY / div,
                MeasureAngle = user1.MeasureAngle / div,
                Stop = user1.Stop / div,
                IntegrationTime_ms = user1.IntegrationTime_ms / div,
                Average = user1.Average / div,
                Smooth = user1.Smooth / div,
                UpperL = Math.Round(user1.UpperL / div, 1),
                UpperC = Math.Round(user1.UpperC / div, 1),
                UpperH = Math.Round(user1.UpperH / div, 1),
                LowerA = Math.Round(user1.LowerA / div, 1),
                LowerB = Math.Round(user1.LowerB / div, 1),
                UpperY = Math.Round(user1.UpperY / div, 1),
                LowerX = Math.Round(user1.LowerX / div, 1),
                LowerY = Math.Round(user1.LowerY / div, 1),
                UpperR = Math.Round(user1.UpperR / div, 1),
                UpperG = Math.Round(user1.UpperG / div, 1),
                UpperB = Math.Round(user1.UpperB / div, 1),
            };

            return retUser;
        }

        public static TestResult Pow(TestResult input, double power)
        {
            TestResult res = new TestResult {
                PositionX = (int)Math.Pow(input.PositionX, power),
                PositionY = (int)Math.Pow(input.PositionY, power),
                MeasureAngle = (int)Math.Pow(input.MeasureAngle, power),
                Stop = (int)Math.Pow(input.Stop, power),
                IntegrationTime_ms = (int)Math.Pow(input.IntegrationTime_ms, power),
                Average = (int)Math.Pow(input.Average, power),
                Smooth = (int)Math.Pow(input.Smooth, power),
                UpperL = Math.Pow(input.UpperL, power),
                UpperC = Math.Pow(input.UpperC, power),
                UpperH = Math.Pow(input.UpperH, power),
                LowerA = Math.Pow(input.LowerA, power),
                LowerB = Math.Pow(input.LowerB, power),
                UpperY = Math.Pow(input.UpperY, power),
                LowerX = Math.Pow(input.LowerX, power),
                LowerY = Math.Pow(input.LowerY, power),
                UpperR = Math.Pow(input.UpperR, power),
                UpperG = Math.Pow(input.UpperG, power),
                UpperB = Math.Pow(input.UpperB, power),
            };

            return res;
        }

        public static TestResult Pow(double input, TestResult power)
        {
            TestResult res = new TestResult
            {
                PositionX = (int)Math.Pow(input, power.PositionX),
                PositionY = (int)Math.Pow(input, power.PositionY),
                MeasureAngle = (int)Math.Pow(input, power.MeasureAngle),
                Stop = (int)Math.Pow(input, power.Stop),
                IntegrationTime_ms = (int)Math.Pow(input, power.IntegrationTime_ms),
                Average = (int)Math.Pow(input, power.Average),
                Smooth = (int)Math.Pow(input, power.Smooth),
                UpperL = Math.Pow(input, power.UpperL),
                UpperC = Math.Pow(input, power.UpperC),
                UpperH = Math.Pow(input, power.UpperH),
                LowerA = Math.Pow(input, power.LowerA),
                LowerB = Math.Pow(input, power.LowerB),
                UpperY = Math.Pow(input, power.UpperY),
                LowerX = Math.Pow(input, power.LowerX),
                LowerY = Math.Pow(input, power.LowerY),
                UpperR = Math.Pow(input, power.UpperR),
                UpperG = Math.Pow(input, power.UpperG),
                UpperB = Math.Pow(input, power.UpperB),
            };

            return res;
        }
    }


    public enum TestStateEnum
    {
        NO_TEST,
        TESTING,
        TESTED
    }
}
