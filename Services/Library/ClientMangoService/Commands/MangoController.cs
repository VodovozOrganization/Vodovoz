using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Mango;
using ClientMangoService.DTO.Users;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ClientMangoService.DTO.HangUp;
using ClientMangoService.DTO.ForwardCall;
using ClientMangoService.DTO;
using ClientMangoService.DTO.MakeCall;

namespace ClientMangoService.Commands
{
	public enum ForwardingMethod
	{
		hold,
		blind
	}

	/// <summary>
	/// Класс , позволяющий отправлять коаманды в Mango
	/// </summary>
	public class MangoController
	{
		private string baseURL = "https://app.mango-office.ru/";
		private string vpbx_api_key;
		private string sign;
		private string json;
		private string vpbx_api_salt;

		/// <param name="vpbx_api_key">Уникальный код вашей АТС.</param>
		/// <param name="vpbx_api_salt">Ключ для создания подписи.</param>
		public MangoController(string vpbx_api_key, string vpbx_api_salt)
		{
			this.vpbx_api_key = vpbx_api_key;
			this.vpbx_api_salt = vpbx_api_salt;
		}
		private object PerformCommand(string command, string json = "")
		{
			this.json = json;
			sign = MangoSignHelper.GetSign(vpbx_api_key, json, vpbx_api_salt);
			HttpResponse response;
			string result = String.Empty;
			using(var request = new HttpRequest(baseURL)) {
				request.AddField("vpbx_api_key", vpbx_api_key, Encoding.ASCII);
				request.AddField("sign", sign, Encoding.ASCII);

				if(String.IsNullOrWhiteSpace(json))
					request.AddField("json", "{}", Encoding.ASCII);
				else
					request.AddField("json", json, Encoding.ASCII);

				response = request.Post(command);

				using(Stream stream = response.ToMemoryStream()) {

					using(StreamReader reader = new StreamReader(stream)) {

						JObject obj = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
						IList<JToken> results = obj["users"].Children().ToList();

						IList<User> users = new List<User>();
						foreach(JToken _result in results) {
							User user = _result.ToObject<User>();
							users.Add(user);
						}
						return users;
					}
				}

			}
		}

		/// <summary>
		/// Возвращает всех ВАТС сотрудников
		/// </summary>
		/// <returns>ClientMangoService.DTO.Users.User type</returns>
		public IEnumerable<User> GetAllVPBXEmploies()
		{
			string command = "vpbx/config/users/request";
			return PerformCommand(command) as List<User>;
		}

		/// <summary>
		/// Вызывает команду "звонок от сотрудника ВАТС" другому сотруднику ВАТС
		/// </summary>
		/// <returns><c>true</c>, если успешно, <c>false</c> неудачно.</returns>
		/// <param name="from_extension">Внутренний номер сотрудника ВАТС , который звонит</param>
		/// <param name="to_extension">Внутренний номер сотрудника ВАТС , которому звонят</param>
		/// <param name="commandId">Не обязательный параметр , обозначающий id комнды (может быть любым , по умолчанию : имя метода)</param>
		public bool MakeCall(string from_extension, string to_extension,[CallerMemberName]string commandId = "")
		{
			string command = @"vpbx/commands/callback";
			MakeCallRequest options = new MakeCallRequest();
			options.command_id = commandId;
			options.from = new From();
			options.from.extension = from_extension;
			options.to_number = to_extension;

			string json = JsonConvert.SerializeObject(options);
			bool result = Convert.ToBoolean((command, json));

			return result;
		}

		/// <summary>
		/// Вызывает команду переадресации , возможно два режима
		/// </summary>
		/// <returns><c>true</c>, если успешно, <c>false</c> неудачно.</returns>
		/// <param name="call_id">Идентетификатор текущего вызова.</param>
		/// <param name="from_extension">Внутренний номер сотрудника ВАТС , который звонит</param>
		/// <param name="to_extension">Внутренний номер сотрудника ВАТС , которому звонят</param>
		/// <param name="method">Метод переадресации , "blind" - слепая перевод , "hold" - консультативный перевод.</param>
		/// <param name="commandId">Не обязательный параметр , обозначающий id комнды (может быть любым , по умолчанию : имя метода)</param>
		public bool ForwardCall(string call_id ,string from_extension,string to_extension,ForwardingMethod method, [CallerMemberName]string commandId= "")
		{
			string command = @"vpbx/commands/transfer";				
			ForwardCallRequest options = new ForwardCallRequest();
			options.call_id = call_id;
			options.command_id = commandId;

			if(method == ForwardingMethod.blind)
				options.method = "blind";
			else
				options.method = "hold";

			options.to_number = to_extension;
			options.initiator = from_extension;

			string json = JsonConvert.SerializeObject(options);
			bool result = Convert.ToBoolean((command, json));

			return result;
		}

		/// <summary>
		/// Вызывает команду "сбросить вызов"
		/// </summary>
		/// <returns><c>true</c>, если успешно, <c>false</c> неудачно.</returns>
		/// <param name="call_id">Идентетификатор текущего вызова.</param>
		/// <param name="commandId">Не обязательный параметр , обозначающий id комнды (может быть любым , по умолчанию : имя метода)</param>
		public bool HangUp(string call_id, [CallerMemberName]string commandId = "")
		{
			string command = "vpbx/result/call/hangup";

			HangUpRequest options = new HangUpRequest();
			options.command_id = "Hangup"+call_id;
			options.call_id = call_id;

			string json = JsonConvert.SerializeObject(options);
			bool result = Convert.ToBoolean((command, json));

			return result;
		}
	}
}