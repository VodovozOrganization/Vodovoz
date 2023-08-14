using NSubstitute;
using NUnit.Framework;
using QS.ErrorReporting;
using QS.Project.Versioning;
using System;
using Vodovoz.Tools;

namespace VodovozBusinessTests.ErrorReporting
{
	[TestFixture(TestOf = typeof(ErrorReporter))]
	public class SingletonErrorReporterTest
	{
		[Test(Description = "Проверяем что действительно не отправим автоматический отчет, если автоматическая отправка отключена в настройках.")]
		public void SendErrorReport_DisableSendAutomatically()
		{
			var sendService = Substitute.For<IErrorReportSender>();
			var appInfo = Substitute.For<IApplicationInfo>();
			appInfo.ProductName.Returns("Test");
			appInfo.Version.Returns(new Version("1.1.1.1"));
			var logService = Substitute.For<ILogService>();
			logService.GetLog(Arg.Any<int>()).Returns("");

			ErrorReporter errorReporter = new ErrorReporter(sendService, appInfo, logService);
			errorReporter.DatabaseName = "DBName";
			errorReporter.AutomaticallySendEnabled = false;
			errorReporter.SendedLogRowCount = 0;

			errorReporter.AutomaticSendErrorReport("", "", null, new Exception());
			sendService.DidNotReceive().SubmitErrorReport(Arg.Any<SubmitErrorRequest>());

			//Для теста самого теста проверяем что при тех же настройках но разрешенной отправки отчет всетаки отправится.
			//Чтобы исключить возможность что он не отправляется по другой причине.
			errorReporter.AutomaticallySendEnabled = true;
			errorReporter.AutomaticSendErrorReport("", "", null, new Exception());
			sendService.Received().SubmitErrorReport(Arg.Any<SubmitErrorRequest>());
		}
	}
}
