using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using TaxcomEdoApi.Config;
using Vodovoz.Core.Data.Documents;

namespace TaxcomEdoApi.Factories
{
	public interface IEdoUpdFactory
	{
		Fajl CreateNewUpdXml(
			InfoForCreatingEdoUpd orderInfoForEdo,
			WarrantOptions warrantOptions,
			string organizationAccountId,
			string certificateSubject);
	}
}
