using Microsoft.Extensions.Logging;
using MoreLinq;
using NHibernate;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Osrm;
using QS.Services;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Delivery;
using Vodovoz.Tools.Interactive.ConfirmationQuestion;
using Vodovoz.ViewModels.Extensions;
using FastDeliveryOrderTransferMode = Vodovoz.ViewModels.ViewModels.Logistic.FastDeliveryOrderTransferFilterViewModel.FastDeliveryOrderTransferMode;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public partial class FastDeliveryOrderTransferViewModel : WindowDialogViewModelBase, IDisposable
	{
		private readonly ILogger<FastDeliveryOrderTransferViewModel> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IUnitOfWork _unitOfWork;
		private readonly TimeSpan _driverOfflineTimeSpan;
		private readonly IRouteListRepository _routeListRepository;
		private readonly ICommonServices _commonServices;
		private readonly IConfirmationQuestionInteractive _confirmationQuestionInteractive;
		private readonly IAddressTransferController _addressTransferController;
		private readonly IRouteListTransferService _routeListTransferService;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private readonly IWageParameterService _wageParameterService;
		private readonly ITrackRepository _trackRepository;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IOsrmClient _osrmClient;
		private RouteList _routeListFrom;
		private RouteListItem _routeListItemToTransfer;

		public FastDeliveryOrderTransferViewModel(
			ILogger<FastDeliveryOrderTransferViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			IRouteListRepository routeListRepository,
			ICommonServices commonServices,
			FastDeliveryOrderTransferFilterViewModel filterViewModel,
			IRouteListItemRepository routeListItemRepository,
			IRouteListProfitabilityController routeListProfitabilityController,
			IWageParameterService wageParameterService,
			ITrackRepository trackRepository,
			IOsrmClient osrmClient,
			IDeliveryRulesSettings deliveryRulesSettings,
			IConfirmationQuestionInteractive confirmationQuestionInteractive,
			IAddressTransferController addressTransferController,
			IRouteListTransferService routeListTransferService,
			int routeListAddressId)
			: base(navigationManager)
		{
			Title = "Перенос заказа с доставкой за час";

			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_confirmationQuestionInteractive = confirmationQuestionInteractive ?? throw new ArgumentNullException(nameof(confirmationQuestionInteractive));
			_addressTransferController = addressTransferController ?? throw new ArgumentNullException(nameof(addressTransferController));
			_routeListTransferService = routeListTransferService ?? throw new ArgumentNullException(nameof(routeListTransferService));
			FilterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_routeListProfitabilityController = routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_osrmClient = osrmClient ?? throw new ArgumentNullException(nameof(osrmClient));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_unitOfWork = _unitOfWorkFactory.CreateWithoutRoot(Title);

			_driverOfflineTimeSpan = _deliveryRulesSettings.MaxTimeOffsetForLatestTrackPoint;

			CancelCommand = new DelegateCommand(Cancel, () => CanCancel);
			TransferCommand = new DelegateCommand(Transfer, () => CanTransfer);

			GetRouteListFromInfo(routeListAddressId);

			FilterViewModel.OnFiltered += OnFiltered;
			FilterViewModel.Update();

		}

		public GenericObservableList<RouteListNode> RouteListNodes { get; } = new GenericObservableList<RouteListNode>();

		public FastDeliveryOrderTransferFilterViewModel FilterViewModel { get; }

		public DelegateCommand TransferCommand { get; }

		public DelegateCommand CancelCommand { get; }

		public RouteListNode RouteListToSelectedNode { get; set; }

		public string AddressInfo => _routeListItemToTransfer?.Order?.DeliveryPoint?.ShortAddress;

		public string DriverInfo => $"от {_routeListFrom?.Driver?.ShortName} {_routeListFrom?.Car?.RegistrationNumber}";

		public bool CanCancel => true;

		public bool CanTransfer => _routeListFrom != null && _routeListItemToTransfer != null || false;

		private void OnFiltered(object sender, EventArgs e)
		{
			var newRouteLists = GetFastDeliveryRouteLists();

			RouteListNodes.Clear();

			foreach(var routeList in newRouteLists)
			{
				RouteListNodes.Add(routeList);
			}
		}

		private void GetRouteListFromInfo(int routeListAddressId)
		{
			_routeListItemToTransfer = _unitOfWork.GetById<RouteListItem>(routeListAddressId);

			if(_routeListItemToTransfer == null || _routeListItemToTransfer.RouteList == null)
			{
				return;
			}

			_routeListFrom = _routeListItemToTransfer.RouteList;
		}

		private bool MakeAddressTransfer(RouteListItem address, RouteList routeListFrom, RouteList routeListTo, decimal? distance)
		{
			_logger.LogDebug("Проверка адреса с номером {AddressId}", address?.Id.ToString() ?? "Неправильный адрес");

			if(address == null
				|| routeListFrom == null
				|| routeListTo == null)
			{
				_logger.LogDebug("Недостаточно данных для выполнения переноса");
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Недостаточно данных для выполнения переноса");
				return false;
			}

			if(!address.Order.IsFastDelivery)
			{
				_logger.LogDebug("Выбран заказ с обычной доставкой. Перенести можно только заказы с быстрой доставкой");
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Выбран заказ с обычной доставкой. Перенести можно только заказы с быстрой доставкой");
				return false;
			}

			if(address.Status == RouteListItemStatus.Transfered)
			{
				_logger.LogDebug("Данный заказ уже был перенесен");
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Данный заказ уже был перенесен");
				return false;
			}

			if(!IsRouteListToHasAcceptableOrdersCountAndDistance(routeListTo, distance))
			{
				return false;
			}

			var hasBalanceForTransfer = _routeListRepository.HasFreeBalanceForOrder(_unitOfWork, address.Order, routeListTo);

			if(!hasBalanceForTransfer)
			{
				_logger.LogDebug("В маршрутном листе получателя недостаточно свободных остатков");
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "В маршрутном листе получателя недостаточно свободных остатков");
				return false;
			}

			if(HasAddressChanges(address))
			{
				_logger.LogDebug("Статус маршрутного листа был изменён другим пользователем.");
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, $"Статус {address.Title} был изменён другим пользователем, для его переноса переоткройте диалог.");
				return false;
			}

			address.AddressTransferType = AddressTransferType.FromFreeBalance;

			var transferredAddressFromRouteListTo =
				_routeListItemRepository.GetTransferredRouteListItemFromRouteListForOrder(_unitOfWork, routeListTo.Id, address.Order.Id);

			RouteListItem newItem;

			if(transferredAddressFromRouteListTo != null)
			{
				newItem = transferredAddressFromRouteListTo;
				newItem.AddressTransferType = AddressTransferType.FromFreeBalance;
				address.WasTransfered = false;
				_routeListTransferService.RevertTransferAddress(_unitOfWork, routeListTo, newItem, address);
				_routeListTransferService.TransferAddressTo(_unitOfWork, routeListFrom, address, newItem);
				newItem.WasTransfered = true;
			}
			else
			{
				newItem = new RouteListItem(routeListTo, address.Order, address.Status)
				{
					WasTransfered = true,
					AddressTransferType = AddressTransferType.FromFreeBalance,
					WithForwarder = routeListTo.Forwarder != null
				};

				routeListTo.ObservableAddresses.Add(newItem);
				_routeListTransferService.TransferAddressTo(_unitOfWork, routeListFrom, address, newItem);
			}

			var transaction = _unitOfWork.Session.GetCurrentTransaction();

			if(transaction is null)
			{
				_unitOfWork.Session.BeginTransaction();
			}

			_unitOfWork.Session.Flush();

			routeListFrom.CalculateWages(_wageParameterService);
			_routeListProfitabilityController.ReCalculateRouteListProfitability(_unitOfWork, routeListFrom);
			routeListTo.CalculateWages(_wageParameterService);
			_routeListProfitabilityController.ReCalculateRouteListProfitability(_unitOfWork, routeListTo);

			address.RecalculateTotalCash();
			newItem.RecalculateTotalCash();

			if(routeListTo.ClosingFilled)
			{
				newItem.FirstFillClosing(_wageParameterService);
			}

			_unitOfWork.Save(address);
			_unitOfWork.Save(newItem);

			UpdateTranferDocuments(address, newItem, AddressTransferType.FromFreeBalance);

			_unitOfWork.Save(routeListTo);
			_unitOfWork.Save(routeListFrom);
			_unitOfWork.Commit();

			return true;
		}

		private bool IsRouteListToHasAcceptableOrdersCountAndDistance(RouteList routeListTo, decimal? distance)
		{
			var confirmationQuestions = new List<ConfirmationQuestion>();

			var maxFastDeliveryOrdersCountInRouteList = routeListTo.GetMaxFastDeliveryOrdersValue();
			if(GetFastDeliveryOrdersCountInRouteList(routeListTo) >= maxFastDeliveryOrdersCountInRouteList)
			{
				_logger.LogDebug("В выбранном маршрутном листе уже имеется максимально допустимое количество заказов с быстрой доставкой. " +
					"Требуется подтверждение переноса.");

				confirmationQuestions.Add(new ConfirmationQuestion
				{
					QuestionText = $"При переносе заказа на\nданного водителя, его\nлимит ДЗЧ будет превышен.\nВы точно хотите перенести\nзаказ?",
					ConfirmationText = "Подтверждаю"
				});
			}

			if(distance == null || distance.Value > routeListTo.GetFastDeliveryMaxDistanceValue())
			{
				var distanceValue =
					distance.HasValue
					? distance.Value.ToString("F2")
					: "'Ошибка при расчете дистанции'";

				_logger.LogDebug("Расстояние до данного заказа {distanceValue}км. Требуется подтверждение переноса.");

				confirmationQuestions.Add(new ConfirmationQuestion
				{
					QuestionText = $"Расстояние до данного\nзаказа {distanceValue}км.\nВы точно хотите осуществить\nперенос?",
					ConfirmationText = "Подтверждаю"
				});
			}

			if(confirmationQuestions.Count > 0)
			{
				var confirmationResult = _confirmationQuestionInteractive.Question(
					confirmationQuestions,
					isNoButtonAvailableByDefault: true,
					imageType: ConfirmationQuestionDialogSettings.ImgType.Warning);

				if(!confirmationResult)
				{
					return false;
				}
			}

			return true;
		}

		private void UpdateTranferDocuments(RouteListItem from, RouteListItem to, AddressTransferType addressTransferType)
		{
			_addressTransferController.UpdateDocuments(from, to, _unitOfWork, addressTransferType);
		}

		private bool HasAddressChanges(RouteListItem address)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Получение статуса адреса"))
			{
				return uow.GetById<RouteListItem>(address.Id).Status != address.Status;
			}
		}

		private int GetFastDeliveryOrdersCountInRouteList(RouteList routeList)
		{
			var fastDeliveryOrdersCount = 0;

			foreach(var address in routeList.Addresses)
			{
				if(address.Order.IsFastDelivery && address.Status == RouteListItemStatus.EnRoute)
				{
					fastDeliveryOrdersCount++;
				}
			}

			return fastDeliveryOrdersCount;
		}

		private List<RouteListNode> GetFastDeliveryRouteLists()
		{
			if(_routeListFrom == null || _routeListItemToTransfer == null)
			{
				return new List<RouteListNode>();
			}

			var routeListFromId = _routeListFrom.Id;

			var routeLists = (from routeList in _unitOfWork.Session.Query<RouteList>()
							  where routeList.Id != routeListFromId
								 && routeList.Status == RouteListStatus.EnRoute
								 && ((FilterViewModel.Mode == FastDeliveryOrderTransferMode.FastDelivery && routeList.AdditionalLoadingDocument.Id != null)
									|| (FilterViewModel.Mode == FastDeliveryOrderTransferMode.Shifted && routeList.AdditionalLoadingDocument.Id == null)
									|| FilterViewModel.Mode == FastDeliveryOrderTransferMode.All)
							  let driverFIO = $"{routeList.Driver.LastName} {routeList.Driver.Name[0]}. {routeList.Driver.Patronymic[0]}."
							  select routeList)
							 .ToList();

			PointOnEarth point = _routeListItemToTransfer.Order.DeliveryPoint.GetPointOnEarth();

			var lastTrackPointsWithRadiuses = _trackRepository
				.GetLastPointForRouteLists(_unitOfWork, routeLists.Select(x => x.Id).ToArray());

			var routeListNodes = new List<RouteListNode>();

			for(int i = 0; i < routeLists.Count; i++)
			{
				var currentLastTrackPointWithRadius = lastTrackPointsWithRadiuses
					.FirstOrDefault(x => x.RouteListId == routeLists[i].Id);

				if(currentLastTrackPointWithRadius != null
					&& (DateTime.Now - currentLastTrackPointWithRadius.Time) < _driverOfflineTimeSpan
					&& _routeListRepository.HasFreeBalanceForOrder(_unitOfWork, _routeListItemToTransfer.Order, routeLists[i]))
				{
					PointOnEarth currentRouteListLastTrackPoint = new PointOnEarth(
						Convert.ToDouble(currentLastTrackPointWithRadius.Latitude),
						Convert.ToDouble(currentLastTrackPointWithRadius.Longitude));

					RouteResponse route = null;

					try
					{
						route = _osrmClient.GetRoute(new List<PointOnEarth> { point, currentRouteListLastTrackPoint });
					}
					catch(Exception e)
					{
						_logger.LogError(e, "Ошибка получения результатов вычисления маршрута");
					}

					var distance = route?.Routes?.First().TotalDistanceKm;

					routeListNodes.Add(new RouteListNode
					{
						RouteListId = routeLists[i].Id,
						CarRegistrationNumber = routeLists[i].Car.RegistrationNumber,
						DriverFullName = routeLists[i].Driver.ShortName,
						Distance = distance
					});
				}
			}

			var resortedRouteLists = routeListNodes
				.OrderByDescending(x => x.Distance.HasValue)
				.ThenBy(x => (x.Distance, x.DriverFullName));

			var rowNumber = 1;

			resortedRouteLists.ForEach(x => x.RowNumber = rowNumber++);

			return resortedRouteLists.ToList();
		}

		private void Transfer()
		{
			if(_routeListFrom == null
				|| _routeListItemToTransfer == null
				|| RouteListToSelectedNode == null)
			{
				_logger.LogDebug("Недостаточно данных для выполнения переноса");
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Недостаточно данных для выполнения переноса");
				return;
			}

			var routeListTo = _unitOfWork.GetById<RouteList>(RouteListToSelectedNode.RouteListId);

			if(routeListTo == null)
			{
				return;
			}

			var isTransferSuccessful = MakeAddressTransfer(_routeListItemToTransfer, _routeListFrom, routeListTo, RouteListToSelectedNode?.Distance);

			if(isTransferSuccessful)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Заказ успешно перенесен!");
				Close(false, CloseSource.Cancel);
			}
		}

		private void Cancel()
		{
			Close(false, CloseSource.Cancel);
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
