using Autodesk.DesignScript.Geometry;
using KDHBridge_DYN48.Element.MathCurve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Nodes.DMathCurve
{
    public class DMathCurve3Da
    {
        public List<Point> VisualPoints { get; }
        public Curve VisualCurve { get; }
        public List<Point> ControlPoints { get; }
        public MathCurve3D MathCurve { get; }
        public List<double> LocalXValues { get; }

        public DMathCurve3Da(Plane plane, DMathCurve2Da hCurve, DMathCurve2Da vCurve, double tol = 0.1)
        {
            MathCurve = new MathCurve3D(plane, hCurve.MathCurve, vCurve.MathCurve, tol);
            VisualPoints = MathCurve.VisualPoints;
            VisualCurve = MathCurve.VisualCurve;
            ControlPoints = MathCurve.ControlPoints;
            LocalXValues = MathCurve.LocalXValues;

        }

        public Point PointAtLocalX(double x) => MathCurve.PointAtLocalX(x);
        public Vector TangentAtLocalX(double x) => MathCurve.TangentAtLocalX(x);
        public Plane PlaneAtLocalX(double x) => MathCurve.PlaneAtLocalX(x);

        public Point PointAtParameter(double param) => MathCurve.PointAtParameter(param);
        public Vector TangentAtParameter(double param) => MathCurve.TangentAtParameter(param);
        public Plane PlaneAtParameter(double param) => MathCurve.PlaneAtParameter(param);

        public DMathCurve3Da SetPlane(Plane plane)
        {
            MathCurve.SetPlane(plane);
            return this;
        }
    }


}
