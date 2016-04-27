using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RWS.Lib.Entities
{
    [DataContract]
    public class LocationProximity : TableEntity
    {
        public LocationProximity()
        {

        }

        public LocationProximity(Location source, Location destination, double distance)
        {
            if (source != null)
            {
                this.SourceCountry = source.Country;
                this.SourceState = source.State;
                this.SourceZipCode = source.ZipCode;
                this.SourceLatitude = source.Latitude;
                this.SourceLongitude = source.Longitude;
            }
            if (destination != null)
            {
                this.DestinationCountry = destination.Country;
                this.DestinationState = destination.State;
                this.DestinationZipCode = destination.ZipCode;
                this.DestinationLatitude = destination.Latitude;
                this.DestinationLongitude = destination.Longitude;
            }
            this.Distance = distance;

            this.PartitionKey = BuildPartitionKey();
            this.RowKey = BuildRowKey();
        }

        protected virtual string BuildPartitionKey()
        {
            return GetKey(this.SourceCountry, this.SourceState, this.SourceZipCode);
        }

        protected virtual string BuildRowKey()
        {
            return GetKey(this.DestinationCountry, this.DestinationState, this.DestinationZipCode);
        }

        public static string GetKey(string country, string state, string zipCode)
        {
            return
                (country ?? "us").ToLower() + "." +
                (state ?? "zz").ToLower() + "." +
                (zipCode ?? "00000").ToLower();
        }

        [DataMember]
        public string SourceCountry { get; set; }
        [DataMember]
        public string SourceState { get; set; }
        [DataMember]
        public string SourceZipCode { get; set; }
        [DataMember]
        public double SourceLatitude { get; set; }
        [DataMember]
        public double SourceLongitude { get; set; }

        [DataMember]
        public string DestinationCountry { get; set; }
        [DataMember]
        public string DestinationState { get; set; }
        [DataMember]
        public string DestinationZipCode { get; set; }
        [DataMember]
        public double DestinationLatitude { get; set; }
        [DataMember]
        public double DestinationLongitude { get; set; }

        [DataMember]
        public double Distance { get; set; }

    }
}
