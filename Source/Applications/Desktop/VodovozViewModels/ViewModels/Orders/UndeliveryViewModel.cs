using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Application.Orders;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sms;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Factories;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Organizations;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewModels.Orders
{
	public class UndeliveryViewModel : DialogTabViewModelBase, IAskSaveOnCloseViewModel, ITdiTabAddedNotifier
	{
		private readonly ICommonServices _commonServices;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly ISubdivisionSettings _subdivisionSettings;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly ISmsNotifier _smsNotifier;
		private ValidationContext _validationContext;
		private bool _addedCommentToOldUndelivery;
		private bool _forceSave;
		private bool _isExternalUoW;

		public UndeliveryViewModel(

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
			IValidationContextFactory validationContextFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			IUnitOfWork externalUoW = null,
			int oldOrderId = 0,
			bool isForSalesDepartment = false)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_undeliveredOrdersRepository = undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_subdivisionSettings = subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_smsNotifier = smsNotifier ?? throw new ArgumentNullException(nameof(smsNotifier));

			if(externalUoW != null)
			{
				UoW = externalUoW;

				_isExternalUoW = true;
			}
			else
			{
				UoW = unitOfWorkFactory.CreateWithoutRoot();
			}

			var undelivery = undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, oldOrderId).FirstOrDefault();

			Entity = undelivery ?? new UndeliveredOrder();

			_currentUser = (employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository))).GetEmployeeForCurrentUser(UoW);

			if(Entity.Id == 0)
			{
				TabName = "Новый недовоз";

				FillNewUndelivery();

				if(oldOrderId > 0)
				{
					Entity.OldOrder = UoW.GetById<Order>(oldOrderId);
				}
			}
			else
			{
				TabName = Entity.Title;
			}

			UndeliveredOrderViewModel = (undeliveredOrderViewModelFactory ?? throw new ArgumentNullException(nameof(undeliveredOrderViewModelFactory)))
				.CreateUndeliveredOrderViewModel(Entity, scope, this, UoW);

			UndeliveryDiscussionsViewModel = (undeliveryDiscussionsViewModelFactory ?? throw new ArgumentNullException(nameof(undeliveryDiscussionsViewModelFactory)))
				.CreateUndeliveryDiscussionsViewModel(Entity, this, scope, UoW);

			CanEdit = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(UndeliveredOrder)).CanUpdate;

			_validationContext = validationContextFactory.CreateNewValidationContext(Entity);
			_validationContext.ServiceContainer.AddService(typeof(IOrderRepository), _orderRepository);

			if(isForSalesDepartment)
			{
				var salesDepartmentId = _subdivisionSettings.GetSalesSubdivisionId();
				Entity.InProcessAtDepartment = UoW.GetById<Subdivision>(salesDepartmentId);
			}

			UndeliveredOrderViewModel.UndelivedOrderSaved += OnEntitySaved;

			Entity.ObservableUndeliveryDiscussions.ElementChanged += OnObservableUndeliveryDiscussionsElementChanged;
			Entity.ObservableUndeliveryDiscussions.ListContentChanged += OnObservableUndeliveryDiscussionsListContentChanged;
		}

		private void FillNewUndelivery()
		{
			Entity.UoW = UoW;
			Entity.Author = Entity.EmployeeRegistrator = _currentUser;

			if(Entity.Author == null)
			{
				AbortOpening("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать недовозы, так как некого указывать в качестве автора документа.");
			}

			Entity.TimeOfCreation = DateTime.Now;
			Entity.CreateOkkDiscussion(UoW);
		}

		private void OnObservableUndeliveryDiscussionsListContentChanged(object sender, EventArgs e)
		{
			OnDiscussionsChanged();
		}

		private void OnObservableUndeliveryDiscussionsElementChanged(object aList, int[] aIdx)
		{
			OnDiscussionsChanged();
		}

		private void OnDiscussionsChanged()
		{
			Entity.UpdateUndeliveryStatus();
		}

		private bool OnEntitySaved()
		{
			_forceSave = true;
			var result = Save(false);
			_forceSave = false;

			return result;
		}

		/// <summary>
		/// Проверка на возможность создания нового недовоза
		/// </summary>
		/// <returns><c>true</c>, если можем создать, <c>false</c> если создать недовоз не можем,
		/// при этом добавляется автокомментарий к существующему недовозу с содержимым
		/// нового (но не добавленного) недовоза.</returns>
		private bool CanCreateUndelivery()
		{
			if(Entity.Id > 0)
			{
				return true;
			}

			var otherUndelivery = _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, Entity.OldOrder).FirstOrDefault();

			if(otherUndelivery == null)
			{
				return true;
			}

			otherUndelivery.AddAutoCommentToOkkDiscussion(UoW, Entity.GetUndeliveryInfo(_orderRepository));

			_addedCommentToOldUndelivery = true;

			return false;
		}

		private void ProcessSmsNotification()
		{
			_smsNotifier.NotifyUndeliveryAutoTransferNotApproved(Entity);
		}

		public override bool Save(bool needClose)
		{
			var validator = _commonServices.ValidationService;

			if(!validator.Validate(Entity, _validationContext))
			{
				return false;
			}

			if(Entity.Id == 0)
			{
				Entity.OldOrder.SetUndeliveredStatus(UoW, _nomenclatureSettings, _callTaskWorker);
			}

			UndeliveredOrderViewModel.BeforeSaveCommand.Execute();

			//случай, если создавать новый недовоз не нужно, но нужно обновить старый заказ
			if(!CanCreateUndelivery())
			{
				if(_forceSave)
				{
					UoW.Save(Entity);
				}

				UoW.Save(Entity.OldOrder);
				UoW.Commit();

				if(_addedCommentToOldUndelivery)
				{
					Saved?.Invoke(this, new UndeliveryOnOrderCloseEventArgs(Entity, needClose));
				}

				Close(false, CloseSource.Self);

				return false;
			}

			UoW.Save(Entity);

			if(!_isExternalUoW)
			{
				UoW.Commit();
			}

			if(Entity.NewOrder != null
			   && Entity.OrderTransferType == TransferType.AutoTransferNotApproved
			   && Entity.NewOrder.OrderStatus != OrderStatus.Canceled)
			{
				ProcessSmsNotification();
			}

			Saved?.Invoke(this, new UndeliveryOnOrderCloseEventArgs(Entity, needClose));

			if(needClose)
			{
				Close(false, CloseSource.Self);
			}

			return true;
		}

		public void OnTabAdded()
		{
			if(Entity.OldOrder == null)
			{
				UndeliveredOrderViewModel.OldOrderSelectCommand.Execute();
			}
		}

		public UndeliveredOrder Entity { get; private set; }

		private Employee _currentUser;

		public UndeliveryDiscussionsViewModel UndeliveryDiscussionsViewModel { get; }

		public UndeliveredOrderViewModel UndeliveredOrderViewModel { get; }

		public bool AskSaveOnClose => CanEdit;

		public bool CanEdit { get; }

		public event EventHandler<UndeliveryOnOrderCloseEventArgs> Saved;

		public override void Dispose()
		{
			UndeliveredOrderViewModel.UndelivedOrderSaved -= OnEntitySaved;
			Entity.ObservableUndeliveryDiscussions.ElementChanged -= OnObservableUndeliveryDiscussionsElementChanged;
			Entity.ObservableUndeliveryDiscussions.ListContentChanged -= OnObservableUndeliveryDiscussionsListContentChanged;
			UndeliveredOrderViewModel.UndelivedOrderSaved -= OnEntitySaved;
			UndeliveredOrderViewModel.Dispose();

			if(!_isExternalUoW)
			{
				base.Dispose();
			}
		}
	}
}
