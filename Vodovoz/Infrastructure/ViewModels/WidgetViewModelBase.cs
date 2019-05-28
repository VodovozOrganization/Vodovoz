using System;
using Vodovoz.Infrastructure.Services;
namespace Vodovoz.Infrastructure.ViewModels
{
	public abstract class WidgetViewModelBase : ViewModelBase
	{
		public WidgetViewModelBase(IInteractiveService interactiveService) : base(interactiveService)
		{
		}
	}
}
