using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using KDHBridge_DYN48.Element.ModelAlignment;
using KDHBridge_DYN48.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Girder
{
    [IsVisibleInDynamoLibrary(false)]
    public class GAlignment : IModelAlignment
    {
        public Curve TopCurve { get; set; }
        public Curve BottomCurve { get; set; }
        public List<Point> ControlPoints { get; set; }
        public Vector LineVector { get; set; }

        internal GAlignment(Girder girder, Point refPoint1, Point refPoint2,
            double camber, double lateral, double inter1=0, double inter2=0)
        {
            // 거더 방향(direction: 교축, tDirection: 교축 직각)
            Vector direction = Vector.ByTwoPoints(refPoint1, refPoint2).Normalized();
            Vector tDirection = direction.Cross(Vector.ZAxis()).Normalized();
            Vector zDirection = tDirection.Cross(direction).Normalized();

            // 선형 시작점 생성 => 끝점은 시작점 + 거더 길이 (refPoint에서 길이가 안맞을 수 있음을 고려)
            Point start = (Point)refPoint1.Translate(direction.Reverse(), inter1).Translate(zDirection, girder.GHeight);
            Point end = (Point)start.Translate(direction, girder.GLength);

            // 끝 참조점 재생성 (refPoint1은 고정, refPoint2는 길이에 따라 변화)
            Point refPoint2New = (Point)refPoint1.Translate(direction, girder.GLength -inter1 - inter2);
            

            // 선형 정의
            Curve tempAlignment;
            Curve tempAlignment2;
            List<Point> controlPoints;
            if (camber == 0 & lateral == 0)
            {
                // 솟음, 횡만곡 0 -> 직선
                tempAlignment = Line.ByStartPointEndPoint(start, end);
                controlPoints = new List<Point> { start, end };
                tempAlignment2 = Line.ByStartPointEndPoint(refPoint1, refPoint2New);
                girder.RefPoint1 = refPoint1;
                girder.RefPoint2 = refPoint2New;
            }
            else
            {
                // 그 외는 Arc (참조점 고정상태로 Arc 생성
                Point mid = (Point)start.Translate(direction, girder.GLength / 2)
                    .Translate(tDirection, lateral)
                    .Translate(zDirection, camber);
                
                // 1) 상부 참조점 위치 + mid로 Arc 일부 생성 - inter1, inter2만큼 연장.
                Point topRef1 = (Point)start.Translate(direction, inter1);
                Point topRef2 = (Point)end.Translate(direction.Reverse(), inter2);

                Arc tempArc = Arc.ByThreePoints(topRef1, mid, topRef2);
                Arc tempArcExtend1 = (Arc)tempArc.Extend(inter1, topRef1);
                Arc tempArcExtend2 = (Arc)tempArcExtend1.Extend(inter2, topRef2);
                
                tempAlignment = tempArcExtend2;
                controlPoints = new List<Point> 
                { 
                    tempAlignment.PointAtParameter(0),
                    tempAlignment.PointAtParameter(0.5),
                    tempAlignment.PointAtParameter(1)
                };
                
                Point tempMid = (Point)refPoint1.Translate(direction, girder.GLength / 2)
                    .Translate(tDirection, lateral)
                    .Translate(zDirection, camber);
                tempAlignment2 = Arc.ByThreePoints(refPoint1, tempMid, refPoint2New);

                // 수정
                girder.RefPoint1 = refPoint1;
                girder.RefPoint2 = refPoint2New;
            }
            LineVector = Vector.ByTwoPoints(girder.RefPoint1, girder.RefPoint2);
            TopCurve = tempAlignment;
            ControlPoints = controlPoints;
            BottomCurve = tempAlignment2;
        }

        public Point PointAtParameter(double param) => TopCurve.PointAtParameter(param);
        public Vector TangentAtParameter(double param) => TopCurve.TangentAtParameter(param);
        public Plane PlaneAtParameter(double param)
        {
            // X축을 바라보는 평면 만들기 - 회전하기
            Point origin = TopCurve.PointAtParameter(param);
            Plane tempPlane = Plane.ByOriginNormalXAxis(origin, Vector.XAxis(), Vector.YAxis());
            return (Plane)GeometryFunctions.RotateByCurve(tempPlane, Vector.XAxis(), TopCurve, param);
        }

    }
}
