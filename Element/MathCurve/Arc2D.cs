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
    public class Arc2D : IMathCurve
    {
        public Plane Plane { get; set; }
        internal double Radius { get; set; }
        public double Length { get; set; }
        internal double Angle { get; set; }

        public Plane StartPlane { get; set; }
        public Plane EndPlane { get; set; }

        public Dictionary<string, double> Parameters { get; set; }

        internal Point Start { get; set; }
        internal Point Center { get; set;}
        internal Point End { get; set; }
        internal Plane CenterPlane { get; set; }
        public Dictionary<string, double> Properties { get; set; }

        public List<Point> ControlPoints { get; set; }
        public List<Point> VisualPoints { get; set; }
        public Curve VisualCurve { get; set; }



        public Arc2D(Plane plane, double radius, double length, double angle = 0, double tol=0.1)
        {
            Plane = plane;
            Radius = radius;
            Length = length;
            Angle = angle;

            Parameters = new Dictionary<string, double>
            {
                { "Radius", Radius },
                { "Length", Length },
                { "Angle", Angle }
            };

            double centerAngle = Length / Radius;

            // 시작점으로부터 중심점 구하기
            Point start = plane.Origin;
            Vector startTangent = Vector.ByTwoPoints(
                start, 
                GeometryFunctions.GetPointAt(plane, Math.Cos(angle), Math.Sin(angle), 0)
                ).Normalized();
            Vector toCenter = plane.Normal.Cross(startTangent).Normalized();
            Plane centerPlane = Plane.ByOriginNormalXAxis(
                (Point)start.Translate(toCenter, Radius),
                plane.Normal, 
                toCenter.Reverse()
                );
            Point center = centerPlane.Origin;

            CenterPlane = centerPlane;
            Start = start;
            Center = center;
            End = PointAtParameter(1);
            

            ControlPoints = new List<Point> { Start, Center, End };

            VisualPoints = GetVisualPoints(tol);
            VisualCurve = Arc.ByCenterPointStartPointEndPoint(Center, Start, End);
            
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
            double parmaAngle = Length * param / Radius;
            Point point = GeometryFunctions.GetPointAt(CenterPlane, 
                Radius * Math.Cos(parmaAngle), Radius * Math.Sin(parmaAngle), 0);
            return point;
        }

        public Vector TangentAtParameter(double param)
        {
            Point point = PointAtParameter(param);
            Vector toCenter = Vector.ByTwoPoints(Center, point);
            return CenterPlane.Normal.Cross(toCenter).Normalized();
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

        public IMathCurve SetPlane(Plane plane) => new Arc2D(plane, Radius, Length, Angle);

    }
}
