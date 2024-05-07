using Autodesk.DesignScript.Geometry;
using KDHBridge_DYN48.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KDHBridge_DYN48.Element.Section
{
    internal class RecSection : IElementSection
    {
        // Input parameters
        internal double Width { get; set; }
        internal double Height { get; set; }
        public Dictionary<string, double> SectionDimensions { get; set; }
        // Properties
        public double Area { get; set; }
        public double MomentOfInertiaX { get; set; }
        public double MomentOfInertiaY { get; set; }
        public double CentroidX { get; set; }
        public double CentroidY { get; set; }
        public double InertiaX { get; set; }
        public double InertiaY { get; set; }
        public Dictionary<string, double> SectionProperties { get; set; }

        public Plane SectionPlane { get; set; }
        public List<Point> Vertices { get; set; }
        public Point Centroid { get; set; }
        public Curve SectionCurve { get; set; }

        internal RecSection(Plane sectionPlane, double width, double height)
        {
            Width = width;
            Height = height;
            SectionDimensions = new Dictionary<string, double>
            {
                { "Width", Width },
                { "Height", Height }
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
            Vertices = new List<Point>
            {
                GeometryFunctions.GetPointAt(sectionPlane, Width / 2, 0, 0),
                GeometryFunctions.GetPointAt(sectionPlane, Width / 2, -Height, 0),
                GeometryFunctions.GetPointAt(sectionPlane, -Width / 2, -Height, 0),
                GeometryFunctions.GetPointAt(sectionPlane, -Width / 2, 0, 0)
            };

            SectionCurve = PolyCurve.ByPoints(Vertices, true);
            Centroid = GeometryFunctions.GetPointAt(sectionPlane, GetCentroidXAt(0), -1 * GetCentroidYAt(0), 0);
        }

        // 
        public double GetArea() => Width * Height;
        // X축 : 상면 Y축 : 중앙
        public double GetMomentOfInertiaYAt(double v) { return Area * (Height / 2 - v); }
        public double GetMomentOfInertiaXAt(double u) { return Area * -1 * u; }
        public double GetCentroidXAt(double u) { return GetMomentOfInertiaXAt(u) / Area; }
        public double GetCentroidYAt(double v) { return GetMomentOfInertiaYAt(v) / Area; }
        public double GetInertiaYAt(double v) { return Width * Math.Pow(Height, 3) / 12 + Area * Math.Pow(CentroidY - v, 2); }
        public double GetInertiaXAt(double u) { return Height * Math.Pow(Width, 3) / 12 + Area * Math.Pow(u, 2); }

        public IElementSection GetSectionAt(Plane plane)
        {
            return new RecSection(plane, Width, Height);
        }
    }
}
