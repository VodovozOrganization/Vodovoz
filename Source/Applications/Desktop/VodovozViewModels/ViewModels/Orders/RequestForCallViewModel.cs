using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using Vodovoz.Factories;
using Vodovoz.Services;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class RequestForCallViewModel : EntityDialogViewModelBase<RequestForCall>, IAskSaveOnCloseViewModel
	{
		private readonly IInteractiveService _interactiveService;
		private readonly Employee _currentEmployee;
		private readonly ValidationContext _validationContext;
		private readonly IPermissionResult _permissionResult;
		private ILifetimeScope _lifetimeScope;

		public RequestForCallViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			ICurrentPermissionService currentPermissionService,
			IEmployeeService employeeService,
			INavigationManager navigation,
			IValidator validator,
			IValidationContextFactory validationContextFactory,
			ILifetimeScope lifetimeScope) : base(entityUoWBuilder, unitOfWorkFactory, navigation, validator)
		{
			if(currentPermissionService == null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}

			_currentEmployee =
				(employeeService ?? throw new ArgumentNullException(nameof(employeeService)))
				.GetEmployeeForCurrentUser(UoW);

			if(_currentEmployee is null)
			{
				Dispose();
				throw new AbortCreatingPageException(
					"Ваш пользователь не привязан к сотруднику. Дальнейшая работа не возможна",
					"Ошибка");
			}

			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_validationContext = validationContextFactory.CreateNewValidationContext(Entity);
			_permissionResult = currentPermissionService.ValidateEntityPermission(typeof(RequestForCall));

			Title = Entity.ToString();
			
			CreateCommands();
			ConfigureEntryViewModels();
			Entity.PropertyChanged += OnEntityPropertyChanged;
		}

		public DelegateCommand GetToWorkCommand { get; private set; }
		public DelegateCommand CloseRequestCommand { get; private set; }
		public DelegateCommand CancelCommand { get; private set; }

		public IEntityEntryViewModel NomenclatureEntryViewModel { get; private set; }
		public IEntityEntryViewModel ClosedReasonEntryViewModel { get; private set; }
		
		public string IdToString => Entity.Id.ToString();
		public bool CanEdit => _permissionResult.CanUpdate;
		public bool AskSaveOnClose => CanEdit;
		public bool CanGetToWork => CanEdit && Entity.EmployeeWorkWith is null;
		public bool CanShowId => Entity.Id > 0;
		
		public bool CanCreateOrder =>
			CanEdit
			&& OrderIsNullAndRequestNotClosedStatus
			&& Entity.EmployeeWorkWith != null
			&& Entity.EmployeeWorkWith.Id == _currentEmployee.Id;
		
		public bool CanCloseRequest =>
			CanEdit
			&& OrderIsNullAndRequestNotClosedStatus
			&& Entity.EmployeeWorkWith != null
			&& Entity.EmployeeWorkWith.Id == _currentEmployee.Id;
		
		public bool CanShowEmployeeWorkWith => Entity.EmployeeWorkWith != null;
		public bool CanShowOrder => Entity.Order != null;

		public string EmployeeWorkWith =>
			Entity.EmployeeWorkWith is null
				? "Заявка не взята в работу"
				: $"{ Entity.EmployeeWorkWith.ShortName }";

		public string Order =>
			Entity.Order is null
				? "Заказ не создан"
				: $"{ Entity.Order.Title }";

		public string Status => Entity.RequestForCallStatus.GetEnumDisplayName();
		
		private bool OrderIsNullAndRequestNotClosedStatus =>
			Entity.Order is null && Entity.RequestForCallStatus != RequestForCallStatus.Closed;

		public void AttachOrder(int orderId)
		{
			var order = UoW.GetById<Order>(orderId);
			Entity.AttachOrder(order);
		}

		protected override bool Validate() => validator.Validate(Entity, _validationContext);
		
		private void CreateCommands()
		{
			CreateGetToWorkCommand();
			CreateCloseRequestCommand();
			CreateCancelCommand();
		}

		private void CreateGetToWorkCommand()
		{
			GetToWorkCommand = new DelegateCommand(
				() =>
				{
					if(Entity.EmployeeWorkWith != null && Entity.EmployeeWorkWith.Id != _currentEmployee.Id)
					{
						_interactiveService.ShowMessage(
							ImportanceLevel.Warning,
							$"Эту заявку уже обрабатывает {Entity.EmployeeWorkWith.ShortName}. Дальнейшая работа не возможна");
						return;
					}

					if(Entity.EmployeeWorkWith is null)
					{
						Entity.EmployeeWorkWith = _currentEmployee;
						Save();
					}
				});
			GetToWorkCommand.CanExecuteChangedWith(this, vm => vm.CanGetToWork);
		}
		
		private void CreateCloseRequestCommand()
		{
			CloseRequestCommand = new DelegateCommand(
				() =>
				{
					if(Entity.ClosedReason is null)
					{
						_interactiveService.ShowMessage(ImportanceLevel.Warning, "Укажите причину закрытия заявки");
						return;
					}
					
					var oldStatus = Entity.RequestForCallStatus;
					Entity.RequestForCallStatus = RequestForCallStatus.Closed;
					
					if(!Save())
					{
						Entity.RequestForCallStatus = oldStatus;
						return;
					}
					
					Close(false, CloseSource.Save);
				});
			CloseRequestCommand.CanExecuteChangedWith(this, vm => vm.CanCloseRequest);
		}
		
		private void CreateCancelCommand()
		{
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
		}
		
		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.EmployeeWorkWith))
			{
				OnPropertyChanged(nameof(EmployeeWorkWith));
				OnPropertyChanged(nameof(CanGetToWork));
				OnPropertyChanged(nameof(CanShowEmployeeWorkWith));
				OnPropertyChanged(nameof(CanCreateOrder));
			}
			
			if(e.PropertyName == nameof(Entity.Order))
			{
				OnPropertyChanged(nameof(Order));
				OnPropertyChanged(nameof(CanShowOrder));
				OnPropertyChanged(nameof(CanCreateOrder));
			}
			
			if(e.PropertyName == nameof(Entity.RequestForCallStatus))
			{
				OnPropertyChanged(nameof(CanCreateOrder));
			}
		}
		
		private void ConfigureEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<RequestForCall>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			NomenclatureEntryViewModel = builder.ForProperty(x => x.Nomenclature)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel>()
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();
			NomenclatureEntryViewModel.IsEditable = false;
			
			ClosedReasonEntryViewModel = builder.ForProperty(x => x.ClosedReason)
				.UseViewModelJournalAndAutocompleter<RequestsForCallClosedReasonsJournalViewModel>()
				.UseViewModelDialog<RequestForCallClosedReasonViewModel>()
				.Finish();
		}

		public override void Dispose()
		{
			Entity.PropertyChanged -= OnEntityPropertyChanged;
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
