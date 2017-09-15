using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrograph
{
    class Spectrograph
    {
        public static OmniDriver.CCoWrapper wrapper = new OmniDriver.CCoWrapper();

        //获取当前连接的光谱仪个数
        public static int GetSpectrographCount()
        {
            int numberOfSpectrometersFound = wrapper.openAllSpectrometers();
            return numberOfSpectrometersFound;
        }

        //横坐标
        private static double[] wavelengthArray;
        public static double[] WavelengthArray
        {
            get
            {
                if (null != wavelengthArray)
                {
                    return wavelengthArray;
                }
               
                if (GetSpectrographCount() == 0)
                {
                    System.Windows.Forms.MessageBox.Show("光谱仪获取失败");
                    return null;
                }

                wavelengthArray = (double[])wrapper.getWavelengths(0);
                return wavelengthArray;
            }
            set
            {

            }
        }

        public static void SetStrobeEnable(bool isStrobeEnable)
        {
            if (GetSpectrographCount() == 0)
            {
                System.Windows.Forms.MessageBox.Show("光谱仪获取失败");
                return;
            }

            if (isStrobeEnable)
            {
                wrapper.setStrobeEnable(0, 1);

                //采集一次使灯亮设置有效
                //积分时间、平均次数设为较小值
                wrapper.setIntegrationTime(0, 100000);
                wrapper.setScansToAverage(0, 1);
                wrapper.setBoxcarWidth(0, 1);

                wrapper.setCorrectForElectricalDark(0, 1);
                wrapper.setCorrectForDetectorNonlinearity(0, 1);

                wrapper.getSpectrum(0);
            }
            else
            {
                wrapper.setStrobeEnable(0, 0);

                //采集一次使灯亮设置有效
                //积分时间、平均次数设为较小值
                wrapper.setIntegrationTime(0, 100000);
                wrapper.setScansToAverage(0, 1);
                wrapper.setBoxcarWidth(0, 1);

                wrapper.setCorrectForElectricalDark(0, 1);
                wrapper.setCorrectForDetectorNonlinearity(0, 1);

                wrapper.getSpectrum(0);
            }
        }

        public static double[] GetRefPixels(int integrationTime, int average, int boxcarWidth)
        {
            if (GetSpectrographCount() == 0)
            {
                System.Windows.Forms.MessageBox.Show("光谱仪获取失败");
                return null;
            }

            wrapper.setIntegrationTime(0, integrationTime);
            wrapper.setScansToAverage(0, average);
            wrapper.setBoxcarWidth(0, boxcarWidth);

            wrapper.setCorrectForElectricalDark(0, 1);
            wrapper.setCorrectForDetectorNonlinearity(0, 1);

            wrapper.setStrobeEnable(0, 1);

            System.Threading.Thread.Sleep(1000);

            double[] pixels = (double[])wrapper.getSpectrum(0);

            wrapper.setStrobeEnable(0, 0);
            wrapper.getSpectrum(0);

            return pixels;
        }

        public static double[] GetDarkPixels(int integrationTime, int average, int boxcarWidth)
        {
            if (GetSpectrographCount() == 0)
            {
                System.Windows.Forms.MessageBox.Show("光谱仪获取失败");
                return null;
            }

            wrapper.setIntegrationTime(0, integrationTime);
            wrapper.setScansToAverage(0, average);
            wrapper.setBoxcarWidth(0, boxcarWidth);

            wrapper.setCorrectForElectricalDark(0, 1);
            wrapper.setCorrectForDetectorNonlinearity(0, 1);

            wrapper.setStrobeEnable(0, 0);

            System.Threading.Thread.Sleep(1000);

            double[] pixels = (double[])wrapper.getSpectrum(0);;

            return pixels;
        }

        public static double[] GetPixels(int integrationTime, int average, int boxcarWidth)
        {
            if (GetSpectrographCount() == 0)
            {
                System.Windows.Forms.MessageBox.Show("光谱仪获取失败");
                return null;
            }

            wrapper.setIntegrationTime(0, integrationTime);
            wrapper.setScansToAverage(0, average);
            wrapper.setBoxcarWidth(0, boxcarWidth);

            wrapper.setCorrectForElectricalDark(0, 1);
            wrapper.setCorrectForDetectorNonlinearity(0, 1);

            double[] pixels = (double[])wrapper.getSpectrum(0);

            return pixels;
        }
    }
}
