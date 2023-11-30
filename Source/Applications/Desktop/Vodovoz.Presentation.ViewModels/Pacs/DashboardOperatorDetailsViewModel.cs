using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class DashboardOperatorDetailsViewModel : WidgetViewModelBase
	{
		private readonly OperatorModel _model;

		private string _tittle;

		public DashboardOperatorDetailsViewModel(OperatorModel model)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			Tittle = $"История оператора: {_model.Employee.GetPersonNameWithInitials()}";
		}

		public virtual string Tittle
		{
			get => _tittle;
			set => SetField(ref _tittle, value);
		}

		public GenericObservableList<OperatorState> States => _model.States;

	}

	public class DashboardCallDetailsViewModel : WidgetViewModelBase
	{
		private readonly CallModel _model;

		private string _detailsInfo;

		public DashboardCallDetailsViewModel(CallModel model)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			DetailsInfo = "Детализация звонка";
		}

		public virtual string DetailsInfo
		{
			get => _detailsInfo;
			set => SetField(ref _detailsInfo, value);
		}

		public GenericObservableList<CallEvent> CallEvents => _model.CallEvents;
	}

	public class DashboardMissedCallDetailsViewModel : WidgetViewModelBase
	{
		private readonly MissedCallModel _model;

		private string _details;

		public DashboardMissedCallDetailsViewModel(MissedCallModel model)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));

			Details =
$@"
Пропущенный звонок: {_model.Call.CurrentState.FromNumber}
Начат: {_model.Call.Started.ToString("MM.dd HH:mm")}
Завершен: {_model.Call.Ended.ToString("MM.dd HH:mm")}
Ожидание: {_model.Call.Duration.ToString("hh\\:mm\\:ss")}
Могли принять {_model.PossibleOperatorsCount} операторов:
{string.Join("\n", _model.PossibleOperators.Select(x => $"{x.Employee.GetPersonNameWithInitials()}. Тел. {x.CurrentState.PhoneNumber}"))}
";
		}

		public virtual string Details
		{
			get => _details;
			set => SetField(ref _details, value);
		}
	}
}
