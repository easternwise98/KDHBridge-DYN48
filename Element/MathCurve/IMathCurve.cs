using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KDHBridge_DYN48.Element.MathCurve
{
    [IsVisibleInDynamoLibrary(false)]
    public interface IMathCurve
    {
        Plane StartPlane { get; }
        Plane EndPlane { get; }
        double Length { get; }
        Dictionary<string, double> Parameters { get; }
        Dictionary<string, double> Properties { get; }
        List<Point> ControlPoints { get; }
        List<Point> VisualPoints { get; }
        Curve VisualCurve { get; }

        Point PointAtParameter(double param);
        Vector TangentAtParameter(double param);
        Plane PlaneAtParameter(double param);

        IMathCurve SetPlane(Plane plane);
    }
}
