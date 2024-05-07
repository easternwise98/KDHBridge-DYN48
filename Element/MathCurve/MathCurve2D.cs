using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Element.MathCurve
{
    [IsVisibleInDynamoLibrary(false)]
    public class MathCurve2D
    {
        // IMathCurve properties
        public Plane StartPlane { get; }
        public Plane EndPlane { get; }
        public double Length { get; }
        public Dictionary<string, double> Parameters { get; }
        public Dictionary<string, double> Properties { get; }
        public List<Point> ControlPoints { get; }
        public List<Point> VisualPoints { get; }
        public Curve VisualCurve { get; }
        public Curve VisualNurbsCurve { get; }

        // MathCurve2D properties
        // Parameters
        internal List<int> IDs { get; }
        internal List<string> Types { get; }
        internal List<double> Lengths { get; }
        internal List<double> Rad1s { get; }
        internal List<double> Rad2s { get; }
        internal List<double> Angles { get; }

        internal List<double> Stations { get; }
        internal List<double> LocalXs { get; }
        internal List<IMathCurve> MathCurves { get; }

        public MathCurve2D(Plane plane, List<int> ids, List<string> types, List<double> lengths, List<double> rad1s,
            List<double> rad2s, List<double> angles, double tol=0.1)
        {
            // Parameters
            IDs = ids;
            Types = types;
            Lengths = lengths;
            Rad1s = rad1s;
            Rad2s = rad2s;
            Angles = angles;
            

            StartPlane = plane;
            // 반복문으로 알맞는 MathCurve 추가하기
            IMathCurve startCurve = GetMathCurve(plane, ids[0], types[0], lengths[0], rad1s[0], rad2s[0], angles[0]);
            MathCurves = new List<IMathCurve>() { startCurve };
            Stations = new List<double>() { 0, startCurve.Length };
            LocalXs = new List<double>() { 0, GetLocalXValue(startCurve) };
            //double loX = LocalXs[1];
            for (int i = 1; i < ids.Count; i++)
            {
                IMathCurve curve = GetMathCurve(MathCurves[i-1].EndPlane, ids[i], types[i], lengths[i], rad1s[i], rad2s[i], angles[i]);
                MathCurves.Add(curve);
                Stations.Add(Stations[i] + curve.Length);
                //loX += GetLocalXValue(curve);
                LocalXs.Add(GetLocalXValue(curve)); 
                LocalXs.Sort();
            }
            Length = Stations.Last();

            // Draw
            ControlPoints = Stations.Select(x => PointAtLength(x)).ToList();
            VisualPoints = GetVisualPoints(tol);
            VisualCurve = PolyCurve.ByPoints(VisualPoints);
            VisualNurbsCurve = NurbsCurve.ByPoints(VisualPoints);

        }

        internal double GetLocalXValue(IMathCurve mathCurve)
        {
            Plane plane = StartPlane;
            Point origin = plane.Origin;
            Point endPoint = mathCurve.PointAtParameter(1);
            Vector point2Origin = Vector.ByTwoPoints(origin, endPoint);
            double xValue = point2Origin.Dot(plane.XAxis);
            // double xValue = plane.XAxis.Dot(point2Origin);
            return xValue;
        }

        public List<Point> GetVisualPoints(double tol = 0.01)
        {
            int n = (int)(Math.Floor(Length / tol) + 1);
            List<double> range = Enumerable.Range(0, n + 1).Select(x => (double)x / n).ToList();
            return range.Select(x => PointAtParameter(x)).ToList();
        }

        internal IMathCurve GetMathCurve(Plane plane, int id, string type, double length, double rad1, double rad2, double angle)
        {
            IMathCurve curve = null;
            if (type == "Line")
            {
                curve = new Line2D(plane, length, angle);
            }
            else if (type == "Arc")
            {
                curve = new Arc2D(plane, rad1, length, angle);
            }
            else if (type == "Clothoid")
            {
                curve = new Clothoid2D(plane, rad1, rad2, length);
            }
            else if (type == "Parabola")
            {
                curve = new Parabola2D(plane, rad1, rad2);
            }
            return curve;
        }
        public Point PointAtXValue(double xValue)
        {
            Point point = null;
            for (int i = 0; i < LocalXs.Count - 1; i++)
            {
                if (LocalXs[i] <= xValue && xValue <= LocalXs[i + 1])
                {
                    double param = (xValue - LocalXs[i]) / (LocalXs[i + 1] - LocalXs[i]);
                    point = MathCurves[i].PointAtParameter(param);
                    break;
                }
            }
            return point;
        }

        public Point PointAtLength(double length)
        {
            Point point = null;
            for (int i = 0; i < Stations.Count; i++)
            {
                if (Stations[i] <= length && length <= Stations[i + 1])
                {
                    double param = (length - Stations[i]) / MathCurves[i].Length;
                    point = MathCurves[i].PointAtParameter(param);
                    break;
                }
            }
            return point;
        }
        public Point PointAtParameter(double param)
        {
            double paramLength = Length * param;
            return PointAtLength(paramLength);
        }

        public Vector TangentAtLength(double length)
        {
            Vector tangent = null;
            for (int i = 0; i < Stations.Count; i++)
            {
                if (Stations[i] <= length && length <= Stations[i + 1])
                {
                    double param = (length - Stations[i]) / MathCurves[i].Length;
                    tangent = MathCurves[i].TangentAtParameter(param);
                    break;
                }
            }
            return tangent;
        }

        public Vector TangentAtParameter(double param)
        {
            double paramLength = Length * param;
            return TangentAtLength(paramLength);
        }

        public Plane PlaneAtLength(double length)
        {
            Plane plane = null;
            for (int i = 0; i < Stations.Count; i++)
            {
                if (Stations[i] <= length && length <= Stations[i + 1])
                {
                    double param = (length - Stations[i]) / MathCurves[i].Length;
                    plane = MathCurves[i].PlaneAtParameter(param);
                    break;
                }
            }
            return plane;
        }

        public Plane PlaneAtParameter(double param)
        {
            double paramLength = Length * param;
            return PlaneAtLength(paramLength);
        }

        public Plane GetEndPlane()
        {
            Point origin = PointAtParameter(1);
            Vector normal = TangentAtParameter(1);
            Vector xVector = normal.Cross(Vector.ZAxis());
            return Plane.ByOriginNormalXAxis(origin, normal, xVector);
        }

        public MathCurve2D SetPlane(Plane plane) => new MathCurve2D(plane, IDs, Types, Lengths, Rad1s, Rad2s, Angles);
    }
}
