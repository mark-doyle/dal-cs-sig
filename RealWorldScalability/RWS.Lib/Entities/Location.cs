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
    public class Location : TableEntity
    {
        public Location()
        {
        }

        public Location(string country, string state, string zipCode, double latitude, double longitude)
        {
            this.Country = country;
            this.State = state;
            this.ZipCode = zipCode;
            this.Latitude = latitude;
            this.Longitude = longitude;

            this.PartitionKey = this.BuildPartitionKey();
            this.RowKey = this.BuildRowKey();
        }

        protected virtual string BuildPartitionKey()
        {
            return GetPartitionKey(this.Country, this.State);
        }

        protected virtual string BuildRowKey()
        {
            return GetRowKey(this.ZipCode);
        }

        public static string GetPartitionKey(string country, string state)
        {
            return (country ?? "us").ToLower() + "." + (state ?? "zz").ToLower();
        }

        public static string GetRowKey (string zipCode)
        {
            return (zipCode ?? "00000").ToLower();
        }

        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public string State { get; set; }
        [DataMember]
        public string ZipCode { get; set; }
        [DataMember]
        public double Latitude { get; set; }
        [DataMember]
        public double Longitude { get; set; }
    }
}
