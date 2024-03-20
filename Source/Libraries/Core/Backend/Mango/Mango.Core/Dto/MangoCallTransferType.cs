using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Mango.Core.Dto
{
	[JsonConverter(typeof(JsonStringEnumMemberConverter))]
	public enum MangoCallTransferType
	{
		[Display(Name = "Консультативный")]
		[EnumMember(Value = "consultative")]
		Consultative,

		[Display(Name = "Слепой")]
		[EnumMember(Value = "blind")]
		Blind,

		[Display(Name = "Возврат слепого перевода")]
		[EnumMember(Value = "return_blind")]
		ReturnBlind
	}
}
