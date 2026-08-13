using System;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories.Cash;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Application.Sale
{
	public interface IVatRateProvider
	{
		decimal? GetActualRate(IUnitOfWork uow, IRecalculateTax saleItem);
	}

	/// <summary>
	/// Получение ставки налога напрямую из БД
	/// </summary>
	public class VatRateFromDbProvider : IVatRateProvider
	{
		private readonly IVatRateRepository _vatRateRepository;

		public VatRateFromDbProvider(
			IVatRateRepository vatRateRepository
		)
		{
			_vatRateRepository = vatRateRepository ?? throw new ArgumentNullException(nameof(vatRateRepository));
		}

		public decimal? GetActualRate(IUnitOfWork uow, IRecalculateTax saleItem)
		{
			var source = saleItem.RecalculateTaxSource;

			if(source is { Organization: { IsUsnMode: true } } && !saleItem.Nomenclature.IsDeposit)
			{
				return _vatRateRepository
					.GetActualVatRateFromOrganization(uow, source.Organization.Id, source.DeliveryDate)?
					.VatNumericValue;
			}

			return _vatRateRepository
				.GetActualVatRateFromNomenclature(uow, saleItem.Nomenclature.Id, source.DeliveryDate)?
				.VatNumericValue;
		}
	}
	
	/// <summary>
	/// Получение ставки налога из сущностей
	/// </summary>
	public class VatRateFromEntitiesProvider : IVatRateProvider
	{
		public decimal? GetActualRate(IUnitOfWork uow, IRecalculateTax saleItem)
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
	
	public abstract class SaleItemTaxHandler
	{
		private readonly IVatRateProvider _vatRateProvider;

		protected SaleItemTaxHandler(
			IVatRateProvider vatRateProvider
			)
		{
			_vatRateProvider = vatRateProvider ?? throw new ArgumentNullException(nameof(vatRateProvider));
		}
		
		public virtual void CalculateTax(IUnitOfWork uow, IRecalculateTax saleItem)
		{
			if(!NHibernateUtil.IsInitialized(saleItem.Nomenclature))
			{
				NHibernateUtil.Initialize(saleItem.Nomenclature);
			}

			TryInitializeSource(saleItem);

			if(saleItem.RecalculateTaxSource is null || saleItem.Nomenclature is null)
			{
				return;
			}

			var vatRate = _vatRateProvider.GetActualRate(uow, saleItem);
			
			if(vatRate is null)
			{
				throw new InvalidOperationException(
					$"У товара #{saleItem.Nomenclature.Id} отсутствует версия НДС на дату доставки #{saleItem.RecalculateTaxSource.DeliveryDate}");
			}
			
			saleItem.ValueAddedTax = vatRate;
			RecalculateTaxSum(saleItem);
		}
		
		public virtual void RecalculateTaxSum(IRecalculateTax saleItem)
		{
			if(saleItem.ValueAddedTax is null or 0)
			{
				saleItem.IncludeNDS = 0;
				return;
			}
			
			saleItem.IncludeNDS = Math.Round(saleItem.ActualSum * saleItem.ValueAddedTax.Value / (1 + saleItem.ValueAddedTax.Value), 2);
		}

		/// <summary>
		/// Пересчитываем налоги
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Продаваемая позиция</param>
		public virtual void RecalculateTax(IUnitOfWork uow, IRecalculateTax saleItem)
		{
			if(!NeedRecalculateTax(saleItem))
			{
				return;
			}

			if(!NeedUseTax(uow, saleItem))
			{
				saleItem.IncludeNDS = null;
				return;
			}

			if(NeedUseTax(uow, saleItem) && saleItem.ValueAddedTax.HasValue)
			{
				saleItem.IncludeNDS = Math.Round(saleItem.ActualSum * saleItem.ValueAddedTax.Value / (1 + saleItem.ValueAddedTax.Value), 2);
			}
		}

		protected abstract bool NeedRecalculateTax(IRecalculateTax taxesItem);
		protected abstract void TryInitializeSource(IRecalculateTax taxesItem);
		
		protected void TryInitializeSource(object saleItemSource)
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

		
	}
}
