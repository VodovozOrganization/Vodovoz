using System;
using System.Collections.Generic;
using System.Data.Bindings;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using NHibernate;
using NLog;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Views.Contacts
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PersonsView : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private GenericObservableList<Person> PersonsList;
		public ISession Session { get; set; }

		private IList<Person> persons;
		public IList<Person> Persons {
			get {
				return persons;
			}
			set {
				if(persons == value)
					return;
				if(PersonsList != null)
					CleanList();
				persons = value;
				buttonAdd.Sensitive = persons != null;
				if(value != null) {
					PersonsList = new GenericObservableList<Person>(persons);
					PersonsList.ElementAdded += OnPersonsListElementAdded;
					PersonsList.ElementRemoved += OnPersonsListElementRemoved;
					if(PersonsList.Count == 0)
						PersonsList.Add(new Person());
					else {
						foreach(Person person in PersonsList)
							AddPersonRow(person);
					}
				}
			}
		}

		void OnPersonsListElementRemoved(object aList, int[] aIdx, object aObject)
		{
			Widget foundWidget = null;
			foreach(Widget wid in datatablePersons.AllChildren) {
				if(wid is IAdaptableContainer && (wid as IAdaptableContainer).Adaptor.Adaptor.FinalTarget == aObject) {
					foundWidget = wid;
					break;
				}
			}
			if(foundWidget == null) {
				logger.Warn("Не найден виджет ассоциированный с удаленным телефоном.");
				return;
			}

			Table.TableChild child = ((Table.TableChild)(this.datatablePersons[foundWidget]));
			RemoveRow(child.TopAttach);
		}

		void OnPersonsListElementAdded(object aList, int[] aIdx)
		{
			foreach(int i in aIdx) {
				AddPersonRow(PersonsList[i]);
			}
		}

		uint RowNum;

		public PersonsView()
		{
			this.Build();
			datatablePersons.NRows = RowNum = 0;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			PersonsList.Add(new Person());
		}

		private void AddPersonRow(Person newPerson)
		{
			datatablePersons.NRows = RowNum + 1;

			Gtk.Label labelSurame = new Gtk.Label("Фамилия:");
			datatablePersons.Attach(labelSurame, (uint)0, (uint)1, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			var nameDataEntry = new yEntry();
			nameDataEntry.Binding.AddBinding(newPerson, e => e.Lastname, w => w.Text).InitializeFromSource();
			nameDataEntry.WidthChars = 20;
			datatablePersons.Attach(nameDataEntry, (uint)1, (uint)2, RowNum, RowNum + 1, AttachOptions.Expand | AttachOptions.Fill, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Label labelName = new Gtk.Label("Имя:");
			datatablePersons.Attach(labelName, (uint)2, (uint)3, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			var surnameDataEntry = new yEntry();
			surnameDataEntry.Binding.AddBinding(newPerson, e => e.Name, w => w.Text).InitializeFromSource();
			surnameDataEntry.WidthChars = 20;
			datatablePersons.Attach(surnameDataEntry, (uint)3, (uint)4, RowNum, RowNum + 1, AttachOptions.Expand | AttachOptions.Fill, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Label labelPatronymic = new Gtk.Label("Отчество:");
			datatablePersons.Attach(labelPatronymic, (uint)4, (uint)5, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			var patronymicDataEntry = new yEntry();
			patronymicDataEntry.Binding.AddBinding(newPerson, e => e.PatronymicName, w => w.Text).InitializeFromSource();
			patronymicDataEntry.WidthChars = 20;
			datatablePersons.Attach(patronymicDataEntry, (uint)5, (uint)6, RowNum, RowNum + 1, AttachOptions.Expand | AttachOptions.Fill, (AttachOptions)0, (uint)0, (uint)0);

			Gtk.Button deleteButton = new Gtk.Button();
			Gtk.Image image = new Gtk.Image();
			image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-delete", global::Gtk.IconSize.Menu);
			deleteButton.Image = image;
			deleteButton.Clicked += OnButtonDeleteClicked;
			datatablePersons.Attach(deleteButton, (uint)6, (uint)7, RowNum, RowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			datatablePersons.ShowAll();

			RowNum++;
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			Table.TableChild delButtonInfo = ((Table.TableChild)(this.datatablePersons[(Widget)sender]));
			Widget foundWidget = null;
			foreach(Widget wid in datatablePersons.AllChildren) {
				if(wid is IAdaptableContainer && delButtonInfo.TopAttach == (datatablePersons[wid] as Table.TableChild).TopAttach) {
					foundWidget = wid;
					break;
				}
			}
			if(foundWidget == null) {
				logger.Warn("Не найден виджет ассоциированный с удаленным человеком.");
				return;
			}

			PersonsList.Remove((Person)(foundWidget as IAdaptableContainer).Adaptor.Adaptor.FinalTarget);
		}

		private void RemoveRow(uint Row)
		{
			foreach(Widget w in datatablePersons.Children)
				if(((Table.TableChild)(this.datatablePersons[w])).TopAttach == Row) {
					datatablePersons.Remove(w);
					w.Destroy();
				}
			for(uint i = Row + 1; i < datatablePersons.NRows; i++)
				MoveRowUp(i);
			datatablePersons.NRows = --RowNum;
		}

		protected void MoveRowUp(uint Row)
		{
			foreach(Widget w in datatablePersons.Children)
				if(((Table.TableChild)(this.datatablePersons[w])).TopAttach == Row) {
					uint Left = ((Table.TableChild)(this.datatablePersons[w])).LeftAttach;
					uint Right = ((Table.TableChild)(this.datatablePersons[w])).RightAttach;
					datatablePersons.Remove(w);
					datatablePersons.Attach(w, Left, Right, Row - 1, Row, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);
				}
		}

		private void CleanList()
		{
			while(PersonsList.Count > 0) {
				PersonsList.RemoveAt(0);
			}
		}

		public void SaveChanges()
		{
			PersonsList.Where(p => String.IsNullOrWhiteSpace(p.Name)
				&& String.IsNullOrWhiteSpace(p.Lastname)
				&& String.IsNullOrWhiteSpace(p.PatronymicName)
			).ToList().ForEach(p => PersonsList.Remove(p));

			foreach(Person person in PersonsList) {
				Session.SaveOrUpdate(person);
			}
		}
	}
}
