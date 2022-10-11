using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ApiHelper
{
	public class ApiClientProvider : IApiClientProvider
	{
		protected HttpClient _сlient;
		private readonly string _apiBaseParameter = "BaseUri";

		public ApiClientProvider(IConfigurationSection apiConfiguration)
		{
			InitializeClient(apiConfiguration ?? throw new ArgumentNullException(nameof(apiConfiguration)));
		}

		public HttpClient Client
		{
			get
			{
				return _сlient;
			}
		}

		protected virtual void InitializeClient(IConfigurationSection apiConfiguration)
		{
			_сlient = new HttpClient();
			_сlient.BaseAddress = new Uri(apiConfiguration.GetValue<string>(_apiBaseParameter));
			_сlient.DefaultRequestHeaders.Accept.Clear();
			_сlient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}
	}
}
