using System;
using Vodovoz.Infrastructure.Services;
using QS.DomainModel.UoW;

namespace Vodovoz.Infrastructure.ViewModels
{
	public class UoWWidgetViewModelBase : WidgetViewModelBase
	{
		public IUnitOfWork UoW { get; set; }

		public UoWWidgetViewModelBase(IInteractiveService interactiveService) : base(interactiveService)
		{
		}
	}
}
