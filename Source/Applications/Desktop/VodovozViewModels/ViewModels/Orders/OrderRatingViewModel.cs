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
using Vodovoz.Domain.Orders;
using Vodovoz.Factories;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OrderRatingViewModel : EntityDialogViewModelBase<OrderRating>, IAskSaveOnCloseViewModel
	{
		private readonly IPermissionResult _permissionResult;
		private readonly ICommonServices _commonServices;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly ValidationContext _validationContext;
		
		public OrderRatingViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			INavigationManager navigation,
			ICommonServices commonServices,
			IValidationContextFactory validationContextFactory,
			IGtkTabsOpener gtkTabsOpener)
			: base(uowBuilder, uowFactory, navigation)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_permissionResult = commonServices.CurrentPermissionService
				.ValidateEntityPermission(typeof(OrderRatingReason));

			_validationContext = validationContextFactory.CreateNewValidationContext(Entity);
			Title = Entity.ToString();
			
			CreateCommands();
			OrderRatingReasons = Entity.OrderRatingReasons;
		}
		
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
		public bool CanEdit => (Entity.Id == 0 && _permissionResult.CanCreate) || _permissionResult.CanUpdate;
		public DelegateCommand SaveAndCloseCommand { get; private set; }
		public DelegateCommand CloseCommand { get; private set; }
		public DelegateCommand OpenOrderCommand { get; private set; }
		public DelegateCommand OpenOnlineOrderCommand { get; private set; }
		public DelegateCommand ProcessCommand { get; private set; }
		public DelegateCommand CreateComplaintCommand { get; private set; }
		public IEnumerable<INamedDomainObject> OrderRatingReasons { get; }
		
		private bool OnlineOrderIsNotNull => Entity.OnlineOrder != null;
		private bool IsNewOrderRatingStatus => Entity.OrderRatingStatus == OrderRatingStatus.New;

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
				() => Entity.Process());
			ProcessCommand.CanExecuteChangedWith(this, x => x.IsNewOrderRatingStatus);
		}

		private void CreateCreateComplaintCommand()
		{
			CreateComplaintCommand = new DelegateCommand(
				() => NavigationManager.OpenViewModel<CreateComplaintViewModel, IEntityUoWBuilder>(
					this, EntityUoWBuilder.ForCreate()));
		}
		
		protected override bool Validate() => _commonServices.ValidationService.Validate(Entity, _validationContext);
	}
}
