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
    public static class DMathCurve2D
    {
        [MultiReturn(new[] { "MathCurve", "VisualPoints", "VisualCurve", "ControlPoints", "Stations", "LocalXs" })]
        public static Dictionary<string, object> SetMathCurve2D(Plane plane, List<int> ids, List<string> types, List<double> lengths, List<double> rad1s,
            List<double> rad2s, List<double> angles, double tol = 0.1)
        {
            MathCurve2D mathCurve = new MathCurve2D(plane, ids, types, lengths, rad1s, rad2s, angles, tol);
            return new Dictionary<string, object>
            {
                { "MathCurve", mathCurve },
                { "VisualPoints", mathCurve.VisualPoints },
                { "VisualCurve", mathCurve.VisualCurve },
                { "ControlPoints", mathCurve.ControlPoints },
                { "Stations", mathCurve.Stations },
                { "LocalXs", mathCurve.LocalXs }
            };
        }

        public static Point PointAtParamter(MathCurve2D mathCurve, double param) => mathCurve.PointAtParameter(param);
        public static Vector TangentAtParameter(MathCurve2D mathCurve, double param) => mathCurve.TangentAtParameter(param);
        public static Plane PlaneAtParameter(MathCurve2D mathCurve, double param) => mathCurve.PlaneAtParameter(param);
        public static MathCurve2D SetPlane(MathCurve2D mathCurve, Plane plane)
        {
            return mathCurve.SetPlane(plane);
        }

    }


}
