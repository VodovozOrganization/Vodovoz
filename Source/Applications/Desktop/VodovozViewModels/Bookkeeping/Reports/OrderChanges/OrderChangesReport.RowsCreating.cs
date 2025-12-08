using DateTimeHelpers;
using MassTransit.Initializers;
using MoreLinq;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using QS.HistoryLog.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.HistoryChanges;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges
{
	public partial class OrderChangesReport
	{
		private const int _oldMonitoringQueriesTimeoutInSeconds = 180;

		private readonly IEnumerable<string> _cashAndCashlessPaymentTypesValues
			= new List<string> { "cash", "cashless", "Cash", "Cashless" };

		private readonly IEnumerable<string> _byCardAndPaidOnlinePaymentTypesValues
			= new List<string> { "ByCard", "PaidOnline" };

		private readonly int _paymentByCardFromSmsId;
		private readonly int _paymentByCardFromFastPaymentServiceId;

		private readonly bool _isPaymentTypeChangeTypeSelected;
		private readonly bool _isPriceChangeTypeSelected;
		private readonly bool _isOrderItemsCountChangeSelected;
		private readonly bool _isActualCountChangeSelected;

		private readonly bool _isSmsIssuesTypeSelected;
		private readonly bool _isQrIssuesTypeSelected;
		private readonly bool _isTerminalIssuesTypeSelected;
		private readonly bool _isManagersIssuesTypeSelected;

		private async Task SetReportRows(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			try
			{
				Rows = await CreateReportRows(uow, cancellationToken);
			}
			catch(Exception ex)
			{
				throw;
			}
		}

		private async Task<IEnumerable<OrderChangesReportRow>> CreateReportRows(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var rows = new List<OrderChangesReportRow>();
			var paymentTypesChangesData = new List<OrderChangesReportRow>();
			var orderItemChangesData = new List<OrderChangesReportRow>();

			if(IsOldMonitoring)
			{
				paymentTypesChangesData =
					(await GetOldMonitoringPaymentTypeChangesData(uow, cancellationToken))
					.OrderBy(x => x.OrderId)
					.ToList();

				orderItemChangesData =
					(await GetOldMonitoringOrderItemsChangesData(uow, cancellationToken))
					.OrderBy(x => x.OrderId)
					.ToList();
			}
			else
			{
				paymentTypesChangesData =
					(await GetPaymentTypeChangesData(uow, cancellationToken))
					.OrderBy(x => x.OrderId)
					.ToList();

				orderItemChangesData =
					(await GetOrderItemsChangesData(uow, cancellationToken))
					.OrderBy(x => x.OrderId)
					.ToList();
			}


			rows.AddRange(paymentTypesChangesData);
			rows.AddRange(orderItemChangesData);

			var counter = 1;
			rows.ForEach(row => row.RowNumber = counter++);

			return rows;
		}

		private async Task<IEnumerable<OrderChangesReportRow>> GetPaymentTypeChangesData(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var query =
				from changedEntity in uow.Session.Query<ChangedEntity>()

				join order in uow.Session.Query<Order>()
					on changedEntity.EntityId equals order.Id

				join counterpartyContract in uow.Session.Query<CounterpartyContract>()
					on order.Contract.Id equals counterpartyContract.Id

				join counterparty in uow.Session.Query<Counterparty>()
					on counterpartyContract.Counterparty.Id equals counterparty.Id

				join fieldChangePayType in uow.Session.Query<FieldChange>()
					on changedEntity.Id equals fieldChangePayType.Entity.Id

				join pf in uow.Session.Query<PaymentFrom>()
					on order.PaymentByCardFrom.Id equals pf.Id into paymentsByCardFrom
				from paymentByCardFrom in paymentsByCardFrom.DefaultIfEmpty()

				join fc in uow.Session.Query<FieldChange>()
					on changedEntity.Id equals fc.Entity.Id into fieldChanges
				from fieldChange in fieldChanges.DefaultIfEmpty()

				join sp in uow.Session.Query<SmsPayment>()
					on new { order.Id, SmsPaymentStatus = SmsPaymentStatus.Paid } equals new { sp.Order.Id, sp.SmsPaymentStatus } into smsPayments
				from smsPayment in smsPayments.DefaultIfEmpty()

				join fp in uow.Session.Query<FastPayment>()
					on new { order.Id, FastPaymentStatus = FastPaymentStatus.Performed } equals new { fp.Order.Id, fp.FastPaymentStatus } into fastPayments
				from fastPayment in fastPayments.DefaultIfEmpty()

				join hc in uow.Session.Query<ChangeSet>()
					on changedEntity.ChangeSet.Id equals hc.Id into changeSets
				from changeSet in changeSets.DefaultIfEmpty()

				join e in uow.Session.Query<Employee>()
					on changeSet.User.Id equals e.User.Id into authors
				from author in authors.DefaultIfEmpty()

				join rla in uow.Session.Query<RouteListItem>()
					on order.Id equals rla.Order.Id into routeListItems
				from routeListItem in routeListItems.DefaultIfEmpty()

				join rl in uow.Session.Query<RouteList>()
					on routeListItem.RouteList.Id equals rl.Id into routeLists
				from routeList in routeLists.DefaultIfEmpty()

				join e2 in uow.Session.Query<Employee>()
					on routeList.Driver.Id equals e2.Id into routeListDrivers
				from driver in routeListDrivers.DefaultIfEmpty()

				let smsNew =
				(from subChangedEntity in uow.Session.Query<ChangedEntity>()
				 join subFieldChange in uow.Session.Query<FieldChange>() on subChangedEntity.EntityId equals subFieldChange.Entity.Id
				 where
					 subFieldChange.Path == "PaymentBySms"
					 && (subFieldChange.Type == FieldChangeType.Added || subFieldChange.Type == FieldChangeType.Changed)
					 && subChangedEntity.EntityId == order.Id
					 && subChangedEntity.ChangeTime < order.TimeDelivered
				 orderby subFieldChange.Id descending
				 select subFieldChange.NewValue)
				 .FirstOrDefault() ?? string.Empty

				let qrNew =
				(from subChangedEntity in uow.Session.Query<ChangedEntity>()
				 join subFieldChange in uow.Session.Query<FieldChange>() on subChangedEntity.EntityId equals subFieldChange.Entity.Id
				 where
					 subFieldChange.Path == "PaymentByQr"
					 && (subFieldChange.Type == FieldChangeType.Added || subFieldChange.Type == FieldChangeType.Changed)
					 && subChangedEntity.EntityId == order.Id
					 && subChangedEntity.ChangeTime < order.TimeDelivered
				 orderby subFieldChange.Id descending
				 select subFieldChange.NewValue)
				 .FirstOrDefault() ?? string.Empty

				let orderSum =
				(decimal?)(from oi in uow.Session.Query<OrderItem>()
						   where oi.Order.Id == order.Id
						   select ((oi.ActualCount == null ? oi.Count : oi.ActualCount.Value) * oi.Price - oi.DiscountMoney))
						   .Sum()

				let lastTimeDeliveredChange =
					(from subChangedEntity in uow.Session.Query<ChangedEntity>()
					 join subFieldChange in uow.Session.Query<FieldChange>()
						on subChangedEntity.Id equals subFieldChange.Entity.Id
					 where subChangedEntity.EntityId == order.Id
						&& subChangedEntity.EntityClassName == "Order"
						&& subFieldChange.Path == "TimeDelivered"
						&& subFieldChange.NewValue != null
					 select (DateTime?)subChangedEntity.ChangeTime)
					.Max()

				let timeDelivered = lastTimeDeliveredChange ?? order.TimeDelivered

				where
					changedEntity.EntityClassName == "Order"
					&& changedEntity.ChangeTime >= _startDate
					&& changedEntity.ChangeTime <= _endDate.LatestDayTime()
					&& changedEntity.Operation == EntityChangeOperation.Change
					&& counterpartyContract.Organization.Id == _selectedOrganization.Id
					&& fieldChangePayType.Type == FieldChangeType.Changed
					&& fieldChangePayType.Path == "PaymentType"
					&& fieldChange.Type == FieldChangeType.Changed
					&& fieldChange.Path == "PaymentType"
					&& (_selectedIssueTypes.Any() || _isPaymentTypeChangeTypeSelected)

					&& changedEntity.ChangeTime > timeDelivered

					&& (((_cashAndCashlessPaymentTypesValues.Contains(fieldChangePayType.NewValue)
								|| _cashAndCashlessPaymentTypesValues.Contains(fieldChangePayType.OldValue))
							&& _isPaymentTypeChangeTypeSelected)
						|| ((fieldChangePayType.NewValue == "Terminal" || fieldChangePayType.OldValue == "Terminal")
							&& _isTerminalIssuesTypeSelected)
						|| ((_byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.NewValue)
								|| _byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.OldValue))
							&& _isManagersIssuesTypeSelected
							&& order.PaymentByCardFrom.Id != null
							&& order.PaymentByCardFrom.Id != _paymentByCardFromSmsId)
						|| (_byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.NewValue)
							&& order.PaymentByCardFrom.Id == _paymentByCardFromSmsId
							&& _isSmsIssuesTypeSelected)
						|| (_byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.NewValue)
							&& order.PaymentByCardFrom.Id == _paymentByCardFromFastPaymentServiceId
							&& _isQrIssuesTypeSelected))

				select new OrderChangesReportRow
				{
					ChangedEntityId = changedEntity.Id,
					RowNumber = 0,
					CounterpartyFullName = counterparty.FullName,
					CounterpartyPersonType = counterparty.PersonType,
					CounterpartyInn = counterparty.INN,
					DriverPhoneComment = order.CommentManager ?? string.Empty,
					PaymentDate = smsPayment == null ? fastPayment.PaidDate : smsPayment.PaidDate,
					OrderId = order.Id,
					OrderSum = orderSum,
					TimeDelivered = order.TimeDelivered,
					ChangeTime = changedEntity.ChangeTime,
					NomenclatureName = string.Empty,
					NomenclatureOfficialName = string.Empty,
					ChangeOperation = changedEntity.Operation,
					OldValue =
						changedEntity.Operation == EntityChangeOperation.Create || changedEntity.Operation == EntityChangeOperation.Delete
						? string.Empty
						: GetPaymentTypeChangedValue(fieldChange.OldValue, paymentByCardFrom, order.PaymentByTerminalSource),
					NewValue =
						changedEntity.Operation == EntityChangeOperation.Create || changedEntity.Operation == EntityChangeOperation.Delete
						? string.Empty
						: GetPaymentTypeChangedValue(fieldChange.NewValue, paymentByCardFrom, order.PaymentByTerminalSource),
					Driver = driver == null ? string.Empty : driver.ShortName,
					Author = author == null ? string.Empty : author.ShortName,
					SmsNew = smsNew,
					QrNew = qrNew,
				};

			return await query.ToListAsync(cancellationToken);
		}

		private async Task<IEnumerable<OrderChangesReportRow>> GetOldMonitoringPaymentTypeChangesData(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var query =
				from changedEntity in uow.Session.Query<ArchivedChangedEntity>()

				join order in uow.Session.Query<Order>()
					on changedEntity.EntityId equals order.Id

				join counterpartyContract in uow.Session.Query<CounterpartyContract>()
					on order.Contract.Id equals counterpartyContract.Id

				join counterparty in uow.Session.Query<Counterparty>()
					on counterpartyContract.Counterparty.Id equals counterparty.Id

				join fieldChangePayType in uow.Session.Query<ArchivedFieldChange>()
					on changedEntity.Id equals fieldChangePayType.Entity.Id

				join pf in uow.Session.Query<PaymentFrom>()
					on order.PaymentByCardFrom.Id equals pf.Id into paymentsByCardFrom
				from paymentByCardFrom in paymentsByCardFrom.DefaultIfEmpty()

				join fc in uow.Session.Query<ArchivedFieldChange>()
					on changedEntity.Id equals fc.Entity.Id into fieldChanges
				from fieldChange in fieldChanges.DefaultIfEmpty()

				join sp in uow.Session.Query<SmsPayment>()
					on new { order.Id, SmsPaymentStatus = SmsPaymentStatus.Paid } equals new { sp.Order.Id, sp.SmsPaymentStatus } into smsPayments
				from smsPayment in smsPayments.DefaultIfEmpty()

				join fp in uow.Session.Query<FastPayment>()
					on new { order.Id, FastPaymentStatus = FastPaymentStatus.Performed } equals new { fp.Order.Id, fp.FastPaymentStatus } into fastPayments
				from fastPayment in fastPayments.DefaultIfEmpty()

				join hc in uow.Session.Query<ArchivedChangeSet>()
					on changedEntity.ChangeSet.Id equals hc.Id into changeSets
				from changeSet in changeSets.DefaultIfEmpty()

				join e in uow.Session.Query<Employee>()
					on changeSet.User.Id equals e.User.Id into authors
				from author in authors.DefaultIfEmpty()

				join rla in uow.Session.Query<RouteListItem>()
					on order.Id equals rla.Order.Id into routeListItems
				from routeListItem in routeListItems.DefaultIfEmpty()

				join rl in uow.Session.Query<RouteList>()
					on routeListItem.RouteList.Id equals rl.Id into routeLists
				from routeList in routeLists.DefaultIfEmpty()

				join e2 in uow.Session.Query<Employee>()
					on routeList.Driver.Id equals e2.Id into routeListDrivers
				from driver in routeListDrivers.DefaultIfEmpty()

				let smsNew =
				(from subChangedEntity in uow.Session.Query<ArchivedChangedEntity>()
				 join subFieldChange in uow.Session.Query<ArchivedFieldChange>() on subChangedEntity.EntityId equals subFieldChange.Entity.Id
				 where
					 subFieldChange.Path == "PaymentBySms"
					 && (subFieldChange.Type == FieldChangeType.Added || subFieldChange.Type == FieldChangeType.Changed)
					 && subChangedEntity.EntityId == order.Id
					 && subChangedEntity.ChangeTime < order.TimeDelivered
				 orderby subFieldChange.Id descending
				 select subFieldChange.NewValue)
				 .FirstOrDefault() ?? string.Empty

				let qrNew =
				(from subChangedEntity in uow.Session.Query<ArchivedChangedEntity>()
				 join subFieldChange in uow.Session.Query<ArchivedFieldChange>() on subChangedEntity.EntityId equals subFieldChange.Entity.Id
				 where
					 subFieldChange.Path == "PaymentByQr"
					 && (subFieldChange.Type == FieldChangeType.Added || subFieldChange.Type == FieldChangeType.Changed)
					 && subChangedEntity.EntityId == order.Id
					 && subChangedEntity.ChangeTime < order.TimeDelivered
				 orderby subFieldChange.Id descending
				 select subFieldChange.NewValue)
				 .FirstOrDefault() ?? string.Empty

				let orderSum =
				(decimal?)(from oi in uow.Session.Query<OrderItem>()
						   where oi.Order.Id == order.Id
						   select ((oi.ActualCount == null ? oi.Count : oi.ActualCount.Value) * oi.Price - oi.DiscountMoney))
						   .Sum()

				where
					changedEntity.ChangeTime > order.TimeDelivered
					&& changedEntity.EntityClassName == "Order"
					&& changedEntity.ChangeTime >= _startDate
					&& changedEntity.ChangeTime <= _endDate.LatestDayTime()
					&& changedEntity.Operation == EntityChangeOperation.Change
					&& counterpartyContract.Organization.Id == _selectedOrganization.Id
					&& fieldChangePayType.Type == FieldChangeType.Changed
					&& fieldChangePayType.Path == "PaymentType"
					&& fieldChange.Type == FieldChangeType.Changed
					&& fieldChange.Path == "PaymentType"
					&& (_selectedIssueTypes.Any() || _isPaymentTypeChangeTypeSelected)

					&& (((_cashAndCashlessPaymentTypesValues.Contains(fieldChangePayType.NewValue)
								|| _cashAndCashlessPaymentTypesValues.Contains(fieldChangePayType.OldValue))
							&& _isPaymentTypeChangeTypeSelected)
						|| ((fieldChangePayType.NewValue == "Terminal" || fieldChangePayType.OldValue == "Terminal")
							&& _isTerminalIssuesTypeSelected)
						|| ((_byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.NewValue)
								|| _byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.OldValue))
							&& _isManagersIssuesTypeSelected
							&& order.PaymentByCardFrom.Id != null
							&& order.PaymentByCardFrom.Id != _paymentByCardFromSmsId)
						|| (_byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.NewValue)
							&& order.PaymentByCardFrom.Id == _paymentByCardFromSmsId
							&& _isSmsIssuesTypeSelected)
						|| (_byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.NewValue)
							&& order.PaymentByCardFrom.Id == _paymentByCardFromFastPaymentServiceId
							&& _isQrIssuesTypeSelected))

				select new OrderChangesReportRow
				{
					ChangedEntityId = changedEntity.Id,
					RowNumber = 0,
					CounterpartyFullName = counterparty.FullName,
					CounterpartyPersonType = counterparty.PersonType,
					CounterpartyInn = counterparty.INN,
					DriverPhoneComment = order.CommentManager ?? string.Empty,
					PaymentDate = smsPayment == null ? fastPayment.PaidDate : smsPayment.PaidDate,
					OrderId = order.Id,
					OrderSum = orderSum,
					TimeDelivered = order.TimeDelivered,
					ChangeTime = changedEntity.ChangeTime,
					NomenclatureName = string.Empty,
					NomenclatureOfficialName = string.Empty,
					ChangeOperation = changedEntity.Operation,
					OldValue =
						changedEntity.Operation == EntityChangeOperation.Create || changedEntity.Operation == EntityChangeOperation.Delete
						? string.Empty
						: GetPaymentTypeChangedValue(fieldChange.OldValue, paymentByCardFrom, order.PaymentByTerminalSource),
					NewValue =
						changedEntity.Operation == EntityChangeOperation.Create || changedEntity.Operation == EntityChangeOperation.Delete
						? string.Empty
						: GetPaymentTypeChangedValue(fieldChange.NewValue, paymentByCardFrom, order.PaymentByTerminalSource),
					Driver = driver == null ? string.Empty : driver.ShortName,
					Author = author == null ? string.Empty : author.ShortName,
					SmsNew = smsNew,
					QrNew = qrNew,
				};

			return await query.WithOptions(options => options.SetTimeout(_oldMonitoringQueriesTimeoutInSeconds)).ToListAsync(cancellationToken);
		}

		private async Task<IEnumerable<OrderChangesReportRow>> GetOrderItemsChangesData(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var operations = new List<EntityChangeOperation>
			{
				EntityChangeOperation.Create,
				EntityChangeOperation.Change,
				EntityChangeOperation.Delete
			};

			var query =
				from changedEntity in uow.Session.Query<ChangedEntity>()

				join orderItem in uow.Session.Query<OrderItem>()
					on changedEntity.EntityId equals orderItem.Id

				join order in uow.Session.Query<Order>()
					on orderItem.Order.Id equals order.Id

				join counterpartyContract in uow.Session.Query<CounterpartyContract>()
					on order.Contract.Id equals counterpartyContract.Id

				join counterparty in uow.Session.Query<Counterparty>()
					on counterpartyContract.Counterparty.Id equals counterparty.Id

				join pf in uow.Session.Query<PaymentFrom>()
					on order.PaymentByCardFrom.Id equals pf.Id into paymentsByCardFrom
				from paymentByCardFrom in paymentsByCardFrom.DefaultIfEmpty()

				join fc in uow.Session.Query<FieldChange>()
					on changedEntity.Id equals fc.Entity.Id into fieldChanges
				from fieldChange in fieldChanges.DefaultIfEmpty()

				join sp in uow.Session.Query<SmsPayment>()
					on new { order.Id, SmsPaymentStatus = SmsPaymentStatus.Paid } equals new { sp.Order.Id, sp.SmsPaymentStatus } into smsPayments
				from smsPayment in smsPayments.DefaultIfEmpty()

				join fp in uow.Session.Query<FastPayment>()
					on new { order.Id, FastPaymentStatus = FastPaymentStatus.Performed } equals new { fp.Order.Id, fp.FastPaymentStatus } into fastPayments
				from fastPayment in fastPayments.DefaultIfEmpty()

				join hc in uow.Session.Query<ChangeSet>()
					on changedEntity.ChangeSet.Id equals hc.Id into changeSets
				from changeSet in changeSets.DefaultIfEmpty()

				join e in uow.Session.Query<Employee>()
					on changeSet.User.Id equals e.User.Id into authors
				from author in authors.DefaultIfEmpty()

				join n in uow.Session.Query<Nomenclature>()
					on orderItem.Nomenclature.Id equals n.Id into nomenclatures
				from nomenclature in nomenclatures.DefaultIfEmpty()

				join rla in uow.Session.Query<RouteListItem>()
					on order.Id equals rla.Order.Id into routeListItems
				from routeListItem in routeListItems.DefaultIfEmpty()

				join rl in uow.Session.Query<RouteList>()
					on routeListItem.RouteList.Id equals rl.Id into routeLists
				from routeList in routeLists.DefaultIfEmpty()

				join e2 in uow.Session.Query<Employee>()
					on routeList.Driver.Id equals e2.Id into routeListDrivers
				from driver in routeListDrivers.DefaultIfEmpty()

				let smsNew =
				(from subChangedEntity in uow.Session.Query<ChangedEntity>()
				 join subFieldChange in uow.Session.Query<FieldChange>() on subChangedEntity.EntityId equals subFieldChange.Entity.Id
				 where
					 subFieldChange.Path == "PaymentBySms"
					 && (subFieldChange.Type == FieldChangeType.Added || subFieldChange.Type == FieldChangeType.Changed)
					 && subChangedEntity.EntityId == order.Id
					 && subChangedEntity.ChangeTime < order.TimeDelivered
				 orderby subFieldChange.Id descending
				 select subFieldChange.NewValue)
				 .FirstOrDefault() ?? string.Empty

				let qrNew =
				(from subChangedEntity in uow.Session.Query<ChangedEntity>()
				 join subFieldChange in uow.Session.Query<FieldChange>() on subChangedEntity.EntityId equals subFieldChange.Entity.Id
				 where
					 subFieldChange.Path == "PaymentByQr"
					 && (subFieldChange.Type == FieldChangeType.Added || subFieldChange.Type == FieldChangeType.Changed)
					 && subChangedEntity.EntityId == order.Id
					 && subChangedEntity.ChangeTime < order.TimeDelivered
				 orderby subFieldChange.Id descending
				 select subFieldChange.NewValue)
				 .FirstOrDefault() ?? string.Empty

				let orderSum =
				(decimal?)(from oi in uow.Session.Query<OrderItem>()
						   where oi.Order.Id == order.Id
						   select ((oi.ActualCount == null ? oi.Count : oi.ActualCount.Value) * oi.Price - oi.DiscountMoney))
						   .Sum()

				let paymentTypeChangesOrderIds = GetPaymentTypeChangesOrderIds(uow)

				where
					changedEntity.EntityClassName == "OrderItem"
					&& paymentTypeChangesOrderIds.Contains(order.Id)
					&& changedEntity.ChangeTime >= _startDate
					&& changedEntity.ChangeTime <= _endDate.LatestDayTime()
					&& operations.Contains(changedEntity.Operation)
					&& counterpartyContract.Organization.Id == _selectedOrganization.Id
					&& changedEntity.ChangeTime > order.TimeDelivered
					&& (((fieldChange.Type == FieldChangeType.Changed || fieldChange.Type == FieldChangeType.Removed)
							&& (fieldChange.Path == "Price" || fieldChange.Path == "ActualCount")
							&& _isPriceChangeTypeSelected
							&& _isActualCountChangeSelected)
						|| ((changedEntity.Operation == EntityChangeOperation.Create || changedEntity.Operation == EntityChangeOperation.Delete)
							&& fieldChange.Path == "Price"
							&& _isOrderItemsCountChangeSelected))

				select new OrderChangesReportRow
				{
					ChangedEntityId = changedEntity.Id,
					RowNumber = 0,
					CounterpartyFullName = counterparty.FullName,
					CounterpartyPersonType = counterparty.PersonType,
					CounterpartyInn = counterparty.INN,
					DriverPhoneComment = order.CommentManager ?? string.Empty,
					PaymentDate = smsPayment == null ? fastPayment.PaidDate : smsPayment.PaidDate,
					OrderId = order.Id,
					OrderSum = orderSum,
					TimeDelivered = order.TimeDelivered,
					ChangeTime = changedEntity.ChangeTime,
					NomenclatureName = nomenclature == null ? string.Empty : nomenclature.Name,
					NomenclatureOfficialName = nomenclature == null ? string.Empty : nomenclature.OfficialName,
					ChangeOperation = changedEntity.Operation,
					OldValue =
						changedEntity.Operation == EntityChangeOperation.Create || changedEntity.Operation == EntityChangeOperation.Delete
						? string.Empty
						: GetPaymentTypeChangedValue(fieldChange.OldValue, paymentByCardFrom, order.PaymentByTerminalSource),
					NewValue =
						changedEntity.Operation == EntityChangeOperation.Create || changedEntity.Operation == EntityChangeOperation.Delete
						? string.Empty
						: GetPaymentTypeChangedValue(fieldChange.NewValue, paymentByCardFrom, order.PaymentByTerminalSource),
					Driver = driver == null ? string.Empty : driver.ShortName,
					Author = author == null ? string.Empty : author.ShortName,
					SmsNew = smsNew,
					QrNew = qrNew,
				};

			var changesData = await query.ToListAsync(cancellationToken);

			return changesData;
		}

		private IQueryable<int> GetPaymentTypeChangesOrderIds(IUnitOfWork uow)
		{
			var query =
				from changedEntity in uow.Session.Query<ChangedEntity>()
				join order in uow.Session.Query<Order>() on changedEntity.EntityId equals order.Id
				join fieldChangePayType in uow.Session.Query<FieldChange>() on changedEntity.Id equals fieldChangePayType.Entity.Id
				where
					changedEntity.EntityClassName == "Order"
					&& (changedEntity.Operation == EntityChangeOperation.Create || changedEntity.Operation == EntityChangeOperation.Change)
					&& (fieldChangePayType.Type == FieldChangeType.Added || fieldChangePayType.Type == FieldChangeType.Changed)
					&& fieldChangePayType.Path == "PaymentType"

					&& (((_cashAndCashlessPaymentTypesValues.Contains(fieldChangePayType.NewValue)
								|| _cashAndCashlessPaymentTypesValues.Contains(fieldChangePayType.OldValue))
							&& _isPaymentTypeChangeTypeSelected)
						|| ((fieldChangePayType.NewValue == "Terminal" || fieldChangePayType.OldValue == "Terminal")
							&& _isTerminalIssuesTypeSelected)
						|| ((_byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.NewValue)
								|| _byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.OldValue))
							&& _isManagersIssuesTypeSelected
							&& order.PaymentByCardFrom.Id != null
							&& order.PaymentByCardFrom.Id != _paymentByCardFromSmsId)
						|| (_byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.NewValue)
							&& order.PaymentByCardFrom.Id == _paymentByCardFromSmsId
							&& _isSmsIssuesTypeSelected)
						|| (_byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.NewValue)
							&& order.PaymentByCardFrom.Id == _paymentByCardFromFastPaymentServiceId
							&& _isQrIssuesTypeSelected)
							)

				select order.Id;

			return query;
		}

		private async Task<IEnumerable<OrderChangesReportRow>> GetOldMonitoringOrderItemsChangesData(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var operations = new List<EntityChangeOperation>
			{
				EntityChangeOperation.Create,
				EntityChangeOperation.Change,
				EntityChangeOperation.Delete
			};

			var query =
				from changedEntity in uow.Session.Query<ArchivedChangedEntity>()

				join orderItem in uow.Session.Query<OrderItem>()
					on changedEntity.EntityId equals orderItem.Id

				join order in uow.Session.Query<Order>()
					on orderItem.Order.Id equals order.Id

				join counterpartyContract in uow.Session.Query<CounterpartyContract>()
					on order.Contract.Id equals counterpartyContract.Id

				join counterparty in uow.Session.Query<Counterparty>()
					on counterpartyContract.Counterparty.Id equals counterparty.Id

				join pf in uow.Session.Query<PaymentFrom>()
					on order.PaymentByCardFrom.Id equals pf.Id into paymentsByCardFrom
				from paymentByCardFrom in paymentsByCardFrom.DefaultIfEmpty()

				join fc in uow.Session.Query<ArchivedFieldChange>()
					on changedEntity.Id equals fc.Entity.Id into fieldChanges
				from fieldChange in fieldChanges.DefaultIfEmpty()

				join sp in uow.Session.Query<SmsPayment>()
					on new { order.Id, SmsPaymentStatus = SmsPaymentStatus.Paid } equals new { sp.Order.Id, sp.SmsPaymentStatus } into smsPayments
				from smsPayment in smsPayments.DefaultIfEmpty()

				join fp in uow.Session.Query<FastPayment>()
					on new { order.Id, FastPaymentStatus = FastPaymentStatus.Performed } equals new { fp.Order.Id, fp.FastPaymentStatus } into fastPayments
				from fastPayment in fastPayments.DefaultIfEmpty()

				join hc in uow.Session.Query<ArchivedChangeSet>()
					on changedEntity.ChangeSet.Id equals hc.Id into changeSets
				from changeSet in changeSets.DefaultIfEmpty()

				join e in uow.Session.Query<Employee>()
					on changeSet.User.Id equals e.User.Id into authors
				from author in authors.DefaultIfEmpty()

				join n in uow.Session.Query<Nomenclature>()
					on orderItem.Nomenclature.Id equals n.Id into nomenclatures
				from nomenclature in nomenclatures.DefaultIfEmpty()

				join rla in uow.Session.Query<RouteListItem>()
					on order.Id equals rla.Order.Id into routeListItems
				from routeListItem in routeListItems.DefaultIfEmpty()

				join rl in uow.Session.Query<RouteList>()
					on routeListItem.RouteList.Id equals rl.Id into routeLists
				from routeList in routeLists.DefaultIfEmpty()

				join e2 in uow.Session.Query<Employee>()
					on routeList.Driver.Id equals e2.Id into routeListDrivers
				from driver in routeListDrivers.DefaultIfEmpty()

				let smsNew =
				(from subChangedEntity in uow.Session.Query<ArchivedChangedEntity>()
				 join subFieldChange in uow.Session.Query<ArchivedFieldChange>() on subChangedEntity.EntityId equals subFieldChange.Entity.Id
				 where
					 subFieldChange.Path == "PaymentBySms"
					 && (subFieldChange.Type == FieldChangeType.Added || subFieldChange.Type == FieldChangeType.Changed)
					 && subChangedEntity.EntityId == order.Id
					 && subChangedEntity.ChangeTime < order.TimeDelivered
				 orderby subFieldChange.Id descending
				 select subFieldChange.NewValue)
				 .FirstOrDefault() ?? string.Empty

				let qrNew =
				(from subChangedEntity in uow.Session.Query<ArchivedChangedEntity>()
				 join subFieldChange in uow.Session.Query<ArchivedFieldChange>() on subChangedEntity.EntityId equals subFieldChange.Entity.Id
				 where
					 subFieldChange.Path == "PaymentByQr"
					 && (subFieldChange.Type == FieldChangeType.Added || subFieldChange.Type == FieldChangeType.Changed)
					 && subChangedEntity.EntityId == order.Id
					 && subChangedEntity.ChangeTime < order.TimeDelivered
				 orderby subFieldChange.Id descending
				 select subFieldChange.NewValue)
				 .FirstOrDefault() ?? string.Empty

				let orderSum =
				(decimal?)(from oi in uow.Session.Query<OrderItem>()
						   where oi.Order.Id == order.Id
						   select ((oi.ActualCount == null ? oi.Count : oi.ActualCount.Value) * oi.Price - oi.DiscountMoney))
						   .Sum()

				where
					changedEntity.EntityClassName == "OrderItem"
					&& changedEntity.ChangeTime >= _startDate
					&& changedEntity.ChangeTime <= _endDate.LatestDayTime()
					&& operations.Contains(changedEntity.Operation)
					&& counterpartyContract.Organization.Id == _selectedOrganization.Id
					&& changedEntity.ChangeTime > order.TimeDelivered
					&& (((fieldChange.Type == FieldChangeType.Changed || fieldChange.Type == FieldChangeType.Removed)
							&& (fieldChange.Path == "Price" || fieldChange.Path == "ActualCount")
							&& _isPriceChangeTypeSelected
							&& _isActualCountChangeSelected)
						|| ((changedEntity.Operation == EntityChangeOperation.Create || changedEntity.Operation == EntityChangeOperation.Delete)
							&& fieldChange.Path == "Price"
							&& _isOrderItemsCountChangeSelected))

				select new OrderChangesReportRow
				{
					ChangedEntityId = changedEntity.Id,
					RowNumber = 0,
					CounterpartyFullName = counterparty.FullName,
					CounterpartyPersonType = counterparty.PersonType,
					CounterpartyInn = counterparty.INN,
					DriverPhoneComment = order.CommentManager ?? string.Empty,
					PaymentDate = smsPayment == null ? fastPayment.PaidDate : smsPayment.PaidDate,
					OrderId = order.Id,
					OrderSum = orderSum,
					TimeDelivered = order.TimeDelivered,
					ChangeTime = changedEntity.ChangeTime,
					NomenclatureName = nomenclature == null ? string.Empty : nomenclature.Name,
					NomenclatureOfficialName = nomenclature == null ? string.Empty : nomenclature.OfficialName,
					ChangeOperation = changedEntity.Operation,
					OldValue =
						changedEntity.Operation == EntityChangeOperation.Create || changedEntity.Operation == EntityChangeOperation.Delete
						? string.Empty
						: GetPaymentTypeChangedValue(fieldChange.OldValue, paymentByCardFrom, order.PaymentByTerminalSource),
					NewValue =
						changedEntity.Operation == EntityChangeOperation.Create || changedEntity.Operation == EntityChangeOperation.Delete
						? string.Empty
						: GetPaymentTypeChangedValue(fieldChange.NewValue, paymentByCardFrom, order.PaymentByTerminalSource),
					Driver = driver == null ? string.Empty : driver.ShortName,
					Author = author == null ? string.Empty : author.ShortName,
					SmsNew = smsNew,
					QrNew = qrNew,
				};

			var changesData = await query.WithOptions(options => options.SetTimeout(_oldMonitoringQueriesTimeoutInSeconds)).ToListAsync(cancellationToken);

			var paymentTypeChangedOrderIds =
				await GetOldMonitoringPaymentTypeChangesOrderIds(uow, changesData.Select(x => x.OrderId).Distinct(), cancellationToken);

			return changesData.Where(x => paymentTypeChangedOrderIds.Contains(x.OrderId));
		}

		private async Task<IEnumerable<int>> GetOldMonitoringPaymentTypeChangesOrderIds(IUnitOfWork uow, IEnumerable<int> orderIds, CancellationToken cancellationToken)
		{
			var query =
				from changedEntity in uow.Session.Query<ArchivedChangedEntity>()
				join order in uow.Session.Query<Order>() on changedEntity.EntityId equals order.Id
				join fieldChangePayType in uow.Session.Query<ArchivedFieldChange>() on changedEntity.Id equals fieldChangePayType.Entity.Id
				where
					changedEntity.EntityClassName == "Order"
					&& (changedEntity.Operation == EntityChangeOperation.Create || changedEntity.Operation == EntityChangeOperation.Change)
					&& (fieldChangePayType.Type == FieldChangeType.Added || fieldChangePayType.Type == FieldChangeType.Changed)
					&& fieldChangePayType.Path == "PaymentType"
					&& orderIds.Contains(order.Id)

					&& (((_cashAndCashlessPaymentTypesValues.Contains(fieldChangePayType.NewValue)
								|| _cashAndCashlessPaymentTypesValues.Contains(fieldChangePayType.OldValue))
							&& _isPaymentTypeChangeTypeSelected)
						|| ((fieldChangePayType.NewValue == "Terminal" || fieldChangePayType.OldValue == "Terminal")
							&& _isTerminalIssuesTypeSelected)
						|| ((_byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.NewValue)
								|| _byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.OldValue))
							&& _isManagersIssuesTypeSelected
							&& order.PaymentByCardFrom.Id != null
							&& order.PaymentByCardFrom.Id != _paymentByCardFromSmsId)
						|| (_byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.NewValue)
							&& order.PaymentByCardFrom.Id == _paymentByCardFromSmsId
							&& _isSmsIssuesTypeSelected)
						|| (_byCardAndPaidOnlinePaymentTypesValues.Contains(fieldChangePayType.NewValue)
							&& order.PaymentByCardFrom.Id == _paymentByCardFromFastPaymentServiceId
							&& _isQrIssuesTypeSelected)
							)

				select order.Id;

			var oderdIds = (await query.WithOptions(options => options.SetTimeout(_oldMonitoringQueriesTimeoutInSeconds)).ToListAsync(cancellationToken)).Distinct().ToList();

			return oderdIds;
		}

		private string GetPaymentTypeChangedValue(string paymentTypeValue, PaymentFrom paymentByCardFrom, PaymentByTerminalSource? terminalSubtype)
		{
			switch(paymentTypeValue)
			{
				case "cash":
				case "Cash":
					return PaymentType.Cash.GetEnumDisplayName();
				case "cashless":
				case "Cashless":
					return PaymentType.Cashless.GetEnumDisplayName();
				case "barter":
				case "Barter":
					return PaymentType.Barter.GetEnumDisplayName();
				case "DriverApplicationQR":
					return PaymentType.DriverApplicationQR.GetEnumDisplayName();
				case "SmsQR":
					return PaymentType.SmsQR.GetEnumDisplayName();
				case "ByCard":
					return $"По карте {paymentByCardFrom?.Name}";
				case "PaidOnline":
					return $"{PaymentType.PaidOnline.GetEnumDisplayName()} {paymentByCardFrom?.Name}";
				case "ContractDoc":
				case "ContractDocumentation":
					return PaymentType.ContractDocumentation.GetEnumDisplayName();
				case "BeveragesWorld":
					return "Мир напитков";
				case "Terminal":
					return $"{PaymentType.Terminal.GetEnumDisplayName()}, {terminalSubtype?.GetEnumDisplayName()}";
				default:
					return paymentTypeValue;
			}
		}
	}
}
