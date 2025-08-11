using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.Factories;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OrderRatingViewModel : EntityDialogViewModelBase<OrderRating>, IAskSaveOnCloseViewModel
	{
		private readonly IPermissionResult _permissionResult;
		private readonly ICommonServices _commonServices;
		private readonly IComplaintsRepository _complaintsRepository;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly ValidationContext _validationContext;
		private readonly Employee _currentEmployee;
		private (int ComplaintId, bool HasOrderRating) _complaintData;
		
		public OrderRatingViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			INavigationManager navigation,
			ICommonServices commonServices,
			IValidationContextFactory validationContextFactory,
			IComplaintsRepository complaintsRepository,
			IEmployeeService employeeService,
			IGtkTabsOpener gtkTabsOpener)
			: base(uowBuilder, uowFactory, navigation)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_permissionResult = commonServices.CurrentPermissionService
				.ValidateEntityPermission(typeof(OrderRatingReason));
			_currentEmployee = (employeeService ?? throw new ArgumentNullException(nameof(employeeService)))
				.GetEmployeeForUser(UoW, _commonServices.UserService.CurrentUserId);

			if(_currentEmployee is null)
			{
				Dispose();
				throw new AbortCreatingPageException("Ваш пользователь не привязан к сотруднику. Дальнейшая работа не возможна", "Ошибка");
			}
			
			_validationContext = validationContextFactory.CreateNewValidationContext(Entity);
			Title = Entity.ToString();
			
			CreateCommands();
			OrderRatingReasons = Entity.OrderRatingReasons;
			UpdateComplaintInformation();
		}

		private (int ComplaintId, bool HasOrderRating) ComplaintData
		{
			get => _complaintData;
			set
			{
				if(SetField(ref _complaintData, value))
				{
					OnPropertyChanged(nameof(CreateOrOpenComplaint));
				}
			} 
		}

		public string CreateOrOpenComplaint => _complaintData.ComplaintId != default ? "Открыть рекламацию" : "Создать рекламацию";
		public string IdToString => Entity.Id.ToString();
		public bool CanShowId => Entity.Id > 0;
		public bool OrderIsNotNull => Entity.Order != null;
		public bool CanShowProcessedBy => Entity.ProcessedByEmployee != null;

		public string OrderIdString =>
			Entity.Order != null
				? Entity.Order.Id.ToString()
				: string.Empty;
		
		public string OnlineOrderIdString =>
			OnlineOrderIsNotNull
				? Entity.OnlineOrder.Id.ToString()
				: string.Empty;

		public string ProcessedBy =>
			Entity.ProcessedByEmployee != null
				? Entity.ProcessedByEmployee.ShortName
				: "Не обработана";

		public bool AskSaveOnClose => CanEdit;
		public bool CanEdit => _permissionResult.CanUpdate;
		public DelegateCommand SaveAndCloseCommand { get; private set; }
		public DelegateCommand CloseCommand { get; private set; }
		public DelegateCommand OpenOrderCommand { get; private set; }
		public DelegateCommand OpenOnlineOrderCommand { get; private set; }
		public DelegateCommand ProcessCommand { get; private set; }
		public DelegateCommand CreateComplaintCommand { get; private set; }
		public IEnumerable<INamedDomainObject> OrderRatingReasons { get; }

		public bool OnlineOrderIsNotNull => Entity.OnlineOrder != null;
		private bool IsNewOrderRatingStatus => Entity.OrderRatingStatus == OrderRatingStatus.New;
		
		private void UpdateComplaintInformation()
		{
			ComplaintData = _complaintsRepository.GetComplaintIdByOrderRating(UoW, Entity.Id);

			if(ComplaintData.ComplaintId == default && Entity.Order != null)
			{
				ComplaintData = _complaintsRepository.GetTodayComplaintIdByOrder(UoW, Entity.Order.Id);
			}
		}

		private void CreateCommands()
		{
			CreateSaveAndCloseCommand();
			CreateCloseCommand();
			CreateOpenOrderCommand();
			CreateOpenOnlineOrderCommand();
			CreateProcessCommand();
			CreateCreateComplaintCommand();
		}

		private void CreateSaveAndCloseCommand()
		{
			SaveAndCloseCommand = new DelegateCommand(() => SaveAndClose());
			SaveAndCloseCommand.CanExecuteChangedWith(this, x => x.CanEdit);
		}

		private void CreateCloseCommand()
		{
			CloseCommand = new DelegateCommand(
				() => Close(false, CloseSource.Cancel)
			);
		}
		
		private void CreateOpenOrderCommand()
		{
			OpenOrderCommand = new DelegateCommand(
				() => _gtkTabsOpener.OpenOrderDlgFromViewModelByNavigator(this, Entity.Order.Id),
				() => Entity.Order != null);
		}
		
		private void CreateOpenOnlineOrderCommand()
		{
			OpenOnlineOrderCommand = new DelegateCommand(
				() => NavigationManager.OpenViewModel<OnlineOrderViewModel, IEntityUoWBuilder>(
					this, EntityUoWBuilder.ForOpen(Entity.OnlineOrder.Id)));
			OpenOnlineOrderCommand.CanExecuteChangedWith(this, x => x.OnlineOrderIsNotNull);
		}
		
		private void CreateProcessCommand()
		{
			ProcessCommand = new DelegateCommand(
				() => Entity.Process(_currentEmployee));
			ProcessCommand.CanExecuteChangedWith(this, x => x.CanEdit);
		}

		private void CreateCreateComplaintCommand()
		{
			CreateComplaintCommand = new DelegateCommand(
				() =>
				{
					if(_complaintData.ComplaintId == default)
					{
						UpdateComplaintInformation();
					}

					if(_complaintData.ComplaintId == default)
					{
						CreateNewComplaint();
					}
					else
					{
						if(!_complaintData.HasOrderRating)
						{
							if(_commonServices.InteractiveService.Question(
								   "Найдена созданная рекламация по оцененному заказу. Перейти в нее и связать с данной оценкой?"))
							{
								var viewModel = NavigationManager.OpenViewModel<ComplaintViewModel, IEntityUoWBuilder>(
									this,
									EntityUoWBuilder.ForOpen(_complaintData.ComplaintId),
									OpenPageOptions.AsSlave,
									vm =>
										vm.EntitySaved += (sender, args) => ComplaintData = (args.Entity.GetId(), true))
									.ViewModel;
								viewModel.SetOrderRating(Entity.Id);
							}
						}
						else
						{
							NavigationManager.OpenViewModel<ComplaintViewModel, IEntityUoWBuilder>(
								this, EntityUoWBuilder.ForOpen(_complaintData.ComplaintId));
						}
					}
				});
			ProcessCommand.CanExecuteChangedWith(this, x => x.CanEdit);
		}

		private void CreateNewComplaint()
		{
			var viewModel =
				NavigationManager.OpenViewModel<CreateComplaintViewModel, IEntityUoWBuilder>(
					this,
					EntityUoWBuilder.ForCreate(),
					OpenPageOptions.AsSlave,
					vm =>
						vm.EntitySaved += (sender, args) => ComplaintData = (args.Entity.GetId(), true)).ViewModel;
			viewModel.SetOrderRating(Entity.Id);
		}

		protected override bool Validate() => _commonServices.ValidationService.Validate(Entity, _validationContext);
	}
}
