using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Fias.Client.Cache;
using Fias.Search.DTO;
using NLog;
using Vodovoz.Domain.Geocoder;

namespace Fias.Client
{
	internal class FiasApiClient : IFiasApiClient
	{
		private static ILogger _logger = LogManager.GetCurrentClassLogger();

		private static HttpClient _client;
		private readonly GeocoderCache _geocoderCache;

		private class RequestSender<T>
		{
			private readonly string _requestParams;
			private readonly string _requestPath;

			public RequestSender(string requestPath, string requestParams = null)
			{
				_requestPath = requestPath ?? throw new ArgumentNullException(nameof(requestPath));
				_requestParams = requestParams != null ? $"?{requestParams}" : "";
			}

			public async Task<T> GetResponseAsync(CancellationToken? cancellationToken = null)
			{
				_client.DefaultRequestHeaders.Accept.Clear();
				_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				
				HttpResponseMessage response;

				try
				{
					if(cancellationToken != null)
					{
						response = _client.GetAsync($"{_requestPath}{_requestParams}", cancellationToken.Value).Result;
					}
					else
					{
						response = _client.GetAsync($"{ _requestPath }{ _requestParams }").Result;
					}
				}
				catch
				{
					return default;
				}

				if(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
				{
					var result = await response.Content.ReadFromJsonAsync<T>();
					return result;
				}

				return default;
			}
		}

		public FiasApiClient(string fiasApiBaseUrl, string fiasApiToken, GeocoderCache geocoderCache)
		{
			_client = new HttpClient()
			{
				BaseAddress = new Uri(fiasApiBaseUrl)
			};
			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", fiasApiToken);
			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_client.Timeout = TimeSpan.FromSeconds(5);
			_geocoderCache = geocoderCache ?? throw new ArgumentNullException(nameof(geocoderCache));
		}

		public IEnumerable<CityDTO> GetCitiesByCriteria(string searchString, int limit, bool isActive = true)
		{
			var inputParams = new Dictionary<string, string>
			{
				{ "searchString", searchString },
				{ "limit", limit.ToString() },
				{ "isActive", isActive.ToString() }
			};
			var requestParams = new FormUrlEncodedContent(inputParams).ReadAsStringAsync().Result;
			var requestSender = new RequestSender<IEnumerable<CityDTO>>("/api/GetCitiesByCriteria", requestParams);
			return requestSender.GetResponseAsync().Result ?? new List<CityDTO>();
		}

		public IEnumerable<StreetDTO> GetStreetsByCriteria(Guid cityGuid, string searchString, int limit, bool isActive = true)
		{
			var inputParams = new Dictionary<string, string>
			{
				{ "cityGuid", cityGuid.ToString() },
				{ "searchString", searchString },
				{ "limit", limit.ToString() },
				{ "isActive", isActive.ToString() }
			};
			var requestParams = new FormUrlEncodedContent(inputParams).ReadAsStringAsync().Result;
			var requestSender = new RequestSender<IEnumerable<StreetDTO>>("/api/GetStreetsByCriteria", requestParams);
			return requestSender.GetResponseAsync().Result ?? new List<StreetDTO>();
		}

		public IEnumerable<HouseDTO> GetHousesFromStreetByCriteria(Guid streetGuid, string searchString, int? limit = null, bool isActive = true)
		{
			var inputParams = new Dictionary<string, string>
			{
				{ "streetGuid", streetGuid.ToString() },
				{ "searchString", searchString },
				{ "limit", limit?.ToString() },
				{ "isActive", isActive.ToString() }
			};
			var requestParams = new FormUrlEncodedContent(inputParams).ReadAsStringAsync().Result;
			var requestSender = new RequestSender<IEnumerable<HouseDTO>>("/api/GetHousesFromStreetByCriteria", requestParams);
			return requestSender.GetResponseAsync().Result ?? new List<HouseDTO>();
		}

