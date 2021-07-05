using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Proposal;
using Vodovoz.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Proposal;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Proposal;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Proposal
{
    public class ApplicationDevelopmentProposalsJournalViewModel : 
        FilterableSingleEntityJournalViewModelBase<ApplicationDevelopmentProposal, 
                                                   ApplicationDevelopmentProposalViewModel, 
                                                   ApplicationDevelopmentProposalsJournalNode,
                                                   ApplicationDevelopmentProposalsJournalFilterViewModel>
    {
	    private readonly IEmployeeService employeeService;
	    private readonly bool canChangeProposalsStatus;

	    public ApplicationDevelopmentProposalsJournalViewModel(
		    EntitiesJournalActionsViewModel journalActionsViewModel,
		    ApplicationDevelopmentProposalsJournalFilterViewModel filterViewModel,
		    IEmployeeService employeeService,
		    IUnitOfWorkFactory uowFactory,
		    ICommonServices commonServices) : base(journalActionsViewModel, filterViewModel, uowFactory, commonServices)
	    {
		    this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
		    canChangeProposalsStatus =
			    commonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_app_development_proposal");

		    TabName = "Журнал предложений по разработке приложения ВВ";
		    
		    UpdateOnChanges(
			    typeof(ApplicationDevelopmentProposal),
			    typeof(Employee)
		    );
	    }
	    
        protected override Func<IUnitOfWork, IQueryOver<ApplicationDevelopmentProposal>> ItemsSourceQueryFunction => uow => {
	        ApplicationDevelopmentProposal proposalAlias = null;
	        ApplicationDevelopmentProposalsJournalNode resultAlias = null;

	        var query = uow.Session.QueryOver(() => proposalAlias);
					
			if(FilterViewModel?.Status != null)
				query.Where(() => proposalAlias.Status == FilterViewModel.Status);
			
			query.Where(GetSearchCriterion(
				() => proposalAlias.Id,
				() => proposalAlias.Title,
				() => proposalAlias.Status
			));

			var resultQuery = query
				.SelectList(list => list
				   .Select(() => proposalAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => proposalAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
				   .Select(() => proposalAlias.Title).WithAlias(() => resultAlias.Title)
				   .Select(() => proposalAlias.Status).WithAlias(() => resultAlias.Status)
				)
				.TransformUsing(Transformers.AliasToBean<ApplicationDevelopmentProposalsJournalNode>());

			return resultQuery;
		};

		protected override Func<ApplicationDevelopmentProposalViewModel> CreateDialogFunction => 
			() => new ApplicationDevelopmentProposalViewModel(
				employeeService,
				EntityUoWBuilder.ForCreate(),
				UnitOfWorkFactory,
				CommonServices);

		protected override Func<JournalEntityNodeBase, ApplicationDevelopmentProposalViewModel> OpenDialogFunction => 
			node => new ApplicationDevelopmentProposalViewModel(
				employeeService,
				EntityUoWBuilder.ForOpen(node.Id),
				UnitOfWorkFactory,
				CommonServices);

		protected override void InitializeJournalActionsViewModel()
		{
			EntitiesJournalActionsViewModel.Initialize(
				SelectionMode, EntityConfigs, this, HideJournal, OnItemsSelected, false);
		}
		
		protected override void CreatePopupActions()
		{
			PopupActionsList.Add(
				new JournalAction(
					"Перевести в обрабатывается",
					selectedItems => 
						selectedItems.All(x => 
							(x as ApplicationDevelopmentProposalsJournalNode).Status == ApplicationDevelopmentProposalStatus.Sent),
					selectedItems => canChangeProposalsStatus,
					selectedItems => {
						foreach (var item in selectedItems)
						{
							var proposal = UoW.GetById<ApplicationDevelopmentProposal>((item as ApplicationDevelopmentProposalsJournalNode).Id);

							proposal?.ChangeStatus(ApplicationDevelopmentProposalStatus.Processing);
							UoW.Save(proposal);
						}

						UoW.Commit();
					}
				)
			);
			
			PopupActionsList.Add(
				new JournalAction(
					"Перевести в формирование задач",
					selectedItems => 
						selectedItems.All(x => 
							(x as ApplicationDevelopmentProposalsJournalNode).Status == ApplicationDevelopmentProposalStatus.Processing),
					selectedItems => canChangeProposalsStatus,
					selectedItems => {
						foreach (var item in selectedItems)
						{
							var proposal = UoW.GetById<ApplicationDevelopmentProposal>((item as ApplicationDevelopmentProposalsJournalNode).Id);

							proposal?.ChangeStatus(ApplicationDevelopmentProposalStatus.CreatingTasks);
							UoW.Save(proposal);
						}

						UoW.Commit();
					}
				)
			);
			
			PopupActionsList.Add(
				new JournalAction(
					"Перевести в выполнение задач",
					selectedItems => 
						selectedItems.All(x => 
							(x as ApplicationDevelopmentProposalsJournalNode).Status == ApplicationDevelopmentProposalStatus.CreatingTasks),
					selectedItems => canChangeProposalsStatus,
					selectedItems => {
						foreach (var item in selectedItems)
						{
							var proposal = UoW.GetById<ApplicationDevelopmentProposal>((item as ApplicationDevelopmentProposalsJournalNode).Id);

							proposal?.ChangeStatus(ApplicationDevelopmentProposalStatus.TasksExecution);
							UoW.Save(proposal);
						}

						UoW.Commit();
					}
				)
			);
			
			PopupActionsList.Add(
				new JournalAction(
					"Перевести в задачи выполнены",
					selectedItems => 
						selectedItems.All(x => 
							(x as ApplicationDevelopmentProposalsJournalNode).Status == ApplicationDevelopmentProposalStatus.TasksExecution),
					selectedItems => canChangeProposalsStatus,
					selectedItems => {
						foreach (var item in selectedItems)
						{
							var proposal = UoW.GetById<ApplicationDevelopmentProposal>((item as ApplicationDevelopmentProposalsJournalNode).Id);

							proposal?.ChangeStatus(ApplicationDevelopmentProposalStatus.TasksCompleted);
							UoW.Save(proposal);
						}

						UoW.Commit();
					}
				)
			);
		}
    }
}