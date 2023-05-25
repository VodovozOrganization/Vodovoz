using System;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Utilities.Text;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.ViewModels.Journals.FilterViewModels.Counterparties;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Client
{
	public class ExternalCounterpartiesMatchingJournalViewModel : JournalViewModelBase
	{
		private readonly ILifetimeScope _scope;
		private ExternalCounterpartiesMatchingJournalFilterViewModel _filterViewModel;

		public ExternalCounterpartiesMatchingJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ILifetimeScope scope,
			Action<ExternalCounterpartiesMatchingJournalFilterViewModel> filterParams = null)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			var dataLoader = new ThreadDataLoader<ExternalCounterpartyMatchingJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;

			TabName = "Сопоставление клиентов из внешних источников";

			ConfigureFilter(filterParams);
			CreateNodeActions();
			UpdateOnChanges(typeof(ExternalCounterpartyMatching));
		}

		private void ConfigureFilter(Action<ExternalCounterpartiesMatchingJournalFilterViewModel> filterParams)
		{
			_filterViewModel = _scope.Resolve<ExternalCounterpartiesMatchingJournalFilterViewModel>();
			JournalFilter = _filterViewModel;
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;

			if(filterParams != null)
			{
				_filterViewModel.SetAndRefilterAtOnce(filterParams);
			}
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateEditAction();
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<ExternalCounterpartyMatchingJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					return true;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<ExternalCounterpartyMatchingJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					var selectedNode = selectedNodes.First();
					NavigationManager.OpenViewModel<ExternalCounterpartyMatchingViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForOpen(selectedNode.GetId()), OpenPageOptions.AsSlave);
				}
			);
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		private IQueryOver<ExternalCounterpartyMatching> ItemsQuery(IUnitOfWork uow)
		{
			ExternalCounterparty externalCounterpartyAlias = null;
			Counterparty counterpartyAlias = null;
			ExternalCounterpartyMatchingJournalNode resultAlias = null;
			Phone phoneAlias = null;
			
			var query = uow.Session.QueryOver<ExternalCounterpartyMatching>()
				.Left.JoinAlias(ecm => ecm.AssignedExternalCounterparty, () => externalCounterpartyAlias)
				.Left.JoinAlias(() => externalCounterpartyAlias.Phone, () => phoneAlias)
				.Left.JoinAlias(() => phoneAlias.Counterparty, () => counterpartyAlias);

			if(_filterViewModel.StartDate.HasValue)
			{
				query.Where(ecm => ecm.Created >= _filterViewModel.StartDate.Value);
			}
			
			if(_filterViewModel.EndDate.HasValue)
			{
				query.Where(ecm => ecm.Created <= _filterViewModel.EndDate.Value);
			}
			
			if(_filterViewModel.MatchingStatus.HasValue)
			{
				query.Where(ecm => ecm.Status == _filterViewModel.MatchingStatus.Value);
			}
			
			if(!string.IsNullOrWhiteSpace(_filterViewModel.PhoneNumber))
			{
				query.Where(
					Restrictions.Like(
						Projections.Property<ExternalCounterpartyMatching>(ecm => ecm.PhoneNumber),
						_filterViewModel.PhoneNumber,
						MatchMode.Anywhere));
			}
			
			if(_filterViewModel.CounterpartyId.HasValue)
			{
				query.Where(() => counterpartyAlias.Id == _filterViewModel.CounterpartyId.Value);
			}

			var result = query
				.SelectList(list => list
					.Select(ecm => ecm.Id).WithAlias(() => resultAlias.Id)
					.Select(ecm => ecm.Created).WithAlias(() => resultAlias.Created)
					.Select(ecm => ecm.CounterpartyFrom).WithAlias(() => resultAlias.CounterpartyFrom)
					.Select(ecm => ecm.Status).WithAlias(() => resultAlias.Status)
					.Select(ecm => ecm.PhoneNumber).WithAlias(() => resultAlias.PhoneNumber)
					.Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.CounterpartyId)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
				)
				.OrderBy(ecm => ecm.Status).Asc
				.ThenBy(ecm => ecm.Created).Desc
				.TransformUsing(Transformers.AliasToBean<ExternalCounterpartyMatchingJournalNode>());

			return result;
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
