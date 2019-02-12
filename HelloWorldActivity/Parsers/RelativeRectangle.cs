using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnius.Parsers
{
    public class RelativeRectangle
    {
        public class Point2D
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }

        public static RelativeRectangle FromPageXml(int pageWidth, int pageHeight, List<Point2D> points)
        {
            var ordered = ClockwiseOrderFromTopLeft(points);
            return new RelativeRectangle
            {
                X = ordered[0].X / pageWidth,
                Y = ordered[0].Y / pageHeight,
                Width = Math.Abs(ordered[0].X - ordered[2].X) / pageWidth,
                Height = Math.Abs(ordered[0].Y - ordered[2].Y) / pageHeight,
            };
        }

        public float[] ToTopLeftWidthHeight(float pageWidth, float pageHeight)
        {
            return new double[]
            {
                Y * pageHeight,
                X * pageWidth,
                Width * pageWidth,
                Height * pageHeight
            }
            .Select(x => (float)x)
            .ToArray();
        }

        public static List<Point2D> ClockwiseOrderFromTopLeft(List<Point2D> points)
        {
            var horisontalySorted = points.OrderBy(XGetter).ThenBy(YGetter).ToList();
            var leftVerticalLine = horisontalySorted.Take(2).OrderBy(YGetter).ToList();
            var rightVerticalLine = horisontalySorted.Skip(2).Take(2).OrderBy(YGetter).ToList();
            return new[] { leftVerticalLine[0], rightVerticalLine[0], rightVerticalLine[1], leftVerticalLine[1] }.ToList();
        }



        //Readability helpers

        private static double Xcoord(RelativeRectangle r)
        {
            return r.X;
        }

        private static double XGetter(Point2D p)
        {
            return p.X;
        }

        private static double YGetter(Point2D p)
        {
            return p.Y;
        }

        private static double Ycoord(RelativeRectangle r)
        {
            return r.Y;
        }
    }
}
