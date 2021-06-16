using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarEventViewModel : EntityTabViewModelBase<CarEvent>
	{
		private DelegateCommand _changeDriverCommand;
		public CarEventViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
			IEntityAutocompleteSelectorFactory carSelectorFactory, IEntityAutocompleteSelectorFactory carEventTypeSelectorFactory)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			CarSelectorFactory = carSelectorFactory ?? throw new ArgumentNullException(nameof(carSelectorFactory));
			CarEventTypeSelectorFactory = carEventTypeSelectorFactory ?? throw new ArgumentNullException(nameof(carEventTypeSelectorFactory));

			TabName = "Событие ТС";

			if(Entity.Id == 0)
			{
				Entity.Author = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
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
