using Mango.Client.DTO.Common;
using Mango.Client.DTO.ForwardCall;
using Mango.Client.DTO.Group;
using Mango.Client.DTO.HangUp;
using Mango.Client.DTO.MakeCall;
using Mango.Client.DTO.User;
using Mango.Core.Sign;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using xNetStandard;

namespace Mango.Client
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
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly ISignGenerator _signGenerator;
		private string baseURL = "https://app.mango-office.ru/";
		private string vpbx_api_key;
		private string sign;
		private string vpbx_api_salt;
		private JsonSerializerSettings settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

		/// <param name="vpbx_api_key">Уникальный код вашей АТС.</param>
		/// <param name="vpbx_api_salt">Ключ для создания подписи.</param>
		public MangoController(string vpbx_api_key, string vpbx_api_salt)
		{
			_signGenerator = new SignGenerator();
			this.vpbx_api_key = vpbx_api_key;
			this.vpbx_api_salt = vpbx_api_salt;
		}

		private string PerformCommand(string command, string json = "")
		{
			sign = _signGenerator.GetSign(vpbx_api_key, vpbx_api_salt, json);
			string result = string.Empty;
			using(var request = new HttpRequest(baseURL))
			{
				request.AddField("vpbx_api_key", vpbx_api_key, Encoding.UTF8);
				request.AddField("sign", sign, Encoding.UTF8);

				if(string.IsNullOrWhiteSpace(json))
					request.AddField("json", "{}", Encoding.UTF8);
				else
					request.AddField("json", json, Encoding.UTF8);

				result = request.Post(command).ToString();
			}
			logger.Debug($"Ответ команды:{result}");
			return result;
		}

		private CommandResult GetParseResult(string json)
		{
			JObject obj = JObject.Parse(json);
			return obj.ToObject<CommandResult>();
		}

		private bool GetSimpleResult(string json)
		{
			JObject obj = JObject.Parse(json);
			CommandResult commandResult = obj.ToObject<CommandResult>();
			return commandResult.result == "1000";
		}

		/// <summary>
		/// Возвращает всех ВАТС сотрудников
		/// </summary>
		/// <returns>ClientMangoService.DTO.Users.User type</returns>
		public IEnumerable<User> GetAllVPBXUsers()
		{
			logger.Info("Запрашиваем пользователей манго");
			string command = "vpbx/config/users/request";
			string response = PerformCommand(command);

			JObject obj = JObject.Parse(response);
			List<JToken> tokens = obj["users"].Children().ToList();

			IList<User> users = new List<User>();
			foreach(JToken _result in tokens)
			{
				User user = _result.ToObject<User>();
				users.Add(user);
			}
			logger.Info($"Получено {users.Count} пользователей");
			return users;
		}
		/// <summary>
		/// Возвращает все группы в ВАТС 
		/// </summary>
		/// <returns>ClientMangoService.DTO.Group.Group type</returns>
		public IEnumerable<Group> GetAllVpbxGroups()
		{
			logger.Info("Запрашиваем группы манго");
			string command = "vpbx/groups";
			string response = PerformCommand(command);

			JObject obj = JObject.Parse(response);
			List<JToken> tokens = obj["groups"].Children().ToList();

			IList<Group> groups = new List<Group>();
			foreach(JToken _result in tokens)
			{
				Group group = _result.ToObject<Group>();
				groups.Add(group);
			}
			logger.Info($"Получено {groups.Count} групп");
			return groups;
		}

		/// <summary>
		/// Вызывает команду "звонок от сотрудника ВАТС" другому сотруднику ВАТС
		/// </summary>
		/// <returns><c>true</c>, если успешно, <c>false</c> неудачно.</returns>
		/// <param name="from_extension">Внутренний номер сотрудника ВАТС , который звонит</param>
		/// <param name="to_extension">Внутренний номер сотрудника ВАТС , которому звонят</param>
		/// <param name="commandId">Не обязательный параметр , обозначающий id комнды (может быть любым , по умолчанию : имя метода)</param>
		public bool MakeCall(string from_extension, string to_extension, [CallerMemberName] string commandId = "")
		{
			logger.Info($"Выполняем звонок на {to_extension}");
			string command = @"vpbx/commands/callback";
			MakeCallRequest options = new MakeCallRequest();
			options.command_id = commandId;
			options.from = new From();
			options.from.extension = from_extension;
			options.to_number = to_extension;
			string json = JsonConvert.SerializeObject(options, settings);
			var result = PerformCommand(command, json);
			var successfully = GetSimpleResult(result);
			logger.Info(successfully ? "Ок" : $"Код ошибки: {GetParseResult(result).result}");
			return successfully;
		}

		/// <summary>
		/// Вызывает команду переадресации, возможно два режима
		/// </summary>
		/// <returns><c>true</c>, если успешно, <c>false</c> неудачно.</returns>
		/// <param name="call_id">Идентетификатор текущего вызова.</param>
		/// <param name="from_extension">Внутренний номер сотрудника ВАТС , который звонит</param>
		/// <param name="to_extension">Внутренний номер сотрудника ВАТС , которому звонят</param>
		/// <param name="method">Метод переадресации , "blind" - слепая перевод , "hold" - консультативный перевод.</param>
		/// <param name="commandId">Не обязательный параметр , обозначающий id комнды (может быть любым , по умолчанию : имя метода)</param>
		public bool ForwardCall(string call_id, string from_extension, string to_extension, ForwardingMethod method, [CallerMemberName] string commandId = "")
		{
			logger.Info($"Выполняем переадресацию на {to_extension}");
			string command = @"vpbx/commands/transfer";
			ForwardCallRequest options = new ForwardCallRequest();
			options.call_id = call_id;
			options.command_id = commandId;
			options.method = method.ToString();
			options.to_number = to_extension;
			options.initiator = from_extension;
			string json = JsonConvert.SerializeObject(options, settings);

			var result = PerformCommand(command, json);
			var successfully = GetSimpleResult(result);
			logger.Info(successfully ? "Ок" : $"Код ошибки: {GetParseResult(result).result}");
			return successfully;
		}

		/// <summary>
		/// Вызывает команду "сбросить вызов"
		/// </summary>
		/// <returns><c>true</c>, если успешно, <c>false</c> неудачно.</returns>
		/// <param name="call_id">Идентетификатор текущего вызова.</param>
		/// <param name="commandId">Не обязательный параметр , обозначающий id комнды (может быть любым , по умолчанию : имя метода)</param>
		public bool HangUp(string call_id, [CallerMemberName] string commandId = "")
		{
			logger.Info($"Выполняем завершение разговора.");
			string command = "vpbx/commands/call/hangup";

			HangUpRequest options = new HangUpRequest();
			options.command_id = commandId;
			options.call_id = call_id;
			string json = JsonConvert.SerializeObject(options, settings);

			var result = PerformCommand(command, json);
			var successfully = GetSimpleResult(result);
			logger.Info(successfully ? "Ок" : $"Код ошибки: {GetParseResult(result).result}");
			return successfully;
		}
	}
}
