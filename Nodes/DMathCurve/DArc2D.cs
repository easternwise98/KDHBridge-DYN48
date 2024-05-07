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
    public static class DArc2D
    {
        [MultiReturn(new[] { "Arc2D", "VisualPoints", "VisualCurve", "ControlPoints" })]
        public static Dictionary<string, object> SetDArc2D(Plane plane, double radius, double length, double angle = 0)
        {
            Arc2D mathCurve = new Arc2D(plane, radius, length, angle);
            return new Dictionary<string, object>
            {
                { "Arc2D", mathCurve },
                { "VisualPoints", mathCurve.VisualPoints },
                { "VisualCurve", mathCurve.VisualCurve },
                { "ControlPoints", mathCurve.ControlPoints }
            };
        }

        [MultiReturn(new[] { "Point", "Tangent"})]
        public static Dictionary<string, object> EvaluateArc2D(Arc2D mathCurve, double param)
        {
            Point point = mathCurve.PointAtParameter(param);
            Vector tangent = mathCurve.TangentAtParameter(param);

            return new Dictionary<string, object>
            {
                { "Point", point },
                { "Tangent", tangent }
            };
        }

        [MultiReturn(new[] { "Point", "Tangent", "Plane" })]
        public static Dictionary<string, object> EvaluateArc2D2(Arc2D mathCurve, double param)
        {
            Point point = mathCurve.PointAtParameter(param);
            Vector tangent = mathCurve.TangentAtParameter(param);
            Plane plane = mathCurve.PlaneAtParameter(param);

            return new Dictionary<string, object>
            {
                { "Point", point },
                { "Tangent", tangent },
                { "Plane", plane }
            };
        }

        public static Arc2D SetArc2DPlane(Arc2D mathCurve, Plane plane)
        {
            return (Arc2D)mathCurve.SetPlane(plane);
        }

    }


}
