using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trace.Model
{
    public class ProjectParam
    {
        public ProjectParam()
        {
        }

        public ProjectParam(int operId, int positionX, int positionY, int measureAngle, int stop)
        {
            OperID = operId;
            PositionX = positionX;
            PositionY = positionY;
            MeasureAngle = measureAngle;
            Stop = stop;
        }

        public int OperID { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int MeasureAngle { get; set; }
        public int Stop { get; set; }
    }
}
