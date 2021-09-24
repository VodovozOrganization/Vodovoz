using System;
using NSubstitute;
using NUnit.Framework;
using QS.ErrorReporting;
using QS.Project.Versioning;
using Vodovoz.Tools;

namespace VodovozBusinessTests.ErrorReporting
{
	[TestFixture(TestOf = typeof(SingletonErrorReporter))]
	public class SingletonErrorReporterTest
	{
		[Test(Description = "Проверяем что действительно не отправим автоматический отчет, если автоматическая отправка отключена в настройках.")]
		public void SendErrorReport_DisableSendAutomatically()
		{
			var sendService = Substitute.For<IErrorReportingService>();
			var appInfo = Substitute.For<IApplicationInfo>();
			appInfo.ProductName.Returns("Test");

			var assembly = System.Reflection.Assembly.GetAssembly(typeof(SingletonErrorReporter));
			var name = assembly.GetName();
			var version = name.Version;
			appInfo.Version.Returns(version);

			SingletonErrorReporter.Initialize(sendService, appInfo, null, "DBName", false, null);
			SingletonErrorReporter.Instance.SendErrorReport(new Exception[] { }, ErrorReportType.Automatic, null, null , null);
			sendService.DidNotReceive().SubmitErrorReport(Arg.Any<ErrorReport>());

			//Для теста самого теста проверяем что при тех же настройках но разрешенной отправки отчет всетаки отправится.
			//Чтобы исключить возможность что он не отправляется по другой причине.
			SingletonErrorReporter.Initialize(sendService, appInfo, null, "DBName", true, null);
			SingletonErrorReporter.Instance.SendErrorReport(new Exception[] { }, ErrorReportType.Automatic, null, null, null);
			sendService.Received().SubmitErrorReport(Arg.Any<ErrorReport>());
		}
	}
}
