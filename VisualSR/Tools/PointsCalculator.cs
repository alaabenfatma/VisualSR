using System.Windows;
using VisualSR.Core;

namespace VisualSR.Tools
{
    /// <summary>
    ///     A class to determine the <c>EXACT</c> coordinates of a point
    /// </summary>
    public static class PointsCalculator
    {
        public static Point PortOrigin(Port port)
        {
            port.CalcOrigin();
            var p = new Point();
            //In case we've got an ObjectPort
            if (port is ObjectPort)
                if (port.PortTypes == PortTypes.Input)
                {
                    var x = port.Origin.X;
                    var y = port.Origin.Y + port.ActualHeight / 2;
                    p = new Point(x + 5, y + 5);
                }
                else
                {
                    var x = port.Origin.X + port.ActualWidth;
                    var y = port.Origin.Y + port.ActualHeight / 2;
                    p = new Point(x - 5, y + 5);
                }
            //In case we've got an execution port
            else if (port is ExecPort)
                if (port.PortTypes == PortTypes.Input)
                {
                    port.CalcOrigin();
                    var x = port.Origin.X;
                    var y = port.Origin.Y + port.ActualHeight / 2;
                    p = new Point(x + 5, y);
                }
                else
                {
                    port.CalcOrigin();
                    var x = port.Origin.X + port.ActualWidth;
                    var y = port.Origin.Y + port.ActualHeight / 2;
                    p = new Point(x - 5, y + 1);
                }
            return p;
        }
    }
}