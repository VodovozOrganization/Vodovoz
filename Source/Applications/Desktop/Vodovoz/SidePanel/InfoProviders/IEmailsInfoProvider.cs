using System.Collections.Generic;
using Vodovoz.Domain.StoredEmails;

namespace Vodovoz.SidePanel.InfoProviders
{
	public interface IEmailsInfoProvider : IInfoProvider
	{
		bool CanHaveEmails { get; }
		List<StoredEmail> GetEmails();
	}
}
