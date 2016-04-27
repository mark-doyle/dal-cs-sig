using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWS.Lib.Utilities
{
    public static class DistanceCalculator
    {
        // Based on https://www.geodatasource.com/developers/c-sharp

        public static double GetDistanceInMiles(double lat1, double lon1, double lat2, double lon2)//, char unit)
        {
            double theta = lon1 - lon2;
            double dist = 
                Math.Sin(ConvertDegreesToRadians(lat1)) * Math.Sin(ConvertDegreesToRadians(lat2)) + 
                Math.Cos(ConvertDegreesToRadians(lat1)) * Math.Cos(ConvertDegreesToRadians(lat2)) * 
                Math.Cos(ConvertDegreesToRadians(theta));
            dist = Math.Acos(dist);
            dist = ConvertRadiansToDegrees(dist);
            dist = dist * 60 * 1.1515;
            //if (unit == 'K')
            //{
            //    dist = dist * 1.609344;
            //}
            //else if (unit == 'N')
            //{
            //    dist = dist * 0.8684;
            //}
            return (dist);
        }

        public static double ConvertDegreesToRadians(double degrees)
        {
            return (degrees * Math.PI / 180.0);
        }

        public static double ConvertRadiansToDegrees(double radians)
        {
            return (radians / Math.PI * 180.0);
        }
    }
}
