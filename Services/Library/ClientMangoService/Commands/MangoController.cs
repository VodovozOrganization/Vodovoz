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
	public class MangoController
	{
		private string baseURL = "https://app.mango-office.ru/";
		private string vpbx_api_key;
		private string sign;
		private string json;
		private string vpbx_api_salt;

		public MangoController(string vpbx_api_key, string vpbx_api_salt)
		{
			this.vpbx_api_key = vpbx_api_key;
			this.vpbx_api_salt = vpbx_api_salt;
		}

		public IEnumerable<User> GetAllVPBXEmploies()
		{
			string command = "vpbx/config/users/request";

			json = "";
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

		public bool MakeCall(string from_extension, string to_extension,[CallerMemberName]string commandId = "")
		{
			string command = @"vpbx/commands/callback";
			MakeCallRequest options = new MakeCallRequest();
			options.command_id = commandId;
			options.from = new From();
			options.from.extension = from_extension;
			options.to_number = to_extension;

			json = JsonConvert.SerializeObject(options);
			sign = MangoSignHelper.GetSign(vpbx_api_key, json, vpbx_api_salt);

			HttpResponse response = null;
			using(var request = new HttpRequest(baseURL)) {
				request.AddField("vpbx_api_key", vpbx_api_key, Encoding.ASCII);
				request.AddField("sign", sign, Encoding.ASCII);

				if(String.IsNullOrWhiteSpace(json))
					request.AddField("json", "{}", Encoding.ASCII);
				else
					request.AddField("json", json, Encoding.ASCII);

				response = request.Post(command);

				CommandResult result = null;
				using(Stream stream = response.ToMemoryStream())
				using(StreamReader reader = new StreamReader(stream)) {
					JObject obj = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
					result = obj.ToObject<CommandResult>();
					if(result.result == "1000")
						return true;
					else
						return false;
				}
			}
		}

		public bool ForwardCall(string call_id ,string from_extension,string to_extension,string method, [CallerMemberName]string commandId= "")
		{
			string command = @"vpbx/commands/transfer";

			ForwardCallRequest options = new ForwardCallRequest();
			options.call_id = call_id;
			options.command_id = commandId;
			options.method = method;
			options.to_number = to_extension;
			options.initiator = from_extension;

			json = JsonConvert.SerializeObject(options);
			sign = MangoSignHelper.GetSign(vpbx_api_key, json, vpbx_api_salt);
			HttpResponse response;

			using(var request = new HttpRequest(baseURL)) {
				request.AddField("vpbx_api_key", vpbx_api_key, Encoding.ASCII);
				request.AddField("sign", sign, Encoding.ASCII);

				if(String.IsNullOrWhiteSpace(json))
					request.AddField("json", "{}", Encoding.ASCII);
				else
					request.AddField("json", json, Encoding.ASCII);

				response = request.Post(command);
				CommandResult result = null;
				using(Stream stream = response.ToMemoryStream())
				using(StreamReader reader = new StreamReader(stream)) {
					JObject obj = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
					result = obj.ToObject<CommandResult>();
					if(result.result == "1000")
						return true;
					else
						return false;
				}
			}

		}

		public bool HangUp(string call_id, [CallerMemberName]string commandId = "")
		{
			string command = "vpbx/result/call/hangup";

			HangUpRequest options = new HangUpRequest();
			options.command_id = "Hangup"+call_id;
			options.call_id = call_id;

			json = JsonConvert.SerializeObject(options);
			sign = MangoSignHelper.GetSign(vpbx_api_key, json, vpbx_api_salt);
			HttpResponse response;

			//Add just parameters 
			using(var request = new HttpRequest(baseURL)) {
				request.AddField("vpbx_api_key", vpbx_api_key, Encoding.ASCII);
				request.AddField("sign", sign, Encoding.ASCII);

				if(String.IsNullOrWhiteSpace(json))
					request.AddField("json", "{}", Encoding.ASCII);
				else
					request.AddField("json", json, Encoding.ASCII);

				response = request.Post(command);

				CommandResult result = null;
				using(Stream stream = response.ToMemoryStream())
				using(StreamReader reader = new StreamReader(stream)) {
					JObject obj = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
					result = obj.ToObject<CommandResult>();
					if(result.result == "1000")
						return true;
					else
						return false;
				}
			}

		}



	}
}