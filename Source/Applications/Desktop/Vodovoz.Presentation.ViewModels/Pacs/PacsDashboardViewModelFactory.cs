using Autofac;
using System;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsDashboardViewModelFactory : IPacsDashboardViewModelFactory
	{
		private readonly ILifetimeScope _scope;

		public PacsDashboardViewModelFactory(ILifetimeScope scope)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
		}

		public DashboardOperatorOnBreakViewModel CreateOperatorOnBreakViewModel(OperatorModel operatorModel)
		{
			return _scope.Resolve<DashboardOperatorOnBreakViewModel>(new TypedParameter(operatorModel.GetType(), operatorModel));
		}

		public DashboardOperatorViewModel CreateOperatorViewModel(OperatorModel operatorModel)
		{
			return _scope.Resolve<DashboardOperatorViewModel>(new TypedParameter(operatorModel.GetType(), operatorModel));
		}

		public DashboardMissedCallViewModel CreateMissedCallViewModel(MissedCallModel missedCallModel)
		{
			return _scope.Resolve<DashboardMissedCallViewModel>(new TypedParameter(missedCallModel.GetType(), missedCallModel));
		}

		public DashboardCallViewModel CreateCallViewModel(CallModel callModel)
		{
			return _scope.Resolve<DashboardCallViewModel>(new TypedParameter(callModel.GetType(), callModel));
		}
	}
}
