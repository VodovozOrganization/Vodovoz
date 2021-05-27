using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "автопремии для раскатных газелей",
		Nominative = "автопремия для раскатных газелей")]
	[EntityPermission]

	public class PremiumGazelRaskat : PremiumBase
	{
		DateTime routeListDate;

		[Display(Name = "Дата маршрутного листа")]
		public virtual DateTime RouteListDate
		{
			get { return routeListDate; }
			set { SetField(ref routeListDate, value); }
		}
	}
}
