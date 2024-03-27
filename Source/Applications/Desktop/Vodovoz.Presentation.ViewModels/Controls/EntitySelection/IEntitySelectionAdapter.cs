using QS.DomainModel.Entity;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public interface IEntitySelectionAdapter<TEntity>
		where TEntity : class, IDomainObject
	{
		TEntity GetEntityByNode(object node);
		EntitySelectionViewModel<TEntity> EntitySelectionViewModel { set; }
	}
}
