using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto
{
	public class NewDeliveryPointInfoDto : DeliveryPointInfoDto
	{
		[Display(Name = "Клиент")]
		public int CounterpartyErpId { get; set; }
		public Source Source { get; set; }
	}
}
