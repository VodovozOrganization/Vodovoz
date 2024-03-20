using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Mango.Core.Dto
{
	[JsonConverter(typeof(JsonStringEnumMemberConverter))]
	public enum MangoCallLocation
	{
		[Display(Name = "Голосовое меню")]
		[EnumMember(Value = "ivr")]
		Ivr,

		[Display(Name = "Очередь дозвона на группу")]
		[EnumMember(Value = "queue")]
		Queue,

		[Display(Name = "Сотрудник")]
		[EnumMember(Value = "abonent")]
		Abonent
	}
}
