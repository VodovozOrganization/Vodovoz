using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Mango.Core.Dto
{
	[JsonConverter(typeof(JsonStringEnumMemberConverter))]
	public enum MangoCallState
	{
		[Display(Name = "Дозвон")]
		[EnumMember(Value = "Appeared")]
		Appeared,

		[Display(Name = "Подключен")]
		[EnumMember(Value = "Connected")]
		Connected,

		[Display(Name = "На удержании")]
		[EnumMember(Value = "OnHold")]
		OnHold,

		[Display(Name = "Отключен")]
		[EnumMember(Value = "Disconnected")]
		Disconnected
	}
}
