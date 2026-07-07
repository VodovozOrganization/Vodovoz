using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.NHibernateProjections.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class MarketingReportViewModel
	{
		private IList<MarketingReportRawRow> GetData(DateTime start, DateTime end, int[] clientIdsFilter, OrderStatus[] includedStatuses)
		{
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			Vodovoz.Domain.Client.Counterparty counterpartyAlias = null;
			CounterpartyClassification counterpartyClassificationAlias = null;
			Employee authorAlias = null;
			Subdivision subdivisionAlias = null;
			OnlineOrder onlineOrderAlias = null;
			OrderRating orderRatingAlias = null;
			MarketingReportRawRow resultAlias = null;

			var lastCalculationSettingId = _unitOfWork.GetAll<CounterpartyClassification>()
				.Select(c => c.ClassificationCalculationSettingsId)
				.OrderByDescending(d => d)
				.FirstOrDefault();

			//взяты из TurnoverWithDynamicsReportViewModel
			#region Classifications Restrictions

			var classificationByBottlesCountProjection =
				Projections.Property(() => counterpartyClassificationAlias.ClassificationByBottlesCount);

			var classificationByOrdersCountProjection =
				Projections.Property(() => counterpartyClassificationAlias.ClassificationByOrdersCount);

			var classificationIsAXRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.A),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.X));

			var classificationIsAYRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.A),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Y));

			var classificationIsAZRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.A),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Z));

			var classificationIsBXRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.B),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.X));

			var classificationIsBYRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.B),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Y));

			var classificationIsBZRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.B),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Z));

			var classificationIsCXRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.C),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.X));

			var classificationIsCYRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.C),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Y));

			var classificationIsCZRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.C),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Z));


			var counterpartyClassificationProjection =
				Projections.Conditional(
					classificationIsAXRestriction, Projections.Constant(CounterpartyCompositeClassification.AX),
						Projections.Conditional(classificationIsAYRestriction, Projections.Constant(CounterpartyCompositeClassification.AY),
						Projections.Conditional(classificationIsAZRestriction, Projections.Constant(CounterpartyCompositeClassification.AZ),
						Projections.Conditional(classificationIsBXRestriction, Projections.Constant(CounterpartyCompositeClassification.BX),
						Projections.Conditional(classificationIsBYRestriction, Projections.Constant(CounterpartyCompositeClassification.BY),
						Projections.Conditional(classificationIsBZRestriction, Projections.Constant(CounterpartyCompositeClassification.BZ),
						Projections.Conditional(classificationIsCXRestriction, Projections.Constant(CounterpartyCompositeClassification.CX),
						Projections.Conditional(classificationIsCYRestriction, Projections.Constant(CounterpartyCompositeClassification.CY),
						Projections.Conditional(classificationIsCZRestriction, Projections.Constant(CounterpartyCompositeClassification.CZ),
					Projections.Constant(CounterpartyCompositeClassification.New))))))))));
			#endregion Classifications Restrictions

			var bottle19LRestriction = Restrictions.And(
				Restrictions.Eq(Projections.Property(() => nomenclatureAlias.Category), NomenclatureCategory.water),
				Restrictions.Eq(Projections.Property(() => nomenclatureAlias.TareVolume), TareVolume.Vol19L));

			var additionalServiceRestriction = Restrictions.In(
				Projections.Property(() => nomenclatureAlias.Category),
				new[] { NomenclatureCategory.service, NomenclatureCategory.additional });

			var ratingSubquery = QueryOver.Of(() => orderRatingAlias)
				.Where(() => orderRatingAlias.Order.Id == orderAlias.Id)
				.Select(Projections.Property(() => orderRatingAlias.Rating))
				.Take(1);

			var query = _unitOfWork.Session.QueryOver(() => orderItemAlias)
				.JoinEntityAlias(() => orderAlias, () => orderItemAlias.Order.Id == orderAlias.Id)
				.Inner.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => orderAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => orderAlias.OnlineOrder, () => onlineOrderAlias)
				.Left.JoinAlias(() => authorAlias.Subdivision, () => subdivisionAlias)
				.JoinEntityAlias(
				() => counterpartyClassificationAlias,
				() => counterpartyAlias.Id == counterpartyClassificationAlias.CounterpartyId
						&& counterpartyClassificationAlias.ClassificationCalculationSettingsId == lastCalculationSettingId,
						JoinType.LeftOuterJoin)
				.Where(() => !orderAlias.IsContractCloser)
				.And(Restrictions.Between(Projections.Property(() => orderAlias.DeliveryDate), start, end));

			if(clientIdsFilter != null && clientIdsFilter.Length > 0)
			{
				query.Where(Restrictions.In(Projections.Property(() => counterpartyAlias.Id), clientIdsFilter));
			}

			if(includedStatuses != null && includedStatuses.Length > 0)
			{
				query.WhereRestrictionOn(() => orderAlias.OrderStatus).IsIn(includedStatuses);
			}


			var result = query.
						SelectList(list => list
						.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
						.Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.ClientId)
						.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.OrderDate)
						.Select(OrderProjections.GetOrderSumProjection()).WithAlias(() => resultAlias.OrderSum)
						.Select(counterpartyClassificationProjection).WithAlias(() => resultAlias.AbcClass)
						.Select(() => onlineOrderAlias.Source).WithAlias(() => resultAlias.OnlineSource)
						.Select(() => subdivisionAlias.Id).WithAlias(() => resultAlias.AuthorSubdivisionId)
						.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.AuthorSubdivisionName)
						.SelectSubQuery(ratingSubquery).WithAlias(() => resultAlias.Rating)
						.Select(Projections.Max(Projections.Conditional(
							additionalServiceRestriction, Projections.Constant(1), Projections.Constant(0))))
						.WithAlias(() => resultAlias.HasAdditionalServiceFlag)
						.Select(Projections.Sum(Projections.Conditional(
							bottle19LRestriction, Projections.Property(() => orderItemAlias.ActualCount), Projections.Constant(0m))))
						.WithAlias(() => resultAlias.BottlesCount19L)
						.Select(() => orderAlias.IsFirstOrder).WithAlias(() => resultAlias.IsFirstOrderEver))
						.SetTimeout(0)
						.TransformUsing(Transformers.AliasToBean<MarketingReportRawRow>())
						.List<MarketingReportRawRow>();

			return result;


		}
	}
}


