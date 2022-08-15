using System;
using Gamma.Utilities;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Proposal;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.ViewModels.Proposal
{
    public class ApplicationDevelopmentProposalViewModel : EntityTabViewModelBase<ApplicationDevelopmentProposal>
    {
        private readonly IEmployeeService employeeService;
        public bool IsProposalResponseVisible { get; }
        public bool UserCanManageProposal { get; }
        public bool EditBtnPressed { get; set; }
        public bool ProposalResponseSensetive => EditBtnPressed && UserCanManageProposal;

        public ApplicationDevelopmentProposalStatus NextState
        {
            get
            {
                switch (Entity.Status)
                {
                    case ApplicationDevelopmentProposalStatus.Sent:
                        return ApplicationDevelopmentProposalStatus.Processing;
                    case ApplicationDevelopmentProposalStatus.Processing:
                        return ApplicationDevelopmentProposalStatus.CreatingTasks;
                    case ApplicationDevelopmentProposalStatus.CreatingTasks:
                        return ApplicationDevelopmentProposalStatus.TasksExecution;
                    case ApplicationDevelopmentProposalStatus.TasksExecution:
                        return ApplicationDevelopmentProposalStatus.TasksCompleted;
                }
                return ApplicationDevelopmentProposalStatus.New;
            }
        }

        public string NextStateName
        {
            get
            {
                if (Entity.Status != ApplicationDevelopmentProposalStatus.Rejected && Entity.Status != ApplicationDevelopmentProposalStatus.TasksCompleted)
                {
                    return "Перевести в статус: " + NextState.GetEnumTitle();
                }
                else
                {
                    return "Перевести в следующий статус";
                }
            }
        }

        public ApplicationDevelopmentProposalViewModel(
            IEmployeeService employeeService,
            IEntityUoWBuilder uowBuilder,
            IUnitOfWorkFactory uowFactory,
            ICommonServices commonServices) : base(uowBuilder, uowFactory, commonServices)
        {
            this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
            
            if (uowBuilder.IsNewEntity) {
                Entity.Author = employeeService.GetEmployeeForUser(UoW, CurrentUser.Id);
            }
            
            var canManageProposal = 
                commonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_app_development_proposal");
            IsProposalResponseVisible = !uowBuilder.IsNewEntity;
            UserCanManageProposal = !uowBuilder.IsNewEntity && canManageProposal;

            ConfigureEntityChangingRelations();
        }

        private DelegateCommand sendCommand;
        public DelegateCommand SendCommand => sendCommand ?? (sendCommand = new DelegateCommand(
                () =>
                {
                    Entity.ChangeStatus(ApplicationDevelopmentProposalStatus.Sent);
                    
                    if (Save(false)) {
                        CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Ваше предложение успешно отправлено.");
                        Close(false, CloseSource.Save);
                    }
                },
                () => Entity.Status == ApplicationDevelopmentProposalStatus.New
            )
        );
        
        private DelegateCommand editCommand;
        public DelegateCommand EditCommand => editCommand ?? (editCommand = new DelegateCommand(
                () =>
                {
                    EditBtnPressed = true;
                    Entity.ChangeStatus(ApplicationDevelopmentProposalStatus.New);
                },
                () => Entity.Status == ApplicationDevelopmentProposalStatus.Sent ||
                               Entity.Status == ApplicationDevelopmentProposalStatus.Rejected
            )
        );
        
        private DelegateCommand rejectCommand;
        public DelegateCommand RejectCommand => rejectCommand ?? (rejectCommand = new DelegateCommand(
                () =>
                {
                    var oldStatus = Entity.Status;
                    Entity.ChangeStatus(ApplicationDevelopmentProposalStatus.Rejected);

                    if (!Save(true)) {
                        Entity.ChangeStatus(oldStatus);
                    }
                },
                () => Entity.Status != ApplicationDevelopmentProposalStatus.Rejected &&
                               Entity.Status != ApplicationDevelopmentProposalStatus.TasksCompleted
            )
        );
        
        private DelegateCommand changeStatusCommand;
        public DelegateCommand ChangeStatusCommand => changeStatusCommand ?? (changeStatusCommand = new DelegateCommand(
                () =>
                {
                    switch (Entity.Status)
                    {
                        case ApplicationDevelopmentProposalStatus.Sent:
                            Entity.ChangeStatus(ApplicationDevelopmentProposalStatus.Processing);
                            break;
                        case ApplicationDevelopmentProposalStatus.Processing:
                            Entity.ChangeStatus(ApplicationDevelopmentProposalStatus.CreatingTasks);
                            break;
                        case ApplicationDevelopmentProposalStatus.CreatingTasks:
                            Entity.ChangeStatus(ApplicationDevelopmentProposalStatus.TasksExecution);
                            break;
                        case ApplicationDevelopmentProposalStatus.TasksExecution:
                            Entity.ChangeStatus(ApplicationDevelopmentProposalStatus.TasksCompleted);
                            break;
                    }
                    SaveAndClose();
                },
                () => Entity.Status != ApplicationDevelopmentProposalStatus.New &&
                               Entity.Status != ApplicationDevelopmentProposalStatus.Rejected &&
                               Entity.Status != ApplicationDevelopmentProposalStatus.TasksCompleted
            )
        );
        
        public bool IsViewElementSensitive => Entity.Status == ApplicationDevelopmentProposalStatus.New;
        public bool IsEditBtnSensitive => Entity.Status == ApplicationDevelopmentProposalStatus.Sent ||
                                          Entity.Status == ApplicationDevelopmentProposalStatus.Rejected;
        public bool IsBtnChangeStatusSensitive => Entity.Status != ApplicationDevelopmentProposalStatus.New &&
                                                  Entity.Status != ApplicationDevelopmentProposalStatus.Rejected &&
                                                  Entity.Status != ApplicationDevelopmentProposalStatus.TasksCompleted;
        public bool IsBtnRejectSensitive => Entity.Status != ApplicationDevelopmentProposalStatus.Rejected &&
                                            Entity.Status != ApplicationDevelopmentProposalStatus.TasksCompleted;
        private void ConfigureEntityChangingRelations()
        {
            SetPropertyChangeRelation(e => e.Status,
                () => IsViewElementSensitive,
                () => IsEditBtnSensitive,
                () => IsBtnChangeStatusSensitive,
                () => IsBtnRejectSensitive
            );
        }
    }
}
