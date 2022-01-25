using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ApiClientProvider
{
	public class ApiBasicAuthClientProvider : ApiClientProvider
	{
		private readonly string _usernameParameter = "Username";
		private readonly string _passwordParameter = "Password";

		private ApiBasicAuthClientProvider(IConfigurationSection apiConfiguration, ICustomHttpClientFactory customHttpClientFactory) : base(apiConfiguration, customHttpClientFactory)
		{
		}

		public ApiBasicAuthClientProvider(IConfigurationSection apiConfiguration, HttpClient servicetHttpClient) : base(apiConfiguration, servicetHttpClient)
		{
		}

		protected override void InitializeClient(IConfigurationSection apiConfiguration)
		{
			base.InitializeClient(apiConfiguration);

			var headerValue = Convert.ToBase64String(
				Encoding.UTF8.GetBytes($"{ apiConfiguration.GetValue<string>(_usernameParameter) }:{ apiConfiguration.GetValue<string>(_passwordParameter) }"));

			Client.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Basic", headerValue);
		}
	}
}
