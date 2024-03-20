using System;
using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class RequestForCallViewModel : EntityTabViewModelBase<RequestForCall>
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly Employee _currentEmployee;

		public RequestForCallViewModel(
			IEntityUoWBuilder entityUoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			INavigationManager navigation,
			ILifetimeScope lifetimeScope) : base(entityUoWBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_currentEmployee =
				(employeeService ?? throw new ArgumentNullException(nameof(employeeService)))
				.GetEmployeeForCurrentUser(UoW);

			if(_currentEmployee is null)
			{
				AbortOpening("Ваш пользователь не привязан к сотруднику. Дальнейшая работа не возможна");
			}
			
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			
			CreateCommands();
			CreatePropertyChangeRelations();
			ConfigureEntryViewModels();
		}

		public DelegateCommand GetToWorkCommand { get; private set; }
		public DelegateCommand CloseRequestCommand { get; private set; }

		public IEntityEntryViewModel NomenclatureViewModel { get; private set; }
		
		public string IdToString => Entity.Id.ToString();
		public bool CanGetToWork => Entity.EmployeeWorkWith is null;
		
		public bool CanCreateOrder =>
			OrderIsNullAndRequestNotClosedStatus
			&& Entity.EmployeeWorkWith != null
			&& Entity.EmployeeWorkWith.Id == _currentEmployee.Id;
		
		public bool CanShowEmployeeWorkWith => Entity.EmployeeWorkWith != null;
		public bool CanShowOrder => Entity.Order != null;
		
		
		public string EmployeeWorkWith =>
			Entity.EmployeeWorkWith is null
				? "Онлайн заказ не взят в работу"
				: $"{ Entity.EmployeeWorkWith.ShortName }";
		
		public string Order =>
			Entity.Order is null
				? "Заказ не создан"
				: $"{ Entity.Order.Title }";
		
		private bool OrderIsNullAndRequestNotClosedStatus =>
			Entity.Order is null && Entity.RequestForCallStatus != RequestForCallStatus.Closed;

		private void CreateCommands()
		{
			CreateGetToWorkCommand();
			CreateCloseRequestCommand();
		}
		
		private void CreateGetToWorkCommand()
		{
			GetToWorkCommand = new DelegateCommand(
				() =>
				{
					if(Entity.EmployeeWorkWith != null && Entity.EmployeeWorkWith.Id != _currentEmployee.Id)
					{
						ShowWarningMessage($"Эту заявку уже обрабатывает {Entity.EmployeeWorkWith.ShortName}. Дальнейшая работа не возможна");
						return;
					}

					if(Entity.EmployeeWorkWith is null)
					{
						Entity.EmployeeWorkWith = _currentEmployee;
						Save(false);
					}
				});
		}
		
		private void CreateCloseRequestCommand()
		{
			CloseRequestCommand = new DelegateCommand(
				() =>
				{
					if(Entity.ClosedReason is null)
					{
						ShowWarningMessage("Укажите причину закрытия заявки");
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
		}
		
		private void CreatePropertyChangeRelations()
		{
			SetPropertyChangeRelation(
				x => x.EmployeeWorkWith,
				() => EmployeeWorkWith,
				() => CanGetToWork,
				() => CanShowEmployeeWorkWith,
				() => CanCreateOrder);
			
			SetPropertyChangeRelation(
				x => x.Order,
				() => Order,
				() => CanShowOrder,
				() => CanCreateOrder);
			
			SetPropertyChangeRelation(
				x => x.RequestForCallStatus,
				() => CanCreateOrder);
		}
		
		private void ConfigureEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<RequestForCall>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			NomenclatureViewModel = builder.ForProperty(x => x.Nomenclature)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel>()
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();
		}
	}
}
