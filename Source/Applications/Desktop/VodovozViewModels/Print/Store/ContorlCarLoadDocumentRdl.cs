﻿using QS.DomainModel.Entity;
using QS.Print;
using QS.Report;
using System;
using System.Collections.Generic;
using System.IO;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Extensions;
using Vodovoz.PrintableDocuments;
using Vodovoz.Reports.Editing;
using Vodovoz.Reports.Editing.Modifiers;

namespace Vodovoz.ViewModels.Print.Store
{
	public class ControlCarLoadDocumentRdl : PropertyChangedBase, ICustomPrintRdlDocument
	{
		public const string DocumentRdlPath = "Reports/Store/CarLoadDocumentControl.rdl";

		private readonly CarLoadDocument _carLoadDocument;
		private int _copiesToPrint;
		private string _printerName;

		private ControlCarLoadDocumentRdl(CarLoadDocument carLoadDocument)
		{
			_carLoadDocument = carLoadDocument ?? throw new ArgumentNullException(nameof(carLoadDocument));
		}

		public Dictionary<object, object> Parameters { get; set; }
		public PrinterType PrintType => PrinterType.RDL;
		public DocumentOrientation Orientation => DocumentOrientation.Portrait;

		public int CopiesToPrint
		{
			get => _copiesToPrint;
			set => SetField(ref _copiesToPrint, value);
		}

		public string PrinterName
		{
			get => _printerName;
			set => SetField(ref _printerName, value);
		}

		public string Name => DocumentType.GetEnumDisplayName();

		public CustomPrintDocumentType DocumentType => CustomPrintDocumentType.ControlCarLoadDocument;

		public ReportInfo GetReportInfo(string connectionString = null)
		{
			var source = GetReportSource();

			var reportInfo = new ReportInfo
			{
				Source = source,
				Title = Name,
				Parameters = new Dictionary<string, object> { { "id", _carLoadDocument.Id } },
				PrintType = ReportInfo.PrintingType.MultiplePrinters
			};

			return reportInfo;
		}

		private string GetReportSource()
		{
			return ModifyReport(DocumentRdlPath);
		}

		private string ModifyReport(string path)
		{
			var modifier = GetReportModifier();

			using(ReportController reportController = new ReportController(path))
			using(var reportStream = new MemoryStream())
			{
				reportController.AddModifier(modifier);
				reportController.Modify();
				reportController.Save(reportStream);

				using(var reader = new StreamReader(reportStream))
				{
					reportStream.Position = 0;
					var outputSource = reader.ReadToEnd();
					return outputSource;
				}
			}
		}

		private ReportModifierBase GetReportModifier()
		{
			var modifier = new ControlCarLoadDocumentModifier();

			modifier.Setup(_carLoadDocument.SeparateTableOrderIds, _carLoadDocument.IsDocumentHasCommonOrders);

			return modifier;
		}

		public static ControlCarLoadDocumentRdl Create(
			UserSettings userSettings,
			CarLoadDocument carLoadDocument)
		{
			var document = new ControlCarLoadDocumentRdl(carLoadDocument);

			var savedPrinterSettings = userSettings.GetPrinterSettingByDocumentType(document.DocumentType);

			document.CopiesToPrint = savedPrinterSettings is null ? 1 : savedPrinterSettings.NumberOfCopies;
			document.PrinterName = savedPrinterSettings?.PrinterName;

			return document;
		}
	}
}
