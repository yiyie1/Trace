using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataBase;

namespace Trace.Util
{
    public class CalculateClass
    {
        private SPAM.CCoSpectralMath spectralMath;
        private SPAM.CCoCIEObserver cieObserver;
        private SPAM.CCoIlluminant illuminant;
        private SPAM.CCoCIEColor cieColor;
        private SPAM.CCoCIELAB cieLAB;
        private SPAM.CCoXYZColor cieXYZ;

        private double[] referencePixels;
        private double[] darkPixels;
        private double[] samplePixels;
        private double[] energyArray;

        public CalculateClass(double[] refrencePixels, double[] darkPixels,
            double[] samplePixels, String observe, String illuminant)
        {
            spectralMath = new SPAM.CCoSpectralMath();

            this.referencePixels = refrencePixels;
            this.darkPixels = darkPixels;
            this.samplePixels = samplePixels;

            SPAM.CCoCIEConstants cieConstants = spectralMath.createCIEConstantsObject();

            this.cieObserver = cieConstants.getObserver(observe);
            this.illuminant = cieConstants.getIlluminantByName(illuminant);

            cieConstants.Dispose();
        }

        //获取反射光谱
        public double[] GetReflection()
        {
            SPAM.CCoCoreReflection coreReflection = spectralMath.createCoreReflectionObject();
            energyArray = coreReflection.processSpectrum(darkPixels, referencePixels, samplePixels);

            coreReflection.Dispose();
            return energyArray;
        }

        public static bool Compensate(ref double[] energyArray)
        {
            //进行补偿参数运算
            List<string> compensateParams = new List<string>();
            if (!DatabaseConnection.GetCompensateParamFromDatabase(ref compensateParams))
            {
                System.Diagnostics.Trace.WriteLine("补偿参数从数据库中读出失败");
                return false;
            }

            if (compensateParams.Count != Constants.PIXELS_COUNT)
            {
                System.Diagnostics.Trace.WriteLine("补偿参数个数错误");
                return false;
            }

            for (int i = 0; i < energyArray.Count(); i++)
            {
                energyArray[i] = energyArray[i] * int.Parse(compensateParams[i]) / 100;
            }

            return true;
        }

        public double UpperL { get; set; }
        public double UpperC { get; set; }
        public double UpperH { get; set; }
        public double LowerA { get; set; }
        public double LowerB { get; set; }
        public double UpperY { get; set; }
        public double LowerX { get; set; }
        public double LowerY { get; set; }
        //public double LowerZ { get; set; }
        public double UpperR { get; set; }
        public double UpperG { get; set; }
        public double UpperB { get; set; }

        //获取各种参数
        public void ComputeParams(double[] waveLengthArray, double[] energyArray)
        {
            SPAM.CCoAdvancedColor advancedColor = spectralMath.createAdvancedColorObject();
            cieColor = advancedColor.computeReflectiveChromaticity(waveLengthArray, energyArray, cieObserver, illuminant);

            advancedColor.Dispose();

            //计算Lab
            cieLAB = spectralMath.createCIELABObject(cieColor);
            UpperL = cieLAB.getLStar();
            LowerA = cieLAB.get_aStar();
            LowerB = cieLAB.get_bStar();

            cieLAB.Dispose();

            //计算XYZ
            cieXYZ = spectralMath.createXYZColorObject(cieColor);
            double X = cieXYZ.getX();
            double Y = cieXYZ.getY();
            double Z = cieXYZ.getZ();

            cieXYZ.Dispose();

            //计算Yxy
            UpperY = Y;
            LowerX = X / (X + Y + Z);
            LowerY = Y / (X + Y + Z);

            //计算LCH
            UpperC = System.Math.Sqrt(((LowerA * LowerA) + (LowerB * LowerB)));
            UpperH = System.Math.Atan(LowerB / LowerA) * 360 / 2 / Math.PI;
            if (UpperH < 0)
            {
                UpperH += 360;
            }

            //计算RGB
            UpperR = ((1.577 * X) - (0.5978 * Y) - (0.312 * Z));
            UpperG = (((-0.5832) * X) + (1.6152 * Y) + (0.1003 * Z));
            UpperB = ((0.0063 * X) - (0.0174 * Y) + (1.2116 * Z));

            //小数位数修正
            UpperL = Math.Round(UpperL, 1);
            UpperC = Math.Round(UpperC, 1);
            UpperH = Math.Round(UpperH, 1);
            LowerA = Math.Round(LowerA, 1);
            LowerB = Math.Round(LowerB, 1);
            UpperY = Math.Round(UpperY, 1);
            LowerX = Math.Round(LowerX, 4);
            LowerY = Math.Round(LowerY, 4);
            UpperR = Math.Round(UpperR, 1);
            UpperG = Math.Round(UpperG, 1);
            UpperB = Math.Round(UpperB, 1);
        }
    }
}

