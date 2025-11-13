using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto
{
	/// <summary>
	/// Получатель письма <br/>
	/// Является словарём хранящим переменные для использования в тексте письма <br/>
	/// </summary>
	public class PackageEmailUser : Dictionary<string, string>
	{
		/// <summary>
		/// Имейл получателя <br/>
		/// </summary>
		[JsonIgnore()]
		public string EmailTo
		{
			get
			{
				if(!this.ContainsKey("emailto"))
				{
					return null;
				}
				return this["emailto"];
			}

			set
			{
				if(!this.ContainsKey("emailto"))
				{
					this.Add("emailto", value);
					return;
				}
				this["emailto"] = value;
			}
		}
	}
}
