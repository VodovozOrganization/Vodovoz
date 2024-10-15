using MoreLinq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Presentation.ViewModels.Extensions;
using Vodovoz.Presentation.ViewModels.Factories;
using static Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets.PotentialFreePromosetsReport;

namespace Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets
{
	public partial class PotentialFreePromosetsReportViewModel : DialogViewModelBase, IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly IGenericRepository<PromotionalSet> _promotionalSetRepository;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IInteractiveService _interactiveService;
		private readonly IDialogSettingsFactory _dialogSettingsFactory;
		private readonly IFileDialogService _fileDialogService;

		private readonly IList<OrderStatus> _notDeliveredOrderStatuses =
			new List<OrderStatus> { OrderStatus.Canceled, OrderStatus.NotDelivered, OrderStatus.DeliveryCanceled };

		private DateTime? _startDate;
		private DateTime? _endDate;
		private PotentialFreePromosetsReport _report;
		private bool _isReportGenerationInProgress;

		public PotentialFreePromosetsReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IGenericRepository<PromotionalSet> promotionalSetRepository,
			IGuiDispatcher guiDispatcher,
			IInteractiveService interactiveService,
			IDialogSettingsFactory dialogSettingsFactory,
			IFileDialogService fileDialogService)
			: base(navigation)
		{
			_promotionalSetRepository = promotionalSetRepository ?? throw new ArgumentNullException(nameof(promotionalSetRepository));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_dialogSettingsFactory = dialogSettingsFactory ?? throw new ArgumentNullException(nameof(dialogSettingsFactory));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_uow = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory)))
				.CreateWithoutRoot(nameof(PotentialFreePromosetsReportViewModel));

			Title = "Отчет по потенциальным халявщикам";

			FillPromotionalSets();

			GenerateReportCommand = new DelegateCommand(GenerateReport);
			AbortReportGenerationCommand = new DelegateCommand(AbortReportGeneration);
			SaveReportCommand = new DelegateCommand(SaveReport);
		}

		public DelegateCommand GenerateReportCommand { get; }
		public DelegateCommand AbortReportGenerationCommand { get; }
		public DelegateCommand SaveReportCommand { get; }

		public IEnumerable<PromosetNode> PromotionalSets { get; private set; }

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public PotentialFreePromosetsReport Report
		{
			get => _report;
			set => SetField(ref _report, value);
		}

		public bool IsReportGenerationInProgress
		{
			get => _isReportGenerationInProgress;
			set => SetField(ref _isReportGenerationInProgress, value);
		}

		private void FillPromotionalSets()
		{
			PromotionalSets =
				_promotionalSetRepository
				.Get(_uow)
				.Select(ps => new PromosetNode
				{
					Id = ps.Id,
					Name = ps.Name,
					IsSelected = ps.PromotionalSetForNewClients
				})
				.ToList();
		}

		private void GenerateReport()
		{
			if(IsReportGenerationInProgress)
			{
				return;
			}

			if(StartDate is null || EndDate is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, "Необходимо ввести полный период");

				return;
			}

			IsReportGenerationInProgress = true;

			var rows = MakeQueries();

			Report = new PotentialFreePromosetsReport(rows);

			IsReportGenerationInProgress = false;
		}

		private void AbortReportGeneration()
		{
			if(!IsReportGenerationInProgress)
			{
				return;
			}

			IsReportGenerationInProgress = false;
		}

		private void SaveReport()
		{
			if(Report is null)
			{
				return;
			}

			var dialogSettings = _dialogSettingsFactory.CreateForClosedXmlReport(_report);

			var saveDialogResult = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(saveDialogResult.Successful)
			{
				_report.RenderTemplate().Export(saveDialogResult.Path);
			}
		}

		private IList<PromosetReportRow> GetAddressDuplicates(IUnitOfWork uow)
		{
			var selectedPromosets = GetSelectedPromotionalSets();

			var deliveryPointsHavingAddresses =
				(from orderItem in _uow.Session.Query<OrderItem>()
				 join order in _uow.Session.Query<Order>() on orderItem.Order.Id equals order.Id
				 join deliveryPoint in _uow.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals deliveryPoint.Id
				 join deliveryPoint2 in _uow.Session.Query<DeliveryPoint>()
				 on new { deliveryPoint.City, deliveryPoint.Street, deliveryPoint.Building, deliveryPoint.Room }
				 equals new { deliveryPoint2.City, deliveryPoint2.Street, deliveryPoint2.Building, deliveryPoint2.Room }
				 where
				 order.CreateDate >= StartDate.Value
				 && order.CreateDate < EndDate.Value.Date.AddDays(1)
				 && selectedPromosets.Contains(orderItem.PromoSet.Id)
				 && orderItem.Order.Id != null
				 && !_notDeliveredOrderStatuses.Contains(order.OrderStatus)
				 select deliveryPoint2.Id)
				 .Distinct()
				 .ToList();

			var ordersHavingPromosetAndDeliveryPoint =
				(from orderItem in _uow.Session.Query<OrderItem>()
				 join order in _uow.Session.Query<Order>() on orderItem.Order.Id equals order.Id
				 join deliveryPoint in _uow.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals deliveryPoint.Id
				 where
				 orderItem.PromoSet.Id != null
				 && orderItem.Order.Id != null
				 && !_notDeliveredOrderStatuses.Contains(order.OrderStatus)
				 && deliveryPointsHavingAddresses.Contains(order.DeliveryPoint.Id)
				 select new OrderDeliveryPointDataNode
				 {
					 OrderId = order.Id,
					 OrderCreateDate = order.CreateDate,
					 OrderDeliveryDate = order.DeliveryDate,
					 AuthorId = order.Author.Id,
					 ClientId = order.Client.Id,
					 PromosetId = orderItem.PromoSet.Id,
					 AddressDataNode = new AddressDataNode
					 {
						 City = deliveryPoint.City.ToLower().Trim(),
						 Street = deliveryPoint.Street.ToLower().Trim(),
						 Building = deliveryPoint.Building.ToLower().Trim(),
						 Room = deliveryPoint.Room.ToLower().Trim()
					 },
					 DeliveryPointCompiledAddress = deliveryPoint.CompiledAddress,
					 DeliveryPointAddressCategoryId = deliveryPoint.Category.Id
				 })
				 .Distinct()
				 .ToList();

			var ordersHavingSelectedPromosetsForPeriod =
				(from ordersData in ordersHavingPromosetAndDeliveryPoint
				 where
				 ordersData.OrderCreateDate >= StartDate.Value
				 && ordersData.OrderCreateDate < EndDate.Value.Date.AddDays(1)
				 && selectedPromosets.Contains(ordersData.PromosetId)
				 select ordersData)
				 .Distinct()
				 .ToList();

			var orderAndDuplicatesOnDeliveryAddresses =
				(from rootOrder in ordersHavingSelectedPromosetsForPeriod
				 join orderDuplicate in ordersHavingPromosetAndDeliveryPoint on new
				 {
					 rootOrder.AddressDataNode.City,
					 rootOrder.AddressDataNode.Street,
					 rootOrder.AddressDataNode.Building,
					 rootOrder.AddressDataNode.Room
				 }
				 equals new
				 {
					 orderDuplicate.AddressDataNode.City,
					 orderDuplicate.AddressDataNode.Street,
					 orderDuplicate.AddressDataNode.Building,
					 orderDuplicate.AddressDataNode.Room
				 }
				 where orderDuplicate.OrderId != rootOrder.OrderId
				 select new { OrderDuplicate = orderDuplicate, RootOrder = rootOrder })
				 .Distinct()
				 .GroupBy(x => x.RootOrder)
				 .ToDictionary(x => x.Key, x => x.Select(o => o.OrderDuplicate).ToList())
				 .OrderBy(x => x.Key.OrderId);

			var orderAuthorIds =
				orderAndDuplicatesOnDeliveryAddresses
				.Select(o => o.Key.AuthorId)
				.Union(orderAndDuplicatesOnDeliveryAddresses.SelectMany(o => o.Value.Select(ord => ord.AuthorId)))
				.Distinct()
				.ToList();

			var authorNames =
				(from author in uow.Session.Query<Employee>()
				 where orderAuthorIds.Contains(author.Id)
				 select new { AuthorId = author.Id, AuthorName = author.ShortName })
				 .ToDictionary(a => a.AuthorId, a => a.AuthorName);

			var orderClientIds =
				orderAndDuplicatesOnDeliveryAddresses
				.Select(o => o.Key.ClientId)
				.Union(orderAndDuplicatesOnDeliveryAddresses.SelectMany(o => o.Value.Select(ord => ord.ClientId)))
				.Distinct()
				.ToList();

			var clientNames =
				(from client in uow.Session.Query<Counterparty>()
				 where orderClientIds.Contains(client.Id)
				 select new { ClientId = client.Id, ClientName = client.FullName })
				 .ToDictionary(c => c.ClientId, a => a.ClientName);

			var orderPromosetIds =
				orderAndDuplicatesOnDeliveryAddresses
				.Select(o => o.Key.PromosetId)
				.Union(orderAndDuplicatesOnDeliveryAddresses.SelectMany(o => o.Value.Select(ord => ord.PromosetId)))
				.Distinct()
				.ToList();

			var promosetNames =
				(from promoset in uow.Session.Query<PromotionalSet>()
				 where orderPromosetIds.Contains(promoset.Id)
				 select new { PromosetId = promoset.Id, PromosetName = promoset.Name })
				 .ToDictionary(c => c.PromosetId, a => a.PromosetName);

			var orderAddressCategoryIds =
				orderAndDuplicatesOnDeliveryAddresses
				.Select(o => o.Key.DeliveryPointAddressCategoryId)
				.Union(orderAndDuplicatesOnDeliveryAddresses.SelectMany(o => o.Value.Select(ord => ord.DeliveryPointAddressCategoryId)))
				.Distinct()
				.ToList();

			var orderAddressCategoryNames =
				(from addressCategory in uow.Session.Query<DeliveryPointCategory>()
				 where orderAddressCategoryIds.Contains(addressCategory.Id)
				 select new { AddressCategoryId = addressCategory.Id, AddressCategoryName = addressCategory.Name })
				 .ToDictionary(c => c.AddressCategoryId, a => a.AddressCategoryName);

			var promosetReportRows = new List<PromosetReportRow>();
			var sequenceNumber = 1;

			foreach(var orderWithDuplicates in orderAndDuplicatesOnDeliveryAddresses)
			{
				var rootRow = CreatePromosetReportRow(
					orderWithDuplicates.Key,
					sequenceNumber++,
					orderAddressCategoryNames,
					clientNames,
					promosetNames,
					authorNames,
					true);

				promosetReportRows.Add(rootRow);

				foreach(var duplicate in orderWithDuplicates.Value)
				{
					var duplicateRow = CreatePromosetReportRow(
					duplicate,
					sequenceNumber++,
					orderAddressCategoryNames,
					clientNames,
					promosetNames,
					authorNames,
					false);

					promosetReportRows.Add(duplicateRow);
				}
			}

			return promosetReportRows;
		}

		private PromosetReportRow CreatePromosetReportRow(
			OrderDeliveryPointDataNode orderDeliveryPointDataNode,
			int sequenceNumber,
			IDictionary<int, string> orderAddressCategoryNames,
			IDictionary<int, string> clientNames,
			IDictionary<int, string> promosetNames,
			IDictionary<int, string> authorNames,
			bool isRoot = false)
		{
			var orderId = orderDeliveryPointDataNode.OrderId;

			var addressCategoryId =
				orderDeliveryPointDataNode.DeliveryPointAddressCategoryId.HasValue
				? orderDeliveryPointDataNode.DeliveryPointAddressCategoryId.Value
				: -1;

			var row = new PromosetReportRow
			{
				SequenceNumber = sequenceNumber,
				Address = orderDeliveryPointDataNode.DeliveryPointCompiledAddress,
				AddressCategory =
					orderAddressCategoryNames.TryGetValue(addressCategoryId, out string addressCategory)
					? addressCategory
					: string.Empty,
				Phone = null,
				Client =
					clientNames.TryGetValue(orderDeliveryPointDataNode.ClientId, out string clientName)
					? clientName
					: string.Empty,
				Order = orderId,
				OrderCreationDate = orderDeliveryPointDataNode.OrderCreateDate,
				OrderDeliveryDate = orderDeliveryPointDataNode.OrderDeliveryDate,
				Promoset =
					promosetNames.TryGetValue(orderDeliveryPointDataNode.PromosetId, out string promosetName)
					? promosetName
					: string.Empty,
				Author =
					authorNames.TryGetValue(orderDeliveryPointDataNode.AuthorId, out string authorName)
					? authorName
					: string.Empty,
				IsRootRow = isRoot
			};

			return row;
		}

		private IList<PromosetReportRow> MakeQueries()
		{
			var rows = new List<PromosetReportRow>();

			var duplicatesByAddress = GetAddressDuplicates(_uow);
			rows.AddRange(duplicatesByAddress);

			var selectedPromosets = GetSelectedPromotionalSets();

			var ordersWithPhonesHavingPromotionalSetsByDeliveryPointPhoneDigitNumbersForPeriod =
				(from order in _uow.Session.Query<Order>()
				 join orderItem in _uow.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
				 join phone in _uow.Session.Query<Phone>() on order.DeliveryPoint.Id equals phone.DeliveryPoint.Id
				 join dp in _uow.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals dp.Id into deliveryPoints
				 from deliveryPoint in deliveryPoints.DefaultIfEmpty()
				 join cl in _uow.Session.Query<Counterparty>() on order.Client.Id equals cl.Id into clients
				 from client in clients.DefaultIfEmpty()
				 join dpc in _uow.Session.Query<DeliveryPointCategory>() on deliveryPoint.Category.Id equals dpc.Id into deliveryPointCategories
				 from deliveryPointCategory in deliveryPointCategories.DefaultIfEmpty()
				 join author in _uow.Session.Query<Employee>() on order.Author.Id equals author.Id
				 join promoset in _uow.Session.Query<PromotionalSet>() on orderItem.PromoSet.Id equals promoset.Id
				 where
				 order.CreateDate >= StartDate.Value.Date
				 && order.CreateDate < EndDate.Value.Date.AddDays(1)
				 && selectedPromosets.Contains(orderItem.PromoSet.Id)
				 && !phone.IsArchive
				 && !_notDeliveredOrderStatuses.Contains(order.OrderStatus)
				 select new OrderWithPhoneDataNode
				 {
					 OrderId = order.Id,
					 ClientId = order.Client.Id,
					 DeliveryPointId = order.DeliveryPoint.Id,
					 OrderCreateDate = order.CreateDate,
					 OrderDeliveryDate = order.DeliveryDate,
					 AuthorId = order.Author.Id,
					 AuthorName = author.ShortName,
					 PhoneNumber = phone.Number,
					 PhoneDigitNumber = phone.DigitsNumber,
					 ClientName = client.FullName,
					 DeliveryPointAddress = deliveryPoint.ShortAddress,
					 DeliveryPointCategory = deliveryPointCategory.Name,
					 PromosetId = promoset.Id,
					 PromosetName = promoset.Name,
					 IsRoot = true
				 }).Distinct().ToList();

			var ordersWithPhonesHavingPromotionalSetsByClientPhoneDigitNumbersForPeriod =
				(from order in _uow.Session.Query<Order>()
				 join orderItem in _uow.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
				 join phone in _uow.Session.Query<Phone>() on order.Client.Id equals phone.Counterparty.Id
				 join dp in _uow.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals dp.Id into deliveryPoints
				 from deliveryPoint in deliveryPoints.DefaultIfEmpty()
				 join cl in _uow.Session.Query<Counterparty>() on order.Client.Id equals cl.Id into clients
				 from client in clients.DefaultIfEmpty()
				 join dpc in _uow.Session.Query<DeliveryPointCategory>() on deliveryPoint.Category.Id equals dpc.Id into deliveryPointCategories
				 from deliveryPointCategory in deliveryPointCategories.DefaultIfEmpty()
				 join author in _uow.Session.Query<Employee>() on order.Author.Id equals author.Id
				 join promoset in _uow.Session.Query<PromotionalSet>() on orderItem.PromoSet.Id equals promoset.Id
				 where
				 order.CreateDate >= StartDate.Value.Date
				 && order.CreateDate < EndDate.Value.Date.AddDays(1)
				 && selectedPromosets.Contains(orderItem.PromoSet.Id)
				 && !phone.IsArchive
				 && !_notDeliveredOrderStatuses.Contains(order.OrderStatus)
				 select new OrderWithPhoneDataNode
				 {
					 OrderId = order.Id,
					 ClientId = order.Client.Id,
					 DeliveryPointId = order.DeliveryPoint.Id,
					 OrderCreateDate = order.CreateDate,
					 OrderDeliveryDate = order.DeliveryDate,
					 AuthorId = order.Author.Id,
					 AuthorName = author.ShortName,
					 PhoneNumber = phone.Number,
					 PhoneDigitNumber = phone.DigitsNumber,
					 ClientName = client.FullName,
					 DeliveryPointAddress = deliveryPoint.ShortAddress,
					 DeliveryPointCategory = deliveryPointCategory.Name,
					 PromosetId = promoset.Id,
					 PromosetName = promoset.Name,
					 IsRoot = true
				 })
				 .Distinct()
				 .ToList();

			var ordersWithPhonesHavingPromotionalSetsByAllDigitNumbersForPeriod =
				ordersWithPhonesHavingPromotionalSetsByDeliveryPointPhoneDigitNumbersForPeriod
				.Union(ordersWithPhonesHavingPromotionalSetsByClientPhoneDigitNumbersForPeriod)
				.DistinctBy(o => new { o.OrderId, o.PhoneDigitNumber, o.PromosetId })
				.OrderBy(o => o.OrderId)
				.ToList();

			var allDigitNumbersHavingPromotionalSetsForPeriod =
				ordersWithPhonesHavingPromotionalSetsByAllDigitNumbersForPeriod
				.Select(o => o.PhoneDigitNumber)
				.Distinct()
				.OrderBy(p => p)
				.ToList();

			var ordersWithPhonesHavingPromotionalSetsByDeliveryPointPhoneDigitNumbers =
				(from order in _uow.Session.Query<Order>()
				 join orderItem in _uow.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
				 join phone in _uow.Session.Query<Phone>() on order.DeliveryPoint.Id equals phone.DeliveryPoint.Id
				 join dp in _uow.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals dp.Id into deliveryPoints
				 from deliveryPoint in deliveryPoints.DefaultIfEmpty()
				 join cl in _uow.Session.Query<Counterparty>() on order.Client.Id equals cl.Id into clients
				 from client in clients.DefaultIfEmpty()
				 join dpc in _uow.Session.Query<DeliveryPointCategory>() on deliveryPoint.Category.Id equals dpc.Id into deliveryPointCategories
				 from deliveryPointCategory in deliveryPointCategories.DefaultIfEmpty()
				 join author in _uow.Session.Query<Employee>() on order.Author.Id equals author.Id
				 join promoset in _uow.Session.Query<PromotionalSet>() on orderItem.PromoSet.Id equals promoset.Id
				 where
				 orderItem.PromoSet.Id != null
				 && !phone.IsArchive
				 && !_notDeliveredOrderStatuses.Contains(order.OrderStatus)
				 && allDigitNumbersHavingPromotionalSetsForPeriod.Contains(phone.DigitsNumber)
				 select new OrderWithPhoneDataNode
				 {
					 OrderId = order.Id,
					 ClientId = order.Client.Id,
					 DeliveryPointId = order.DeliveryPoint.Id,
					 OrderCreateDate = order.CreateDate,
					 OrderDeliveryDate = order.DeliveryDate,
					 AuthorId = order.Author.Id,
					 AuthorName = author.ShortName,
					 PhoneNumber = phone.Number,
					 PhoneDigitNumber = phone.DigitsNumber,
					 ClientName = client.FullName,
					 DeliveryPointAddress = deliveryPoint.ShortAddress,
					 DeliveryPointCategory = deliveryPointCategory.Name,
					 PromosetId = promoset.Id,
					 PromosetName = promoset.Name
				 })
				 .Distinct()
				 .ToList();

			var ordersWithPhonesHavingPromotionalSetsByClientPhoneDigitNumbers =
				(from order in _uow.Session.Query<Order>()
				 join orderItem in _uow.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
				 join phone in _uow.Session.Query<Phone>() on order.Client.Id equals phone.Counterparty.Id
				 join dp in _uow.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals dp.Id into deliveryPoints
				 from deliveryPoint in deliveryPoints.DefaultIfEmpty()
				 join cl in _uow.Session.Query<Counterparty>() on order.Client.Id equals cl.Id into clients
				 from client in clients.DefaultIfEmpty()
				 join dpc in _uow.Session.Query<DeliveryPointCategory>() on deliveryPoint.Category.Id equals dpc.Id into deliveryPointCategories
				 from deliveryPointCategory in deliveryPointCategories.DefaultIfEmpty()
				 join author in _uow.Session.Query<Employee>() on order.Author.Id equals author.Id
				 join promoset in _uow.Session.Query<PromotionalSet>() on orderItem.PromoSet.Id equals promoset.Id
				 where
				 orderItem.PromoSet.Id != null
				 && !phone.IsArchive
				 && !_notDeliveredOrderStatuses.Contains(order.OrderStatus)
				 && allDigitNumbersHavingPromotionalSetsForPeriod.Contains(phone.DigitsNumber)
				 select new OrderWithPhoneDataNode
				 {
					 OrderId = order.Id,
					 ClientId = order.Client.Id,
					 DeliveryPointId = order.DeliveryPoint.Id,
					 OrderCreateDate = order.CreateDate,
					 OrderDeliveryDate = order.DeliveryDate,
					 AuthorId = order.Author.Id,
					 AuthorName = author.ShortName,
					 PhoneNumber = phone.Number,
					 PhoneDigitNumber = phone.DigitsNumber,
					 ClientName = client.FullName,
					 DeliveryPointAddress = deliveryPoint.ShortAddress,
					 DeliveryPointCategory = deliveryPointCategory.Name,
					 PromosetId = promoset.Id,
					 PromosetName = promoset.Name
				 })
				 .Distinct()
				 .ToList();

			var ordersWithPhonesHavingPromotionalSetsByAllDigitNumbers =
				ordersWithPhonesHavingPromotionalSetsByDeliveryPointPhoneDigitNumbers
				.Union(ordersWithPhonesHavingPromotionalSetsByClientPhoneDigitNumbers)
				.DistinctBy(o => new { o.OrderId, o.PhoneDigitNumber, o.PromosetId })
				.OrderBy(o => o.OrderId)
				.ThenBy(o => o.IsRoot)
				.ToList();

			var phonesByClientAndDeliveryPoint =
				ordersWithPhonesHavingPromotionalSetsByAllDigitNumbers
				.GroupBy(o => (o.ClientId, o.DeliveryPointId))
				.ToDictionary(g => g.Key, g => g.Select(o => o.PhoneNumber).Distinct().ToList());

			var ff =
				(from orderForPeriod in ordersWithPhonesHavingPromotionalSetsByAllDigitNumbersForPeriod
				 join orderDuplicate in ordersWithPhonesHavingPromotionalSetsByAllDigitNumbers on orderForPeriod.PhoneDigitNumber equals orderDuplicate.PhoneDigitNumber
				 where orderForPeriod.OrderId != orderDuplicate.OrderId
				 select new { OrderForPeriod = orderForPeriod, OrderDuplicate = orderDuplicate })
				.GroupBy(o => o.OrderForPeriod)
				.OrderBy(g => g.Key.OrderId)
				.ToDictionary(g => g.Key, g => g.Select(o => o.OrderDuplicate).DistinctBy(o => o.OrderId).ToList())
				.DistinctBy(g => new { g.Key.OrderId, g.Key.DeliveryPointId, g.Key.PromosetId });


			var promosetReportRows = new List<PromosetReportRow>();
			var sequenceNumber = duplicatesByAddress.Count + 1;

			foreach(var item in ff)
			{
				var rootOrder = item.Key;
				var rootOrderPhones =
					phonesByClientAndDeliveryPoint.TryGetValue((rootOrder.ClientId, rootOrder.DeliveryPointId), out var rootPhoneNumbers)
					? string.Join(", ", rootPhoneNumbers)
					: rootOrder.PhoneNumber;

				var rootRow = new PromosetReportRow
				{
					SequenceNumber = sequenceNumber++,
					Address = rootOrder.DeliveryPointAddress,
					AddressCategory = rootOrder.DeliveryPointCategory,
					Phone = rootOrderPhones,
					Client = rootOrder.ClientName,
					Order = rootOrder.OrderId,
					OrderCreationDate = rootOrder.OrderCreateDate,
					OrderDeliveryDate = rootOrder.OrderDeliveryDate,
					Promoset = rootOrder.PromosetName,
					Author = rootOrder.AuthorName,
					IsRootRow = true
				};

				promosetReportRows.Add(rootRow);

				foreach(var duplicateOrder in item.Value)
				{
					var duplicateOrderPhones =
						phonesByClientAndDeliveryPoint.TryGetValue((duplicateOrder.ClientId, duplicateOrder.DeliveryPointId), out var duplicatePhoneNumbers)
						? string.Join(", ", duplicatePhoneNumbers)
						: duplicateOrder.PhoneNumber;

					var duplicateRow = new PromosetReportRow
					{
						SequenceNumber = sequenceNumber++,
						Address = duplicateOrder.DeliveryPointAddress,
						AddressCategory = duplicateOrder.DeliveryPointCategory,
						Phone = duplicateOrderPhones,
						Client = duplicateOrder.ClientName,
						Order = duplicateOrder.OrderId,
						OrderCreationDate = duplicateOrder.OrderCreateDate,
						OrderDeliveryDate = duplicateOrder.OrderDeliveryDate,
						Promoset = duplicateOrder.PromosetName,
						Author = duplicateOrder.AuthorName,
						IsRootRow = duplicateOrder.IsRoot
					};

					promosetReportRows.Add(duplicateRow);
				}
			}

			rows.AddRange(promosetReportRows);

			return rows;
		}

		private IEnumerable<int> GetSelectedPromotionalSets()
		{
			if(PromotionalSets.Any(x => x.IsSelected))
			{
				return PromotionalSets.Where(x => x.IsSelected).Select(x => x.Id);
			}

			return PromotionalSets.Select(x => x.Id);
		}

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}

	public class OrderWithPhoneDataNode
	{
		public int OrderId { get; set; }
		public int ClientId { get; set; }
		public int DeliveryPointId { get; set; }
		public DateTime? OrderCreateDate { get; set; }
		public DateTime? OrderDeliveryDate { get; set; }
		public int AuthorId { get; set; }
		public string AuthorName { get; set; }
		public string PhoneNumber { get; set; }
		public string PhoneDigitNumber { get; set; }
		public string ClientName { get; set; }
		public string DeliveryPointAddress { get; set; }
		public string DeliveryPointCategory { get; set; }
		public int PromosetId { get; set; }
		public string PromosetName { get; set; }
		public bool IsRoot { get; set; }
	}

	public class OrderDeliveryPointDataNode
	{
		public int OrderId { get; set; }
		public DateTime? OrderCreateDate { get; set; }
		public DateTime? OrderDeliveryDate { get; set; }
		public int AuthorId { get; set; }
		public int ClientId { get; set; }
		public int PromosetId { get; set; }
		public AddressDataNode AddressDataNode { get; set; }
		public string DeliveryPointCompiledAddress { get; set; }
		public int? DeliveryPointAddressCategoryId { get; set; }
	}

	public class AddressDataNode
	{
		public string City { get; set; }
		public string Street { get; set; }
		public string Building { get; set; }
		public string Room { get; set; }
	}
}
