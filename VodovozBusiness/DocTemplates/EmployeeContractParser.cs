using System;
using QSDocTemplates;
using Vodovoz.Domain.Employees;

namespace Vodovoz.DocTemplates
{
	public class EmployeeContractParser : DocParserBase<EmployeeContract>
	{
		public override void UpdateFields()
		{
			fieldsList.Clear();

			AddField(x => x.Document.Document, PatternFieldType.FString);
			AddField(x => x.Document.PassportSeria, PatternFieldType.FString);
			AddField(x => x.Document.PassportNumber, PatternFieldType.FString);
			AddField(x => x.Document.PassportIssuedOrg, PatternFieldType.FString);
			AddField(x => x.Document.PassportIssuedDate, PatternFieldType.FString);

			AddField(x => x.FirstDay, PatternFieldType.FString);
			AddField(x => x.LastDay, PatternFieldType.FString);

			SortFields();
		}
	}
}
