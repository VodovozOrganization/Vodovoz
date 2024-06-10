using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using Vodovoz.Core.Application.Entity;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.ViewModels.Pacs;
using Vodovoz.Presentation.ViewModels.Pacs.Journals;
using Vodovoz.ViewModels.Journals.JournalNodes.Pacs;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Pacs
{
	public class OperatorsJournalViewModel : EntityJournalViewModelBase<Operator, PacsOperatorReferenceBookViewModel, OperatorNode>
	{
		private readonly OperatorFilterViewModel _operatorFilterViewModel;

		public OperatorsJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService,
			IDeleteEntityService deleteEntityService,
			IEntityChangeWatcher entityNotifier,
			OperatorFilterViewModel operatorFilterViewModel)
			: base(
				  uowFactory,
				  interactiveService,
				  navigationManager,
				  deleteEntityService,
				  currentPermissionService)
		{
			if(entityNotifier is null)
			{
				throw new ArgumentNullException(nameof(entityNotifier));
			}

			_operatorFilterViewModel = operatorFilterViewModel ?? throw new ArgumentNullException(nameof(operatorFilterViewModel));

			JournalFilter = operatorFilterViewModel;

			Title = "Операторы";

			VisibleDeleteAction = false;

			UpdateOnChanges(typeof(Operator));
			_operatorFilterViewModel.OnFiltered += OnFilterFiltered;
		}

		private void OnFilterFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override IQueryOver<Operator> ItemsQuery(IUnitOfWork uow)
		{
			Operator operatorAlias = null;
			Employee employeeAlias = null;
			WorkShift workShiftAlias = null;
			OperatorNode resultAlias = null;

			var query = uow.Session.QueryOver(() => operatorAlias)
				.JoinEntityAlias(() => employeeAlias, () => operatorAlias.Id == employeeAlias.Id, JoinType.InnerJoin)
				.JoinAlias(() => operatorAlias.WorkShift, () => workShiftAlias)
				.SelectList(list => list
					.Select(() => operatorAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(Projections.Entity(() => employeeAlias)).WithAlias(() => resultAlias.Operator)
					.Select(() => workShiftAlias.Name).WithAlias(() => resultAlias.WorkshiftName)
					.Select(() => operatorAlias.PacsEnabled).WithAlias(() => resultAlias.PacsEnabled))
				.TransformUsing(Transformers.AliasToBean<OperatorNode>());

			if(_operatorFilterViewModel.OperatorIsWorkingFilteringMode != OperatorFilterViewModel.OperatorIsWorkingFilteringModeEnum.All)
			{
				switch(_operatorFilterViewModel.OperatorIsWorkingFilteringMode)
				{
					case OperatorFilterViewModel.OperatorIsWorkingFilteringModeEnum.Enabled:
						query.Where(() => operatorAlias.PacsEnabled);
						break;
					case OperatorFilterViewModel.OperatorIsWorkingFilteringModeEnum.Disabled:
						query.Where(() => !operatorAlias.PacsEnabled);
						break;
				}
			}

			query.Where(GetSearchCriterion(
				() => operatorAlias.Id,
				() => employeeAlias.LastName,
				() => employeeAlias.Name,
				() => employeeAlias.Patronymic,
				() => workShiftAlias.Name));

			return query;
		}

		protected override void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<PacsOperatorReferenceBookViewModel, IEntityIdentifier>(this, EntityIdentifier.NewEntity());
		}

		protected override void EditEntityDialog(OperatorNode node)
		{
			NavigationManager.OpenViewModel<PacsOperatorReferenceBookViewModel, IEntityIdentifier>(this, EntityIdentifier.OpenEntity(node.Operator.Id));
		}

		public override void Dispose()
		{
			if(_operatorFilterViewModel != null)
			{
				_operatorFilterViewModel.OnFiltered += OnFilterFiltered;
			}

			base.Dispose();
		}
	}
}
