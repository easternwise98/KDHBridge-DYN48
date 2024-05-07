using Autodesk.DesignScript.Geometry;
using KDHBridge_DYN48.Element.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Nodes.DElementSection
{
    public class DHSection
    {
        private HSection Section { get; }
        public List<Point> Vertices { get; }
        public Curve SectionCurve { get; }
        public Point Centeroid { get; }
        public Dictionary<string, double> Parameters { get; }
        public Dictionary<string, double> Properties { get; }

        public DHSection(Plane sectionPlane, double height, double tFW, double tFH, double tFIH, double tFHH,
                       double wW, double bFW, double bFH, double bFIH, double bFHH)
        {
            Section = new HSection(sectionPlane, height, tFW, tFH, tFIH, tFHH, wW, bFW, bFH, bFIH, bFHH);
            Vertices = Section.Vertices;
            SectionCurve = Section.SectionCurve;
            Centeroid = Section.Centroid;
            Parameters = Section.SectionDimensions;
            Properties = Section.SectionProperties;
        }


    }
}
