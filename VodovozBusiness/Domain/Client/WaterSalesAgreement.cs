using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Repositories;

namespace Vodovoz.Domain.Client
{

	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения продажи воды",
		Nominative = "доп. соглашение продажи воды")]
	[HistoryTrace]
	[EntityPermission]
	public class WaterSalesAgreement : AdditionalAgreement, IBusinessObject
	{
		public virtual IUnitOfWorkGeneric<WaterSalesAgreement> UoWGeneric { set; get; }
		public virtual IUnitOfWork UoW { set; get; }

		public virtual IList<NomenclatureFixedPrice> FixedPrices {
			get {
				if (DeliveryPoint == null) {
					return Contract.Counterparty.NomenclatureFixedPrices;
				}

				return DeliveryPoint.NomenclatureFixedPrices;
			}
		}
	}

}
