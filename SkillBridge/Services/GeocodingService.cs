using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkillBridge.Services
{
    /// <summary>
    /// Stub geocoding service. In production, integrate with Nominatim or Google Maps API.
    /// </summary>
    public class GeocodingService : IGeocodingService
    {
        private readonly ILogger<GeocodingService> _logger;

        public GeocodingService(ILogger<GeocodingService> logger)
        {
            _logger = logger;
        }

        public Task<(double lat, double lng)?> GeocodeAsync(string address)
        {
            _logger.LogWarning("GeocodingService is a stub. Returning null for address: {Address}", address);
            // TODO: Integrate with Nominatim (OpenStreetMap) or Google Geocoding API
            // For development, return a default South Africa location if address is not empty
            if (!string.IsNullOrWhiteSpace(address))
            {
                // Default: Johannesburg CBD
                return Task.FromResult<(double lat, double lng)?>( (-26.2041, 28.0473) );
            }
            return Task.FromResult<(double lat, double lng)?>(null);
        }
    }
}
