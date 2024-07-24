using System;

namespace Vodovoz.Domain
{
	public interface INotification
	{
		int? HttpCode { get; set; }
		DateTime? SentDate { get; set; }
	}
}
