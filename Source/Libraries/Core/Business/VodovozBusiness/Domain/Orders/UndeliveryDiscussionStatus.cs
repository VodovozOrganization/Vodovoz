using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	[Appellative(
	Nominative = "Статус обсужденимя недовоза",
	NominativePlural = "Статусы бсужденимя недовоза")]

	public enum UndeliveryDiscussionStatus
	{
		[Display(Name = "В работе")]
		InProcess,
		[Display(Name = "На проверке")]
		Checking,
		[Display(Name = "Закрыт")]
		Closed
	}
}
