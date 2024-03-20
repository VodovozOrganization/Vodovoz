using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Vodovoz.Application.Pacs;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class DashboardCallDetailsViewModel : WidgetViewModelBase
	{
		private readonly CallModel _model;

		private IEnumerable<SubCall> _callDetails;
		private string _detailsInfo;

		public DashboardCallDetailsViewModel(CallModel model)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			DetailsInfo = "Детализация звонка";

			_model.PropertyChanged += OnModelChanged;
		}

		private void OnModelChanged(object sender, PropertyChangedEventArgs e)
		{
			CallDetails = _model.Call.SubCalls;
		}

		public virtual string DetailsInfo
		{
			get => _detailsInfo;
			set => SetField(ref _detailsInfo, value);
		}

		public IEnumerable<SubCall> CallDetails
		{
			get => _callDetails;
			set => SetField(ref _callDetails,value);
		}
	}
}
