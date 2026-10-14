using DateTimeHelpers;
using MoreLinq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
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
				OrganizationEntity organizationAlias = null;
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
					from gtins in unitOfWork.Session.Query<GtinEntity>()
					select gtins.GtinNumber
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
				.Where(x => x.Gtin == gtin && x.SerialNumber == serialNumber && x.CheckCode == checkCode);

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
				.Where(x => !string.IsNullOrWhiteSpace(x.GtinNumber))
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
				.Where(x => !string.IsNullOrWhiteSpace(x.GtinNumber))
				.GroupBy(x => x.GtinNumber)
				.ToDictionary(x => x.Key, x => x.Sum(g => g.Count));

			return gtinsSoldYesterdayCount;
		}

		public async Task<IDictionary<string, int>> GetMissingCodesCount(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			Gtin gtinAlias = null;
			EdoProblemGtinItem problemItemAlias = null;
			EdoTaskProblem problemAlias = null;
			FormalEdoRequest requestAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			var gtinProblems = await uow.Session.QueryOver(() => gtinAlias)
				.JoinEntityAlias(
					() => problemItemAlias,
					() => problemItemAlias.Gtin.Id == gtinAlias.Id,
					JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => problemItemAlias.Problem, () => problemAlias,
					() => problemItemAlias.Problem.Id == problemAlias.Id
						&& problemAlias.Type == EdoTaskProblemType.Exception
						&& problemAlias.SourceName == nameof(EdoCodePoolMissingCodeException)
						&& problemAlias.State == TaskProblemState.Active
				)
				.JoinEntityAlias(
					() => requestAlias,
					() => requestAlias.Task.Id == problemAlias.EdoTask.Id,
					JoinType.LeftOuterJoin)
				.JoinEntityAlias(
					() => orderItemAlias,
					() => orderItemAlias.Order.Id == requestAlias.Order.Id,
					JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias,
					() => orderItemAlias.Nomenclature.Id == nomenclatureAlias.Id
						&& gtinAlias.Nomenclature.Id == nomenclatureAlias.Id
				)
				.WhereRestrictionOn(() => nomenclatureAlias.Id).IsNotNull()
				.SelectList(list => list
					.SelectGroup(() => gtinAlias.GtinNumber)
					.Select(Projections.Sum(
						Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, ?2)"),
							NHibernateUtil.Decimal,
							Projections.Property(() => orderItemAlias.ActualCount),
							Projections.Property(() => orderItemAlias.Count)
						))
					)
				)
				.TransformUsing(Transformers.PassThrough)
				.ListAsync<object[]>();

			var result = gtinProblems
				.Where(key => !string.IsNullOrWhiteSpace((string)key[0]))
				.ToDictionary(
					key => (string)key[0],
					key => (int)(decimal)key[1]);

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

		/// <inheritdoc />
		public bool AreCodesUsedByOtherEdoTasks(IUnitOfWork uow, int taskId, IEnumerable<int> codeIds)
		{
			TrueMarkProductCode productCode = null;
			var ids = codeIds.ToArray();
			return uow.Session.QueryOver<EdoTaskItem>()
				.JoinAlias(x => x.ProductCode, () => productCode)
				.Where(x => x.CustomerEdoTask.Id != taskId)
				.Where(Restrictions.Disjunction()
					.Add(Restrictions.In(Projections.Property(() => productCode.SourceCode.Id), ids))
					.Add(Restrictions.In(Projections.Property(() => productCode.ResultCode.Id), ids)))
				.RowCount() > 0;
		}

		public IEnumerable<CarLoadDocumentItemTrueMarkProductCode> GetCodesFromWarehouseByOrder(IUnitOfWork uow, int orderId, int? edoTaskId = null)
		{
			CarLoadDocumentItemTrueMarkProductCode carLoadTrueMarkProductCodeAlias = null;
			CarLoadDocumentItem carLoadDocumentItemAlias = null;

			var carLoadCodes = uow.Session.QueryOver(() => carLoadTrueMarkProductCodeAlias)
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.JoinEntityAlias(
					() => carLoadDocumentItemAlias,
					() => carLoadDocumentItemAlias.Id == carLoadTrueMarkProductCodeAlias.CarLoadDocumentItemId,
					JoinType.LeftOuterJoin);

			if(edoTaskId.HasValue)
			{
				carLoadCodes.WithSubquery.WhereProperty(x => x.Id).In(
					QueryOver.Of<EdoTaskItem>()
						.Where(x => x.CustomerEdoTask.Id == edoTaskId.Value)
						.Select(x => x.ProductCode.Id));
			}
			else
			{
				carLoadCodes.Where(() => carLoadDocumentItemAlias.OrderId == orderId);
			}

			return carLoadCodes.List();
		}

		public IEnumerable<RouteListItemTrueMarkProductCode> GetCodesFromDriverByOrder(IUnitOfWork uow, int orderId, int? edoTaskId = null)
		{
			RouteListItemTrueMarkProductCode routeListTrueMarkProductCodeAlias = null;
			RouteListItem routeListItemAlias = null;

			var routeListCodes = uow.Session.QueryOver(() => routeListTrueMarkProductCodeAlias)
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.JoinEntityAlias(
					() => routeListItemAlias,
					() => routeListItemAlias.Id == routeListTrueMarkProductCodeAlias.RouteListItemId,
					JoinType.LeftOuterJoin);

			if(edoTaskId.HasValue)
			{
				routeListCodes.WithSubquery.WhereProperty(x => x.Id).In(
					QueryOver.Of<EdoTaskItem>()
						.Where(x => x.CustomerEdoTask.Id == edoTaskId.Value)
						.Select(x => x.ProductCode.Id));
			}
			else
			{
				routeListCodes.Where(() => routeListItemAlias.Order.Id == orderId);
			}

			return routeListCodes.List();
		}

		public IEnumerable<SelfDeliveryDocumentItemTrueMarkProductCode> GetCodesFromSelfdeliveryByOrder(IUnitOfWork uow, int orderId, int? edoTaskId = null)
		{
			SelfDeliveryDocumentItemTrueMarkProductCode selfdeliveryTrueMarkProductCodeAlias = null;
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItemAlias = null;
			SelfDeliveryDocumentEntity selfDeliveryDocumentAlias = null;

			var selfdeliveryCodes = uow.Session.QueryOver(() => selfdeliveryTrueMarkProductCodeAlias)
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.JoinEntityAlias(
					() => selfDeliveryDocumentItemAlias,
					() => selfDeliveryDocumentItemAlias.Id == selfdeliveryTrueMarkProductCodeAlias.SelfDeliveryDocumentItemId,
					JoinType.LeftOuterJoin)
				.Left.JoinAlias(
					() => selfDeliveryDocumentItemAlias.Document,
					() => selfDeliveryDocumentAlias
				);

			if(edoTaskId.HasValue)
			{
				selfdeliveryCodes.WithSubquery.WhereProperty(x => x.Id).In(
					QueryOver.Of<EdoTaskItem>()
						.Where(x => x.CustomerEdoTask.Id == edoTaskId.Value)
						.Select(x => x.ProductCode.Id));
			}
			else
			{
				selfdeliveryCodes.Where(() => selfDeliveryDocumentAlias.Order.Id == orderId);
			}

			return selfdeliveryCodes.List();
		}

		public IEnumerable<AutoTrueMarkProductCode> GetCodesFromPoolByOrder(IUnitOfWork uow, int orderId, int? edoTaskId = null)
			=> GetRequestCodesByOrder<AutoTrueMarkProductCode>(uow, orderId, edoTaskId);

		/// <inheritdoc/>
		public IEnumerable<ResentTrueMarkProductCode> GetResentCodesByOrder(IUnitOfWork uow, int orderId)
			=> GetRequestCodesByOrder<ResentTrueMarkProductCode>(uow, orderId);

		private IList<T> GetRequestCodesByOrder<T>(IUnitOfWork uow, int orderId, int? edoTaskId = null) where T : TrueMarkProductCode
		{
			T autoProductCodeAlias = null;
			FormalEdoRequest edoRequestAlias = null;
			EdoTaskItem edoTaskItemAlias = null;	

			var poolCodes = uow.Session.QueryOver(() => autoProductCodeAlias)
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.Fetch(SelectMode.Fetch, x => x.SourceCode.Tag1260CodeCheckResult)
				.Fetch(SelectMode.Fetch, x => x.ResultCode)
				.Fetch(SelectMode.Fetch, x => x.ResultCode.Tag1260CodeCheckResult)
				.JoinEntityAlias(
					() => edoTaskItemAlias,
					() => edoTaskItemAlias.ProductCode.Id == autoProductCodeAlias.Id,
					JoinType.LeftOuterJoin
				)
				.Left.JoinAlias(
					() => autoProductCodeAlias.CustomerEdoRequest,
					() => edoRequestAlias
				);

			if(edoTaskId.HasValue)
			{
				poolCodes.Where(() => edoTaskItemAlias.CustomerEdoTask.Id == edoTaskId.Value);
			}
			else
			{
				poolCodes.Where(() => edoRequestAlias.Order.Id == orderId);
				poolCodes.Where(() => edoTaskItemAlias.CustomerEdoTask.Id == edoRequestAlias.Task.Id);
				poolCodes.Where(() => autoProductCodeAlias.SourceCodeStatus != SourceProductCodeStatus.SavedToPool);
			}

			return poolCodes.List();
		}

		public IList<TrueMarkProductCode> GetRejectedProductCodesByOrder(IUnitOfWork uow, int orderId)
		{
			RouteListItemTrueMarkProductCode routeListProductCodeAlias = null;
			RouteListItemEntity routeListItemAlias = null;
			CarLoadDocumentItemTrueMarkProductCode carLoadProductCodeAlias = null;
			CarLoadDocumentItemEntity carLoadDocumentItemAlias = null;
			SelfDeliveryDocumentItemTrueMarkProductCode selfDeliveryProductCodeAlias = null;
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItemAlias = null;
			SelfDeliveryDocumentEntity selfDeliveryDocumentAlias = null;
			AutoTrueMarkProductCode autoProductCodeAlias = null;
			FormalEdoRequest edoRequestAlias = null;

			var routeListProductCodes = uow.Session.QueryOver(() => routeListProductCodeAlias)
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.JoinEntityAlias(
					() => routeListItemAlias,
					() => routeListItemAlias.Id == routeListProductCodeAlias.RouteListItemId,
					JoinType.InnerJoin)
				.Where(() => routeListItemAlias.Order.Id == orderId)
				.Where(() => routeListProductCodeAlias.SourceCodeStatus == SourceProductCodeStatus.Rejected)
				.Where(() => routeListProductCodeAlias.SourceCode != null)
				.List();

			var carLoadProductCodes = uow.Session.QueryOver(() => carLoadProductCodeAlias)
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.JoinEntityAlias(
					() => carLoadDocumentItemAlias,
					() => carLoadDocumentItemAlias.Id == carLoadProductCodeAlias.CarLoadDocumentItemId,
					JoinType.InnerJoin)
				.Where(() => carLoadDocumentItemAlias.OrderId == orderId)
				.Where(() => carLoadProductCodeAlias.SourceCodeStatus == SourceProductCodeStatus.Rejected)
				.Where(() => carLoadProductCodeAlias.SourceCode != null)
				.List();

			var selfDeliveryProductCodes = uow.Session.QueryOver(() => selfDeliveryProductCodeAlias)
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.JoinEntityAlias(
					() => selfDeliveryDocumentItemAlias,
					() => selfDeliveryDocumentItemAlias.Id == selfDeliveryProductCodeAlias.SelfDeliveryDocumentItemId,
					JoinType.InnerJoin)
				.JoinAlias(
					() => selfDeliveryDocumentItemAlias.Document,
					() => selfDeliveryDocumentAlias)
				.Where(() => selfDeliveryDocumentAlias.Order.Id == orderId)
				.Where(() => selfDeliveryProductCodeAlias.SourceCodeStatus == SourceProductCodeStatus.Rejected)
				.Where(() => selfDeliveryProductCodeAlias.SourceCode != null)
				.List();

			var autoProductCodes = uow.Session.QueryOver(() => autoProductCodeAlias)
				.Fetch(SelectMode.Fetch, x => x.SourceCode)
				.JoinAlias(
					() => autoProductCodeAlias.CustomerEdoRequest,
					() => edoRequestAlias)
				.Where(() => edoRequestAlias.Order.Id == orderId)
				.Where(() => autoProductCodeAlias.SourceCodeStatus == SourceProductCodeStatus.Rejected)
				.Where(() => autoProductCodeAlias.SourceCode != null)
				.List();

			var resentProductCodes = uow.Session.Query<ResentTrueMarkProductCode>()
				.Fetch(x => x.SourceCode)
				.Where(x => x.CustomerEdoRequest.Order.Id == orderId)
				.Where(x => x.SourceCodeStatus == SourceProductCodeStatus.Rejected && x.SourceCode != null)
				.ToList();

			return routeListProductCodes
				.Cast<TrueMarkProductCode>()
				.Concat(carLoadProductCodes)
				.Concat(selfDeliveryProductCodes)
				.Concat(autoProductCodes)
				.Concat(resentProductCodes)
				.ToList();
		}

		public IList<TrueMarkProductCode> GetReusableProductCodesByManualEdoRequests(
			IUnitOfWork uow,
			int orderId,
			string gtin,
			SourceProductCodeStatus[] sourceCodeStatuses,
			ProductCodeProblem problem)
		{
			var statuses = sourceCodeStatuses ?? Array.Empty<SourceProductCodeStatus>();

			return (
				from productCode in uow.Session.Query<TrueMarkProductCode>()
				join manualRequest in uow.Session.Query<ManualEdoRequest>()
					on productCode.CustomerEdoRequest.Id equals manualRequest.Id
				where (productCode is AutoTrueMarkProductCode || productCode is ResentTrueMarkProductCode)
					&& manualRequest.Order.Id == orderId
					&& productCode.SourceCode != null
					&& productCode.SourceCode.Gtin == gtin
					&& statuses.Contains(productCode.SourceCodeStatus)
					&& productCode.Problem == problem
				orderby manualRequest.Time descending
				select productCode)
				.ToList();
		}

		public IList<TrueMarkProductCode> GetProductCodesByIdentificationCodeIds(
			IUnitOfWork uow,
			HashSet<int> identificationCodeIds,
			HashSet<int> excludedProductCodeIds)
		{
			var codeIds = identificationCodeIds ?? new HashSet<int>();
			var excludedIds = excludedProductCodeIds ?? new HashSet<int>();

			if(!codeIds.Any())
			{
				return new List<TrueMarkProductCode>();
			}

			var productCodes = uow.Session.Query<TrueMarkProductCode>()
				.Fetch(x => x.CustomerEdoRequest)
				.ThenFetch(x => x.Order)
				.Where(x =>
					(x.SourceCode != null && codeIds.Contains(x.SourceCode.Id))
					|| (x.ResultCode != null && codeIds.Contains(x.ResultCode.Id)))
				.Where(x => !excludedIds.Contains(x.Id))
				.ToList();

			return productCodes;
		}

		public HashSet<int> GetRejectedIdentificationCodeIds(
			IUnitOfWork uow,
			int[] identificationCodeIds,
			OrderStatus orderStatus)
		{
			var codeIds = identificationCodeIds ?? Array.Empty<int>();

			if(!codeIds.Any())
			{
				return new HashSet<int>();
			}

			RouteListItemTrueMarkProductCode routeListProductCodeAlias = null;
			RouteListItemEntity routeListItemAlias = null;
			OrderEntity routeListOrderAlias = null;

			var routeListIdentificationCodeIds = uow.Session.QueryOver(() => routeListProductCodeAlias)
				.JoinEntityAlias(
					() => routeListItemAlias,
					() => routeListItemAlias.Id == routeListProductCodeAlias.RouteListItemId,
					JoinType.InnerJoin)
				.JoinAlias(() => routeListItemAlias.Order, () => routeListOrderAlias)
				.WhereRestrictionOn(() => routeListProductCodeAlias.SourceCode.Id).IsIn(codeIds)
				.Where(() => routeListProductCodeAlias.ResultCode == null)
				.Where(() => routeListProductCodeAlias.SourceCodeStatus == SourceProductCodeStatus.Rejected)
				.Where(() => routeListOrderAlias.OrderStatus == orderStatus)
				.Select(x => x.SourceCode.Id)
				.List<int>();

			CarLoadDocumentItemTrueMarkProductCode carLoadProductCodeAlias = null;
			CarLoadDocumentItemEntity carLoadDocumentItemAlias = null;
			OrderEntity carLoadOrderAlias = null;

			var carLoadIdentificationCodeIds = uow.Session.QueryOver(() => carLoadProductCodeAlias)
				.JoinEntityAlias(
					() => carLoadDocumentItemAlias,
					() => carLoadDocumentItemAlias.Id == carLoadProductCodeAlias.CarLoadDocumentItemId,
					JoinType.InnerJoin)
				.JoinEntityAlias(() => carLoadOrderAlias, () => carLoadOrderAlias.Id == carLoadDocumentItemAlias.OrderId)
				.WhereRestrictionOn(() => carLoadProductCodeAlias.SourceCode.Id).IsIn(codeIds)
				.Where(() => carLoadProductCodeAlias.ResultCode == null)
				.Where(() => carLoadProductCodeAlias.SourceCodeStatus == SourceProductCodeStatus.Rejected)
				.Where(() => carLoadOrderAlias.OrderStatus == orderStatus)
				.Select(x => x.SourceCode.Id)
				.List<int>();

			SelfDeliveryDocumentItemTrueMarkProductCode selfDeliveryProductCodeAlias = null;
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItemAlias = null;
			SelfDeliveryDocumentEntity selfDeliveryDocumentAlias = null;
			OrderEntity selfDeliveryOrderAlias = null;

			var selfDeliveryIdentificationCodeIds = uow.Session.QueryOver(() => selfDeliveryProductCodeAlias)
				.JoinEntityAlias(
					() => selfDeliveryDocumentItemAlias,
					() => selfDeliveryDocumentItemAlias.Id == selfDeliveryProductCodeAlias.SelfDeliveryDocumentItemId,
					JoinType.InnerJoin)
				.JoinAlias(() => selfDeliveryDocumentItemAlias.Document, () => selfDeliveryDocumentAlias)
				.JoinAlias(() => selfDeliveryDocumentAlias.Order, () => selfDeliveryOrderAlias)
				.WhereRestrictionOn(() => selfDeliveryProductCodeAlias.SourceCode.Id).IsIn(codeIds)
				.Where(() => selfDeliveryProductCodeAlias.ResultCode == null)
				.Where(() => selfDeliveryProductCodeAlias.SourceCodeStatus == SourceProductCodeStatus.Rejected)
				.Where(() => selfDeliveryOrderAlias.OrderStatus == orderStatus)
				.Select(x => x.SourceCode.Id)
				.List<int>();

			AutoTrueMarkProductCode autoProductCodeAlias = null;
			FormalEdoRequest edoRequestAlias = null;
			OrderEntity autoOrderAlias = null;

			var autoIdentificationCodeIds = uow.Session.QueryOver(() => autoProductCodeAlias)
				.JoinAlias(() => autoProductCodeAlias.CustomerEdoRequest, () => edoRequestAlias)
				.JoinAlias(() => edoRequestAlias.Order, () => autoOrderAlias)
				.WhereRestrictionOn(() => autoProductCodeAlias.SourceCode.Id).IsIn(codeIds)
				.Where(() => autoProductCodeAlias.ResultCode == null)
				.Where(() => autoProductCodeAlias.SourceCodeStatus == SourceProductCodeStatus.Rejected)
				.Where(() => autoOrderAlias.OrderStatus == orderStatus)
				.Select(x => x.SourceCode.Id)
				.List<int>();

			return new HashSet<int>(routeListIdentificationCodeIds
				.Concat(carLoadIdentificationCodeIds)
				.Concat(selfDeliveryIdentificationCodeIds)
				.Concat(autoIdentificationCodeIds)
				.Concat(uow.Session.Query<ResentTrueMarkProductCode>()
					.Where(x => codeIds.Contains(x.SourceCode.Id)
						&& x.ResultCode == null
						&& x.SourceCodeStatus == SourceProductCodeStatus.Rejected
						&& x.CustomerEdoRequest.Order.OrderStatus == orderStatus)
					.Select(x => x.SourceCode.Id)
					.ToList()));
		}

		public IDictionary<int, int> GetProductCodesCountByOrderItems(IUnitOfWork uow, IEnumerable<int> orderItemIds)
		{
			if(!orderItemIds.Any())
			{
				return new Dictionary<int, int>();
			}

			var codesCountByOrderItems = uow.Session.Query<TrueMarkProductCodeOrderItem>()
				.Where(x => orderItemIds.Contains(x.OrderItemId))
				.GroupBy(x => x.OrderItemId)
				.ToDictionary(x => x.Key, x => x.Count());

			return codesCountByOrderItems;
		}

		public int GetCodesRequiredByOrder(IUnitOfWork uow, int orderId)
		{
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			var codesRequired = uow.Session.QueryOver(() => orderItemAlias)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => orderItemAlias.Order.Id == orderId)
				.Where(() => nomenclatureAlias.IsAccountableInTrueMark)
				.Select(Projections.Sum(
					Projections.Conditional(
							Restrictions.IsNull(
								Projections.Property(() => orderItemAlias.ActualCount)

							),
							Projections.Property(() => orderItemAlias.Count),
							Projections.Property(() => orderItemAlias.ActualCount)
						)))
				.SingleOrDefault<decimal>();

			return (int)codesRequired;
		}

		public async Task<IEnumerable<StagingTrueMarkCode>> GetUsedTrueMarkStagingCodeByStagingTrueMarkCode(
			IUnitOfWork uow,
			StagingTrueMarkCode stagingTrueMarkCode,
			CancellationToken cancellationToken)
		{
			var usedCodes = new List<StagingTrueMarkCode>();

			var allIdentificationCodes = stagingTrueMarkCode.AllIdentificationCodes;

			if(allIdentificationCodes is null || !allIdentificationCodes.Any())
			{
				return usedCodes;
			}

			var serialNumbers = allIdentificationCodes.Select(x => x.SerialNumber).Distinct().ToList();
			var gtin = allIdentificationCodes.First().Gtin;

			var query =
				from stagingCode in uow.Session.Query<StagingTrueMarkCode>()
				where
				stagingCode.CodeType == StagingTrueMarkCodeType.Identification
				&& stagingCode.Gtin == gtin
				&& serialNumbers.Contains(stagingCode.SerialNumber)
				&& stagingCode.RelatedDocumentId != stagingTrueMarkCode.RelatedDocumentId
				select stagingCode;

			usedCodes = await query.ToListAsync(cancellationToken);

			return usedCodes;
		}

		public async Task<int?> GetOrderIdByStagingCode(
			IUnitOfWork uow,
			StagingTrueMarkCode stagingTrueMarkCode,
			CancellationToken cancellationToken)
		{
			int? orderId;

			switch (stagingTrueMarkCode.RelatedDocumentType)
			{
				case StagingTrueMarkCodeRelatedDocumentType.CarLoadDocumentItem:
					orderId = uow.Session.Query<CarLoadDocumentItemEntity>()
						.Where(x => x.Id == stagingTrueMarkCode.RelatedDocumentId)
						.Select(x => x.OrderId)
						.FirstOrDefault();
					break;
				case StagingTrueMarkCodeRelatedDocumentType.RouteListItem:
					orderId = uow.Session.Query<RouteListItemEntity>()
						.Where(x => x.Id == stagingTrueMarkCode.RelatedDocumentId)
						.Select(x => x.Order.Id)
						.FirstOrDefault();
					break;
				case StagingTrueMarkCodeRelatedDocumentType.SelfDeliveryDocumentItem:
					orderId =
						(from documentItem in uow.Session.Query<SelfDeliveryDocumentItemEntity>()
						 join orderItem in uow.Session.Query<OrderItemEntity>() on documentItem.OrderItem.Id equals orderItem.Id
						 where documentItem.Id == stagingTrueMarkCode.RelatedDocumentId
						 select orderItem.Order.Id)
						.FirstOrDefault();
					break;
				default:
					throw new InvalidOperationException("Тип кода неизвестен");
			}

			return orderId;
		}

		public async Task<IEnumerable<TrueMarkProductCode>> GetUsedTrueMarkProductCodeByStagingTrueMarkCode(
			IUnitOfWork uow,
			StagingTrueMarkCode stagingTrueMarkCode,
			CancellationToken cancellationToken)
		{
			var usedCodes = new List<TrueMarkProductCode>();

			var allIdentificationCodes = stagingTrueMarkCode.AllIdentificationCodes;

			if(allIdentificationCodes is null || !allIdentificationCodes.Any())
			{
				return usedCodes;
			}

			var serialNumbers = allIdentificationCodes.Select(x => x.SerialNumber).Distinct().ToList();
			var gtin = allIdentificationCodes.First().Gtin;

			var query =
				from identificationCode in uow.Session.Query<TrueMarkWaterIdentificationCode>()
				join productCode in uow.Session.Query<TrueMarkProductCode>() on identificationCode.Id equals productCode.ResultCode.Id
				where
				productCode != null
				&& gtin == identificationCode.Gtin
				&& serialNumbers.Contains(identificationCode.SerialNumber)
				select new { identificationCode.Gtin, identificationCode.SerialNumber, ProductCode = productCode };

			var usedCodesData =
				(await query.ToListAsync(cancellationToken))
				.ToLookup(
					x => new { x.Gtin, x.SerialNumber },
					x => x.ProductCode);

			foreach(var identificationCode in allIdentificationCodes)
			{
				var searchKey = new { identificationCode.Gtin, identificationCode.SerialNumber };

				var usedCodesByKey = usedCodesData[searchKey];

				if(!usedCodesByKey.Any())
				{
					continue;
				}

				usedCodes.AddRange(usedCodesByKey.ToList());
			}

			return usedCodes;
		}

		public async Task<int?> GetOrderIdByTrueMarkProductCode(IUnitOfWork uow, TrueMarkProductCode trueMarkProductCode, CancellationToken cancellationToken)
		{
			var query = CreateOrderIdByTrueMarkProductCodeQuery(uow, trueMarkProductCode);

			if(query is null)
			{
				return null;
			}

			return await query.FirstOrDefaultAsync(cancellationToken);
		}

		public int? GetOrderIdByTrueMarkProductCode(IUnitOfWork uow, TrueMarkProductCode trueMarkProductCode)
		{
			var query = CreateOrderIdByTrueMarkProductCodeQuery(uow, trueMarkProductCode);

			if(query is null)
			{
				return null;
			}

			return query.FirstOrDefault();
		}

		private IQueryable<int?> CreateOrderIdByTrueMarkProductCodeQuery(IUnitOfWork uow, TrueMarkProductCode trueMarkProductCode)
		{
			switch(trueMarkProductCode)
			{
				case CarLoadDocumentItemTrueMarkProductCode carLoadDocumentItemTrueMarkProduct:
					{
						var carLoadDocumentItemId = carLoadDocumentItemTrueMarkProduct.CarLoadDocumentItemId;

						if(carLoadDocumentItemId is null)
						{
							return null;
						}

						return
							from carLoadDocumentItem in uow.Session.Query<CarLoadDocumentItemEntity>()
							where carLoadDocumentItem.Id == carLoadDocumentItemId.Value
							select carLoadDocumentItem.OrderId;
					}
				case RouteListItemTrueMarkProductCode routeListItemTrueMarkProductCode:
					{
						var routeListItemId = routeListItemTrueMarkProductCode.RouteListItemId;

						if(routeListItemId is null)
						{
							return null;
						}

						return
							from routeListItem in uow.Session.Query<RouteListItemEntity>()
							where routeListItem.Id == routeListItemId.Value
							select (int?)routeListItem.Order.Id;
					}
				case SelfDeliveryDocumentItemTrueMarkProductCode selfDeliveryDocumentItemTrueMarkProductCode:
					{
						var selfDeliveryDocumentItemId = selfDeliveryDocumentItemTrueMarkProductCode.SelfDeliveryDocumentItemId;

						if(selfDeliveryDocumentItemId is null)
						{
							return CreateOrderIdByCustomerEdoRequestQuery(uow, trueMarkProductCode);
						}

						return
							from selfDeliveryDocumentItem in uow.Session.Query<SelfDeliveryDocumentItemEntity>()
							where selfDeliveryDocumentItem.Id == selfDeliveryDocumentItemId.Value
							select (int?)selfDeliveryDocumentItem.Document.Order.Id;
					}
				case AutoTrueMarkProductCode _:
				case ResentTrueMarkProductCode _:
					{
						return CreateOrderIdByCustomerEdoRequestQuery(uow, trueMarkProductCode);
					}
				default:
					throw new InvalidOperationException("Неизвестный тип кода ЧЗ товара");
			}
		}

		private IQueryable<int?> CreateOrderIdByCustomerEdoRequestQuery(IUnitOfWork uow, TrueMarkProductCode trueMarkProductCode)
		{
			var customerEdoRequestId = trueMarkProductCode.CustomerEdoRequest?.Id;

			if(customerEdoRequestId is null)
			{
				return null;
			}

			return
				from customerEdoRequest in uow.Session.Query<FormalEdoRequest>()
				where customerEdoRequest.Id == customerEdoRequestId.Value
				select (int?)customerEdoRequest.Order.Id;
		}

		public IList<StagingTrueMarkCode> GetAllStagingCodesByOrderId(IUnitOfWork uow, int orderId)
		{
			var codes =
				from code in uow.Session.Query<StagingTrueMarkCode>()

				join cldi in uow.Session.Query<CarLoadDocumentItem>()
				on new { Id = code.RelatedDocumentId, Type = code.RelatedDocumentType }
				equals new { Id = cldi.Id, Type = StagingTrueMarkCodeRelatedDocumentType.CarLoadDocumentItem } into cldis
				from carLoadDocumentItem in cldis.DefaultIfEmpty()

				join rli in uow.Session.Query<RouteListItemEntity>()
				on new { Id = code.RelatedDocumentId, Type = code.RelatedDocumentType }
				equals new { Id = rli.Id, Type = StagingTrueMarkCodeRelatedDocumentType.RouteListItem } into rlis
				from routeListItem in rlis.DefaultIfEmpty()

				join sdi in uow.Session.Query<SelfDeliveryDocumentItemEntity>()
				on new { Id = code.RelatedDocumentId, Type = code.RelatedDocumentType }
				equals new { Id = sdi.Id, Type = StagingTrueMarkCodeRelatedDocumentType.SelfDeliveryDocumentItem } into sdis
				from selfDeliveryItem in sdis.DefaultIfEmpty()

				where
				(carLoadDocumentItem != null && carLoadDocumentItem.OrderId == orderId)
				|| (routeListItem != null && routeListItem.Order.Id == orderId)
				|| (selfDeliveryItem != null && selfDeliveryItem.Document.Order.Id == orderId)

				select code;

			return codes.ToList();
		}

		/// <inheritdoc/>
		public IList<RouteListItemTrueMarkProductCode> GetRouteListItemProductCodes(IUnitOfWork uow, int routeListItemId)
		{
			return GetRouteListItemProductCodesQuery(uow, routeListItemId).ToList();
		}

		/// <inheritdoc/>
		public async Task<IList<RouteListItemTrueMarkProductCode>> GetRouteListItemProductCodesAsync(
			IUnitOfWork uow,
			int routeListItemId,
			CancellationToken cancellationToken)
		{
			return await GetRouteListItemProductCodesQuery(uow, routeListItemId).ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IList<RouteListItemTrueMarkProductCode>> GetRouteListItemProductCodesByIdsWithSourceOrResultAsync(
			IUnitOfWork uow,
			int routeListItemId,
			IEnumerable<int> productCodeIds,
			CancellationToken cancellationToken)
		{
			var ids = GetDistinctPositiveIds(productCodeIds);

			if(!ids.Any())
			{
				return new List<RouteListItemTrueMarkProductCode>();
			}

			return await uow.Session.Query<RouteListItemTrueMarkProductCode>()
				.Where(x => x.RouteListItemId == routeListItemId
					&& ids.Contains(x.Id)
					&& (x.SourceCode != null || x.ResultCode != null))
				.OrderBy(x => x.Id)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IList<TrueMarkWaterIdentificationCode>> GetRouteListItemProductCodesSourceCodesAsync(
			IUnitOfWork uow,
			int routeListItemId,
			CancellationToken cancellationToken)
		{
			return await uow.Session.Query<RouteListItemTrueMarkProductCode>()
				.Where(x => x.RouteListItemId == routeListItemId && x.SourceCode != null)
				.Select(x => x.SourceCode)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<bool> IsRouteListItemHasProductCodesAsync(IUnitOfWork uow, int routeListItemId, CancellationToken cancellationToken)
		{
			return await uow.Session.Query<RouteListItemTrueMarkProductCode>()
				.AnyAsync(x => x.RouteListItemId == routeListItemId, cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IList<CarLoadDocumentItemTrueMarkProductCode>> GetCarLoadDocumentItemsProductCodesAsync(
			IUnitOfWork uow,
			IEnumerable<int> carLoadDocumentItemIds,
			CancellationToken cancellationToken)
		{
			var ids = GetDistinctPositiveIds(carLoadDocumentItemIds);

			if(!ids.Any())
			{
				return new List<CarLoadDocumentItemTrueMarkProductCode>();
			}

			return await uow.Session.Query<CarLoadDocumentItemTrueMarkProductCode>()
				.Where(x => x.CarLoadDocumentItemId != null && ids.Contains(x.CarLoadDocumentItemId.Value))
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<IDictionary<int, int>> GetCarLoadDocumentItemsProductCodesCountAsync(
			IUnitOfWork uow,
			IEnumerable<int> carLoadDocumentItemIds,
			CancellationToken cancellationToken)
		{
			var ids = GetDistinctPositiveIds(carLoadDocumentItemIds);

			if(!ids.Any())
			{
				return new Dictionary<int, int>();
			}

			var counts = await uow.Session.Query<CarLoadDocumentItemTrueMarkProductCode>()
				.Where(x => x.CarLoadDocumentItemId != null && ids.Contains(x.CarLoadDocumentItemId.Value))
				.GroupBy(x => x.CarLoadDocumentItemId)
				.Select(g => new { ItemId = g.Key, Count = g.Count() })
				.ToListAsync(cancellationToken);

			return counts.ToDictionary(x => x.ItemId.Value, x => x.Count);
		}

		/// <inheritdoc/>
		public async Task<IList<TrueMarkWaterIdentificationCode>> GetCarLoadDocumentItemProductCodesSourceCodesAsync(
			IUnitOfWork uow,
			int carLoadDocumentItemId,
			CancellationToken cancellationToken)
		{
			return await uow.Session.Query<CarLoadDocumentItemTrueMarkProductCode>()
				.Where(x => x.CarLoadDocumentItemId == carLoadDocumentItemId && x.SourceCode != null)
				.Select(x => x.SourceCode)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public async Task<bool> IsCarLoadDocumentItemsHaveAddedProductCodesAsync(
			IUnitOfWork uow,
			IEnumerable<int> carLoadDocumentItemIds,
			CancellationToken cancellationToken)
		{
			var ids = GetDistinctPositiveIds(carLoadDocumentItemIds);

			if(!ids.Any())
			{
				return false;
			}

			return await uow.Session.Query<CarLoadDocumentItemTrueMarkProductCode>()
				.AnyAsync(x => x.CarLoadDocumentItemId != null
					&& ids.Contains(x.CarLoadDocumentItemId.Value)
					&& (x.SourceCode != null || x.ResultCode != null), cancellationToken);
		}

		/// <inheritdoc/>
		public IList<SelfDeliveryDocumentItemTrueMarkProductCode> GetSelfDeliveryDocumentItemsProductCodes(
			IUnitOfWork uow,
			IEnumerable<int> selfDeliveryDocumentItemIds)
		{
			var ids = GetDistinctPositiveIds(selfDeliveryDocumentItemIds);

			if(!ids.Any())
			{
				return new List<SelfDeliveryDocumentItemTrueMarkProductCode>();
			}

			return GetSelfDeliveryDocumentItemsProductCodesQuery(uow, ids).ToList();
		}

		/// <inheritdoc/>
		public async Task<IList<SelfDeliveryDocumentItemTrueMarkProductCode>> GetSelfDeliveryDocumentItemsProductCodesAsync(
			IUnitOfWork uow,
			IEnumerable<int> selfDeliveryDocumentItemIds,
			CancellationToken cancellationToken)
		{
			var ids = GetDistinctPositiveIds(selfDeliveryDocumentItemIds);

			if(!ids.Any())
			{
				return new List<SelfDeliveryDocumentItemTrueMarkProductCode>();
			}

			return await GetSelfDeliveryDocumentItemsProductCodesQuery(uow, ids).ToListAsync(cancellationToken);
		}

		/// <inheritdoc/>
		public IDictionary<int, int> GetSelfDeliveryDocumentItemsProductCodesCount(
			IUnitOfWork uow,
			IEnumerable<int> selfDeliveryDocumentItemIds)
		{
			var ids = GetDistinctPositiveIds(selfDeliveryDocumentItemIds);

			if(!ids.Any())
			{
				return new Dictionary<int, int>();
			}

			var counts = GetSelfDeliveryDocumentItemsProductCodesCountQuery(uow, ids).ToList();

			return counts.ToDictionary(x => x.ItemId.Value, x => x.Count);
		}

		/// <inheritdoc/>
		public async Task<IDictionary<int, int>> GetSelfDeliveryDocumentItemsProductCodesCountAsync(
			IUnitOfWork uow,
			IEnumerable<int> selfDeliveryDocumentItemIds,
			CancellationToken cancellationToken)
		{
			var ids = GetDistinctPositiveIds(selfDeliveryDocumentItemIds);

			if(!ids.Any())
			{
				return new Dictionary<int, int>();
			}

			var counts = await GetSelfDeliveryDocumentItemsProductCodesCountQuery(uow, ids).ToListAsync(cancellationToken);

			return counts.ToDictionary(x => x.ItemId.Value, x => x.Count);
		}

		/// <inheritdoc/>
		public async Task<IList<TrueMarkWaterIdentificationCode>> GetSelfDeliveryDocumentItemProductCodesSourceCodesAsync(
			IUnitOfWork uow,
			int selfDeliveryDocumentItemId,
			CancellationToken cancellationToken)
		{
			return await uow.Session.Query<SelfDeliveryDocumentItemTrueMarkProductCode>()
				.Where(x => x.SelfDeliveryDocumentItemId == selfDeliveryDocumentItemId && x.SourceCode != null)
				.Select(x => x.SourceCode)
				.ToListAsync(cancellationToken);
		}

		private static IQueryable<RouteListItemTrueMarkProductCode> GetRouteListItemProductCodesQuery(IUnitOfWork uow, int routeListItemId)
		{
			return uow.Session.Query<RouteListItemTrueMarkProductCode>()
				.Where(x => x.RouteListItemId == routeListItemId);
		}

		private static IQueryable<SelfDeliveryDocumentItemTrueMarkProductCode> GetSelfDeliveryDocumentItemsProductCodesQuery(
			IUnitOfWork uow,
			IList<int> selfDeliveryDocumentItemIds)
		{
			return uow.Session.Query<SelfDeliveryDocumentItemTrueMarkProductCode>()
				.Where(x => x.SelfDeliveryDocumentItemId != null && selfDeliveryDocumentItemIds.Contains(x.SelfDeliveryDocumentItemId.Value));
		}

		private static IQueryable<ProductCodesCountNode> GetSelfDeliveryDocumentItemsProductCodesCountQuery(
			IUnitOfWork uow,
			IList<int> selfDeliveryDocumentItemIds)
		{
			return GetSelfDeliveryDocumentItemsProductCodesQuery(uow, selfDeliveryDocumentItemIds)
				.GroupBy(x => x.SelfDeliveryDocumentItemId)
				.Select(g => new ProductCodesCountNode { ItemId = g.Key, Count = g.Count() });
		}

		private sealed class ProductCodesCountNode
		{
			public int? ItemId { get; set; }
			public int Count { get; set; }
		}

		private static List<int> GetDistinctPositiveIds(IEnumerable<int> ids)
		{
			return ids.Where(x => x > 0).Distinct().ToList();
		}
	}
}
