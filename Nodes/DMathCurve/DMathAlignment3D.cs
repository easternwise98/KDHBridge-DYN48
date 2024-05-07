using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using KDHBridge_DYN48.Element.MathCurve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Nodes.DMathCurve
{
    public static class DMathAlignment3D
    {

        [MultiReturn(new[] { "MathAlignment3D", "VisualPoints", "VisualCurve", "ControlPoints", "Stations" })]
        public static Dictionary<string, object> SetMathAlignment3D(Plane plane, MathCurve2D hMathCurve, MathCurve2D vMathCurve, double tol = 0.1, bool isConstraint = true)
        {
            MathAlignment3D mathCurve = new MathAlignment3D(plane, hMathCurve, vMathCurve, tol, isConstraint);
            return new Dictionary<string, object>
            {
                { "MathAlignment3D", mathCurve },
                { "VisualPoints", mathCurve.VisualPoints },
                { "VisualCurve", mathCurve.VisualCurve },
                { "ControlPoints", mathCurve.ControlPoints },
                { "Stations", mathCurve.Stations }
            };
        }

        public static Point PointAtStation(MathAlignment3D mathCurve, double station) => mathCurve.PointAtStation(station);
        public static Vector TangentAtStation(MathAlignment3D mathCurve, double station) => mathCurve.TangentAtStation(station);
        public static Plane PlaneAtStation(MathAlignment3D mathCurve, double station) => mathCurve.PlaneAtStation(station);

        public static Point PointAtParameter(MathAlignment3D mathCurve, double param) => mathCurve.PointAtParameter(param);
        public static Vector TangentAtParameter(MathAlignment3D mathCurve, double param) => mathCurve.TangentAtParameter(param);
        public static Plane PlaneAtParameter(MathAlignment3D mathCurve, double param) => mathCurve.PlaneAtParameter(param);
        public static MathAlignment3D SetPlane(MathAlignment3D mathCurve, Plane plane)
        {
            return mathCurve.SetPlane(plane);
        }


    }


}
