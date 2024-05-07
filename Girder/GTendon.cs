using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DynamoUnits;
using KDHBridge_DYN48.Element.MathCurve;
using KDHBridge_DYN48.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Girder
{
    [IsVisibleInDynamoLibrary(false)]
    public class GTendon
    {
        // 평면과 변수를 받으면 MathCurve로 Profile을 생성함.
        internal int ID { get; set; }
        internal Girder HostGirder { get; set; }
        internal double Diameter { get; set; }
        internal double Area { get; set; }
        internal double DuctDiameter { get; set; }
        internal double DuctArea { get; set; }
        internal double StrandDiameter { get; set; }
        internal int StrandNumber { get; set; }
        internal double StrandArea { get; set; }

        public double ETendon { get; set; }
        public double PSStress { get; set; }

        internal MathCurve2D HProfile2D { get; set; }
        internal MathCurve2D VProfile2D { get; set; }
        internal MathCurve3D Profile3D { get; set; }
        internal double LocateDistance { get; set; }
        internal double LocateHeight { get; set; }
        
        public Curve VisualCurve { get; set; }
        public List<Point> VisualPoints { get; set; }

        public Solid TendonSolid { get; set; }
        public Solid DuctSolid { get; set; }



        public GTendon(int id, Girder girder, double diameter, double ductDiameter, double strandDiameter, int strandNumber, 
            double eTendon, double psStress)
        {
            ID = id;
            HostGirder = girder;
            Diameter = diameter;
            Area = Math.PI * Math.Pow(Diameter / 2, 2);
            DuctDiameter = ductDiameter;
            DuctArea = Math.PI * Math.Pow(DuctDiameter / 2, 2);
            StrandDiameter = strandDiameter;
            StrandNumber = strandNumber;
            StrandArea = Math.PI * Math.Pow(StrandDiameter / 2, 2) * StrandNumber;

            ETendon = eTendon;
            PSStress = psStress;
            
        }

        public void Set2DHProfileByData(Plane plane, List<int> ids, List<string> types, List<double> lengths, List<double> rad1s,
            List<double> rad2s, List<double> angles, double tol = 0.1)
        {
            HProfile2D = new MathCurve2D(plane, ids, types, lengths, rad1s, rad2s, angles, tol);
        }

        public void Set2DVProfileByData(Plane plane, List<int> ids, List<string> types, List<double> lengths, List<double> rad1s,
            List<double> rad2s, List<double> angles, double tol = 0.1)
        {
            VProfile2D = new MathCurve2D(plane, ids, types, lengths, rad1s, rad2s, angles, tol);
        }

        public void Set2DHProfile(MathCurve2D hProfile2D)
        {
            HProfile2D = hProfile2D;
        }

        public void Set2DVProfile(MathCurve2D vProfile2D)
        {
            VProfile2D = vProfile2D;
        }

        public void Set3DProfile(Plane plane, double gDistance, double gHeight, double tol=0.1)
        {
            Profile3D = new MathCurve3D(plane, HProfile2D, VProfile2D);
            LocateDistance = gDistance;
            LocateHeight = gHeight;

            // 캠버가 있으면 거더 위치에 맞춰서 회전하기
            if (HostGirder.Camber != 0 & HostGirder.Lateral != 0)
            {
                int n = (int)(Math.Floor(Profile3D.LocalXValues.Last() / tol) + 1);
                List<double> range = Enumerable.Range(0, n + 1).Select(x => (double)x / n).ToList();
                List<double> localXs = range.Select(x => x * Profile3D.LocalXValues.Last()).ToList();

                // 텐던 위치에서의 거더 포인트
                List<double> girderXs = localXs.Select(x => x + LocateDistance).ToList();
                List<Point> gPoints = girderXs.Select(x => HostGirder.Alignment.PointAtParameter(x / HostGirder.GLength)).ToList();
                List<Vector> gToVectors = girderXs.Select(x => HostGirder.Alignment.TangentAtParameter(x / HostGirder.GLength)).ToList();
                List<Plane> planes = girderXs.Select(x => HostGirder.Alignment.PlaneAtParameter(x / HostGirder.GLength)).ToList();

                // MathCurve에서 Localx, y, z를 구한다음 거더 선형 평면에 대고 점 찍기
                List<Point> rotPoints = new List<Point>();
                for (int i = 0; i < localXs.Count; i++)
                {
                    Point localPoint = Profile3D.LocalPointAtParamter(localXs[i] / Profile3D.LocalXValues.Last(), localXs[i] / Profile3D.LocalXValues.Last());
                    Point rotPoint = GeometryFunctions.GetPointAt(planes[i], localPoint.Y, localPoint.Z - LocateHeight, 0);
                    rotPoints.Add(rotPoint);
                }

                VisualCurve = NurbsCurve.ByPoints(rotPoints);
                VisualPoints = rotPoints;
            }
            else
            {
                VisualCurve = Profile3D.VisualCurve;
                VisualPoints = Profile3D.VisualPoints;
            }
        }

        public void SetTendonSolid()
        {
            // 단면 
            Point startPoint = VisualCurve.StartPoint;
            Vector normal = VisualCurve.TangentAtParameter(0);
            Circle tenCircle = Circle.ByCenterPointRadiusNormal(startPoint, Diameter / 2, normal);
            Circle ductCircle = Circle.ByCenterPointRadiusNormal(startPoint, DuctDiameter / 2, normal);

            // 단면을 이용해서 Solid 생성
            Solid tendonSolid = Solid.BySweep(tenCircle, VisualCurve);
            Solid ductSolid = Solid.BySweep(ductCircle, VisualCurve);

            TendonSolid = tendonSolid;
            DuctSolid = ductSolid;
            
        }

        public Point RotatePointAtParameter(double param, Point origin, Vector from, Vector to)
        {
            Point point = Profile3D.PointAtParameter(param);
            Utility.GeometryFunctions.RotatePointByVector(point, origin, from, to);
            return point;
        }
        
        public Point GetRotatedPointAtLocalX(double localX)
        {
            double girderX = localX + LocateDistance;
            Point gPoint = HostGirder.Alignment.PointAtParameter(girderX / HostGirder.GLength);
            Vector gToVector = HostGirder.Alignment.TangentAtParameter(girderX / HostGirder.GLength);
            return RotatePointAtParameter(localX / Profile3D.LocalXValues.Last(), gPoint, Profile3D.Plane.YAxis, gToVector);
        }



        // Point 얻기
        public Point PointAtParameter(double t)
        {
            return Profile3D.PointAtParameter(t);
        }

        // Tangent 얻기
        public Vector TangentAtParameter(double t)
        {
            return Profile3D.TangentAtParameter(t);
        }

        // Plane 얻기
        public Plane PlaneAtParameter(double t)
        {
            return Profile3D.PlaneAtParameter(t);
        }




    }
}
