using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum OrderSignatureType
	{
		[Display(Name = "По печати")]
		BySeal,
		[Display(Name = "По доверенности")]
		ByProxy,
		[Display(Name = "Доверенность на адресе")]
		ProxyOnDeliveryPoint,
		[Display(Name = "Подпись/расшифровка")]
		SignatureTranscript
	}
}
