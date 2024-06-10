using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using Vodovoz.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;

namespace Vodovoz.ViewModels.Dialogs.Complaints
{
	public class ComplaintsJournalsViewModel : TabViewModelBase, IComplaintsInfoProvider
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private JournalViewModelBase _journal;
		private ILifetimeScope _lifetimeScope;
		private ComplaintFilterViewModel _filterViewModel;

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		public ComplaintsJournalsViewModel(
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IUnitOfWorkFactory unitOfWorkFactory,
			ILifetimeScope lifetimeScope,
			Action<ComplaintFilterViewModel> filterConfig = null)
			: base(commonServices.InteractiveService, navigationManager)
		{
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

			_filterViewModel.JournalViewModel = this;
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
			DisposeOldJournal();
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
			DisposeOldJournal();
			UoW.Dispose();
			
			if(_filterViewModel != null)
			{
				_filterViewModel.Dispose();
				_filterViewModel = null;
			}
			
			_lifetimeScope = null;
			base.Dispose();
		}
		
		private void DisposeOldJournal()
		{
			if(Journal?.DataLoader != null)
			{
				Journal.DataLoader.ItemsListUpdated -= OnDataLoaderItemsListUpdated;
			}

			if(Journal is IChangeComplaintJournal journal)
			{
				journal.ChangeView -= ChangeView;
			}
			
			Journal?.Dispose();
		}

		private ComplaintsJournalViewModel GetStandartJournal()
		{
			var journal = _lifetimeScope.Resolve<ComplaintsJournalViewModel>(
				new TypedParameter(typeof(ComplaintFilterViewModel), _filterViewModel));
			return journal;
		}

		private ComplaintsWithDepartmentsReactionJournalViewModel GetJournalWithDepartmentsReaction()
		{
			var journal = _lifetimeScope.Resolve<ComplaintsWithDepartmentsReactionJournalViewModel>(
				new TypedParameter(typeof(ComplaintFilterViewModel), _filterViewModel));
			return journal;
		}
	}
}
