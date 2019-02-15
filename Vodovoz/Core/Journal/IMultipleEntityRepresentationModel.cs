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
	}
}
