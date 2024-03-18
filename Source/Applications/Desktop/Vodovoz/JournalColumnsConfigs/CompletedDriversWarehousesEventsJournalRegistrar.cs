﻿using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class CompletedDriversWarehousesEventsJournalRegistrar :
		ColumnsConfigRegistrarBase<CompletedDriversWarehousesEventsJournalViewModel, CompletedDriversWarehousesEventsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<CompletedDriversWarehousesEventsJournalNode> config) =>
			config
				.AddColumn(CompletedDriversWarehousesEventsJournalNode.IdColumn)
					.AddNumericRenderer(x => x.Id)
				.AddColumn(CompletedDriversWarehousesEventsJournalNode.EventNameColumn)
					.AddTextRenderer(x => x.EventName)
				.AddColumn(CompletedDriversWarehousesEventsJournalNode.EventTypeColumn)
					.AddEnumRenderer(x => x.EventType).Editing(false)
				.AddColumn(CompletedDriversWarehousesEventsJournalNode.EmployeeColumn)
					.AddTextRenderer(x => x.DriverName)
				.AddColumn(CompletedDriversWarehousesEventsJournalNode.CarColumn)
					.AddTextRenderer(x => x.Car)
				.AddColumn(CompletedDriversWarehousesEventsJournalNode.CompletedDateColumn)
					.AddTextRenderer(x => x.CompletedDate.ToString())
				.AddColumn(CompletedDriversWarehousesEventsJournalNode.DistanceColumn)
					.AddNumericRenderer(x => x.DistanceMetersFromScanningLocation)
				.AddColumn("")
				.Finish();
	}
}
