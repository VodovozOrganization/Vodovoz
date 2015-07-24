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

		int savedCounterparty = 0;

		public int SavedCounterparty {
			get {
				return savedCounterparty;
			}
			set {
				savedCounterparty = value;
				labelSaved.LabelProp = SavedCounterparty.ToString ();
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

		int linkAccounts = 0;

		public int LinkedAccounts {
			get {
				return linkAccounts;
			}
			set {
				linkAccounts = value;
				labelLinkedAccount.LabelProp = LinkedAccounts.ToString ();
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
			progressbar.Text = "Читаем XML файл...";
			TotalCounterparty = SkipedCounterparty = ReadedAccounts = LinkedAccounts = InactiveAccounts = ReadedBanks = InactiveBanks = 0;
			CounterpatiesList.Clear ();
			AccountsList.Clear ();
			Banks1cList.Clear ();
			XmlDocument content = new XmlDocument ();
			content.Load (filechooserXML.Filename);

			progressbar.Text = "Разбор данных в файле...";
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
				case "КонтактнаяИнформация":
					ParseContactInfo (node);
					break;
				}
			}
			progressbar.Text = "Сопопоставляем расчетные счета с владельщами.";
			progressbar.Adjustment.Value = 0;
			progressbar.Adjustment.Upper = AccountsList.Count;

			foreach(var ac1c in AccountsList)
			{
				progressbar.Adjustment.Value++;
				QSMain.WaitRedraw ();

				if (ac1c.DomainAccount.Inactive)
					continue;

				var counterparty = CounterpatiesList.Find (c => c.Code1c == ac1c.OwnerCode1c);

				if(counterparty != null)
				{
					counterparty.AddAccount (ac1c.DomainAccount);
					LinkedAccounts++;
				}
			}

			progressbar.Text = "Выполнено";
			buttonSave.Sensitive = checkRewrite.Sensitive = CounterpatiesList.Count > 0;
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
				var counterparty = new Counterparty ();

				var nameNode = node.SelectSingleNode ("Свойство[@Имя='Наименование']/Значение");
				counterparty.Name = nameNode.InnerText;
				logger.Debug ("Читаем контрагента <{0}>", counterparty.Name);

				counterparty.Code1c = codeNode.InnerText;

				var jurNode = node.SelectSingleNode ("Свойство[@Имя='ЮрФизЛицо']/Значение");
				counterparty.PersonType = jurNode.InnerText == "ЮрЛицо" ? PersonType.legal : PersonType.natural;

				var accountNode = node.SelectSingleNode ("Свойство[@Имя='ОсновнойБанковскийСчет']/Ссылка/Свойство[@Имя='Код']/Значение");
				if(accountNode != null)
				{
					var ac1c = AccountsList.Find (a => a.DomainAccount.Code1c == accountNode.InnerText);
					counterparty.DefaultAccount = ac1c.DomainAccount;
				}

				var commentNode = node.SelectSingleNode ("Свойство[@Имя='Комментарий']/Значение");
				if(commentNode != null)
				{
					counterparty.Comment = commentNode.InnerText;
				}

				var fullnameNode = node.SelectSingleNode ("Свойство[@Имя='НаименованиеПолное']/Значение");
				if(fullnameNode != null)
				{
					counterparty.FullName = fullnameNode.InnerText;
				}

				var INNNode = node.SelectSingleNode ("Свойство[@Имя='ИНН']/Значение");
				if(INNNode != null)
				{
					counterparty.INN = INNNode.InnerText;
				}

				var KPPNode = node.SelectSingleNode ("Свойство[@Имя='КПП']/Значение");
				if(KPPNode != null)
				{
					counterparty.KPP = KPPNode.InnerText;
				}
					
				string[] InnSplited = counterparty.INN.Split ('/');
				if(InnSplited.Length > 1)
				{
					counterparty.INN = InnSplited [0];
					counterparty.KPP = InnSplited [1];
				}

				var MainNode = node.SelectSingleNode ("Свойство[@Имя='ГоловнойКонтрагент']/Значение");
				if (MainNode != null)
					logger.Warn ("ГоловнойКонтрагент не пустой");

				TotalCounterparty++;
				CounterpatiesList.Add (counterparty);
			}

		}

		void ParseAccount(XmlNode node)
		{
			var account1c = new Account1c ();
			Account account = account1c.DomainAccount;
			var codeNode = node.SelectSingleNode ("Ссылка/Свойство[@Имя='Код']/Значение");
			account.Code1c = codeNode.InnerText;

			var ownerNode = node.SelectSingleNode ("Ссылка/Свойство[@Имя='Владелец']/Ссылка/Свойство[@Имя='Код']/Значение");
			if (ownerNode != null)
				account1c.OwnerCode1c = ownerNode.InnerText;
			else
				logger.Warn ("Счет с кодом {0}, без владельца.", codeNode.InnerText);

			var nameNode = node.SelectSingleNode ("Свойство[@Имя='Наименование']/Значение");
			account.Name = nameNode.InnerText;

			var bankNode = node.SelectSingleNode ("Свойство[@Имя='Банк']/Ссылка/Свойство[@Имя='Код']/Значение");

			if (bankNode != null)
			{
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
			}
			else
				logger.Warn ("Счет с кодом {0}, без банка.", codeNode.InnerText);

			var numberNode = node.SelectSingleNode ("Свойство[@Имя='НомерСчета']/Значение");
			if (numberNode == null)
			{
				logger.Warn ("Пустой номер счета.");
				if(!account.Inactive)
					InactiveAccounts++;
				account.Inactive = true;
			}
			else
				account.Number = numberNode.InnerText;

			AccountsList.Add (account1c);
			ReadedAccounts++;
		}

		void ParseBank(XmlNode node)
		{
			var bank1c = new Bank1c ();
			var codeNode = node.SelectSingleNode ("Ссылка/Свойство[@Имя='Код']/Значение");

			if (codeNode == null)
			{
				logger.Warn ("В банке нет  кода!!! Пропускаем...");
				return;
			}
			
			bank1c.Code1c = codeNode.InnerText;

			var domainBank = Banks.FirstOrDefault (b => b.Bik == bank1c.Code1c);
			bank1c.DomainBank = domainBank;
			bank1c.IsDead = domainBank == null;

			if (bank1c.IsDead)
				InactiveBanks++;

			Banks1cList.Add (bank1c);
			ReadedBanks++;
		}

		void ParseContactInfo(XmlNode node)
		{
			var typeNode = node.SelectSingleNode ("Свойство[@Имя='Вид']/Значение");
			var objectNode = node.SelectSingleNode ("Свойство[@Имя='Объект']/Ссылка/Свойство[@Имя='Код']/Значение");

			var counterparty = CounterpatiesList.Find (c => c.Code1c == objectNode.InnerText);
			if(counterparty == null)
			{
				logger.Debug ("Контактная информация видемо для пропущеного контаргента. Идем дальше...");
				return;
			}
			var presentationNode = node.SelectSingleNode ("Свойство[@Имя='Представление']/Значение");

			switch(typeNode.InnerText)
			{
			case "ЮрАдресКонтрагента":
				counterparty.JurAddress = presentationNode.InnerText;
				break;
			case "ФактАдресКонтрагента":
				counterparty.Address = presentationNode.InnerText;
				break;
			case "ТелефонКонтрагента":
				counterparty.Comment = String.Format ("Телефоны импортированные из 1с 7.7: {0}\n{1}", presentationNode.InnerText, counterparty.Comment);
				break;
			default:
				logger.Warn ("Неизвестный тип контактной информации ({0})", typeNode.InnerText);
				break;
			}
		}

		protected void OnFilechooserXMLSelectionChanged (object sender, EventArgs e)
		{
			buttonLoad.Sensitive = !String.IsNullOrWhiteSpace (filechooserXML.Filename);
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			progressbar.Text = "Загружаем таблицу существующих контрагентов.";
			QSMain.WaitRedraw ();
			var ExistCouterpaties = Repository.CounterpartyRepository.All (UoW);

			progressbar.Text = "Сверяем контрагентов..";
			progressbar.Adjustment.Value = 0;
			progressbar.Adjustment.Value = CounterpatiesList.Count;
			QSMain.WaitRedraw ();

			foreach(var loaded in CounterpatiesList)
			{
				progressbar.Adjustment.Value++;
				QSMain.WaitRedraw ();

				var exist = ExistCouterpaties.FirstOrDefault (c => c.Code1c == loaded.Code1c);
				//TODO подумать про проверку по ИНН если надо.

				if (exist != null && !checkRewrite.Active)
					continue;

				if(exist != null)
				{
					loaded.Id = exist.Id;
					foreach(var loadedAcc in  loaded.Accounts)
					{
						var existAcc = exist.Accounts.FirstOrDefault (a => a.Code1c == loadedAcc.Code1c);
						if (existAcc != null)
							loadedAcc.Id = existAcc.Id;
					}
				}

				UoW.Save (loaded);
				SavedCounterparty++;
			}

			progressbar.Text = "Записывам в базу..";
			UoW.Commit ();
			progressbar.Text = "Выполнено";
		}
	}
}

