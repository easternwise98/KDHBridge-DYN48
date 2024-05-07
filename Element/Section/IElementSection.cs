using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Element.Section
{
    [IsVisibleInDynamoLibrary(false)]
    public interface IElementSection
    {
        // Properties
        Dictionary<string, double> SectionDimensions { get; }
        Dictionary<string, double> SectionProperties { get; }

        double Area { get; }
        double MomentOfInertiaX { get; }
        double MomentOfInertiaY { get; }
        double CentroidX { get; }
        double CentroidY { get; }
        double InertiaX { get; }
        double InertiaY { get; }


        Plane SectionPlane { get; }
        List<Point> Vertices { get; }
        Point Centroid { get; }
        Curve SectionCurve { get; }

        // Methods
        double GetArea();
        double GetMomentOfInertiaXAt(double u);
        double GetMomentOfInertiaYAt(double v);
        double GetCentroidXAt(double u);
        double GetCentroidYAt(double v);
        double GetInertiaXAt(double u);
        double GetInertiaYAt(double v);

        IElementSection GetSectionAt(Plane plane);

    }
}
