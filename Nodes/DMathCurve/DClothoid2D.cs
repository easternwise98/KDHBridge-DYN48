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
    public static class DClothoid2D
    {
        [MultiReturn(new[] { "Clothoid2D", "VisualPoints", "VisualCurve", "ControlPoints" })]
        public static Dictionary<string, object> SetDClothoid2D(Plane plane, double length, double radius1, double radius2)
        {
            Clothoid2D mathCurve = new Clothoid2D(plane, radius1, radius2, length);
            return new Dictionary<string, object>
            {
                { "Clothoid2D", mathCurve },
                { "VisualPoints", mathCurve.VisualPoints },
                { "VisualCurve", mathCurve.VisualCurve },
                { "ControlPoints", mathCurve.ControlPoints }
            };
        }

        [MultiReturn(new[] { "Point", "Tangent"})]
        public static Dictionary<string, object> EvaluateClothoid2D(Clothoid2D mathCurve, double param)
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
        public static Dictionary<string, object> EvaluateClothoid2D2(Clothoid2D mathCurve, double param)
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

        public static Clothoid2D SetClothoid2DPlane(Clothoid2D mathCurve, Plane plane)
        {
            return (Clothoid2D)mathCurve.SetPlane(plane);
        }

    }


}
