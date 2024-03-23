using Gamma.Binding.Core;
using Gtk;
using Pango;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Infrastructure;
using VodovozInfrastructure.Utils;

namespace Vodovoz.ViewWidgets.GtkUI
{
	[ToolboxItem(true)]
	public partial class RestrictedDatePicker : Bin
	{
		public static int? CalendarFontSize;

		private readonly Gdk.Color _dangerTextHtmlColor = GdkColors.DangerText;

		private DateTime? _date = null;
		private Dialog _editDateDialog;
		private bool _autoSeparation = true;
		private Func<IEnumerable<DateTime>> _buttonsDatesLoaderFunc;

		public RestrictedDatePicker()
		{
			Build();

			Binding = new BindingControler<RestrictedDatePicker>(this, new Expression<Func<RestrictedDatePicker, object>>[] {
				(w => w.Date),
				(w => w.DateOrNull),
				(w => w.DateText),
				(w => w.IsEmpty)
			});

			yentryDate.FocusInEvent += (s, e) => OnEntryDateFocusInEvent(s, e);
			yentryDate.FocusOutEvent += (s, e) => OnEntryDateFocusOutEvent(s, e);
			yentryDate.Changed += (s, e) => OnEntryDateChanged(s, e);
			yentryDate.TextInserted += (s, e) => OnEntryDateTextInserted(s, e);
			yentryDate.Activated += (s, e) => OnEntryDateActivated(s, e);
		}

		public event EventHandler DateChanged;
		public event EventHandler DateChangedByUser;

		public BindingControler<RestrictedDatePicker> Binding { get; private set; }

		public bool HideCalendarButton
		{
			get => !ybuttonEditDate.Visible;
			set => ybuttonEditDate.Visible = !value;
		}

		public string DateText => yentryDate.Text;

		public DateTime? DateOrNull
		{
			get => _date;
			set
			{
				if(_date == value)
				{
					return;
				}

				_date = value;

				EntrySetDateTime(_date);
				OnDateChanged();
			}
		}

		public DateTime Date
		{
			get => _date.GetValueOrDefault();
			set
			{
				if(value == default)
				{
					DateOrNull = null;
				}
				else
				{
					DateOrNull = value;
				}
			}
		}

		public bool IsEmpty => !_date.HasValue;

		[DefaultValue(true)]
		public bool IsEditable
		{
			get => yentryDate.IsEditable;
			set
			{
				yentryDate.IsEditable = value;
				ybuttonEditDate.Sensitive = value;
			}
		}

		[DefaultValue(true)]
		public bool AutoSeparation
		{
			get => _autoSeparation;
			set => _autoSeparation = value;
		}

		public Func<IEnumerable<DateTime>> ButtonsDatesLoaderFunc
		{
			get => _buttonsDatesLoaderFunc;
			set
			{
				if(_buttonsDatesLoaderFunc == value)
				{
					return;
				}

				_buttonsDatesLoaderFunc = value;
			}
		}

		protected void OnButtonEditDateClicked(object sender, EventArgs e)
		{
			Window parentWin = (Window)Toplevel;

			_editDateDialog = new Dialog(
				"Укажите дату",
				parentWin,
				DialogFlags.DestroyWithParent
			)
			{
				HeightRequest = 260,
				WidthRequest = 200,
				Modal = true
			};

			_editDateDialog.VBox.Add(GetButtonsVbox());

			_editDateDialog.AddButton("Отмена", ResponseType.Cancel);

			_editDateDialog.ShowAll();
			_editDateDialog.Run();

			_editDateDialog.Destroy();
		}

		private VBox GetButtonsVbox()
		{
			var vboxButtons = new VBox();

			var dates = ButtonsDatesLoaderFunc?.Invoke() ?? new List<DateTime>();

			if(dates.Count() > 0)
			{
				var date = dates.Take(1).First();

				var firstDateButton = new Button
				{
					Label = GetDateString(date)
				};

				firstDateButton.Clicked +=
					(s, ev) => OnDateSelected(new DateSelectedEventArgs { Date = date });

				vboxButtons.Add(firstDateButton);
			}

			if(dates.Count() > 1)
			{
				var date = dates.Skip(1).Take(1).First();

				var seconDatedButton = new Button
				{
					Label = GetDateString(date)
				};

				seconDatedButton.Clicked +=
					(s, ev) => OnDateSelected(new DateSelectedEventArgs { Date = date });

				vboxButtons.Add(seconDatedButton);
			}

			var selectDateFromCalendarButton = new Button
			{
				Label = $"Другая дата"
			};

			selectDateFromCalendarButton.Clicked += (s, ev) => OnSelectDateFromCalendarButtonClicked();

			vboxButtons.Add(selectDateFromCalendarButton);

			return vboxButtons;
		}

