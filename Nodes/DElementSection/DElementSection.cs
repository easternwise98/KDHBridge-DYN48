using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using KDHBridge_DYN48.Element.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace KDHBridge_DYN48.Nodes.DElementSection
{
    public static class DElementSection
    {

        [MultiReturn(new[] { "RecSection", "SectionPlane", "SectionCurve", "Vertices", "Centroid", "SectionDimensions", "SectionProperties" })]
        public static Dictionary<string, object> DRecSection(Plane sectionPlane, double width, double height)
        {
            IElementSection Section = new RecSection(sectionPlane, width, height);
            return new Dictionary<string, object>
            {
                { "RecSection", Section },
                { "SectionPlane", Section.SectionPlane },
                { "SectionCurve", Section.SectionCurve },
                { "Vertices", Section.Vertices },
                { "Centroid", Section.Centroid },
                { "SectionDimensions", Section.SectionDimensions },
                { "SectionProperties", Section.SectionProperties }
            };
        }

        [MultiReturn(new[] { "HSection", "SectionPlane", "SectionCurve", "Vertices", "Centroid", "SectionDimensions", "SectionProperties" })]
        public static Dictionary<string, object> DHSection(Plane sectionPlane, double height, double tFW, double tFH, double tFIH, double tFHH,
                       double wW, double bFW, double bFH, double bFIH, double bFHH)
        {
            IElementSection section = new HSection(sectionPlane, height, tFW, tFH, tFIH, tFHH, wW, bFW, bFH, bFIH, bFHH);
            return new Dictionary<string, object>
            {
                { "HSection", section },
                { "SectionPlane", section.SectionPlane },
                { "SectionCurve", section.SectionCurve },
                { "Vertices", section.Vertices },
                { "Centroid", section.Centroid },
                { "SectionDimensions", section.SectionDimensions },
                { "SectionProperties", section.SectionProperties }
            };
        }

        [MultiReturn(new[] { "CircleSection", "SectionPlane", "SectionCurve", "Vertices", "Centroid", "SectionDimensions", "SectionProperties" })]
        public static Dictionary<string, object> DCirSection(Plane sectionPlane, double diameter)
        {
            IElementSection section = new CirSection(sectionPlane, diameter);
            return new Dictionary<string, object>
            {
                { "CircleSection", section },
                { "SectionPlane", section.SectionPlane },
                { "SectionCurve", section.SectionCurve },
                { "Vertices", section.Vertices },
                { "Centroid", section.Centroid },
                { "SectionDimensions", section.SectionDimensions },
                { "SectionProperties", section.SectionProperties }
            };
        }


        [MultiReturn(new[] { "Area", "MomentOfInertiaX", "MomentOfInertiaY", "CentroidX", "CentroidY", "InertiaX", "InertiaY" })]
        public static Dictionary<string, double> GetSectionProperties(IElementSection section)
        {
            double Area = section.GetArea();
            double MomentOfInertiaX = section.GetMomentOfInertiaXAt(0);
            double MomentOfInertiaY = section.GetMomentOfInertiaYAt(0);
            double CentroidX = section.GetCentroidXAt(0);
            double CentroidY = section.GetCentroidYAt(0);
            double InertiaX = section.GetInertiaXAt(0);
            double InertiaY = section.GetInertiaYAt(0);

            return new Dictionary<string, double>
            {
                { "Area", Area },
                { "MomentOfInertiaX", MomentOfInertiaX },
                { "MomentOfInertiaY", MomentOfInertiaY },
                { "CentroidX", CentroidX },
                { "CentroidY", CentroidY },
                { "InertiaX", InertiaX },
                { "InertiaY", InertiaY }
            };
        }
    }
}
