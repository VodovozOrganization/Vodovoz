using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Services;
using Vodovoz.Settings.Delivery;

namespace Vodovoz.ViewModels.Logistic
{
    public sealed class DriverDistrictPrioritySetViewModel : TabViewModelBase
    {
        public DriverDistrictPrioritySetViewModel(
            DriverDistrictPrioritySet entity,
            IUnitOfWork uow,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
            IEmployeeRepository employeeRepository,
            INavigationManager navigation = null) 
            : base(commonServices.InteractiveService, navigation)
        {
            this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            this.commonServices = commonServices;

            Entity = entity ?? new DriverDistrictPrioritySet();
            permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DriverDistrictPrioritySet));
            OnPropertyChanged(nameof(CanEdit));
            
            FillObservableDriverWorkSchedules();
            UpdateTabName();
            
            Entity.PropertyChanged += (sender, args) => {
                switch(args.PropertyName) {
                    case nameof(Entity.Driver):
                        UpdateTabName();
                        break;
                    case nameof(Entity.Id):
                        OnPropertyChanged(nameof(Id));
                        OnPropertyChanged(nameof(IsInfoVisible));
                        break;
                    case nameof(Entity.Author):
                        OnPropertyChanged(nameof(Author));
                        break;
                    case nameof(Entity.DateActivated):
                        OnPropertyChanged(nameof(DateActivated));
                        break;
                    case nameof(Entity.DateDeactivated):
                        OnPropertyChanged(nameof(DateDeactivated));
                        break;
                }
            };
        }

        #region Поля и свойства

        private readonly IUnitOfWork uow;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly ICommonServices commonServices;
        private readonly IEmployeeRepository employeeRepository;
        private readonly IPermissionResult permissionResult;

		public EventHandler<DriverDistrictPrioritySetAcceptedEventArgs> EntityAccepted;

		public bool IsInfoVisible => Entity.Id != 0;
        public bool CanEdit => (Entity.Id == 0 && permissionResult.CanCreate) || (Entity.Id != 0 && permissionResult.CanUpdate);

        public string Id => Entity.Id.ToString();
        public string Author => Entity.Author == null ? " - " : Entity.Author.ShortName;
        public string DateActivated => Entity.DateActivated?.ToString("g") ?? "";
        public string DateDeactivated => Entity.DateDeactivated == null ? " - " : Entity.DateDeactivated.Value.ToString("g");

        private DriverDistrictPrioritySet entity;
        public DriverDistrictPrioritySet Entity {
            get => entity;
            private set => SetField(ref entity, value);
        }
        
        public GenericObservableList<DriverDistrictPriorityNode> ObservableDriverDistrictPriorities { get; set; }

        #endregion

        #region Команды
        
        private DelegateCommand acceptCommand;
        public DelegateCommand AcceptCommand => acceptCommand ?? (acceptCommand = new DelegateCommand(
            () => {
                if(ObservableDriverDistrictPriorities.Any()
                    && ObservableDriverDistrictPriorities.Any(x =>
                        x.District.DistrictsSet.Id !=
                        ObservableDriverDistrictPriorities.First().District.DistrictsSet.Id)
                ) {
                    commonServices.InteractiveService.ShowMessage(
                        ImportanceLevel.Error, "Все районы должны быть из одной версии");
                    return;
                }
                
                var employeeForCurrentUser = employeeRepository.GetEmployeeForCurrentUser(uow);
                if(Entity.Author == null) {
                    Entity.Author = employeeForCurrentUser;
                }
                Entity.LastEditor = employeeForCurrentUser;
            
                SynchronizeDriverDistrictPriorities();
                Close(true, CloseSource.Self);
                EntityAccepted?.Invoke(this, new DriverDistrictPrioritySetAcceptedEventArgs(Entity));
            },
            () => CanEdit
        ));

        private DelegateCommand addDistrictCommand;
        public DelegateCommand AddDistrictsCommand => addDistrictCommand ?? (addDistrictCommand = new DelegateCommand(
            () => {
                var filter = new DistrictJournalFilterViewModel { Status = DistrictsSetStatus.Active, OnlyWithBorders = true };
                var journalViewModel = new DistrictJournalViewModel(filter, unitOfWorkFactory, commonServices) {
                    EnableDeleteButton = false, EnableEditButton = false, EnableAddButton = false,
                    SelectionMode = JournalSelectionMode.Multiple
                };
                journalViewModel.OnEntitySelectedResult += (o, args) => {
                    if(args.SelectedNodes == null || !args.SelectedNodes.Any()) {
                        return;
                    }
                    var districtNodesToAdd = args.SelectedNodes
                        .Where(x => ObservableDriverDistrictPriorities.All(n => n.District.Id != x.Id));
                    var districtsToAdd = uow.GetById<District>(districtNodesToAdd.Select(x => x.Id));

                    foreach(var district in districtsToAdd) {
                        ObservableDriverDistrictPriorities.Add(
                            new DriverDistrictPriorityNode {
                                District = district
                            });
                    }
                    CheckAndFixDistrictsPrioritiesCommand.Execute();
                };
                TabParent.AddSlaveTab(this, journalViewModel);
            },
            () => CanEdit
        ));

        private DelegateCommand<IList<DriverDistrictPriorityNode>> deleteDistrictCommand;
        public DelegateCommand<IList<DriverDistrictPriorityNode>> DeleteDistrictsCommand => deleteDistrictCommand
            ?? (deleteDistrictCommand = new DelegateCommand<IList<DriverDistrictPriorityNode>>(
                priorities => {
                    if(!priorities.Any()) {
                        commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Не выбран ни 1 приоритет района водителя", "Удаление");
                        return;
                    }
                    foreach(var priority in priorities) {
                        ObservableDriverDistrictPriorities.Remove(priority);
                    }
                },
                priorities => CanEdit
            ));
        
        private DelegateCommand checkAndFixDistrictsPrioritiesCommand;
        public DelegateCommand CheckAndFixDistrictsPrioritiesCommand => checkAndFixDistrictsPrioritiesCommand ?? (checkAndFixDistrictsPrioritiesCommand = new DelegateCommand(
            () => {
                for(int i = 0; i < ObservableDriverDistrictPriorities.Count; i++) {
                    if(ObservableDriverDistrictPriorities[i].Priority != i) {
                        ObservableDriverDistrictPriorities[i].Priority = i;
                    }
                }
            },
            () => CanEdit
        ));

        #endregion

        #region Приватные методы

        private void FillObservableDriverWorkSchedules()
        {
            ObservableDriverDistrictPriorities = new GenericObservableList<DriverDistrictPriorityNode>();
            
            foreach(DriverDistrictPriority driverDistrictPriority in Entity.ObservableDriverDistrictPriorities) {
                ObservableDriverDistrictPriorities.Add(new DriverDistrictPriorityNode {
                    District = driverDistrictPriority.District,
                    Priority = driverDistrictPriority.Priority
                });
            }
        }

        private void SynchronizeDriverDistrictPriorities()
        {
            //Удаляем удалённые из сущности
            for(int i = 0; i < Entity.ObservableDriverDistrictPriorities.Count; i++) {
                if(ObservableDriverDistrictPriorities.All(x =>
                    x.District.Id != Entity.ObservableDriverDistrictPriorities[i].District.Id))
                {
                    Entity.ObservableDriverDistrictPriorities.RemoveAt(i);
                    i--;
                }
            }
            //Добавляем новые в сущность
            foreach(DriverDistrictPriorityNode priorityNode in ObservableDriverDistrictPriorities) {
                
                var existingPriority = Entity.ObservableDriverDistrictPriorities
                    .SingleOrDefault(x => x.District.Id == priorityNode.District.Id);
                
                if(existingPriority != null) {
                    existingPriority.Priority = priorityNode.Priority;
                }
                else {
                    Entity.ObservableDriverDistrictPriorities.Add(new DriverDistrictPriority {
                        District = priorityNode.District,
                        Priority = priorityNode.Priority,
                        DriverDistrictPrioritySet = Entity
                    });
                }
            }
            Entity.CheckAndFixDistrictsPriorities();
        }

        private void UpdateTabName()
        {
            if(Entity.Id == 0) {
                TabName = "Новая версия приоритетов районов водителя" + (Entity.Driver != null ? $" [{Entity.Driver.ShortName}]" : "");
            }
            else {
                TabName = "Версия приоритетов районов водителя" + (Entity.Driver != null ? $" [{Entity.Driver.ShortName}]" : "");
            }
        }

        #endregion
    }

	public class DriverDistrictPrioritySetAcceptedEventArgs : EventArgs
	{
		public DriverDistrictPrioritySetAcceptedEventArgs(DriverDistrictPrioritySet driverDistrictPrioritySet)
		{
			AcceptedEntity = driverDistrictPrioritySet;
		}

		public DriverDistrictPrioritySet AcceptedEntity { get; private set; }
	}
}

