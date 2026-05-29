using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Factories;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OrderProductHandler : ProductHandler
	{
		private readonly IOrderContractUpdater _contractUpdater;

		public OrderProductHandler(
			IOrderContractUpdater contractUpdater,
			ISaleItemFactory saleItemFactory,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureService nomenclatureService,
			INomenclatureRepository nomenclatureRepository,
			IGoodsPriceCalculator goodsPriceCalculator,
			IAddProductValidator addProductValidator)
			: base(
				saleItemFactory,
				nomenclatureSettings,
				nomenclatureService,
				nomenclatureRepository,
				goodsPriceCalculator,
				addProductValidator
			)
		{
			_contractUpdater = contractUpdater ?? throw new ArgumentNullException(nameof(contractUpdater));
		}

		public override Result TryAddProduct(
			IUnitOfWork uow,
			IAddProductSource addProductSource,
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			IEnumerable<DiscountReason> discountReasons = null)
		{
			var addProductResult = base.TryAddProduct(uow, addProductSource, nomenclature, count, discount, discountReasons);

			if(addProductResult.IsSuccess)
			{
				_contractUpdater.UpdateContract(uow, addProductSource.Source as Order);
			}
			
			return addProductResult;
		}

		protected override void Recalculate()
		{
			throw new NotImplementedException();
		}
	}
}
