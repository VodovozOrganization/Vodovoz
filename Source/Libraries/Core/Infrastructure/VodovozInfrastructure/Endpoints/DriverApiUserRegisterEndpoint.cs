using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ApiClientProvider;

namespace VodovozInfrastructure.Endpoints
{
	public class DriverApiUserRegisterEndpoint
	{
		private IApiClientProvider _apiHelper;
		private readonly string _sendEndpointPath = "Register";
		private const int _minPasswordLength = 3;

		public DriverApiUserRegisterEndpoint(IApiClientProvider apiHelper)
		{
			_apiHelper = apiHelper;
		}

		public async Task Register(string username, string password)
		{
			if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			{
				throw new ArgumentException("Имя пользователь и пароль не могут быть пустыми");
			}

			if(password.Length < _minPasswordLength)
			{
				throw new ArgumentException("Пароль не может быть короче 3х символов");
			}

			var payload = new RegisterPayload { Username = username, Password = password };
			
			using(HttpResponseMessage response = await _apiHelper.Client.PostAsJsonAsync(_sendEndpointPath, payload))
			{
				if(!response.IsSuccessStatusCode)
				{
					ErrorMessage error = null;

					try
					{
						error = await response.Content.ReadAsAsync<ErrorMessage>();
					}
					catch {}

					if(error != null)
					{
						throw new Exception(error.Error);
					}

					throw new Exception(response.ReasonPhrase);
				}
			}
		}
	}

	internal class ErrorMessage
	{
		[JsonPropertyName("error")]
		public string Error { get; set; }
	}
}
