using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Fias.Search.DTO;

namespace Fias.Service
{
	public class FiasApiClient : IFiasApiClient
	{
		private static HttpClient _client;

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
						response = await _client.GetAsync($"{_requestPath}{_requestParams}", cancellationToken.Value);
					}
					else
					{
						response = await _client.GetAsync($"{ _requestPath }{ _requestParams }");
					}
				}
				catch
				{
					return default;
				}

				if(response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
				{
					var result = response.Content.ReadFromJsonAsync<T>().Result;
					return result;
				}

				return default;
			}
		}

		public FiasApiClient(string fiasApiBaseUrl, string fiasApiToken)
		{
			_client = new HttpClient()
			{
				BaseAddress = new Uri(fiasApiBaseUrl)
			};
			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", fiasApiToken);
			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_client.Timeout = TimeSpan.FromSeconds(5);
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

		public Task<PointDTO> GetCoordinatesByGeoCoderAsync(string address, CancellationToken cancellationToken)
		{
			var inputParams = new Dictionary<string, string>
			{
				{ "address", address }
			};
			var requestParams = new FormUrlEncodedContent(inputParams).ReadAsStringAsync().Result;
			var requestSender = new RequestSender<PointDTO>("/api/GetCoordinatesByGeoCoder", requestParams);
			return requestSender.GetResponseAsync(cancellationToken);
		}

		public Task<string> GetAddressByGeoCoder(decimal latitude, decimal longitude, CancellationToken cancellationToken)
		{
			var inputParams = new Dictionary<string, string>
			{
				{ "latitude", latitude.ToString(CultureInfo.InvariantCulture) },
				{ "longitude", longitude.ToString(CultureInfo.InvariantCulture) }
			};
			
			var requestParams = new FormUrlEncodedContent(inputParams).ReadAsStringAsync().Result;
			var requestSender = new RequestSender<string>("/api/GetAddressByGeoCoder", requestParams);
			return requestSender.GetResponseAsync(cancellationToken);
		}
	}
}
