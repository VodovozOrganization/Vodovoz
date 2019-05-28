using System;
using QS.DomainModel.Entity;
using QS.RepresentationModel.GtkUI;
using QS.Tdi;
namespace Vodovoz.Infrastructure.Services
{
	public interface IRepresentationEntityPicker
	{
		void OpenSingleSelectionJournal<TEntity>(IRepresentationModel model, Action<TEntity[]> onSelectedAction, Action<ITdiTab> openTabAction)
			where TEntity : class, IDomainObject;

		void OpenMultipleSelectionJournal<TEntity>(IRepresentationModel model, Action<TEntity[]> onSelectedAction, Action<ITdiTab> openTabAction)
			where TEntity : class, IDomainObject;
	}
}
