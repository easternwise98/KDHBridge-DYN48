using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DynamoUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Nodes
{
    public static class GeometryFunctionNodes
    {
        [MultiReturn(new[] { "Arrow" })]
        public static Dictionary<string, object> Arrow(Vector vector, Point origin, double length=1)
        {
            Line l = Line.ByStartPointDirectionLength(origin, vector, length);
            Cone cone = Cone.ByPointsRadius(l.PointAtParameter(0.8), l.EndPoint, length / 20);
            
            List<Geometry> arrow = new List<Geometry> { l, cone };

            return new Dictionary<string, object>
            {
                { "Arrow", arrow }
            };
        }

        public static Geometry RotateByVector(Geometry geom, Point origin, Vector from, Vector to)
        {
            // 1차 회전 (기존 벡터 => 원하는 벡터의 XY평면)
            Vector toXY = Vector.ByCoordinates(to.X, to.Y, 0, true);
            Vector cross1 = from.Cross(toXY);
            double angle1 = from.AngleWithVector(toXY);
            Geometry rotGeom1 = geom.Rotate(origin, cross1, angle1);

            // 2차 회전 (원하는 XY평면 => 원하는 벡터)
            Vector cross2 = toXY.Cross(to);
            double angle2 = toXY.AngleWithVector(to);
            Geometry rotGeom2 = rotGeom1.Rotate(origin, cross2, angle2);
            return rotGeom2;
        }

        public static Point GetPointAt(Plane plane, double u=0, double v=0, double w=0)
        {
            CoordinateSystem cs = plane.ToCoordinateSystem();
            return Point.ByCartesianCoordinates(cs, u, v, w);
        }
    }
}
