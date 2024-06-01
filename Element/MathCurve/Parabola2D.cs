using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DynamoUnits;
using KDHBridge_DYN48.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Element.MathCurve
{
    [IsVisibleInDynamoLibrary(false)]
    public class Parabola2D : IMathCurve
    {
        internal Plane Plane { get; set; }
        public double Length { get; set; }
        internal double Angle { get; set; }

        public Plane StartPlane { get; set; }
        public Plane EndPlane { get; set; }
        public Dictionary<string, double> Parameters { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }
        internal Point Start { get; set; }
        internal Point End { get; set; }
        public Dictionary<string, double> Properties { get; set; }
        public List<Point> ControlPoints { get; set; }
        public List<Point> VisualPoints { get; set; }
        public Curve VisualCurve { get; set; }


        public Parabola2D(Plane plane, double width, double height, double tol=0.1)
        {
            Plane = plane;
            Width = width;
            Height = height;
            

            Parameters = new Dictionary<string, double>
            {
                { "Length", Length },
                { "Angle", Angle }
            };

            Start = PointAtParameter(0);
            End = PointAtParameter(1);
            ControlPoints = new List<Point> { Start, PointAtParameter(0.5), End };

            Length = GetLength();

            VisualPoints = GetVisualPoints(tol);
            VisualCurve = PolyCurve.ByPoints(VisualPoints);

            Length = VisualCurve.Length;

            StartPlane = plane;
            EndPlane = GetEndPlane();
        }

        public List<Point> GetVisualPoints(double tol = 0.01)
        {
            int n = (int)(Math.Floor(Length / tol) + 1);
            List<double> range = Enumerable.Range(0, n + 1).Select(x => (double)x / n).ToList();
            return range.Select(x => PointAtParameter(x)).ToList();
        }
        public Point PointAtParameter(double param)
        {
            double localX = param * Width;
            double localY = GetLocalY(localX);

            return GeometryFunctions.GetPointAt(Plane, localX, localY, 0);
        }

        public Vector TangentAtParameter(double param)
        {
            double localX = param * Width;
            double localY = GetLocalY(localX);
            double localX2 = localX + 1;
            double localY2 = GetDiffLocalY(localX);
            Point point1 = GeometryFunctions.GetPointAt(Plane, localX, localY, 0);
            Point point2 = GeometryFunctions.GetPointAt(Plane, localX2, localY2, 0);
            Vector vector = Vector.ByTwoPoints(point1, point2).Normalized();
            return vector;
        }
        public Plane PlaneAtParameter(double param)
        {
            Point origin = PointAtParameter(param);
            Vector tangent = TangentAtParameter(param);
            Vector xAxis = tangent.Cross(Plane.Normal).Normalized();
            return Plane.ByOriginNormalXAxis(origin, tangent, xAxis);
        }

        internal Plane GetEndPlane()
        {
            Point origin = PointAtParameter(1);
            Vector xAxis = TangentAtParameter(1);
            Vector normal = Plane.Normal;
            return Plane.ByOriginNormalXAxis(origin, normal, xAxis);
        }

        internal double GetLocalY(double x)
        {
            return (4 * Height) / Math.Pow(Width, 2) * Math.Pow(x - Width / 2, 2) - Height;
        }
        internal double GetDiffLocalY(double x)
        {
            return 2 * (4 * Height) / Math.Pow(Width, 2) * (x - Width / 2) * ((x + 1) - x) + GetLocalY(x);
        }



        public IMathCurve SetPlane(Plane plane) => new Parabola2D(plane, Width, Height);

        internal double GetLength()
        {
            double a = Height;
            double b = Width;
            double length = 0.5 * Math.Sqrt(b * b + 16 * a * a + b * b)
                   - (b * b / (8 * a)) * Math.Log(4 * a + Math.Sqrt(b * b + 16 * a * a));

            return Math.Abs(length);
        }


    }
}
