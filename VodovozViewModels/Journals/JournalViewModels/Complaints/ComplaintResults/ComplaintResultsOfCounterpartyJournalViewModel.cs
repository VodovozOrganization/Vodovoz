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
	public class ComplaintResultsOfCounterpartyJournalViewModel
		: SingleEntityJournalViewModelBase<ComplaintResultOfCounterparty, ComplaintResultOfCounterpartyViewModel, ComplaintResultsOfCounterpartyJournalNode>
	{
		public ComplaintResultsOfCounterpartyJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false
		) : base(uowFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			TabName = "Журнал результатов рекламаций по клиенту";
			
			UpdateOnChanges(typeof(ComplaintResultOfCounterparty));
		}
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultEditAction();
			CreateDefaultAddActions();
		}
		
		protected override Func<IUnitOfWork, IQueryOver<ComplaintResultOfCounterparty>> ItemsSourceQueryFunction => uow =>
		{
			ComplaintResultOfCounterparty resultOfCounterpartyAlias = null;
			ComplaintResultsOfCounterpartyJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver(() => resultOfCounterpartyAlias);

			query.Where(GetSearchCriterion(() => resultOfCounterpartyAlias.Name));
			
			var result = query.SelectList(list => list
					.Select(c => c.Id).WithAlias(() => resultAlias.Id)
					.Select(c => c.Name).WithAlias(() => resultAlias.Name)
					.Select(c => c.IsArchive).WithAlias(() => resultAlias.IsArchive))
				.TransformUsing(Transformers.AliasToBean<ComplaintResultsOfCounterpartyJournalNode>());
			
			return result;
		};

		protected override Func<ComplaintResultOfCounterpartyViewModel> CreateDialogFunction => () =>
			new ComplaintResultOfCounterpartyViewModel(
				EntityUoWBuilder.ForCreate(),
				UnitOfWorkFactory,
				commonServices);
		
		protected override Func<ComplaintResultsOfCounterpartyJournalNode, ComplaintResultOfCounterpartyViewModel> OpenDialogFunction
			=> n =>
				new ComplaintResultOfCounterpartyViewModel(
					EntityUoWBuilder.ForOpen(n.Id),
					UnitOfWorkFactory,
					commonServices);
	}
}
