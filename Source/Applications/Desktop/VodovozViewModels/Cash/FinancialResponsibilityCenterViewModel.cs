using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Extensions;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Store.Reports;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewModels.Cash
{
	public class FinancialResponsibilityCenterViewModel : EntityTabViewModelBase<FinancialResponsibilityCenter>
	{
		private Employee _responsibleEmployee;
		private Employee _viceResponsibleEmployee;
		private object _selectedSubdivisionNodeObject;

		public FinancialResponsibilityCenterViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IGenericRepository<Subdivision> subdivisionRepository,
			ViewModelEEVMBuilder<Employee> responsibleEmployeeViewModelBuilder,
			ViewModelEEVMBuilder<Employee> viceResponsibleEmployeeViewModelBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(subdivisionRepository is null)
			{
				throw new ArgumentNullException(nameof(subdivisionRepository));
			}

			if(responsibleEmployeeViewModelBuilder is null)
			{
				throw new ArgumentNullException(nameof(responsibleEmployeeViewModelBuilder));
			}

			if(viceResponsibleEmployeeViewModelBuilder is null)
			{
				throw new ArgumentNullException(nameof(viceResponsibleEmployeeViewModelBuilder));
			}
			
			ResponsibleEmployeeViewModel = responsibleEmployeeViewModelBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.ResponsibleEmployee)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			ViceResponsibleEmployeeViewModel = viceResponsibleEmployeeViewModelBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.ViceResponsibleEmployee)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			if(Entity.Id != 0)
			{
				ResponsibleOfSubdivisions = subdivisionRepository.GetValue(
					UoW,
					subdivision => new EntityIdToNameNode { Id = subdivision.Id, Name = subdivision.Name },
					subdivision => subdivision.FinancialResponsibilityCenterId == Entity.Id);
			}

			SaveCommand = new DelegateCommand(SaveAndClose);
			CancelCommand = new DelegateCommand(() => Close(HasChanges, CloseSource.Cancel));

			OpenSubdivisionCommand = new DelegateCommand(OpenSubdivision, ()=> CanOpenSubdivision);
			OpenSubdivisionCommand.CanExecuteChangedWith(this, x => x.CanOpenSubdivision);
		}

		[PropertyChangedAlso(nameof(SelectedSubdivisionNode))]
		[PropertyChangedAlso(nameof(CanOpenSubdivision))]
		public object SelectedSubdivisionNodeObject
		{
			get => _selectedSubdivisionNodeObject;
			set => SetField(ref _selectedSubdivisionNodeObject, value);
		}

		public Employee ResponsibleEmployee
		{
			get => this.GetIdRefField(ref _responsibleEmployee, Entity.ResponsibleEmployeeId);
			set => this.SetIdRefField(SetField, ref _responsibleEmployee, () => Entity.ResponsibleEmployeeId, value);
		}

		public Employee ViceResponsibleEmployee
		{
			get => this.GetIdRefField(ref _viceResponsibleEmployee, Entity.ViceResponsibleEmployeeId);
			set => this.SetIdRefField(SetField, ref _viceResponsibleEmployee, () => Entity.ViceResponsibleEmployeeId, value);
		}

		public IEntityEntryViewModel ResponsibleEmployeeViewModel { get; }
		public IEntityEntryViewModel ViceResponsibleEmployeeViewModel { get; }
		public IEnumerable<EntityIdToNameNode> ResponsibleOfSubdivisions { get; }
		public DelegateCommand SaveCommand { get; set; }
		public DelegateCommand CancelCommand { get; set; }
		public DelegateCommand OpenSubdivisionCommand { get; }

		public EntityIdToNameNode SelectedSubdivisionNode => SelectedSubdivisionNodeObject as EntityIdToNameNode;

		public bool CanOpenSubdivision => SelectedSubdivisionNode != null;

		private void OpenSubdivision()
		{
			if(SelectedSubdivisionNode is null)
			{
				return;
			}

			NavigationManager.OpenViewModel<SubdivisionViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(SelectedSubdivisionNode.Id));
		}
	}
}
