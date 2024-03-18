using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Application.Pacs;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
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
}
