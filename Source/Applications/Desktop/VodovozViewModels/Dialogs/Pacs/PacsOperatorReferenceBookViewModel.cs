using Autofac;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.ViewModels.Employees;
using Vodovoz.Presentation.ViewModels.Pacs.Journals;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsOperatorReferenceBookViewModel : EntityReactiveDialog<Operator>
	{
		private readonly IEmployeeService _employeeService;
		private readonly IValidator _validator;
		private readonly ILifetimeScope _scope;
		private Employee _operator;

		public PacsOperatorReferenceBookViewModel(
			IEntityIdentifier entityId,
			EntityModelFactory entityModelFactory,
			IEmployeeService employeeService,
			IValidator validator,
			ILifetimeScope scope,
			INavigationManager navigator
		) : base(entityId, entityModelFactory, navigator)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			Title = "Оператор";

			this.WhenAnyValue(x => x.Entity.Id)
				.Subscribe(x =>
				{
					if(x == 0)
					{
						Operator = null;
					}
					else
					{
						Operator = _employeeService.GetEmployee(Model.UoW, x);
					}
				})
				.DisposeWith(Subscriptions);
			this.WhenAnyValue(x => x.Operator)
				.Where(x => x != null)
				.Subscribe(x => Entity.Id = Operator.Id)
				.DisposeWith(Subscriptions);

			OperatorEntry = new CommonEEVMBuilderFactory<PacsOperatorReferenceBookViewModel>(this, this, Model.UoW, navigator, _scope)
				.ForProperty(x => x.Operator)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(
					filter =>
					{
						filter.RestrictCategory = EmployeeCategory.office;
						filter.Status = EmployeeStatus.IsWorking;
					}
				).Finish();

			WorkShiftEntry = new CommonEEVMBuilderFactory<Operator>(this, Entity, Model.UoW, navigator, _scope)
				.ForProperty(x => x.WorkShift)
				.UseViewModelDialog<WorkShiftViewModel>()
				.UseViewModelJournalAndAutocompleter<WorkShiftJournalViewModel>()
				.Finish();
		}

		public bool CanChangeOperator => Model.IsNewEntity;

		public IEntityEntryViewModel OperatorEntry { get; }
		public IEntityEntryViewModel WorkShiftEntry { get; }

		public Employee Operator
		{
			get => _operator;
			set => this.RaiseAndSetIfChanged(ref _operator, value);
		}

		public override void Save()
		{
			if(!_validator.Validate(Entity))
			{
				return;
			}
			base.Save();
		}
	}
}
