using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.NHibernateProjections.Goods
{
	public static class ProductGroupProjections
	{
		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase названия сущности + Alias<br/>
		/// Используется <see cref="ProductGroup"/> productGroupAlias
		/// </summary>
		/// <returns></returns>
		public static IProjection GetProductGroupNameWithEnclosureProjection()
		{
			ProductGroup productGroupAlias = null;

			return Projections.SqlFunction(
			new SQLFunctionTemplate(NHibernateUtil.String,
			@"(SELECT
					CONCAT_WS("" / "", name_source_1.name, name_source_2.name, name_source_3.name) AS FullName
				FROM (
					WITH RECURSIVE groups_source (DEPTH,
						id,
						parent_id,
						name,
						level_1_id,
						level_2_id,
						level_3_id) AS (
						SELECT
							1 AS DEPTH,
							ng.id AS id,
							0 AS parent_id,
							ng.name,
							ng.id AS level_1_id,
							0 AS level_2_id,
							0 AS level_3_id
						FROM
							nomenclature_groups ng
						WHERE
							ng.parent_id IS NULL
					UNION
						SELECT
							groups_source.depth + 1 AS DEPTH,
							ng.id AS id,
							groups_source.id AS parent_id,
							ng.name,
							groups_source.level_1_id AS level_1_id,
							IF(groups_source.depth = 1,
							ng.id,
							groups_source.level_2_id) AS level_2_id,
							IF(groups_source.depth = 2,
							ng.id,
							groups_source.level_3_id) AS level_3_id
						FROM
							nomenclature_groups ng,
							groups_source
						WHERE
							ng.parent_id = groups_source.id
					)
					SELECT
						*
					FROM
						groups_source
				) AS groups_with_levels
				LEFT JOIN nomenclature_groups name_source_1 ON
					name_source_1.id = groups_with_levels.level_1_id
				LEFT JOIN nomenclature_groups name_source_2 ON
					name_source_2.id = groups_with_levels.level_2_id
				LEFT JOIN nomenclature_groups name_source_3 ON
					name_source_3.id = groups_with_levels.level_3_id
				WHERE groups_with_levels.id = ?1)"),
			NHibernateUtil.String, Projections.Property(() => productGroupAlias.Id));
		}
	}
}
