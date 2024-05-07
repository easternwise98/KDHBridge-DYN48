using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;

namespace KDHBridge_DYN48.Element.Section
{
    internal class CirSection : IElementSection
    {
        // Input parameters
        internal double Diameter { get; set; }
        public Dictionary<string, double> SectionDimensions { get; set; }

        // Properties
        public double Area { get; set; }
        public double InertiaX { get; set; }
        public double InertiaY { get; set; }
        public double MomentOfInertiaX { get; set; }
        public double MomentOfInertiaY { get; set; }
        public double CentroidX { get; set; }
        public double CentroidY { get; set; }
        public Dictionary<string, double> SectionProperties { get; set; }

        public Plane SectionPlane { get; set; }
        public List<Point> Vertices { get; set; }
        public Point Centroid { get; set; }
        public Curve SectionCurve { get; set; }

        internal CirSection(Plane sectionPlane, double diameter)
        {
            Diameter = diameter;
            SectionDimensions = new Dictionary<string, double>
            {
                { "Diameter", Diameter }
            };

            Area = GetArea();

            MomentOfInertiaX = GetMomentOfInertiaXAt(0);
            MomentOfInertiaY = GetMomentOfInertiaYAt(0);
            CentroidX = GetCentroidXAt(0);
            CentroidY = GetCentroidYAt(0);
            InertiaX = GetInertiaXAt(0);
            InertiaY = GetInertiaYAt(0);

            SectionProperties = new Dictionary<string, double>
            {
                { "Area", Area },
                { "InertiaX", InertiaX },
                { "InertiaY", InertiaY },
                { "MomentOfInertiaX", MomentOfInertiaX },
                { "MomentOfInertiaY", MomentOfInertiaY },
                { "CentroidX", CentroidX },
                { "CentroidY", CentroidY }
            };

            SectionPlane = sectionPlane;
            SectionCurve = Circle.ByPlaneRadius(sectionPlane, diameter / 2);
            List<double> vert_range = Enumerable.Range(0, 10).Select(x => x / 10.0).ToList();
            Vertices = vert_range.Select(x => SectionCurve.PointAtParameter(x)).ToList();
            Centroid = sectionPlane.Origin;
        }

        // 기준: X, Y 중심점
        public double GetArea() { return Math.PI * Math.Pow(Diameter, 2); }
        public double GetMomentOfInertiaXAt(double u) { return Area * (-1* u); }
        public double GetMomentOfInertiaYAt(double v) { return Area * v; }
        public double GetCentroidXAt(double u) { return u; }
        public double GetCentroidYAt(double v) { return v; }
    
        public double GetInertiaXAt(double u) { return Math.PI * Math.Pow(Diameter, 4) / 64 + Area * Math.Pow(u, 2); }
        public double GetInertiaYAt(double v) { return Math.PI * Math.Pow(Diameter, 4) / 64 + Area * Math.Pow(v, 2); }    

        public IElementSection GetSectionAt(Plane plane)
        {
            return new CirSection(plane, Diameter);
        }

    }
}
