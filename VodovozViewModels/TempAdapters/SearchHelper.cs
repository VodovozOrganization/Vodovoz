using System;
using System.Linq.Expressions;
using NHibernate.Criterion;
using System.Linq;
using NHibernate;
using QS.Project.Journal;

namespace Vodovoz.TempAdapters
{
	public class SearchHelper
	{
		private readonly IJournalSearch journalSearch;

		public SearchHelper(IJournalSearch journalSearch)
		{
			this.journalSearch = journalSearch ?? throw new ArgumentNullException(nameof(journalSearch));
		}

		public ICriterion GetSearchCriterion<TEntity>(params Expression<Func<TEntity, object>>[] aliases)
		{
			Type[] digitsTypes = { typeof(decimal), typeof(int) };

			Conjunction conjunctionCriterion = new Conjunction();

			if(journalSearch.SearchValues == null || !journalSearch.SearchValues.Any()) {
				return conjunctionCriterion;
			}

			foreach(var sv in journalSearch.SearchValues) {
				if(string.IsNullOrWhiteSpace(sv)) {
					continue;
				}
				Disjunction disjunctionCriterion = new Disjunction();

				foreach(var alias in aliases) {
					Type typeOfPropery = null;
					if(alias.Body is UnaryExpression) {
						UnaryExpression unaryExpession = alias.Body as UnaryExpression;
						typeOfPropery = unaryExpession.Operand.Type;
					} else if(alias.Body is MemberExpression info)
						typeOfPropery = info.Type;
					else {
						throw new InvalidOperationException($"{nameof(alias)} должен быть {nameof(UnaryExpression)} или {nameof(MemberExpression)}");
					}

					if(typeOfPropery == typeof(int)) {
						if(int.TryParse(sv, out int intValue)) {
							ICriterion restriction = Restrictions.Eq(Projections.Property(alias), intValue);
							disjunctionCriterion.Add(restriction);
						}
					} else if(typeOfPropery == typeof(uint) || typeOfPropery == typeof(uint?)) {
						if(uint.TryParse(sv, out uint uintValue)) {
							ICriterion restriction = Restrictions.Eq(Projections.Property(alias), uintValue);
							disjunctionCriterion.Add(restriction);
						}
					} else if(typeOfPropery == typeof(decimal)) {
						if(decimal.TryParse(sv, out decimal decimalValue)) {
							ICriterion restriction = Restrictions.Eq(Projections.Property(alias), decimalValue);
							disjunctionCriterion.Add(restriction);
						}
					} else if(typeOfPropery == typeof(string)) {
						var likeRestriction = Restrictions.Like(Projections.Cast(NHibernateUtil.String, Projections.Property(alias)), sv, MatchMode.Anywhere);
						disjunctionCriterion.Add(likeRestriction);
					} else {
						throw new NotSupportedException($"Тип {typeOfPropery} не поддерживается");
					}

				}
				conjunctionCriterion.Add(disjunctionCriterion);
			}

			return conjunctionCriterion;
		}

		public ICriterion GetSearchCriterion(params Expression<Func<object>>[] aliases)
		{
			Type[] digitsTypes = { typeof(decimal), typeof(int) };

			Conjunction conjunctionCriterion = new Conjunction();

			if(journalSearch.SearchValues == null || !journalSearch.SearchValues.Any()) {
				return conjunctionCriterion;
			}

			foreach(var sv in journalSearch.SearchValues) {
				if(string.IsNullOrWhiteSpace(sv)) {
					continue;
				}
				Disjunction disjunctionCriterion = new Disjunction();

				bool intParsed = int.TryParse(sv, out int intValue);
				bool decimalParsed = decimal.TryParse(sv, out decimal decimalValue);

				foreach(var alias in aliases) {
					bool aliasIsInt = false;
					bool aliasIsDecimal = false;
					if(alias.Body is UnaryExpression) {
						UnaryExpression unaryExpession = alias.Body as UnaryExpression;
						aliasIsInt = unaryExpession.Operand.Type == typeof(int);
						aliasIsDecimal = unaryExpession.Operand.Type == typeof(decimal);
					} else if(!(alias.Body is MemberExpression)) {
						throw new InvalidOperationException($"{nameof(alias)} должен быть {nameof(UnaryExpression)} или {nameof(MemberExpression)}");
					}

					if(aliasIsInt) {
						if((intParsed)) {
							ICriterion restriction = Restrictions.Eq(Projections.Property(alias), intValue);
							disjunctionCriterion.Add(restriction);
						} else {
							continue;
						}
					} else if(aliasIsDecimal) {
						if((decimalParsed)) {
							ICriterion restriction = Restrictions.Eq(Projections.Property(alias), decimalValue);
							disjunctionCriterion.Add(restriction);
						} else {
							continue;
						}
					} else {
						var likeRestriction = Restrictions.Like(Projections.Cast(NHibernateUtil.String, Projections.Property(alias)), sv, MatchMode.Anywhere);
						disjunctionCriterion.Add(likeRestriction);
					}
				}
				conjunctionCriterion.Add(disjunctionCriterion);
			}

			return conjunctionCriterion;
		}

