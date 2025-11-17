using QS.ViewModels;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.EntityRepositories;
using QS.Navigation;
using QS.Project.Domain;
using Vodovoz.Services;
using Vodovoz.Domain.Employees;
using QS.Project.Services;
using System;
using QS.Commands;
using Vodovoz.Domain.Client;
using System.Linq;
using System.Collections.Generic;
using Vodovoz.Domain.Organizations;
using NHibernate.Transform;
using Vodovoz.Domain.Retail;
using QS.DomainModel.Entity;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Application.FileStorage;
using System.Threading;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.ViewModels.Dialogs.Counterparties
{
	public class CloseSupplyToCounterpartyViewModel : EntityTabViewModelBase<Domain.Client.Counterparty>
	{
		private readonly ICommonServices _commonServices;
		private readonly IEmployeeService _employeeService;
		private readonly IUserRepository _userRepository;
		private readonly ICounterpartyFileStorageService _counterpartyFileStorageService;
		private readonly int _currentUserId = ServicesConfig.UserService.CurrentUserId;

		private Employee _currentEmployee;
		private string _closeDeliveryComment = string.Empty;
		private List<ClientCameFrom> _clientCameFromPlaces;
		private List<string> _allOrganizationOwnershipTypesAbbreviations;
		private List<Domain.Organizations.Organization> _allOrganizations;
		private List<SalesChannelNode> _salesChannels;
		private bool _canOpenCloseDeliveries;

		private DelegateCommand _closeDeliveryCommand;
		private DelegateCommand _saveCloseCommentCommand;
		private DelegateCommand _editCloseCommentCommand;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public CloseSupplyToCounterpartyViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeService employeeService,
			IUserRepository userRepository,
			ICounterpartyFileStorageService counterpartyFileStorageService,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory)
			: base(uowBuilder, uowFactory, commonServices, navigationManager)
		{
			if(attachedFileInformationsViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			}

			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_counterpartyFileStorageService = counterpartyFileStorageService ?? throw new ArgumentNullException(nameof(counterpartyFileStorageService));
			CloseDeliveryComment = Entity.CloseDeliveryComment ?? string.Empty;
			_canOpenCloseDeliveries =
				_commonServices.CurrentPermissionService.ValidatePresetPermission("can_close_deliveries_for_counterparty");
				
			Title = $"Открытие/закрытие поставок {Entity.Name}";

			AttachedFileInformationsViewModel = attachedFileInformationsViewModelFactory.CreateAndInitialize<Counterparty, CounterpartyFileInformation>(
				UoW,
				Entity,
				_counterpartyFileStorageService,
				_cancellationTokenSource.Token,
				Entity.AddFileInformation,
				Entity.RemoveFileInformation);
		}

		#region Свойства

		public string CloseDeliveryComment
		{
			get => _closeDeliveryComment;
			set => SetField(ref _closeDeliveryComment, value);
		}

		public Employee CurrentEmployee =>
			_currentEmployee ?? (_currentEmployee = _employeeService.GetEmployeeForUser(UoW, _currentUserId));

		public string CloseDeliveryLabelInfo => Entity.IsDeliveriesClosed
					? $"Поставки закрыл : {Entity.GetCloseDeliveryInfo()} {Environment.NewLine}<b>Комментарий по закрытию поставок:</b>"
					: "<b>Комментарий по закрытию поставок:</b>";

		public List<ClientCameFrom> ClientCameFromPlaces =>
			_clientCameFromPlaces ?? (_clientCameFromPlaces = UoW.GetAll<ClientCameFrom>().ToList());

		public List<string> AllOrganizationOwnershipTypesAbbreviations =>
				_allOrganizationOwnershipTypesAbbreviations
				?? (_allOrganizationOwnershipTypesAbbreviations = UoW.GetAll<OrganizationOwnershipType>().Select(o => o.Abbreviation).ToList());

		public List<Domain.Organizations.Organization> AllOrganizations =>
				_allOrganizations ?? (_allOrganizations = UoW.GetAll<Domain.Organizations.Organization>().ToList());

		public List<SalesChannelNode> SalesChannels =>
			_salesChannels ?? (_salesChannels = GetSalesChannels());

		public bool CanManageCachReceipts => _commonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_cash_receipts");

		public bool CanSaveEntity => CanCloseDelivery
			&& Entity.RevenueStatus == RevenueStatus.Active
			&& ((!string.IsNullOrEmpty(Entity.CloseDeliveryComment) && Entity.IsDeliveriesClosed)
				|| (string.IsNullOrEmpty(Entity.CloseDeliveryComment) && !Entity.IsDeliveriesClosed));

		public IUserRepository UserRepository => _userRepository;

		#endregion

		private List<SalesChannelNode> GetSalesChannels()
		{
			var salesChannels = new List<SalesChannelNode>();
			if(Entity.IsForRetail)
			{
				SalesChannel salesChannelAlias = null;
				SalesChannelNode salesChannelSelectableNodeAlias = null;

				salesChannels = UoW.Session.QueryOver(() => salesChannelAlias)
					.SelectList(scList => scList
						.SelectGroup(() => salesChannelAlias.Id).WithAlias(() => salesChannelSelectableNodeAlias.Id)
						.Select(() => salesChannelAlias.Name).WithAlias(() => salesChannelSelectableNodeAlias.Name)
					).TransformUsing(Transformers.AliasToBean<SalesChannelNode>()).List<SalesChannelNode>().ToList();

				foreach(var selectableChannel in salesChannels.Where(x => Entity.SalesChannels.Any(sc => sc.Id == x.Id)))
				{
					selectableChannel.Selected = true;
				}
			}
			return salesChannels;
		}

		#region Commands

		#region CloseDelveryCommand
		public DelegateCommand CloseDeliveryCommand
		{
			get
			{
				if(_closeDeliveryCommand == null)
				{
					_closeDeliveryCommand = new DelegateCommand(CloseDelivery, () => CanCloseDelivery);
					_closeDeliveryCommand.CanExecuteChangedWith(this, x => x.CanCloseDelivery);
				}
				return _closeDeliveryCommand;
			}
		}

		public bool CanCloseDelivery => PermissionResult.CanUpdate && _canOpenCloseDeliveries;

		private void CloseDelivery()
		{
			if(CanCloseDelivery)
			{
				Entity.ToggleDeliveryOption(CurrentEmployee, _canOpenCloseDeliveries);
				CloseDeliveryComment = string.Empty;
				OnPropertyChanged(nameof(CloseDeliveryLabelInfo));
			}
		}
		#endregion CloseDelveryCommand

		#region SaveCloseComment
		public DelegateCommand SaveCloseCommentCommand
		{
			get
			{
				if(_saveCloseCommentCommand == null)
				{
					_saveCloseCommentCommand = new DelegateCommand(SaveCloseComment, () => CanSaveCloseComment);
					_saveCloseCommentCommand.CanExecuteChangedWith(this, x => x.CanSaveCloseComment);
				}
				return _saveCloseCommentCommand;
			}
		}

		public bool CanSaveCloseComment => CanCloseDelivery && string.IsNullOrWhiteSpace(Entity.CloseDeliveryComment);

		private void SaveCloseComment()
		{
			if(string.IsNullOrWhiteSpace(CloseDeliveryComment))
			{
				return;
			}

			if(!CanCloseDelivery)
			{
				_commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Warning, "У вас нет прав для изменения комментария по закрытию поставок");
				return;
			}

			if(CanSaveCloseComment)
			{
				Entity.AddCloseDeliveryComment(CloseDeliveryComment, CurrentEmployee);
				CloseDeliveryComment = Entity.CloseDeliveryComment;
			}
		}
		#endregion SaveCloseComment

		#region EditCloseComment
		public DelegateCommand EditCloseCommentCommand
		{
			get
			{
				if(_editCloseCommentCommand == null)
				{
					_editCloseCommentCommand = new DelegateCommand(EditCloseComment, () => CanEditCloseComment);
					_editCloseCommentCommand.CanExecuteChangedWith(this, x => x.CanEditCloseComment);
				}
				return _editCloseCommentCommand;
			}
		}

		public bool CanEditCloseComment => CanCloseDelivery && !string.IsNullOrWhiteSpace(Entity.CloseDeliveryComment);

		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel { get; }

		private void EditCloseComment()
		{
			if(!CanCloseDelivery)
			{
				_commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Warning, "У вас нет прав для изменения комментария по закрытию поставок");
				return;
			}

			if(_commonServices.InteractiveService.Question("Вы уверены что хотите изменить комментарий (преведущий комментарий будет удален)?"))
			{
				if(CanEditCloseComment)
				{
					Entity.CloseDeliveryComment = string.Empty;
					CloseDeliveryComment = string.Empty;
				}
			}
		}
		#endregion EditCloseComment

		#endregion Commands

		public override bool Save(bool needClose)
		{
			if(Entity.IsDeliveriesClosed && string.IsNullOrWhiteSpace(Entity.CloseDeliveryComment))
			{
				_commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Warning, "Необходимо заполнить комментарий по закрытию поставок");
				return false;
			}

			if(Entity.IsDeliveriesClosed && Entity.CloseDeliveryDebtType is null)
			{
				_commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Warning, "Необходимо выбрать тип задолженности");
				return false;
			}

			if(!CanCloseDelivery)
			{
				_commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Warning, "У вас нет прав для открытия/закрытия поставок");
				return false;
			}

			if(!needClose)
			{
				SaveUoW();
				return true;
			}

			if(!HasChanges)
			{
				Close(false, CloseSource.Save);
				return true;
			}

			SaveUoW();
			Close(false, CloseSource.Save);
			return true;
		}

		private void SaveUoW()
		{
			UoW.Save();
		}

		public override void Dispose()
		{
			UoW?.Dispose();
			base.Dispose();
		}
	}

	public class SalesChannelNode : PropertyChangedBase
	{

		private int _id;
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		private bool _selected;
		public virtual bool Selected
		{
			get => _selected;
			set => SetField(ref _selected, value);
		}

		private string _name;
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
	}
}
