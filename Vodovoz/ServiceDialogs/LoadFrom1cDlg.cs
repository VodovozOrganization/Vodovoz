using System;
using System.Collections.Generic;
using System.Xml;
using Gtk;
using QSBanks;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain;
using Vodovoz.LoadFrom1c;
using System.Linq;

namespace Vodovoz
{
	public partial class LoadFrom1cDlg : TdiTabBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot ();

		List<string> IncludeParents = new List<string>{
			"03282588",
			"03282397",
			"00014477",
			"03296274"
		};

		private IList<Bank> banks;

		private IList<Bank> Banks {
			get {
				if(banks == null)
				{
					banks = QSBanks.Repository.BankRepository.ActiveBanks (UoW);
				}
				return banks;
			}
		}

		int totalCounterparty = 0;

		public int TotalCounterparty {
			get {
				return totalCounterparty;
			}
			set {
				totalCounterparty = value;
				labelTotalCounterparty.LabelProp = totalCounterparty.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int skipedCounterparty = 0;

		public int SkipedCounterparty {
			get {
				return skipedCounterparty;
			}
			set {
				skipedCounterparty = value;
				labelSkipedCounterparty.LabelProp = SkipedCounterparty.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int readedAccounts = 0;

		public int ReadedAccounts {
			get {
				return readedAccounts;
			}
			set {
				readedAccounts = value;
				labelReadedAccounts.LabelProp = ReadedAccounts.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int inactiveAccounts = 0;

		public int InactiveAccounts {
			get {
				return inactiveAccounts;
			}
			set {
				inactiveAccounts = value;
				labelInactiveAccounts.LabelProp = InactiveAccounts.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int readedBanks = 0;

		public int ReadedBanks {
			get {
				return readedBanks;
			}
			set {
				readedBanks = value;
				labelTotalBanks.LabelProp = ReadedBanks.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int inactiveBanks = 0;

		public int InactiveBanks {
			get {
				return inactiveBanks;
			}
			set {
				inactiveBanks = value;
				labelInactiveBanks.LabelProp = InactiveBanks.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		List<Counterparty> CounterpatiesList = new List<Counterparty>();
		List<Account1c> AccountsList = new List<Account1c>();
		List<Bank1c> Banks1cList = new List<Bank1c>();

		public LoadFrom1cDlg ()
		{
			this.Build ();
			TabName = "Загрузка контрагентов из 1с 7.7";

			FileFilter Filter = new FileFilter();
			Filter.Name = "XML выгрузка";
			Filter.AddMimeType("application/xml");
			Filter.AddPattern("*.xml");
			filechooserXML.Filter = Filter;
		}

		protected void OnButtonLoadClicked (object sender, EventArgs e)
		{
			logger.Info ("Читаем XML файл...");
			TotalCounterparty = SkipedCounterparty = ReadedAccounts = InactiveAccounts = ReadedBanks = InactiveBanks = 0;
			XmlDocument content = new XmlDocument ();
			content.Load (filechooserXML.Filename);

			foreach(XmlNode node in content.SelectNodes ("/ФайлОбмена/Объект"))
			{
				string ruleName = node.Attributes ["ИмяПравила"].Value;
				switch(ruleName)
				{
				case "Контрагенты":
					ParseCounterparty (node);
					break;
				case "БанковскиеСчета":
					ParseAccount (node);
					break;
				case "Банки":
					ParseBank (node);
					break;

				}

			} 
		}

		void ParseCounterparty(XmlNode node)
		{

			var parrentNode = node.SelectSingleNode ("Свойство[@Имя='Родитель']/Ссылка/Свойство[@Имя='Код']/Значение");

			if(parrentNode == null || !IncludeParents.Contains (parrentNode.InnerText))
			{
				logger.Debug ("Пропускаем... так как не входит в выборку.");
				SkipedCounterparty++;
				return;
			}

			bool isGroup = false;
			var groupNode = node.SelectSingleNode ("Ссылка/Свойство[@Имя='ЭтоГруппа']/Значение");
			if (groupNode != null && groupNode.InnerText == "true") //Если не группа то isGroup == null так как нода "Значение" нет.
			{
				logger.Debug ("Это группа");
				isGroup = true;
			}

			var codeNode = node.SelectSingleNode ("Ссылка/Свойство[@Имя='Код']/Значение");

			if(isGroup)
			{
				IncludeParents.Add (codeNode.InnerText);
				logger.Debug ("добавили группу {0}.", codeNode.InnerText);
			}
			else
			{
				var couterparty = new Counterparty ();

				var nameNode = node.SelectSingleNode ("Свойство[@Имя='Наименование']/Значение");
				couterparty.Name = nameNode.InnerText;
				logger.Debug ("Читаем контрагента <{0}>", couterparty.Name);

				couterparty.Code1c = codeNode.InnerText;

				var jurNode = node.SelectSingleNode ("Свойство[@Имя='ЮрФизЛицо']/Значение");
				couterparty.PersonType = jurNode.InnerText == "ЮрЛицо" ? PersonType.legal : PersonType.natural;

				//TODO подумат про основной счет

				TotalCounterparty++;
				CounterpatiesList.Add (couterparty);
			}

		}

		void ParseAccount(XmlNode node)
		{
			var account1c = new Account1c ();
			Account account = account1c.DomainAccount;
			var codeNode = node.SelectSingleNode ("Ссылка/Свойство[@Имя='Код']/Значение");
			account.Code1c = codeNode.InnerText;

			var ownerNode = node.SelectSingleNode ("Ссылка/Свойство[@Имя='Владелец']/Ссылка/Свойство[@Имя='Код']/Значение");
			account1c.OwnerCode1c = ownerNode.InnerText;

			var nameNode = node.SelectSingleNode ("Свойство[@Имя='Наименование']/Значение");
			account.Name = nameNode.InnerText;

			var bankNode = node.SelectSingleNode ("Свойство[@Имя='Банк']/Ссылка/Свойство[@Имя='Код']/Значение");

			var readedBank = Banks1cList.Find (b => b.Code1c == bankNode.InnerText);
			if(readedBank.IsDead)
			{
				account.Inactive = true;
				InactiveAccounts++;
			}
			else
			{
				account.InBank = readedBank.DomainBank;
			}

			var numberNode = node.SelectSingleNode ("Свойство[@Имя='НомерСчета']/Значение");
			account.Number = numberNode.InnerText;

			AccountsList.Add (account1c);
			ReadedAccounts++;
		}

		void ParseBank(XmlNode node)
		{
			var bank1c = new Bank1c ();
			var codeNode = node.SelectSingleNode ("Ссылка/Свойство[@Имя='Код']/Значение");
			bank1c.Code1c = codeNode.InnerText;

			var domainBank = Banks.FirstOrDefault (b => b.Bik == bank1c.Code1c);
			bank1c.DomainBank = domainBank;
			bank1c.IsDead = domainBank == null;

			if (bank1c.IsDead)
				InactiveBanks++;

			Banks1cList.Add (bank1c);
			ReadedBanks++;
		}

		protected void OnFilechooserXMLSelectionChanged (object sender, EventArgs e)
		{
			buttonLoad.Sensitive = !String.IsNullOrWhiteSpace (filechooserXML.Filename);
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}
	}
}

