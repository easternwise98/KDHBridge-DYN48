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
    public static class DMathCurve3D
    {
        [MultiReturn(new[] { "MathCurve", "VisualPoints", "VisualCurve", "ControlPoints" })]
        public static Dictionary<string, object> SetMathCurve3D(Plane plane, MathCurve2D hCurve, MathCurve2D vCurve, double tol = 0.1)
        {
            MathCurve3D mathCurve = new MathCurve3D(plane, hCurve, vCurve, tol);
            return new Dictionary<string, object>
            {
                { "MathCurve", mathCurve },
                { "VisualPoints", mathCurve.VisualPoints },
                { "VisualCurve", mathCurve.VisualCurve },
                { "ControlPoints", mathCurve.ControlPoints }
            };

        }

        public static Point PointAtLocalX(MathCurve3D mathCurve, double x) => mathCurve.PointAtLocalX(x);
        public static Vector TangentAtLocalX(MathCurve3D mathCurve, double x) => mathCurve.TangentAtLocalX(x);
        public static Plane PlaneAtLocalX(MathCurve3D mathCurve, double x) => mathCurve.PlaneAtLocalX(x);

        public static Point PointAtParameter(MathCurve3D mathCurve, double param) => mathCurve.PointAtParameter(param);
        public static Vector TangentAtParameter(MathCurve3D mathCurve, double param) => mathCurve.TangentAtParameter(param);
        public static Plane PlaneAtParameter(MathCurve3D mathCurve, double param) => mathCurve.PlaneAtParameter(param);

        public static MathCurve3D SetPlane(MathCurve3D mathCurve, Plane plane)
        {
            return mathCurve.SetPlane(plane);
        }

    }


}
