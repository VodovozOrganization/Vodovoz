using QS.Dialog;
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
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly CallModel _model;

		private IEnumerable<SubCall> _callDetails;
		private string _detailsInfo;

		public DashboardCallDetailsViewModel(IGuiDispatcher guiDispatcher, CallModel model)
		{
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_model = model ?? throw new ArgumentNullException(nameof(model));
			DetailsInfo = "Детализация звонка";

			GetDetails();
			_model.PropertyChanged += OnModelChanged;
		}

		private void OnModelChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(CallModel.Call):
					_guiDispatcher.RunInGuiTread(() => GetDetails());
					break;
				default:
					break;
			}
		}

		private void GetDetails()
		{
			CallDetails = _model.OperatorSubCalls;
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
