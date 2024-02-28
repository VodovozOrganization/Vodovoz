using Autofac;
using System;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsViewModelFactory : IPacsViewModelFactory
	{
		private readonly ILifetimeScope _scope;

		public PacsViewModelFactory(ILifetimeScope scope)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
		}

		public PacsSettingsViewModel CreateSettingsViewModel()
		{
			return _scope.Resolve<PacsSettingsViewModel>();
		}

		public PacsDashboardViewModel CreateDashboardViewModel()
		{
			return _scope.Resolve<PacsDashboardViewModel>();
		}

		public PacsReportsViewModel CreateReportsViewModel()
		{
			return _scope.Resolve<PacsReportsViewModel>();
		}

		public PacsOperatorViewModel CreateOperatorViewModel()
		{
			return _scope.Resolve<PacsOperatorViewModel>();
		}
	}
}
