using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client.ClientClassification;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class MarketingReportViewModel
	{
		private static IList<MarketingReportGroupKey> BuildGroups(bool splitByAbc, bool splitByOrderAuthor,
			IList<CounterpartyCompositeClassification> includeAbc,
			IList<OrderAuthorSubtype> includeAuthorSubTypes,
			IList<MarketingReportRawRow> periodRows)
		{
			var result = new List<MarketingReportGroupKey>();

			if(splitByAbc)
			{
				var AbcValues = includeAbc != null && includeAbc.Any() ? includeAbc
					: System.Enum.GetValues(typeof(CounterpartyCompositeClassification)).Cast<CounterpartyCompositeClassification>().ToList();

				foreach(var abcValue in AbcValues)
				{
					result.Add(new MarketingReportGroupKey
					{
						Dimension = MarketingReportGroupDimension.Abc,
						AbcValue = abcValue,
						ColumnTitle = abcValue.ToString()
					});
				}
			}
			if(splitByOrderAuthor)
			{
				var subtypes = includeAuthorSubTypes != null && includeAuthorSubTypes.Any() ? includeAuthorSubTypes
					: System.Enum.GetValues(typeof(OrderAuthorSubtype)).Cast<OrderAuthorSubtype>().ToList();

				if(subtypes.Contains(OrderAuthorSubtype.Site))
				{
					result.Add(new MarketingReportGroupKey
					{
						Dimension = MarketingReportGroupDimension.Site,
						ColumnTitle = "Сайт ВВ"
					});
				}
				if(subtypes.Contains(OrderAuthorSubtype.MobileApp))
				{
					result.Add(new MarketingReportGroupKey
					{
						Dimension = MarketingReportGroupDimension.MobileApp,
						ColumnTitle = "Мобильное приложение"
					});
				}
				if(subtypes.Contains(OrderAuthorSubtype.Subdivision))
				{
					var subdivisions = periodRows
							.Where(r => r.AuthorSubdivisionId.HasValue)
							.Select(r => new { r.AuthorSubdivisionId, r.AuthorSubdivisionName })
							.Distinct()
							.OrderBy(x => x.AuthorSubdivisionName);

					foreach(var subdivision in subdivisions)
					{
						result.Add(new MarketingReportGroupKey
						{
							Dimension = MarketingReportGroupDimension.Subdivision,
							SubdivisionId = subdivision.AuthorSubdivisionId,
							ColumnTitle = subdivision.AuthorSubdivisionName
						});
					}
				}
			}

			if(!result.Any())
			{
				result.Add(new MarketingReportGroupKey
				{
					Dimension = MarketingReportGroupDimension.All,
					ColumnTitle = "Все"
				});
			}

			return result;
		}
		private static IList<MarketingReportRawRow> FilterByGroup(IList<MarketingReportRawRow> rows, MarketingReportGroupKey key)
		{
			switch(key.Dimension)
			{
				case MarketingReportGroupDimension.Abc:
					return rows.Where(r => r.AbcClass == key.AbcValue).ToList();
				case MarketingReportGroupDimension.Site:
					return rows.Where(r => r.OnlineSource == Vodovoz.Core.Domain.Clients.Source.VodovozWebSite).ToList();
				case MarketingReportGroupDimension.MobileApp:
					return rows.Where(r => r.OnlineSource == Vodovoz.Core.Domain.Clients.Source.MobileApp).ToList();
				case MarketingReportGroupDimension.Subdivision:
					return rows.Where(r => r.AuthorSubdivisionId == key.SubdivisionId).ToList();
				default:
					return rows;
			}
		}
	}
}
