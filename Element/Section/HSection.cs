using Autodesk.DesignScript.Geometry;
using KDHBridge_DYN48.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Element.Section
{
    internal class HSection : IElementSection
    {
        // Input Paramters
        internal double Height { get; set; }
        internal double TFW { get; set; }
        internal double TFH { get; set; }
        internal double TFIH { get; set; }
        internal double TFHH { get; set; }
        internal double WW { get; set; }
        internal double BFW { get; set; }
        internal double BFH { get; set; }
        internal double BFIH { get; set; }
        internal double BFHH { get; set; }

        public Dictionary<string, double> SectionDimensions { get; set; }

        // Properties
        public double Area { get; set; }
        public double InertiaX { get; set; }
        public double InertiaY { get; set; }
        public double MomentOfInertiaX { get; set; }
        public double MomentOfInertiaY { get; set; }
        public double CentroidX { get; set; }
        public double CentroidY { get; set; }
        public Dictionary<string, double> SectionProperties { get; set; }

        public Plane SectionPlane { get; set; }
        public List<Point> Vertices { get; set; }
        public Point Centroid { get; set; }
        public Curve SectionCurve { get; set; }

        public HSection(Plane sectionPlane, double height, double tFW, double tFH, double tFIH, double tFHH,
            double wW, double bFW, double bFH, double bFIH, double bFHH)
        {
            Height = height;
            TFW = tFW;
            TFH = tFH;
            TFIH = tFIH;
            TFHH = tFHH;
            WW = wW;
            BFW = bFW;
            BFH = bFH;
            BFIH = bFIH;
            BFHH = bFHH;
            SectionDimensions = new Dictionary<string, double>
            {
                { "Height", Height },
                { "TFW", TFW },
                { "TFH", TFH },
                { "TFIH", TFIH },
                { "TFHH", TFHH },
                { "WW", WW },
                { "BFW", BFW },
                { "BFH", BFH },
                { "BFIH", BFIH },
                { "BFHH", BFHH }
            };  
            //
            Area = GetArea();
            MomentOfInertiaX = GetMomentOfInertiaXAt(0);
            MomentOfInertiaY = GetMomentOfInertiaYAt(0);
            CentroidX = GetCentroidXAt(0);
            CentroidY = GetCentroidYAt(0);
            InertiaX = GetInertiaXAt(0);
            InertiaY = GetInertiaYAt(0);

            SectionProperties = new Dictionary<string, double>
            {
                { "Area", Area },
                { "MomentOfInertiaX", MomentOfInertiaX },
                { "MomentOfInertiaY", MomentOfInertiaY },
                { "CentroidX", CentroidX },
                { "CentroidY", CentroidY },
                { "InertiaX", InertiaX },
                { "InertiaY", InertiaY },
            };


            SectionPlane = sectionPlane;
            Vertices = new List<Point>()
            {
                // Top
                GeometryFunctions.GetPointAt(sectionPlane, TFW / 2, 0, 0),
                GeometryFunctions.GetPointAt(sectionPlane, TFW / 2, -TFH, 0),
                GeometryFunctions.GetPointAt(sectionPlane, WW / 2 + TFHH, -TFH - TFIH, 0),
                GeometryFunctions.GetPointAt(sectionPlane, WW / 2, -TFH - TFIH - TFHH, 0),
                // Bottom 
                GeometryFunctions.GetPointAt(sectionPlane, WW / 2, -Height + BFH + BFIH + BFHH, 0),
                GeometryFunctions.GetPointAt(sectionPlane, WW / 2 + BFHH, -Height + BFH + BFIH, 0),
                GeometryFunctions.GetPointAt(sectionPlane, BFW / 2, -Height + BFH, 0),
                GeometryFunctions.GetPointAt(sectionPlane, BFW / 2, -Height, 0),

                GeometryFunctions.GetPointAt(sectionPlane, -BFW / 2, -Height, 0),
                GeometryFunctions.GetPointAt(sectionPlane, -BFW / 2, -Height + BFH, 0),
                GeometryFunctions.GetPointAt(sectionPlane, -WW / 2 - BFHH, -Height + BFH + BFIH, 0),
                GeometryFunctions.GetPointAt(sectionPlane, -WW / 2, -Height + BFH + BFIH + BFHH, 0),

                // Top
                GeometryFunctions.GetPointAt(sectionPlane, -WW / 2, -TFH - TFIH - TFHH, 0),
                GeometryFunctions.GetPointAt(sectionPlane, -WW / 2 - TFHH, -TFH - TFIH, 0),
                GeometryFunctions.GetPointAt(sectionPlane, -TFW / 2, -TFH, 0),
                GeometryFunctions.GetPointAt(sectionPlane, -TFW / 2, 0, 0)
            };

            // TFW 부분 수정해야함.
            Centroid = GeometryFunctions.GetPointAt(sectionPlane, CentroidX, -1 * (Height - CentroidY), 0);
            // Vertices에서 중복 제거 후 Curve 생성
            List<Point> curvePoints = Vertices.Distinct().ToList();
            SectionCurve = PolyCurve.ByPoints(curvePoints, true);

        }

        public double GetArea()
        {
            double a1 = MathFunctions.GetRecArea(TFW, TFH);
            double a2 = MathFunctions.GetTriArea((TFW - WW - 2 * TFHH) / 2, TFIH);
            double a3 = MathFunctions.GetRecArea(WW + 2 * TFHH, TFIH);
            double a4 = MathFunctions.GetTriArea(TFHH, TFHH);

            double a5 = MathFunctions.GetRecArea(WW, Height - TFH - TFIH - BFH - BFIH);

            double a6 = MathFunctions.GetTriArea(BFHH, BFHH);
            double a7 = MathFunctions.GetRecArea(WW + 2 * BFHH, BFIH);
            double a8 = MathFunctions.GetTriArea((BFW - WW - 2 * BFHH) / 2, BFIH);
            double a9 = MathFunctions.GetRecArea(BFW, BFH);
            return a1 + 2 * a2 + a3 + 2 * a4 + a5 + a6 + a7 + 2 * a8 + a9;
        }


        // 작업 해야함.
        public double GetMomentOfInertiaXAt(double u)
        {
            double a1 = MathFunctions.GetRecArea(TFW, TFH);
            double a2 = MathFunctions.GetTriArea((TFW - WW - 2 * TFHH) / 2, TFIH);
            double a3 = MathFunctions.GetRecArea(WW + 2 * TFHH, TFIH);
            double a4 = MathFunctions.GetTriArea(TFHH, TFHH);

            double a5 = MathFunctions.GetRecArea(WW, Height - TFH - TFIH - BFH - BFIH);

            double a6 = MathFunctions.GetTriArea(BFHH, BFHH);
            double a7 = MathFunctions.GetRecArea(WW + 2 * BFHH, BFIH);
            double a8 = MathFunctions.GetTriArea((BFW - WW - 2 * BFHH) / 2, BFIH);
            double a9 = MathFunctions.GetRecArea(BFW, BFH);

            double y1 = -1 * u;
            double y2l = -1 * ((TFW - WW - 2 * TFHH) / (2*3) + WW / 2 + TFHH + u);
            double y2r = ((TFW - WW - 2 * TFHH) / (2*3) + WW / 2 + TFHH - u);
            double y3 = -1 * u;
            double y4l = -1 * (TFHH / 3 + WW / 2 + u);
            double y4r = (TFHH / 3 + WW / 2 - u);

            double y5 = -1 * u;
            
            double y6l = -1 * (BFHH / 3 + WW / 2 + u);
            double y6r = (BFHH / 3 + WW / 2 - u);
            double y7 = -1 * u;
            double y8l = -1 * ((BFW - WW - 2 * BFHH) / (2 * 3) + WW / 2 + BFHH + u);
            double y8r = ((BFW - WW - 2 * BFHH) / (2 * 3) + WW / 2 + BFHH - u);
            double y9 = -1 * u;
            return a1 * y1 + 2 * a2 * (y2l + y2r) + a3 * y3 + 2 * a4 * (y4l + y4r) + a5 * y5 + a6 * (y6l + y6r) + a7 * y7 + 2 * a8 * (y8l + y8r) + a9 * y9;
        }
        public double GetMomentOfInertiaYAt(double v)
        {
            double a1 = MathFunctions.GetRecArea(TFW, TFH);
            double a2 = MathFunctions.GetTriArea((TFW - WW - 2 * TFHH) / 2, TFIH);
            double a3 = MathFunctions.GetRecArea(WW + 2 * TFHH, TFIH);
            double a4 = MathFunctions.GetTriArea(TFHH, TFHH);

            double a5 = MathFunctions.GetRecArea(WW, Height - TFH - TFIH - BFH - BFIH);

            double a6 = MathFunctions.GetTriArea(BFHH, BFHH);
            double a7 = MathFunctions.GetRecArea(WW + 2 * BFHH, BFIH);
            double a8 = MathFunctions.GetTriArea((BFW - WW - 2 * BFHH) / 2, BFIH);
            double a9 = MathFunctions.GetRecArea(BFW, BFH);

            double y1 = TFH / 2 - v;
            double y2 = TFH + TFIH / 3 - v;
            double y3 = TFH + TFIH / 2 - v;
            double y4 = TFH + TFIH + TFHH / 3 - v;

            double y5 = TFH + TFIH + (Height - TFH - TFIH - BFH - BFIH) / 2 - v;

            double y6 = Height - (BFH + BFIH + BFHH / 3) - v;
            double y7 = Height - (BFH + BFIH / 2) - v;
            double y8 = Height - (BFH + BFIH / 3) - v;
            double y9 = Height - (BFH / 2) - v;

            return a1 * y1 + 2 * a2 * y2 + a3 * y3 + 2 * a4 * y4 + a5 * y5 + a6 * y6 + a7 * y7 + 2 * a8 * y8 + a9 * y9;
        }

        // X축 : 아래 Y축 : 중앙
        public double GetCentroidXAt(double u) { return GetMomentOfInertiaXAt(0) / Area - u; }
        public double GetCentroidYAt(double v) { return GetMomentOfInertiaYAt(0) / Area - v; }

        public double GetInertiaXAt(double u)
        {
            double i1 = MathFunctions.GetRecInertiaX(TFW, TFH);
            double i2 = MathFunctions.GetTriInertiaX((TFW - WW - 2 * TFHH) / 2, TFIH);
            double i3 = MathFunctions.GetRecInertiaX(WW + 2 * TFHH, TFIH);
            double i4 = MathFunctions.GetTriInertiaX(TFHH, TFHH);

            double i5 = MathFunctions.GetRecInertiaX(WW, Height - TFH - TFIH - BFH - BFIH);

            double i6 = MathFunctions.GetTriInertiaX(BFHH, BFHH);
            double i7 = MathFunctions.GetRecInertiaX(WW + 2 * BFHH, BFIH);
            double i8 = MathFunctions.GetTriInertiaX((BFW - WW - 2 * BFHH) / 2, BFIH);
            double i9 = MathFunctions.GetRecInertiaX(BFW, BFH);
            double inertia0 = i1 + 2 * i2 + i3 + 2 * i4 + i5 + i6 + i7 + 2 * i8 + i9;

            List<double> areas = new List<double>
            {
                MathFunctions.GetRecArea(TFW, TFH),
                MathFunctions.GetTriArea((TFW - WW - 2 * TFHH) / 2, TFIH),
                MathFunctions.GetRecArea(WW + 2 * TFHH, TFIH),
                MathFunctions.GetTriArea(TFHH, TFHH),
                MathFunctions.GetRecArea(WW, Height - TFH - TFIH - BFH - BFIH),
                MathFunctions.GetTriArea(BFHH, BFHH),
                MathFunctions.GetRecArea(WW + 2 * BFHH, BFIH),
                MathFunctions.GetTriArea((BFW - WW - 2 * BFHH) / 2, BFIH),
                MathFunctions.GetRecArea(BFW, BFH)
            };


            List<double> xValues = new List<double>
            {
                -1 * u,
                -1 * ((TFW - WW - 2 * TFHH) / (2*3) + WW / 2 + TFHH + u),
                ((TFW - WW - 2 * TFHH) / (2*3) + WW / 2 + TFHH - u),
                -1 * u,
                -1 * (TFHH / 3 + WW / 2 + u),
                (TFHH / 3 + WW / 2 - u),
                -1 * u,
                -1 * (BFHH / 3 + WW / 2 + u),
                (BFHH / 3 + WW / 2 - u),
                -1 * u,
                -1 * ((BFW - WW - 2 * BFHH) / (2 * 3) + WW / 2 + BFHH + u),
                ((BFW - WW - 2 * BFHH) / (2 * 3) + WW / 2 + BFHH - u),
                -1 * u
            };
            List<double> aXSquare = areas.Zip(xValues, (area, x) => area * Math.Pow(x - CentroidX, 2)).ToList();
            

            return inertia0 + aXSquare.Sum();
        }

        public double GetInertiaYAt(double v)
        {
            double i1 = MathFunctions.GetRecInertiaY(TFW, TFH);
            double i2 = MathFunctions.GetTriInertiaY((TFW - WW - 2 * TFHH) / 2, TFIH);
            double i3 = MathFunctions.GetRecInertiaY(WW + 2 * TFHH, TFIH);
            double i4 = MathFunctions.GetTriInertiaY(TFHH, TFHH);

            double i5 = MathFunctions.GetRecInertiaY(WW, Height - TFH - TFIH - BFH - BFIH);

            double i6 = MathFunctions.GetTriInertiaY(BFHH, BFHH);
            double i7 = MathFunctions.GetRecInertiaY(WW + 2 * BFHH, BFIH);
            double i8 = MathFunctions.GetTriInertiaY((BFW - WW - 2 * BFHH) / 2, BFIH);
            double i9 = MathFunctions.GetRecInertiaY(BFW, BFH);
            double inertia0 = i1 + 2 * i2 + i3 + 2 * i4 + i5 + i6 + i7 + 2 * i8 + i9;

            List<double> areas = new List<double>
            {
                MathFunctions.GetRecArea(TFW, TFH),
                MathFunctions.GetTriArea((TFW - WW - 2 * TFHH) / 2, TFIH),
                MathFunctions.GetRecArea(WW + 2 * TFHH, TFIH),
                MathFunctions.GetTriArea(TFHH, TFHH),
                MathFunctions.GetRecArea(WW, Height - TFH - TFIH - BFH - BFIH),
                MathFunctions.GetTriArea(BFHH, BFHH),
                MathFunctions.GetRecArea(WW + 2 * BFHH, BFIH),
                MathFunctions.GetTriArea((BFW - WW - 2 * BFHH) / 2, BFIH),
                MathFunctions.GetRecArea(BFW, BFH)
            };

            List<double> yValues = new List<double>
            {
                TFH / 2 - v,
                TFH + TFIH / 3 - v,
                TFH + TFIH / 2 - v,
                TFH + TFIH + TFHH / 3 - v,
                TFH + TFIH + (Height - TFH - TFIH - BFH - BFIH) / 2 - v,
                Height - (BFH + BFIH + BFHH / 3) - v,
                Height - (BFH + BFIH / 2) - v,
                Height - (BFH + BFIH / 3) - v,
                Height - (BFH / 2) - v
            };
            List<double> aYSquare = areas.Zip(yValues, (area, y) => area * Math.Pow(CentroidY - y, 2)).ToList();

            return inertia0 + aYSquare.Sum();
        }

        public IElementSection GetSectionAt(Plane sectionPlane)
        {
            return new HSection(sectionPlane, Height, TFW, TFH, TFIH, TFHH, WW, BFW, BFH, BFIH, BFHH);
        }

        


    }
}

