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
	public class GroupGtinJournalViewModel : JournalViewModelBase
	{
		private Nomenclature _nomenclature;
		private readonly IInteractiveService _interactiveService;

		public GroupGtinJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager)
			: base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			Title = "Журнал групповых GTIN";

			DataLoader = new AnyDataLoader<GroupGtin>(GetItems);

			CreateNodeActions();
			UpdateOnChanges(typeof(GroupGtin));

			Refresh();
		}

		public Nomenclature Nomenclature
		{
			get => _nomenclature;
			set
			{
				SetField(ref _nomenclature, value);

				Title = $"Журнал групповых GTIN \"{_nomenclature.Name}\"";

				Refresh();
			}
		}

		protected IList<GroupGtin> GetItems(CancellationToken token)
		{
			var result = _nomenclature?.GroupGtins ?? new ObservableList<GroupGtin>();

			if(Search.SearchValues != null && Search.SearchValues.Any())
			{
				var gtinFilter = Search.SearchValues.FirstOrDefault();
				result = new ObservableList<GroupGtin>(result.Where(x => x.GtinNumber.Contains(gtinFilter)));
			}

			return result;
		}

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();

			var addAction = new JournalAction("Добавить",
					(selected) => true,
					(selected) => Nomenclature != null,
					(selected) => CreateDialog(),
					"Insert"
					);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
					(selected) => selected.Count() == 1,
					(selected) => Nomenclature != null,
					(selected) => EditDialog(selected.Cast<GroupGtin>().FirstOrDefault())
					);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}

			var deleteAction = new JournalAction("Удалить",
					(selected) => true,
					(selected) => Nomenclature != null,
					(selected) => Delete(selected.Cast<GroupGtin>()),
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

		private void CreateDialog()
		{
			if(Nomenclature is null)
			{
				throw new InvalidOperationException("Не установлена номенклатура");
			}

			NavigationManager.OpenViewModel<GroupGtinViewModel, INavigationManager, Nomenclature>(
				this, NavigationManager, _nomenclature, OpenPageOptions.AsSlave);
		}

		private void EditDialog(GroupGtin groupGtin)
		{
			if(Nomenclature is null)
			{
				throw new InvalidOperationException("Не установлена номенклатура");
			}

			NavigationManager.OpenViewModel<GroupGtinViewModel, INavigationManager, GroupGtin, Nomenclature>(
				this, NavigationManager, groupGtin, _nomenclature, OpenPageOptions.AsSlave);
		}

		private void Delete(IEnumerable<GroupGtin> groupGtins)
		{
			if(Nomenclature is null)
			{
				throw new InvalidOperationException("Не установлена номенклатура");
			}

			if(!_interactiveService.Question("Удалить выбранные Gtin?"))
			{
				return;
			}

			foreach(GroupGtin node in groupGtins)
			{
				_nomenclature.GroupGtins.Remove(node);
			}
		}
	}
}
