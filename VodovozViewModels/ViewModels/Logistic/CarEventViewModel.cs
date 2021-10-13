using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarEventViewModel : EntityTabViewModelBase<CarEvent>
	{
		private DelegateCommand _changeDriverCommand;
		public CarEventViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICarJournalFactory carJournalFactory,
			ICarEventTypeJournalFactory carEventTypeJournalFactory,
			IEmployeeService employeeService)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			if(employeeService == null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}
			
			CarSelectorFactory = carJournalFactory.CreateCarAutocompleteSelectorFactory();
			CarEventTypeSelectorFactory = carEventTypeJournalFactory.CreateCarEventTypeAutocompleteSelectorFactory();

			TabName = "Событие ТС";

			if(Entity.Id == 0)
			{
				Entity.Author = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				Entity.CreateDate = DateTime.Now;
			}
		}

		public IEntityAutocompleteSelectorFactory CarSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory CarEventTypeSelectorFactory { get; }

		public DelegateCommand ChangeDriverCommand => _changeDriverCommand ?? (_changeDriverCommand =
			new DelegateCommand(() =>
				{
					if(Entity.Car != null)
					{
						Entity.Driver = (Entity.Car.Driver != null && Entity.Car.Driver.Status != EmployeeStatus.IsFired)
							? Entity.Car.Driver
							: null;
					}
				},
				() => true
			));
	}
}
