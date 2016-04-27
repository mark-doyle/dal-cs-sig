using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RWS.Lib.Entities
{
    [DataContract]
    public class RelativeLocation : Location
    {
        public RelativeLocation() : base()
        {
        }

        public RelativeLocation(string country, string state, string zipCode, double latitude, double longitude, double distance, int sequence)
            : base(country, state, zipCode, latitude, longitude)
        {
            this.Distance = distance;
            this.Sequence = sequence;

            base.PartitionKey = zipCode;
            base.RowKey = sequence.ToString();

            this.PartitionKey = this.BuildPartitionKey();
            this.RowKey = this.BuildRowKey();
        }

        public RelativeLocation(Location location, double distance, int sequence) 
            : this(location.Country, location.State, location.ZipCode, location.Latitude, location.Longitude, distance, sequence)
        {

        }

        protected override string BuildPartitionKey()
        {
            return base.BuildPartitionKey() + (base.ZipCode ?? "00000").ToLower();
        }

        protected override string BuildRowKey()
        {
            return this.Sequence.ToString();
        }

        [DataMember]
        public int Sequence { get; set; }
        [DataMember]
        public double Distance { get; set; }
    }
}
