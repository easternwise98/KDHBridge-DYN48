using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using KDHBridge_DYN48.Deck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Nodes.DDeck
{
    public static class DDeck
    {
        public static Deck.Deck SetDeck(int id, string name, double deckWidth, double deckLength, double deckDepth,
            double elevationRatio, double haunchRatio = 1 / 3)
        {
            return new Deck.Deck(id, name, deckWidth, deckLength, deckDepth, elevationRatio, haunchRatio);
        }

        [MultiReturn(new[] { "DeckSection", "SectionCurve", "SectionPoints" })]
        public static Dictionary<string, object> GetSectionByData(Deck.Deck deck, Plane sectionPlane, double startCantWidth, double endCantWidth, bool side,
                       double startWallWidth, double endWallWidth, double girderWidth, int girderNumber, bool isFull = false)
        {
            DeckSection section = deck.GetSectionByData(sectionPlane, startCantWidth, endCantWidth, side, startWallWidth, endWallWidth, girderWidth, girderNumber, isFull);
            return new Dictionary<string, object>
            {
                { "DeckSection", section },
                { "SectionCurve", section.SectionCurve },
                { "SectionPoints", section.SectionPoints }
            };
        }

        [MultiReturn(new[] { "Deck", "DeckAlignment", "StartPlane", "EndPlane", "StartCurve", "EndCurve"})]
        public static Dictionary<string, object> SetDeckSection(Deck.Deck deck, DeckSection startSection, DeckSection endSection, Plane startPlane, Plane endPlane)
        {
            DeckSection start = startSection.GetSectionAtPlane(startPlane);
            DeckSection end = endSection.GetSectionAtPlane(endPlane);
            deck.SetSection(start, end);

            return new Dictionary<string, object>
            {
                { "Deck", deck },
                { "DeckAlignment", deck.DeckAlignment },
                { "StartPlane", deck.StartSectionPlane },
                { "EndPlane", deck.EndSectionPlane },
                { "StartCurve", deck.StartDeckSection.SectionCurve },
                { "EndCurve", deck.EndDeckSection.SectionCurve }

            };
        }

        [MultiReturn(new[] { "Deck", "Solid" })]
        public static Dictionary<string, object> SetDeckSolid(Deck.Deck deck)
        {
            deck.SetSolid();
            return new Dictionary<string, object>
            {
                { "Deck", deck },
                { "Solid", deck.Solid }
            };
        }

        [MultiReturn(new[] { "Deck", "ShearPocket", "StartSectionPoints", "StartSectionCurve", "StartControlPoint",
            "EndSectionPoints", "EndSectionCurve", "EndControlPoint", "Solid"})]
        public static Dictionary<string, object> SetShearPocket(Deck.Deck deck, double pocketDepth, double startWidth, double startLength, double endWidth, double endLength,
            double wStart, double lStart, double wSpacing, double lSpacing, int wCount, int lCount)
        {
            deck.SetShearPocket(pocketDepth, startWidth, startLength, endWidth, endLength, wStart, lStart, wSpacing, lSpacing, wCount, lCount);
            return new Dictionary<string, object>
            {
                { "Deck", deck },
                { "ShearPocket", deck.ShearPocket },
                { "StartSectionPoints", deck.ShearPocket.StartSectionPoints },
                { "StartSectionCurve", deck.ShearPocket.StartSectionPoints },
                { "StartControlPoint", deck.ShearPocket.StartSectionPoints },
                { "EndSectionPoints", deck.ShearPocket.StartSectionPoints },
                { "EndSectionCurve", deck.ShearPocket.StartSectionPoints },
                { "EndControlPoint", deck.ShearPocket.StartSectionPoints },
                { "Solid", deck.ShearPocket.PocketSolids }
            };
        }

        [MultiReturn(new[] {"Deck", "StartConnectorPocket", "StartSectionPoints", "StartSectionCurve", "StartControlPoint",
            "EndSectionPoints", "EndSectionCurve", "EndControlPoint", "Solid"})]
        public static Dictionary<string, object> SetConnectorPocket(Deck.Deck deck, double pocketDepth, double startWidth, double startLength, double endWidth, double endLength,
            double wStart, double lStart, double wSpacing, double lSpacing, int wCount, int lCount)
        {
            deck.SetStartConnectorPocket(pocketDepth, startWidth, startLength, endWidth, endLength, wStart, lStart, wSpacing, lSpacing, wCount, lCount);

            return new Dictionary<string, object>
            {
                { "Deck", deck },
                { "StartConnectorPocket", deck.StartConnectorPocket },
                { "StartSectionPoints", deck.StartConnectorPocket.StartSectionPoints },
                { "StartSectionCurve", deck.StartConnectorPocket.StartSectionCurves },
                { "StartControlPoint", deck.StartConnectorPocket.StartControlPoints },
                { "EndSectionPoints", deck.StartConnectorPocket.EndSectionPoints },
                { "EndSectionCurve", deck.StartConnectorPocket.EndSectionCurves },
                { "EndControlPoint", deck.StartConnectorPocket.EndControlPoints },
                { "Solid", deck.StartConnectorPocket.PocketSolids }
            };
        }

        [MultiReturn(new[] { "Deck", "EndConnectorPocket", "StartSectionPoints", "StartSectionCurve", "StartControlPoint",
                   "EndSectionPoints", "EndSectionCurve", "EndControlPoint", "Solid" })]
        public static Dictionary<string, object> SetEndConnectorPocket(Deck.Deck deck, double pocketDepth, double startWidth, double startLength, double endWidth, double endLength,
            double wStart, double lStart, double wSpacing, double lSpacing, int wCount, int lCount)
        {
            deck.SetEndConnectorPocket(pocketDepth, startWidth, startLength, endWidth, endLength, wStart, lStart, wSpacing, lSpacing, wCount, lCount);

            return new Dictionary<string, object>
            {
                { "Deck", deck },
                { "EndConnectorPocket", deck.EndConnectorPocket },
                { "StartSectionPoints", deck.EndConnectorPocket.StartSectionPoints },
                { "StartSectionCurve", deck.EndConnectorPocket.StartSectionCurves },
                { "StartControlPoint", deck.EndConnectorPocket.StartControlPoints },
                { "EndSectionPoints", deck.EndConnectorPocket.EndSectionPoints },
                { "EndSectionCurve", deck.EndConnectorPocket.EndSectionCurves },
                { "EndControlPoint", deck.EndConnectorPocket.EndControlPoints },
                { "Solid", deck.EndConnectorPocket.PocketSolids }
            };
        }

        [MultiReturn(new[] { "Deck", "ShearRebar", "ShearRebarCurve"})]
        public static Dictionary<string, object> SetShearRebar(Deck.Deck deck, List<string> names, List<double> diameters, List<double> startLocations, List<double> startWidths, List<double> startHeights,
            List<double> spacings, List<double> nums, List<List<double>> lengthLists, List<List<double>> angleHLists, List<List<double>> anglePlaneLists, bool isClear=true)
        {
            if (isClear) deck.ShearRebar.Clear(); deck.ShearRebarCurves.Clear();
            deck.SetShearRebarProfiles(names, diameters, startLocations, startWidths, startHeights, spacings, nums, lengthLists, angleHLists, anglePlaneLists);
            return new Dictionary<string, object>
            {
                { "Deck", deck },
                { "ShearRebar", deck.ShearRebar },
                { "ShearRebarCurve", deck.ShearRebarCurves },
            };
        }

        [MultiReturn(new[] { "Deck", "MainRebar", "MainRebarCurve" })]
        public static Dictionary<string, object> SetMainRebar(Deck.Deck deck, List<string> names, List<double> diameters, List<double> startLocations, List<double> startWidths, List<double> startHeights,
                       List<double> spacings, List<double> nums, List<List<double>> lengthLists, List<List<double>> angleHLists, List<List<double>> anglePlaneLists, bool isClear=true)
        {
            if (isClear) deck.MainRebar.Clear(); deck.MainRebarCurves.Clear();
            deck.SetMainRebarProfiles(names, diameters, startLocations, startWidths, startHeights, spacings, nums, lengthLists, angleHLists, anglePlaneLists);
            return new Dictionary<string, object>
            {
                { "Deck", deck },
                { "MainRebar", deck.MainRebar },
                { "MainRebarCurve", deck.MainRebarCurves }
            };
        }
        public static List<Solid> SetShearRebarSolid(Deck.Deck deck)
        {
            List<Solid> solids = new List<Solid>();
            foreach (DeckRebar r in deck.ShearRebar)
            {
                r.SetRebarSolid();
                solids.AddRange(r.RebarSolids);
            }
            return solids;
        }
        public static List<Solid> SetMainRebarSolid(Deck.Deck deck)
        {
            List<Solid> solids = new List<Solid>();
            foreach (DeckRebar r in deck.MainRebar)
            {
                r.SetRebarSolid();
                solids.AddRange(r.RebarSolids);
            }
            return solids;
        }

        // 기능
        [MultiReturn(new[] { "Point", "Vector", "Plane" })]
        public static Dictionary<string, object> EvaluateDeck(Deck.Deck deck, double param)
        {
            Point point = deck.PointAtParameter(param);
            Vector tangent = deck.TangentAtParameter(param);
            Plane plane = deck.PlaneAtParameter(param);

            return new Dictionary<string, object>
            {
                { "Point", point },
                { "Vector", tangent },
                { "Plane", plane }
            };
        }

        [MultiReturn(new[] { "DeckSection", "SectionCurve", "SectionPoints" })]
        public static Dictionary<string, object> GetDeckSectionAtParameter(Deck.Deck deck, double param)
        {
            DeckSection section = deck.GetSectionAtParameter(param);
            return new Dictionary<string, object>
            {
                { "DeckSection", section },
                { "SectionCurve", section.SectionCurve },
                { "SectionPoints", section.SectionPoints }
            };
        }

        public static Point GetDeckSectionPointAt(DeckSection section, double u, double v)
        {
            return section.GetSectionPointAtWidth(u, v);
        }

    }
}
