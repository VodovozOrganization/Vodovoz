using System.Collections;
using System.Collections.Generic;
using QS.RepresentationModel;
using Vodovoz.Representations;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.ViewModels.Infrastructure.InfoProviders
{
	public interface ICashInfoProvider : IInfoProvider
	{
		CashDocumentsFilter CashFilter { get; }
	}
}
