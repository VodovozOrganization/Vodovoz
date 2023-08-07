using System;
using System.Linq;
using System.Text.RegularExpressions;
using QS.DomainModel.UoW;
using QS.ErrorReporting;
using QS.Project.Domain;
using QS.Services;

namespace Vodovoz.Tools
{
	public class DefaultErrorMessageModel : ErrorMessageModelBase
	{
		public DefaultErrorMessageModel(
			IErrorReporter errorReporter,
			IUserService userService,
			IUnitOfWorkFactory unitOfWorkFactory) : base(errorReporter)
		{
			this.userService = userService;
			this.unitOfWorkFactory = unitOfWorkFactory;
		}

		IUserService userService;
		IUnitOfWorkFactory unitOfWorkFactory;

		public override bool CanSendErrorReportManually => !String.IsNullOrWhiteSpace(Description);
		public override bool CanSendErrorReportAutomatically => errorReporter.AutomaticallySendEnabled;
		public override string CanSendManuallyText => "Для отправки необходимо ввести описание";
		public override bool IsEmailValid => new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$").IsMatch(Email ?? "");

		public override string ExceptionText =>
			string.Join("\n Следующее исключение:\n", Exceptions.Select(ex => ex.ToString()));

		public override string ErrorData =>
			$"Продукт: {errorReporter.ProductName}\n" +
			$"Версия: {errorReporter.Version}\n" +
			$"Редакция: {errorReporter.Edition}\n" +
			$"Ошибка: {string.Join("\n Следующее исключение:\n", Exceptions.Select(ex => ex.ToString()))}";

		public override void SendErrorReport()
		{
			if(ReportSent)
			{
				return;
			}

			if(!CanSendErrorReportManually && ErrorReportType != ReportType.Automatic)
			{
				return;
			}

			if(!CanSendErrorReportAutomatically && ErrorReportType == ReportType.Automatic)
			{
				return;
			}

			UserBase user = null;
			try {
				if(userService != null && unitOfWorkFactory != null) {
					using(IUnitOfWork uow = unitOfWorkFactory.CreateWithoutRoot()) {
						user = userService.GetCurrentUser();
					}
				}
			} catch(Exception ex) { 
				AddDescription($"Не удалось автоматически получить пользователя ({ex.Message})"); 
			}

			if(ErrorReportType == ReportType.Automatic)
			{
				ReportSent = errorReporter.AutomaticSendErrorReport(
					Description,
					Email,
					user,
					Exceptions.ToArray()
				);
			}
			else
			{
				ReportSent = errorReporter.ManuallySendErrorReport(
					Description,
					Email,
					user,
					Exceptions.ToArray()
				);
			}
			
		}
	}
}
