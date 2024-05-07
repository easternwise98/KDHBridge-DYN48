using Autodesk.DesignScript.Geometry;
using KDHBridge_DYN48.Element.ModelAlignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Utility
{
    internal class GeometryFunctions
    {
        // Plane
        internal static Point GetPointAt(Plane plane, double u, double v, double w)
        {
            CoordinateSystem cs = plane.ToCoordinateSystem();
            return Point.ByCartesianCoordinates(cs, u, v, w);
        }

        // Rotate
        internal static Geometry RotateByVector(Geometry geom, Point origin, Vector from, Vector to)
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

        internal static Point RotatePointByVector(Point point, Point origin, Vector from, Vector to)
        {
            // 1차 회전 (기존 벡터 => 원하는 벡터의 XY평면)
            Vector toXY = Vector.ByCoordinates(to.X, to.Y, 0, true);
            Vector cross1 = from.Cross(toXY);
            double angle1 = from.AngleWithVector(toXY);
            Point rotPoint1 = (Point)point.Rotate(origin, cross1, angle1);

            // 2차 회전 (원하는 XY평면 => 원하는 벡터)
            Vector cross2 = toXY.Cross(to);
            double angle2 = toXY.AngleWithVector(to);
            Point rotPoint2 = (Point)rotPoint1.Rotate(origin, cross2, angle2);
            return rotPoint2;
        }

        internal static Geometry RotateByCurve(Geometry geom, Vector from, Curve cur, double loc)
        {
            Point origin = cur.PointAtParameter(loc);
            Vector to = cur.TangentAtParameter(loc);
            return RotateByVector(geom, origin, from, to);
        }

        internal static Point RotatePointByCurve(Point point, Vector from, Curve cur, double loc)
        {
            Point origin = cur.PointAtParameter(loc);
            Vector to = cur.TangentAtParameter(loc);
            return RotatePointByVector(point, origin, from, to);
        }

        internal static Geometry RotateByAlignment(Geometry geom, Vector from, IModelAlignment al, double param)
        {
            Point origin = al.PointAtParameter(param);
            Vector to = al.TangentAtParameter(param);
            return RotateByVector(geom, origin, from, to);
        }
        
        // MathCurve에 대해 회전하는거 만들기.

        // Vector
        
        
    }
}
