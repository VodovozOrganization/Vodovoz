using Core.Infrastructure;
using DateTimeHelpers;
using MoreLinq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.TrueMark;
using VodovozBusiness.Domain.Goods;

namespace Vodovoz.Infrastructure.Persistance.TrueMark
{
	internal sealed class TrueMarkRepository : ITrueMarkRepository
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public TrueMarkRepository(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public ISet<string> GetAllowedCodeOwnersInn()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				Organization organizationAlias = null;
				var queryOrganization = uow.Session.QueryOver(() => organizationAlias)
					.Where(() => organizationAlias.OrganizationEdoType != OrganizationEdoType.WithoutEdo)
					.Select(Projections.Property(() => organizationAlias.INN));
				var organizations = queryOrganization.List<string>();

				var result = organizations.Distinct().ToHashSet();
				return result;
			}
		}

		public ISet<string> GetAllowedCodeOwnersGtins()
		{
			using(var unitOfWork = _uowFactory.CreateWithoutRoot("Get our Gtins"))
			{
				var result =
				(
					from nomenclatures in unitOfWork.Session.Query<Gtin>()
					select nomenclatures.GtinNumber
				)
				.Distinct()
				.ToHashSet();

				return result;
			}
		}

		public async Task<IEnumerable<TrueMarkWaterIdentificationCode>> LoadWaterCodes(List<int> codeIds, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var result = await uow.Session.QueryOver<TrueMarkWaterIdentificationCode>()
					.WhereRestrictionOn(x => x.Id).IsIn(codeIds)
					.ListAsync(cancellationToken);
				return result;
			}
		}

		public IEnumerable<TrueMarkWaterIdentificationCode> GetTrueMarkCodeDuplicates(
			IUnitOfWork uow,
			string gtin,
			string serialNumber,
			string checkCode)
		{
			var query = uow.Session.Query<TrueMarkWaterIdentificationCode>()
				.Where(x => x.GTIN == gtin && x.SerialNumber == serialNumber && x.CheckCode == checkCode);

			return query.ToList();
		}

		public async Task<IDictionary<string, List<string>>> GetGtinsNomenclatureData(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var gtins =
				await (from gtin in uow.Session.Query<Gtin>()
					   join nomenclature in uow.Session.Query<Nomenclature>() on gtin.Nomenclature.Id equals nomenclature.Id
					   select new { GtinNumber = gtin.GtinNumber, Nomenclature = nomenclature.Name })
				.ToListAsync(cancellationToken);

			var groupedData = gtins
				.OrderBy(x => x.GtinNumber)
				.GroupBy(x => x.GtinNumber)
				.ToDictionary(x => x.Key, x => x.Select(g => g.Nomenclature).ToList());

			return groupedData;
		}

		public async Task<IDictionary<string, int>> GetSoldYesterdayGtinsCount(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var yesterdayDate = DateTime.Today.Date.AddDays(-1);

			var gtinsSoldYesterdayData =
				await (from order in uow.Session.Query<Domain.Orders.Order>()
					   join orderItem in uow.Session.Query<OrderItem>() on order.Id equals orderItem.Order.Id
					   join gtin in uow.Session.Query<Gtin>() on orderItem.Nomenclature.Id equals gtin.Nomenclature.Id
					   where
						   order.DeliveryDate >= yesterdayDate
						   && order.DeliveryDate <= yesterdayDate.LatestDayTime()
					   select new { GtinNumber = gtin.GtinNumber, Count = (int)(orderItem.ActualCount ?? 0) })
				 .ToListAsync(cancellationToken);

			var gtinsSoldYesterdayCount = gtinsSoldYesterdayData
				.GroupBy(x => x.GtinNumber)
				.ToDictionary(x => x.Key, x => x.Sum(g => g.Count));

			return gtinsSoldYesterdayCount;
		}

		public async Task<IDictionary<string, int>> GetMissingCodesCount(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var result = new Dictionary<string, int>();

			var orderProblems =
				await (from edoTaskProblem in uow.Session.Query<EdoTaskProblem>()
					   join edoRequest in uow.Session.Query<OrderEdoRequest>() on edoTaskProblem.EdoTask.Id equals edoRequest.Task.Id
					   join order in uow.Session.Query<Domain.Orders.Order>() on edoRequest.Order.Id equals order.Id
					   where
					   edoTaskProblem.Type == EdoTaskProblemType.Exception
					   && edoTaskProblem.SourceName == nameof(EdoCodePoolMissingCodeException)
					   && edoTaskProblem.State == TaskProblemState.Active
					   select new
					   {
						   Order = order,
						   Items = edoTaskProblem.CustomItems.ToList()
					   })
				 .ToListAsync(cancellationToken);

			var ordersProblemItems = orderProblems
				.GroupBy(x => x.Order)
				.ToDictionary(
					g => g.Key,
					g => g.SelectMany(x => x.Items).ToList());

			foreach(var orderProblemItems in ordersProblemItems)
			{
				var orderItems = orderProblemItems.Key?.OrderItems;

				if(orderItems is null || !orderItems.Any())
				{
					continue;
				}

				var missingGtins = orderProblemItems.Value
					.Where(x => x is EdoProblemGtinItem)
					.Cast<EdoProblemGtinItem>()
					.Select(x => x.Gtin.GtinNumber)
					.Distinct()
					.ToList();

				foreach(var missingGtin in missingGtins)
				{
					var bottlesHavingGtinCount = (int)orderItems
						.Where(x => x.Nomenclature.Gtins.Select(gtin => gtin.GtinNumber).Contains(missingGtin))
						.Select(x => x.ActualCount ?? x.Count)
						.Sum();

					result.TryGetValue(missingGtin, out int gtinsCount);

					result[missingGtin] = gtinsCount + bottlesHavingGtinCount;
				}
			}

			return result;
		}

		public bool IsTrueMarkAnyCodeAlreadySaved(IUnitOfWork uow, TrueMarkAnyCode trueMarkAnyCode)
		{
			var isCodeAlreadySaved = trueMarkAnyCode.Match(
				transportCode =>
					uow.Session.Query<TrueMarkTransportCode>().Where(x => x.RawCode == trueMarkAnyCode.TrueMarkTransportCode.RawCode).Any(),
				groupCode =>
					uow.Session.Query<TrueMarkWaterGroupCode>().Where(x => x.RawCode == trueMarkAnyCode.TrueMarkTransportCode.RawCode).Any(),
				waterCode =>
					uow.Session.Query<TrueMarkWaterIdentificationCode>().Where(x => x.RawCode == trueMarkAnyCode.TrueMarkTransportCode.RawCode).Any());

			return isCodeAlreadySaved;
		}

		public IEnumerable<TrueMarkTransportCode> GetTransportCodes(IUnitOfWork uow, IEnumerable<int> transportCodeIds)
		{
			TrueMarkTransportCode trueMarkTransportCodeAlias = null;

			var routeListCodes = uow.Session.QueryOver(() => trueMarkTransportCodeAlias)
				.WhereRestrictionOn(() => trueMarkTransportCodeAlias.Id).IsIn(transportCodeIds.ToArray())
				.List();

			return routeListCodes;
		}

		public IEnumerable<TrueMarkWaterGroupCode> GetGroupWaterCodes(IUnitOfWork uow, IEnumerable<int> groupCodeIds)
		{
			TrueMarkWaterGroupCode trueMarkWaterGroupCodeAlias = null;

			var routeListCodes = uow.Session.QueryOver(() => trueMarkWaterGroupCodeAlias)
				.WhereRestrictionOn(() => trueMarkWaterGroupCodeAlias.Id).IsIn(groupCodeIds.ToArray())
				.List();

			return routeListCodes;
		}

		public IEnumerable<CarLoadDocumentItemTrueMarkProductCode> GetCodesFromWarehouseByOrder(IUnitOfWork uow, int orderId)
		{
			CarLoadDocumentItemTrueMarkProductCode carLoadTrueMarkProductCodeAlias = null;
			CarLoadDocumentItem carLoadDocumentItemAlias = null;

			var carLoadCodes = uow.Session.QueryOver(() => carLoadTrueMarkProductCodeAlias)
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.Left.JoinAlias(
					() => carLoadTrueMarkProductCodeAlias.CarLoadDocumentItem,
					() => carLoadDocumentItemAlias
				)
				.Where(() => carLoadDocumentItemAlias.OrderId == orderId)
				.List();

			return carLoadCodes;
		}

		public IEnumerable<RouteListItemTrueMarkProductCode> GetCodesFromDriverByOrder(IUnitOfWork uow, int orderId)
		{
			RouteListItemTrueMarkProductCode routeListTrueMarkProductCodeAlias = null;
			RouteListItem routeListItemAlias = null;

			var routeListCodes = uow.Session.QueryOver(() => routeListTrueMarkProductCodeAlias)
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.Left.JoinAlias(
					() => routeListTrueMarkProductCodeAlias.RouteListItem,
					() => routeListItemAlias
				)
				.Where(() => routeListItemAlias.Order.Id == orderId)
				.List();

			return routeListCodes;
		}

		public IEnumerable<SelfDeliveryDocumentItemTrueMarkProductCode> GetCodesFromSelfdeliveryByOrder(IUnitOfWork uow, int orderId)
		{
			SelfDeliveryDocumentItemTrueMarkProductCode selfdeliveryTrueMarkProductCodeAlias = null;
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItemAlias = null;
			SelfDeliveryDocumentEntity selfDeliveryDocumentAlias = null;

			var selfdeliveryCodes = uow.Session.QueryOver(() => selfdeliveryTrueMarkProductCodeAlias)
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.Left.JoinAlias(
					() => selfdeliveryTrueMarkProductCodeAlias.SelfDeliveryDocumentItem,
					() => selfDeliveryDocumentItemAlias
				)
				.Left.JoinAlias(
					() => selfDeliveryDocumentItemAlias.SelfDeliveryDocument,
					() => selfDeliveryDocumentAlias
				)
				.Where(() => selfDeliveryDocumentAlias.Order.Id == orderId)
				.List();

			return selfdeliveryCodes;
		}

		public IEnumerable<AutoTrueMarkProductCode> GetCodesFromPoolByOrder(IUnitOfWork uow, int orderId)
		{
			AutoTrueMarkProductCode autoProductCodeAlias = null;
			OrderEdoRequest customerEdoRequestAlias = null;

			var poolCodes = uow.Session.QueryOver(() => autoProductCodeAlias)
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.Left.JoinAlias(
					() => autoProductCodeAlias.CustomerEdoRequest,
					() => customerEdoRequestAlias
				)
				.Where(() => customerEdoRequestAlias.Order.Id == orderId)
				.List();

			return poolCodes;
		}

		public int GetCodesRequiredByOrder(IUnitOfWork uow, int orderId)
		{
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			var codesRequired = uow.Session.QueryOver(() => orderItemAlias)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => orderItemAlias.Order.Id == orderId)
				.Where(() => nomenclatureAlias.IsAccountableInTrueMark)
				.Select(Projections.Sum(Projections.Property<OrderItem>(x => x.Count)))
				.SingleOrDefault<decimal>();

			return (int)codesRequired;
		}
	}
}
