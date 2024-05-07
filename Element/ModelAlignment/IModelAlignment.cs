using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Element.ModelAlignment
{
    internal interface IModelAlignment
    {
        Point PointAtParameter(double param);
        Vector TangentAtParameter(double param);
        Plane PlaneAtParameter(double param);

    }
}
