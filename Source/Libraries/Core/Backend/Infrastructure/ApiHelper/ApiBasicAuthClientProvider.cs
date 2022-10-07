using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http.Headers;
using System.Text;

namespace ApiHelper
{
	public class ApiBasicAuthClientProvider : ApiClientProvider
	{
		private readonly string _usernameParameter = "Username";
		private readonly string _passwordParameter = "Password";

		public ApiBasicAuthClientProvider(IConfigurationSection apiConfiguration) : base(apiConfiguration)
		{
		}

		protected override void InitializeClient(IConfigurationSection apiConfiguration)
		{
			base.InitializeClient(apiConfiguration);

			var headerValue = Convert.ToBase64String(
				Encoding.UTF8.GetBytes($"{ apiConfiguration.GetValue<string>(_usernameParameter) }:{ apiConfiguration.GetValue<string>(_passwordParameter) }"));

			_сlient.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Basic", headerValue);
		}
	}
}
