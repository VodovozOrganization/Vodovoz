using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Sale;
using Vodovoz.Journals.JournalViewModels.Logistic;
using QS.Navigation;

namespace Vodovoz.ViewModels.ForAdministrators
{
	public class DeliverySchedulesCopierViewModel : TabViewModelBase, ISingleUoWDialog
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		readonly IUnitOfWorkFactory unitOfWorkFactory;
		readonly IInteractiveService interactiveService;
		readonly ICommonServices commonServices;

		public DeliverySchedulesCopierViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, IInteractiveService interactiveService, INavigationManager navigationManager) : base(interactiveService, navigationManager)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			TabName = "Копирование интервалов доставки";
		}

		public IUnitOfWork UoW { get; private set; }

		District sourceDistrict;
		public virtual District SourceDistrict {
			get => sourceDistrict;
			set {
				if(SetField(ref sourceDistrict, value)) {
					CanAddDistricts = CanCopy = value != null;
					RemoveCommand.Execute(value);
				}
			}
		}

		IList<District> districtsToEdit = new List<District>();
		public virtual IList<District> DistrictsToEdit {
			get => districtsToEdit;
			set => SetField(ref districtsToEdit, value);
		}

		GenericObservableList<District> observableDistrictsToEdit;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<District> ObservableDistrictsToEdit {
			get {
				if(observableDistrictsToEdit == null)
					observableDistrictsToEdit = new GenericObservableList<District>(DistrictsToEdit);
				return observableDistrictsToEdit;
			}
		}

		bool canSave = false;
		public virtual bool CanSave {
			get => canSave;
			set => SetField(ref canSave, value);
		}

		bool canCopy;
		public virtual bool CanCopy {
			get => canCopy;
			set => SetField(ref canCopy, value);
		}

		bool canAddDistricts;
		public virtual bool CanAddDistricts {
			get => canAddDistricts;
			set => SetField(ref canAddDistricts, value);
		}

		bool itemSelected;
		public virtual bool ItemSelected {
			get => itemSelected;
			set => SetField(ref itemSelected, value);
		}

		DelegateCommand saveCommand;
		public DelegateCommand SaveCommand {
			get {
				if(saveCommand == null) {
					saveCommand = new DelegateCommand(
						Save,
						() => CanSave
					);
				}
				return saveCommand;
			}
		}

		DelegateCommand cancelCommand;
		public DelegateCommand CancelCommand {
			get {
				if(cancelCommand == null) {
					cancelCommand = new DelegateCommand(
						() => Close(false, CloseSource.Cancel),
						() => true
					);
				}
				return cancelCommand;
			}
		}

		DelegateCommand copySchedulesCommand;
		public DelegateCommand CopySchedulesCommand {
			get {
				if(copySchedulesCommand == null) {
					copySchedulesCommand = new DelegateCommand(
						() => {
							// #region Monday
							// if(SourceDistrict.ScheduleRestrictionMonday != null) {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		if(distr.ScheduleRestrictionMonday != null)
							// 			distr.ScheduleRestrictionMonday.ObservableSchedules.Clear();
							// 		else
							// 			distr.CreateScheduleRestriction(WeekDayName.monday);
							//
							// 		foreach(DeliverySchedule schedule in SourceDistrict.ScheduleRestrictionMonday.ObservableSchedules) {
							// 			distr.ScheduleRestrictionMonday.ObservableSchedules.Add(schedule);
							// 		}
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// } else {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		distr.ScheduleRestrictionMonday = null;
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// }
							// #endregion Monday
							//
							// #region Tuesday
							// if(SourceDistrict.ScheduleRestrictionTuesday != null) {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		if(distr.ScheduleRestrictionTuesday != null)
							// 			distr.ScheduleRestrictionTuesday.ObservableSchedules.Clear();
							// 		else
							// 			distr.CreateScheduleRestriction(WeekDayName.tuesday);
							//
							// 		foreach(DeliverySchedule schedule in SourceDistrict.ScheduleRestrictionTuesday.ObservableSchedules) {
							// 			distr.ScheduleRestrictionTuesday.ObservableSchedules.Add(schedule);
							// 		}
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// } else {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		distr.ScheduleRestrictionTuesday = null;
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// }
							// #endregion Tuesday
							//
							// #region Wednesday
							// if(SourceDistrict.ScheduleRestrictionWednesday != null) {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		if(distr.ScheduleRestrictionWednesday != null)
							// 			distr.ScheduleRestrictionWednesday.ObservableSchedules.Clear();
							// 		else
							// 			distr.CreateScheduleRestriction(WeekDayName.wednesday);
							//
							// 		foreach(DeliverySchedule schedule in SourceDistrict.ScheduleRestrictionWednesday.ObservableSchedules) {
							// 			distr.ScheduleRestrictionWednesday.ObservableSchedules.Add(schedule);
							// 		}
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// } else {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		distr.ScheduleRestrictionWednesday = null;
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// }
							// #endregion Wednesday
							//
							// #region Thursday
							// if(SourceDistrict.ScheduleRestrictionThursday != null) {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		if(distr.ScheduleRestrictionThursday != null)
							// 			distr.ScheduleRestrictionThursday.ObservableSchedules.Clear();
							// 		else
							// 			distr.CreateScheduleRestriction(WeekDayName.thursday);
							//
							// 		foreach(DeliverySchedule schedule in SourceDistrict.ScheduleRestrictionThursday.ObservableSchedules) {
							// 			distr.ScheduleRestrictionThursday.ObservableSchedules.Add(schedule);
							// 		}
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// } else {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		distr.ScheduleRestrictionThursday = null;
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// }
							// #endregion Thursday
							//
							// #region Friday
							// if(SourceDistrict.ScheduleRestrictionFriday != null) {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		if(distr.ScheduleRestrictionFriday != null)
							// 			distr.ScheduleRestrictionFriday.ObservableSchedules.Clear();
							// 		else
							// 			distr.CreateScheduleRestriction(WeekDayName.friday);
							//
							// 		foreach(DeliverySchedule schedule in SourceDistrict.ScheduleRestrictionFriday.ObservableSchedules) {
							// 			distr.ScheduleRestrictionFriday.ObservableSchedules.Add(schedule);
							// 		}
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// } else {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		distr.ScheduleRestrictionFriday = null;
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// }
							// #endregion Friday
							//
							// #region Saturday
							// if(SourceDistrict.ScheduleRestrictionSaturday != null) {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		if(distr.ScheduleRestrictionSaturday != null)
							// 			distr.ScheduleRestrictionSaturday.ObservableSchedules.Clear();
							// 		else
							// 			distr.CreateScheduleRestriction(WeekDayName.saturday);
							//
							// 		foreach(DeliverySchedule schedule in SourceDistrict.ScheduleRestrictionSaturday.ObservableSchedules) {
							// 			distr.ScheduleRestrictionSaturday.ObservableSchedules.Add(schedule);
							// 		}
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// } else {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		distr.ScheduleRestrictionSaturday = null;
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// }
							// #endregion Saturday
							//
							// #region Sunday
							// if(SourceDistrict.ScheduleRestrictionSunday != null) {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		if(distr.ScheduleRestrictionSunday != null)
							// 			distr.ScheduleRestrictionSunday.ObservableSchedules.Clear();
							// 		else
							// 			distr.CreateScheduleRestriction(WeekDayName.sunday);
							//
							// 		foreach(DeliverySchedule schedule in SourceDistrict.ScheduleRestrictionSunday.ObservableSchedules) {
							// 			distr.ScheduleRestrictionSunday.ObservableSchedules.Add(schedule);
							// 		}
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// } else {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		distr.ScheduleRestrictionSunday = null;
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// }
							// #endregion Sunday
							//
							// #region Today
							// if(SourceDistrict.ScheduleRestrictionToday != null) {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		if(distr.ScheduleRestrictionToday != null)
							// 			distr.ScheduleRestrictionToday.ObservableSchedules.Clear();
							// 		else
							// 			distr.CreateScheduleRestriction(WeekDayName.today);
							//
							// 		foreach(DeliverySchedule schedule in SourceDistrict.ScheduleRestrictionToday.ObservableSchedules) {
							// 			distr.ScheduleRestrictionToday.ObservableSchedules.Add(schedule);
							// 		}
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// } else {
							// 	foreach(District distr in ObservableDistrictsToEdit) {
							// 		distr.ScheduleRestrictionToday = null;
							// 		distr.Save(UoW);
							// 		CanSave = true;
							// 	}
							// }
							// #endregion Today

							SourceDistrict = null;
							ObservableDistrictsToEdit.Clear();

							if(CanSave && AskQuestion("Интервалы скопированы успешно. Сохранить и закрыть вкладку? В случае отказа сохранить изменения можно будет кликнув кнопку \"Сохранить\""))
								SaveCommand.Execute();
						},
						() => SourceDistrict != null && ObservableDistrictsToEdit.Any()
					);
				}
				return copySchedulesCommand;
			}
		}

		DelegateCommand addDistrictCommand;
		public DelegateCommand AddDistrictCommand {
			get {
				if(addDistrictCommand == null) {
					addDistrictCommand = new DelegateCommand(
						() => {
							var districtsJournalViewModel = new DistrictsJournalViewModel(
								unitOfWorkFactory,
								commonServices
							) {
								SelectionMode = JournalSelectionMode.Multiple
							};
							districtsJournalViewModel.SetRestriction(() => Restrictions.Not(Restrictions.IdEq(SourceDistrict.Id)));
							districtsJournalViewModel.HideCreateAndOpenBtns();
							districtsJournalViewModel.OnEntitySelectedResult += (sender, e) => {
								e.SelectedNodes.OrderBy(n => n.Title).ToList().ForEach(n => ObservableDistrictsToEdit.Add(UoW.GetById<District>(n.Id)));
							};
							TabParent.AddSlaveTab(this, districtsJournalViewModel);
						},
						() => CanAddDistricts
					);
				}
				return addDistrictCommand;
			}
		}

		DelegateCommand<District> removeCommand;
		public DelegateCommand<District> RemoveCommand {
			get {
				if(removeCommand == null) {
					removeCommand = new DelegateCommand<District>(
						d => ObservableDistrictsToEdit.Remove(d),
						d => d != null && ObservableDistrictsToEdit.Any(x => x.Id == d.Id)
					);
				}
				return removeCommand;
			}
		}

		public IList<District> GetAllDistricts()
		{
			var lst = UoW.Session.QueryOver<District>()
								 .List<District>()
								 .OrderBy(d => d.DistrictName)
								 .ToList()
								 ;

			return lst;
		}

		public void Save()
		{
			try {
				UoW.Commit();
			} catch(Exception e) {
				throw e;
			} finally {
				Close(false, CloseSource.Save);
			}
		}

		public override void Dispose()
		{
			UoW?.Dispose();
			base.Dispose();
		}
	}
}
