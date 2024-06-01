using KDHBridge_DYN48.Element.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using KDHBridge_DYN48.Utility;
using Autodesk.DesignScript.Runtime;
using KDHBridge_DYN48.Element.MathCurve;
using System.Security.Cryptography;

namespace KDHBridge_DYN48.Girder
{
    [IsVisibleInDynamoLibrary(false)]
    public class Girder
    {
        // Meta Data
        internal int Id { get; set; }
        internal string Name { get; set; }
        internal double GLength { get; set; }
        internal double GHeight { get; set; }
        internal double EConcrete { get; set; }
        internal Point RefPoint1 { get; set; }
        internal Point RefPoint2 { get; set; }
        
        // Alignment
        public GAlignment Alignment { get; set; }
        public double Camber { get; set; }
        public double Lateral { get; set; }

        // Section
        internal Dictionary<int, IElementSection> GSectionDict { get; set; }
        internal List<int> GSectionType { get; set; }
        internal List<double> GSectionLoc { get; set; }
        internal List<Point> GSectionControlPoints { get; set; }
        internal List<Plane> GSectionPlanes { get; set;}
        internal List<List<Point>> GSectionPoints { get; set; }
        internal List<Curve> GSectionCurves { get; set; }
        internal List<IElementSection> GSections { get; set; }
        
        // Solid
        public Solid Solid { get; set; }
        public List<Solid> SolidParts { get; set; }
        public List<List<Curve>> GuideCurves { get; set; }
        
        // Tendon
        public List<GTendon> Tendons { get; set; }

        // Rebar
        public List<GHRebar> HRebars { get; set; }
        public List<GMainRebar> MainRebars { get; set; }

        public List<Curve> HRebarCurves { get; set; }
        public List<Curve> MainRebarCurves { get; set; }

        // Analysis



        internal Girder(int id, string name, double gLength, double gHeight, double eConcrete)
        {
            Id = id;
            Name = name;
            GLength = gLength;
            GHeight = gHeight;
            EConcrete = eConcrete;
            GSectionDict = new Dictionary<int, IElementSection>();
            Tendons = new List<GTendon>();

            HRebars = new List<GHRebar>();
            MainRebars = new List<GMainRebar>();

            HRebarCurves = new List<Curve>();
            MainRebarCurves = new List<Curve>();

        }

        public void SetAlignment(double refX1, double refY1, double refZ1, double refX2, double refY2, double refZ2,
            double camber, double lateral, double inter1=0, double inter2=0)
        {
            Point refPoint1 = Point.ByCoordinates(refX1, refY1, refZ1);
            Point refPoint2 = Point.ByCoordinates(refX2, refY2, refZ2);
            Camber = camber;
            Lateral = lateral;
            // Set alignment
            Alignment = new GAlignment(this, refPoint1, refPoint2, camber, lateral, inter1, inter2);
        }

        public void SetSectionDict(int id, double gh, double tfw, double tfh, double tfih, double tfhh,
            double ww, double bfw, double bfh, double bfih, double bfhh)
        {
            // Set section
            HSection section = new HSection(Plane.XY(), gh, tfw, tfh, tfih, tfhh, ww, bfw, bfh, bfih, bfhh);
            // 이미 있으면 덮어쓰기
            if (GSectionDict.ContainsKey(id))
            {
                GSectionDict.Remove(id);
            }
            GSectionDict.Add(id, (IElementSection)section);
        }

        public void SetSection(List<int> sectionType, List<double> sectionLoc)
        {
            GSectionType = sectionType;
            GSectionLoc = sectionLoc;

            List<Plane> planes = new List<Plane>();
            List<Point> points = new List<Point>();
            List<List<Point>> sectionPoints = new List<List<Point>>();
            List<Curve> sectionCurves = new List<Curve>();
            List<IElementSection> sections = new List<IElementSection>();
            for (int i = 0; i < sectionType.Count; i++)
            {
                // Section 선택하기 - ControlPoint 선택 - 선형 평면 생성 - Section 재생성 - Point, Curve, Section 추가
                double param = sectionLoc[i] / GLength;
                IElementSection section = GSectionDict[sectionType[i]];

                Plane sectionPlane = Alignment.PlaneAtParameter(param);
                IElementSection newSection = section.GetSectionAt(sectionPlane);

                planes.Add(sectionPlane);
                points.Add(sectionPlane.Origin);
                sections.Add(newSection);
                sectionPoints.Add(newSection.Vertices);
                sectionCurves.Add(newSection.SectionCurve);
            }
            GSectionPlanes = planes;
            GSectionControlPoints = points;
            GSectionPoints = sectionPoints;
            GSectionCurves = sectionCurves;
            GSections = sections;


        }

