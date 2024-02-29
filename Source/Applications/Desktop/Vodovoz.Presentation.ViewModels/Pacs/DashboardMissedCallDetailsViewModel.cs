using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Application.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
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
