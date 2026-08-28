using System;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories.Cash;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Interfaces.Sale;

namespace Vodovoz.Core.Application.Sale
{
	/// <summary>
	/// Получение ставки налога напрямую из БД
	/// </summary>
	public class VatRateFromDbProvider : IVatRateProvider
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IVatRateRepository _vatRateRepository;

		public VatRateFromDbProvider(
			IUnitOfWorkFactory uowFactory,
			IVatRateRepository vatRateRepository
		)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_vatRateRepository = vatRateRepository ?? throw new ArgumentNullException(nameof(vatRateRepository));
		}

		/// <inheritdoc/>
		public decimal? GetActualRate(IRecalculateTax saleItem)
		{
			var source = saleItem.RecalculateTaxSource;
			using var uow = _uowFactory.CreateWithoutRoot("Получение текущих ставок НДС");

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
}
