using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Mango.Talks;

namespace Vodovoz.ViewModels.Mango
{
	public class InternalTalkViewModel : TalkViewModelBase
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly ITdiCompatibilityNavigation tdiCompatibilityNavigation;
		private readonly IInteractiveQuestion interactive;

		public InternalTalkViewModel(IUnitOfWorkFactory unitOfWorkFactory, 
			ITdiCompatibilityNavigation navigation, 
			IInteractiveQuestion interactive,
			MangoManager manager) : base(navigation,manager)
		{
			this.unitOfWorkFactory = unitOfWorkFactory;
			this.tdiCompatibilityNavigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
		}

		#region Действия View
		public string GetPhoneNumber()
		{
			return MangoManager.CallerNumber;
		}

		public string GetCallerName()
		{
			return MangoManager.CallerName;
		}
		#endregion

		#region CallEvents
		public void FinishCallCommand()
		{
			MangoManager.HangUp();
			Close(false, CloseSource.Self);
		}

		public void ForwardCallCommand()
		{
			Action action =  () => { Close(false, CloseSource.Self); };
			IPage page = NavigationManager.OpenViewModelNamedArgs<SubscriberSelectionViewModel>
			(this, new Dictionary<string, object>()
				{ {"manager",MangoManager },{"dialogType", SubscriberSelectionViewModel.DialogType.AdditionalCall },
				{"exitAction", action}
			});
		}

		#endregion

	}
}
