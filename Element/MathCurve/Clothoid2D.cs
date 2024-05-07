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
    public class Clothoid2D : IMathCurve
    {
        internal Plane Plane { get; set; }
        internal double Radius1 { get; set; }
        internal double Radius2 { get; set; }
        public double Length { get; set; }

        public Plane StartPlane { get; set; }
        public Plane EndPlane { get; set; }
        public Dictionary<string, double> Parameters { get; set; }

        internal double CLengthStart { get; set; }
        internal double CLengthEnd { get; set; }
        internal double ParamA { get; set; }

        public Dictionary<string, double> Properties { get; set; }

        public List<Point> ControlPoints { get; set; }
        public List<Point> VisualPoints { get; set; }
        public Curve VisualCurve { get; set; }

        public Clothoid2D(Plane plane, double radius1, double radius2, double length, double tol=0.1)
        {
            Plane = plane;
            Radius1 = radius1;
            Radius2 = radius2;
            Length = length;

            // 클로소이드
            CLengthStart = Radius2 * Length / (Radius1 - Radius2);
            ParamA = Math.Sqrt(Math.Abs(Radius1 * CLengthStart));
            CLengthEnd = CLengthStart + Length;

            Parameters = new Dictionary<string, double>
            {
                { "Radius1", Radius1 },
                { "Radius2", Radius2 },
                { "Length", Length }
            };

            Point start = PointAtParameter(0);
            Point end = PointAtParameter(1);    
            ControlPoints = new List<Point> { start, end };

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

        // 선형 길이로부터 클로소이드 변수들 구하기
        internal double GetClothoidLength(double length) => CLengthStart + length;
        internal double GetParamT(double length) => Math.Pow(GetClothoidLength(length), 2) / (2 * Math.Pow(ParamA, 2));
        internal double GetRadiusAtLength(double length) => Math.Pow(ParamA, 2) / GetClothoidLength(length);

        // 클로소이드 구하기
        internal Point GetClothoidLocalPoint(double length)
        {
            double l = GetClothoidLength(length);
            double R = Radius2;
            double L = CLengthEnd;
            double cX = l 
                - Math.Pow(l, 5) / (40 * Math.Pow(R*L, 2)) 
                + Math.Pow(l, 9) / (3456 * Math.Pow(R*L, 4))
                - Math.Pow(l, 13) / (599040 * Math.Pow(R*L, 6))
                + Math.Pow(l, 17) / (17418240 * Math.Pow(R*L, 8));
            double cY = Math.Pow(l, 3) / (6 * R*L)
                - Math.Pow(l, 7) / (336 * Math.Pow(R*L, 3))
                + Math.Pow(l, 11) / (42240 * Math.Pow(R*L, 5))
                - Math.Pow(l, 15) / (9676800 * Math.Pow(R*L, 7))
                + Math.Pow(l, 19) / (3344302080 * Math.Pow(R*L, 9));
            return Point.ByCoordinates(cX, cY, 0);
        }

        internal Vector GetClothoidLocalTangent(double length)
        {
            Point clPoint = GetClothoidLocalPoint(length);
            Vector tangent = Vector.ByCoordinates(Math.Cos(GetParamT(length)), Math.Sin(GetParamT(length)), 0);
            return tangent;
        }

        internal Point GetClothoidLocalCenter(double length)
        {
            Point clPoint = GetClothoidLocalPoint(length);
            Vector tangent = GetClothoidLocalTangent(length);
            double rad = GetRadiusAtLength(length);
            Vector toCenter = tangent.Cross(Vector.ZAxis()).Normalized();
            Point center = (Point)clPoint.Translate(toCenter, rad);
            return center;
        }

        internal Point GetGlobalPoint(double length)
        {
            Point localPoint = GetClothoidLocalPoint(length);
            Point localStart = GetClothoidLocalPoint(0);

            // 클로소이드 시작점(클로소이드 좌표계) => 원점좌표계로 이동
            Vector start2Origin = Vector.ByTwoPoints(localStart, Point.Origin());
            Point point1 = (Point)localPoint.Translate(start2Origin);
            // 원점 좌표계에서 원점을 기준으로 각도만큼 회전
            Point point2 = (Point)point1.Rotate(Point.Origin(), Vector.ZAxis(), -1 * GetParamT(0) * 180 / Math.PI);
            return point2;
        }

        internal Vector GetGlobalVector(double length)
        {
            Vector localVector = GetClothoidLocalTangent(length);
            // 클로소이드 시작점(클로소이드 좌표계) => 원점좌표계로 이동
            Vector vector1 = localVector.Rotate(Vector.ZAxis(), -1 * GetParamT(0) * 180 / Math.PI);
            return vector1;
        }

        internal Point GetGlobalCenter(double length)
        {
            Point localPoint = GetClothoidLocalCenter(length);
            Point localStart = GetClothoidLocalPoint(0);
            // 클로소이드 시작점(클로소이드 좌표계) => 원점좌표계로 이동
            Vector start2Origin = Vector.ByTwoPoints(localStart, Point.Origin());
            Point point1 = (Point)localPoint.Translate(start2Origin);
            // 원점 좌표계에서 원점을 기준으로 각도만큼 회전
            Point point2 = (Point)point1.Rotate(Point.Origin(), Vector.ZAxis(), -1 * GetParamT(0) * 180 / Math.PI);
            return point2;
        }

        public Point PointAtParameter(double param)
        {
            double paramLength = param * Length;
            Point point = GetGlobalPoint(paramLength);
            return GeometryFunctions.GetPointAt(Plane, point.X, point.Y, 0);
        }

        public Vector TangentAtParameter(double param)
        {
            double paramLength = param * Length;
            // 원점 좌표계
            Point point = GetGlobalPoint(paramLength);
            Vector vector =  GetGlobalVector(paramLength);
            Point point2 = point.Add(vector);
            // 글로벌 좌표계
            Point point3 = GeometryFunctions.GetPointAt(Plane, point.X, point.Y, 0);
            Point point4 = GeometryFunctions.GetPointAt(Plane, point2.X, point2.Y, 0);
            return Vector.ByTwoPoints(point3, point4);
        }
        public Plane PlaneAtParameter(double param)
        {
            Point origin = PointAtParameter(param);
            Vector tangent = TangentAtParameter(param);
            Vector xAxis = tangent.Cross(Plane.Normal).Normalized();
            return Plane.ByOriginNormalXAxis(origin, tangent, xAxis);
        }

        // 애매함
        public Point CenterPointAtParameter(double param)
        {
            double paramLength = param * Length;
            Point point =  GetGlobalCenter(paramLength);
            return GeometryFunctions.GetPointAt(Plane, point.X, point.Y, 0);
        }

        internal Plane GetEndPlane()
        {
            Point origin = PointAtParameter(1);
            Vector xAxis = TangentAtParameter(1);
            Vector normal = Plane.Normal;
            return Plane.ByOriginNormalXAxis(origin, normal, xAxis);
        }

        public IMathCurve SetPlane(Plane plane) => new Clothoid2D(plane, Radius1, Radius2, Length);

    }
}
