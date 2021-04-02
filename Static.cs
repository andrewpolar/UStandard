using System;
using System.Collections.Generic;
using System.Text;

namespace UStandard
{
    static class Static
    {
        public static double GetMin(double[] x)
        {
            double min = x[0];
            for (int i = 0; i < x.Length; ++i)
            {
                if (x[i] < min) min = x[i];
            }
            return min;
        }

        public static double GetMax(double[] x)
        {
            double max = x[0];
            for (int i = 0; i < x.Length; ++i)
            {
                if (x[i] > max) max = x[i];
            }
            return max;
        }

        private static double GetMeanValue(double[] x)
        {
            double mean = 0.0;
            for (int i = 0; i < x.Length; ++i)
            {
                mean += x[i];
            }
            mean /= (double)(x.Length);
            return mean;
        }

        public static double PearsonCorrelation(double[] x, double[] y)
        {
            int length = x.Length;
            if (length > y.Length)
            {
                length = y.Length;
            }

            double xy = 0.0;
            double x2 = 0.0;
            double y2 = 0.0;
            for (int i = 0; i < length; ++i)
            {
                xy += x[i] * y[i];
                x2 += x[i] * x[i];
                y2 += y[i] * y[i];
            }
            xy /= (double)(length);
            x2 /= (double)(length);
            y2 /= (double)(length);
            double xav = 0.0;
            for (int i = 0; i < length; ++i)
            {
                xav += x[i];
            }
            xav /= length;
            double yav = 0.0;
            for (int i = 0; i < length; ++i)
            {
                yav += y[i];
            }
            yav /= length;
            double ro = xy - xav * yav;
            ro /= Math.Sqrt(x2 - xav * xav);
            ro /= Math.Sqrt(y2 - yav * yav);
            return ro;
        }
    }
}
