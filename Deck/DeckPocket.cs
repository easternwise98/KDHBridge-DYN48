using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Deck
{
    [IsVisibleInDynamoLibrary(false)]
    public class DeckPocket
    {
        public Deck HostDeck { get; set; }
        public double StartWidth { get; set; }
        public double StartLength { get; set; }
        public double EndWidth { get; set; }
        public double EndLength { get; set; }
        public double PocketDepth { get; set; }
        public double WStart { get; set; }
        public double LStart { get; set; }
        public double DStart { get; set; }
        public double WSpacing { get; set; }
        public double LSpacing { get; set; }
        public int WCount { get; set; }
        public int LCount { get; set; }

        // 결과 
        public List<List<Point>> StartSectionPoints { get; set; }
        public List<Curve> StartSectionCurves { get; set; }
        public List<List<Point>> EndSectionPoints { get; set; }
        public List<Curve> EndSectionCurves { get; set; }
        public List<Point> StartControlPoints { get; set; }
        public List<Point> EndControlPoints { get; set; }
        public List<Solid> PocketSolids { get; set; }

        public DeckPocket(Deck hostDeck, double pocketDepth, double startWidth, double startLength,
            double endWidth, double endLength)
        {
            HostDeck = hostDeck;
            PocketDepth = pocketDepth;
            StartWidth = startWidth;
            StartLength = startLength;
            EndWidth = endWidth;
            EndLength = endLength;
        }

        public Dictionary<string, object> GetShearPocketAtLocation(double locWidth, double locLength)
        {
            double coef = 1.01;
            double tol = -0.01 * PocketDepth;
            // 상부 
            double startLoc1 = locLength - StartLength / 2;
            double startLoc2 = locLength + StartLength / 2;
            DeckSection dsu1 = HostDeck.GetSectionAtParameter(startLoc1 / HostDeck.DeckLength);
            DeckSection dsu2 = HostDeck.GetSectionAtParameter(startLoc2 / HostDeck.DeckLength);
            Point pu1 = dsu1.GetSectionPointAtWidth(locWidth - StartWidth / 2, tol);
            Point pu2 = dsu1.GetSectionPointAtWidth(locWidth + StartWidth / 2, tol);
            Point pu3 = dsu2.GetSectionPointAtWidth(locWidth + StartWidth / 2, tol);
            Point pu4 = dsu2.GetSectionPointAtWidth(locWidth - StartWidth / 2, tol);
            List<Point> startPoints = new List<Point> { pu1, pu2, pu3, pu4 };
            List<Point> startCurvePoints = new List<Point> { pu1, pu2, pu3, pu4, pu1 };
            Curve startCurve = PolyCurve.ByPoints(startCurvePoints);

            // 상부 중앙점
            DeckSection dsu = HostDeck.GetSectionAtParameter(locLength / HostDeck.DeckLength);
            Point pu = dsu.GetSectionPointAtWidth(locWidth, 0);

            // 하부
            double endLoc1 = locLength - EndLength / 2;
            double endLoc2 = locLength + EndLength / 2;
            DeckSection dsl1 = HostDeck.GetSectionAtParameter(endLoc1 / HostDeck.DeckLength);
            DeckSection dsl2 = HostDeck.GetSectionAtParameter(endLoc2 / HostDeck.DeckLength);
            Point pl1 = dsl1.GetSectionPointAtWidth(locWidth - EndWidth / 2, Math.Max(PocketDepth, coef * dsl1.MaxDepth));
            Point pl2 = dsl1.GetSectionPointAtWidth(locWidth + EndWidth / 2, Math.Max(PocketDepth, coef * dsl1.MaxDepth));
            Point pl3 = dsl2.GetSectionPointAtWidth(locWidth + EndWidth / 2, Math.Max(PocketDepth, coef * dsl2.MaxDepth));
            Point pl4 = dsl2.GetSectionPointAtWidth(locWidth - EndWidth / 2, Math.Max(PocketDepth, coef * dsl2.MaxDepth));
            List<Point> endPoints = new List<Point> { pl1, pl2, pl3, pl4 };
            List<Point> endCurvePoints = new List<Point> { pl1, pl2, pl3, pl4, pl1 };
            Curve endCurve = PolyCurve.ByPoints(endCurvePoints);

            // 하부 중앙점
            DeckSection dsl = HostDeck.GetSectionAtParameter(locLength / HostDeck.DeckLength);
            Point pl = dsl.GetSectionPointAtWidth(locWidth, Math.Max(PocketDepth, coef * dsl.MaxDepth));

            return new Dictionary<string, object>
            {
                {"StartPoints", startPoints},
                {"StartCurve", startCurve},
                {"StartPoint", pu},
                {"EndPoints", endPoints},
                {"EndCurve", endCurve},
                {"EndPoint", pl},
                {"StartControlPoint", pu},
                {"EndControlPoint", pl}
            };
        }

        public Dictionary<string, object> GetConnectorPocketAtLocation(double locWidth, double locLength, bool side)
        {
            double tol = -0.01 * PocketDepth;
            // 상부
            double startLoc1 = locLength;
            double startLoc2 = side ? locLength + StartLength : locLength - StartLength;

            DeckSection dsu1 = HostDeck.GetSectionAtParameter(startLoc1 / HostDeck.DeckLength);
            DeckSection dsu2 = HostDeck.GetSectionAtParameter(startLoc2 / HostDeck.DeckLength);
            Point pu1 = dsu1.GetSectionPointAtWidth(locWidth - StartWidth / 2, tol);
            Point pu2 = dsu1.GetSectionPointAtWidth(locWidth + StartWidth / 2, tol);
            Point pu3 = dsu2.GetSectionPointAtWidth(locWidth + StartWidth / 2, tol);
            Point pu4 = dsu2.GetSectionPointAtWidth(locWidth - StartWidth / 2, tol);
            List<Point> startPoints = new List<Point> { pu1, pu2, pu3, pu4 };
            List<Point> startCurvePoints = new List<Point> { pu1, pu2, pu3, pu4, pu1 };
            Curve startCurve = PolyCurve.ByPoints(startCurvePoints);

            // 상부 중앙점
            DeckSection dsu = HostDeck.GetSectionAtParameter(locLength / HostDeck.DeckLength);
            Point pu = dsu.GetSectionPointAtWidth(locWidth, 0);

            // 하부
            double endLoc1 = locLength;
            double endLoc2 = side ? locLength + EndLength : locLength - EndLength;
            DeckSection dsl1 = HostDeck.GetSectionAtParameter(endLoc1 / HostDeck.DeckLength);
            DeckSection dsl2 = HostDeck.GetSectionAtParameter(endLoc2 / HostDeck.DeckLength);
            Point pl1 = dsl1.GetSectionPointAtWidth(locWidth - EndWidth / 2, PocketDepth);
            Point pl2 = dsl1.GetSectionPointAtWidth(locWidth + EndWidth / 2, PocketDepth);
            Point pl3 = dsl2.GetSectionPointAtWidth(locWidth + EndWidth / 2, PocketDepth);
            Point pl4 = dsl2.GetSectionPointAtWidth(locWidth - EndWidth / 2, PocketDepth);
            List<Point> endPoints = new List<Point> { pl1, pl2, pl3, pl4 };
            List<Point> endCurvePoints = new List<Point> { pl1, pl2, pl3, pl4, pl1 };
            Curve endCurve = PolyCurve.ByPoints(endCurvePoints);

            // 하부 중앙점
            DeckSection dsl = HostDeck.GetSectionAtParameter(locLength / HostDeck.DeckLength);
            Point pl = dsl.GetSectionPointAtWidth(locWidth, PocketDepth);

            return new Dictionary<string, object>
            {
                {"StartPoints", startPoints},
                {"StartCurve", startCurve},
                {"StartPoint", pu},
                {"EndPoints", endPoints},
                {"EndCurve", endCurve},
                {"EndPoint", pl},
                {"StartControlPoint", pu},
                {"EndControlPoint", pl}
            };
        }

        public void ArrangeShearPocket(double wStart, double lStart,
            double wSpacing, double lSpacing, int wCount, int lCount)
        {
            WStart = wStart;
            LStart = lStart;
            WSpacing = wSpacing;
            LSpacing = lSpacing;
            WCount = wCount;
            LCount = lCount;

            StartSectionPoints = new List<List<Point>>();
            StartSectionCurves = new List<Curve>();
            EndSectionPoints = new List<List<Point>>();
            EndSectionCurves = new List<Curve>();
            StartControlPoints = new List<Point>();
            EndControlPoints = new List<Point>();
            PocketSolids = new List<Solid>();

            for (int i = 0; i < wCount; i++)
            {
                for (int j = 0; j < lCount; j++)
                {
                    double locWidth = WStart + i * WSpacing;
                    double locLength = LStart + j * LSpacing;
                    Dictionary<string, object> pocketDict = GetShearPocketAtLocation(locWidth, locLength);
                    StartSectionPoints.Add((List<Point>)pocketDict["StartPoints"]);
                    StartSectionCurves.Add((Curve)pocketDict["StartCurve"]);
                    StartControlPoints.Add((Point)pocketDict["StartControlPoint"]);
                    EndSectionPoints.Add((List<Point>)pocketDict["EndPoints"]);
                    EndSectionCurves.Add((Curve)pocketDict["EndCurve"]);
                    EndControlPoints.Add((Point)pocketDict["EndControlPoint"]);
                    PocketSolids.Add(Solid.ByLoft(new List<Curve> { (Curve)pocketDict["StartCurve"], (Curve)pocketDict["EndCurve"] }));
                }
            }
        }

        public void ArrangeConnectorPocket(double wStart, double lStart,
                       double wSpacing, double lSpacing, int wCount, int lCount, bool side)
        {
            WStart = wStart;
            LStart = lStart;
            WSpacing = wSpacing;
            LSpacing = lSpacing;
            WCount = wCount;
            LCount = lCount;

            StartSectionPoints = new List<List<Point>>();
            StartSectionCurves = new List<Curve>();
            EndSectionPoints = new List<List<Point>>();
            EndSectionCurves = new List<Curve>();
            StartControlPoints = new List<Point>();
            EndControlPoints = new List<Point>();
            PocketSolids = new List<Solid>();
            for (int i = 0; i < wCount; i++)
            {
                for (int j = 0; j < lCount; j++)
                {
                    double locWidth = WStart + i * WSpacing;
                    double locLength = LStart + j * LSpacing;
                    Dictionary<string, object> pocketDict = GetConnectorPocketAtLocation(locWidth, locLength, side);
                    StartSectionPoints.Add((List<Point>)pocketDict["StartPoints"]);
                    StartSectionCurves.Add((Curve)pocketDict["StartCurve"]);
                    StartControlPoints.Add((Point)pocketDict["StartControlPoint"]);
                    EndSectionPoints.Add((List<Point>)pocketDict["EndPoints"]);
                    EndSectionCurves.Add((Curve)pocketDict["EndCurve"]);
                    EndControlPoints.Add((Point)pocketDict["EndControlPoint"]);
                    PocketSolids.Add(Solid.ByLoft(new List<Curve> { (Curve)pocketDict["StartCurve"], (Curve)pocketDict["EndCurve"] }));
                }
            }
        }

    }

    
}