		private string GetDateString(DateTime date)
		{
			var dayName = GeneralUtils.GetDayNameByDate(date, true);

			return $"{dayName} {date: dd.MM.yyyy}";
		}

		private void OnDateSelected(DateSelectedEventArgs dateSelectedEventArgs)
		{
			_editDateDialog.Destroy();
			DateOrNull = dateSelectedEventArgs.Date;
			OnDateChangedByUser();
		}

		protected void OnSelectDateFromCalendarButtonClicked()
		{
			_editDateDialog.Destroy();

			OpenCalendarWindow();
		}

		private void OpenCalendarWindow()
		{
			if(!(Toplevel is Window parentWin))
			{
				return;
			}

			_editDateDialog = new Dialog(
				"Укажите дату",
				parentWin,
				DialogFlags.DestroyWithParent)
			{
				Modal = true
			};

			_editDateDialog.AddButton("Отмена", ResponseType.Cancel);
			_editDateDialog.AddButton("Ok", ResponseType.Ok);

			var calendar = new Gtk.Calendar();

			calendar.DisplayOptions =
				CalendarDisplayOptions.ShowHeading |
				CalendarDisplayOptions.ShowDayNames |
				CalendarDisplayOptions.ShowWeekNumbers;

			calendar.DaySelectedDoubleClick += OnCalendarDaySelectedDoubleClick;
			calendar.Date = _date ?? DateTime.Now.Date;

			if(CalendarFontSize.HasValue)
			{
				var desc = new FontDescription { AbsoluteSize = CalendarFontSize.Value * 1000 };
				calendar.ModifyFont(desc);
			}

			_editDateDialog.VBox.Add(calendar);
			_editDateDialog.ShowAll();

			int response = _editDateDialog.Run();

			if(response == (int)ResponseType.Ok)
			{
				DateOrNull = calendar.GetDate();
				OnDateChangedByUser();
			}

			calendar.Destroy();
			_editDateDialog.Destroy();
		}

		public void Clear()
		{
			DateOrNull = null;
		}

		protected virtual void OnDateChanged()
		{
			Binding.FireChange(new Expression<Func<RestrictedDatePicker, object>>[] {
				(w => w.Date),
				(w => w.DateOrNull),
				(w => w.DateText),
				(w => w.IsEmpty)
			});

			DateChanged?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnDateChangedByUser()
		{
			DateChangedByUser?.Invoke(this, EventArgs.Empty);
		}

		protected void OnEntryDateFocusInEvent(object o, FocusInEventArgs args)
		{
			yentryDate.SelectRegion(0, 10);
		}

		protected void OnEntryDateFocusOutEvent(object o, FocusOutEventArgs args)
		{
			if(yentryDate.Text == "")
			{
				DateOrNull = null;
				OnDateChangedByUser();
				return;
			}

			if(DateTime.TryParse(yentryDate.Text, out DateTime outDate))
			{
				DateOrNull = outDate;
				OnDateChangedByUser();
			}
			else
			{
				EntrySetDateTime(DateOrNull);
			}
		}

		void EntrySetDateTime(DateTime? date)
		{
			if(date.HasValue)
			{
				yentryDate.Text = date.Value.ToShortDateString();
			}
			else
			{
				yentryDate.Text = string.Empty;
			}
		}

		protected void OnEntryDateChanged(object sender, EventArgs e)
		{
			if(DateTime.TryParse(yentryDate.Text, out DateTime outDate))
			{
				yentryDate.ModifyText(StateType.Normal);
			}
			else
			{
				yentryDate.ModifyText(StateType.Normal, _dangerTextHtmlColor);
			}
		}

		protected void OnCalendarDaySelectedDoubleClick(object sender, EventArgs e)
		{
			_editDateDialog.Respond(ResponseType.Ok);
		}

		protected void OnEntryDateTextInserted(object o, TextInsertedArgs args)
		{
			if(!_autoSeparation)
			{
				return;
			}

			if(args.Length == 1 &&
			   (args.Position == 3 || args.Position == 6) &&
			   args.Text != System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator &&
			   args.Position == yentryDate.Text.Length)
			{
				int Pos = args.Position - 1;
				yentryDate.InsertText(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, ref Pos);
				args.Position++;
			}
		}

		protected void OnEntryDateActivated(object sender, EventArgs e)
		{
			ChildFocus(DirectionType.TabForward);
		}

		public new void ModifyBase(StateType state, Gdk.Color color)
		{
			yentryDate.ModifyBase(state, color);
		}

		class DateSelectedEventArgs : EventArgs
		{
			public DateTime Date { get; set; }
		}
	}
}
