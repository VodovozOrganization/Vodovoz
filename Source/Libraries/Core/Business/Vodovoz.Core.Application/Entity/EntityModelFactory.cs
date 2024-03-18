using QS.DomainModel.UoW;
using System;

namespace Vodovoz.Core.Application.Entity
{
	public class EntityModelFactory
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public EntityModelFactory(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public EntityModel<TEntity> Create<TEntity>(IEntityIdentifier entityId)
			where TEntity : class, new()
		{
			return new EntityModel<TEntity>(entityId, _uowFactory);
		}
	}
}
