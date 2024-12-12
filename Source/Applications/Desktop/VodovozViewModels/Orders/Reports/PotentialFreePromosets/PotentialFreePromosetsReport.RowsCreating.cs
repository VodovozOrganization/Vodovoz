using MoreLinq;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets
{
	public partial class PotentialFreePromosetsReport
	{

		private async Task SetReportRows(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			try
			{
				var addressDupliceteRows = await GetAddressDuplicates(uow, cancellationToken);
				var phoneDuplicatesRows = await GetPhoneDuplicates(uow, cancellationToken);

				Rows = new List<PromosetReportRow>();
				Rows.AddRange(addressDupliceteRows);
				Rows.AddRange(phoneDuplicatesRows);
			}
			catch(Exception ex)
			{
				throw;
			}

			SetRowsSequenceNumbers();
		}

		private async Task<IList<PromosetReportRow>> GetAddressDuplicates(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var deliveryPointsHavingAddresses =
				await (from order in uow.Session.Query<Order>()
					   join orderItem in uow.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
					   join deliveryPoint in uow.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals deliveryPoint.Id
					   join deliveryPoint2 in uow.Session.Query<DeliveryPoint>()
					   on new { deliveryPoint.City, deliveryPoint.Street, deliveryPoint.Building, deliveryPoint.Room }
					   equals new { deliveryPoint2.City, deliveryPoint2.Street, deliveryPoint2.Building, deliveryPoint2.Room }
					   where
					   order.CreateDate >= _startDate
					   && order.CreateDate < _endDate.Date.AddDays(1)
					   && _selectedPromosets.Contains(orderItem.PromoSet.Id)
					   && !_notDeliveredOrderStatuses.Contains(order.OrderStatus)
					   select deliveryPoint2.Id)
					   .Distinct()
					   .ToListAsync();

			cancellationToken.ThrowIfCancellationRequested();

			var ordersHavingPromosetAndDeliveryPoint =
				await (from order in uow.Session.Query<Order>()
					   join orderItem in uow.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
					   join deliveryPoint in uow.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals deliveryPoint.Id
					   join author in uow.Session.Query<Employee>() on order.Author.Id equals author.Id
					   join client in uow.Session.Query<Counterparty>() on order.Client.Id equals client.Id
					   join promoset in uow.Session.Query<PromotionalSet>() on orderItem.PromoSet.Id equals promoset.Id
					   join dpc in uow.Session.Query<DeliveryPointCategory>() on deliveryPoint.Category.Id equals dpc.Id into deliveryPointCategories
					   from deliveryPointCategory in deliveryPointCategories.DefaultIfEmpty()
					   where
					   !_notDeliveredOrderStatuses.Contains(order.OrderStatus)
					   && deliveryPointsHavingAddresses.Contains(order.DeliveryPoint.Id)
					   select new OrderDeliveryPointDataNode
					   {
						   OrderId = order.Id,
						   OrderCreateDate = order.CreateDate,
						   OrderDeliveryDate = order.DeliveryDate,
						   AuthorId = order.Author.Id,
						   AuthorName = author.ShortName,
						   ClientId = order.Client.Id,
						   ClientName = client.Name,
						   PromosetId = orderItem.PromoSet.Id,
						   PromosetName = promoset.Name,
						   AddressDataNode = new AddressDataNode
						   {
							   City = deliveryPoint.City.ToLower().Trim(),
							   Street = deliveryPoint.Street.ToLower().Trim(),
							   Building = deliveryPoint.Building.ToLower().Trim(),
							   Room = deliveryPoint.Room.ToLower().Trim()
						   },
						   DeliveryPointCompiledAddress = deliveryPoint.CompiledAddress,
						   DeliveryPointAddressCategoryId = deliveryPoint.Category.Id,
						   DeliveryPointAddressCategoryName = deliveryPointCategory == null ? string.Empty : deliveryPointCategory.Name
					   })
			.Distinct()
			.ToListAsync();

			cancellationToken.ThrowIfCancellationRequested();

			var ordersHavingSelectedPromosetsForPeriod =
				(from ordersData in ordersHavingPromosetAndDeliveryPoint
				 where
				 ordersData.OrderCreateDate >= _startDate
				 && ordersData.OrderCreateDate < _endDate.Date.AddDays(1)
				 && _selectedPromosets.Contains(ordersData.PromosetId)
				 select ordersData)
				 .Distinct()
				 .ToList();

			var groupedByRootOrdersAndDuplicates =
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

			var promosetReportRows = new List<PromosetReportRow>();

			foreach(var ordersData in groupedByRootOrdersAndDuplicates)
			{
				var rootRow = CreatePromosetReportRow(
					ordersData.Key,
					true);

				promosetReportRows.Add(rootRow);

				foreach(var duplicate in ordersData.Value)
				{
					var duplicateRow = CreatePromosetReportRow(
					duplicate,
					false);

					promosetReportRows.Add(duplicateRow);
				}
			}

			return promosetReportRows;
		}

		private async Task<IList<PromosetReportRow>> GetPhoneDuplicates(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var ordersWithPhonesHavingPromotionalSetsByDeliveryPointPhoneDigitNumbersForPeriod =
				await (from order in uow.Session.Query<Order>()
					   join orderItem in uow.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
					   join phone in uow.Session.Query<Phone>() on order.DeliveryPoint.Id equals phone.DeliveryPoint.Id
					   join dp in uow.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals dp.Id into deliveryPoints
					   from deliveryPoint in deliveryPoints.DefaultIfEmpty()
					   join cl in uow.Session.Query<Counterparty>() on order.Client.Id equals cl.Id into clients
					   from client in clients.DefaultIfEmpty()
					   join dpc in uow.Session.Query<DeliveryPointCategory>() on deliveryPoint.Category.Id equals dpc.Id into deliveryPointCategories
					   from deliveryPointCategory in deliveryPointCategories.DefaultIfEmpty()
					   join author in uow.Session.Query<Employee>() on order.Author.Id equals author.Id
					   join promoset in uow.Session.Query<PromotionalSet>() on orderItem.PromoSet.Id equals promoset.Id
					   where
					   order.CreateDate >= _startDate.Date
					   && order.CreateDate < _endDate.Date.AddDays(1)
					   && _selectedPromosets.Contains(orderItem.PromoSet.Id)
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
					   .ToListAsync(cancellationToken);

			var ordersWithPhonesHavingPromotionalSetsByClientPhoneDigitNumbersForPeriod =
				await (from order in uow.Session.Query<Order>()
					   join orderItem in uow.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
					   join phone in uow.Session.Query<Phone>() on order.Client.Id equals phone.Counterparty.Id
					   join dp in uow.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals dp.Id into deliveryPoints
					   from deliveryPoint in deliveryPoints.DefaultIfEmpty()
					   join cl in uow.Session.Query<Counterparty>() on order.Client.Id equals cl.Id into clients
					   from client in clients.DefaultIfEmpty()
					   join dpc in uow.Session.Query<DeliveryPointCategory>() on deliveryPoint.Category.Id equals dpc.Id into deliveryPointCategories
					   from deliveryPointCategory in deliveryPointCategories.DefaultIfEmpty()
					   join author in uow.Session.Query<Employee>() on order.Author.Id equals author.Id
					   join promoset in uow.Session.Query<PromotionalSet>() on orderItem.PromoSet.Id equals promoset.Id
					   where
					   order.CreateDate >= _startDate.Date
					   && order.CreateDate < _endDate.Date.AddDays(1)
					   && _selectedPromosets.Contains(orderItem.PromoSet.Id)
					   && !phone.IsArchive
					   && !_notDeliveredOrderStatuses.Contains(order.OrderStatus)
					   select new OrderWithPhoneDataNode
					   {
						   OrderId = order.Id,
						   ClientId = order.Client.Id,
						   DeliveryPointId = order.DeliveryPoint == null ? default : order.DeliveryPoint.Id,
						   OrderCreateDate = order.CreateDate,
						   OrderDeliveryDate = order.DeliveryDate,
						   AuthorId = order.Author.Id,
						   AuthorName = author.ShortName,
						   PhoneNumber = phone.Number,
						   PhoneDigitNumber = phone.DigitsNumber,
						   ClientName = client.FullName,
						   DeliveryPointAddress = deliveryPoint == null ? string.Empty : deliveryPoint.ShortAddress,
						   DeliveryPointCategory = deliveryPointCategory == null ? string.Empty : deliveryPointCategory.Name,
						   PromosetId = promoset.Id,
						   PromosetName = promoset.Name,
						   IsRoot = true
					   })
					   .Distinct()
					   .ToListAsync(cancellationToken);

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
				await (from order in uow.Session.Query<Order>()
					   join orderItem in uow.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
					   join phone in uow.Session.Query<Phone>() on order.DeliveryPoint.Id equals phone.DeliveryPoint.Id
					   join dp in uow.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals dp.Id into deliveryPoints
					   from deliveryPoint in deliveryPoints.DefaultIfEmpty()
					   join cl in uow.Session.Query<Counterparty>() on order.Client.Id equals cl.Id into clients
					   from client in clients.DefaultIfEmpty()
					   join dpc in uow.Session.Query<DeliveryPointCategory>() on deliveryPoint.Category.Id equals dpc.Id into deliveryPointCategories
					   from deliveryPointCategory in deliveryPointCategories.DefaultIfEmpty()
					   join author in uow.Session.Query<Employee>() on order.Author.Id equals author.Id
					   join promoset in uow.Session.Query<PromotionalSet>() on orderItem.PromoSet.Id equals promoset.Id
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
					   .ToListAsync(cancellationToken);

			var ordersWithPhonesHavingPromotionalSetsByClientPhoneDigitNumbers =
				await (from order in uow.Session.Query<Order>()
					   join orderItem in uow.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
					   join phone in uow.Session.Query<Phone>() on order.Client.Id equals phone.Counterparty.Id
					   join dp in uow.Session.Query<DeliveryPoint>() on order.DeliveryPoint.Id equals dp.Id into deliveryPoints
					   from deliveryPoint in deliveryPoints.DefaultIfEmpty()
					   join cl in uow.Session.Query<Counterparty>() on order.Client.Id equals cl.Id into clients
					   from client in clients.DefaultIfEmpty()
					   join dpc in uow.Session.Query<DeliveryPointCategory>() on deliveryPoint.Category.Id equals dpc.Id into deliveryPointCategories
					   from deliveryPointCategory in deliveryPointCategories.DefaultIfEmpty()
					   join author in uow.Session.Query<Employee>() on order.Author.Id equals author.Id
					   join promoset in uow.Session.Query<PromotionalSet>() on orderItem.PromoSet.Id equals promoset.Id
					   where
					   orderItem.PromoSet.Id != null
					   && !phone.IsArchive
					   && !_notDeliveredOrderStatuses.Contains(order.OrderStatus)
					   && allDigitNumbersHavingPromotionalSetsForPeriod.Contains(phone.DigitsNumber)
					   select new OrderWithPhoneDataNode
					   {
						   OrderId = order.Id,
						   ClientId = order.Client.Id,
						   DeliveryPointId = order.DeliveryPoint == null ? default : order.DeliveryPoint.Id,
						   OrderCreateDate = order.CreateDate,
						   OrderDeliveryDate = order.DeliveryDate,
						   AuthorId = order.Author.Id,
						   AuthorName = author.ShortName,
						   PhoneNumber = phone.Number,
						   PhoneDigitNumber = phone.DigitsNumber,
						   ClientName = client.FullName,
						   DeliveryPointAddress = deliveryPoint == null ? string.Empty : deliveryPoint.ShortAddress,
						   DeliveryPointCategory = deliveryPointCategory == null ? string.Empty : deliveryPointCategory.Name,
						   PromosetId = promoset.Id,
						   PromosetName = promoset.Name
					   })
					   .Distinct()
					   .ToListAsync(cancellationToken);

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

			var groupedByRootOrdersAndDuplicates =
				(from orderForPeriod in ordersWithPhonesHavingPromotionalSetsByAllDigitNumbersForPeriod
				 join orderDuplicate in ordersWithPhonesHavingPromotionalSetsByAllDigitNumbers on orderForPeriod.PhoneDigitNumber equals orderDuplicate.PhoneDigitNumber
				 where orderForPeriod.OrderId != orderDuplicate.OrderId
				 select new { OrderForPeriod = orderForPeriod, OrderDuplicate = orderDuplicate })
				.GroupBy(o => o.OrderForPeriod)
				.OrderBy(g => g.Key.OrderId)
				.ToDictionary(g => g.Key, g => g.Select(o => o.OrderDuplicate).DistinctBy(o => o.OrderId).ToList())
				.DistinctBy(g => new { g.Key.OrderId, g.Key.DeliveryPointId, g.Key.PromosetId });

			var promosetReportRows = new List<PromosetReportRow>();

			foreach(var ordersData in groupedByRootOrdersAndDuplicates)
			{
				var rootOrder = ordersData.Key;

				var rootRow = CreatePromosetReportRow(rootOrder, phonesByClientAndDeliveryPoint);

				promosetReportRows.Add(rootRow);

				foreach(var duplicateOrder in ordersData.Value)
				{
					var duplicateRow = CreatePromosetReportRow(duplicateOrder, phonesByClientAndDeliveryPoint);

					promosetReportRows.Add(duplicateRow);
				}
			}

			return promosetReportRows;
		}

		private PromosetReportRow CreatePromosetReportRow(
			OrderDeliveryPointDataNode orderDeliveryPointDataNode,
			bool isRoot = false)
		{
			var orderId = orderDeliveryPointDataNode.OrderId;

			var row = new PromosetReportRow
			{
				Address = orderDeliveryPointDataNode.DeliveryPointCompiledAddress,
				AddressCategory = orderDeliveryPointDataNode.DeliveryPointAddressCategoryName,
				Phone = null,
				Client = orderDeliveryPointDataNode.ClientName,
				Order = orderId,
				OrderCreationDate = orderDeliveryPointDataNode.OrderCreateDate,
				OrderDeliveryDate = orderDeliveryPointDataNode.OrderDeliveryDate,
				Promoset = orderDeliveryPointDataNode.PromosetName,
				Author = orderDeliveryPointDataNode.AuthorName,
				IsRootRow = isRoot
			};

			return row;
		}

		private PromosetReportRow CreatePromosetReportRow(
			OrderWithPhoneDataNode orderWithPhoneDataNode,
			IDictionary<(int ClientId, int DeliveryPointId), List<string>> phonesByClientAndDeliveryPoint)
		{
			var duplicateOrderPhones =
				phonesByClientAndDeliveryPoint.TryGetValue((orderWithPhoneDataNode.ClientId, orderWithPhoneDataNode.DeliveryPointId), out var duplicatePhoneNumbers)
				? string.Join(", ", duplicatePhoneNumbers)
				: orderWithPhoneDataNode.PhoneNumber;

			var row = new PromosetReportRow
			{
				Address = orderWithPhoneDataNode.DeliveryPointAddress,
				AddressCategory = orderWithPhoneDataNode.DeliveryPointCategory,
				Phone = duplicateOrderPhones,
				Client = orderWithPhoneDataNode.ClientName,
				Order = orderWithPhoneDataNode.OrderId,
				OrderCreationDate = orderWithPhoneDataNode.OrderCreateDate,
				OrderDeliveryDate = orderWithPhoneDataNode.OrderDeliveryDate,
				Promoset = orderWithPhoneDataNode.PromosetName,
				Author = orderWithPhoneDataNode.AuthorName,
				IsRootRow = orderWithPhoneDataNode.IsRoot
			};

			return row;
		}

		private void SetRowsSequenceNumbers()
		{
			var counter = 1;

			foreach(var row in Rows)
			{
				row.SequenceNumber = counter++;
			}
		}
	}
}
