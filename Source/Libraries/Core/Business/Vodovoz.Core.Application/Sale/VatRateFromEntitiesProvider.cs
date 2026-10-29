using System;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Application.Sale
{
	/// <summary>
	/// Получение ставки налога из сущностей
	/// </summary>
	public class VatRateFromEntitiesProvider : IVatRateProvider
	{
		/// <inheritdoc/>
		public decimal? GetActualRate(IRecalculateTax saleItem)
		{
			if(saleItem.Nomenclature is not NomenclatureEntity nomenclature)
			{
				throw new InvalidOperationException("Не получилось преобразовать IDepositNomenclature в NomenclatureEntity");
			}
			
			var organization = saleItem.RecalculateTaxSource.Organization as OrganizationEntity;
			
			return nomenclature
				.GetEffectiveVatRateVersion(organization, saleItem.RecalculateTaxSource.DeliveryDate)?
				.VatRate
				.VatNumericValue;
		}
	}
}
