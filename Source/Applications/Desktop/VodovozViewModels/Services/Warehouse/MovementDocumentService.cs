using ClosedXML.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Organizations;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Nodes.TTN;

namespace Vodovoz.ViewModels.Services.Warehouse
{
	public class MovementDocumentService : IMovementDocumentService
	{
		private const string _ttnTemplatePath = @".\Reports\Warehouse\TTN.xlsx";
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public MovementDocumentService(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public TtnReport BuildTtnReport(MovementDocument entity, Employee currentEmployee)
		{
			if(entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			var docDate = entity.TimeStamp != default ? entity.TimeStamp : DateTime.Now;

			OrganizationVersion senderVersion;
			OrganizationVersion receiverVersion;
			OrganizationVersion payerVersion;

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				senderVersion = GetActiveOrganizationVersion(uow, entity.TtnCargoSender?.Id, docDate);
				receiverVersion = GetActiveOrganizationVersion(uow, entity.TtnCargoReceiver?.Id, docDate);
				payerVersion = GetActiveOrganizationVersion(uow, entity.TtnPayer?.Id, docDate);
			}

			var report = new TtnReport
			{
				DocNumber = entity.Id != 0 ? entity.Id.ToString() : "",
				DocDay = docDate.ToString("dd"),
				DocMonth = docDate.ToString("MM"),
				DocYear = docDate.ToString("yyyy"),

				CargoSenderText = BuildOrganizationText(entity.TtnCargoSender, senderVersion),
				CargoReceiverText = BuildOrganizationText(entity.TtnCargoReceiver, receiverVersion),
				PayerText = BuildOrganizationText(entity.TtnPayer, payerVersion),

				Rows = BuildTtnRows(entity),

				ReleaseAllowedPosition = currentEmployee?.Post?.Name ?? "",
				ReleaseAllowedName = ShortName(currentEmployee?.FullName),
				ReleaseProducedPosition = currentEmployee?.Post?.Name ?? "",
				ReleaseProducedName = ShortName(currentEmployee?.FullName),
				CargoAcceptedPosition = "водитель",
				CargoAcceptedName = ShortName(entity.TtnDriver?.FullName),

				DeliveryDay = docDate.ToString("dd"),
				DeliveryMonthText = MonthToRussianText(docDate.Month),
				DeliveryYear = docDate.ToString("yyyy"),
				OrganizationText = BuildOrganizationText(entity.TtnCargoSender, senderVersion),
				CarModel = entity.MovementWagon?.Name ?? "",
				CarRegistrationNumber = entity.MovementWagon?.Name ?? "",
				DriverFullName = entity.TtnDriver?.FullName ?? "",
				DriverLicenseNumber = entity.TtnDriver?.DrivingLicense ?? "",
				LoadingPointAddress = entity.FromWarehouse?.Address ?? "",
				UnloadingPointAddress = entity.ToWarehouse?.Address ?? "",
				TrailerModel = entity.TtnSemitrailer?.CarModel?.Name ?? "",
				TrailerRegistrationNumber = entity.TtnSemitrailer?.RegistrationNumber ?? "",
			};

			report.MassBruttoValue = report.Rows.Sum(r => r.Weight);
			report.MassBruttoText = NumberToWords.ToWords((int)report.MassBruttoValue) + " кг";

			return report;
		}

		public void SaveTtnReport(TtnReport report, string outputPath)
		{
			if(report is null)
			{
				throw new ArgumentNullException(nameof(report));
			}

			if(string.IsNullOrWhiteSpace(outputPath))
			{
				throw new ArgumentException("Путь сохранения не задан.", nameof(outputPath));
			}

			var templateFile = new FileInfo(_ttnTemplatePath);

			ExcelPackage.License.SetNonCommercialPersonal("Имя");

			using(var package = new ExcelPackage(templateFile))
			{
				var ws1 = package.Workbook.Worksheets["Товарный раздел"];
				const int firstItemRow = 18;
				var rowsCount = report.Rows.Count;

				ws1.Cells["FM6"].Value = report.DocNumber;
				ws1.Cells["FM7"].Value = report.DocDay;
				ws1.Cells["FS7"].Value = report.DocMonth;
				ws1.Cells["FZ7"].Value = report.DocYear;

				ws1.Cells["V9"].Value = report.CargoSenderText;
				ws1.Cells["V11"].Value = report.CargoReceiverText;
				ws1.Cells["V13"].Value = report.PayerText;

				if(rowsCount > 1)
				{
					ws1.InsertRow(firstItemRow + 1, rowsCount - 1);

					for(int i = 1; i < rowsCount; i++)
					{
						ws1.Cells[firstItemRow, 1, firstItemRow, ws1.Dimension.End.Column]
							.Copy(ws1.Cells[firstItemRow + i, 1]);

						ws1.Row(firstItemRow + i).Height = ws1.Row(firstItemRow).Height;
					}
				}

				var row = firstItemRow;
				foreach(var item in report.Rows)
				{
					ws1.Cells[$"A{row}"].Value = item.Code;
					ws1.Cells[$"AS{row}"].Value = item.Count;
					ws1.Cells[$"BU{row}"].Value = item.Name;
					ws1.Cells[$"EO{row}"].Value = item.Weight;
					ws1.Cells[$"FC{row}"].Value = item.Sum;
					row++;
				}

				var shift = rowsCount - 1;

				var totalCount = report.Rows.Sum(r => (int)r.Count);
				var totalSum = report.Rows.Sum(r => r.Sum);

				ws1.Cells[$"AS{19 + shift}"].Value = totalCount;
				ws1.Cells[$"FC{19 + shift}"].Value = totalSum;
				ws1.Cells[$"AS{20 + shift}"].Value = totalCount;
				ws1.Cells[$"FC{20 + shift}"].Value = totalSum;

				ws1.Cells[$"EO{19 + shift}"].Value = report.MassBruttoValue;
				ws1.Cells[$"EO{20 + shift}"].Value = report.MassBruttoValue;

				ws1.Cells[$"DS{25 + shift}"].Value = report.MassBruttoValue;
				ws1.Cells[$"CR{26 + shift}"].Value = report.MassBruttoText;

				ws1.Cells[$"A{34 + shift}"].Value = report.ReleaseAllowedPosition;
				ws1.Cells[$"AG{34 + shift}"].Value = report.ReleaseAllowedName;
				ws1.Cells[$"AB{37 + shift}"].Value = report.ReleaseProducedPosition;
				ws1.Cells[$"BR{37 + shift}"].Value = report.ReleaseProducedName;
				ws1.Cells[$"EB{31 + shift}"].Value = report.CargoAcceptedPosition;
				ws1.Cells[$"FI{31 + shift}"].Value = report.CargoAcceptedName;

				var ws2 = package.Workbook.Worksheets["Транспортный раздел"];

				const int firstCargoRow = 23;
				var cargoRowsCount = report.Rows.Count;
				const int cargoRowsInTemplate = 3;
				const int cargoStyleSourceRow = 24;

				if(cargoRowsCount > cargoRowsInTemplate)
				{
					var insertCount = cargoRowsCount - cargoRowsInTemplate;
					ws2.InsertRow(firstCargoRow + cargoRowsInTemplate, insertCount);

					for(int i = 0; i < insertCount; i++)
					{
						var targetRow = firstCargoRow + cargoRowsInTemplate + i;

						ws2.Cells[cargoStyleSourceRow, 1, cargoStyleSourceRow, ws2.Dimension.End.Column]
							.Copy(ws2.Cells[targetRow, 1]);

						ws2.Row(targetRow).Height = ws2.Row(cargoStyleSourceRow).Height;
					}
				}

				var cargoRow = firstCargoRow;
				for(int i = 0; i < report.Rows.Count; i++)
				{
					ws2.Cells[$"A{cargoRow}"].Value = i + 1;
					ws2.Cells[$"D{cargoRow}"].Value = report.Rows[i].Name;
					ws2.Cells[$"EJ{cargoRow}"].Value = report.Rows[i].Code;
					ws2.Cells[$"FU{cargoRow}"].Value = report.Rows[i].Weight;
					cargoRow++;
				}

				var cargoShift = Math.Max(0, report.Rows.Count - cargoRowsInTemplate);

				ws2.Cells["FP2"].Value = report.DocNumber;
				ws2.Cells["X3"].Value = report.DeliveryDay;
				ws2.Cells["AD3"].Value = report.DeliveryMonthText;
				ws2.Cells["AW3"].Value = report.DeliveryYear;
				ws2.Cells["N4"].Value = report.OrganizationText;
				ws2.Cells["X7"].Value = report.PayerText;
				ws2.Cells["L9"].Value = report.DriverFullName;
				ws2.Cells["CU9"].Value = report.DriverLicenseNumber;
				ws2.Cells["CO4"].Value = report.CarModel;
				ws2.Cells["EL4"].Value = report.CarRegistrationNumber;
				ws2.Cells["Q14"].Value = report.LoadingPointAddress;
				ws2.Cells["CT14"].Value = report.UnloadingPointAddress;
				ws2.Cells["CH16"].Value = report.TrailerModel;
				ws2.Cells["EF16"].Value = report.TrailerRegistrationNumber;
				ws2.Cells[$"FU{26 + cargoShift}"].Value = report.MassBruttoValue;
				ws2.Cells[$"I{31 + cargoShift}"].Value = report.ReleaseAllowedPosition;
				ws2.Cells[$"AH{31 + cargoShift}"].Value = report.ReleaseAllowedName;
				ws2.Cells[$"AA{36 + cargoShift}"].Value = report.CargoAcceptedName;
				ws2.Cells[$"DC{31 + cargoShift}"].Value = report.CargoAcceptedName;
				ws2.Cells[$"CF{29 + cargoShift}"].Value = report.MassBruttoText;

				package.SaveAs(new FileInfo(outputPath));
			}
		}

		private static OrganizationVersion GetActiveOrganizationVersion(IUnitOfWork uow, int? organizationId, DateTime date)
		{
			if(organizationId == null)
			{
				return null;
			}

			return uow.Session.QueryOver<OrganizationVersion>()
				.Where(v => v.Organization.Id == organizationId.Value)
				.Where(v => v.StartDate <= date && (v.EndDate == null || v.EndDate >= date))
				.OrderBy(v => v.StartDate).Desc
				.Take(1)
				.SingleOrDefault();
		}

		private static string BuildOrganizationText(Organization org, OrganizationVersion version)
		{
			if(org == null)
			{
				return string.Empty;
			}

			var parts = new List<string>();

			if(!string.IsNullOrWhiteSpace(org.FullName))
			{
				parts.Add(org.FullName);
			}
			else if(!string.IsNullOrWhiteSpace(org.Name))
			{
				parts.Add(org.Name);
			}

			var address = version?.JurAddress;
			if(!string.IsNullOrWhiteSpace(address))
			{
				parts.Add(address);
			}

			if(!string.IsNullOrWhiteSpace(org.INN))
			{
				parts.Add($"ИНН {org.INN}");
			}

			if(!string.IsNullOrWhiteSpace(org.KPP))
			{
				parts.Add($"КПП {org.KPP}");
			}

			return string.Join(", ", parts);
		}

		private static string MonthToRussianText(int month)
		{
			switch(month)
			{
				case 1: return "январь";
				case 2: return "февраль";
				case 3: return "март";
				case 4: return "апрель";
				case 5: return "май";
				case 6: return "июнь";
				case 7: return "июль";
				case 8: return "август";
				case 9: return "сентябрь";
				case 10: return "октябрь";
				case 11: return "ноябрь";
				case 12: return "декабрь";
				default: return "";
			}
		}

		private static IList<TtnReportRow> BuildTtnRows(MovementDocument entity)
		{
			var rows = new List<TtnReportRow>();

			foreach(var item in entity.Items)
			{
				rows.Add(new TtnReportRow
				{
					Code = item.Nomenclature?.Id.ToString() ?? "",
					Name = item.Nomenclature?.OfficialName ?? "",
					Count = item.SentAmount,
					Weight = item.Nomenclature?.Weight ?? 0m,
					Sum = 0,
				});
			}

			return rows;
		}

		private static string ShortName(string fullName)
		{
			if(string.IsNullOrWhiteSpace(fullName))
			{
				return string.Empty;
			}

			var parts = fullName
				.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if(parts.Length == 1)
			{
				return parts[0];
			}

			var lastName = parts[0];
			var initials = new List<string>();

			for(int i = 1; i < parts.Length; i++)
			{
				var part = parts[i];
				if(!string.IsNullOrWhiteSpace(part))
				{
					initials.Add(part[0] + ".");
				}
			}

			return initials.Count > 0
				? $"{lastName} {string.Join(" ", initials)}"
				: lastName;
		}
	}
}
