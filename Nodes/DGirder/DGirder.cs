using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using KDHBridge_DYN48.Element.MathCurve;
using KDHBridge_DYN48.Element.Section;
using KDHBridge_DYN48.Girder;
using KDHBridge_DYN48.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Nodes.DGirder
{
    public static class DGirder
    {

        public static Girder.Girder SetDGirder(int id, string name, double gLength, double gHeight, double eConcrete)
        {
            Girder.Girder g = new Girder.Girder(id, name, gLength, gHeight, eConcrete);
            return g;
        }

        [MultiReturn(new[] { "Girder", "AlignmentCurve", "ControlPoints", "RefPoint1", "RefPoint2" })]
        public static Dictionary<string, object> SetGAlignment(Girder.Girder Girder, double refX1, double refY1, double refZ1, double refX2, double refY2, double refZ2,
            double camber, double lateral, double inter1 = 0, double inter2 = 0)
        {
            Girder.SetAlignment(refX1, refY1, refZ1, refX2, refY2, refZ2, camber, lateral, inter1, inter2);
            return new Dictionary<string, object>
            {
                { "Girder", Girder },
                { "AlignmentCurve", Girder.Alignment.TopCurve },
                { "ControlPoints", Girder.Alignment.ControlPoints },
                { "RefPoint1", Girder.RefPoint1 },
                { "RefPoint2", Girder.RefPoint2 }
            };
        }

        [MultiReturn(new[] { "Girder", "GSectionDict" })]
        public static Dictionary<string, object> SetSectionDict(Girder.Girder Girder, List<int> ids, List<double> ghs, List<double> tfws, List<double> tfhs, List<double> tfihs, List<double> tfhhs,
            List<double> wws, List<double> bfws, List<double> bfhs, List<double> bfihs, List<double> bfhhs)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                Girder.SetSectionDict(ids[i], ghs[i], tfws[i], tfhs[i], tfihs[i], tfhhs[i], wws[i], bfws[i], bfhs[i], bfihs[i], bfhhs[i]);
            }
            List<IElementSection> sections = Girder.GSectionDict.Values.ToList();
            return new Dictionary<string, object>
            {
                { "Girder", Girder },
                { "GSectionDict", sections }
            };
        }

        [MultiReturn(new[] { "Girder", "GSControlPoints", "GSPlanes", "GSectionPoints", "GSectionCurves" })]
        public static Dictionary<string, object> SetSection(Girder.Girder Girder, List<int> sectionType, List<double> sectionLoc)
        {
            Girder.SetSection(sectionType, sectionLoc);
            return new Dictionary<string, object>
            {
                { "Girder", Girder },
                { "GSControlPoints", Girder.GSectionControlPoints},
                { "GSPlanes", Girder.GSectionPlanes },
                { "GSectionPoints", Girder.GSectionPoints },
                { "GSectionCurves", Girder.GSectionCurves }
            };
        }

        [MultiReturn(new[] { "Girder", "Solid", "SolidParts", "GuideCurves" })]
        public static Dictionary<string, object> SetSolid(Girder.Girder Girder)
        {
            Girder.SetSolid();
            return new Dictionary<string, object>
            {
                { "Girder", Girder },
                { "Solid", Girder.Solid },
                { "SolidParts", Girder.SolidParts },
                { "GuideCurves", Girder.GuideCurves }
            };
        }

        // Tendon


        [MultiReturn(new[] { "Girder", "Tendon", "TendonCurves" })]
        public static Dictionary<string, object> SetTendonProfiles(Girder.Girder Girder, List<int> id, List<MathCurve2D> hProfile, List<MathCurve2D> vProfile,
            List<double> alignParameter, List<double> alignHeight, List<double> diameter, List<double> ductdiameter, 
            List<double> strandDiameter, List<int> strandNumber, List<double> eTendon, List<double>psStress, bool isReset = true)
        {
            if (isReset) Girder.Tendons.Clear();

            for (int i = 0; i < id.Count; i++)
            {
                Girder.SetTendonProfile(id[i], hProfile[i], vProfile[i], alignParameter[i], alignHeight[i], diameter[i], ductdiameter[i],
                    strandDiameter[i], strandNumber[i], eTendon[i], psStress[i]);
            }

            List<Curve> visualCurves = new List<Curve>();
            List<List<Point>> tendonPoints = new List<List<Point>>();
            foreach (GTendon tendon in Girder.Tendons)
            {
                visualCurves.Add(tendon.VisualCurve);
                tendonPoints.Add(tendon.VisualPoints);
            }
            return new Dictionary<string, object>
            {
                { "Girder", Girder },
                { "Tendon", Girder.Tendons },
                { "TendonCurves", visualCurves },
                { "TendonPoints", tendonPoints },
            };
        }

        [MultiReturn(new[] { "Girder", "Tendons", "TendonSolids"})]
        public static Dictionary<string, object> SetTendonSolid(Girder.Girder Girder)
        {
            List<Solid> tenSolids = Girder.GetTendonSolid();
            return new Dictionary<string, object>
            {
                { "Girder", Girder },
                { "Tendons", Girder.Tendons },
                { "TendonSolids", tenSolids }
            };
        }

        // 기능
        [MultiReturn(new[] { "ElementSection", "SectionPlane", "SectionCurve", "Area", "Ix", "Iy", "Mx", "My", "centroidX", "centroidY", "CentroidPoint" } )]
        public static Dictionary<string, object> GetNaiveSectionPropertyAt(Girder.Girder girder, double alignDistance, bool isTapered=true)
        {
            Dictionary<string, object> sectionProperty = girder.GetNaiveSectionPropertyAt(alignDistance, isTapered);
            IElementSection elementSection = (IElementSection)sectionProperty["ElementSection"];
            return new Dictionary<string, object>
            {
                { "ElementSection", elementSection },
                { "SectionPlane", elementSection.SectionPlane },
                { "SectionCurve", elementSection.SectionCurve },
                { "Area", sectionProperty["Area"] },
                { "Ix", sectionProperty["InertiaX"] },
                { "Iy", sectionProperty["InertiaY"] },
                { "Mx", sectionProperty["MomentOfInertiaX"] },
                { "My", sectionProperty["MomentOfInertiaY"] },
                { "centroidX", sectionProperty["CentroidX"] },
                { "centroidY", sectionProperty["CentroidY"] },
                { "CentroidPoint", elementSection.Centroid }
            };
        }

        [MultiReturn(new[] { "TendonPoints", "TendonLocalX", "TendonLocalY" })]
        public static Dictionary<string, object> GetTendonPointAt(Girder.Girder girder, double alignDistance)
        {
            Dictionary<string, List<double>> tendonXY = girder.GetTendonXYAt(alignDistance);
            List<Point> points = girder.GetTendonPointAt(alignDistance);
            return new Dictionary<string, object>
            {
                { "TendonPoints", points },
                { "TendonLocalX", tendonXY["LocalX"] },
                { "TendonLocalY", tendonXY["LocalY"] }
            };
        }

        [MultiReturn(new[] { "ElementSection", "SectionPlane", "SectionCurve", "TendonPoints", "TendonSection", "Area", "Ix", "Iy", "Mx", "My", "centroidX", "centroidY", "CentroidPoint" })]
        public static Dictionary<string, object> GetTendonSectionPropertyAt(Girder.Girder girder, double alignDistance)
        {
            Dictionary<string, object> sectionProperty = girder.GetTendonSectionPropertyAt(alignDistance);
            IElementSection elementSection = sectionProperty["ElementSection"] as IElementSection;
            Plane plane = elementSection.SectionPlane;
            Point centroidPoint = GeometryFunctions.GetPointAt(plane, elementSection.CentroidX, -1 * elementSection.CentroidY, 0);
            return new Dictionary<string, object>
            {
                { "ElementSection", elementSection },
                { "SectionPlane", elementSection.SectionPlane },
                { "SectionCurve", elementSection.SectionCurve },
                { "TendonPoints", sectionProperty["TendonPoints"] },
                { "TendonSection", sectionProperty["TendonSection"] },
                { "Area", sectionProperty["Area"] },
                { "Ix", sectionProperty["InertiaX"] },
                { "Iy", sectionProperty["InertiaY"] },
                { "Mx", sectionProperty["MomentOfInertiaX"] },
                { "My", sectionProperty["MomentOfInertiaY"] },
                { "centroidX", sectionProperty["CentroidX"] },
                { "centroidY", sectionProperty["CentroidY"] },
                { "CentroidPoint", centroidPoint }
            };
        }

        


       

    }
}
