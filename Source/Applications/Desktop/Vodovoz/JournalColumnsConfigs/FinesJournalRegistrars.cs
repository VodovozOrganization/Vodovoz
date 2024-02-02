﻿using Gamma.ColumnConfig;
using System.Globalization;
using Pango;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Employees;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FinesJournalRegistrars : ColumnsConfigRegistrarBase<FinesJournalViewModel, FineJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FineJournalNode> config) =>
			config.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
				.AddColumn("Сотудники").AddTextRenderer(node => node.FinedEmployeesNames)
				.AddColumn("Сумма штрафа").AddTextRenderer(node => node.FineSum.ToString(CultureInfo.CurrentCulture))
				.AddColumn("Причина штрафа")
					.AddTextRenderer(node => node.FineReason)
					.WrapMode(WrapMode.Word)
					.WrapWidth(600)
				.AddColumn("Автор штрафа").AddTextRenderer(node => node.AuthorName)
				.AddColumn("Подразделения сотрудников").AddTextRenderer(node => node.FinedEmployeesSubdivisions)
				.AddColumn("")
				.Finish();
	}
}