        public void SetSolid()
        {
            // Set solid
            List<Solid> solidParts = new List<Solid>();
            List<List<Curve>> guideCurves = new List<List<Curve>>();
            for (int i = 0; i < GSections.Count - 1; i++)
            {
                // 시작단면 - 끝단면
                List<Point> sectionPoints1 = GSections[i].Vertices;
                Curve sectionCurve1 = GSections[i].SectionCurve;
                List<Point> sectionPoints2 = GSections[i + 1].Vertices;
                Curve sectionCurve2 = GSections[i + 1].SectionCurve;

                // 가이드 커브 + 해당 구간 선형
                List<Curve> gCurves = new List<Curve>();
                Curve al = Alignment.TopCurve.TrimByParameter(GSectionLoc[i]/GLength, GSectionLoc[i+1]/GLength);

                // 솔리드
                Solid solidPart;

                if (Camber == 0 & Lateral == 0)
                {
                    // 가이드 커브는 선형 + 단면 절점끼리 이은 직선
                    List<Curve> tempGCurves = sectionPoints1.Zip(sectionPoints2, (x, y) => (Curve)Line.ByStartPointEndPoint(x, y)).ToList();
                    tempGCurves.Add(Alignment.TopCurve);
                    gCurves= RemoveIntersectCurves(tempGCurves);

                    // 솔리드 생성
                    solidPart = Solid.ByLoft(new List<Curve>() { sectionCurve1, sectionCurve2 }, gCurves);
                }
                else
                {
                    // 변단면 구간에선 중앙 단면 생성, 그외는 선형으로 연결
                    if (GSectionType[i] != GSectionType[i + 1])
                    {
                        // 중앙 단면
                        List<Point> halfPoints = GetHalfSection(sectionPoints1, sectionPoints2);
                        Point halfControlPoint = Line.ByStartPointEndPoint(al.StartPoint, al.EndPoint)
                            .PointAtParameter(0.5);
                        Vector halfPointsDirection = Line.ByStartPointEndPoint(al.StartPoint, al.EndPoint).Direction;
                        Vector toAlignment = Vector.ByTwoPoints(halfControlPoint, al.PointAtParameter(0.5));
                        List<Point> movedHPs = halfPoints.Select(x => (Point)x.Translate(toAlignment)).ToList();
                        List<Point> rotHPs = movedHPs.Select(mx => GeometryFunctions.RotatePointByCurve(mx, halfPointsDirection, al, 0.5)).ToList();

                        // 가이드 커브 생성
                        for (int j = 0; j < rotHPs.Count - 1; j++)
                        {
                            gCurves.Add(Arc.ByThreePoints(sectionPoints1[j], rotHPs[j], sectionPoints2[j]));
                        }

                        // 만약 gCurves가 서로 intersect 하면 한개는 제외
                        gCurves.Add(Alignment.TopCurve);
                        gCurves = RemoveIntersectCurves(gCurves);

                        

                        // 솔리드 생성
                        solidPart = Solid.ByLoft(new List<Curve>() { sectionCurve1, sectionCurve2 }, gCurves);
                        //solidPart = null;
                    }
                    else
                    {
                        gCurves.Add(al);
                        solidPart = Solid.ByLoft(new List<Curve>() { sectionCurve1, sectionCurve2 }, new List<Curve> { Alignment.TopCurve });
                    }
                }
                guideCurves.Add(gCurves);
                solidParts.Add(solidPart);
            }

            Solid = Solid.ByUnion(solidParts);
            SolidParts = solidParts;
            GuideCurves = guideCurves;
        }
        
