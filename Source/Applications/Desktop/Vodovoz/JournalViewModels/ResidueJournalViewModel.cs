using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels;

namespace Vodovoz.JournalViewModels
{
	public class ResidueJournalViewModel : EntityJournalViewModelBase<Residue, ResidueViewModel, ResidueJournalNode>
	{
		private readonly ResidueFilterViewModel _filterViewModel;

		public ResidueJournalViewModel(
			ResidueFilterViewModel filterViewModel,
			INavigationManager navigationManager,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService) 
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));

			_filterViewModel.OnFiltered += OnFfilterViewModelFiltered;
			JournalFilter = _filterViewModel;

			TabName = "Журнал остатков";
			UseSlider = true;

			UpdateOnChanges(typeof(Residue));
		}

		private void OnFfilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override IQueryOver<Residue> ItemsQuery(IUnitOfWork uow)
		{
			Counterparty counterpartyAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			ResidueJournalNode resultAlias = null;
			Residue residueAlias = null;
			DeliveryPoint deliveryPointAlias = null;

			var residueQuery = uow.Session.QueryOver(() => residueAlias)
				.JoinQueryOver(() => residueAlias.Customer, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => residueAlias.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => residueAlias.LastEditAuthor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => residueAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			if(_filterViewModel != null)
			{
				var dateCriterion = Projections.SqlFunction(
					   new SQLFunctionTemplate(
						   NHibernateUtil.Date,
						   "Date(?1)"
						  ),
					   NHibernateUtil.Date,
					   Projections.Property(() => residueAlias.Date)
					);

				if(_filterViewModel.StartDate.HasValue)
				{
					residueQuery.Where(Restrictions.Ge(dateCriterion, _filterViewModel.StartDate.Value));
				}

				if(_filterViewModel.EndDate.HasValue)
				{
					residueQuery.Where(Restrictions.Le(dateCriterion, _filterViewModel.EndDate.Value));
				}
			}

			residueQuery.Where(GetSearchCriterion(
				() => residueAlias.Id,
				() => counterpartyAlias.Name,
				() => deliveryPointAlias.CompiledAddress
			));

			var resultQuery = residueQuery
				.SelectList(list => list
				   .Select(() => residueAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => residueAlias.Date).WithAlias(() => resultAlias.Date)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
				   .Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.DeliveryPoint)
				   .Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
				   .Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
				   .Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
				   .Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
				   .Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
				   .Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
				   .Select(() => residueAlias.LastEditTime).WithAlias(() => resultAlias.LastEditedTime)
				)
				.OrderBy(() => residueAlias.Date).Desc
				.TransformUsing(Transformers.AliasToBean<ResidueJournalNode>());

			return resultQuery;
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFfilterViewModelFiltered;

			base.Dispose();
		}
	}
}
