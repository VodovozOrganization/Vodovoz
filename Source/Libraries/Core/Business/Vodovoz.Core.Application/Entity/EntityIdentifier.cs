using System;

namespace Vodovoz.Core.Application.Entity
{
	public class EntityIdentifier : IEntityIdentifier
	{
		public bool IsNewEntity { get; private set; }

		public object Id { get; private set; }

		public static IEntityIdentifier NewEntity()
		{
			return new EntityIdentifier { IsNewEntity = true, Id = null};
		}

		public static IEntityIdentifier OpenEntity(object id)
		{
			return new EntityIdentifier { IsNewEntity = false, Id = id };
		}

		public static IEntityIdentifier OpenByCompositeKey<TEntity>(Action<TEntity> keysSetter)
			where TEntity : class, new()
		{
			var identifier = new TEntity();
			keysSetter(identifier);
			return new EntityIdentifier { IsNewEntity = false, Id = identifier };
		}
	}
}