        public void SetTendonProfile(int id, MathCurve2D hProfile, MathCurve2D vProfile, double alignDistance, double alignHeight,  double diameter, double ductdiameter, 
            double strandDiameter, int strandNumber, double eTendon, double psStress)
        {
            // Plane 설정하기 
            Plane refPlane = Alignment.PlaneAtParameter(alignDistance / GLength);
            Point refPoint = GeometryFunctions.GetPointAt(refPlane, 0, -1 * alignHeight, 0);
            Plane tendonPlane = Plane.ByOriginXAxisYAxis(refPoint, refPlane.Normal, refPlane.XAxis);

            // Tendon 설정하기
            GTendon tendon = new GTendon(id: id, girder: this, diameter: diameter, ductDiameter: ductdiameter, 
                strandDiameter: strandDiameter, strandNumber: strandNumber, eTendon:eTendon, psStress:psStress);
            tendon.Set2DHProfile(hProfile);
            tendon.Set2DVProfile(vProfile);
            tendon.Set3DProfile(tendonPlane, alignDistance, alignHeight);
            
            Tendons.Add(tendon);
        }

        public List<Solid> GetTendonSolid()
        {
            List<Solid> tenSolids = new List<Solid>();
            for (int i = 0; i < Tendons.Count; i++)
            {
                Tendons[i].SetTendonSolid();
                tenSolids.Add(Tendons[i].TendonSolid);
            }
            return tenSolids;
        }


        public void SetHRebar(List<string> names, List<double> diameters, List<double> startLocations, List<double> startWidths, List<double> startHeights,
            List<double> spacings, List<double> nums, List<List<double>> lengthLists, List<List<double>> angleHLists, List<List<double>> anglePlaneLists)
        {
            // Set connector
            for (int i = 0; i < names.Count; i++)
            {
                GHRebar rebar = new GHRebar(this, names[i], diameters[i]);
                rebar.SetHRebarCurve(startLocations[i], startWidths[i], startHeights[i], spacings[i], nums[i], lengthLists[i], angleHLists[i], anglePlaneLists[i]);
                HRebars.Add(rebar);
                HRebarCurves.AddRange(rebar.RebarCurves);
            }
        }
        public void SetHRebarSolid()
        {
            List<Solid> solids = new List<Solid>();
            foreach (GHRebar rebar in HRebars)
            {
                rebar.SetHRebarSolid();
                solids.AddRange(rebar.RebarSolids);
            }
            SolidParts.AddRange(solids);
        }

        public void SetMainRebar(List<string> names, List<double> diameters, List<List<double>> alignLocs,
            List<List<double>> alignWidths, List<List<double>> alignHeights)
        {
            for (int i = 0; i < names.Count; i++)
            {
                GMainRebar rebar = new GMainRebar(this, names[i], diameters[i]);
                rebar.SetProfile(alignLocs[i], alignWidths[i], alignHeights[i]);
                MainRebars.Add(rebar);
                MainRebarCurves.Add(rebar.RebarCurve);
            }
        }

        public void SetMainRebarSolid()
        {
            List<Solid> solids = new List<Solid>();
            foreach (GMainRebar rebar in MainRebars)
            {
                rebar.SetSolid();
                solids.Add(rebar.RebarSolid);
            }
            SolidParts.AddRange(solids);
        }

        

        // 기능 
        // 1) 선형 지점 고르면 단면 선택
        // 2) 각 단면에 따른 중립축, 넓이, 계수 등 뽑기

