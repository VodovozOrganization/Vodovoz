using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения продажи воды",
		Nominative = "доп. соглашение продажи воды")]
	[HistoryTrace]
	[EntityPermission]
	public class WaterSalesAgreementEntity : AdditionalAgreementEntity, IBusinessObject
	{
		public virtual IUnitOfWork UoW { set; get; }
		public virtual IList<NomenclatureFixedPriceEntity> FixedPrices
		{
			get
			{
				if(DeliveryPoint == null)
				{
					return Contract.Counterparty.NomenclatureFixedPrices;
				}

				return DeliveryPoint.NomenclatureFixedPrices;
			}
		}
	}
}
