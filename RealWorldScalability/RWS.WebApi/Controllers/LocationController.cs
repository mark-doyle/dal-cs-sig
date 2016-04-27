using RWS.Lib.Caching;
using RWS.Lib.Entities;
using RWS.Lib.Storage;
using RWS.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RWS.WebApi.Controllers
{
    public class LocationController : ApiController
    {
        private LocationRepository _locationRepo;
        private LocationProximityRepository _locationProximityRepo1;
        private LocationProximityRepository _locationProximityRepo2;
        private LocationProximityRepository _locationProximityRepo3;
        private LocationProximityRepository _locationProximityRepo4;

        public LocationController()
        {
            _locationRepo = new LocationRepository();
            _locationProximityRepo1 = new LocationProximityRepository("LocationProximity1");
            _locationProximityRepo2 = new LocationProximityRepository("LocationProximity2");
            _locationProximityRepo3 = new LocationProximityRepository("LocationProximity3");
            _locationProximityRepo4 = new LocationProximityRepository("LocationProximity4");
        }

        #region Distance calculation, no result caching or storage

        [HttpGet]
        [ActionName("GetProximity")]
        public LocationProximity GetProximity(string country1, string state1, string zipCode1, string country2, string state2, string zipCode2)
        {
            LocationProximity locationProximity = null;

            Location location1 = _locationRepo.GetLocation(country1, state1, zipCode1);
            Location location2 = _locationRepo.GetLocation(country2, state2, zipCode2);
            if (location1 != null && location2 != null)
            {
                double distance = DistanceCalculator.GetDistanceInMiles(location1.Latitude, location1.Longitude, location2.Latitude, location2.Longitude);
                locationProximity = new LocationProximity(location1, location2, distance);
            }

            return locationProximity;
        }

        [HttpGet]
        [ActionName("GetProximityAsync")]
        public async Task<LocationProximity> GetProximityAsync(string country1, string state1, string zipCode1, string country2, string state2, string zipCode2)
        {
            LocationProximity locationProximity = null;
            Location location1 = null;
            Location location2 = null;

            await Task.WhenAll(
                Task.Run(async () => { location1 = await _locationRepo.GetLocationAsync(country1, state1, zipCode1); }),
                Task.Run(async () => { location2 = await _locationRepo.GetLocationAsync(country2, state2, zipCode2); })
                );

            if (location1 != null && location2 != null)
            {
                double distance = DistanceCalculator.GetDistanceInMiles(location1.Latitude, location1.Longitude, location2.Latitude, location2.Longitude);
                locationProximity = new LocationProximity(location1, location2, distance);
            }

            return locationProximity;
        }

        #endregion // Distance calculation, no result caching or storage

        #region Distance calculation, stored results but no caching

        [HttpGet]
        [ActionName("GetStoredProximity")]
        public LocationProximity GetStoredProximity(string country1, string state1, string zipCode1, string country2, string state2, string zipCode2)
        {
            LocationProximity locationProximity = _locationProximityRepo1.GetLocationProximity(country1, state1, zipCode1, country2, state2, zipCode2);
            if (locationProximity == null)
            {
                Location location1 = _locationRepo.GetLocation(country1, state1, zipCode1);
                Location location2 = _locationRepo.GetLocation(country2, state2, zipCode2);
                if (location1 != null && location2 != null)
                {
                    double distance = DistanceCalculator.GetDistanceInMiles(location1.Latitude, location1.Longitude, location2.Latitude, location2.Longitude);
                    locationProximity = new LocationProximity(location1, location2, distance);
                    LocationProximity reversed = new LocationProximity(location2, location1, distance);

                    _locationProximityRepo1.SaveLocationProximity(locationProximity);
                    _locationProximityRepo1.SaveLocationProximity(reversed);
                }
            }

            return locationProximity;
        }

        [HttpGet]
        [ActionName("GetStoredProximityAsync")]
        public async Task<LocationProximity> GetStoredProximityAsync(string country1, string state1, string zipCode1, string country2, string state2, string zipCode2)
        {
            LocationProximity locationProximity = await _locationProximityRepo2.GetLocationProximityAsync(country1, state1, zipCode1, country2, state2, zipCode2);
            if (locationProximity == null)
            {
                Location location1 = null;
                Location location2 = null;

                await Task.WhenAll(
                    Task.Run(async () => { location1 = await _locationRepo.GetLocationAsync(country1, state1, zipCode1); }),
                    Task.Run(async () => { location2 = await _locationRepo.GetLocationAsync(country2, state2, zipCode2); })
                    );

                if (location1 != null && location2 != null)
                {
                    double distance = DistanceCalculator.GetDistanceInMiles(location1.Latitude, location1.Longitude, location2.Latitude, location2.Longitude);
                    locationProximity = new LocationProximity(location1, location2, distance);
                    LocationProximity reversed = new LocationProximity(location2, location1, distance);

                    await Task.WhenAll(
                        _locationProximityRepo2.SaveLocationProximityAsync(locationProximity),
                        _locationProximityRepo2.SaveLocationProximityAsync(reversed)
                        );
                }
            }

            return locationProximity;
        }

        #endregion // Distance calculation, stored results but no caching

        #region Distance calculation, stored and in-mem cached results

        #region Helpers

        private string _GetLocationProximityCacheKey(string country1, string state1, string zipCode1, string country2, string state2, string zipCode2, bool isAsync)
        {
            return string.Format("_LocProx{6}-{0}.{1}.{2}-{3}.{4}.{5}",
                country1, state1, zipCode1, country2, state2, zipCode2, (isAsync ? "Async" : "")
                );
        }

        private LocationProximity _GetCachedLocationProximity(LocationProximityRepository repo, string country1, string state1, string zipCode1, string country2, string state2, string zipCode2)
        {
            string key = _GetLocationProximityCacheKey(country1, state1, zipCode1, country2, state2, zipCode2, false);
            return InMemoryCacheService.GetCachedItem<LocationProximity>(key, () =>
            {
                return repo.GetLocationProximity(country1, state1, zipCode1, country2, state2, zipCode2);
            });
        }

        private async Task<LocationProximity> _GetCachedLocationProximityAsync(LocationProximityRepository repo, string country1, string state1, string zipCode1, string country2, string state2, string zipCode2)
        {
            string key = _GetLocationProximityCacheKey(country1, state1, zipCode1, country2, state2, zipCode2, true);
            return await InMemoryCacheService.GetCachedItemAsync<LocationProximity>(key, async () =>
            {
                return await repo.GetLocationProximityAsync(country1, state1, zipCode1, country2, state2, zipCode2);
            });
        }

        private void _CacheAndSaveLocationProximity(LocationProximityRepository repo, LocationProximity locationProximity)
        {
            string key = _GetLocationProximityCacheKey(
                locationProximity.SourceCountry, locationProximity.SourceState, locationProximity.SourceZipCode,
                locationProximity.DestinationCountry, locationProximity.DestinationState, locationProximity.DestinationZipCode, 
                false);
            InMemoryCacheService.AddItem(locationProximity, key);
            repo.SaveLocationProximity(locationProximity);
        }

        private async Task _CacheAndSaveLocationProximityAsync(LocationProximityRepository repo, LocationProximity locationProximity)
        {
            string key = _GetLocationProximityCacheKey(
                locationProximity.SourceCountry, locationProximity.SourceState, locationProximity.SourceZipCode,
                locationProximity.DestinationCountry, locationProximity.DestinationState, locationProximity.DestinationZipCode,
                true);
            await Task.WhenAll(
                InMemoryCacheService.AddItemAsync(locationProximity, key),
                repo.SaveLocationProximityAsync(locationProximity)
                );
        }

        #endregion // Helpers

        [HttpGet]
        [ActionName("GetCachedStoredProximity")]
        public LocationProximity GetCachedStoredProximity(string country1, string state1, string zipCode1, string country2, string state2, string zipCode2)
        {
            LocationProximity locationProximity = _GetCachedLocationProximity(_locationProximityRepo3, country1, state1, zipCode1, country2, state2, zipCode2);
            if (locationProximity == null)
            {
                Location location1 = _locationRepo.GetLocation(country1, state1, zipCode1);
                Location location2 = _locationRepo.GetLocation(country2, state2, zipCode2);
                if (location1 != null && location2 != null)
                {
                    double distance = DistanceCalculator.GetDistanceInMiles(location1.Latitude, location1.Longitude, location2.Latitude, location2.Longitude);
                    locationProximity = new LocationProximity(location1, location2, distance);
                    LocationProximity reversed = new LocationProximity(location2, location1, distance);

                    _CacheAndSaveLocationProximity(_locationProximityRepo3, locationProximity);
                    _CacheAndSaveLocationProximity(_locationProximityRepo3, reversed);
                }
            }

            return locationProximity;
        }

        [HttpGet]
        [ActionName("GetCachedStoredProximityAsync")]
        public async Task<LocationProximity> GetCachedStoredProximityAsync(string country1, string state1, string zipCode1, string country2, string state2, string zipCode2)
        {
            LocationProximity locationProximity = await _GetCachedLocationProximityAsync(_locationProximityRepo4, country1, state1, zipCode1, country2, state2, zipCode2);
            if (locationProximity == null)
            {
                Location location1 = null;
                Location location2 = null;

                await Task.WhenAll(
                    Task.Run(async () => { location1 = await _locationRepo.GetLocationAsync(country1, state1, zipCode1); }),
                    Task.Run(async () => { location2 = await _locationRepo.GetLocationAsync(country2, state2, zipCode2); })
                    );

                if (location1 != null && location2 != null)
                {
                    double distance = DistanceCalculator.GetDistanceInMiles(location1.Latitude, location1.Longitude, location2.Latitude, location2.Longitude);
                    locationProximity = new LocationProximity(location1, location2, distance);
                    LocationProximity reversed = new LocationProximity(location2, location1, distance);

                    await Task.WhenAll(
                        _CacheAndSaveLocationProximityAsync(_locationProximityRepo4, locationProximity),
                        _CacheAndSaveLocationProximityAsync(_locationProximityRepo4, reversed)
                        );
                }
            }

            return locationProximity;
        }

        #endregion // Distance calculation, stored and in-mem cached results

        #region Relative distance calculation, no storage or caching of results

        #region Helpers

        private List<Location> _GetAllLocationsCached()
        {
            return InMemoryCacheService.GetCachedItem<List<Location>>("AllLocations", () =>
            {
                return _locationRepo.GetAllLocations();
            });
        }

        private async Task<List<Location>> _GetAllLocationsCachedAsync()
        {
            return await InMemoryCacheService.GetCachedItemAsync<List<Location>>("AllLocations", async () =>
            {
                return await _locationRepo.GetAllLocationsAsync();
            });
        }

        #endregion // Helpers

        [HttpGet]
        [ActionName("GetNearest")]
        public List<RelativeLocation> GetNearest(string country, string state, string zipCode, int limit)
        {
            List<RelativeLocation> nearest = null;

            List<Location> locations = _GetAllLocationsCached();
            Location current = locations.FirstOrDefault(loc => loc.Country == country && loc.State == state && loc.ZipCode == zipCode);

            if (current != null)
            {
                nearest = locations
                    .Where(loc => !(loc.Country == country && loc.State == state && loc.ZipCode == zipCode))
                    .Select(loc => new RelativeLocation(loc, DistanceCalculator.GetDistanceInMiles(current.Latitude, current.Longitude, loc.Latitude, loc.Longitude), 0))
                    .OrderBy(loc => loc.Distance)
                    .Take(limit)
                    .ToList();
                nearest.ForEach(loc => loc.Sequence = (nearest.IndexOf(loc) + 1));
            }

            return nearest;
        }

        [HttpGet]
        [ActionName("GetNearestAsync")]
        public async Task<List<RelativeLocation>> GetNearestAsync(string country, string state, string zipCode, int limit)
        {
            List<RelativeLocation> nearest = null;

            List<Location> locations = await _GetAllLocationsCachedAsync();
            Location current = locations.FirstOrDefault(loc => loc.Country == country && loc.State == state && loc.ZipCode == zipCode);

            if (current != null)
            {
                nearest = (await Task.WhenAll(
                    locations
                    .Where(loc => !(loc.Country == country && loc.State == state && loc.ZipCode == zipCode))
                    .Select(loc => Task.Run(() => new RelativeLocation(loc, DistanceCalculator.GetDistanceInMiles(current.Latitude, current.Longitude, loc.Latitude, loc.Longitude), 0)))
                    ))
                    .OrderBy(loc => loc.Distance)
                    .Take(limit)
                    .ToList();
                nearest.ForEach(loc => loc.Sequence = (nearest.IndexOf(loc) + 1));
            }

            return nearest;
        }

        #endregion // Relative distance calculation, no storage or caching of results

        #region Relative distance calculation, using distributed caching of results

        #region Helpers

        private string _GetRelativeLocationCacheKey(string country, string state, string zipCode, int limit, bool isAsync)
        {
            return string.Format("_RelLoc{4}-{0}.{1}.{2}-{3}",
                country, state, zipCode, limit.ToString(), (isAsync ? "Async" : "")
                );
        }

        #endregion // Helpers

        [HttpGet]
        [ActionName("GetNearestCached")]
        public List<RelativeLocation> GetNearestCached(string country, string state, string zipCode, int limit)
        {
            string key = _GetRelativeLocationCacheKey(country, state, zipCode, limit, false);
            return DistributedCacheService.GetCachedItem<List<RelativeLocation>>(key, () =>
            {
                List<RelativeLocation> nearest = null;

                List<Location> locations = _GetAllLocationsCached();
                Location current = locations.FirstOrDefault(loc => loc.Country == country && loc.State == state && loc.ZipCode == zipCode);

                if (current != null)
                {
                    nearest = locations
                        .Where(loc => !(loc.Country == country && loc.State == state && loc.ZipCode == zipCode))
                        .Select(loc => new RelativeLocation(loc, DistanceCalculator.GetDistanceInMiles(current.Latitude, current.Longitude, loc.Latitude, loc.Longitude), 0))
                        .OrderBy(loc => loc.Distance)
                        .Take(limit)
                        .ToList();
                    nearest.ForEach(loc => loc.Sequence = (nearest.IndexOf(loc) + 1));
                }

                return nearest;
            });
        }

        [HttpGet]
        [ActionName("GetNearestCachedAsync")]
        public async Task<List<RelativeLocation>> GetNearestCachedAsync(string country, string state, string zipCode, int limit)
        {
            string key = _GetRelativeLocationCacheKey(country, state, zipCode, limit, true);
            return await DistributedCacheService.GetCachedItemAsync<List<RelativeLocation>>(key, async () =>
            {
                List<RelativeLocation> nearest = null;

                List<Location> locations = await _GetAllLocationsCachedAsync();
                Location current = locations.FirstOrDefault(loc => loc.Country == country && loc.State == state && loc.ZipCode == zipCode);

                if (current != null)
                {
                    nearest = (await Task.WhenAll(
                        locations
                        .Where(loc => !(loc.Country == country && loc.State == state && loc.ZipCode == zipCode))
                        .Select(loc => Task.Run(() => new RelativeLocation(loc, DistanceCalculator.GetDistanceInMiles(current.Latitude, current.Longitude, loc.Latitude, loc.Longitude), 0)))
                        ))
                        .OrderBy(loc => loc.Distance)
                        .Take(limit)
                        .ToList();
                    nearest.ForEach(loc => loc.Sequence = (nearest.IndexOf(loc) + 1));
                }

                return nearest;
            });
        }

        #endregion // Relative distance calculation, using distributed caching of results

    }
}