        public Dictionary<string, object> GetNaiveSectionPropertyAt(double alignDistance, bool isTapered)
        {
            IElementSection elementSection = null;
            double area = 0;
            double inertiaX = 0;
            double inertiaY = 0;
            double momentInertiaX = 0;
            double momentInertiaY = 0;
            double centroidX = 0;
            double centroidY = 0;
            double param = alignDistance / GLength;
            for (int i = 0; i < GSectionLoc.Count-1; i++)
            {
                if (GSectionLoc[i] <= alignDistance & alignDistance < GSectionLoc[i + 1])
                {
                    // 다음 단면 타입과 같으면 그대로 사용
                    if (GSectionType[i] != GSectionType[i + 1])
                    {
                        double taperedParam;
                        if (isTapered)
                        {
                            taperedParam = (alignDistance - GSectionLoc[i]) / (GSectionLoc[i + 1] - GSectionLoc[i]);
                        }
                        else
                        {
                            taperedParam = 0.5;
                        }
                        Dictionary<string, double> section1Dimensions = GSections[i].SectionDimensions;
                        Dictionary<string, double> section2Dimensions = GSections[i + 1].SectionDimensions;
                        Dictionary<string, double> sectionDimensions = new Dictionary<string, double>();
                        foreach (KeyValuePair<string, double> kvp in section1Dimensions)
                        {
                            sectionDimensions.Add(kvp.Key, kvp.Value + (section2Dimensions[kvp.Key] - kvp.Value) * taperedParam);
                        }
                        elementSection = new HSection(Alignment.PlaneAtParameter(param), sectionDimensions["Height"], sectionDimensions["TFW"],
                            sectionDimensions["TFH"], sectionDimensions["TFIH"], sectionDimensions["TFHH"], sectionDimensions["WW"], sectionDimensions["BFW"],
                            sectionDimensions["BFH"], sectionDimensions["BFIH"], sectionDimensions["BFHH"]);
                        area = elementSection.Area;
                        inertiaX = elementSection.InertiaX;
                        inertiaY = elementSection.InertiaY;
                        momentInertiaX = elementSection.MomentOfInertiaX;
                        momentInertiaY = elementSection.MomentOfInertiaY;
                        centroidX = elementSection.CentroidX;
                        centroidY = elementSection.CentroidY;
                    }
                    else
                    {
                        Dictionary<string, double> sectionDimensions = GSections[i].SectionDimensions;
                        elementSection = new HSection(Alignment.PlaneAtParameter(param), sectionDimensions["Height"], sectionDimensions["TFW"],
                            sectionDimensions["TFH"], sectionDimensions["TFIH"], sectionDimensions["TFHH"], sectionDimensions["WW"], sectionDimensions["BFW"],
                            sectionDimensions["BFH"], sectionDimensions["BFIH"], sectionDimensions["BFHH"]);
                        area = GSections[i].Area;
                        inertiaX = GSections[i].InertiaX;
                        inertiaY = GSections[i].InertiaY;
                        momentInertiaX = GSections[i].MomentOfInertiaX;
                        momentInertiaY = GSections[i].MomentOfInertiaY;
                        centroidX = GSections[i].CentroidX;
                        centroidY = GSections[i].CentroidY;
                    }
                }
                else if(alignDistance == GSectionLoc[i+1])
                {
                    Dictionary<string, double> sectionDimensions = GSections[i + 1].SectionDimensions;
                    elementSection = new HSection(Alignment.PlaneAtParameter(alignDistance / GLength), sectionDimensions["Height"], sectionDimensions["TFW"],
                                               sectionDimensions["TFH"], sectionDimensions["TFIH"], sectionDimensions["TFHH"], sectionDimensions["WW"], sectionDimensions["BFW"],
                                                                      sectionDimensions["BFH"], sectionDimensions["BFIH"], sectionDimensions["BFHH"]);
                    area = GSections[i + 1].Area;
                    inertiaX = GSections[i + 1].InertiaX;
                    inertiaY = GSections[i + 1].InertiaY;
                    momentInertiaX = GSections[i + 1].MomentOfInertiaX;
                    momentInertiaY = GSections[i + 1].MomentOfInertiaY;
                    centroidX = GSections[i + 1].CentroidX;
                    centroidY = GSections[i + 1].CentroidY;
                }
                else
                {
                    continue;
                }
                
            }
            return new Dictionary<string, object>
            {
                { "ElementSection", elementSection },
                { "Area", area },
                { "InertiaX", inertiaX },
                { "InertiaY", inertiaY },
                { "MomentOfInertiaX", momentInertiaX },
                { "MomentOfInertiaY", momentInertiaY },
                { "CentroidX", centroidX },
                { "CentroidY", centroidY }
            };
        }

