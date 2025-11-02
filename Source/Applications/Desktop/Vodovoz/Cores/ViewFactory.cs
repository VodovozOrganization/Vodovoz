using Gtk;
using QS.ViewModels;
using QS.Views.Resolve;
using System;

namespace Vodovoz.Core
{
	public class ViewFactory : IGtkViewFactory
	{
		public Widget Create(Type viewClass, ViewModelBase viewModel)
		{
			return (Widget)Activator.CreateInstance(viewClass, viewModel);
		}
	}
}
