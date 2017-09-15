using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trace.Model
{
    public class AjustParam
    {
        public AjustParam(int angle, int stop)
        {
            Angle = angle;
            Stop = stop;
        }

        public int Angle { get; set; }
        public int Stop { get; set; }
        public int IntegrationTime_us { get; set; }
        public int Average { get; set; }
        public int Smooth { get; set; }
        public double[] Refrence = new double[Constants.PIXELS_COUNT];
        public double[] Dark = new double[Constants.PIXELS_COUNT];
    }
}