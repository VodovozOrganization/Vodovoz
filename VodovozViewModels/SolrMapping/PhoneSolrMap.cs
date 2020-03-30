using System;
using QS.Contacts;
using SolrSearch;
using SolrSearch.Mapping;
using Vodovoz.SolrModel;

namespace Vodovoz.SolrMapping
{
	public class PhoneSolrMap : SolrOrmSourceClassMap<PhoneSolrEntity, Phone, PhoneSolrEntityFactory>
	{
		public PhoneSolrMap()
		{
			Map(se => se.Id, e => e.Id, 999999);
			Map(se => se.Number, e => e.Number, 1);
			Map(se => se.DigitsNumber, e => e.DigitsNumber, 1);
		}
	}

	public class PhoneSolrEntityFactory : SolrEntityFactoryBase<PhoneSolrEntity>
	{
		public override PhoneSolrEntity CreateEntity(EntityContentProvider entityContentProvider)
		{
			PhoneSolrEntity entity = new PhoneSolrEntity();
			entity.Id = entityContentProvider.GetPropertyContent<int>(nameof(entity.Id));
			entity.Number = entityContentProvider.GetPropertyContent<string>(nameof(entity.Number));
			entity.DigitsNumber = entityContentProvider.GetPropertyContent<string>(nameof(entity.DigitsNumber));
			return entity;
		}
	}
}
