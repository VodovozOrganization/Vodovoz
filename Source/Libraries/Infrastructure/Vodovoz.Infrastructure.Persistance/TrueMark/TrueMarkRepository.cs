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

			var result = gtinProblems.ToDictionary(
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
					() => selfDeliveryDocumentItemAlias.Document,
					() => selfDeliveryDocumentAlias
				)
				.Where(() => selfDeliveryDocumentAlias.Order.Id == orderId)
				.List();

			return selfdeliveryCodes;
		}

		public IEnumerable<AutoTrueMarkProductCode> GetCodesFromPoolByOrder(IUnitOfWork uow, int orderId)
		{
			AutoTrueMarkProductCode autoProductCodeAlias = null;
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
				)
				.Where(() => edoTaskItemAlias.CustomerEdoTask.Id == edoRequestAlias.Task.Id)
				.Where(() => edoRequestAlias.Order.Id == orderId)
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
				join tmpc in uow.Session.Query<TrueMarkProductCode>() on identificationCode.Id equals tmpc.ResultCode.Id into productCodes
				from productCode in productCodes.DefaultIfEmpty()
				where
				productCode.Id != null
				&& serialNumbers.Contains(identificationCode.SerialNumber)
				select productCode;

			var existingCodesHavingReuqiredSerialNumbers = await query.ToListAsync(cancellationToken);

			foreach(var identificationCode in allIdentificationCodes)
			{
				var existingTrueMarkProductCode = existingCodesHavingReuqiredSerialNumbers
					.FirstOrDefault(x => x.SourceCode.Gtin == gtin && x.SourceCode.SerialNumber == identificationCode.SerialNumber);

				if(existingTrueMarkProductCode is null)
				{
					continue;
				}

				usedCodes.Add(existingTrueMarkProductCode);
			}

			return usedCodes;
		}

		public async Task<int?> GetOrderIdByTrueMarkProductCode(IUnitOfWork uow, TrueMarkProductCode trueMarkProductCode, CancellationToken cancellationToken)
		{
			switch(trueMarkProductCode)
			{
				case CarLoadDocumentItemTrueMarkProductCode carLoadDocumentItemTrueMarkProduct:
					{
						var query =
							from carLoadDocumentItem in uow.Session.Query<CarLoadDocumentItemEntity>()
							where carLoadDocumentItem.Id == carLoadDocumentItemTrueMarkProduct.CarLoadDocumentItem.Id
							select carLoadDocumentItem.OrderId;

						return await query.FirstOrDefaultAsync(cancellationToken);
					}
				case RouteListItemTrueMarkProductCode routeListItemTrueMarkProductCode:
					{
						var query =
							from routeListItem in uow.Session.Query<RouteListItemEntity>()
							where routeListItem.Id == routeListItemTrueMarkProductCode.RouteListItem.Id
							select routeListItem.Order.Id;

						return await query.FirstOrDefaultAsync(cancellationToken);
					}
				case SelfDeliveryDocumentItemTrueMarkProductCode selfDeliveryDocumentItemTrueMarkProductCode:
					{
						var query =
							from selfDeliveryDocumentItem in uow.Session.Query<SelfDeliveryDocumentItemEntity>()
							join selfDeliveryDocument in uow.Session.Query<SelfDeliveryDocumentEntity>()
								on selfDeliveryDocumentItem.Document.Id equals selfDeliveryDocument.Id
							where selfDeliveryDocumentItem.Id == selfDeliveryDocumentItemTrueMarkProductCode.SelfDeliveryDocumentItem.Id
							select selfDeliveryDocument.Order.Id;

						return await query.FirstOrDefaultAsync(cancellationToken);
					}
				case AutoTrueMarkProductCode autoTrueMarkProductCode:
					{
						var query =
							from orderEdoReques in uow.Session.Query<FormalEdoRequest>()
							where orderEdoReques.Id == autoTrueMarkProductCode.CustomerEdoRequest.Id
							select orderEdoReques.Order.Id;

						return await query.FirstOrDefaultAsync(cancellationToken);
					}
				default:
					throw new InvalidOperationException("Неизвестный тип кода ЧЗ товара");
			}
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
	}
}
