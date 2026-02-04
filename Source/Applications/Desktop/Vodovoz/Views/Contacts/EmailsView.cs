using Gamma.Widgets;
using Gtk;
using NLog;
using QS.Views.GtkUI;
using QSWidgetLib;
using System;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Contacts;
using Vodovoz.ViewModels.ViewModels.Contacts;

namespace Vodovoz.Views.Contacts
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmailsView : WidgetViewBase<EmailsViewModel>
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private uint _rowNum;

		public EmailsView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			ViewModel.EmailsList.ElementAdded += OnEmailListElementAdded;
			ViewModel.EmailsList.ElementRemoved += OnEmailListElementRemoved;

			if(ViewModel.EmailsList.Any())
			{
				foreach(Email email in ViewModel.ActiveEmails)
				{
					AddEmailRow(email);
				}
			}
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddEmailCommand.Execute();
		}

		private void AddEmailRow(Email newEmail)
		{
			datatableEmails.NRows = _rowNum + 1;

			newEmail.PropertyChanged += OnEmailPropertyChanged;

			var emailDataCombo = new yListComboBox();
			emailDataCombo.WidthRequest = 100;
			emailDataCombo.SetRenderTextFunc((EmailType x) => x.Name);
			emailDataCombo.ItemsList = ViewModel.EmailTypes;
			emailDataCombo.Binding.AddBinding(newEmail, e => e.EmailType, w => w.SelectedItem).InitializeFromSource();
			datatableEmails.Attach(emailDataCombo, (uint)0, (uint)1, _rowNum, _rowNum + 1, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);

			yValidatedEntry emailDataEntry = new yValidatedEntry();
			emailDataEntry.ValidationMode = ValidationType.email;
			emailDataEntry.Tag = newEmail;
			emailDataEntry.Binding.AddBinding(newEmail, e => e.Address, w => w.Text).InitializeFromSource();
			datatableEmails.Attach(emailDataEntry, (uint)1, (uint)2, _rowNum, _rowNum + 1, AttachOptions.Expand | AttachOptions.Fill, (AttachOptions)0, (uint)0, (uint)0);

			Button deleteButton = new Button();
			Image image = new Image();
			image.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-delete", IconSize.Menu);
			deleteButton.Image = image;
			deleteButton.Clicked += OnButtonDeleteClicked;
			datatableEmails.Attach(deleteButton, (uint)2, (uint)3, _rowNum, _rowNum + 1, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);

			datatableEmails.ShowAll();

			_rowNum++;
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var delButtonInfo = (Table.TableChild)datatableEmails[(Widget)sender];
			yValidatedEntry foundWidget = null;
			foreach(Widget wid in datatableEmails.AllChildren)
			{
				if(wid is yValidatedEntry entry && delButtonInfo.TopAttach == (datatableEmails[entry] as Table.TableChild).TopAttach)
				{
					foundWidget = entry;
					break;
				}
			}
			if(foundWidget == null)
			{
				_logger.Warn("Не найден виджет ассоциированный с удаленной электронкой");
				return;
			}

			var email = (Email)foundWidget.Tag;
			
			if(ViewModel.HasExternalCounterpartiesWithEmail(email.Id))
			{
				if(ViewModel.InteractiveService.Question(
					"Данная почта привязана к пользователю МП или сайта. Вы действительно хотите ее удалить?"))
				{
					ViewModel.RemoveEmailWithAllReferencesCommand.Execute(email);
				}
			}
			else
			{
				ViewModel.RemoveEmailCommand.Execute(email);
			}
		}

		private void RemoveRow(uint Row)
		{
			foreach(Widget w in datatableEmails.Children)
			{
				if(((Table.TableChild)datatableEmails[w]).TopAttach == Row)
				{
					datatableEmails.Remove(w);
					w.Destroy();
				}
			}

			for(uint i = Row + 1; i < datatableEmails.NRows; i++)
			{
				MoveRowUp(i);
			}

			datatableEmails.NRows = --_rowNum;
		}

		protected void MoveRowUp(uint Row)
		{
			foreach(Widget w in datatableEmails.Children)
			{
				if(((Table.TableChild)datatableEmails[w]).TopAttach == Row)
				{
					uint Left = ((Table.TableChild)datatableEmails[w]).LeftAttach;
					uint Right = ((Table.TableChild)datatableEmails[w]).RightAttach;
					datatableEmails.Remove(w);
					if(w.GetType() == typeof(yListComboBox))
					{
						datatableEmails.Attach(w, Left, Right, Row - 1, Row, AttachOptions.Fill | AttachOptions.Expand, (AttachOptions)0, (uint)0, (uint)0);
					}
					else
					{
						datatableEmails.Attach(w, Left, Right, Row - 1, Row, (AttachOptions)0, (AttachOptions)0, (uint)0, (uint)0);
					}
				}
			}
		}

		void OnEmailListElementAdded(object aList, int[] aIdx)
		{
			foreach(var i in aIdx)
			{
				AddEmailRow(ViewModel.EmailsList[i]);
			}
		}
		
		private void OnEmailListElementRemoved(object aList, int[] aIdx, object aObject)
		{
			if(aObject is Email email)
			{
				email.PropertyChanged -= OnEmailPropertyChanged;
			}

			Widget foundWidget = null;
			foreach(Widget wid in datatableEmails.AllChildren)
			{
				if(wid is yValidatedEntry entry && entry.Tag == aObject)
				{
					foundWidget = entry;
					break;
				}
			}
			if(foundWidget == null)
			{
				_logger.Warn("Не найден виджет ассоциированный с удаленной электронкой");
				return;
			}

			var child = (Table.TableChild)datatableEmails[foundWidget];
			RemoveRow(child.TopAttach);
		}

		private void OnEmailPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Email.EmailType))
			{
				var email = sender as Email;
				if(email?.EmailType != null && ViewModel != null)
				{
					ViewModel.OnEmailTypeChanged(email);
				}
			}
		}
	}
}
