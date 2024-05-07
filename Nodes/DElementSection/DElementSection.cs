using Autodesk.DesignScript.Runtime;
using KDHBridge_DYN48.Element.Section;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Nodes.DElementSection
{
    public static class DElementSection
    {
        [MultiReturn(new[] { "SectionPlane", "SectionCurve", "Vertices", "Centroid", "SectionDimensions", "SectionProperties" })]
        public static Dictionary<string, object> GetSectionProperty(IElementSection section)
        {
            return new Dictionary<string, object>
            {
                { "SectionPlane", section.SectionPlane },
                { "SectionCurve", section.SectionCurve },
                { "Vertices", section.Vertices },
                { "Centroid", section.Centroid },
                { "SectionDimensions", section.SectionDimensions },
                { "SectionProperties", section.SectionProperties }
            };
        }
    }
}
