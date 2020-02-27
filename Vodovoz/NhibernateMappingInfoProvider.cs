using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Utilities;
using NHibernate.Cfg;
using NHibernate.Mapping;
using SolrSearch.Mapping;

namespace Vodovoz
{
	public class NhibernateMappingInfoProvider : IOrmMappingInfoProvider
	{
		private readonly Configuration nhibernateCfg;

		public NhibernateMappingInfoProvider(Configuration nhibernateCfg)
		{
			this.nhibernateCfg = nhibernateCfg ?? throw new ArgumentNullException(nameof(nhibernateCfg));
		}

		public string GetMappedTableName<TEntity>() where TEntity : class
		{
			return GetMappedTableName(typeof(TEntity));
		}

		public string GetMappedTableName(Type entityType)
		{
			var classMapping = nhibernateCfg.GetClassMapping(entityType);
			return classMapping.Table.Name;
		}

		public string GetMappedColumnName<TEntity>(Expression<Func<TEntity, object>> propertySelector)
			where TEntity : class
		{
			if(propertySelector == null) {
				throw new ArgumentNullException(nameof(propertySelector));
			}

			string propertyName = PropertyUtil.GetName(propertySelector);

			return GetMappedColumnName<TEntity>(propertyName);
		}

		public string GetMappedColumnName<TEntity>(string propertyName)
			where TEntity : class
		{

			var properties = GetMappingProperties<TEntity>();

			var propertyMappingInfo = properties.FirstOrDefault(x => x.Name == propertyName);
			if(propertyMappingInfo == null) {
				throw new InvalidOperationException($"Свойство {propertyName} не найдено в маппинге ORM");
			}
			var columnInfo = propertyMappingInfo.ColumnIterator.OfType<Column>().FirstOrDefault();
			if(columnInfo == null) {
				throw new InvalidProgramException($"Не найдена информация о колонке для свойства {propertyName} в маппинге ORM");
			}
			return columnInfo.Name;
		}

		private IEnumerable<Property> GetMappingProperties<TEntity>()
			where TEntity : class
		{
			var classMapping = nhibernateCfg.GetClassMapping(typeof(TEntity));
			List<Property> properties = new List<Property>();
			properties.Add(classMapping.IdentifierProperty);
			if(classMapping.RootClazz != null) {
				properties.AddRange(classMapping.RootClazz.PropertyClosureIterator);
			}
			properties.AddRange(classMapping.PropertyIterator);

			return properties;
		}
	}
}
