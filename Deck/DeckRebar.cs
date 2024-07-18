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
    public class DeckRebar
    {
        public Deck HostDeck { get; set; }
        public string Name { get; set; }
        public double Diameter { get; set; }
        public double StartOffset { get; set; }
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double Spacing { get; set; }
        public int Number { get; set; }
        public List<double> Lengths { get; set; }
        public List<double> AnglesH { get; set; }
        public List<double> AnglesP { get; set; }

        // Output
        public Curve RebarCurve { get; set; }
        public List<Curve> RebarCurves { get; set; }
        public double RebarLength { get; set; }
        public double AllRebarLength { get; set; }
        public List<Solid> RebarSolids { get; set; }

        public DeckRebar(Deck hostDeck, string name, double diameter)
        {
            HostDeck = hostDeck;
            Name = name;
            Diameter = diameter;
        }

        public void SetShearRebarCurve(double startLocation, double startWidth, double startHeight, double spacing, double num,
            List<double> lengths, List<double> angleH, List<double> anglePlane)
        {
            // 선형으로부터 원점 잡기
            double deckLength = HostDeck.DeckLength;

            List<Curve> rebarCurves = new List<Curve>();
            for (int i = 0; i < num; i++)
            {
                double param = (startLocation + spacing * i) / deckLength;
                Plane startPlane = HostDeck.PlaneAtParameter(param);
                DeckSection deckSection = HostDeck.GetSectionAtParameter(param);

                Point originPoint = deckSection.GetSectionPointAtWidth(startWidth, startHeight);

                // 편경사만큼 돌리기
                Plane originPlane = Plane.ByOriginXAxisYAxis(originPoint, startPlane.XAxis, startPlane.YAxis);
                Plane superPlane = (Plane)originPlane.Rotate(originPlane.Origin, originPlane.Normal, -Math.Tanh(HostDeck.ElevationRatio) * 180 / Math.PI);

                List<Point> drawPoints = new List<Point>() { originPoint };
                List<Plane> drawPlanes = new List<Plane>() { superPlane };
                Plane angleRefPlane = superPlane;
                // 원점에서 철근 그리기
                for (int j = 0; j < lengths.Count; j++)
                {
                    angleRefPlane.Rotate(angleRefPlane.Origin, angleRefPlane.YAxis, - 1 * anglePlane[j] * (Math.PI / 180.0));
                    Point nextPoint = Utility.GeometryFunctions.GetPointAt(angleRefPlane,lengths[j] * Math.Cos(angleH[j] * (Math.PI / 180.0)),
                       lengths[j] * Math.Sin(angleH[j] * (Math.PI / 180.0)), 0);
                    angleRefPlane = (Plane)angleRefPlane.Translate(Vector.ByTwoPoints(angleRefPlane.Origin, nextPoint));

                    drawPoints.Add(nextPoint);
                    drawPlanes.Add(angleRefPlane);
                }


                Curve rebarCurve = PolyCurve.ByPoints(drawPoints.Distinct().ToList());
                rebarCurves.Add(rebarCurve);
            }

            RebarCurves = rebarCurves;
        }

        public void SetMainRebarCurve(double startLocation, double startWidth, double startHeight, double spacing, double num,
            List<double> lengths, List<double> angleH, List<double> anglePlane)
        {
            // 선형으로부터 원점 잡기
            double deckLength = HostDeck.DeckLength;

            List<Curve> rebarCurves = new List<Curve>();
            for (int i = 0; i < num; i++)
            {
                double param = (startLocation) / deckLength;
                Plane startPlane = HostDeck.PlaneAtParameter(param);
                DeckSection deckSection = HostDeck.GetSectionAtParameter(param);

                Point originPoint = deckSection.GetSectionPointAtWidth(startWidth + spacing * i, startHeight);

                Plane originPlane = Plane.ByOriginXAxisYAxis(originPoint, startPlane.Normal, startPlane.YAxis);

                List<Point> drawPoints = new List<Point>() { originPoint };
                List<Plane> drawPlanes = new List<Plane>() { originPlane };
                Plane angleRefPlane = originPlane;
                // 원점에서 철근 그리기
                for (int j = 0; j < lengths.Count; j++)
                {
                    angleRefPlane.Rotate(angleRefPlane.Origin, angleRefPlane.YAxis,- 1 * anglePlane[j] * (Math.PI / 180.0));
                    Point nextPoint = Utility.GeometryFunctions.GetPointAt(angleRefPlane, lengths[j] * Math.Cos(angleH[j] * (Math.PI / 180.0)),
                       lengths[j] * Math.Sin(angleH[j] * (Math.PI / 180.0)), 0);
                    angleRefPlane = (Plane)angleRefPlane.Translate(Vector.ByTwoPoints(angleRefPlane.Origin, nextPoint));

                    drawPoints.Add(nextPoint);
                    drawPlanes.Add(angleRefPlane);
                }

                Curve rebarCurve = PolyCurve.ByPoints(drawPoints);
                rebarCurves.Add(rebarCurve);
            }

            RebarCurves = rebarCurves;
        }

        public void SetRebarSolid()
        {

            List<Solid> rebarSolids = new List<Solid>();
            foreach (Curve curve in RebarCurves)
            {
                Circle rebarCircle = Circle.ByPlaneRadius(curve.PlaneAtParameter(0), Diameter / 2);
                Curve rebarCurve = rebarCircle.ToNurbsCurve();
                Solid rebarSolid = Solid.BySweep(rebarCircle, curve, false);
                rebarSolids.Add(rebarSolid);
            }
            RebarSolids = rebarSolids;
        }
    }
}
