using System;
using QS.Commands;
using QS.Dialog;
using QS.ErrorReporting;
using QS.ViewModels;

namespace Vodovoz.Tools
{
	public class ErrorMessageViewModel : ViewModelBase
	{
		public ErrorMessageViewModel(ErrorMessageModelBase errorMessageModel, IInteractiveMessage interactiveMessage = null)
		{
			this.errorMessageModel = errorMessageModel ?? throw new ArgumentNullException(nameof(errorMessageModel));
			this.interactiveMessage = interactiveMessage;
			errorMessageModel.PropertyChanged += (sender, e) => {
				OnPropertyChanged(nameof(CanSendErrorReportManually));
				OnPropertyChanged(nameof(IsEmailValid));
			};
			CreateSendReportCommand();
		}

		protected ErrorMessageModelBase errorMessageModel;
		protected IInteractiveMessage interactiveMessage;

		public string ErrorData => errorMessageModel.ErrorData;
		public string ExceptionText => errorMessageModel.ExceptionText;
		public bool CanSendErrorReportManually => errorMessageModel.CanSendErrorReportManually;
		public string CanSendManuallyText => errorMessageModel.CanSendManuallyText;
		public bool IsEmailValid => errorMessageModel.IsEmailValid;
		public bool ReportSent { get; protected set; }

		private string desciption;
		public string Description {
			get => desciption;
			set { 
				if(SetField(ref desciption, value, () => Description))
					errorMessageModel.Description = desciption;
			 }
		}

		private string email;
		public string Email {
			get => email;
			set {
				if(SetField(ref email, value, () => Email))
					errorMessageModel.Email = email;
			}
		}

		public void AddException(Exception exception)
		{
			errorMessageModel.Exceptions.Add(exception);
		}

		public void SendReportAutomatically()
		{
			if(errorMessageModel.CanSendErrorReportAutomatically)
				SendReportCommand.Execute(ReportType.Automatic);
		}

		public DelegateCommand<ReportType> SendReportCommand;
		private void CreateSendReportCommand()
		{
			SendReportCommand = new DelegateCommand<ReportType>(
				(errorReportType) => {
					errorMessageModel.ErrorReportType = errorReportType;
					errorMessageModel.SendErrorReport();

					if(!errorMessageModel.ReportSent && errorReportType != ReportType.Automatic){
						interactiveMessage?.ShowMessage(ImportanceLevel.Warning, "Отправка сообщения не удалась.\n" +
							"Проверьте ваше интернет соединение и повторите попытку. " +
							"Если отправка неудастся возможно имеются проблемы на стороне сервера."
						);
					}
				},
				(errorReportType) => errorReportType == ReportType.Automatic || CanSendErrorReportManually
			);
		}
	}
}
