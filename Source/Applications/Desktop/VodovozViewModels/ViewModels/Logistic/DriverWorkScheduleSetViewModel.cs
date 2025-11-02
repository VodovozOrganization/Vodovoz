using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Settings.Delivery;

namespace Vodovoz.ViewModels.Logistic
{
	public sealed class DriverWorkScheduleSetViewModel : TabViewModelBase
    {
        public DriverWorkScheduleSetViewModel(
            DriverWorkScheduleSet entity,
            IUnitOfWork uow,
            ICommonServices commonServices,
            IDeliveryScheduleSettings deliveryScheduleSettings,
            IEmployeeRepository employeeRepository,
            INavigationManager navigation = null) 
            : base(commonServices.InteractiveService, navigation)
        {
			if(deliveryScheduleSettings is null)
			{
				throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			}

			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
            this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));

            Entity = entity ?? new DriverWorkScheduleSet();

            permissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DriverWorkScheduleSet));

            DeliveryDaySchedules = new List<DeliveryDaySchedule>(uow.GetAll<DeliveryDaySchedule>());

            FillObservableDriverWorkSchedules(deliveryScheduleSettings);
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
        private readonly IEmployeeRepository employeeRepository;
        private readonly IPermissionResult permissionResult;

        public EventHandler EntityAccepted;

        public bool IsInfoVisible => Entity.Id != 0;
        public bool CanEdit => (Entity.Id == 0 && permissionResult.CanCreate) || (Entity.Id != 0 && permissionResult.CanUpdate);
        
        public string Id => Entity.Id.ToString();
        public string Author => Entity.Author == null ? " - " : Entity.Author.ShortName;
        public string DateActivated => Entity.DateActivated.ToString("g");
        public string DateDeactivated => Entity.DateDeactivated == null ? " - " : Entity.DateDeactivated.Value.ToString("g");

        private DriverWorkScheduleSet entity;
        public DriverWorkScheduleSet Entity {
            get => entity;
            set => SetField(ref entity, value);
        }

        public IList<DeliveryDaySchedule> DeliveryDaySchedules { get; }
        public GenericObservableList<DriverWorkScheduleNode> ObservableDriverWorkSchedules { get; set; }

        #endregion

        #region Команды

        private DelegateCommand acceptCommand;
        public DelegateCommand AcceptCommand => acceptCommand ?? (acceptCommand = new DelegateCommand(
            () => {
                var employeeForCurrentUser = employeeRepository.GetEmployeeForCurrentUser(uow);
                if(Entity.Author == null) {
                    Entity.Author = employeeForCurrentUser;
                }
                Entity.LastEditor = employeeForCurrentUser;
            
                SynchronizeDriverWorkSchedules();
                Close(true, CloseSource.Self);
                EntityAccepted?.Invoke(this, EventArgs.Empty);
            },
            () => CanEdit
        ));

        #endregion

        #region Приватные методы

        private void FillObservableDriverWorkSchedules(IDeliveryScheduleSettings deliveryScheduleSettings)
        {
            var defaultDaySchedule = uow.GetById<DeliveryDaySchedule>(deliveryScheduleSettings.DefaultDeliveryDayScheduleId);

            ObservableDriverWorkSchedules = new GenericObservableList<DriverWorkScheduleNode> {
                new DriverWorkScheduleNode { WeekDay = WeekDayName.Monday, DaySchedule = defaultDaySchedule },
                new DriverWorkScheduleNode { WeekDay = WeekDayName.Tuesday, DaySchedule = defaultDaySchedule },
                new DriverWorkScheduleNode { WeekDay = WeekDayName.Wednesday, DaySchedule = defaultDaySchedule },
                new DriverWorkScheduleNode { WeekDay = WeekDayName.Thursday, DaySchedule = defaultDaySchedule },
                new DriverWorkScheduleNode { WeekDay = WeekDayName.Friday, DaySchedule = defaultDaySchedule },
                new DriverWorkScheduleNode { WeekDay = WeekDayName.Saturday, DaySchedule = defaultDaySchedule },
                new DriverWorkScheduleNode { WeekDay = WeekDayName.Sunday, DaySchedule = defaultDaySchedule }
            };

            foreach(DriverWorkScheduleNode scheduleNode in ObservableDriverWorkSchedules) {
                var existingDriverWorkSchedule =
                    Entity.DriverWorkSchedules.SingleOrDefault(x => x.WeekDay == scheduleNode.WeekDay);

                if(existingDriverWorkSchedule != null) {
                    scheduleNode.AtWork = true;
                    scheduleNode.DaySchedule = existingDriverWorkSchedule.DaySchedule;
                    scheduleNode.DriverWorkSchedule = existingDriverWorkSchedule;
                }
            }
        }
        
        private void UpdateTabName()
        {
            if(Entity.Id == 0) {
                TabName = "Новая версия графиков работы водителя" + (Entity.Driver != null ? $" [{Entity.Driver.ShortName}]" : "");
            }
            else {
                TabName = "Версия графиков работы водителя" + (Entity.Driver != null ? $" [{Entity.Driver.ShortName}]" : "");
            }
        }

        private void SynchronizeDriverWorkSchedules()
        {
            //Синхронизируем список из ObservableDriverWorkSchedules со списком из базы
            foreach(DriverWorkScheduleNode scheduleNode in ObservableDriverWorkSchedules) {
                if(scheduleNode.AtWork && scheduleNode.DriverWorkSchedule == null) {
                    var newWorkDay = new DriverWorkSchedule {
                        DaySchedule = scheduleNode.DaySchedule,
                        WeekDay = scheduleNode.WeekDay,
                        DriverWorkScheduleSet = Entity
                    };
                    scheduleNode.DriverWorkSchedule = newWorkDay;
                    Entity.ObservableDriverWorkSchedules.Add(newWorkDay);
                }
                else if(scheduleNode.AtWork && scheduleNode.DriverWorkSchedule != null) {
                    var driverWorkSchedule = Entity.ObservableDriverWorkSchedules.Single(x => x.WeekDay == scheduleNode.WeekDay);
                    driverWorkSchedule.DaySchedule = scheduleNode.DaySchedule;
                }
                else if(!scheduleNode.AtWork && scheduleNode.DriverWorkSchedule != null) {
                    var driverWorkSchedule = Entity.ObservableDriverWorkSchedules.Single(x => x.WeekDay == scheduleNode.WeekDay);
                    scheduleNode.DriverWorkSchedule = null;
                    Entity.ObservableDriverWorkSchedules.Remove(driverWorkSchedule);
                }
            }
        }

        #endregion
    }
}
