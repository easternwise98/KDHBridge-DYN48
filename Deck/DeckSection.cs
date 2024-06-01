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
    public class DeckSection
    {
        public Plane SectionPlane { get; set; }

        // 입력값
        public double DeckWidth { get; set; }
        public double DeckDepth { get; set; }
        public double StartCantWidth { get; set; }
        public double EndCantWidth { get; set; }
        public double HaunchRatio { get; set; }
        public double ElevationRatio { get; set; }
        internal double StartWallWidth { get; set; }
        internal double EndWallWidth { get; set; }

        internal double GirderWidth { get; set; }
        internal int GirderNumber { get; set; }

        internal bool Side { get; set; }
        internal bool IsFull { get; set; }

        // 산정값
        // 상부
        internal double StartWallLength { get; set; }
        internal double EndWallLength { get; set; }
        internal double UpperLength { get; set; }
        // 하부
        internal double StartCantLength { get; set; }
        internal double EndCantLength { get; set; }
        internal double GLength { get; set; }
        internal double HaunchLength { get; set; }
        internal double GInterLength { get; set; }
        internal Vector ERVector { get; set; }
        internal Vector HRVector { get; set; }
        internal Vector XVector { get; set; }
        internal Vector YVector { get; set; }


        // 결과값
        public Point OriginPoint { get; set; }
        public List<Point> SectionPoints { get; set; }
        public Curve SectionCurve { get; set; }
        public double MaxDepth { get; set; }

        public DeckSection(Plane sectionPlane, double deckWidth, double deckDepth,
            double startCantWidth, double endCantWdith,
            double elevationRatio, double haunchRatio, bool side = true)
        {
            SectionPlane = sectionPlane;
            DeckWidth = deckWidth;
            DeckDepth = deckDepth;
            StartCantWidth = startCantWidth;
            EndCantWidth = endCantWdith;
            ElevationRatio = elevationRatio;
            HaunchRatio = haunchRatio;
            Side = side;
            MaxDepth = deckDepth;


            OriginPoint = sectionPlane.Origin;
            if (side)
            {
                XVector = sectionPlane.XAxis;
                YVector = sectionPlane.YAxis;
                ERVector = Vector.ByTwoPoints(SectionPlane.Origin, Utility.GeometryFunctions.GetPointAt(SectionPlane, 1, -ElevationRatio, 0)).Normalized();
                HRVector = Vector.ByTwoPoints(SectionPlane.Origin, Utility.GeometryFunctions.GetPointAt(SectionPlane, 1, -HaunchRatio, 0)).Normalized();;
            }
            else
            {
                XVector = sectionPlane.XAxis.Reverse();
                YVector = sectionPlane.YAxis;
                ERVector = Vector.ByTwoPoints(SectionPlane.Origin, Utility.GeometryFunctions.GetPointAt(SectionPlane, -1, -ElevationRatio, 0)).Normalized();
                HRVector = Vector.ByTwoPoints(SectionPlane.Origin, Utility.GeometryFunctions.GetPointAt(SectionPlane, -1, -HaunchRatio, 0)).Normalized();
            }
        }

        public void SetSection(double startWallWidth, double endWallWidth, double gWidth, int gNumber, bool isFull = false)
        {
            StartWallWidth = startWallWidth;
            EndWallWidth = endWallWidth;
            GirderWidth = gWidth;
            GirderNumber = gNumber;
            IsFull = isFull;



            // 상부
            double upperX = DeckWidth - (StartWallWidth + EndWallWidth);
            double upperY = upperX * ElevationRatio;
            UpperLength = Math.Sqrt(upperX * upperX + upperY * upperY);

            // 헌치부
            double haunchX = (GirderWidth * ElevationRatio) / (HaunchRatio - ElevationRatio);
            double haunchY = haunchX * HaunchRatio;
            HaunchLength = Math.Sqrt(haunchX * haunchX + haunchY * haunchY);

            // 거더부
            if (IsFull)
            {
                double gInterX = (DeckWidth - (StartCantWidth + EndCantWidth)
                    - GirderWidth * GirderNumber
                    - haunchX * GirderNumber) / (GirderNumber - 1);
                double gInterY = gInterX * ElevationRatio;
                GInterLength = Math.Sqrt(gInterX * gInterX + gInterY * gInterY);
            }
            else
            {
                double gInterX = (DeckWidth - (StartCantWidth + EndCantWidth)
                    - GirderWidth * (GirderNumber - 1)
                    - haunchX * (GirderNumber - 1)) / (GirderNumber - 1);
                double gInterY = gInterX * ElevationRatio;
                GInterLength = Math.Sqrt(gInterX * gInterX + gInterY * gInterY);
            }

            // 하부
            double startCantX = StartCantWidth - StartWallWidth;
            double startCantY = startCantX * ElevationRatio;
            StartCantLength = Math.Sqrt(startCantX * startCantX + startCantY * startCantY);

            double endCantX = EndCantWidth - EndWallWidth;
            double endCantY = endCantX * ElevationRatio;
            EndCantLength = Math.Sqrt(endCantX * endCantX + endCantY * endCantY);

            StartWallLength = StartWallWidth;
            EndWallLength = EndWallWidth;

            // 단면생성
            SetSectionPoints();

        }

        internal void SetSectionPoints()
        {

            // 상부
            Point pu1 = OriginPoint;
            Point pu2 = (Point)pu1.Translate(XVector, StartWallLength);
            Point pu3 = (Point)pu2.Translate(ERVector, UpperLength);
            Point pu4 = (Point)pu3.Translate(XVector, EndWallLength);
            List<Point> upperList = new List<Point> { pu1, pu2, pu3, pu4 };

            // 하부
            Point pl1 = (Point)OriginPoint.Translate(YVector, -1 * DeckDepth);
            Point pl2 = (Point)pl1.Translate(XVector, StartWallLength);
            Point pl3 = (Point)pl2.Translate(ERVector, StartCantLength);

            List<Point> gList = GetGirderPoints(pl3);
            Point pl4 = (Point)gList.Last().Translate(ERVector, EndCantLength);
            Point pl5 = (Point)pl4.Translate(XVector, EndWallLength);
            List<Point> lowerList = new List<Point> { pl1, pl2, pl3 };
            lowerList.AddRange(gList);
            lowerList.AddRange(new List<Point> { pl4, pl5 });
            lowerList.Reverse();

            List<Point> sectionPoints = new List<Point>();
            sectionPoints.AddRange(upperList);
            sectionPoints.AddRange(lowerList);

            // 최대 두께 산정
            MaxDepth = Math.Max(MaxDepth, upperList.Last().DistanceTo(lowerList.First()));

            SectionPoints = sectionPoints;
            // 중복제거
            SectionPoints = SectionPoints.Distinct().ToList();

            
            SectionCurve = PolyCurve.ByPoints(SectionPoints, true);
        }

        internal List<Point> GetGirderPoints(Point startPoint)
        {
            List<Point> girderPoints = new List<Point>() { startPoint };
            if (IsFull)
            {
                for (int i = 0; i < GirderNumber - 1; i++)
                {
                    Point p1 = (Point)girderPoints.Last().Translate(HRVector,HaunchLength);
                    Point p2 = (Point)p1.Translate().Translate(XVector, GirderWidth);
                    Point p3 = (Point)p2.Translate(ERVector, GInterLength);
                    girderPoints.AddRange(new List<Point> { p1, p2, p3 });
                }
                Point pp1 = (Point)girderPoints.Last().Translate(HRVector, HaunchLength);
                Point pp2 = (Point)pp1.Translate(XVector, GirderWidth);
                girderPoints.AddRange(new List<Point> { pp1, pp2 });
            }
            else
            {
                Point pp1 = (Point)girderPoints.Last().Translate(ERVector, GInterLength);
                girderPoints.Add(pp1);
                for (int i = 0; i < GirderNumber - 2; i++)
                {
                    Point p1 = (Point)girderPoints.Last().Translate(HRVector, HaunchLength);
                    Point p2 = (Point)p1.Translate(XVector, GirderWidth);
                    Point p3 = (Point)p2.Translate(ERVector, GInterLength);
                    girderPoints.AddRange(new List<Point> { p1, p2, p3 });
                }
                Point pp2 = (Point)girderPoints.Last().Translate(HRVector, HaunchLength);
                Point pp3 = (Point)pp2.Translate(XVector, GirderWidth);
                girderPoints.AddRange(new List<Point> { pp2, pp3 });
            }
            return girderPoints;
        }

        // 단면 위치에서 점 찾기
        public Point GetSectionPointAtWidth(double u, double v)
        {
            Point point = OriginPoint;
            if (u > 0 && u <= StartWallWidth)
            {
                point = (Point)OriginPoint.Translate(XVector, StartWallLength * (u / StartWallWidth));
            }
            else if (StartWallWidth < u && u <= DeckWidth - EndWallWidth)
            {
                point = (Point)OriginPoint.Translate(XVector, StartWallLength).Translate(
                    ERVector, UpperLength * ((u - StartWallWidth) / (DeckWidth - StartWallWidth - EndWallWidth)));
            }
            else if (DeckWidth - EndWallWidth < u && u <= DeckWidth)
            {
                point = (Point)OriginPoint.Translate(XVector, (StartWallLength)).Translate(
                    ERVector, UpperLength).Translate(
                    XVector, u - (DeckWidth - EndWallWidth));
            }
            else if (u < 0)
            {
                point = (Point)OriginPoint.Translate(XVector, u);
            }
            else if (u > DeckWidth)
            {
                 point = (Point)OriginPoint.Translate(XVector, (StartWallLength)).Translate(
                    ERVector, UpperLength).Translate(
                    XVector, EndWallWidth).Translate(
                        u - DeckWidth);
            }
            return (Point)point.Translate(YVector, -1 * v);
        }

        public DeckSection GetSectionAtPlane(Plane plane)
        {
            DeckSection section = new DeckSection(plane, DeckWidth, DeckDepth, StartCantWidth, EndCantWidth, ElevationRatio, HaunchRatio, Side);
            section.SetSection(StartWallWidth, EndWallWidth, GirderWidth, GirderNumber, IsFull);
            return section;
        }
    }
}
