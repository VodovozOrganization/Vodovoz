using GeoCoderApi.Client.Contracts;
using GeoCoderApi.Client.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GeoCoderApi.Client
{
	internal class GeoCoderApiClient : IGeoCoderApiClient
	{
		private const string _getAddressByCoordinatesEndpoint = "address";
		private const string _getCoordinatesAtAddressEndpoint = "coordinates";

		private readonly HttpClient _httpClient;

		public GeoCoderApiClient(HttpClient httpClient)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		}

		public async Task<AddressResponse> GetAddressByCoordinateAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default)
		{
			var parameters = new Dictionary<string, object>
			{
				["Latitude"] = latitude,
				["Longitude"] = longitude
			};

			HttpResponseMessage response = await _httpClient.GetAsync(_getAddressByCoordinatesEndpoint, parameters, cancellationToken);

			AddressResponse result = null;

			if(response.IsSuccessStatusCode)
			{
				result = JsonSerializer.Deserialize<AddressResponse>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return result;
			}

			try
			{
				var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				throw new Exception(problemDetails.Detail);
			}
			catch
			{
				throw;
			}

			throw new Exception("Error");
		}

		public async Task<GeographicPointResponse> GetCoordinateAtAddressAsync(string address, CancellationToken cancellationToken = default)
		{
			HttpResponseMessage response = await _httpClient.GetAsync(_getCoordinatesAtAddressEndpoint, "address", address, cancellationToken);

			GeographicPointResponse result = null;

			if(response.IsSuccessStatusCode)
			{
				result = JsonSerializer.Deserialize<GeographicPointResponse>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return result;
			}

			try
			{
				var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				throw new Exception(problemDetails.Detail);
			}
			catch
			{
				throw;
			}

			throw new Exception("Error");
		}
	}
}
