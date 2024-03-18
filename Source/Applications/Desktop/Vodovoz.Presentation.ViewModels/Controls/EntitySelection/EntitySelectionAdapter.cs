using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using DomainModel = QS.DomainModel;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class EntitySelectionAdapter<TEntity> : IEntitySelectionAdapter<TEntity>, IDisposable
		where TEntity : class, IDomainObject
	{
		private readonly IUnitOfWork _uow;

		public EntitySelectionAdapter(IUnitOfWork uow)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			DomainModel.NotifyChange.NotifyConfiguration.Instance?.BatchSubscribeOnEntity(ExternalEntityChangeEventMethod, typeof(TEntity));
		}

		public EntitySelectionViewModel<TEntity> EntitySelectionViewModel { set; get; }


		public TEntity GetEntityByNode(object node)
		{
			var entity = _uow.GetById<TEntity>(node.GetId());
			return entity;
		}

		void ExternalEntityChangeEventMethod(DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			if(EntitySelectionViewModel is null)
			{
				return;
			}

			var foundUpdatedObject = changeEvents.FirstOrDefault(e => DomainHelper.EqualDomainObjects(e.Entity, EntitySelectionViewModel.Entity));
			if(foundUpdatedObject != null && _uow.Session.IsOpen && _uow.Session.Contains(EntitySelectionViewModel.Entity))
			{
				if(foundUpdatedObject.EventType == DomainModel.NotifyChange.TypeOfChangeEvent.Delete)
				{
					EntitySelectionViewModel.Entity = null;
				}
				else
				{
					_uow.Session.Refresh(EntitySelectionViewModel.Entity);
				}
			}
		}

		public void Dispose()
		{
			DomainModel.NotifyChange.NotifyConfiguration.Instance?.UnsubscribeAll(this);
			EntitySelectionViewModel = null;
		}
	}
}
