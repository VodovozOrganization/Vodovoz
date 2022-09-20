using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints.ComplaintResults;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Complaints.ComplaintResults
{
	public class ComplaintResultsOfEmployeesJournalViewModel
		: SingleEntityJournalViewModelBase<ComplaintResultOfEmployees, ComplaintResultOfEmployeesViewModel, ComplaintResultsOfEmployeesJournalNode>
	{
		public ComplaintResultsOfEmployeesJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false
		) : base(uowFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			TabName = "Журнал результатов рекламаций по сотрудникам";
			
			UpdateOnChanges(typeof(ComplaintResultOfEmployees));
		}
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultEditAction();
			CreateDefaultAddActions();
		}
		
		protected override Func<IUnitOfWork, IQueryOver<ComplaintResultOfEmployees>> ItemsSourceQueryFunction => uow =>
		{
			ComplaintResultOfEmployees resultOfEmployeesAlias = null;
			ComplaintResultsOfEmployeesJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver(() => resultOfEmployeesAlias);

			query.Where(GetSearchCriterion(() => resultOfEmployeesAlias.Name));
			
			var result = query.SelectList(list => list
					.Select(c => c.Id).WithAlias(() => resultAlias.Id)
					.Select(c => c.Name).WithAlias(() => resultAlias.Name)
					.Select(c => c.IsArchive).WithAlias(() => resultAlias.IsArchive))
				.TransformUsing(Transformers.AliasToBean<ComplaintResultsOfEmployeesJournalNode>());
			
			return result;
		};

		protected override Func<ComplaintResultOfEmployeesViewModel> CreateDialogFunction => () =>
			new ComplaintResultOfEmployeesViewModel(
				EntityUoWBuilder.ForCreate(),
				UnitOfWorkFactory,
				commonServices);
		
		protected override Func<ComplaintResultsOfEmployeesJournalNode, ComplaintResultOfEmployeesViewModel> OpenDialogFunction
			=> n =>
				new ComplaintResultOfEmployeesViewModel(
					EntityUoWBuilder.ForOpen(n.Id),
					UnitOfWorkFactory,
					commonServices);
	}
}
