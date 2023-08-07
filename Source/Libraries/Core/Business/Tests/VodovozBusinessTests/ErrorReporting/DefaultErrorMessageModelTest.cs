using System;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.ErrorReporting;
using QS.Project.Domain;
using Vodovoz.Tools;

namespace VodovozBusinessTests.ErrorReporting
{
	[TestFixture(TestOf = typeof(DefaultErrorMessageModel))]
	public class DefaultErrorMessageModelTest
	{
		[Test(Description = "Проверяем что действительно не отправим автоматический отчет, если уже был отправлен ручной.")]
		public void SendErrorReport_DontSendAutomaticallyAfterManual()
		{
			var interactive = Substitute.For<IInteractiveMessage>();
			interactive.ShowMessage(Arg.Any<ImportanceLevel>(), Arg.Any<string>());

			var reporter = Substitute.For<Vodovoz.Tools.IErrorReporter>();
			reporter.AutomaticallySendEnabled.Returns(true);
			reporter.AutomaticSendErrorReport(
				Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UserBase>(), Arg.Any<Exception[]>())
				.Returns(true);
			reporter.ManuallySendErrorReport(
				Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UserBase>(), Arg.Any<Exception[]>())
				.Returns(true);
			var model = new DefaultErrorMessageModel(reporter, null, null);


			model.Description = "Test";
			model.ErrorReportType = ReportType.User;
			model.SendErrorReport();

			model.ErrorReportType = ReportType.Automatic;
			model.SendErrorReport();
			reporter.DidNotReceive().AutomaticSendErrorReport(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UserBase>(), Arg.Any<Exception[]>());

		}
	}
}

