using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Vodovoz.Application.FileStorage;
using Vodovoz.Application.Orders;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sms;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Factories;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Organizations;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewModels.Orders
{
	public class UndeliveryViewModel : DialogTabViewModelBase, IAskSaveOnCloseViewModel, ITdiTabAddedNotifier
	{
		private readonly ILogger<UndeliveryViewModel> _logger;
		private readonly ICommonServices _commonServices;
		private readonly IInteractiveService _interactiveService;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly ISubdivisionSettings _subdivisionSettings;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly ISmsNotifier _smsNotifier;
		private readonly ILifetimeScope _scope;
		private readonly IUndeliveredOrderViewModelFactory _undeliveredOrderViewModelFactory;
		private readonly IUndeliveryDiscussionsViewModelFactory _undeliveryDiscussionsViewModelFactory;
		private readonly IValidationContextFactory _validationContextFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IUndeliveryDiscussionCommentFileStorageService _undeliveryDiscussionCommentFileStorageService;
		private readonly IRouteListService _routeListService;
		private ValidationContext _validationContext;
		private bool _addedCommentToOldUndelivery;
		private bool _forceSave;
		private bool _isExternalUoW;
		private bool _isNewUndelivery;
		private bool _isFromRouteListClosing;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public UndeliveryViewModel(
			ILogger<UndeliveryViewModel> logger,
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
			IUndeliveryDiscussionCommentFileStorageService undeliveryDiscussionCommentFileStorageService,
			IRouteListService routeListService)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_interactiveService = commonServices?.InteractiveService ?? throw new ArgumentNullException(nameof(commonServices.InteractiveService));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_undeliveredOrdersRepository = undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_subdivisionSettings = subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_smsNotifier = smsNotifier ?? throw new ArgumentNullException(nameof(smsNotifier));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_undeliveredOrderViewModelFactory = undeliveredOrderViewModelFactory ?? throw new ArgumentNullException(nameof(undeliveredOrderViewModelFactory));
			_undeliveryDiscussionsViewModelFactory = undeliveryDiscussionsViewModelFactory ?? throw new ArgumentNullException(nameof(undeliveryDiscussionsViewModelFactory));
			_validationContextFactory = validationContextFactory ?? throw new ArgumentNullException(nameof(validationContextFactory));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_undeliveryDiscussionCommentFileStorageService = undeliveryDiscussionCommentFileStorageService ?? throw new ArgumentNullException(nameof(undeliveryDiscussionCommentFileStorageService));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
		}

		public void Initialize(IUnitOfWork extrenalUoW = null, int oldOrderId = 0, bool isForSalesDepartment = false, bool isFromRouteListClosing = false)
		{
			_isFromRouteListClosing = isFromRouteListClosing;

			if(extrenalUoW != null)
			{
				UoW = extrenalUoW;

				_isExternalUoW = true;
			}
			else
			{
				UoW = _unitOfWorkFactory.CreateWithoutRoot();
			}

			_currentUser = _employeeRepository.GetEmployeeForCurrentUser(UoW);

			var undelivery = _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, oldOrderId).FirstOrDefault();

			Entity = undelivery ?? new UndeliveredOrder();			

			if(Entity.Id == 0)
			{
				_isNewUndelivery = true;

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

			UndeliveredOrderViewModel =
				_undeliveredOrderViewModelFactory.CreateUndeliveredOrderViewModel(Entity, _scope, this, UoW);

			UndeliveredOrderViewModel.SaveUndelivery += SaveUndelivery;

			UndeliveryDiscussionsViewModel = 
				_undeliveryDiscussionsViewModelFactory.CreateUndeliveryDiscussionsViewModel(Entity, this, _scope, UoW);

			CanEdit = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(UndeliveredOrder)).CanUpdate;

			_validationContext = _validationContextFactory.CreateNewValidationContext(Entity);

			if(isForSalesDepartment)
			{
				var salesDepartmentId = _subdivisionSettings.GetSalesSubdivisionId();
				Entity.InProcessAtDepartment = UoW.GetById<Subdivision>(salesDepartmentId);
			}

			Entity.ObservableUndeliveryDiscussions.ElementChanged += OnObservableUndeliveryDiscussionsElementChanged;
			Entity.ObservableUndeliveryDiscussions.ListContentChanged += OnObservableUndeliveryDiscussionsListContentChanged;
		}

		private void FillNewUndelivery()
		{
			Entity.UoW = UoW;
			Entity.Author = Entity.EmployeeRegistrator = _currentUser ?? throw new ArgumentNullException(nameof(_currentUser));

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
			if(!_isNewUndelivery)
			{
				Entity.UpdateUndeliveryStatusByDiscussionsStatus();
			}
		}

		private bool SaveUndelivery(bool needClose = false)
		{
			_forceSave = true;
			var result = Save(needClose);
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
			if(_isExternalUoW)
			{
				_smsNotifier.NotifyUndeliveryAutoTransferNotApproved(Entity, UoW);
			}
			else
			{
				_smsNotifier.NotifyUndeliveryAutoTransferNotApproved(Entity);
			}
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
				Entity.OldOrder.SetUndeliveredStatus(UoW, _routeListService, _nomenclatureSettings, _callTaskWorker, needCreateDeliveryFreeBalanceOperation: 
					!_isFromRouteListClosing);
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

				if(!_isExternalUoW)
				{
					UoW.Commit();
				}

				if(_addedCommentToOldUndelivery)
				{
					Saved?.Invoke(this, new UndeliveryOnOrderCloseEventArgs(Entity, !_isExternalUoW || needClose));
				}

				Close(false, CloseSource.Self);

				return false;
			}

			UoW.Save(Entity);

			if(!_isExternalUoW)
			{
				UoW.Commit();
			}

			AddDiscussionsCommentFilesIfNeeded();

			if(Entity.NewOrder != null
			   && Entity.OrderTransferType == TransferType.AutoTransferNotApproved
			   && Entity.NewOrder.OrderStatus != OrderStatus.Canceled)
			{
				ProcessSmsNotification();
			}

			var needCloseParrentTab = !_isExternalUoW || (needClose && !_isFromRouteListClosing);

			Saved?.Invoke(this, new UndeliveryOnOrderCloseEventArgs(Entity, needCloseParrentTab));

			if(needClose)
			{
				Close(false, CloseSource.Self);
			}

			return true;
		}

		private void AddDiscussionsCommentFilesIfNeeded()
		{
			var errors = new Dictionary<string, string>();
			var repeat = false;

			do
			{
				foreach(var undeliveryDiscussionViewModel in UndeliveryDiscussionsViewModel.ObservableUndeliveryDiscussionViewModels)
				{
					foreach(var keyValuePair in undeliveryDiscussionViewModel.FilesToUploadOnSave)
					{
						var commentId = keyValuePair.Key.Invoke();

						_logger.LogInformation(
							"Попытка сохранения файлов по комментарию {CommentId} из обсуждения {DiscussionId}",
							commentId,
							undeliveryDiscussionViewModel.Entity.Id);
						
						var comment = Entity
							.ObservableUndeliveryDiscussions
							.FirstOrDefault(cd => cd.Comments.Any(c => c.Id == commentId))
							?.Comments
							?.FirstOrDefault(c => c.Id == commentId);

						foreach(var fileToUploadPair in keyValuePair.Value)
						{
							_logger.LogInformation("Сохранение файла {FileName} размером {Size}",
								fileToUploadPair.Key,
								fileToUploadPair.Value.Length);
							
							using(var ms = new MemoryStream(fileToUploadPair.Value))
							{
								var result = _undeliveryDiscussionCommentFileStorageService.CreateFileAsync(
								comment,
									fileToUploadPair.Key,
									ms,
									_cancellationTokenSource.Token)
									.GetAwaiter()
									.GetResult();

								if(result.IsFailure && !result.Errors.All(x => x.Code == Application.Errors.S3.FileAlreadyExists.ToString()))
								{
									_logger.LogWarning("Не удалось сохранить файл {FileName} размером {Size}",
										fileToUploadPair.Key,
										fileToUploadPair.Value.Length);
									errors.Add(fileToUploadPair.Key, string.Join(", ", result.Errors.Select(e => e.Message)));
								}
							}
						}
					}
				}

				if(errors.Any())
				{
					repeat = _interactiveService.Question(
						"Не удалось загрузить файлы:\n" +
						string.Join("\n- ", errors.Select(fekv => $"{fekv.Key} - {fekv.Value}")) + "\n" +
						"\n" +
						"Повторить попытку?",
						"Ошибка загрузки файлов");

					errors.Clear();
				}
				else
				{
					repeat = false;
				}
			}
			while(repeat);
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

		public UndeliveryDiscussionsViewModel UndeliveryDiscussionsViewModel { get; private set; }

		public UndeliveredOrderViewModel UndeliveredOrderViewModel { get; private set; }

		public bool AskSaveOnClose => CanEdit;

		public bool CanEdit { get; private set; }

		public event EventHandler<UndeliveryOnOrderCloseEventArgs> Saved;

		public override void Dispose()
		{
			UndeliveredOrderViewModel.SaveUndelivery -= SaveUndelivery;
			Entity.ObservableUndeliveryDiscussions.ElementChanged -= OnObservableUndeliveryDiscussionsElementChanged;
			Entity.ObservableUndeliveryDiscussions.ListContentChanged -= OnObservableUndeliveryDiscussionsListContentChanged;
			UndeliveredOrderViewModel.Dispose();

			if(!_isExternalUoW)
			{
				base.Dispose();
			}
		}
	}
}
