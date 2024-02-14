using Autofac;
using System;
using Vodovoz.Application.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsDashboardViewModelFactory : IPacsDashboardViewModelFactory
	{
		private ILifetimeScope _scope;

		public PacsDashboardViewModelFactory(ILifetimeScope scope)
		{
			_scope = scope;
		}

		public DashboardOperatorOnBreakViewModel CreateOperatorOnBreakViewModel(OperatorModel operatorModel)
		{
			var parameter = new TypedParameter(operatorModel.GetType(), operatorModel);
			return _scope.Resolve<DashboardOperatorOnBreakViewModel>(parameter);
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

		public DashboardOperatorDetailsViewModel CreateOperatorDetailsViewModel(OperatorModel operatorModel)
		{
			return _scope.Resolve<DashboardOperatorDetailsViewModel>(new TypedParameter(operatorModel.GetType(), operatorModel));
		}

		public DashboardCallDetailsViewModel CreateCallDetailsViewModel(CallModel callModel)
		{
			return _scope.Resolve<DashboardCallDetailsViewModel>(new TypedParameter(callModel.GetType(), callModel));
		}

		public DashboardMissedCallDetailsViewModel CreateMissedCallDetailsViewModel(MissedCallModel missedCallModel)
		{
			return _scope.Resolve<DashboardMissedCallDetailsViewModel>(new TypedParameter(missedCallModel.GetType(), missedCallModel));
		}
	}
}
