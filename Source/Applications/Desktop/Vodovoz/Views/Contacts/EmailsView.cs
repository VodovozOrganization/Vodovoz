using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Widgets;
using Gtk;
using NLog;
using QS.DomainModel.UoW;
using QSWidgetLib;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Views.Contacts
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmailsView : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private IUnitOfWork uow;
		public GenericObservableList<Email> EmailsList;
		private IList<EmailType> emailTypes;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				if(uow != null) {
					emailTypes = UoW.GetAll<EmailType>().ToList();
				}
			}
		}

		private IList<Email> emails;
		public IList<Email> Emails {
			get {
				return emails;
			}
			set {
				if(emails == value)
					return;
				if(EmailsList != null)
					CleanList();
				emails = value;
				buttonAdd.Sensitive = emails != null;
				if(value != null) {
					EmailsList = new GenericObservableList<Email>(emails);
					EmailsList.ElementAdded += OnEmailListElementAdded;
					EmailsList.ElementRemoved += OnEmailListElementRemoved;

					foreach(Email email in EmailsList)
						AddEmailRow(email);
				}
			}
		}

		void OnEmailListElementRemoved(object aList, int[] aIdx, object aObject)
		{
			Widget foundWidget = null;
			foreach(Widget wid in datatableEmails.AllChildren) {
				if(wid is yValidatedEntry && (wid as yValidatedEntry).Tag == aObject) {
					foundWidget = wid;
					break;
				}
			}
			if(foundWidget == null) {
				logger.Warn("Не найден виджет ассоциированный с удаленным телефоном.");
				return;
			}

			Table.TableChild child = ((Table.TableChild)(this.datatableEmails[foundWidget]));
			RemoveRow(child.TopAttach);
		}

		void OnEmailListElementAdded(object aList, int[] aIdx)
		{
			foreach(int i in aIdx) {
				AddEmailRow(EmailsList[i]);
			}
		}

		uint RowNum;

		public EmailsView()
		{
			this.Build();
			datatableEmails.NRows = RowNum = 0;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			EmailsList.Add(new Email());
		}

		private void AddEmailRow(Email newEmail)
		{
			datatableEmails.NRows = RowNum + 1;

			var emailDataCombo = new yListComboBox();
			emailDataCombo.WidthRequest = 100;
			emailDataCombo.SetRenderTextFunc((EmailType x) => x.Name);
			emailDataCombo.ItemsList = emailTypes;
			emailDataCombo.Binding.AddBinding(newEmail, e => e.EmailType, w => w.SelectedItem).InitializeFromSource();
			datatableEmails.Attach(emailDataCombo, (uint)0, (uint)1, RowNum, RowNum + 1, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);

			yValidatedEntry emailDataEntry = new yValidatedEntry();
			emailDataEntry.ValidationMode = ValidationType.email;
			emailDataEntry.Tag = newEmail;
			emailDataEntry.Binding.AddBinding(newEmail, e => e.Address, w => w.Text).InitializeFromSource();
			datatableEmails.Attach(emailDataEntry, (uint)1, (uint)2, RowNum, RowNum + 1, AttachOptions.Expand | AttachOptions.Fill, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Button deleteButton = new Gtk.Button();
			Gtk.Image image = new Gtk.Image();
			image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-delete", global::Gtk.IconSize.Menu);
			deleteButton.Image = image;
			deleteButton.Clicked += OnButtonDeleteClicked;
			datatableEmails.Attach(deleteButton, (uint)2, (uint)3, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			datatableEmails.ShowAll();

			RowNum++;
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			Table.TableChild delButtonInfo = ((Table.TableChild)(this.datatableEmails[(Widget)sender]));
			yValidatedEntry foundWidget = null;
			foreach(Widget wid in datatableEmails.AllChildren) {
				if(wid is yValidatedEntry && delButtonInfo.TopAttach == (datatableEmails[wid] as Table.TableChild).TopAttach) {
					foundWidget = (yValidatedEntry)wid;
					break;
				}
			}
			if(foundWidget == null) {
				logger.Warn("Не найден виджет ассоциированный с удаленным телефоном.");
				return;
			}
			EmailsList.Remove((Email)foundWidget.Tag);
		}

		private void RemoveRow(uint Row)
		{
			foreach(Widget w in datatableEmails.Children)
				if(((Table.TableChild)(this.datatableEmails[w])).TopAttach == Row) {
					datatableEmails.Remove(w);
					w.Destroy();
				}
			for(uint i = Row + 1; i < datatableEmails.NRows; i++)
				MoveRowUp(i);
			datatableEmails.NRows = --RowNum;
		}

		protected void MoveRowUp(uint Row)
		{
			foreach(Widget w in datatableEmails.Children)
				if(((Table.TableChild)(this.datatableEmails[w])).TopAttach == Row) {
					uint Left = ((Table.TableChild)(this.datatableEmails[w])).LeftAttach;
					uint Right = ((Table.TableChild)(this.datatableEmails[w])).RightAttach;
					datatableEmails.Remove(w);
					if(w.GetType() == typeof(yListComboBox))
						datatableEmails.Attach(w, Left, Right, Row - 1, Row, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);
					else
						datatableEmails.Attach(w, Left, Right, Row - 1, Row, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);
				}
		}

		private void CleanList()
		{
			while(EmailsList.Count > 0) {
				EmailsList.RemoveAt(0);
			}
		}

		/// <summary>
		/// Необходимо выполнить перед сохранением или в геттере HasChanges
		/// </summary>
		public void RemoveEmpty()
		{
			Emails.Where(p => string.IsNullOrWhiteSpace(p.Address))
				.ToList()
				.ForEach(p => Emails.Remove(p));
		}
	}
}
