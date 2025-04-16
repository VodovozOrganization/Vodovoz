using Gamma.Utilities;
using NHibernate;
using NHibernate.Transform;
using NHibernate.Type;
using System;
using System.Linq.Expressions;

namespace Vodovoz.Core.Data.NHibernate.Extensions
{
	public static class NhibernateSqlQueryMapExtenstions
	{
		/// <summary>
		/// Добавляет параметр в запрос и мапит его на свойство объекта
		/// </summary>
		/// <typeparam name="Node">Тип ноды используемый в трансформере AliasToBean</typeparam>
		/// <param name="parameter">Имя парметра. В тексте SQL запроса должен быть записан как :parameter_name</param>
		/// <param name="propertySelection">Выражение для выбра свойства ноды на которе будет маппиться параметр</param>
		/// <param name="type">Тип данных параметра</param>
		public static ISQLQuery MapScalarParameter<Node>(
			this ISQLQuery query, 
			string parameter,
			Expression<Func<Node, object>> propertySelection, 
			IType type
			)
		{
			var propertyName = PropertyUtil.GetName(propertySelection);

			query.SetParameter(parameter, propertyName);
			query.AddScalar(propertyName, type);
			return query;
		}

		/// <summary>
		/// Настраивает маппинг параметров на свойства ноды
		/// </summary>
		/// <typeparam name="Node">Тип ноды</typeparam>
		public static ISQLQueryParameterMapping<Node> MapParametersToNode<Node>(this ISQLQuery query)
			where Node : class
		{
			return new SQLQueryParameterMapping<Node>(query);
		}

		public interface ISQLQueryParameterMapping<Node>
			where Node : class
		{
			/// <summary>
			/// Добавляет параметр в запрос и мапит его на свойство объекта
			/// </summary>
			/// <typeparam name="Node">Тип ноды используемый в трансформере AliasToBean</typeparam>
			/// <param name="parameter">Имя парметра. В тексте SQL запроса должен быть записан как :parameter_name</param>
			/// <param name="propertySelection">Выражение для выбра свойства ноды на которе будет маппиться параметр</param>
			/// <param name="type">Тип данных параметра</param>
			ISQLQueryParameterMapping<Node> Map(
				string parameter,
				Expression<Func<Node, object>> propertySelection,
				IType type
			);

			/// <summary>
			/// Устанавливает AliasToBean трансформер
			/// </summary>
			/// <returns></returns>
			IQuery SetResultTransformer();
		}

		private class SQLQueryParameterMapping<Node> : ISQLQueryParameterMapping<Node>
			where Node : class
		{
			private readonly ISQLQuery _query;

			public SQLQueryParameterMapping(ISQLQuery query)
			{
				_query = query ?? throw new ArgumentNullException(nameof(query));
			}

			public ISQLQueryParameterMapping<Node> Map(
				string parameter,
				Expression<Func<Node, object>> propertySelection,
				IType type
				)
			{
				var propertyName = PropertyUtil.GetName(propertySelection);

				_query.SetParameter(parameter, propertyName);
				_query.AddScalar(propertyName, type);

				return this;
			}

			public IQuery SetResultTransformer()
			{
				return _query.SetResultTransformer(Transformers.AliasToBean<Node>());
			}
		}
	}
}
