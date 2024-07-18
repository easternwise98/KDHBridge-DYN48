using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Girder
{
    [IsVisibleInDynamoLibrary(false)]
    public class GMainRebar
    {
        public string Name { get; set; }
        public Girder HostGirder { get; set; }
        public double Diameter { get; set; }
        public List<double> AlignLoc { get; set; }
        public List<double> AlignWidth { get; set; }
        public List<double> AlignHeight { get; set; }

        public List<Point> ControlPoints { get; set; }
        public Curve RebarCurve { get; set; }
        public Solid RebarSolid { get; set; }

        public List<Curve> RebarCurves { get; set; }
        public List<Solid> RebarSolids { get; set; }

        public GMainRebar(Girder hostGirder, string name, double diameter)
        {
            Name = name;
            HostGirder = hostGirder;
            Diameter = diameter;
        }

        public void SetProfile(List<double> alignLoc, List<double> alignWidth, List<double> alignHeight, bool isBending)
        {
            AlignLoc = alignLoc;
            AlignWidth = alignWidth;
            AlignHeight = alignHeight;

            List<Point> controlPoints = new List<Point>();
            // 철근 그리기
            double girderLength = HostGirder.GLength;
            for (int i = 0; i < AlignLoc.Count; i++)
            {
                double param = AlignLoc[i] / girderLength;
                Plane drawPlane = HostGirder.Alignment.PlaneAtParameter(param);
                Point controlPoint = Utility.GeometryFunctions.GetPointAt(drawPlane, AlignWidth[i], -AlignHeight[i], 0);
                controlPoints.Add(controlPoint);
            }

            // Curve 생성
            if (HostGirder.Camber == 0 && HostGirder.Lateral == 0)
            {
                Curve curve = PolyCurve.ByPoints(controlPoints);
                RebarCurve = curve;
            }
            else if (isBending == true)
            {
                Curve curve = NurbsCurve.ByPoints(controlPoints);
                RebarCurve = curve;
            }
            else if (isBending == false)
            {
                Curve curve = PolyCurve.ByPoints(controlPoints);
                RebarCurve = curve;
            }
            else
            {
                Curve curve = NurbsCurve.ByPoints(controlPoints);
                RebarCurve = curve;
            }
            
        }

        public void SetSolid()
        {
            Circle circle = Circle.ByPlaneRadius(HostGirder.Alignment.PlaneAtParameter(0), Diameter / 2);
            Solid solid = Solid.BySweep(circle, RebarCurve, false);
            RebarSolid = solid;
        }


    }
}
