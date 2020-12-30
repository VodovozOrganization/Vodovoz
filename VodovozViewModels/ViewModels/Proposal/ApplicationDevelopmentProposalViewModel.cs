using System;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Proposal;
using Vodovoz.Infrastructure.Services;

namespace Vodovoz.ViewModels.ViewModels.Proposal
{
    public class ApplicationDevelopmentProposalViewModel : EntityTabViewModelBase<ApplicationDevelopmentProposal>
    {
        private readonly IEmployeeService employeeService;
        public bool IsProposalResponseVisible { get; }
        public bool IsProposalResponseSensitive { get; }

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
            
            IsProposalResponseVisible = !uowBuilder.IsNewEntity;
            IsProposalResponseSensitive = 
                !uowBuilder.IsNewEntity &&
                commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_app_development_proposal_response");

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
                () => true
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