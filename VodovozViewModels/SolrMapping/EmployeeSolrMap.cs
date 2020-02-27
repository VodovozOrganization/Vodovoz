using System;
using SolrSearch.Mapping;
using Vodovoz.SolrModel;
using Vodovoz.Domain.Employees;
using SolrSearch;

namespace Vodovoz.SolrMapping
{
	public class EmployeeSolrMap : SolrOrmSourceClassMap<EmployeeSolrEntity, Employee, EmployeeSolrEntityFactory>
	{
		public EmployeeSolrMap()
		{
			Map(se => se.Id, e => e.Id);
			Map(se => se.Name, e => e.Name);
			Map(se => se.LastName, e => e.LastName);
			Map(se => se.Patronymic, e => e.Patronymic);
		}
	}

	public class EmployeeSolrEntityFactory : SolrEntityFactoryBase<EmployeeSolrEntity>
	{
		public override EmployeeSolrEntity CreateEntity(EntityContentProvider entityContentProvider)
		{
			EmployeeSolrEntity entity = new EmployeeSolrEntity();
			entity.Id = entityContentProvider.GetPropertyContent<int>(nameof(entity.Id));
			entity.Name = entityContentProvider.GetPropertyContent<string>(nameof(entity.Name));
			entity.LastName = entityContentProvider.GetPropertyContent<string>(nameof(entity.LastName));
			entity.Patronymic = entityContentProvider.GetPropertyContent<string>(nameof(entity.Patronymic));
			return entity;
		}
	}
}
