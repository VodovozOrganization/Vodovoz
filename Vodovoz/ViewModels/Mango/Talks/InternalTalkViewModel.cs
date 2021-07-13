using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Mango.Talks
{
	public class InternalTalkViewModel : TalkViewModelBase
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly IUnitOfWork UoW;
		private readonly ITdiCompatibilityNavigation tdiCompatibilityNavigation;
		private readonly IInteractiveQuestion interactive;
		private readonly IOrderParametersProvider _orderParametersProvider;
		
		public InternalTalkViewModel(IUnitOfWorkFactory unitOfWorkFactory,
			ITdiCompatibilityNavigation navigation,
			IInteractiveQuestion interactive,
			MangoManager manager,
			IOrderParametersProvider orderParametersProvider) : base(navigation,manager)
		{
			this.unitOfWorkFactory = unitOfWorkFactory;
			this.tdiCompatibilityNavigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.UoW = unitOfWorkFactory.CreateWithoutRoot();
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
		}

		#region Свойства View

		public string OnLineText => MangoManager.CurrentTalk?.OnHoldText;
		public bool ShowTransferCaller => MangoManager.CurrentTalk?.IsTransfer ?? false;
		public bool ShowReturnButton => (MangoManager.CurrentTalk?.IsTransfer ?? false) && MangoManager.IsOutgoing;
		public bool ShowTransferButton => !MangoManager.CurrentTalk?.IsTransfer ?? true;

		#endregion

		#region Действия View

		public string GetCallerName()
		{
			return MangoManager.CallerName;
		}
		#endregion
	}
}
