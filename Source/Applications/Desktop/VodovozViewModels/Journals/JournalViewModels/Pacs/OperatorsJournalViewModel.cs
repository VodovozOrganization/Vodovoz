using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.ViewModels.Employees;
using Vodovoz.Presentation.ViewModels.Pacs;
using Vodovoz.ViewModels.Journals.JournalNodes.Pacs;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Pacs
{
	public class OperatorsJournalViewModel : EntityJournalViewModelBase<Operator, PacsOperatorReferenceBookViewModel, OperatorNode>
	{
		public OperatorsJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService
		) : base(uowFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			Title = "Рабочие смены";
		}

		protected override IQueryOver<Operator> ItemsQuery(IUnitOfWork uow)
		{
			Operator operatorAlias = null;
			Employee employeeAlias = null;
			WorkShift workShiftAlias = null;
			OperatorNode resultAlias = null;

			return uow.Session.QueryOver(() => operatorAlias)
				.JoinEntityAlias(() => employeeAlias, () => operatorAlias.Id == employeeAlias.Id, JoinType.InnerJoin)
				.JoinAlias(() => operatorAlias.WorkShift, () => workShiftAlias)
				.SelectList(list => list
					.Select(() => employeeAlias).WithAlias(() => resultAlias.Operator)
					.Select(() => workShiftAlias.Name).WithAlias(() => resultAlias.WorkshiftName)
				)
				.TransformUsing(Transformers.AliasToBean<OperatorNode>());
		}

		protected override void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<PacsOperatorReferenceBookViewModel, IEntityIdentifier>(this, EntityIdentifier.NewEntity());
		}

		protected override void EditEntityDialog(OperatorNode node)
		{
			NavigationManager.OpenViewModel<PacsOperatorReferenceBookViewModel, IEntityIdentifier>(this, EntityIdentifier.OpenEntity(node.Operator.Id));
		}
	}
}
