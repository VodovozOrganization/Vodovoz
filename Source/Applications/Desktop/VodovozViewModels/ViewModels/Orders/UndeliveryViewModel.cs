using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sms;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Organizations;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewModels.Orders
{
	public class UndeliveryViewModel : EntityTabViewModelBase<UndeliveredOrder>, IAskSaveOnCloseViewModel, ITdiTabAddedNotifier
	{

		private readonly IEmployeeRepository _employeeRepository;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly ISubdivisionSettings _subdivisionSettings;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly ISmsNotifier _smsNotifier;

		public UndeliveryViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeRepository employeeRepository,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			IOrderRepository orderRepository,
			ISubdivisionSettings subdivisionSettings,
			ICallTaskWorker callTaskWorker,
			INomenclatureSettings nomenclatureSettings,
			ISmsNotifier smsNotifier,
			ILifetimeScope scope,
			IUndeliveredOrderViewModelFactory undeliveredOrderViewModelFactory,
			IUndeliveryDiscussionsViewModelFactory undeliveryDiscussionsViewModelFactory,
			bool isForSalesDepartment = false)
			: base(uowBuilder, uowFactory, commonServices, navigationManager)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_undeliveredOrdersRepository = undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_subdivisionSettings = subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_smsNotifier = smsNotifier ?? throw new ArgumentNullException(nameof(smsNotifier));

			UndeliveredOrderViewModel = (undeliveredOrderViewModelFactory ?? throw new ArgumentNullException(nameof(undeliveredOrderViewModelFactory)))
				.CreateUndeliveredOrderViewModel(Entity, scope, this, UoW);

			UndeliveryDiscussionsViewModel = (undeliveryDiscussionsViewModelFactory ?? throw new ArgumentNullException(nameof(undeliveryDiscussionsViewModelFactory)))
				.CreateUndeliveryDiscussionsViewModel(Entity, this, scope, UoW);

			if(UoW.IsNew)
			{
				FillNewUndelivery();
				TabName = "Новый недовоз";
			}
			else
			{
				TabName = UndeliveredOrder.Title;
			}

			ConfigureDlg(isForSalesDepartment);
		}

		private void FillNewUndelivery()
		{			
			UndeliveredOrder.Author = UndeliveredOrder.EmployeeRegistrator = _employeeRepository.GetEmployeeForCurrentUser(UoW);

			if(UndeliveredOrder.Author == null)
			{
				AbortOpening("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать недовозы, так как некого указывать в качестве автора документа.");				
			}

			TabName = "Новый недовоз";
			UndeliveredOrder.TimeOfCreation = DateTime.Now;			
		}

		public void ConfigureDlg(bool isForSalesDepartment = false)
		{
			if(isForSalesDepartment)
			{
				var salesDepartmentId = _subdivisionSettings.GetSalesSubdivisionId();
				UndeliveredOrder.InProcessAtDepartment = UoW.GetById<Subdivision>(salesDepartmentId);
			}			

			UndeliveredOrderViewModel.IsSaved += IsSaved;
		}

		private bool IsSaved() => Save(false);

		public override bool Save(bool needClose = true)
		{
			var validator = CommonServices.ValidationService;

			if(!validator.Validate(UndeliveredOrder))
			{
				return false;
			}

			if(UndeliveredOrder.Id == 0)
			{
				UndeliveredOrder.OldOrder.SetUndeliveredStatus(UoW, _nomenclatureSettings, _callTaskWorker);
			}

			UndeliveredOrderViewModel.BeforeSaveCommand.Execute();

			//случай, если создавать новый недовоз не нужно, но нужно обновить старый заказ
			if(!CanCreateUndelivery())
			{
				UoW.Save(UndeliveredOrder.OldOrder);
				UoW.Commit();
				Close(false, CloseSource.Self);
				return false;
			}

			UoW.Save(UndeliveredOrder);

			if(UndeliveredOrder.NewOrder != null
			   && UndeliveredOrder.OrderTransferType == TransferType.AutoTransferNotApproved
			   && UndeliveredOrder.NewOrder.OrderStatus != OrderStatus.Canceled)
			{
				ProcessSmsNotification();
			}

			if(needClose)
			{
				Close(false, CloseSource.Self);
			}

			return true;
		}

		/// <summary>
		/// Проверка на возможность создания нового недовоза
		/// </summary>
		/// <returns><c>true</c>, если можем создать, <c>false</c> если создать недовоз не можем,
		/// при этом добавляется автокомментарий к существующему недовозу с содержимым
		/// нового (но не добавленного) недовоза.</returns>
		private bool CanCreateUndelivery()
		{
			if(UndeliveredOrder.Id > 0)
			{
				return true;
			}

			var otherUndelivery = _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, UndeliveredOrder.OldOrder).FirstOrDefault();
			
			if(otherUndelivery == null)
			{
				return true;
			}

			otherUndelivery.AddCommentToTheField(UoW, CommentedFields.Reason, UndeliveredOrder.GetUndeliveryInfo(_orderRepository));
			
			return false;
		}


		private void ProcessSmsNotification()
		{			
			_smsNotifier.NotifyUndeliveryAutoTransferNotApproved(UndeliveredOrder);
		}

		//реализация метода интерфейса ITdiTabAddedNotifier
		public void OnTabAdded()
		{
			UndeliveredOrderViewModel.OldOrderSelectCommand.Execute();
		}

		public UndeliveryDiscussionsViewModel UndeliveryDiscussionsViewModel { get; }

		public UndeliveredOrderViewModel UndeliveredOrderViewModel { get; }

		public UndeliveredOrder UndeliveredOrder => Entity;

		public bool AskSaveOnClose => CanEdit;

		public bool CanEdit => PermissionResult.CanUpdate;

		public override void Dispose()
		{
			UndeliveredOrderViewModel.IsSaved -= IsSaved;
			UndeliveredOrderViewModel.Dispose();
			UoW?.Dispose();
			base.Dispose();
		}
	}
}
