using System.Text.Json;

namespace BloodDonationAPI.Service
{
    public interface IGeoapifyService
    {
        Task<GeoapifyDistanceResult> CalculateDistanceAsync(string origin, string destination);
        Task<List<GeoapifyDistanceResult>> CalculateMultipleDistancesAsync(string origin, List<string> destinations);
        Task<(double latitude, double longitude)> GetCoordinatesAsync(string address);
    }

    public class GeoapifyDistanceResult
    {
        public double DistanceInKm { get; set; }
        public string DistanceText { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
        public string DurationText { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class GeoapifyService : IGeoapifyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GeoapifyService> _logger;

        public GeoapifyService(HttpClient httpClient, IConfiguration configuration, ILogger<GeoapifyService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Geoapify:ApiKey"] ?? "";
            _logger = logger;
        }

        public async Task<GeoapifyDistanceResult> CalculateDistanceAsync(string origin, string destination)
        {
            try
            {
                var results = await CalculateMultipleDistancesAsync(origin, new List<string> { destination });
                return results.FirstOrDefault() ?? new GeoapifyDistanceResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "No result returned",
                    DistanceInKm = double.MaxValue
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance from {Origin} to {Destination}", origin, destination);
                return new GeoapifyDistanceResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = ex.Message,
                    DistanceInKm = double.MaxValue
                };
            }
        }

        public async Task<List<GeoapifyDistanceResult>> CalculateMultipleDistancesAsync(string origin, List<string> destinations)
        {
            var results = new List<GeoapifyDistanceResult>();

            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    // Fallback to mock data when no API key
                    return destinations.Select(d => CreateMockDistanceResult()).ToList();
                }

                // Get coordinates for origin
                var originCoords = await GetCoordinatesAsync(origin);
                if (originCoords.latitude == 0 && originCoords.longitude == 0)
                {
                    return destinations.Select(d => new GeoapifyDistanceResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Cannot get origin coordinates",
                        DistanceInKm = double.MaxValue
                    }).ToList();
                }

                foreach (var destination in destinations)
                {
                    var destCoords = await GetCoordinatesAsync(destination);
                    if (destCoords.latitude == 0 && destCoords.longitude == 0)
                    {
                        results.Add(new GeoapifyDistanceResult
                        {
                            IsSuccess = false,
                            ErrorMessage = "Cannot get destination coordinates",
                            DistanceInKm = double.MaxValue
                        });
                        continue;
                    }

                    // Use Geoapify Routing API to get actual driving distance and time
                    var routingResult = await GetRoutingDataAsync(originCoords, destCoords);
                    results.Add(routingResult);

                    // Add delay to respect rate limits
                    await Task.Delay(100);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating multiple distances from {Origin}", origin);
                return destinations.Select(d => CreateMockDistanceResult()).ToList();
            }
        }

        public async Task<(double latitude, double longitude)> GetCoordinatesAsync(string address)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    // Return mock coordinates for TP.HCM area
                    return (10.7769 + new Random().NextDouble() * 0.1, 106.7009 + new Random().NextDouble() * 0.1);
                }

                var url = $"https://api.geoapify.com/v1/geocode/search?text={Uri.EscapeDataString(address)}&format=json&apiKey={_apiKey}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<GeoapifyGeocodingResponse>(jsonResponse);
                
                if (data?.results?.Length > 0)
                {
                    var result = data.results[0];
                    return (result.lat, result.lon);
                }
                
                // Fallback về tọa độ cơ sở Staff
                return (10.841962, 106.810627);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coordinates for address: {Address}", address);
                return (10.841962, 106.810627);
            }
        }

        private async Task<GeoapifyDistanceResult> GetRoutingDataAsync((double lat, double lon) origin, (double lat, double lon) destination)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    return CreateMockDistanceResult();
                }

                var url = $"https://api.geoapify.com/v1/routing?waypoints={origin.lat},{origin.lon}|{destination.lat},{destination.lon}&mode=drive&apiKey={_apiKey}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<GeoapifyRoutingResponse>(jsonResponse);
                
                if (data?.features?.Length > 0 && data.features[0].properties != null)
                {
                    var props = data.features[0].properties;
                    var distanceInKm = Math.Round(props.distance / 1000.0, 1);
                    var durationInMinutes = props.time / 60;

                    return new GeoapifyDistanceResult
                    {
                        DistanceInKm = distanceInKm,
                        DistanceText = FormatDistance(distanceInKm),
                        DurationInMinutes = durationInMinutes,
                        DurationText = FormatDuration(durationInMinutes),
                        IsSuccess = true
                    };
                }
                
                return CreateMockDistanceResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting routing data");
                return CreateMockDistanceResult();
            }
        }

        private GeoapifyDistanceResult CreateMockDistanceResult()
        {
            var random = new Random();
            var distanceKm = Math.Round(random.NextDouble() * 20 + 1, 1); // 1-21 km
            var durationMinutes = (int)(distanceKm * 3 + random.Next(5, 15)); // Roughly 3 min per km + traffic

            return new GeoapifyDistanceResult
            {
                DistanceInKm = distanceKm,
                DistanceText = FormatDistance(distanceKm),
                DurationInMinutes = durationMinutes,
                DurationText = FormatDuration(durationMinutes),
                IsSuccess = true
            };
        }

        private string FormatDistance(double distanceInKm)
        {
            if (distanceInKm < 1)
            {
                int meters = (int)(distanceInKm * 1000);
                return $"{meters} m";
            }
            else if (distanceInKm < 10)
            {
                return $"{distanceInKm:F1} km";
            }
            else
            {
                return $"{Math.Round(distanceInKm)} km";
            }
        }

        private string FormatDuration(int durationInMinutes)
        {
            if (durationInMinutes < 60)
            {
                return $"{durationInMinutes} phút";
            }
            else
            {
                int hours = durationInMinutes / 60;
                int minutes = durationInMinutes % 60;
                return minutes > 0 ? $"{hours} giờ {minutes} phút" : $"{hours} giờ";
            }
        }
    }

    // DTOs for Geoapify API
    public class GeoapifyGeocodingResponse
    {
        public GeoapifyGeocodingResult[] results { get; set; } = Array.Empty<GeoapifyGeocodingResult>();
    }

    public class GeoapifyGeocodingResult
    {
        public double lat { get; set; }
        public double lon { get; set; }
    }

    public class GeoapifyRoutingResponse
    {
        public GeoapifyFeature[] features { get; set; } = Array.Empty<GeoapifyFeature>();
    }

    public class GeoapifyFeature
    {
        public GeoapifyProperties properties { get; set; } = new();
    }

    public class GeoapifyProperties
    {
        public double distance { get; set; } // in meters
        public int time { get; set; } // in seconds
    }
}
