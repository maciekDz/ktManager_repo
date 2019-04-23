using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace My
{
    class Convert
    {
        const string defaulDate = "1900-1-1";

        public static int toInt(string val)
        {
            try
            {
                return int.Parse(val);
            }
            catch (Exception )
            {
                return  0;
               
            }
        }

        public static DateTime toDate(string val)
        {
            try
            {
                return DateTime.Parse(val);
            }
            catch (Exception)
            {
                return DateTime.Parse(defaulDate);

            }
        }

        public static double toDouble(string val)
        {
            try
            {
                return double.Parse(val);
            }
            catch (Exception)
            {
                return 0;

            }
        }

        public static double roundToTwo(string val)
        {
            try
            {
                return Math.Round(double.Parse(val),2);
            }
            catch (Exception)
            {
                return 0;

            }
        }

        public static double formatControlToDouble(Control ctrl)
        {
            try
            {
                return Math.Round(double.Parse(ctrl.Text), 2);
            }
            catch (Exception)
            {
                return 0;

            }
        }

        public static decimal toDecimal(string val)
        {
            try
            {
                return decimal.Parse(val);
            }
            catch (Exception)
            {
                return 0;

            }
        }
    }
}
