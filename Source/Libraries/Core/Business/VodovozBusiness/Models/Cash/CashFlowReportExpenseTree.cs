using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Models.Cash
{
	/// <summary>
	/// Строит дерево расходов с детализацией по финансовым центрам
	/// </summary>
	public sealed class CashFlowReportExpenseTree
	{
		private const string _undefinedCenter = "Не указано";

		private readonly IReadOnlyList<CashFlowReportGrouping> _groupings;
		private readonly IReadOnlyList<CashFlowReportLine> _articles;
		private readonly IReadOnlyDictionary<int, string> _subdivisionNames;
		private readonly bool _hideZeroLeaves;

		private CashFlowReportExpenseTree(
			IReadOnlyList<CashFlowReportGrouping> groupings,
			IReadOnlyList<CashFlowReportLine> articles,
			IReadOnlyDictionary<int, string> subdivisionNames,
			bool hideZeroLeaves)
		{
			_groupings = groupings;
			_articles = articles;
			_subdivisionNames = subdivisionNames;
			_hideZeroLeaves = hideZeroLeaves;
		}

		/// <summary>
		/// Группирует учтённые расходы и возвраты по выбранным уровням
		/// </summary>
		/// <param name="groupings">Выбранные группировки в порядке вложенности</param>
		/// <param name="articles">Иерархия допустимых групп и статей расходов</param>
		/// <param name="movements">Расходы и возвраты со знаками операций</param>
		/// <param name="subdivisionNames">Названия подразделений по идентификаторам</param>
		/// <param name="hideZeroLeaves">Скрыть нулевые конечные строки и пустые ветки</param>
		/// <returns>Дочерние строки раздела расходов</returns>
		public static IReadOnlyList<CashFlowReportLine> Build(
			IReadOnlyList<CashFlowReportGrouping> groupings,
			IReadOnlyList<CashFlowReportLine> articles,
			IEnumerable<CashFlowReportExpenseNode> movements,
			IReadOnlyDictionary<int, string> subdivisionNames,
			bool hideZeroLeaves)
		{
			var expenseTree = new CashFlowReportExpenseTree(groupings, articles, subdivisionNames, hideZeroLeaves);
			var articleIds = new HashSet<int>(GetArticleIds(articles));

			return expenseTree.BuildLevel(0, movements.Where(x => articleIds.Contains(x.ExpenseCategoryId))).ToArray();
		}

		private static IEnumerable<int> GetArticleIds(IEnumerable<CashFlowReportLine> articles)
		{
			foreach(var line in articles)
			{
				if(line.IsArticle)
				{
					yield return line.Id.Value;
				}

				foreach(var id in GetArticleIds(line.Children))
				{
					yield return id;
				}
			}
		}

		private IEnumerable<CashFlowReportLine> BuildLevel(int level, IEnumerable<CashFlowReportExpenseNode> movements)
		{
			switch(_groupings[level])
			{
				case CashFlowReportGrouping.Article:
					return BuildArticleLevel(level, _articles, movements.ToLookup(x => x.ExpenseCategoryId));
				case CashFlowReportGrouping.CfoSubdivision:
					return BuildCenterLevel(level, movements.GroupBy(x => x.CfoSubdivisionId));
				case CashFlowReportGrouping.CfzSubdivision:
					return BuildCenterLevel(level, movements.GroupBy(x => x.CfzSubdivisionId));
				default:
					throw new ArgumentOutOfRangeException(nameof(level));
			}
		}

		private IEnumerable<CashFlowReportLine> BuildArticleLevel(
			int level, IEnumerable<CashFlowReportLine> articles, ILookup<int, CashFlowReportExpenseNode> movements)
		{
			foreach(var article in articles)
			{
				if(article.IsArticle)
				{
					var articleMovements = movements[article.Id.Value];

					if(articleMovements.Any())
					{
						var line = CreateMovementLine(level, article.Title, article.Id, article.Numbering, true, articleMovements);

						if(line != null)
						{
							yield return line;
						}
					}

					continue;
				}

				var children = BuildArticleLevel(level, article.Children, movements).ToArray();

				if(children.Length > 0)
				{
					yield return new CashFlowReportLine(article.Title, children.Sum(x => x.Money), children, article.Id, article.Numbering);
				}
			}
		}

		private IEnumerable<CashFlowReportLine> BuildCenterLevel(int level, IEnumerable<IGrouping<int?, CashFlowReportExpenseNode>> centers)
		{
			var ordered = centers
				.Select(center => new { Center = center, Title = GetCenterTitle(center.Key) })
				.OrderBy(x => x.Title)
				.ThenBy(x => x.Center.Key);

			foreach(var item in ordered)
			{
				var line = CreateMovementLine(level, item.Title, item.Center.Key, null, false, item.Center);

				if(line != null)
				{
					yield return line;
				}
			}
		}

		private CashFlowReportLine CreateMovementLine(
			int level, string title, int? id, string numbering, bool isArticle, IEnumerable<CashFlowReportExpenseNode> movements)
		{
			if(level + 1 < _groupings.Count)
			{
				var children = BuildLevel(level + 1, movements).ToArray();

				return children.Length == 0
					? null
					: new CashFlowReportLine(title, children.Sum(x => x.Money), children, id, numbering, isArticle);
			}

			var money = movements.Sum(x => x.Money);

			return _hideZeroLeaves && money == 0
				? null
				: new CashFlowReportLine(title, money, id: id, numbering: numbering, isArticle: isArticle);
		}

		private string GetCenterTitle(int? centerId)
		{
			return centerId.HasValue && _subdivisionNames.TryGetValue(centerId.Value, out var name) && !string.IsNullOrWhiteSpace(name)
				? name
				: _undefinedCenter;
		}
	}
}
