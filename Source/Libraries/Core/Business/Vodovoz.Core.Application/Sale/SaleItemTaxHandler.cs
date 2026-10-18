using System;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Interfaces.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class SaleItemTaxHandler : ISaleItemTaxHandler
	{
		private readonly IVatRateProvider _vatRateProvider;

		public SaleItemTaxHandler(
			IVatRateProvider vatRateProvider
			)
		{
			_vatRateProvider = vatRateProvider ?? throw new ArgumentNullException(nameof(vatRateProvider));
		}
		
		/// <inheritdoc/>
		public virtual void CalculateTax(IRecalculateTax saleItem)
		{
			if(saleItem.RecalculateTaxSource is null || saleItem.Nomenclature is null)
			{
				return;
			}

			var vatRate = _vatRateProvider.GetActualRate(saleItem);
			
			if(vatRate is null)
			{
				throw new InvalidOperationException(
					$"У товара #{saleItem.Nomenclature.Id} отсутствует версия НДС на дату доставки #{saleItem.RecalculateTaxSource.DeliveryDate}");
			}
			
			saleItem.ValueAddedTax = vatRate;
			RecalculateTaxSum(saleItem);
		}
		
		/// <inheritdoc/>
		public virtual void RecalculateTaxSum(IRecalculateTax saleItem)
		{
			if(saleItem.ValueAddedTax is null or 0)
			{
				saleItem.IncludeNDS = 0;
				return;
			}
			
			saleItem.IncludeNDS = Math.Round(saleItem.ActualSum * saleItem.ValueAddedTax.Value / (1 + saleItem.ValueAddedTax.Value), 2);
		}
	}
}
