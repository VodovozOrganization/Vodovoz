using QS.Dialog;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Application.Mango;
using Vodovoz.Core.Domain.Mango;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.ViewModels.Widgets.Mango
{
	/// <inheritdoc cref="IMangoCallButtonViewModelFactory"/>
	public class MangoCallButtonViewModelFactory : IMangoCallButtonViewModelFactory
	{
		private const string _orderIsNotInRouteListMessage = "Заказ не добавлен в маршрутный лист";
		private const string _orderIsNotEnRouteMessage = "Заказ не в пути";
		private const string _routeListNotFoundMessage = "Маршрутный лист не найден";
		private const string _routeListIsNotEnRouteMessage = "Маршрутный лист не в пути";
		private const string _routeListHasNoDriverMessage = "В маршрутном листе не указан водитель";
		private const string _driverHasNoExtensionNumberMessage = "У водителя нет добавочного номера";

		private readonly IMangoManager _mangoManager;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IInteractiveService _interactiveService;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IGenericRepository<DriverMangoExtensionNumber> _driverMangoExtensionNumberRepository;

		public MangoCallButtonViewModelFactory(
			IMangoManager mangoManager,
			IGuiDispatcher guiDispatcher,
			IInteractiveService interactiveService,
			IRouteListItemRepository routeListItemRepository,
			IGenericRepository<DriverMangoExtensionNumber> driverMangoExtensionNumberRepository)
		{
			_mangoManager = mangoManager ?? throw new ArgumentNullException(nameof(mangoManager));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_driverMangoExtensionNumberRepository =
				driverMangoExtensionNumberRepository ?? throw new ArgumentNullException(nameof(driverMangoExtensionNumberRepository));
		}

		/// <inheritdoc/>
		public MangoCallButtonViewModel CreateForRouteListDriver(IUnitOfWork uow, RouteList routeList)
		{
			var viewModel = CreateViewModel();

			SetRouteListDriverAvailability(viewModel, uow, routeList);

			return viewModel;
		}

		/// <inheritdoc/>
		public MangoCallButtonViewModel CreateForOrderDriver(IUnitOfWork uow, Order order)
		{
			var viewModel = CreateViewModel();

			if(order is null || order.Id == 0)
			{
				viewModel.SetUnavailabilityReason(_orderIsNotInRouteListMessage);
				return viewModel;
			}

			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(uow, order);

			if(routeListItem is null)
			{
				viewModel.SetUnavailabilityReason(_orderIsNotInRouteListMessage);
				return viewModel;
			}

			if(routeListItem.Status != RouteListItemStatus.EnRoute)
			{
				viewModel.SetUnavailabilityReason(_orderIsNotEnRouteMessage);
				return viewModel;
			}

			SetRouteListDriverAvailability(viewModel, uow, routeListItem.RouteList);

			return viewModel;
		}

		/// <inheritdoc/>
		public void UpdateForRouteListDriver(MangoCallButtonViewModel viewModel, IUnitOfWork uow, RouteList routeList)
		{
			if(viewModel is null)
			{
				throw new ArgumentNullException(nameof(viewModel));
			}

			SetRouteListDriverAvailability(viewModel, uow, routeList);
		}

		private MangoCallButtonViewModel CreateViewModel() =>
			new MangoCallButtonViewModel(_mangoManager, _guiDispatcher, _interactiveService);

		/// <summary>
		/// Проверяет, можно ли позвонить водителю маршрутного листа,
		/// и задаёт вью-модели либо добавочный номер, либо причину недоступности звонка
		/// </summary>
		private void SetRouteListDriverAvailability(MangoCallButtonViewModel viewModel, IUnitOfWork uow, RouteList routeList)
		{
			if(routeList is null)
			{
				viewModel.SetUnavailabilityReason(_routeListNotFoundMessage);
				return;
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				viewModel.SetUnavailabilityReason(_routeListIsNotEnRouteMessage);
				return;
			}

			if(routeList.Driver is null)
			{
				viewModel.SetUnavailabilityReason(_routeListHasNoDriverMessage);
				return;
			}

			var extensionNumber = GetActiveExtensionNumber(uow, routeList.Driver.Id);

			if(extensionNumber is null)
			{
				viewModel.SetUnavailabilityReason(_driverHasNoExtensionNumberMessage);
				return;
			}

			viewModel.SetExtension(extensionNumber.Value);
		}

		private int? GetActiveExtensionNumber(IUnitOfWork uow, int driverId) =>
			_driverMangoExtensionNumberRepository
				.Get(
					uow,
					x => x.DriverId == driverId
						&& x.Status == DriverMangoExtensionNumberStatus.Active
						&& x.ExtensionNumber != null,
					limit: 1)
				.FirstOrDefault()
				?.ExtensionNumber;
	}
}
