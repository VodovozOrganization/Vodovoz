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

			var propertiesText1Disjunction = new Disjunction();

			foreach(var property in _searchProperties)
			{
				var propertyText1Criterion = property.GetCriterion(_journalSearch.EntrySearchText1);

				if(propertyText1Criterion is null)
				{
					continue;
				}

				propertiesText1Disjunction.Add(propertyText1Criterion);
			}

			var propertiesText2Disjunction = new Disjunction();

			foreach(var property in _searchProperties)
			{
				var propertyText2Criterion = property.GetCriterion(_journalSearch.EntrySearchText2);

				if(propertyText2Criterion is null)
				{
					continue;
				}

				propertiesText2Disjunction.Add(propertyText2Criterion);
			}

			var propertiesText3Disjunction = new Disjunction();

			foreach(var property in _searchProperties)
			{
				var propertyText3Criterion = property.GetCriterion(_journalSearch.EntrySearchText3);

				if(propertyText3Criterion is null)
				{
					continue;
				}

				propertiesText3Disjunction.Add(propertyText3Criterion);
			}

			var propertiesText4Disjunction = new Disjunction();

			foreach(var property in _searchProperties)
			{
				var propertyText4Criterion = property.GetCriterion(_journalSearch.EntrySearchText4);

				if(propertyText4Criterion is null)
				{
					continue;
				}

				propertiesText4Disjunction.Add(propertyText4Criterion);
			}

			return GenerateAlgebraicExpression(
				propertiesText1Disjunction,
				_journalSearch.Operand1,
				propertiesText2Disjunction,
				_journalSearch.Operand2,
				propertiesText3Disjunction,
				_journalSearch.Operand3,
				propertiesText4Disjunction);
		}

		private ICriterion GenerateAlgebraicExpression(
			ICriterion propertiesText1Disjunction,
			OperandType operand1,
			ICriterion propertiesText2Disjunction,
			OperandType operand2,
			ICriterion propertiesText3Disjunction,
			OperandType operand3,
			ICriterion propertiesText4Disjunction)
		{
			if(operand1 == OperandType.Disabled)
			{
				return propertiesText1Disjunction;
			}

			if(operand2 == OperandType.Disabled)
			{
				if(operand1 == OperandType.And)
				{
					// and
					return Restrictions.And(propertiesText1Disjunction, propertiesText2Disjunction);
				}

				// or
				return Restrictions.Or(propertiesText1Disjunction, propertiesText2Disjunction);
			}

			if(operand3 == OperandType.Disabled)
			{
				if(operand1 == OperandType.And)
				{
					if(operand2 == OperandType.And)
					{
						// and and
						var criterionAndAnd = new Conjunction();

						criterionAndAnd.Add(propertiesText1Disjunction);
						criterionAndAnd.Add(propertiesText2Disjunction);
						criterionAndAnd.Add(propertiesText3Disjunction);

						return criterionAndAnd;
					}

					// and or

					return Restrictions.Or(
						Restrictions.And(propertiesText1Disjunction, propertiesText2Disjunction),
						propertiesText3Disjunction);
				}

				if(operand2 == OperandType.And)
				{
					// or and

					return Restrictions.Or(
						propertiesText1Disjunction,
						Restrictions.And(propertiesText2Disjunction, propertiesText3Disjunction));
				}

				// or or
				var disjunctionOrOr = new Disjunction();
				disjunctionOrOr.Add(propertiesText1Disjunction);
				disjunctionOrOr.Add(propertiesText2Disjunction);
				disjunctionOrOr.Add(propertiesText3Disjunction);

				return disjunctionOrOr;
			}

			if(operand1 == OperandType.And)
			{
				if(operand2 == OperandType.And)
				{
					// and and and
					if(operand3 == OperandType.And)
					{
						var criterionAndAndAnd = new Conjunction();

						criterionAndAndAnd.Add(propertiesText1Disjunction);
						criterionAndAndAnd.Add(propertiesText2Disjunction);
						criterionAndAndAnd.Add(propertiesText3Disjunction);
						criterionAndAndAnd.Add(propertiesText4Disjunction);

						return criterionAndAndAnd;
					}

					// and and or
					var criterionAndAnd = new Conjunction();

					criterionAndAnd.Add(propertiesText1Disjunction);
					criterionAndAnd.Add(propertiesText2Disjunction);
					criterionAndAnd.Add(propertiesText3Disjunction);

					return Restrictions.Or(criterionAndAnd, propertiesText4Disjunction);
				}

				// and or and
				if(operand3 == OperandType.And)
				{
					return Restrictions.Or(
						Restrictions.And(propertiesText1Disjunction, propertiesText2Disjunction),
						Restrictions.And(propertiesText3Disjunction, propertiesText4Disjunction));
				}

				// and or or

				var disjunctionAndOrOr = new Disjunction();

				disjunctionAndOrOr.Add(Restrictions.And(propertiesText1Disjunction, propertiesText2Disjunction));

				disjunctionAndOrOr.Add(propertiesText3Disjunction);
				disjunctionAndOrOr.Add(propertiesText4Disjunction);

				return disjunctionAndOrOr;
			}

			if(operand2 == OperandType.And)
			{
				if(operand3 == OperandType.And)
				{
					// or and and

					var conjunction = new Conjunction();

					conjunction.Add(propertiesText2Disjunction);
					conjunction.Add(propertiesText3Disjunction);
					conjunction.Add(propertiesText4Disjunction);

					return Restrictions.Or(propertiesText1Disjunction, conjunction);
				}

				// or and or

				Disjunction disjunctionOrAndOr = new Disjunction();

				disjunctionOrAndOr.Add(Restrictions.And(propertiesText2Disjunction, propertiesText3Disjunction));

				disjunctionOrAndOr.Add(propertiesText1Disjunction);
				disjunctionOrAndOr.Add(propertiesText4Disjunction);

				return disjunctionOrAndOr;
			}

			// or or and

			if(operand3 == OperandType.And)
			{
				var disjunctionOrOrAnd = new Disjunction();

				disjunctionOrOrAnd.Add(propertiesText1Disjunction);
				disjunctionOrOrAnd.Add(propertiesText2Disjunction);
				disjunctionOrOrAnd.Add(Restrictions.And(propertiesText3Disjunction, propertiesText4Disjunction));

				return disjunctionOrOrAnd;
			}

			// or or or

			var disjunction = new Disjunction();

			disjunction.Add(propertiesText1Disjunction);
			disjunction.Add(propertiesText2Disjunction);
			disjunction.Add(propertiesText3Disjunction);
			disjunction.Add(propertiesText4Disjunction);

			return disjunction;
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
