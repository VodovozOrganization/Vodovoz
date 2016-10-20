using System;
using System.Linq;
using System.Text.RegularExpressions;
using Gtk;
using QSBanks;
using QSBanks.Repository;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain;
using Vodovoz.Domain.Accounting;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Repository;
using Vodovoz.Repository.Cash;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class LoadBankTransferDocumentDlg : TdiTabBase
	{
		private BankTransferDocumentParser parser;
		private IUnitOfWork uow;

		private const string NeedToAdd = "light coral";
		private const string OddRowColor = "white";
		private const string EvenRowColor = "gray94";

		private int rowsCount = 0;

		private decimal totalIncome = 0;
		private decimal totalExpense = 0;
	
		private ListStore documents = new ListStore (
			                              typeof(bool), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string),
			                              typeof(string),
			                              typeof(TransferDocument),
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string), 
			                              typeof(string),
			                              typeof(string),
			                              typeof(ListStore),
			                              typeof(string),
			                              typeof(object)
		                              );
		private ListStore expenseCategories = new ListStore (typeof(string), typeof(ExpenseCategory));
		private ListStore incomeCategories = new ListStore (typeof(string), typeof(IncomeCategory));

		private enum Columns
		{
			CheckCol,
			NumberCol,
			DateCol,
			TotalCol,
			PayerNameCol,
			PayerAccountCol,
			PayerBankCol,
			RecipientNameCol,
			RecipientAccountCol,
			RecipientBankCol,
			PaymentReasonCol,
			TransferDocumentCol,
			PayerNameColorCol,
			PayerAccountColorCol,
			PayerBankColorCol,
			RecipientNameColorCol,
			RecipientAccountColorCol,
			RecipientBankColorCol,
			CategoryComboModel,
			CategoryName,
			CategoryObject
		}

		public LoadBankTransferDocumentDlg ()
		{
			this.Build ();
			uow = UnitOfWorkFactory.CreateWithoutRoot ();
			TabName = "Загрузка из банк-клиента";

			var txtFilter = new FileFilter ();
			txtFilter.AddPattern ("*.txt");
			txtFilter.Name = "Текстовые файлы (*.txt)";
			var allFilter = new FileFilter ();
			allFilter.AddPattern ("*");
			allFilter.Name = "Все файлы";
			filechooser.AddFilter (txtFilter);
			filechooser.AddFilter (allFilter);

			labelDescription1.Markup = String.Format ("<span background=\"{0}\">     </span> - объект будет создан", NeedToAdd);

			foreach (var category in CategoryRepository.ExpenseCategories (uow)) {
				expenseCategories.AppendValues (category.Name, category);
			}
			foreach (var category in CategoryRepository.IncomeCategories (uow)) {
				incomeCategories.AppendValues (category.Name, category);
			}

			TreeViewColumn checkColumn = new TreeViewColumn ();
			checkColumn.Title = "";
			CellRendererToggle checkCell = new CellRendererToggle ();
			checkCell.Activatable = true;
			checkCell.Toggled += CheckCell_Toggled;
			checkColumn.PackStart (checkCell, true);
			checkColumn.AddAttribute (checkCell, "active", (int)Columns.CheckCol);


			TreeViewColumn numberColumn = new TreeViewColumn ();
			numberColumn.Title = "№";
			CellRendererText numberCell = new CellRendererText ();
			numberCell.Background = EvenRowColor;
			numberColumn.PackStart (numberCell, true);
			numberColumn.AddAttribute (numberCell, "text", (int)Columns.NumberCol);

			TreeViewColumn dateColumn = new TreeViewColumn ();
			dateColumn.Title = "Дата";
			CellRendererText dateCell = new CellRendererText ();
			dateColumn.PackStart (dateCell, true);
			dateColumn.AddAttribute (dateCell, "text", (int)Columns.DateCol);

			TreeViewColumn totalColumn = new TreeViewColumn ();
			totalColumn.Title = "Сумма";
			CellRendererText totalCell = new CellRendererText ();
			totalCell.Background = EvenRowColor;
			totalColumn.PackStart (totalCell, true);
			totalColumn.AddAttribute (totalCell, "text", (int)Columns.TotalCol);

			TreeViewColumn payerColumn = new TreeViewColumn ();
			payerColumn.Title = "Плательщик";
			CellRendererText payerCell = new CellRendererText ();
			payerCell.WidthChars = 40;
			payerColumn.PackStart (payerCell, true);
			payerColumn.AddAttribute (payerCell, "text", (int)Columns.PayerNameCol);
			payerColumn.AddAttribute (payerCell, "background", (int)Columns.PayerNameColorCol);
			payerColumn.Resizable = true;

			TreeViewColumn payerAccountColumn = new TreeViewColumn ();
			payerAccountColumn.Title = "Счет плательщика";
			CellRendererText payerAccountCell = new CellRendererText ();
			payerAccountColumn.PackStart (payerAccountCell, true);
			payerAccountColumn.AddAttribute (payerAccountCell, "text", (int)Columns.PayerAccountCol);
			payerAccountColumn.AddAttribute (payerAccountCell, "background", (int)Columns.PayerAccountColorCol);
			payerAccountColumn.Visible = false;

			TreeViewColumn payerBankColumn = new TreeViewColumn ();
			payerBankColumn.Title = "Банк плательщика";
			CellRendererText payerBankCell = new CellRendererText ();
			payerBankCell.WidthChars = 40;
			payerBankColumn.PackStart (payerBankCell, true);
			payerBankColumn.AddAttribute (payerBankCell, "text", (int)Columns.PayerBankCol);
			payerBankColumn.AddAttribute (payerBankCell, "background", (int)Columns.PayerBankColorCol);
			payerBankColumn.Resizable = true;
			payerBankColumn.Visible = false;

			TreeViewColumn recipientColumn = new TreeViewColumn ();
			recipientColumn.Title = "Получатель";
			CellRendererText recipientCell = new CellRendererText ();
			recipientCell.WidthChars = 40;
			recipientColumn.PackStart (recipientCell, true);
			recipientColumn.AddAttribute (recipientCell, "text", (int)Columns.RecipientNameCol);
			recipientColumn.AddAttribute (recipientCell, "background", (int)Columns.RecipientNameColorCol);
			recipientColumn.Resizable = true;

			TreeViewColumn recipientAccountColumn = new TreeViewColumn ();
			recipientAccountColumn.Title = "Счет получателя";
			CellRendererText recipientAccountCell = new CellRendererText ();
			recipientAccountColumn.PackStart (recipientAccountCell, true);
			recipientAccountColumn.AddAttribute (recipientAccountCell, "text", (int)Columns.RecipientAccountCol);
			recipientAccountColumn.AddAttribute (recipientAccountCell, "background", (int)Columns.RecipientAccountColorCol);
			recipientAccountColumn.Visible = false;

			TreeViewColumn recipientBankColumn = new TreeViewColumn ();
			recipientBankColumn.Title = "Банк получателя";
			CellRendererText recipientBankCell = new CellRendererText ();
			recipientBankCell.WidthChars = 40;
			recipientBankColumn.PackStart (recipientBankCell, true);
			recipientBankColumn.AddAttribute (recipientBankCell, "text", (int)Columns.RecipientBankCol);
			recipientBankColumn.AddAttribute (recipientBankCell, "background", (int)Columns.RecipientBankColorCol);
			recipientBankColumn.Resizable = true;
			recipientBankColumn.Visible = false;

			TreeViewColumn paymentReasonColumn = new TreeViewColumn ();
			paymentReasonColumn.Title = "Назначение платежа";
			CellRendererText paymentReasonCell = new CellRendererText ();
			paymentReasonCell.WidthChars = 40;
			paymentReasonColumn.PackStart (paymentReasonCell, true);
			paymentReasonColumn.AddAttribute (paymentReasonCell, "text", (int)Columns.PaymentReasonCol);
			paymentReasonColumn.Resizable = true;

			TreeViewColumn categoryColumn = new TreeViewColumn ();
			categoryColumn.Title = "Категория дохода/расхода";
			CellRendererCombo categoryCombo = new CellRendererCombo ();
			categoryCombo.TextColumn = 0;
			categoryCombo.Editable = true;
			categoryCombo.HasEntry = false;
			categoryCombo.Edited += CategoryCombo_Edited;
			categoryColumn.PackStart (categoryCombo, true);
			categoryColumn.AddAttribute (categoryCombo, "model", (int)Columns.CategoryComboModel);
			categoryColumn.AddAttribute (categoryCombo, "text", (int)Columns.CategoryName);

			treeDocuments.AppendColumn (checkColumn);
			treeDocuments.AppendColumn (numberColumn);
			treeDocuments.AppendColumn (dateColumn);
			treeDocuments.AppendColumn (totalColumn);
			treeDocuments.AppendColumn (categoryColumn);
			treeDocuments.AppendColumn (payerColumn);
			treeDocuments.AppendColumn (payerAccountColumn);
			treeDocuments.AppendColumn (payerBankColumn);
			treeDocuments.AppendColumn (recipientColumn);
			treeDocuments.AppendColumn (recipientAccountColumn);
			treeDocuments.AppendColumn (recipientBankColumn);
			treeDocuments.AppendColumn (paymentReasonColumn);

			treeDocuments.Model = documents;
			treeDocuments.TooltipColumn = (int)Columns.PaymentReasonCol;

			buttonUpload.Sensitive = buttonReadFile.Sensitive = false;
			checkButtonAll.Sensitive = false;
		}

		void CategoryCombo_Edited (object o, EditedArgs args)
		{
			TreeIter iter, refIter;
			if (!documents.GetIterFromString (out iter, args.Path))
				return;
			if (String.IsNullOrWhiteSpace (args.NewText))
				return;
			documents.SetValue (iter, (int)Columns.CategoryName, args.NewText);
			var listStore = (ListStore)documents.GetValue (iter, (int)Columns.CategoryComboModel);
			if (ListStoreWorks.SearchListStore (listStore, args.NewText, out refIter)) {
				var selected = listStore.GetValue (refIter, 1);
				documents.SetValue (iter, (int)Columns.CategoryObject, selected);
			}
		}

		void CheckCell_Toggled (object o, ToggledArgs args)
		{
			TreeIter iter;

			if (documents.GetIter (out iter, new TreePath (args.Path))) {
				bool old = (bool)documents.GetValue (iter, (int)Columns.CheckCol);
				documents.SetValue (iter, (int)Columns.CheckCol, !old);
			}

			checkButtonAll.Toggled -= OnCheckButtonAllToggled;
			checkButtonAll.Active = false;
			checkButtonAll.Toggled += OnCheckButtonAllToggled;
		}

		protected void OnButtonReadFileClicked (object sender, EventArgs e)
		{
			uow = UnitOfWorkFactory.CreateWithoutRoot ();
			documents.Clear ();
			checkButtonAll.Active = true;
			rowsCount = 0;
			parser = new BankTransferDocumentParser (filechooser.Filename);
			parser.Parse ();
			buttonUpload.Sensitive = true;
			checkButtonAll.Sensitive = true;
			foreach (var doc in parser.TransferDocuments) {
				documents.AppendValues (
					true, 
					doc.Number, 
					doc.Date.ToShortDateString (), 
					doc.Total.ToString (), 
					doc.PayerName,
					doc.PayerCheckingAccount,
					doc.PayerBank,
					doc.RecipientName,
					doc.RecipientCheckingAccount,
					doc.RecipientBank,
					doc.PaymentPurpose,
					doc,
					OddRowColor, EvenRowColor, OddRowColor, EvenRowColor, OddRowColor, EvenRowColor, 
					null, String.Empty, null
				);
				rowsCount++;
			}
			HighlightDocuments ();
		}

		protected void OnFilechooserSelectionChanged (object sender, EventArgs e)
		{
			buttonReadFile.Sensitive = !String.IsNullOrWhiteSpace (filechooser.Filename);
			buttonUpload.Sensitive = false;
		}

		protected bool CheckAllCategoriesSelected ()
		{
			TreeIter iter;
			if (!documents.GetIterFirst (out iter))
				return false;
			
			do {
				if (!(bool)documents.GetValue (iter, (int)Columns.CheckCol))
					continue;
				if (documents.GetValue (iter, (int)Columns.CategoryObject) == null)
					return false;
			} while (documents.IterNext (ref iter));
			return true;
		}

		protected void HighlightDocuments ()
		{
			totalIncome = totalExpense = 0;
			TreeIter iter;
			if (!documents.GetIterFirst (out iter))
				return;

			var defaultIncomeCategory = CategoryRepository.DefaultIncomeCategory (uow);
			progressBar.Fraction = 0;
			progressBar.Text = "Идет обработка файла выгрузки...";
			double progressStep = 1.0 / rowsCount;
			do {
				//Шевелим прогрессбаром. I'd like to move it, move it.
				if (progressBar.Fraction + progressStep > 1)
					progressBar.Fraction = 1;
				else
					progressBar.Fraction += progressStep;
				while (Application.EventsPending ())
					Application.RunIteration ();

				var doc = (documents.GetValue (iter, (int)Columns.TransferDocumentCol) as TransferDocument);

				//Сначала пробуем найти нашу организацию. Она должна фигурировать либо как получатель либо как плательщик.
				var organization = OrganizationRepository.GetOrganizationByInn (uow, doc.PayerInn);
				if (organization == null) {
					//Нам платят
					organization = OrganizationRepository.GetOrganizationByInn (uow, doc.RecipientInn);
					if (organization == null) {
						organization = OrganizationRepository.GetOrganizationByAccountNumber (uow, doc.RecipientCheckingAccount);
						if (organization == null) {
							progressBar.Fraction = 0;
							progressBar.Text = "Ошибка обработки выгрузки!";
							throw new Exception ("Не удалось обнаружить нашу организацию ни по ИНН, ни по номеру счета. Заполните организацию, или проверьте корректность ИНН.");
						}
					}
					//Проверяем наш счет
					if (!organization.Accounts.Any (acc => acc.Number == doc.RecipientCheckingAccount))
						documents.SetValue (iter, (int)Columns.RecipientAccountColorCol, NeedToAdd);
					//Ищем плательщика
					var payerCounterparty = CounterpartyRepository.GetCounterpartyByINN (uow, doc.PayerInn);
					if(payerCounterparty == null)
						payerCounterparty = CounterpartyRepository.GetCounterpartyByAccount (uow, doc.PayerAccount);
					
					if (payerCounterparty == null) {
						documents.SetValue (iter, (int)Columns.PayerNameColorCol, NeedToAdd);
						documents.SetValue (iter, (int)Columns.PayerAccountColorCol, NeedToAdd);
					} else if (!payerCounterparty.Accounts.Any (acc => acc.Number == doc.PayerCheckingAccount))
						documents.SetValue (iter, (int)Columns.PayerAccountColorCol, NeedToAdd);
					documents.SetValue (iter, (int)Columns.CategoryComboModel, incomeCategories);
					documents.SetValue (iter, (int)Columns.CategoryName, defaultIncomeCategory.Name);
					documents.SetValue (iter, (int)Columns.CategoryObject, defaultIncomeCategory);
					totalIncome += doc.Total;
				} else {
					//Мы платим
					//Проверяем наш счет
					if (!organization.Accounts.Any (acc => acc.Number == doc.PayerCheckingAccount))
						documents.SetValue (iter, (int)Columns.PayerAccountColorCol, NeedToAdd);
					//Ищем получателя
					var recipientCounterparty = CounterpartyRepository.GetCounterpartyByINN (uow, doc.RecipientInn);
					if (recipientCounterparty == null) {
						//Возможно это сотрудник
						var employee = EmployeeRepository.GetEmployeeByINNAndAccount (uow, doc.RecipientInn, doc.RecipientCheckingAccount);
						if (employee == null) {
							documents.SetValue (iter, (int)Columns.RecipientNameColorCol, NeedToAdd);
							documents.SetValue (iter, (int)Columns.RecipientAccountColorCol, NeedToAdd);
						}
					} else if (!recipientCounterparty.Accounts.Any (acc => acc.Number == doc.RecipientCheckingAccount))
						documents.SetValue (iter, (int)Columns.RecipientAccountColorCol, NeedToAdd);
					documents.SetValue (iter, (int)Columns.CategoryComboModel, expenseCategories);
					if (recipientCounterparty != null && recipientCounterparty.DefaultExpenseCategory != null) {
						documents.SetValue (iter, (int)Columns.CategoryName, recipientCounterparty.DefaultExpenseCategory.Name);
						documents.SetValue (iter, (int)Columns.CategoryObject, recipientCounterparty.DefaultExpenseCategory);
					}
					totalExpense += doc.Total;
				}

				//Проверяем банки
				var payerBank = BankRepository.GetBankByBik (uow, doc.PayerBik);
				if (payerBank == null && !String.IsNullOrEmpty (doc.PayerBik))
					documents.SetValue (iter, (int)Columns.PayerBankColorCol, NeedToAdd);
				var recipientBank = BankRepository.GetBankByBik (uow, doc.RecipientBik);
				if (recipientBank == null && !String.IsNullOrEmpty (doc.PayerBik))
					documents.SetValue (iter, (int)Columns.RecipientBankColorCol, NeedToAdd);
			} while (documents.IterNext (ref iter));
			progressBar.Text = "Обработка файла выгрузки завершена";
		}

		protected void OnButtonUploadClicked (object sender, EventArgs e)
		{
			if (!CheckAllCategoriesSelected ()) {
				MessageDialogWorks.RunErrorDialog ("Не у всех отмеченных документов выбраны категории дохода/расхода.");
				return;
			}
			FillCounterparties ();
			FillDocuments ();
		}

		protected void FillCounterparties ()
		{
			uow = UnitOfWorkFactory.CreateWithoutRoot ();
			TreeIter iter;
			progressBar.Fraction = 0;
			progressBar.Text = "Идет исправление/создание недостающих объектов... Операция 1/2";
			double progressStep = 1.0 / rowsCount;

			if (!documents.GetIterFirst (out iter))
				return;
//			try {
				do {
					//Шевелим прогрессбаром. I'd like to move it, move it.
					if (progressBar.Fraction + progressStep > 1)
						progressBar.Fraction = 1;
					else
						progressBar.Fraction += progressStep;
					while (Application.EventsPending ())
						Application.RunIteration ();

					//Проверяем галочку.
					if (!(bool)documents.GetValue (iter, (int)Columns.CheckCol))
						continue;

					//Получаем документ
					var doc = (TransferDocument)documents.GetValue (iter, (int)Columns.TransferDocumentCol);

					//Обрабатываем банк плательщика.
					if ((documents.GetValue (iter, (int)Columns.PayerBankColorCol) as string) == NeedToAdd) {
						var buow = UnitOfWorkFactory.CreateWithNewRoot<Bank> ();
						buow.Root.Bik = doc.PayerBik;
						buow.Root.Name = doc.PayerBank;
						buow.Root.CorAccount = doc.PayerCorrespondentAccount;
						buow.Save ();
						documents.SetValue (iter, (int)Columns.PayerBankColorCol, OddRowColor);
						DeselectSameBanks (doc.PayerBik);
					}

					//Обрабатываем плательщика
					if ((documents.GetValue (iter, (int)Columns.PayerNameColorCol) as string) == NeedToAdd) {
						var cuow = UnitOfWorkFactory.CreateWithNewRoot<Counterparty> ();
						cuow.Root.Name = doc.PayerName;
						cuow.Root.FullName = doc.PayerName;
						cuow.Root.INN = doc.PayerInn;
						cuow.Root.KPP = doc.PayerKpp;
						cuow.Root.PaymentMethod = PaymentType.cashless;
						cuow.Root.TypeOfOwnership = TryGetOrganizationType (doc.PayerName);
						if (cuow.Root.TypeOfOwnership != null)
							cuow.Root.PersonType = PersonType.legal;
						else {
							if (MessageDialogWorks.RunQuestionDialog (
								    String.Format ("Не удалось определить тип контрагента. Контрагент \"{0}\" является юридическим лицом?", doc.PayerName)))
								cuow.Root.PersonType = PersonType.legal;
							else
								cuow.Root.PersonType = PersonType.natural;
						}
						cuow.Root.ObservableAccounts.Add (new Account () {
							Number = doc.PayerCheckingAccount,
							InBank = BankRepository.GetBankByBik (uow, doc.PayerBik)
						});
						cuow.Save ();
						documents.SetValue (iter, (int)Columns.PayerNameColorCol, OddRowColor);
						documents.SetValue (iter, (int)Columns.PayerAccountColorCol, EvenRowColor);
						DeselectSameOrganizations (doc.PayerInn, doc.PayerName);
					} else if ((documents.GetValue (iter, (int)Columns.PayerAccountColorCol) as string) == NeedToAdd) {
						var cuow = UnitOfWorkFactory.CreateForRoot <Counterparty> (
							           CounterpartyRepository.GetCounterpartyByINN (uow, doc.PayerInn).Id);
						if (!cuow.Root.Accounts.Any (acc => acc.Number == doc.PayerCheckingAccount)) {
							cuow.Root.Accounts.Add (new Account () {
								Number = doc.PayerCheckingAccount,
								InBank = BankRepository.GetBankByBik (uow, doc.PayerBik)
							});
							cuow.Save ();
						}
						documents.SetValue (iter, (int)Columns.PayerAccountColorCol, EvenRowColor);
					}

					//Обрабатываем банк получателя.
					if ((documents.GetValue (iter, (int)Columns.RecipientBankColorCol) as string) == NeedToAdd) {
						var buow = UnitOfWorkFactory.CreateWithNewRoot<Bank> ();
						buow.Root.Bik = doc.RecipientBik;
						buow.Root.Name = doc.RecipientBank;
						buow.Root.CorAccount = doc.RecipientCorrespondentAccount;
						buow.Save ();
						documents.SetValue (iter, (int)Columns.RecipientBankColorCol, EvenRowColor);
						DeselectSameBanks (doc.RecipientBik);
					}

					//Обрабатываем получателя
					if ((documents.GetValue (iter, (int)Columns.RecipientNameColorCol) as string) == NeedToAdd) {
						var cuow = UnitOfWorkFactory.CreateWithNewRoot<Counterparty> ();
						cuow.Root.Name = doc.RecipientName;
						cuow.Root.FullName = doc.RecipientName;
						cuow.Root.INN = doc.RecipientInn;
						cuow.Root.KPP = doc.RecipientKpp;
						cuow.Root.CooperationSupplier = true;
						cuow.Root.PaymentMethod = PaymentType.cashless;
						cuow.Root.TypeOfOwnership = TryGetOrganizationType (doc.RecipientName);
						if (cuow.Root.TypeOfOwnership != null)
							cuow.Root.PersonType = PersonType.legal;
						else {
							if (MessageDialogWorks.RunQuestionDialog (
								    String.Format ("Не удалось определить тип контрагента. Контрагент \"{0}\" является юридическим лицом?", doc.RecipientName)))
								cuow.Root.PersonType = PersonType.legal;
							else
								cuow.Root.PersonType = PersonType.natural;
						}
						cuow.Root.ObservableAccounts.Add (new Account () {
							Number = doc.RecipientCheckingAccount,
							InBank = BankRepository.GetBankByBik (uow, doc.RecipientBik)
						});
						cuow.Save ();
						documents.SetValue (iter, (int)Columns.RecipientNameColorCol, EvenRowColor);
						documents.SetValue (iter, (int)Columns.RecipientAccountColorCol, OddRowColor);
						DeselectSameOrganizations (doc.RecipientInn, doc.RecipientName);
					} else if ((documents.GetValue (iter, (int)Columns.RecipientAccountColorCol) as string) == NeedToAdd) {
						var cuow = UnitOfWorkFactory.CreateForRoot <Counterparty> (
							           CounterpartyRepository.GetCounterpartyByINN (uow, doc.RecipientInn).Id);
						if (!cuow.Root.Accounts.Any (acc => acc.Number == doc.RecipientCheckingAccount)) {
							cuow.Root.Accounts.Add (new Account () {
								Number = doc.RecipientCheckingAccount,
								InBank = BankRepository.GetBankByBik (uow, doc.RecipientBik)
							});
							cuow.Save ();	
						}
						documents.SetValue (iter, (int)Columns.RecipientAccountColorCol, OddRowColor);
					}
				} while (documents.IterNext (ref iter));
//			} catch (Exception ex) {
//				progressBar.Text = "Произошла ошибка!";
//				progressBar.Fraction = 0;
//				throw ex;
//			}
			progressBar.Text = "Исправление/создание недостающих объектов завершено";
		}

		protected void FillDocuments ()
		{
			uow = UnitOfWorkFactory.CreateWithoutRoot ();
			TreeIter iter;
			progressBar.Fraction = 0;
			progressBar.Text = "Загружаем выгрузку из банк-клиента... Операция 2/2";
			double progressStep = 1.0 / rowsCount;

			if (!documents.GetIterFirst (out iter))
				return;
				do {
					//Шевелим прогрессбаром. I'd like to move it, move it.
					if (progressBar.Fraction + progressStep > 1)
						progressBar.Fraction = 1;
					else
						progressBar.Fraction += progressStep;
					while (Application.EventsPending ())
						Application.RunIteration ();

					//Проверяем галочку.
					if (!(bool)documents.GetValue (iter, (int)Columns.CheckCol))
						continue;
					//Получаем документ
					var doc = (TransferDocument)documents.GetValue (iter, (int)Columns.TransferDocumentCol);
					var organization = OrganizationRepository.GetOrganizationByInn (uow, doc.RecipientInn);
					//Мы платим
					if (organization == null) {
						if (!AccountExpenseRepository.AccountExpenseExists (uow, doc.Date.Year, Int32.Parse (doc.Number), doc.PayerCheckingAccount)) {
							organization = OrganizationRepository.GetOrganizationByInn (uow, doc.PayerInn);
							var expenseUoW = UnitOfWorkFactory.CreateWithNewRoot <AccountExpense> ();
							expenseUoW.Root.Number = Int32.Parse (doc.Number);
							expenseUoW.Root.Date = doc.Date;
							expenseUoW.Root.Total = doc.Total;
							expenseUoW.Root.Description = doc.PaymentPurpose;
							expenseUoW.Root.Organization = organization;
							expenseUoW.Root.OrganizationAccount = organization.Accounts.First (acc => acc.Number == doc.PayerCheckingAccount);
							expenseUoW.Root.Counterparty = CounterpartyRepository.GetCounterpartyByINN (expenseUoW, doc.RecipientInn);
							if (expenseUoW.Root.Counterparty == null) {
								expenseUoW.Root.Employee = EmployeeRepository.GetEmployeeByINNAndAccount (expenseUoW, doc.RecipientInn, doc.RecipientCheckingAccount);
								expenseUoW.Root.EmployeeAccount = expenseUoW.Root.Employee.Accounts.First (acc => acc.Number == doc.RecipientCheckingAccount);
							} else {
								expenseUoW.Root.CounterpartyAccount = expenseUoW.Root.Counterparty.Accounts.First (acc => acc.Number == doc.RecipientCheckingAccount);
							}
							expenseUoW.Root.Category = (ExpenseCategory)documents.GetValue (iter, (int)Columns.CategoryObject);
							expenseUoW.Save ();
						}
					} 
					//Нам платят
					else {
						if (!AccountIncomeRepository.AccountIncomeExists (uow, doc.Date.Year, Int32.Parse (doc.Number), doc.PayerInn, doc.PayerCheckingAccount)) {
							var incomeUoW = UnitOfWorkFactory.CreateWithNewRoot <AccountIncome> ();
							incomeUoW.Root.Number = Int32.Parse (doc.Number);
							incomeUoW.Root.Date = doc.Date;
							incomeUoW.Root.Total = doc.Total;
							incomeUoW.Root.Description = doc.PaymentPurpose;
						var counterparty = CounterpartyRepository.GetCounterpartyByINN (incomeUoW, doc.PayerInn);
						if(counterparty == null)
							counterparty  = CounterpartyRepository.GetCounterpartyByAccount (incomeUoW, doc.PayerAccount);
							
							incomeUoW.Root.Counterparty = counterparty;
							incomeUoW.Root.CounterpartyAccount = incomeUoW.Root.Counterparty.Accounts.First (acc => acc.Number == doc.PayerCheckingAccount);
							incomeUoW.Root.Organization = organization;
							incomeUoW.Root.OrganizationAccount = organization.Accounts.First (acc => acc.Number == doc.RecipientCheckingAccount);
							incomeUoW.Root.Category = (IncomeCategory)documents.GetValue (iter, (int)Columns.CategoryObject);
							incomeUoW.Save ();
						}
					}
				} while (documents.IterNext (ref iter));
			progressBar.Text = "Загрузка завершена успешно";
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			this.OnCloseTab (false);
		}

		protected void OnCheckButtonAllToggled (object sender, EventArgs e)
		{
			if (!checkButtonAll.HasFocus)
				return;
			TreeIter iter;
			if (!documents.GetIterFirst (out iter))
				return;
			do {
				documents.SetValue (iter, (int)Columns.CheckCol, checkButtonAll.Active);
			} while (documents.IterNext (ref iter));
		}

		protected string TryGetOrganizationType (string name)
		{
			foreach (var pair in InformationHandbook.OrganizationTypes) {
				string pattern = String.Format (@".*(^|\(|\s|\W|['""]){0}($|\)|\s|\W|['""]).*", pair.Key);
				string fullPattern = String.Format (@".*(^|\(|\s|\W|['""]){0}($|\)|\s|\W|['""]).*", pair.Value);
				Regex regex = new Regex (pattern, RegexOptions.IgnoreCase);
				if (regex.IsMatch (name))
					return pair.Key;
				regex = new Regex (fullPattern, RegexOptions.IgnoreCase);
				if (regex.IsMatch (name))
					return pair.Key;
			}
			return null;
		}

		protected void DeselectSameOrganizations (string INN, string name)
		{
			TreeIter iter;
			if (!documents.GetIterFirst (out iter))
				return;
			do {
				var document = documents.GetValue (iter, (int)Columns.TransferDocumentCol) as TransferDocument;
				if (document.PayerInn == INN
				    && document.PayerName == name
				    && (documents.GetValue (iter, (int)Columns.PayerNameColorCol) as string) == NeedToAdd)
					documents.SetValue (iter, (int)Columns.PayerNameColorCol, OddRowColor);
				if (document.RecipientInn == INN
				    && document.RecipientName == name
				    && (documents.GetValue (iter, (int)Columns.RecipientNameColorCol) as string) == NeedToAdd)
					documents.SetValue (iter, (int)Columns.RecipientNameColorCol, EvenRowColor);
			} while (documents.IterNext (ref iter));
			while (Application.EventsPending ())
				Application.RunIteration ();
		}

		protected void DeselectSameBanks (string bik)
		{
			TreeIter iter;
			if (!documents.GetIterFirst (out iter))
				return;
			do {
				var document = documents.GetValue (iter, (int)Columns.TransferDocumentCol) as TransferDocument;
				if (document.PayerBik == bik
				    && (documents.GetValue (iter, (int)Columns.PayerBankColorCol) as string) == NeedToAdd)
					documents.SetValue (iter, (int)Columns.PayerBankColorCol, OddRowColor);
				if (document.RecipientBik == bik
				    && (documents.GetValue (iter, (int)Columns.RecipientBankColorCol) as string) == NeedToAdd)
					documents.SetValue (iter, (int)Columns.RecipientBankColorCol, EvenRowColor);
			} while (documents.IterNext (ref iter));
			while (Application.EventsPending ())
				Application.RunIteration ();
		}
	}
}