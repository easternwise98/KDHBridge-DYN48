using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using KDHBridge_DYN48.Utility;


namespace KDHBridge_DYN48.Element.MathCurve
{
    [IsVisibleInDynamoLibrary(false)]
    public class MathAlignment3D
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
        public List<double> Stations { get; set; }


        // MathCurve3D는 서로 다른 두 MathCurve2D를 한 축으로 통일하여 기준으로 생성함.
        public MathAlignment3D(Plane plane, MathCurve2D hCurve, MathCurve2D vCurve, double tol=0.1, bool isConstraint=true)
        {
            Plane = plane;
            LocalHMathCurve2D = hCurve.SetPlane(Plane.XY());
            LocalVMathCurve2D = vCurve.SetPlane(Plane.ByOriginXAxisYAxis(Point.Origin(), Vector.XAxis(), Vector.ZAxis()));

            Stations = GetStaitions(isConstraint);

            Length = GetTotalLength(tol);

            VisualPoints = GetVisualPoints(tol);
            VisualCurve = PolyCurve.ByPoints(VisualPoints);
            ControlPoints = Stations.Select(x => PointAtStation(x)).ToList();
            
        }
        
        internal List<double> GetStaitions(bool constMode)
        {
            // contraint를 걸면 HMathCurve를 기준으로 생성 그외엔 짧은거 기준
            List<double> stations = new List<double>();
            if (constMode)
            {
                stations = LocalHMathCurve2D.Stations;
            }
            else
            {
                List<double> hStations = LocalHMathCurve2D.Stations;
                List<double> vStations = LocalVMathCurve2D.Stations;
                stations = hStations.Concat(vStations).Distinct().ToList();
                stations.Sort();
                double shortValue = Math.Min(hStations.Last(), vStations.Last());
                stations = stations.Where(x => x <= shortValue).ToList();
            }
            return stations;
        }

        internal Point LocalPointAtStation(double station)
        {
            Point xy = LocalHMathCurve2D.PointAtLength(station);
            Point xz = LocalVMathCurve2D.PointAtLength(station);
            Point xyz = Point.ByCoordinates(xy.X, xy.Y, xz.Z);
            return xyz;
        }   

        public Point PointAtStation (double station)
        {
            Point xyz = LocalPointAtStation(station);
            return GeometryFunctions.GetPointAt(Plane, xyz.X, xyz.Y, xyz.Z);
        }

        internal double GetTotalLength(double tol)
        {
            // tolerance를 기반으로 길이 및 station산정.
            int n = (int)(Math.Floor(Stations.Last() / tol) + 1);
            List<double> range = Enumerable.Range(0, n + 1).Select(x => x * Stations.Last() / n).ToList();

            List<Point> points = range.Select(x => PointAtStation(x)).ToList();
            PolyCurve polyCurve = PolyCurve.ByPoints(points);
            return polyCurve.Length;
        }

        public List<Point> GetVisualPoints(double tol)
        {
            int n = (int)(Math.Floor(Stations.Last() / tol) + 1);
            List<double> range = Enumerable.Range(0, n + 1).Select(x => (double)x / n).ToList();
            return range.Select(x => PointAtStation(Stations.Last() * x)).ToList();
        }

        public Vector TangentAtStation(double station)
        {
            //Vector vec1 = LocalHMathCurve2D.TangentAtLength(station);
            //Vector vec2 = LocalVMathCurve2D.TangentAtLength(station);
            //Vector vec = vec1.Cross(vec2);
            //Point point = PointAtStation(station);
            //Point point2 = (Point)point.Translate(vec);
            //Point rotPoint = GeometryFunctions.RotatePointByVector(point2, point, Vector.ZAxis(), Plane.Normal);
            //Vector rotVec = Vector.ByTwoPoints(point, rotPoint);
            //return rotVec;
            Point point1 = PointAtStation(station);
            Vector vec;
            if (station == Stations.Last())
            {
                Point point2 = PointAtStation(station - 0.001);
                vec = Vector.ByTwoPoints(point2, point1).Normalized();
            }
            else
            {
                Point point2 = PointAtStation(station + 0.001);
                vec = Vector.ByTwoPoints(point1, point2).Normalized();
            }
            return vec;
            
        }

        public Plane PlaneAtStation(double station)
        {
            Point point = PointAtStation(station);
            Vector tangent = TangentAtStation(station);
            Vector xVector = Plane.Normal.Cross(tangent);
            return Plane.ByOriginNormalXAxis(point, tangent, xVector);
        }

        public Point PointAtParameter(double param) => PointAtStation(Stations.Last() * param);
        public Vector TangentAtParameter(double param) => TangentAtStation(Stations.Last() * param);
        public Plane PlaneAtParameter(double param) => PlaneAtStation(Stations.Last() * param);
        public MathAlignment3D SetPlane(Plane plane) => new MathAlignment3D(plane, HMathCurve, VMathCurve);

    }
}