        public Dictionary<string, double> GetTendonYPropertyAt(double alignDistance, bool isTapered=true)
        {
            // 결과 값
            List<double> areas = new List<double>();
            List<double> yTops = new List<double>();
            List<double> aYTops = new List<double>();
            List<double> centroidYs = new List<double>();
            List<double> aCentroidY2s = new List<double>();

            Dictionary<string, object> sectionProperties = GetNaiveSectionPropertyAt(alignDistance, isTapered);

            Dictionary<string, List<double>> tendonXY = GetTendonXYAt(alignDistance);
            List<double> localX = tendonXY["LocalX"];
            List<double> localY = tendonXY["LocalY"];

            List<GTendon> tendons = new List<GTendon>();
            for (int i = 0; i < Tendons.Count; i++)
            {
                if (alignDistance >= Tendons[i].LocateDistance)
                {
                    // 텐던이 위치한 경우 추가하고 값을 구하기
                    tendons.Add(Tendons[i]);
                    double area = Tendons[i].StrandArea;
                    double yTop = localY[i];
                    double aYTop = area * yTop;
                    double centroidY = aYTop / area - (double)sectionProperties["CentroidY"];
                    double aCentroidY2 = area * Math.Pow(centroidY, 2);

                    areas.Add(area);
                    yTops.Add(yTop);
                    aYTops.Add(aYTop);
                    centroidYs.Add(centroidY);
                    aCentroidY2s.Add(aCentroidY2);
                }
                else
                {
                    continue;
                }
            }

            double totalArea = areas.Sum();
            double totalYTop = aYTops.Sum() / tendons.Count;
            double totalAYTop = aYTops.Sum();
            double totalCentroidY = totalAYTop / totalArea - (double)sectionProperties["CentroidY"];
            double totalACentroidY2 = aCentroidY2s.Sum();

            return new Dictionary<string, double>
            {
                { "TotalArea", totalArea },
                { "TotalYTop", totalYTop },
                { "TotalAYTop", totalAYTop },
                { "TotalCentroidY", totalCentroidY },
                { "TotalACentroidY2", totalACentroidY2 }
            };
            
        }

        public Dictionary<string, double> GetTendonXPropertyAt(double alignDistance, bool isTapered = true)
        {
            // 결과 값
            List<double> areas = new List<double>();
            List<double> xCenter = new List<double>();
            List<double> aXCenter = new List<double>();
            List<double> centroidXs = new List<double>();
            List<double> aCentroidX2s = new List<double>();

            Dictionary<string, object> sectionProperties = GetNaiveSectionPropertyAt(alignDistance, isTapered);

            Dictionary<string, List<double>> tendonXY = GetTendonXYAt(alignDistance);
            List<double> localX = tendonXY["LocalX"];
            List<double> localY = tendonXY["LocalY"];

            List<GTendon> tendons = new List<GTendon>();
            for (int i = 0; i < Tendons.Count; i++)
            {
                if (alignDistance >= Tendons[i].LocateDistance)
                {
                    // 텐던이 위치한 경우 추가하고 값을 구하기
                    tendons.Add(Tendons[i]);
                    double area = Tendons[i].StrandArea;
                    double xCenterValue = localX[i];
                    double aXCenterValue = area * xCenterValue;
                    double centroidX = aXCenterValue / area - (double)sectionProperties["CentroidX"];
                    double aCentroidX2 = area * Math.Pow(centroidX, 2);

                    areas.Add(area);
                    xCenter.Add(xCenterValue);
                    aXCenter.Add(aXCenterValue);
                    centroidXs.Add(centroidX);
                    aCentroidX2s.Add(aCentroidX2);
                }
                else
                {
                    continue;
                }
            }

            double totalArea = areas.Sum();
            double totalXCenter = aXCenter.Sum() / tendons.Count;
            double totalAXCenter = aXCenter.Sum();
            double totalCentroidX = totalAXCenter / totalArea - (double)sectionProperties["CentroidX"];
            double totalACentroidX2 = aCentroidX2s.Sum();

            return new Dictionary<string, double>
            {
                { "TotalArea", totalArea },
                { "TotalXCenter", totalXCenter },
                { "TotalAXCenter", totalAXCenter },
                { "TotalCentroidX", totalCentroidX },
                { "TotalACentroidX2", totalACentroidX2 }
            };
        }

