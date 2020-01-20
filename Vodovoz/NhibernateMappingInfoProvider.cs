using System;
using System.Linq.Expressions;
using NHibernate.Cfg;
using QS.DomainModel.Entity;
using System.Linq;
using Gamma.Utilities;
using NHibernate.Mapping;
using SolrSearch;
using System.Collections;
using System.Collections.Generic;

namespace Vodovoz
{
	public class NhibernateMappingInfoProvider : IOrmMappingInfoProvider
	{
		private readonly Configuration nhibernateCfg;

		public NhibernateMappingInfoProvider(Configuration nhibernateCfg)
		{
			this.nhibernateCfg = nhibernateCfg ?? throw new ArgumentNullException(nameof(nhibernateCfg));
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
			properties.AddRange(classMapping.PropertyIterator);

			return properties;
		}
	}
}
