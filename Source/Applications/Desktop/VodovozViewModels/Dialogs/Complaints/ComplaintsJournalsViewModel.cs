using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.RepresentationModel;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Dialogs.Complaints
{
	public class ComplaintsJournalsViewModel : TabViewModelBase, IComplaintsInfoProvider
	{
		private JournalViewModelBase _journal;
		private readonly IComplaintsJournalFactory _complaintsJournalFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ILifetimeScope _lifetimeScope;
		private ComplaintFilterViewModel _filterViewModel;

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		public ComplaintsJournalsViewModel(
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IComplaintsJournalFactory complaintsJournalFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			ILifetimeScope lifetimeScope,
			Action<ComplaintFilterViewModel> filterConfig = null)
			: base(commonServices.InteractiveService, navigationManager)
		{
			_complaintsJournalFactory = complaintsJournalFactory ?? throw new ArgumentNullException(nameof(complaintsJournalFactory));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			CreateFilter(filterConfig);

			Title = "Журнал рекламаций";
			ChangeView(typeof(ComplaintsJournalViewModel));
		}

		private void CreateFilter(Action<ComplaintFilterViewModel> filterConfig)
		{
			Autofac.Core.Parameter[] parameters = {
				new TypedParameter(typeof(ITdiTab), this)
			};

			_filterViewModel = _lifetimeScope.Resolve<ComplaintFilterViewModel>(parameters);

			if(filterConfig != null)
			{
				_filterViewModel.SetAndRefilterAtOnce(filterConfig);
			}

			_filterViewModel.DisposeOnDestroy = false;
		}

		private void ChangeView(Type switchToType)
		{
			var newJournal = GetNewJournal(switchToType);
			UpdateJournal(newJournal);
		}

		private JournalViewModelBase GetNewJournal(Type switchToType)
		{
			if(switchToType == typeof(ComplaintsWithDepartmentsReactionJournalViewModel))
			{
				var withDepartmentsReactionComplaintsJournal = GetJournalWithDepartmentsReaction();
				withDepartmentsReactionComplaintsJournal.ChangeView += ChangeView;
				withDepartmentsReactionComplaintsJournal.DataLoader.ItemsListUpdated += OnDataLoaderItemsListUpdated;
				return withDepartmentsReactionComplaintsJournal;
			}

			if(switchToType == typeof(ComplaintsJournalViewModel))
			{
				var standartComplaintsJournal = GetStandartJournal();
				standartComplaintsJournal.ChangeView += ChangeView;
				standartComplaintsJournal.DataLoader.ItemsListUpdated += OnDataLoaderItemsListUpdated;
				return standartComplaintsJournal;
			}

			throw new InvalidOperationException($"Тип {switchToType} не поддерживается");
		}

		private void OnDataLoaderItemsListUpdated(object sender, EventArgs e)
		{
			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(null));
		}

		private void UpdateJournal(JournalViewModelBase newJournal)
		{
			if(Journal?.DataLoader != null)
			{
				Journal.DataLoader.ItemsListUpdated -= OnDataLoaderItemsListUpdated;
			}
			Journal?.Dispose();
			Journal = newJournal;
		}

		public JournalViewModelBase Journal
		{
			get => _journal;
			set => SetField(ref _journal, value);
		}

		public ComplaintFilterViewModel ComplaintsFilterViewModel => _filterViewModel;

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.ComplaintPanelView };

		public IUnitOfWork UoW => _unitOfWorkFactory.CreateWithoutRoot();

		public override void Dispose()
		{
			if(Journal?.DataLoader != null)
			{
				Journal.DataLoader.ItemsListUpdated -= OnDataLoaderItemsListUpdated;
			}
			UoW.Dispose();
			_filterViewModel.Dispose();
			base.Dispose();
		}

		private ComplaintsJournalViewModel GetStandartJournal()
		{
			var journal = _complaintsJournalFactory.GetStandartJournal(_filterViewModel);
			journal.ParentTab = this;
			return journal;
		}

		private ComplaintsWithDepartmentsReactionJournalViewModel GetJournalWithDepartmentsReaction()
		{
			var journal = _complaintsJournalFactory.GetJournalWithDepartmentsReaction(_filterViewModel);
			journal.ParentTab = this;
			return journal;
		}
	}
}
