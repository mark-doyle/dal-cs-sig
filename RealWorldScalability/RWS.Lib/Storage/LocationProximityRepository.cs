using RWS.Lib.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWS.Lib.Storage
{
    public class LocationProximityRepository
    {
        private string _tableName;
        private TableStorageService<LocationProximity> _locationProximityStorage;

        public LocationProximityRepository(string tableName)
        {
            // Ordinarily, the consumer of this repo wouldn't need to know or specify the table name.
            // However, for the test, we need the results to be separate.
            _tableName = tableName;
            _locationProximityStorage = new TableStorageService<LocationProximity>(_tableName);
        }

        public LocationProximity GetLocationProximity(string country1, string state1, string zipCode1, string country2, string state2, string zipCode2)
        {
            return _locationProximityStorage.GetEntity(
                LocationProximity.GetKey(country1, state1, zipCode1),
                LocationProximity.GetKey(country2, state2, zipCode2)
                );
        }

        public async Task<LocationProximity> GetLocationProximityAsync(string country1, string state1, string zipCode1, string country2, string state2, string zipCode2)
        {
            return await _locationProximityStorage.GetEntityAsync(
                LocationProximity.GetKey(country1, state1, zipCode1),
                LocationProximity.GetKey(country2, state2, zipCode2)
                );
        }

        public void SaveLocationProximity(LocationProximity locationProximity)
        {
            _locationProximityStorage.UpsertEntity(locationProximity);
        }

        public async Task SaveLocationProximityAsync(LocationProximity locationProximity)
        {
            await _locationProximityStorage.UpsertEntityAsync(locationProximity);
        }

    }
}