		public ICriterion GetSearchCriterionNew(params SearchParameter[] searchParameters)
		{
			Type[] digitsTypes = { typeof(decimal), typeof(int) }; // только цифры

			Conjunction conjunctionCriterion = new Conjunction();

			if(journalSearch.SearchValues == null || !journalSearch.SearchValues.Any()) {
				return conjunctionCriterion;
			}

			foreach(var sv in journalSearch.SearchValues) { // это то что между И
				if(string.IsNullOrWhiteSpace(sv)) {
					continue;
				}
				Disjunction disjunctionCriterion = new Disjunction();

				bool intParsed = int.TryParse(sv, out int intValue);
				bool decimalParsed = decimal.TryParse(sv, out decimal decimalValue);

				foreach(var parameter in searchParameters) { // а это поиск по каждому из критериев
					bool aliasIsInt = false;
					bool aliasIsDecimal = false;

					if(parameter.alias.Body is UnaryExpression) {
						UnaryExpression unaryExpession = parameter.alias.Body as UnaryExpression;
						aliasIsInt = unaryExpession.Operand.Type == typeof(int);
						aliasIsDecimal = unaryExpession.Operand.Type == typeof(decimal);
					} else if(!(parameter.alias.Body is MemberExpression)) {
						throw new InvalidOperationException($"{nameof(parameter.alias)} должен быть {nameof(UnaryExpression)} или {nameof(MemberExpression)}");
					}

					if(aliasIsInt) {
						if((intParsed)) {
							ICriterion restriction = Restrictions.Eq(Projections.Property(parameter.alias), intValue);
							disjunctionCriterion.Add(restriction);
						} else {
							continue;
						}
					} else if(aliasIsDecimal) {
						if((decimalParsed)) {
							ICriterion restriction = Restrictions.Eq(Projections.Property(parameter.alias), decimalValue);
							disjunctionCriterion.Add(restriction);
						} else {
							continue;
						}
					} else {
						var likeRestriction = Restrictions.Like(Projections.Cast(NHibernateUtil.String, Projections.Property(parameter.alias)), sv, MatchMode.Anywhere);
			
						// Если это номер то обработать
						if (parameter.type == SearchParametrType.DigitsNumber && sv.Any(char.IsDigit)) {

							bool needRunBoth;
							string processedDigitsNumber = VodovozInfrastructure.Utils.PhoneUtils.NumberTrim(sv, out needRunBoth);


							// Если тру значит нужно запустить и до и после обработки
							if (needRunBoth) {
								var likeRestrictionProcessedNumber = Restrictions.Like(Projections.Cast(NHibernateUtil.String, Projections.Property(parameter.alias)), sv, MatchMode.Anywhere);
								var likeRestrictionNotProcessedNumber = Restrictions.Like(Projections.Cast(NHibernateUtil.String, Projections.Property(parameter.alias)), processedDigitsNumber, MatchMode.Anywhere);
								disjunctionCriterion.Add(likeRestrictionProcessedNumber);
								disjunctionCriterion.Add(likeRestrictionNotProcessedNumber);
							// Запустить только после обработки
							} else {
								var likeRestrictionProcessedNumber = Restrictions.Like(Projections.Cast(NHibernateUtil.String, Projections.Property(parameter.alias)), processedDigitsNumber, MatchMode.Anywhere);
								disjunctionCriterion.Add(likeRestrictionProcessedNumber);
							}
							// Если это не номер то добавить как было
						}
						else {
							disjunctionCriterion.Add(likeRestriction);
						}

					}
				}
				conjunctionCriterion.Add(disjunctionCriterion);
			}

			return conjunctionCriterion;
		}

	}
}
