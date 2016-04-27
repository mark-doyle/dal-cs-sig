using RWS.Lib.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWS.Lib.Storage
{
    public class LocationRepository
    {
        private TableStorageService<Location> _locationStorage;

        public LocationRepository()
        {
            _locationStorage = new TableStorageService<Location>("Location");
        }

        public Location GetLocation(string country, string state, string zipCode)
        {
            return _locationStorage.GetEntity(
                Location.GetPartitionKey(country, state),
                Location.GetRowKey(zipCode));
        }

        public async Task<Location> GetLocationAsync(string country, string state, string zipCode)
        {
            return await _locationStorage.GetEntityAsync(
                Location.GetPartitionKey(country, state),
                Location.GetRowKey(zipCode));
        }

        public List<Location> GetLocations(string country, string state)
        {
            return _locationStorage.GetEntities(
                Location.GetPartitionKey(country, state)).ToList();
        }

        public async Task<List<Location>> GetLocationsAsync(string country, string state)
        {
            return (await _locationStorage.GetEntitiesAsync(
                Location.GetPartitionKey(country, state))).ToList();
        }

        public List<Location> GetAllLocations()
        {
            return _locationStorage.GetAllEntities().ToList();
        }

        public async Task<List<Location>> GetAllLocationsAsync()
        {
            return (await _locationStorage.GetAllEntitiesAsync()).ToList();
        }

    }
}
