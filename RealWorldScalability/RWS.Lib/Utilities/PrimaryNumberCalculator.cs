using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWS.Lib.Utilities
{
    public static class PrimaryNumberCalculator
    {
        public static bool IsPrime(int number)
        {
            bool isPrime = true;

            if (number == 2)
            {
                isPrime = true;
            }
            else if (number == 1 || number % 2 == 0)
            {
                isPrime = false;
            }
            else
            {
                int ceiling = (number + 1) / 2;
                for (int i = 3; i < ceiling; i += 2)
                {
                    if (number % i == 0)
                    {
                        isPrime = false;
                        break;
                    };
                }
            }

            return isPrime;

        }
    }
}
