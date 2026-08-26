using System;
using Vodovoz.Core.Domain.Interfaces;

namespace Vodovoz.Core.Application.Sale
{
	public interface IVatRateProvider
	{
		decimal? GetActualRate(IRecalculateTax saleItem);
	}

	public interface ISaleItemTaxHandler
	{
		/// <summary>
		/// Расчет налогов для позиции
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		void CalculateTax(IRecalculateTax saleItem);

		/// <summary>
		/// Пересчитываем налоги
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		void RecalculateTaxSum(IRecalculateTax saleItem);
	}

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
			//TODO-5967 погонять без методов инициализации хибернэйта

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

		//protected abstract bool NeedRecalculateTax(IRecalculateTax taxesItem);
		//protected abstract void TryInitializeSource(IRecalculateTax taxesItem);
		
		/*protected void TryInitializeSource(object saleItemSource)
		{
			if(!NHibernateUtil.IsInitialized(saleItemSource))
			{
				NHibernateUtil.Initialize(saleItemSource);
			}
		}
		
		private bool NeedUseTax(IUnitOfWork uow, IRecalculateTax saleItem)
		{
			TryInitializeSource(saleItem);

			bool canUseVAT = true;

			if(saleItem.RecalculateTaxSource != null)
			{
				canUseVAT = _vatRateProvider.GetActualRate(uow, saleItem) != 0m;
			}

			return canUseVAT;
		}
		*/
	}
}
