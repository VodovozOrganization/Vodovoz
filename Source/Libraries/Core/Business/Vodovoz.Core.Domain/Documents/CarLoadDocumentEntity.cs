using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vodovoz.Core.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "талоны погрузки автомобилей",
		Nominative = "талон погрузки автомобиля")]
	[EntityPermission]
	[HistoryTrace]
	public class CarLoadDocumentEntity
	{
		public virtual int Id { get; set; }
	}
}
