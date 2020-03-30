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
			Map(se => se.Id, e => e.Id, 999999);
			Map(se => se.Name, e => e.Name, 2);
			Map(se => se.LastName, e => e.LastName, 3);
			Map(se => se.Patronymic, e => e.Patronymic, 1);
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
