using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using KDHBridge_DYN48.Utility;


namespace KDHBridge_DYN48.Element.MathCurve
{
    [IsVisibleInDynamoLibrary(false)]
    public class MathCurve3D
    {
        // IMathCurve Properties
        public double Length { get; set; }
        public List<Point> ControlPoints { get; set; }
        public List<Point> VisualPoints { get; set; }
        public Curve VisualCurve { get; set; }

        // MathCurve3D Properties
        internal Plane Plane { get; set; }
        internal MathCurve2D HMathCurve { get; set; }
        internal MathCurve2D VMathCurve { get; set; }

        internal MathCurve2D LocalHMathCurve2D { get; set; }
        internal MathCurve2D LocalVMathCurve2D { get; set; }
        internal List<double> LocalHXValues { get; set; }
        internal List<double> LocalVXValues { get; set; }
        internal List<double> LocalXValues { get; set; }

        public List<double> Stations { get; set; }


        // MathCurve3D는 서로 다른 두 MathCurve2D를 한 축으로 통일하여 기준으로 생성함.
        public MathCurve3D(Plane plane, MathCurve2D hCurve, MathCurve2D vCurve, double tol=0.1)
        {
            Plane = plane;
            LocalHMathCurve2D = hCurve.SetPlane(Plane.XY());
            LocalVMathCurve2D = vCurve.SetPlane(Plane.ByOriginXAxisYAxis(Point.Origin(), Vector.XAxis(), Vector.ZAxis()));
            

            LocalHXValues = LocalHMathCurve2D.LocalXs;
            LocalVXValues = LocalVMathCurve2D.LocalXs;

            LocalXValues = GetLocalXValues();

            Length = GetTotalLength(tol);
            

            VisualPoints = GetVisualPoints(tol);
            VisualCurve = PolyCurve.ByPoints(VisualPoints);
            ControlPoints = LocalXValues.Select(x => PointAtLocalX(x)).ToList();
        }
        
        internal List<double> GetLocalXValues()
        {
            //List<IMathCurve> hCurves = LocalHMathCurve2D.MathCurves;
            //List<IMathCurve> vCurves = LocalVMathCurve2D.MathCurves;
            //List<double> hXValues = GetLocalXValue(hCurves);
            //List<double> vXValues = GetLocalXValue(vCurves);
            //List<double> xValues = hXValues.Concat(vXValues).ToList();
            //xValues.Sort();
            //double localXLength = Math.Min(hXValues.Last(), vXValues.Last());
            //return xValues.Where(x => x <= localXLength).Distinct().ToList();
            List<double> hXValues = LocalHMathCurve2D.LocalXs;
            List<double> vXValues = LocalVMathCurve2D.LocalXs;
            List<double> xValues = hXValues.Concat(vXValues).ToList();
            xValues.Sort();
            double localXLength = Math.Min(hXValues.Last(), vXValues.Last());
            return xValues.Where(x => x <= localXLength).Distinct().ToList();

        }

        internal double GetTotalLength(double tol)
        {
            // Tolerance를 기반으로 길이 및 Station산정.
            int n = (int)(Math.Floor(LocalXValues.Last() / tol) + 1);
            List<double> range = Enumerable.Range(0, n + 1).Select(x => x * LocalXValues.Last() / n).ToList();

            // LocalXValues 사이의 Point들을 구해서 PolyCurve로 만들기 - Length 구하기
            List<Point> points = range.Select(x => PointAtLocalX(x)).ToList();
            PolyCurve polyCurve = PolyCurve.ByPoints(points);
            return polyCurve.Length;
        }

        internal List<double> GetLocalXValue(List<IMathCurve> mathCurves)
        {
            double x = 0;
            List<double> xValues = new List<double>() { 0 };
            foreach (IMathCurve curve in mathCurves)
            {
                x += curve.PointAtParameter(1).X;
                xValues.Add(x);
            }
            return xValues;
        }

        public List<Point> GetVisualPoints(double tol = 0.01)
        {
            int n = (int)(Math.Floor(Length / tol) + 1);
            List<double> range = Enumerable.Range(0, n + 1).Select(x => (double)x / n).ToList();
            return range.Select(x => PointAtLocalX(LocalXValues.Last() * x)).ToList();
        }

        public Point LocalPointAtParamter(double param1, double param2)
        {
            Point xy = LocalHMathCurve2D.PointAtParameter(param1);
            Point xz = LocalVMathCurve2D.PointAtParameter(param2);
            return Point.ByCoordinates(xy.X, xy.Y, xz.Z);
        }

        // IMathCurve Methods
        public Point PointAtLocalX(double x)
        {
            double hParam = x / LocalHXValues.Last();
            double vParam = x / LocalVXValues.Last();
            Point localPoint = LocalPointAtParamter(hParam, vParam);
            return GeometryFunctions.GetPointAt(Plane, localPoint.X, localPoint.Y, localPoint.Z);
        }

        public Point PointAtParameter(double param)
        {
            double x = LocalXValues.Last() * param;
            return PointAtLocalX(x);
        }

        public Vector TangentAtLocalX(double x)
        {
            double hParam = x / LocalHXValues.Last();
            double vParam = x / LocalVXValues.Last();
            Vector hTangent = LocalHMathCurve2D.TangentAtParameter(hParam);
            Vector vTangent = LocalVMathCurve2D.TangentAtParameter(vParam);
            return hTangent.Cross(vTangent).Normalized();
        }

        public Vector TangentAtParameter(double param)
        {
            double x = LocalXValues.Last() * param;
            return TangentAtLocalX(x);
        }
        
        public Plane PlaneAtLocalX(double x)
        {
            Point point = PointAtLocalX(x);
            Vector tangent = TangentAtLocalX(x);
            Vector xAxis = tangent.Cross(Plane.Normal).Normalized();
            return Plane.ByOriginNormalXAxis(point, tangent, xAxis);
        }

        public Plane PlaneAtParameter(double param)
        {
            double x = LocalXValues.Last() * param;
            return PlaneAtLocalX(x);
        }

        public MathCurve3D SetPlane(Plane plane) => new MathCurve3D(plane, LocalHMathCurve2D, LocalVMathCurve2D);
    }
}
