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
    public static class DParabola2D
    {
        [MultiReturn(new[] { "Parabola2D", "VisualPoints", "VisualCurve", "ControlPoints" })]
        public static Dictionary<string, object> SetDParabola2D(Plane plane, double width, double height)
        {
            Parabola2D mathCurve = new Parabola2D(plane, width, height);
            return new Dictionary<string, object>
            {
                { "Parabola2D", mathCurve },
                { "VisualPoints", mathCurve.VisualPoints },
                { "VisualCurve", mathCurve.VisualCurve },
                { "ControlPoints", mathCurve.ControlPoints }
            };
        }

        [MultiReturn(new[] { "Point", "Tangent"})]
        public static Dictionary<string, object> EvaluateParabola2D(Parabola2D mathCurve, double param)
        {
            Point point = mathCurve.PointAtParameter(param);
            Vector tangent = mathCurve.TangentAtParameter(param);

            return new Dictionary<string, object>
            {
                { "Point", point },
                { "Tangent", tangent },
            };
        }

        [MultiReturn(new[] { "Point", "Tangent", "Plane" })]
        public static Dictionary<string, object> EvaluateParabola2D2(Parabola2D mathCurve, double param)
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

        public static Parabola2D SetParabola2DPlane(Parabola2D mathCurve, Plane plane)
        {
            return (Parabola2D)mathCurve.SetPlane(plane);
        }

    }


}
