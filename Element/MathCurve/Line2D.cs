using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using KDHBridge_DYN48.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Element.MathCurve
{
    [IsVisibleInDynamoLibrary(false)]
    public class Line2D : IMathCurve
    {
        internal Plane Plane { get; set; }
        public double Length { get; set; }
        internal double Angle { get; set; }

        public Plane StartPlane { get; set; }
        public Plane EndPlane { get; set; }
        public Dictionary<string, double> Parameters { get; set; }

        internal Point Start { get; set; }
        internal Point End { get; set; }
        public Dictionary<string, double> Properties { get; set; }
        public List<Point> ControlPoints { get; set; }
        public List<Point> VisualPoints { get; set; }
        public Curve VisualCurve { get; set; }


        public Line2D(Plane plane, double length, double angle = 0, double tol=0.1)
        {
            Plane = plane;
            Length = length;
            Angle = angle;

            Parameters = new Dictionary<string, double>
            {
                { "Length", Length },
                { "Angle", Angle }
            };

            Start = PointAtParameter(0);
            End = PointAtParameter(1);
            ControlPoints = new List<Point> { Start, End};

            
            VisualPoints = GetVisualPoints(tol);
            VisualCurve = PolyCurve.ByPoints(VisualPoints);

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
            double localX = Math.Cos(Angle) * Length * param;
            double localY = Math.Sin(Angle) * Length * param;
            return GeometryFunctions.GetPointAt(Plane, localX, localY, 0);
        }

        public Vector TangentAtParameter(double param)
        {
            double localX = Math.Cos(Angle);
            double localY = Math.Sin(Angle);
            Point point2 = GeometryFunctions.GetPointAt(Plane, localX, localY, 0);
            Vector vector = Vector.ByTwoPoints(Plane.Origin, point2).Normalized();
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

        public IMathCurve SetPlane(Plane plane) => new Line2D(plane, Length, Angle);

    }
}
