using System;
using System.Collections.Generic;
using QS.Tdi;

namespace Vodovoz.Core.Journal
{
	public interface IMultipleEntityRepresentationModel : QS.RepresentationModel.GtkUI.IRepresentationModel
	{
		IEnumerable<ActionForCreateEntityConfig> NewEntityActionsConfigs { get; }
		ITdiTab GetOpenEntityDlg(object node);
		Type GetEntityType(object node);
		int GetDocumentId(object node);

		//FIXME думаю что лучше будет добавить этот метод в базовый интерфейс IRepresentationModel
		string GetSummaryInfo();
	}
}
