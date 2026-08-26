using System.Collections.Generic;
using VodovozBusiness.Nodes;

namespace Vodovoz.SidePanel.InfoProviders
{
	public interface IEmailsInfoProvider : IInfoProvider
	{
		bool CanHaveEmails { get; }
		IList<SentStoredEmailNode> GetEmails();
	}
}
