using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RouteListMaxFastDeliveryOrdersViewModel : EntityTabViewModelBase<RouteList>
	{
		private readonly ILogger<RouteListFastDeliveryMaxDistanceViewModel> _logger;
		private int _maxFastDeliveryOrders;

		public RouteListMaxFastDeliveryOrdersViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IRouteListItemRepository routeListItemRepository,
			ILogger<RouteListFastDeliveryMaxDistanceViewModel> logger) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_maxFastDeliveryOrders = Entity.GetMaxFastDeliveryOrdersValue();
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			ValidationContext.Items.Add(nameof(IRouteListItemRepository), routeListItemRepository);
			//Для возможности изменения пустых МЛ
			ValidationContext.Items.Add(
				Core.Domain.Permissions.LogisticPermissions.RouteList.CanCreateRouteListWithoutOrders,
				true);

			TabName = $"Изменение макс. кол-ва заказов ДЗЧ для МЛ №{Entity.Id}";
		}

		public int MaxFastDeliveryOrders
		{
			get => _maxFastDeliveryOrders;
			set => SetField(ref _maxFastDeliveryOrders, value);
		}

		public override bool Save(bool close)
		{
			if(MaxFastDeliveryOrders != Entity.GetMaxFastDeliveryOrdersValue())
			{
				Entity.UpdateMaxFastDeliveryOrdersValue(MaxFastDeliveryOrders);
			}

			_logger.LogInformation("Добавляем новое значение макс. кол-ва заказов ДЗЧ...");
			return base.Save(close);
		}
	}
}
