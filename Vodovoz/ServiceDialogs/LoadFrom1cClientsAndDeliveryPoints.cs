using System;
using System.Collections.Generic;
using Gtk;
using System.Linq;
using System.Xml;
using Vodovoz.Domain.Client;
using QSTDI;

namespace Vodovoz.ServiceDialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class LoadFrom1cClientsAndDeliveryPoints : TdiTabBase
	{
		List<Counterparty> CounterpatiesList = new List<Counterparty>();

		public LoadFrom1cClientsAndDeliveryPoints()
		{
			this.Build();

			FileFilter Filter = new FileFilter();
			Filter.Name = "XML выгрузка";
			Filter.AddMimeType("application/xml");
			Filter.AddPattern("*.xml");
			filechooserXML.Filter = Filter;
		}

		void ParseCounterparty(XmlNode node)
		{
			var counterparty = new Counterparty();

			var codeNode = node.SelectSingleNode("Ссылка/Свойство[@Имя='Код']/Значение");

			var nameNode = node.SelectSingleNode("Свойство[@Имя='Наименование']/Значение");
			counterparty.Name = nameNode.InnerText;

			counterparty.Code1c = codeNode.InnerText;

			var jurNode = node.SelectSingleNode("Свойство[@Имя='ВидКонтрагента']/Значение");
			counterparty.PersonType = jurNode.InnerText == "ЮрЛицо" ? PersonType.legal : PersonType.natural;
			counterparty.PaymentMethod = jurNode.InnerText == "ЮрЛицо" ? PaymentType.cashless : PaymentType.cash;

			var accountNode = node.SelectSingleNode("Свойство[@Имя='НомерСчета']/Значение");
			if(accountNode != null) {
				var ac1c = AccountsList.Find(a => a.DomainAccount.Code1c == accountNode.InnerText);
				if(!ac1c.DomainAccount.Inactive)
					counterparty.DefaultAccount = ac1c.DomainAccount;
			}

			var commentNode = node.SelectSingleNode("Свойство[@Имя='Комментарий']/Значение");
			if(commentNode != null) {
				counterparty.Comment = commentNode.InnerText;
			}

			var fullnameNode = node.SelectSingleNode("Свойство[@Имя='НаименованиеПолное']/Значение");
			if(fullnameNode != null) {
				counterparty.FullName = fullnameNode.InnerText;
			}

			var INNNode = node.SelectSingleNode("Свойство[@Имя='ИНН']/Значение");
			if(INNNode != null) {
				if(INNNode.InnerText.Length > 12)
					counterparty.INN = INNNode.InnerText.Substring(0, 12);
				else
					counterparty.INN = INNNode.InnerText;
			}

			var KPPNode = node.SelectSingleNode("Свойство[@Имя='КПП']/Значение");
			if(KPPNode != null) {
				if(KPPNode.InnerText.Length > 9)
					counterparty.KPP = KPPNode.InnerText.Substring(0, 9);
				else
					counterparty.KPP = KPPNode.InnerText;
			}

			string[] InnSplited = counterparty.INN.Split('/');
			if(InnSplited.Length > 1) {
				counterparty.INN = InnSplited[0];
				counterparty.KPP = InnSplited[1];
			}

			if(counterparty.PersonType == PersonType.legal) {
				counterparty.FullName = counterparty.FullName.TrimStart();
				var found = CommonValues.Ownerships.FirstOrDefault(x => counterparty.FullName.StartsWith(x.Key));
				if(!String.IsNullOrEmpty(found.Key))
					counterparty.TypeOfOwnership = found.Key;
			}

			var MainNode = node.SelectSingleNode("Свойство[@Имя='ГоловнойКонтрагент']/Значение");
			if(MainNode != null)

			var phoneNode = node.SelectSingleNode("Свойство[@Имя='НомерТелефона']/Значение");
			if(phoneNode != null) {
				counterparty.PhoneFrom1c = phoneNode.InnerText;
			}

			TotalCounterparty++;
			CounterpatiesList.Add(counterparty);

		}


		protected void OnFilechooserXMLSelectionChanged(object sender, EventArgs e)
		{
			buttonLoad.Sensitive = !String.IsNullOrWhiteSpace(filechooserXML.Filename);
		}

		protected void OnButtonLoadClicked(object sender, EventArgs e)
		{
			XmlDocument content = new XmlDocument();
			content.Load(filechooserXML.Filename);

			progressbar.Text = "Разбор данных в файле...";
			foreach(XmlNode node in content.SelectNodes("/ФайлОбмена/Объект")) {
				string ruleName = node.Attributes["ИмяПравила"].Value;
				switch(ruleName) {
					case "Контрагенты":
						ParseCounterparty(node);
						break;
					/*case "БанковскиеСчета":
						ParseAccount(node);
						break;
					case "Банки":
						ParseBank(node);
						break;
					case "КонтактнаяИнформация":
						ParseContactInfo(node);
						break;
					case "Номенклатура":
						ParseNomenclature(node);
						break;
					case "РеализацияТоваровУслуг":
						ParseOrders(node);
						break;*/
				}
			}
			progressbar.Text = "Сопопоставляем расчетные счета с владельцами.";
			progressbar.Adjustment.Value = 0;
			progressbar.Adjustment.Upper = AccountsList.Count;

			foreach(var ac1c in AccountsList) {
				progressbar.Adjustment.Value++;
				QSMain.WaitRedraw();

				if(ac1c.DomainAccount.Inactive)
					continue;

				var counterparty = CounterpatiesList.Find(c => c.Code1c == ac1c.OwnerCode1c);

				if(counterparty != null) {
					counterparty.AddAccount(ac1c.DomainAccount);
					LinkedAccounts++;
				}
			}

			progressbar.Text = "Выполнено";
			buttonCreate.Sensitive = checkRewrite.Sensitive = checkOnlyAddress.Sensitive =
				CounterpatiesList.Count > 0 || OrdersList.Count > 0 || NomenclaturesList.Count > 0;
			checkSkipCounterparties.Sensitive = true;
		}
	}
}