		public IEnumerable<HouseDTO> GetHousesFromCityByCriteria(Guid cityGuid, string searchString, int? limit = null, bool isActive = true)
		{
			var inputParams = new Dictionary<string, string>
			{
				{ "cityGuid", cityGuid.ToString() },
				{ "searchString", searchString },
				{ "limit", limit?.ToString() },
				{ "isActive", isActive.ToString() }
			};
			var requestParams = new FormUrlEncodedContent(inputParams).ReadAsStringAsync().Result;
			var requestSender = new RequestSender<IEnumerable<HouseDTO>>("/api/GetHousesFromCityByCriteria", requestParams);
			return requestSender.GetResponseAsync().Result ?? new List<HouseDTO>();
		}

		public async Task<PointDTO> GetCoordinatesByGeoCoder(string address, CancellationToken cancellationToken)
		{
			var cache = GetCachedCoordinates(address);
			if(cache != null)
			{
				var culture = CultureInfo.CreateSpecificCulture("ru-RU");
				culture.NumberFormat.NumberDecimalSeparator = ".";
				var result = new PointDTO
				{
					Latitude = cache.Latitude.ToString(culture),
					Longitude = cache.Longitude.ToString(culture)
				};

				return result;
			}

			var inputParams = new Dictionary<string, string>
			{
				{ "address", address }
			};
			var requestParams = new FormUrlEncodedContent(inputParams).ReadAsStringAsync().Result;
			var requestSender = new RequestSender<PointDTO>("/api/GetCoordinatesByGeoCoder", requestParams);
			_logger.Info($"Обращение к яндексу за координатами");
			var response = await requestSender.GetResponseAsync(cancellationToken);
			_logger.Info($"Координаты по адресу {address}: {response?.Latitude},{response?.Longitude}");
			if(response != null)
			{
				CacheAddress(address, response.Latitude, response.Longitude);
			}
			return response;
		}

		public async Task<string> GetAddressByGeoCoder(decimal latitude, decimal longitude, CancellationToken cancellationToken)
		{
			var cache = GetCachedAddress(latitude, longitude);
			if(cache != null)
			{
				return cache.Address;
			}

			var inputParams = new Dictionary<string, string>
			{
				{ "latitude", latitude.ToString(CultureInfo.InvariantCulture) },
				{ "longitude", longitude.ToString(CultureInfo.InvariantCulture) }
			};

			var requestParams = new FormUrlEncodedContent(inputParams).ReadAsStringAsync().Result;
			var requestSender = new RequestSender<string>("/api/GetAddressByGeoCoder", requestParams);
			_logger.Info($"Обращение к яндексу за адресом");
			var response = await requestSender.GetResponseAsync(cancellationToken);
			_logger.Info($"Адрес по координатам {latitude},{longitude}: {response}");
			if(!string.IsNullOrWhiteSpace(response))
			{
				CacheCoordinates(latitude, longitude, response);
			}
			return response;
		}

		private GeocoderCoordinatesCache GetCachedAddress(decimal latitude, decimal longitude)
		{
			try
			{
				var cache = _geocoderCache.GetAddress(latitude, longitude);
					return cache;
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "Ошибка при получении кэша адреса");
			}

			return null;
		}

		private GeocoderAddressCache GetCachedCoordinates(string address)
		{
			try
			{
				var cache = _geocoderCache.GetCoordinates(address);
				return cache;
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "Ошибка при получении кэша координат");
			}

			return null;
		}

		private void CacheAddress(string address, string latitude, string longitude)
		{
			try
			{
				decimal lat = decimal.Parse(latitude, CultureInfo.InvariantCulture);
				decimal lon = decimal.Parse(longitude, CultureInfo.InvariantCulture);
				_geocoderCache.CacheAddress(address, lat, lon);
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "Ошибка при сохранении адреса в кэш");
			}
		}

		private void CacheCoordinates(decimal latitude, decimal longitude, string address)
		{
			try
			{
				_geocoderCache.CacheCoordinates(latitude, longitude, address);
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "Ошибка при сохранении координат в кэш");
			}
		}
	}
}
