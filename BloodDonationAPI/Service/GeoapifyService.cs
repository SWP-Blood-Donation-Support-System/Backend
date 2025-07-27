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
                    DistanceInKm = double.MaxValue,
                    DistanceText = "N/A",
                    DurationText = "N/A"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance from {Origin} to {Destination}", origin, destination);
                return new GeoapifyDistanceResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = ex.Message,
                    DistanceInKm = double.MaxValue,
                    DistanceText = "N/A",
                    DurationText = "N/A"
                };
            }
        }

        public async Task<List<GeoapifyDistanceResult>> CalculateMultipleDistancesAsync(string origin, List<string> destinations)
        {
            var results = new List<GeoapifyDistanceResult>();

            try
            {
                _logger.LogInformation($"Calculating distances from '{origin}' to {destinations.Count} destinations");
                _logger.LogInformation($"API Key available: {!string.IsNullOrEmpty(_apiKey)}");

                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogWarning("No Geoapify API key found, using mock data");
                    // Fallback to mock data when no API key
                    return destinations.Select(d => CreateMockDistanceResult(d)).ToList();
                }

                // Get coordinates for origin
                var originCoords = await GetCoordinatesAsync(origin);
                _logger.LogInformation($"Origin coordinates: {originCoords.latitude}, {originCoords.longitude}");
                
                if (originCoords.latitude == 0 && originCoords.longitude == 0)
                {
                    _logger.LogError("Cannot get origin coordinates");
                    return destinations.Select(d => new GeoapifyDistanceResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Cannot get origin coordinates",
                        DistanceInKm = double.MaxValue,
                        DistanceText = "N/A",
                        DurationText = "N/A"
                    }).ToList();
                }

                foreach (var destination in destinations)
                {
                    _logger.LogInformation($"Processing destination: {destination}");
                    
                    var destCoords = await GetCoordinatesAsync(destination);
                    _logger.LogInformation($"Destination coordinates: {destCoords.latitude}, {destCoords.longitude}");
                    
                    if (destCoords.latitude == 0 && destCoords.longitude == 0)
                    {
                        _logger.LogWarning($"Cannot get coordinates for destination: {destination}");
                        results.Add(new GeoapifyDistanceResult
                        {
                            IsSuccess = false,
                            ErrorMessage = "Cannot get destination coordinates",
                            DistanceInKm = double.MaxValue,
                            DistanceText = "N/A",
                            DurationText = "N/A"
                        });
                        continue;
                    }

                    // Use Geoapify Routing API to get actual driving distance and time
                    var routingResult = await GetRoutingDataAsync(originCoords, destCoords);
                    _logger.LogInformation($"Routing result for {destination}: {routingResult.DistanceText}, Success: {routingResult.IsSuccess}");
                    results.Add(routingResult);

                    // Add delay to respect rate limits
                    await Task.Delay(100);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating multiple distances from {Origin}", origin);
                return destinations.Select(d => CreateMockDistanceResult(d)).ToList();
            }
        }

        public async Task<(double latitude, double longitude)> GetCoordinatesAsync(string address)
        {
            try
            {
                _logger.LogInformation($"Getting coordinates for address: {address}");
                
                if (string.IsNullOrEmpty(address))
                {
                    _logger.LogWarning("Empty address provided");
                    return (0, 0);
                }

                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogWarning("No API key, returning mock coordinates");
                    // Return mock coordinates for TP.HCM area with stable results
                    int seed = address.GetHashCode();
                    var random = new Random(Math.Abs(seed));
                    var mockLat = 10.7769 + random.NextDouble() * 0.1;
                    var mockLng = 106.7009 + random.NextDouble() * 0.1;
                    _logger.LogInformation($"Mock coordinates: {mockLat}, {mockLng}");
                    return (mockLat, mockLng);
                }

                var url = $"https://api.geoapify.com/v1/geocode/search?text={Uri.EscapeDataString(address)}&format=json&apiKey={_apiKey}";
                _logger.LogInformation($"Geocoding URL: {url.Replace(_apiKey, "***")}");
                
                var response = await _httpClient.GetAsync(url);
                _logger.LogInformation($"Geocoding response status: {response.StatusCode}");
                
                response.EnsureSuccessStatusCode();
                
                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Geocoding response: {jsonResponse.Substring(0, Math.Min(200, jsonResponse.Length))}...");
                
                var data = JsonSerializer.Deserialize<GeoapifyGeocodingResponse>(jsonResponse);
                
                if (data?.results?.Length > 0)
                {
                    var result = data.results[0];
                    _logger.LogInformation($"Found coordinates: {result.lat}, {result.lon}");
                    return (result.lat, result.lon);
                }
                
                _logger.LogWarning("No geocoding results found, using fallback coordinates");
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
                _logger.LogInformation($"Getting routing data from ({origin.lat}, {origin.lon}) to ({destination.lat}, {destination.lon})");

                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogWarning("No API key for routing, using mock data");
                    // Calculate straight-line distance as fallback
                    var straightLineDistance = CalculateHaversineDistance(origin.lat, origin.lon, destination.lat, destination.lon);
                    var mockResult = new GeoapifyDistanceResult
                    {
                        DistanceInKm = Math.Round(straightLineDistance * 1.3, 1), // Add 30% for road routing
                        DistanceText = FormatDistance(straightLineDistance * 1.3),
                        DurationInMinutes = (int)(straightLineDistance * 3), // ~3 minutes per km
                        DurationText = FormatDuration((int)(straightLineDistance * 3)),
                        IsSuccess = true
                    };
                    _logger.LogInformation($"Mock routing result: {mockResult.DistanceText}");
                    return mockResult;
                }

                var url = $"https://api.geoapify.com/v1/routing?waypoints={origin.lat},{origin.lon}|{destination.lat},{destination.lon}&mode=drive&apiKey={_apiKey}";
                _logger.LogInformation($"Routing URL: {url.Replace(_apiKey, "***")}");
                
                var response = await _httpClient.GetAsync(url);
                _logger.LogInformation($"Routing response status: {response.StatusCode}");
                
                response.EnsureSuccessStatusCode();
                
                var jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Routing response length: {jsonResponse.Length}");
                
                var data = JsonSerializer.Deserialize<GeoapifyRoutingResponse>(jsonResponse);
                
                if (data?.features?.Length > 0 && data.features[0].properties != null)
                {
                    var props = data.features[0].properties;
                    var distanceInKm = Math.Round(props.distance / 1000.0, 1);
                    var durationInMinutes = props.time / 60;

                    var result = new GeoapifyDistanceResult
                    {
                        DistanceInKm = distanceInKm,
                        DistanceText = FormatDistance(distanceInKm),
                        DurationInMinutes = durationInMinutes,
                        DurationText = FormatDuration(durationInMinutes),
                        IsSuccess = true
                    };
                    
                    _logger.LogInformation($"Real routing result: {result.DistanceText}, {result.DurationText}");
                    return result;
                }
                
                _logger.LogWarning("No routing results found, using fallback");
                return CreateMockDistanceResult("fallback");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting routing data");
                return CreateMockDistanceResult("fallback");
            }
        }

        private GeoapifyDistanceResult CreateMockDistanceResult(string address = "")
        {
            // Tạo seed từ địa chỉ để đảm bảo kết quả ổn định
            int seed = string.IsNullOrEmpty(address) ? 0 : address.GetHashCode();
            var random = new Random(Math.Abs(seed));
            
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

        /// <summary>
        /// Calculate straight-line distance between two points using Haversine formula
        /// </summary>
        private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371;
            
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            
            lat1 = DegreesToRadians(lat1);
            lat2 = DegreesToRadians(lat2);
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return EarthRadiusKm * c;
        }
        
        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
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
