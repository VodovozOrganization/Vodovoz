using System.Collections.Generic;

namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	public partial class CarIsNotAtLineReport
	{
		public class UiRow : Row
		{
			private UiRow(Row row)
			{
				Id = row.Id;
				IdString = row.Id.ToString();
				DowntimeStartedAt = row.DowntimeStartedAt;
				DowntimeStartedAtString = row.DowntimeStartedAtString;
				CarType = row.CarType;
				CarOwnType = row.CarOwnType;
				CarTypeWithGeographicalGroup = row.CarTypeWithGeographicalGroup;
				RegistationNumber = row.RegistationNumber;
				TimeAndBreakdownReason = row.TimeAndBreakdownReason;
				AreasOfResponsibility = row.AreasOfResponsibility;
				PlannedReturnToLineDate = row.PlannedReturnToLineDate;
				PlannedReturnToLineDateAndReschedulingReason = row.PlannedReturnToLineDateAndReschedulingReason;
				CarEventTypes = row.CarEventTypes;
				RowType = UiRowType.Main;
			}

			private UiRow(CarReceptionRow row)
			{
				Id = row.Id;
				IdString = row.Id.ToString();
				DowntimeStartedAt = row.RecievedAt;
				DowntimeStartedAtString = row.RecievedAt.ToString(_defaultDateTimeFormat);
				CarTypeWithGeographicalGroup = row.CarTypeWithGeographicalGroup;
				CarOwnType = row.CarOwnType;
				RegistationNumber = row.RegistationNumber;
				TimeAndBreakdownReason = row.Comment;
				RowType = UiRowType.CarReception;
			}

			private UiRow(CarTransferRow row)
			{
				Id = row.Id;
				IdString = row.Id.ToString();
				DowntimeStartedAt = row.TransferedAt;
				DowntimeStartedAtString = row.TransferedAt.ToString(_defaultDateTimeFormat);
				CarTypeWithGeographicalGroup = row.CarTypeWithGeographicalGroup;
				CarOwnType = row.CarOwnType;
				RegistationNumber = row.RegistationNumber;
				TimeAndBreakdownReason = row.Comment;
				RowType = UiRowType.CarTransfer;
			}

			private UiRow() { }

			/// <summary>
			/// Идентификатор строки
			/// </summary>
			public new string IdString { get; set; }

			/// <summary>
			/// Дата начала простоя или комментарий (в зависимости от типа строки)
			/// </summary>
			public new string DowntimeStartedAtString { get; set; }

			/// <summary>
			/// Тип строки отчета в UI
			/// </summary>
			public UiRowType RowType { get; set; }

			public bool IsMainRow => RowType == UiRowType.Main;
			public bool IsCatTransferRow => RowType == UiRowType.CarTransfer;
			public bool IsCarReceptionRow => RowType == UiRowType.CarReception;
			public bool IsSubtableNameRow => RowType == UiRowType.SubtableName;
			public bool IsSubtableHeadereRow => RowType == UiRowType.SubtableHeader;

			private static UiRow CreateSubrtableNameRow(string subtableName) => new UiRow
			{
				IdString = subtableName,
				RowType = UiRowType.SubtableName
			};

			private static UiRow CreateSubtableHeaderRow() => new UiRow
			{
				IdString = "№ п/п",
				DowntimeStartedAtString = "Дата",
				CarTypeWithGeographicalGroup = "Тип авто",
				CarOwnType = "П",
				RegistationNumber = "Госномер",
				TimeAndBreakdownReason = "Комментарий",
				RowType = UiRowType.SubtableHeader
			};
			
			private static UiRow CreateEmptyRow() => new UiRow
			{
				RowType = UiRowType.Empty
			};

			private static UiRow CreateSummaryRow(string key, string value) => new UiRow
			{
				IdString = key,
				DowntimeStartedAtString = value,
				RowType = UiRowType.Summary
			};

			public static IList<UiRow> CreateUiRows(
				IEnumerable<Row> rows,
				IEnumerable<CarTransferRow> carTransferRows,
				IEnumerable<CarReceptionRow> carReceptionRows,
				string eventsSummary,
				string eventsSummaryDetails)
			{
				var uiRows = new List<UiRow>();
				uiRows.AddRange(AddRows(rows));
				uiRows.AddRange(AddSummaryRows(eventsSummary, eventsSummaryDetails));
				uiRows.Add(UiRow.CreateEmptyRow());
				uiRows.AddRange(AddTransferRows(carTransferRows));
				uiRows.Add(UiRow.CreateEmptyRow());
				uiRows.Add(UiRow.CreateEmptyRow());
				uiRows.AddRange(AddReceptionRows(carReceptionRows));

				return uiRows;
			}

			private static IEnumerable<UiRow> AddRows(IEnumerable<Row> rows)
			{
				var uiRows = new List<UiRow>();

				foreach(var row in rows)
				{
					uiRows.Add(new UiRow(row));
				}

				return uiRows;
			}

			private static IEnumerable<UiRow> AddSummaryRows(string eventsSummary, string eventsSummaryDetails)
			{
				var uiRows = new List<UiRow>
				{
					UiRow.CreateSummaryRow("Итог", eventsSummary),
					UiRow.CreateSummaryRow("Детализация\nпростоя", eventsSummaryDetails)
				};

				return uiRows;
			}

			private static IEnumerable<UiRow> AddTransferRows(IEnumerable<CarTransferRow> carTransferRows)
			{
				var uiRows = new List<UiRow>
				{
					UiRow.CreateSubrtableNameRow("Передача"),
					UiRow.CreateSubtableHeaderRow()
				};

				foreach(var row in carTransferRows)
				{
					uiRows.Add(new UiRow(row));
				}

				return uiRows;
			}

			private static IEnumerable<UiRow> AddReceptionRows(IEnumerable<CarReceptionRow> carReceptionRows)
			{
				var uiRows = new List<UiRow>
				{
					UiRow.CreateSubrtableNameRow("Прием"),
					UiRow.CreateSubtableHeaderRow()
				};

				foreach(var row in carReceptionRows)
				{
					uiRows.Add(new UiRow(row));
				}

				return uiRows;
			}
		}
	}
}
