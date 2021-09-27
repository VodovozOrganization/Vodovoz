using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.ViewModels.Complaints
{
	public class GuiltyItemViewModel : EntityWidgetViewModelBase<ComplaintGuiltyItem>
	{
		public GuiltyItemViewModel(
			ComplaintGuiltyItem entity,
			ICommonServices commonServices,
			ISubdivisionRepository subdivisionRepository,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory,
			IUnitOfWork uow
		) : base(entity, commonServices)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			EmployeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			if(subdivisionRepository == null) {
				throw new ArgumentNullException(nameof(subdivisionRepository));
			}
			ConfigureEntityPropertyChanges();
			AllDepartments = subdivisionRepository.GetAllDepartmentsOrderedByName(UoW);
		}

		public event EventHandler OnGuiltyItemReady;

		private IList<Subdivision> allDepartments;
		public IList<Subdivision> AllDepartments {
			get => allDepartments;
			private set => SetField(ref allDepartments, value);
		}

		public bool CanChooseEmployee => Entity.GuiltyType == ComplaintGuiltyTypes.Employee;

		public bool CanChooseSubdivision => Entity.GuiltyType == ComplaintGuiltyTypes.Subdivision;

		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }

		void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(
				e => e.GuiltyType,
				() => CanChooseEmployee,
				() => CanChooseSubdivision
			);

			OnEntityPropertyChanged(
				() => OnGuiltyItemReady?.Invoke(this, EventArgs.Empty),
				e => e.GuiltyType,
				e => e.Employee,
				e => e.Subdivision
			);
		}
	}
}