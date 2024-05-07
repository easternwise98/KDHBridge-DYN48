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
    public static class DLine2D
    {
        [MultiReturn(new[] { "Line2D", "VisualPoints", "VisualCurve", "ControlPoints" })]
        public static Dictionary<string, object> SetDLine2D(Plane plane, double length, double angle = 0)
        {
            Line2D mathCurve = new Line2D(plane, length, angle);
            return new Dictionary<string, object>
            {
                { "Line2D", mathCurve },
                { "VisualPoints", mathCurve.VisualPoints },
                { "VisualCurve", mathCurve.VisualCurve },
                { "ControlPoints", mathCurve.ControlPoints }
            };
        }

        [MultiReturn(new[] { "Point", "Tangent"})]
        public static Dictionary<string, object> EvaluateLine2D(Line2D mathCurve, double param)
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
        public static Dictionary<string, object> EvaluateLine2D2(Line2D mathCurve, double param)
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

        public static Line2D SetLine2DPlane(Line2D mathCurve, Plane plane)
        {
            return (Line2D)mathCurve.SetPlane(plane);
        }

    }


}
