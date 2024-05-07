using Autodesk.DesignScript.Geometry;
using KDHBridge_DYN48.Element.MathCurve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Nodes.DMathCurve
{
    public class DMathCurve2Da
    {
        public List<Point> VisualPoints { get; }
        public Curve VisualCurve { get; }
        public List<Point> ControlPoints { get; }
        public MathCurve2D MathCurve { get; }
        public List<double> Stations { get; }
        public List<double> LocalXs { get; }

        public DMathCurve2Da(Plane plane, List<int> ids, List<string> types, List<double> lengths, List<double> rad1s,
            List<double> rad2s, List<double> angles, double tol = 0.1)
        {
            MathCurve = new MathCurve2D(plane, ids, types, lengths, rad1s, rad2s, angles, tol);
            VisualPoints = MathCurve.VisualPoints;
            VisualCurve = MathCurve.VisualCurve;
            ControlPoints = MathCurve.ControlPoints;
            Stations = MathCurve.Stations;
            LocalXs = MathCurve.LocalXs;
        }

        public Point PointAtParameter(double param) => MathCurve.PointAtParameter(param);
        public Vector TangentAtParameter(double param) => MathCurve.TangentAtParameter(param);
        public Plane PlaneAtParameter(double param) => MathCurve.PlaneAtParameter(param);
        public DMathCurve2Da SetPlane(Plane plane)
        {
            MathCurve.SetPlane(plane);
            return this;
        }

    }


}
