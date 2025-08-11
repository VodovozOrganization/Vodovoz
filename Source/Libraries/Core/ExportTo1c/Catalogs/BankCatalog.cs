using System.Collections.Generic;
using ExportTo1c.Library.ExportNodes;
using QS.Banks.Domain;
using Vodovoz.EntityRepositories.Orders;

namespace ExportTo1c.Library.Catalogs
{
	/// <summary>
	/// Банк
	/// </summary>
	public class BankCatalog : GenericCatalog<Bank>
	{
		public BankCatalog(ExportData exportData)
			: base(exportData)
		{
		}

		protected override string Name => exportData.ExportMode == Export1cMode.ComplexAutomation ? "КлассификаторБанков" : "Банки";

		public override ReferenceNode CreateReferenceTo(Bank bank)
		{
			int id = GetReferenceId(bank);

			var referenceNode = new ReferenceNode(id, new PropertyNode("Код", Common1cTypes.String, bank.Bik));

			referenceNode.Properties.Add(new PropertyNode("ЭтоГруппа", Common1cTypes.ReferenceCounterparty));

			return referenceNode;
		}

		protected override PropertyNode[] GetProperties(Bank bank)
		{
			var properties = new List<PropertyNode>();
			properties.Add(
				new PropertyNode("ПометкаУдаления",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new PropertyNode("Наименование",
					Common1cTypes.String,
					bank.Name
				)
			);
			properties.Add(
				new PropertyNode("Родитель",
					Common1cTypes.ReferenceBank(exportData.ExportMode)
				)
			);
			properties.Add(
				new PropertyNode("Город",
					Common1cTypes.String,
					bank.City
				)
			);
			properties.Add(
				new PropertyNode("КоррСчет",
					Common1cTypes.String,
					bank.DefaultCorAccount.CorAccountNumber
				)
			);
			properties.Add(
				new PropertyNode("Адрес",
					Common1cTypes.String,
					bank.RegionText
				)
			);
			properties.Add(
				new PropertyNode("Телефоны",
					Common1cTypes.String
				)
			);
			return properties.ToArray();
		}
	}
}
