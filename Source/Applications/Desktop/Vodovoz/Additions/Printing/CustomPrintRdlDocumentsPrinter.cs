using fyiReporting.RDL;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.Domain;
using QS.Report.Repository;
using QS.Services;
using System;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Vodovoz.PrintableDocuments;
using Vodovoz.ViewModels.Infrastructure.Print;
using static QS.Report.Domain.UserPrintSettings;

namespace Vodovoz.Additions.Printing
{
	public class CustomPrintRdlDocumentsPrinter : ICustomPrintRdlDocumentsPrinter
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly IUserPrintingRepository _userPrintingRepository;
		private readonly IUserService _userService;

		public CustomPrintRdlDocumentsPrinter(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IUserPrintingRepository userPrintingRepository,
			IUserService userService)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_userPrintingRepository = userPrintingRepository ?? throw new ArgumentNullException(nameof(userPrintingRepository));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		public event EventHandler DocumentsPrinted;
		public event EventHandler PrintingCanceled;

		public void Print(ICustomPrintRdlDocument document)
		{
			if(document is null)
			{
				throw new ArgumentNullException(nameof(document));
			}

			var pages = GetReportPages(document.GetReportInfo());

			var multiplePrintOperation = new MultiplePrintOperation(_unitOfWorkFactory, _commonServices, _userPrintingRepository);

			if(IsNeedToSelectPrinterForDocument(document))
			{
				multiplePrintOperation.Run(pages);
			}
			else
			{
				var userPrintSettings = new UserPrintSettings
				{
					User = _userService.GetCurrentUser(),
					NumberOfCopies = (uint)document.CopiesToPrint,
					PageOrientation = (PageOrientationType)Enum.Parse(typeof(PageOrientationType), document.Orientation.ToString())
				};

				var isWindowOs = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

				multiplePrintOperation.Run(pages, document.PrinterName, userPrintSettings, isWindowOs);
			}
		}

		private bool IsNeedToSelectPrinterForDocument(ICustomPrintRdlDocument document)
		{
			if(string.IsNullOrWhiteSpace(document.PrinterName) || document.CopiesToPrint < 1)
			{
				return true;
			}

			var installedPrinters = PrinterSettings.InstalledPrinters.Cast<string>().ToList();

			if(!installedPrinters.Contains(document.PrinterName))
			{
				return true;
			}
			
			return !_commonServices.InteractiveService.Question(
				$"Документ {document.Name} будет распечатан на принтере {document.PrinterName}.\nПродолжить?");
		}

		private Pages GetReportPages(ReportInfo reportInfo)
		{
			var reportPath = reportInfo.GetPath();
			var source = reportInfo.Source ?? File.ReadAllText(reportPath);

			var rdlParser = new RDLParser(source)
			{
				Folder = Path.GetDirectoryName(reportPath),
				OverwriteConnectionString = reportInfo.ConnectionString,
				OverwriteInSubreport = true
			};

			var report = rdlParser.Parse();
			report.RunGetData(reportInfo.Parameters);
			var pages = report.BuildPages();

			return pages;
		}
	}
}
