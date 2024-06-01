using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using KDHBridge_DYN48.Element.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Crossbeam
{
    [IsVisibleInDynamoLibrary(false)]
    public class HCrossbeam
    {
        public Girder.Girder StartGirder { get; set; }
        public Girder.Girder EndGirder { get; set; }
        public string Name { get; set; }
        public double StartLoc { get; set; }
        public double EndLoc { get; set; }
        public double StartWidth { get; set; }
        public double EndWidth { get; set; }
        public double StartHeight { get; set; }
        public double EndHeight { get; set; }

        //
        public HSection StartSection { get; set; }
        public HSection EndSection { get; set; }
        public Curve AlignmentCurve { get; set; }
        public Curve StartSectionCurve { get; set; }
        public Curve EndSectionCurve { get; set; }
        public Solid Solid { get; set; }

        public HCrossbeam(Girder.Girder startGirder, Girder.Girder endGirder, string name)
        {
            StartGirder = startGirder;
            EndGirder = endGirder;
            Name = name;
        }
        public void SetCrossbeamSection(HSection startSection, HSection endSection)
        {
            StartSection = startSection;
            EndSection = endSection;
        }
        public void SetCrossbeamCurve(double startLoc, double startWidth, double startHeight, double endLoc, double endWidth, double endHeight)
        {
            StartLoc = startLoc;
            EndLoc = endLoc;
            StartWidth = startWidth;
            EndWidth = endWidth;
            StartHeight = startHeight;
            EndHeight = endHeight;

            double startParam = StartLoc / StartGirder.GLength;
            double endParam = EndLoc / EndGirder.GLength;
            Plane startDrawPlane = StartGirder.Alignment.PlaneAtParameter(startParam);
            Plane endDrawPlane = EndGirder.Alignment.PlaneAtParameter(endParam);

            Point startPoint = Utility.GeometryFunctions.GetPointAt(startDrawPlane, startWidth, -startHeight, 0);
            Point endPoint = Utility.GeometryFunctions.GetPointAt(endDrawPlane, endWidth, -endHeight, 0);

            Plane startPlane = Plane.ByOriginXAxisYAxis(startPoint, startDrawPlane.Normal, startDrawPlane.YAxis);
            Plane endPlane = Plane.ByOriginXAxisYAxis(endPoint, endDrawPlane.Normal, endDrawPlane.YAxis);

            StartSection = (HSection)StartSection.GetSectionAt(startPlane);
            EndSection = (HSection)EndSection.GetSectionAt(endPlane);

            StartSectionCurve = StartSection.SectionCurve;
            EndSectionCurve = EndSection.SectionCurve;

            AlignmentCurve = Line.ByStartPointEndPoint(startPoint, endPoint);
        }

        public void SetSolid()
        {
            Solid = Solid.ByLoft(new List<Curve>() { StartSectionCurve, EndSectionCurve }, new List<Curve>() { AlignmentCurve });
        }

    }
}
