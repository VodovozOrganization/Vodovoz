using NHibernate.Criterion;
using QS.Project.Journal.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Vodovoz.ViewModels.Widgets.Search
{
	public class CompositeAlgebraicSearchCriterion
	{
		private readonly CompositeAlgebraicSearchViewModel _journalSearch;
		private readonly List<SearchProperty> _searchProperties = new List<SearchProperty>();
		private MatchMode _likeMatchMode = MatchMode.Anywhere;

		public CompositeAlgebraicSearchCriterion(CompositeAlgebraicSearchViewModel journalSearch)
		{
			_journalSearch = journalSearch ?? throw new ArgumentNullException(nameof(journalSearch));
		}

		#region Fluent

		public ICriterion Finish()
		{
			Disjunction disjunctionCriterion = new Disjunction();

			if(_journalSearch.SearchValues == null
				|| !_journalSearch.SearchValues.Any()
				|| string.IsNullOrWhiteSpace(_journalSearch.EntrySearchText1)
				|| (_journalSearch.Operand1 != OperandType.Disabled
					&& string.IsNullOrWhiteSpace(_journalSearch.EntrySearchText2))
				|| (_journalSearch.Operand2 != OperandType.Disabled
					&& string.IsNullOrWhiteSpace(_journalSearch.EntrySearchText3))
				|| (_journalSearch.Operand3 != OperandType.Disabled
					&& string.IsNullOrWhiteSpace(_journalSearch.EntrySearchText4)))
			{
				return new Conjunction();
			}

			foreach(var property in _searchProperties)
			{
				ICriterion restriction = property.GetCriterion(_journalSearch.EntrySearchText1);

				if(_journalSearch.Operand1 == OperandType.Disabled)
				{
					disjunctionCriterion.Add(restriction);
					continue;
				}

				restriction = AddCriterionFor(
					restriction,
					_journalSearch.Operand1,
					property,
					_journalSearch.EntrySearchText2);

				if(_journalSearch.Operand2 == OperandType.Disabled)
				{
					disjunctionCriterion.Add(restriction);
					continue;
				}

				restriction = AddCriterionFor(
					restriction,
					_journalSearch.Operand2,
					property,
					_journalSearch.EntrySearchText3);

				if(_journalSearch.Operand3 == OperandType.Disabled)
				{
					disjunctionCriterion.Add(restriction);
					continue;
				}

				restriction = AddCriterionFor(
					restriction,
					_journalSearch.Operand3,
					property,
					_journalSearch.EntrySearchText4);

				if(restriction != null)
				{
					disjunctionCriterion.Add(restriction);
				}
			}

			return disjunctionCriterion;
		}

		private ICriterion AddCriterionFor(
			ICriterion originalCriterion,
			OperandType operandType,
			SearchProperty searchProperty,
			string entryText)
		{
			var entryTextCriterion = searchProperty.GetCriterion(entryText);

			if(operandType == OperandType.And)
			{
				originalCriterion = Restrictions.And(originalCriterion, entryTextCriterion);
			}
			else if(operandType == OperandType.Or)
			{
				originalCriterion = Restrictions.Or(originalCriterion, entryTextCriterion);
			}

			return originalCriterion;
		}

		public CompositeAlgebraicSearchCriterion By(params Expression<Func<object>>[] aliases)
		{
			foreach(var alias in aliases)
			{
				_searchProperties.Add(SearchProperty.Create(alias, _likeMatchMode));
			}
			return this;
		}

		public CompositeAlgebraicSearchCriterion WithLikeMode(MatchMode matchMode)
		{
			_likeMatchMode = matchMode;
			return this;
		}

		#endregion Fluent
	}
}
