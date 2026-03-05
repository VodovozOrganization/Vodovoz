using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.Services
{
	/// <param name="Source"></param>
	/// <param name="ExternalOrderId"></param>
	/// <param name="TransactionId"></param>
	/// <param name="Signature"></param>
	public record CancelOrderDto(
		[property: Required] Source Source,
		[property: Required] Guid ExternalOrderId,
		[property: StringLength(100)] string TransactionId,
		string Signature);
}