        public Dictionary<string, object> GetTendonSectionPropertyAt(double alignDistance, bool isTapered = true)
        {
            Dictionary<string, object> sectionProperties = GetNaiveSectionPropertyAt(alignDistance, isTapered);
            Dictionary<string, double> tendonYProperties = GetTendonYPropertyAt(alignDistance, isTapered);
            Dictionary<string, double> tendonXProperties = GetTendonXPropertyAt(alignDistance, isTapered);

            List<Point> tendonPoints = GetTendonPointAt(alignDistance);
            Plane sectionPlane = Alignment.PlaneAtParameter(alignDistance / GLength);
            List<Circle> sections = tendonPoints.Select((x, i) => Circle.ByCenterPointRadiusNormal(x, Tendons[i].Diameter / 2, sectionPlane.Normal)).ToList();

            // 탄성계수비
            double n = Tendons.Select(x => x.ETendon).Average() / EConcrete;
            // PS환산 단면
            double area = (double)sectionProperties["Area"] + tendonYProperties["TotalArea"] * (n-1);
            double momentOfInertiaX = (double)sectionProperties["MomentOfInertiaX"] + tendonXProperties["TotalAXCenter"];
            double momentOfInertiaY = (double)sectionProperties["MomentOfInertiaY"] + tendonYProperties["TotalAYTop"];
            double centroidX = momentOfInertiaX / area;
            double centroidY = momentOfInertiaY / area;
            double InertiaX = (double)sectionProperties["InertiaX"] + tendonXProperties["TotalACentroidX2"];
            double InertiaY = (double)sectionProperties["InertiaY"] + tendonYProperties["TotalACentroidY2"];

            return new Dictionary<string, object>
            {
                { "ElementSection", sectionProperties["ElementSection"] },
                { "TendonSection", sections },
                { "TendonPoints", tendonPoints },
                { "Area", area },
                { "InertiaX", InertiaX },
                { "InertiaY", InertiaY },
                { "MomentOfInertiaX", momentOfInertiaX },
                { "MomentOfInertiaY", momentOfInertiaY },
                { "CentroidX", centroidX },
                { "CentroidY", centroidY }
            };
        }

        public Dictionary<string, List<double>> GetTendonXYAt(double alignDistance)
        {
            double param = alignDistance / GLength;
            List<double> tendonLocalX = new List<double>();
            List<double> tendonLocalY = new List<double>();
            for (int i = 0; i < Tendons.Count; i++)
            {
                GTendon tendon = Tendons[i];
                Point localPoint = tendon.Profile3D.LocalPointAtParamter((alignDistance - tendon.LocateDistance) / tendon.Profile3D.LocalXValues.Last(),
                    (alignDistance - tendon.LocateDistance) / tendon.Profile3D.LocalXValues.Last());
                double x = localPoint.Y;
                double y = tendon.LocateHeight - localPoint.Z;
                tendonLocalX.Add(x);
                tendonLocalY.Add(y);
            }
            return new Dictionary<string, List<double>>
            {
                { "LocalX", tendonLocalX },
                { "LocalY", tendonLocalY }
            };
        }

        public List<Point> GetTendonPointAt(double alignDistance)
        {
            Dictionary<string, List<double>> tendonXY = GetTendonXYAt(alignDistance);
            List<double> localX = tendonXY["LocalX"];
            List<double> localY = tendonXY["LocalY"];
            Plane alPlane = Alignment.PlaneAtParameter(alignDistance / GLength);
            List<Point> points = new List<Point>();
            for (int i = 0; i < localX.Count; i++)
            {
                Point localPoint = Point.ByCoordinates(0, localX[i], localY[i]);
                Point rotPoint = GeometryFunctions.GetPointAt(alPlane, localPoint.Y, -1 * localPoint.Z, 0);
                points.Add(rotPoint);
            }
            return points;
        }

        // 3) 선형 지점 고르면 텐던 위치 뽑기

        // 내부함수
        internal static List<Point> GetHalfSection(List<Point> section1, List<Point> section2)
        {
            // 두개의 단면을 받아서 중간점을 연결한 단면을 반환
            List<Point> resultPoints = new List<Point>();
            for (int i = 0; i < section1.Count; i++)
            {
                Line l = Line.ByStartPointEndPoint(section1[i], section2[i]);
                Point p = l.PointAtParameter(0.5);
                resultPoints.Add(p);
            }
            return resultPoints;
        }

        internal static List<Curve> RemoveIntersectCurves(List<Curve> curves)
        {
            // 만약 curves가 서로 중복하거나intersect 하면 한개는 제외 
            for (int k = 0; k < curves.Count - 1; k++)
            {
                if (curves[k].DoesIntersect(curves[k + 1]))
                {
                    curves.RemoveAt(k);
                }
            }
            for (int r = 0; r < curves.Count - 1; r++)
            {
                if (curves[r].DoesIntersect(curves[r + 1]))
                {
                    curves.RemoveAt(r);
                }
            }

            return curves;
        }
    }
}
