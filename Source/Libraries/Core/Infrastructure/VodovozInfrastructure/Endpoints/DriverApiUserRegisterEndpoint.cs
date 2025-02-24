using ApiClientProvider;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace VodovozInfrastructure.Endpoints
{
	public class DriverApiUserRegisterEndpoint
	{
		private IApiClientProvider _apiHelper;
		private const string _createUserEndpoint = "Register";
		private const string _addRoleEndpoint = "AddRoleToUser";
		private const string _removeRoleEndpoint = "RemoveRoleFromUser";
		private const int _minPasswordLength = 3;

		public DriverApiUserRegisterEndpoint(IApiClientProvider apiHelper)
		{
			_apiHelper = apiHelper;
		}

		public async Task RegisterUser(string username, string password, string userRole)
		{
			Validate(username, password);
			var userData = new UserData { Username = username, Password = password, UserRole = userRole };
			await Send(userData, _createUserEndpoint);
		}
		
		public async Task AddRoleToUser(string username, string password, string userRole)
		{
			Validate(username, password);
			var userData = new UserData { Username = username, Password = password, UserRole = userRole };
			await Send(userData, _addRoleEndpoint);
		}
		
		public async Task RemoveRoleFromUser(string username, string password, string userRole)
		{
			Validate(username, password);
			var userData = new UserData { Username = username, Password = password, UserRole = userRole };
			await Send(userData, _removeRoleEndpoint);
		}

		private static void Validate(string username, string password)
		{
			if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			{
				throw new ArgumentException("Имя пользователь и пароль не могут быть пустыми");
			}

			if(password.Length < _minPasswordLength)
			{
				throw new ArgumentException("Пароль не может быть короче 3х символов");
			}
		}

		private async Task Send(UserData payload, string endpoint)
		{
			using(var response = await _apiHelper.Client.PostAsJsonAsync(endpoint, payload))
			{
				if(!response.IsSuccessStatusCode)
				{
					ErrorMessage error = null;

					try
					{
						error = await response.Content.ReadFromJsonAsync<ErrorMessage>();
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
}
