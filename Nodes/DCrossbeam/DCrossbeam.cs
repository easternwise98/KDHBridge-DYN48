using KDHBridge_DYN48.Crossbeam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KDHBridge_DYN48.Girder;
using KDHBridge_DYN48.Element.Section;
using Autodesk.DesignScript.Runtime;

namespace KDHBridge_DYN48.Nodes.DCrossbeam
{
    public static class DCrossbeam
    {
        public static HCrossbeam SetHCrossbeam(Girder.Girder startGirder, Girder.Girder endGirder, string name)
        {
            return new HCrossbeam(startGirder, endGirder, name);
        }

        [MultiReturn(new[] { "HCrossbeam", "CrossbeamAlignment", "StartSection", "EndSection"})]
        public static Dictionary<string, object> SetCrossbeamSection(HCrossbeam crossbeam, HSection startSection, 
            HSection endSection, double startLoc, double startWidth, double startHeight, double endLoc, double endWidth, double endHeight)
        {
            crossbeam.SetCrossbeamSection(startSection, endSection);
            crossbeam.SetCrossbeamCurve(startLoc, startWidth, startHeight, endLoc, endWidth, endHeight);

            return new Dictionary<string, object>
            {
                { "HCrossbeam", crossbeam },
                { "CrossbeamAlignment", crossbeam.AlignmentCurve },
                { "StartSection", crossbeam.StartSectionCurve },
                { "EndSection", crossbeam.EndSectionCurve }
            };
        }

        [MultiReturn(new[] { "HCrossbeam", "Solid"})]
        public static Dictionary<string, object> SetCrossbeamSolid(HCrossbeam crossbeam)
        {
            crossbeam.SetSolid();
            return new Dictionary<string, object>
            {
                { "HCrossbeam", crossbeam },
                { "Solid", crossbeam.Solid }
            };
        }


    }
}
