using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RWS.Lib.Entities
{
    [DataContract]
    public class PrimeCalculation
    {
        public PrimeCalculation()
        {
            this.PrimeNumbers = new List<int>();
        }

        public PrimeCalculation(int start, int end, List<int> primes)
        {
            this.RangeStart = start;
            this.RangeEnd = end;
            this.PrimeNumbers = primes;
        }

        [DataMember]
        public int RangeStart { get; set; }
        [DataMember]
        public int RangeEnd { get; set; }
        [DataMember]
        public List<int> PrimeNumbers { get; set; }
    }
}
