using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.Core.Domain.Goods.Recomendations;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Goods
{
	public class RecomendationViewModel : EntityTabViewModelBase<Recomendation>
	{
		private IEnumerable<RecomendationItem> _selectedRecomendationItems = Enumerable.Empty<RecomendationItem>();

		public RecomendationViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IDomainEntityNodeInMemoryCacheRepository<Nomenclature> nomenclatureCacheRepository)
			: base(
				  uowBuilder,
				  unitOfWorkFactory,
				  commonServices,
				  navigation)
		{
			NomenclatureCacheRepository = nomenclatureCacheRepository
				?? throw new ArgumentNullException(nameof(nomenclatureCacheRepository));

			SaveCommand = new DelegateCommand(SaveAndClose, () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, x => x.CanSave);

			SetPropertyChangeRelation(
				recomendation => recomendation.Id,
				() => CanSave);

			CancelCommand = new DelegateCommand(() => Close(HasChanges, CloseSource.Cancel));

			AddNomenclatureCommand = new DelegateCommand(OpenNomenclatureSelectJournal);
			RemoveNomenclatureCommand = new DelegateCommand(RemoveNomenclature);

			UpdatePriorityCommand = new DelegateCommand(UpdatePriority);
		}

		private void UpdatePriority()
		{
			Entity.UpdatePriority();
		}

		[PropertyChangedAlso(nameof(SelectedRecomendationItemObjects))]
		public IEnumerable<RecomendationItem> SelectedRecomendationItems
		{
			get => _selectedRecomendationItems;
			set => SetField(ref _selectedRecomendationItems, value);
		}

		public object[] SelectedRecomendationItemObjects
		{
			get => SelectedRecomendationItems.Cast<object>().ToArray();
			set => SelectedRecomendationItems = value.OfType<RecomendationItem>();
		}

		public IDomainEntityNodeInMemoryCacheRepository<Nomenclature> NomenclatureCacheRepository { get; }

		public DelegateCommand SaveCommand { get; }

		public DelegateCommand CancelCommand { get; }

		public DelegateCommand AddNomenclatureCommand { get; }

		public DelegateCommand RemoveNomenclatureCommand { get; }
		public DelegateCommand UpdatePriorityCommand { get; }

		public bool CanSave => Entity.Id == 0 ? PermissionResult.CanCreate : PermissionResult.CanUpdate;

		private void OpenNomenclatureSelectJournal()
		{
			NavigationManager.OpenViewModel<NomenclaturesJournalViewModel>(
				this,
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Multiple;
					viewModel.OnSelectResult += OnNomenclatureToAddSelected;
				});
		}

		private void OnNomenclatureToAddSelected(object sender, JournalSelectedEventArgs e)
		{
			if(sender is NomenclaturesJournalViewModel nomenclaturesJournalViewModel)
			{
				nomenclaturesJournalViewModel.OnSelectResult -= OnNomenclatureToAddSelected;
			}

			var nomenclatureIds = e.SelectedObjects.Cast<NomenclatureJournalNode>().Select(x => x.Id).ToArray();

			var currentLastIndex = Entity.Items.Count;

			for(var i = 0; i < nomenclatureIds.Length; i++)
			{
				Entity.TryAddItem(nomenclatureIds[i], currentLastIndex + i + 1);
			}
		}

		private void RemoveNomenclature()
		{
			foreach(var selectedItem in SelectedRecomendationItems)
			{
				Entity.TryRemoveItem(selectedItem.NomenclatureId);
			}
		}
	}
}
