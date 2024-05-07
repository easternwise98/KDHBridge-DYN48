using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDHBridge_DYN48.Utility
{
    internal class MathFunctions
    {
        // Rectangle Section
        public static double GetRecArea(double width, double height) { return width * height; }
        public static double GetRecInertiaY(double width, double height) { return width * Math.Pow(height, 3) / 12; }
        public static double GetRecInertiaX(double width, double height) { return height * Math.Pow(width, 3) / 12; }
        public static double GetRecCentroidY(double height) { return height / 2; }
        public static double GetRecCentroidX(double width) { return width / 2; }

        // Triangle Section
        public static double GetTriArea(double baseLength, double height) { return baseLength * height / 2; }
        public static double GetTriInertiaY(double baseLength, double height) { return baseLength * Math.Pow(height, 3) / 36; }
        public static double GetTriInertiaX(double baseLength, double height) { return height * Math.Pow(baseLength, 3) / 36; }
        public static double GetTriCentroidY(double height) { return height / 3; }
        public static double GetTriCentroidX(double baseLength) { return baseLength / 3; }
    }
}
