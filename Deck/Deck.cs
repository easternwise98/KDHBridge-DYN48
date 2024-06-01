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
    public class Deck
    {
        // Meta Data
        public int ID { get; set; }
        public string Name { get; set; }
        public double DeckWidth { get; set; }
        public double DeckDepth { get; set; }
        public double DeckLength { get; set; }
        public double ElevationRatio { get; set; }
        public double HaunchRatio { get; set; }

        // Section
        public Plane StartSectionPlane { get; set; }
        public Plane EndSectionPlane { get; set; }
        public DeckSection StartDeckSection { get; set; }
        public DeckSection EndDeckSection { get; set; }
        public Curve DeckAlignment { get; set; }

        // Solid
        public Solid Solid { get; set; }

        // Pocket
        public DeckPocket ShearPocket { get; set; }
        public DeckPocket StartConnectorPocket { get; set; }
        public DeckPocket EndConnectorPocket { get; set; }

        // Rebar
        public List<DeckRebar> MainRebar { get; set; }
        public List<Curve> MainRebarCurves { get; set; }
        public List<DeckRebar> ShearRebar { get; set; }
        public List<Curve> ShearRebarCurves { get; set; }

        public Deck(int id, string name, double deckWidth, double deckLength, double deckDepth,
            double elevationRatio, double haunchRatio = 1 / 3)
        {
            ID = id;
            Name = name;
            DeckWidth = deckWidth;
            DeckLength = deckLength;
            DeckDepth = deckDepth;
            ElevationRatio = elevationRatio;
            HaunchRatio = haunchRatio;

            // Rebar
            MainRebar = new List<DeckRebar>();
            MainRebarCurves = new List<Curve>();
            ShearRebar = new List<DeckRebar>();
            ShearRebarCurves = new List<Curve>();

        }

        public DeckSection GetSectionByData(Plane sectionPlane, double startCantWidth, double endCantWidth, bool side,
                       double startWallWidth, double endWallWidth, double girderWidth, int girderNumber, bool isFull = false)
        {
            DeckSection ds = new DeckSection(sectionPlane, DeckWidth, DeckDepth,
                               startCantWidth, endCantWidth, ElevationRatio, HaunchRatio, side);
            ds.SetSection(startWallWidth, endWallWidth, girderWidth, girderNumber, isFull);
            return ds;
        }

        public void SetStartSectionByData(Plane startSection, double startCantWidth, double endCantWidth,
            double startWallWidth, double endWallWidth, double girderWidth, int girderNumber, bool side = true, bool isFull = false)
        {
            StartSectionPlane = startSection;
            StartDeckSection = GetSectionByData(startSection, startCantWidth, endCantWidth, side,
                               startWallWidth, endWallWidth, girderWidth, girderNumber, isFull);
        }
        public void SetEndSectionByData(Plane endSection, double startCantWidth, double endCantWidth,
                       double startWallWidth, double endWallWidth, double girderWidth, int girderNumber, bool side = true, bool isFull = false)
        {
            EndSectionPlane = endSection;
            EndDeckSection = GetSectionByData(endSection, startCantWidth, endCantWidth, side,
                               startWallWidth, endWallWidth, girderWidth, girderNumber, isFull);
        }

        public void SetSection(DeckSection startSection, DeckSection endSection)
        {
            StartDeckSection = startSection;
            EndDeckSection = endSection;
            StartSectionPlane = startSection.SectionPlane;
            EndSectionPlane = endSection.SectionPlane;
            DeckAlignment = Line.ByStartPointEndPoint(StartSectionPlane.Origin, EndSectionPlane.Origin);
        }

        // 솔리드
        public void SetSolid()
        {
            Curve startSectionCurve = StartDeckSection.SectionCurve;
            Curve endSectionCurve = EndDeckSection.SectionCurve;
            Solid = Solid.ByLoft(new List<Curve>() { startSectionCurve, endSectionCurve }, new List<Curve>() { DeckAlignment });
        }

        // 연결부
        public void SetShearPocket(double pocketDepth, double startWidth, double startLength, double endWidth, double endLength,
            double wStart, double lStart, double wSpacing, double lSpacing, int wCount, int lCount)
        {
            DeckPocket shaerPocket = new DeckPocket(this, pocketDepth, startWidth, startLength, endWidth, endLength);
            shaerPocket.ArrangeShearPocket(wStart, lStart, wSpacing, lSpacing, wCount, lCount);
            ShearPocket = shaerPocket;
        }
        public void SetStartConnectorPocket(double pocketDepth, double startWidth, double startLength, double endWidth, double endLength,
            double wStart, double lStart, double wSpacing, double lSpacing, int wCount, int lCount)
        {
            DeckPocket startPocket = new DeckPocket(this, pocketDepth, startWidth, startLength, endWidth, endLength);
            startPocket.ArrangeConnectorPocket(wStart, lStart, wSpacing, lSpacing, wCount, lCount, true);
            StartConnectorPocket = startPocket;
        }
        public void SetEndConnectorPocket(double pocketDepth, double startWidth, double startLength, double endWidth, double endLength,
            double wStart, double lStart, double wSpacing, double lSpacing, int wCount, int lCount)
        {
            DeckPocket shaerPocket = new DeckPocket(this, pocketDepth, startWidth, startLength, endWidth, endLength);
            shaerPocket.ArrangeConnectorPocket(wStart, lStart, wSpacing, lSpacing, wCount, lCount, false);
            EndConnectorPocket = shaerPocket;
        }

        // 철근
        public void SetShearRebarProfiles(List<string> names, List<double> diameters, List<double> startLocations, List<double> startWidths, List<double> startHeights,
            List<double> spacings, List<double> nums, List<List<double>> lengthLists, List<List<double>> angleHLists, List<List<double>> anglePlaneLists)
        {
            for (int i = 0; i < names.Count; i++)
            {
                DeckRebar deckRebar = new DeckRebar(this, names[i], diameters[i]);
                deckRebar.SetShearRebarCurve(startLocations[i], startWidths[i], startHeights[i], spacings[i], nums[i],
                                                          lengthLists[i], angleHLists[i], anglePlaneLists[i]);
                ShearRebar.Add(deckRebar);
                ShearRebarCurves.AddRange(deckRebar.RebarCurves);
            }
        }

        public void SetMainRebarProfiles(List<string> names, List<double> diameters, List<double> startLocations, List<double> startWidths, List<double> startHeights,
            List<double> spacings, List<double> nums, List<List<double>> lengthLists, List<List<double>> angleHLists, List<List<double>> anglePlaneLists)
        {
            for (int i = 0; i < names.Count; i++)
            {
                DeckRebar deckRebar = new DeckRebar(this, names[i], diameters[i]);
                deckRebar.SetMainRebarCurve(startLocations[i], startWidths[i], startHeights[i], spacings[i], nums[i],
                                                          lengthLists[i], angleHLists[i], anglePlaneLists[i]);
                MainRebar.Add(deckRebar);
                MainRebarCurves.AddRange(deckRebar.RebarCurves);
            }
        }


        // 기능
        // 선형
        public Point PointAtParameter(double param)
        {
            if(param == 0)
            {
                return StartSectionPlane.Origin;
            }
            else if(param == 1)
            {
                return EndSectionPlane.Origin;
            }
            else
            {
                return DeckAlignment.PointAtParameter(param);
            }

        }
        public Vector TangentAtParameter(double param)
        {
            if (param == 0)
            {
                return StartSectionPlane.Normal;
            }
            else if (param == 1)
            {
                return EndSectionPlane.Normal;
            }
            else
            {
                return DeckAlignment.TangentAtParameter(param);
            }
        }
        public Plane PlaneAtParameter(double param)
        {
            Point origin = PointAtParameter(param);
            Vector tangent = TangentAtParameter(param);
            Vector xAxis = Vector.ZAxis().Cross(tangent);
            Vector yAxis = tangent.Cross(xAxis);
            return Plane.ByOriginXAxisYAxis(origin, xAxis, yAxis);
        }

        public Plane HorizonPlaneAtParameter(double param, double u, double v, double w)
        {
            Plane alPlane = PlaneAtParameter(param);
            Point origin = Utility.GeometryFunctions.GetPointAt(alPlane, u, v, w);
            return Plane.ByOriginXAxisYAxis(origin, alPlane.XAxis, alPlane.Normal);
        }

        public DeckSection GetSectionAtParameter(double param)
        {
            Plane sectionPlane = PlaneAtParameter(param);

            double deckWidth = StartDeckSection.DeckWidth + (StartDeckSection.DeckWidth - EndDeckSection.DeckWidth) * param;
            double deckDepth = StartDeckSection.DeckDepth + (StartDeckSection.DeckDepth - EndDeckSection.DeckDepth) * param;
            double startCantWidth = StartDeckSection.StartCantWidth + (StartDeckSection.StartCantWidth - EndDeckSection.StartCantWidth) * param;
            double endCantWidth = StartDeckSection.EndCantWidth + (StartDeckSection.EndCantWidth - EndDeckSection.EndCantWidth) * param;
            double elevationRatio = StartDeckSection.ElevationRatio + (StartDeckSection.ElevationRatio - EndDeckSection.ElevationRatio) * param;
            double haunchRatio = StartDeckSection.HaunchRatio + (StartDeckSection.HaunchRatio - EndDeckSection.HaunchRatio) * param;

            DeckSection ds = new DeckSection(
                sectionPlane, deckWidth, deckDepth, startCantWidth, endCantWidth,
                elevationRatio, haunchRatio, StartDeckSection.Side);
            double startWallWidth = StartDeckSection.StartWallWidth + (StartDeckSection.StartWallWidth - EndDeckSection.StartWallWidth) * param;
            double endWallWidth = StartDeckSection.EndWallWidth + (StartDeckSection.EndWallWidth - EndDeckSection.EndWallWidth) * param;
            double girderWidth = StartDeckSection.GirderWidth + (StartDeckSection.GirderWidth - EndDeckSection.GirderWidth) * param;
            int girderNumber = StartDeckSection.GirderNumber;
            bool isFull = StartDeckSection.IsFull;
            ds.SetSection(startWallWidth, endWallWidth, girderWidth, girderNumber, isFull);
            return ds;
        }
    }
}
