using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;
using VodovozBusiness.Domain.Goods;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	public class GtinJournalViewModel : JournalViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private Nomenclature _nomenclature;

		public GtinJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, Nomenclature nomenclature)
			: base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_nomenclature = nomenclature ?? throw new ArgumentNullException(nameof(nomenclature));

			DataLoader = new AnyDataLoader<Gtin>(GetItems);

			Title = $"Журнал Gtin для номенклатуры {nomenclature.Id} {nomenclature.Name}";

			CreateNodeActions();

			UpdateOnChanges(typeof(Gtin));
		}

		protected IList<Gtin> GetItems(CancellationToken token)
		{
			var result = _nomenclature.Gtins;

			if(Search.SearchValues != null && Search.SearchValues.Any())
			{
				var gtinFilter = Search.SearchValues.FirstOrDefault();
				result = new ObservableList<Gtin>(result.Where(x => x.GtinNumber.Contains(gtinFilter)));
			}

			return result;
		}

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();

			var addAction = new JournalAction("Добавить",
					(selected) => true,
					(selected) => true,
					(selected) => CreateDialog(),
					"Insert"
					);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
					(selected) => selected.Count() == 1,
					(selected) => true,
					(selected) => EditDialog(selected.Cast<Gtin>().FirstOrDefault())
					);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}

			var deleteAction = new JournalAction("Удалить",
					(selected) => true,
					(selected) => true,
					(selected) => Delete(selected.Cast<Gtin>()),
					"Delete"
					);
			NodeActionsList.Add(deleteAction);

			var closeJournalAction = new JournalAction("Готово",
				(selected) => true,
				(selected) => true,
				(selected) => Close(false, CloseSource.Self)
			);
			NodeActionsList.Add(closeJournalAction);
		}

		protected virtual void CreateDialog()
		{
			NavigationManager.OpenViewModel<GtinViewModel, INavigationManager, Nomenclature>(this, NavigationManager, _nomenclature, OpenPageOptions.AsSlave);
		}

		protected virtual void EditDialog(Gtin gtin)
		{
			if(gtin == null)
			{
				return;
			}

			NavigationManager.OpenViewModel<GtinViewModel, INavigationManager, Gtin, Nomenclature>(this, NavigationManager, gtin, _nomenclature, OpenPageOptions.AsSlave);
		}

		protected virtual void Delete(IEnumerable<Gtin> nodes)
		{
			if(!_interactiveService.Question("Удалить выбранные Gtin?"))
			{
				return;
			}

			foreach(Gtin node in nodes)
			{
				_nomenclature.Gtins.Remove(node);
			}
		}
	}
}
