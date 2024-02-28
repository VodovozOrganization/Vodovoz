using QS.DomainModel.UoW;
using System;

namespace Vodovoz.Core.Application.Entity
{
	public class EntityModel<TEntity> : IDisposable, ISaveModel 
		where TEntity : class, new()
	{
		private readonly IEntityIdentifier _entityId;

		private bool _saved;
		public bool IsNewEntity => _entityId.IsNewEntity && !_saved;
		public TEntity Entity { get; }
		public IUnitOfWork UoW { get; }

		public EntityModel(IEntityIdentifier entityId, IUnitOfWorkFactory uowFactory)
		{
			if(uowFactory is null)
			{
				throw new ArgumentNullException(nameof(uowFactory));
			}
			_entityId = entityId ?? throw new ArgumentNullException(nameof(entityId));

			UoW = uowFactory.CreateWithoutRoot();

			if(_entityId.IsNewEntity)
			{
				Entity = new TEntity();
			}
			else
			{
				Entity = UoW.Session.Get<TEntity>(_entityId.Id);
			}
		}

		public virtual void Save()
		{
			UoW.Save(Entity);
			UoW.Commit();
			_saved = true;
		}

		public virtual void Dispose()
		{
			UoW?.Dispose();
		}
	}
}
