using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Girder
{
    [IsVisibleInDynamoLibrary(false)]
    public class GHRebar
    {
        [IsVisibleInDynamoLibrary(false)]
        public Girder HostGirder { get; set; }
        public string Name { get; set; }
        public double Diameter { get; set; }
        public double StartOffset { get; set; }
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double Spacing { get; set; }
        public double Number { get; set; }
        public List<double> Lengths { get; set; }
        public List<double> AnglesH { get; set; }
        public List<double> AnglesP { get; set; }

        // Output
        public Curve RebarCurve { get; set; }
        public List<Curve> RebarCurves { get; set; }
        public double RebarLength { get; set; }
        public double AllRebarLength { get; set; }
        public List<Solid> RebarSolids { get; set; }

        public GHRebar(Girder girder, string name, double diameter)
        {
            HostGirder = girder;
            Name = name;
            Diameter = diameter;
        }

        public void SetHRebarCurve(double startLocation, double startWidth, double startHeight, double spacing, double num,
            List<double> lengths, List<double> angleH, List<double> anglePlane)
        {
            // 선형으로부터 원점 잡기
            double girderLength = HostGirder.GLength;

            // 시작점
            List<Curve> rebarCurves = new List<Curve>();
            for (int i = 0; i < num; i++)
            {
                double param = (startLocation + spacing * i) / girderLength;
                Plane startPlane = HostGirder.Alignment.PlaneAtParameter(param);

                Point originPoint = Utility.GeometryFunctions.GetPointAt(startPlane, startWidth, -startHeight, 0);
                Plane originPlane = Plane.ByOriginXAxisYAxis(originPoint, startPlane.XAxis, startPlane.YAxis);

                List<Point> drawPoints = new List<Point>() { originPoint };
                List<Plane> drawPlanes = new List<Plane>() { originPlane };
                Plane angleRefPlane = originPlane;
                // 철근 그리기
                for (int j = 0; j < lengths.Count; j++)
                {
                    angleRefPlane = (Plane)angleRefPlane.Rotate(angleRefPlane.Origin, angleRefPlane.YAxis, - 1 * anglePlane[j] * (Math.PI / 180.0));
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

        public void SetHRebarSolid()
        {
            List<Solid> solids = new List<Solid>();
            foreach (Curve curve in RebarCurves)
            {
                Circle circle = Circle.ByPlaneRadius(curve.PlaneAtParameter(0), Diameter / 2);
                Solid solid = Solid.BySweep(circle, curve, false);
                solids.Add(solid);
            }
            RebarSolids = solids;
        }
    }
}
