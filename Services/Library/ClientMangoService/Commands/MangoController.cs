using System;
using System.Security.Cryptography;
using System.Text;
using xNet;
using Newtonsoft.Json;
using Mango;

namespace ClientMangoService.Commands
{
	public class MangoController
	{
		private string baseURL = "https://app.mango-office.ru";
		private string vpbx_api_key;
		private string sign;
		private string json;
		private string vpbx_api_salt;

		public MangoController(string vpbx_api_key,string vpbx_api_salt)
		{
			this.vpbx_api_key = vpbx_api_key;
			this.vpbx_api_salt = vpbx_api_salt;
		}

		public void GetAllVPBXEmploies()
		{
			string command = "/vpbx/config/users/request";

			json = "";
			sign = MangoSignHelper.GetSign(vpbx_api_key, json, vpbx_api_salt);

			HttpResponse response;
			using(var request = new HttpRequest(baseURL)) {
				request.AddField("vpbx_api_key", vpbx_api_key, Encoding.ASCII);
				request.AddField("sign", sign, Encoding.ASCII);
				request.AddField("json", json, Encoding.ASCII);

				response = request.Post(command);
			}
			Console.WriteLine(response);
		}

		public void HangUp(string call_id)
		{
			string command = "/vpbx/result/call/hangup";

			CommandOptions options = new CommandOptions();
			options.command_id = "hang.up.command-" + call_id;
			options.call_id = call_id;

			json = JsonConvert.SerializeObject(options);
			sign = MangoSignHelper.GetSign(vpbx_api_key, json, vpbx_api_salt);

			//Add just parameters 
			using(var request = new HttpRequest(baseURL)) {

			}
		}
	}
	public class CommandOptions
	{
		#region SubClasses
		public class From
		{
			public string extension;
			public string number;	
		}
		#endregion
		public string command_id;
		public string call_id;

		public From from;
		public string str_from;

		//public To to;
		public string to_number;
		public string line_number;
	}
}


//она этот запрос конвертанёт в POST...
//{
	//"startDate" : "10/28/2019",
	//"multipart/form-data" : 
