using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Extensions.Logging;
using QS.Navigation;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Mango;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Factories;
using VodovozBusiness.Nodes;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	/// <summary>
	/// Вью модель для онлайн заказа 2 версии
	/// </summary>
	public class OnlineOrderV2ViewModel : OnlineOrderViewModel
	{
		private readonly IPromotionalSetRepository _promoSetRepository;

		public OnlineOrderV2ViewModel(
			ILogger<OnlineOrderV2ViewModel> logger,
			ILifetimeScope scope,
			IGtkTabsOpener gtkTabsOpener,
			IEntityViewModelContext viewModelContext,
			ICommonServices commonServices,
			INavigationManager navigation,
			IEmployeeService employeeService,
			IOnlineOrderValidatorCreator onlineOrderValidatorCreator,
			IExternalCounterpartyMatchingRepository externalCounterpartyMatchingRepository,
			ViewModelEEVMBuilder<DeliveryPoint> deliveryPointViewModelBuilder,
			DeliveryPointJournalFilterViewModel deliveryPointJournalFilterViewModel,
			IDiscountController discountController,
			IOrderOrganizationManager orderOrganizationManager,
			MangoManager mangoManager,
			IPromotionalSetRepository promoSetRepository
		)
			: base(
				logger,
				scope,
				gtkTabsOpener,
				viewModelContext,
				commonServices,
				navigation,
				employeeService,
				onlineOrderValidatorCreator,
				externalCounterpartyMatchingRepository,
				deliveryPointViewModelBuilder,
				deliveryPointJournalFilterViewModel,
				discountController,
				orderOrganizationManager,
				mangoManager
			)
		{
			_promoSetRepository = promoSetRepository ?? throw new ArgumentNullException(nameof(promoSetRepository));
			Initialize();
		}
		
		/// <summary>
		/// Онлайн заказ
		/// </summary>
		public new OnlineOrderV2 Entity => (OnlineOrderV2)entity;
		
		/// <summary>
		/// Можно ли отображать виджеты промонаборов
		/// </summary>
		public override bool CanShowPromoSetsWidgets => CanShowPromoSets;
		/// <summary>
		/// Можно ли отображать промонаборы
		/// </summary>
		public virtual bool CanShowPromoSets => OnlineOrderPromoSets.Any();
		/// <summary>
		/// Список данных по промонаборам
		/// </summary>
		public IList<OnlineOrderPromoSetNode> OnlineOrderPromoSets { get; } = new List<OnlineOrderPromoSetNode>();
		
		protected override void GetOnlineOrderItems()
		{
			foreach(var item in Entity.OnlineOrderItems)
			{
				OnlineOrderNotPromoItems.Add(item);
			}

			GetPromoSetsData();
			OnlineRentPackages = Entity.OnlineRentPackages;
		}

		private void GetPromoSetsData()
		{
			var promoSetsData = _promoSetRepository.GetOnlineOrderPromoSetsData(UoW, Entity.Id);
			var promoSetsItemsData = _promoSetRepository
				.GetOnlineOrderPromoSetItemsData(UoW, Entity.Id)
				.ToLookup(x => x.OnlinePromoSetId);
			
			foreach(var promoSetNode in promoSetsData)
			{
				var items = promoSetsItemsData[promoSetNode.Id];
				
				if(!items.Any())
				{
					throw new InvalidOperationException("Произошла нестандартная ситуация. У промонабора не может быть пустой список товаров");
				}

				foreach(var item in items)
				{
					promoSetNode.AddItem(item);
				}
				
				OnlineOrderPromoSets.Add(promoSetNode);
			}
		}
	}
}
