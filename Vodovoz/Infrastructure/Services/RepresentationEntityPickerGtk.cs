using System;
using QS.RepresentationModel.GtkUI;
using QS.Tdi;
using System.Linq;
using Vodovoz.Infrastructure.Journal;
using QS.Project.Dialogs;
using QS.DomainModel.Entity;

namespace Vodovoz.Infrastructure.Services
{
	public class RepresentationEntityPickerGtk : IRepresentationEntityPicker
	{
		private readonly IRepresentationJournalFactory journalFactory;

		public RepresentationEntityPickerGtk(IRepresentationJournalFactory journalFactory)
		{
			this.journalFactory = journalFactory ?? throw new ArgumentNullException(nameof(journalFactory));
		}

		public void OpenSingleSelectionJournal<TEntity>(IRepresentationModel model, Action<TEntity[]> onSelectedAction, Action<ITdiTab> openTabAction)
			where TEntity : class, IDomainObject
		{
			if(model == null) {
				throw new ArgumentNullException(nameof(model));
			}

			if(onSelectedAction == null) {
				throw new ArgumentNullException(nameof(onSelectedAction));
			}

			if(openTabAction == null) {
				throw new ArgumentNullException(nameof(openTabAction));
			}

			if(model.EntityType != typeof(TEntity)) {
				throw new InvalidOperationException($"Тип в модели представления \"{nameof(model)}\" должен совпадать с типом сущности \"{typeof(TEntity).Name}\" ");
			}

			var journal = journalFactory.CreateJournal(model);
			journal.Mode = JournalSelectMode.Single;
			journal.ObjectSelected += (sender, e) => {
				TEntity[] selectedEntities = e.Selected.OfType<TEntity>().ToArray();
				onSelectedAction.Invoke(selectedEntities);
			};
			openTabAction.Invoke(journal);
		}

		public void OpenMultipleSelectionJournal<TEntity>(IRepresentationModel model, Action<TEntity[]> onSelectedAction, Action<ITdiTab> openTabAction)
			where TEntity : class, IDomainObject
		{
			if(model == null) {
				throw new ArgumentNullException(nameof(model));
			}

			if(onSelectedAction == null) {
				throw new ArgumentNullException(nameof(onSelectedAction));
			}

			if(openTabAction == null) {
				throw new ArgumentNullException(nameof(openTabAction));
			}

			if(model.EntityType != typeof(TEntity)) {
				throw new InvalidOperationException($"Тип в модели представления \"{nameof(model)}\" должен совпадать с типом сущности \"{typeof(TEntity).Name}\" ");
			}

			var journal = journalFactory.CreateJournal(model);
			journal.Mode = JournalSelectMode.Multiple;
			journal.ObjectSelected += (sender, e) => {
				TEntity[] selectedEntities = e.Selected.OfType<TEntity>().ToArray();
				onSelectedAction.Invoke(selectedEntities);
			};
			openTabAction.Invoke(journal);
		}
	}
}
