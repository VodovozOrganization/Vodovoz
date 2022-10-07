using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Fias.Service.Loaders;
using Gamma.Binding.Core;
using Gdk;
using GLib;
using Gtk;
using Key = Gdk.Key;

namespace VodovozFiasWidgets
{
	[ToolboxItem(true)]
	[Category("Vodovoz Fias Widgets")]
	public class StreetEntry : Entry
	{
		private ListStore _completionListStore;
		private Guid? _fiasGuid;
		private bool _firstCompletion = true;
		private string _streetName;
		private string _streetDistrict;
		private string _streetTypeName;
		private string _streetTypeNameShort;
		private IStreetsDataLoader _streetsDataLoader;

		public StreetEntry()
		{
			Binding = new BindingControler<StreetEntry>(this, new Expression<Func<StreetEntry, object>>[]
			{
				w => w.FiasGuid, w => w.StreetName, w => w.StreetDistrict, w => w.StreetTypeName, w => w.StreetTypeNameShort
			});

			Completion = new EntryCompletion();
			Completion.MinimumKeyLength = 0;
			Completion.MatchSelected += Completion_MatchSelected;
			Completion.MatchFunc = (completion, key, iter) => true;
			var cell = new CellRendererText();
			Completion.PackStart(cell, true);
			Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);
		}

		private void UpdateText()
		{
			Text = GenerateEntryText();
		}

		private string GenerateEntryText()
		{
			var street = string.Empty;

			if(!string.IsNullOrWhiteSpace(StreetTypeName))
			{
				street += StreetTypeName;
			}

			if(!string.IsNullOrWhiteSpace(StreetName))
			{
				street += $" { StreetName }";
			}

			if(!string.IsNullOrWhiteSpace(StreetDistrict))
			{
				street += $" ({ StreetDistrict })";
			}

			return street;
		}

		private void EntryTextChanges(object o, TextInsertedArgs args)
		{
			if(CityGuid != null)
			{
				if(_firstCompletion)
				{
					EmptyCompletion();
					_firstCompletion = false;
				}

				_streetsDataLoader.LoadStreets(CityGuid, Text);
			}
		}

		private void EntryTextChanges(object o, TextDeletedArgs args)
		{
			EntryTextChanges(o, EventArgs.Empty as TextInsertedArgs);
		}

		private void OnCellLayoutDataFunc(CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var streetName = (string)tree_model.GetValue(iter, (int)columns.StreetName);
			var streetTypeName = (string)tree_model.GetValue(iter, (int)columns.StreetTypeName);
			var streetDistrict = (string)tree_model.GetValue(iter, (int)columns.StreeDistrict);
			var pattern = string.Format("\\b{0}", Regex.Escape(Text.ToLower()));
			streetName = Regex.Replace(streetName, pattern, match => $"<b>{ match.Value }</b>", RegexOptions.IgnoreCase);

			if(!string.IsNullOrWhiteSpace(streetDistrict))
			{
				streetDistrict = $"({ streetDistrict })";
			}

			((CellRendererText) cell).Markup =
				string.IsNullOrWhiteSpace(streetDistrict) ? $"{ streetTypeName.ToLower() } { streetName }" :
					$"{ streetTypeName.ToLower() } { streetName } { streetDistrict }";
		}

		[ConnectBefore]
		private void Completion_MatchSelected(object o, MatchSelectedArgs args)
		{
			if(args.Model.GetValue(args.Iter, (int)columns.StreetName).ToString() == "Загрузка...")
			{
				args.RetVal = false;
				return;
			}

			FiasGuid = (Guid)args.Model.GetValue(args.Iter, (int)columns.FiasGuid);
			StreetName = args.Model.GetValue(args.Iter, (int)columns.StreetName).ToString();
			StreetDistrict = args.Model.GetValue(args.Iter, (int)columns.StreeDistrict)?.ToString();
			StreetTypeName = args.Model.GetValue(args.Iter, (int)columns.StreetTypeName).ToString();
			StreetTypeNameShort = args.Model.GetValue(args.Iter, (int)columns.StreetTypeNameShort).ToString();

			FireStreetChange();
			args.RetVal = true;
		}

		private void ChangeDataLoader(IStreetsDataLoader oldValue, IStreetsDataLoader newValue)
		{
			if(oldValue == newValue)
			{
				return;
			}

			if(oldValue != null)
			{
				oldValue.StreetLoaded -= StreetLoaded;
				TextInserted -= EntryTextChanges;
				TextDeleted -= EntryTextChanges;
			}

			_streetsDataLoader = newValue;
			if(StreetsDataLoader == null)
			{
				return;
			}

			StreetsDataLoader.StreetLoaded += StreetLoaded;
			TextInserted += EntryTextChanges;
			TextDeleted += EntryTextChanges;
		}

		private void EmptyCompletion()
		{
			_completionListStore = new ListStore(typeof(Guid), typeof(string), typeof(string), typeof(string));
			_completionListStore.AppendValues("", "Загрузка...", "");
			Completion.Model = _completionListStore;
			Completion.Complete();
		}

		private void StreetLoaded()
		{
			var streets = _streetsDataLoader.GetStreets();
			_completionListStore = new ListStore(typeof(Guid), typeof(string), typeof(string), typeof(string), typeof(string));

			foreach(var s in streets)
			{
				_completionListStore.AppendValues(
					s.FiasGuid,
					s.Name,
					s.TypeName,
					s.TypeShortName,
					s.StreetDistrict
				);
			}

			Application.Invoke((sender, e) =>
			{
				if(Completion != null)
				{
					Completion.Model = _completionListStore;
					if(HasFocus)
					{
						Completion.Complete();
					}
				}
			});
		}

		//Костыль, для отображения выпадающего списка
		protected override bool OnKeyPressEvent(EventKey evnt)
		{
			if(evnt.Key == Key.Control_R)
			{
				InsertText("");
			}

			return base.OnKeyPressEvent(evnt);
		}

		protected override bool OnFocusOutEvent(EventFocus evnt)
		{
			if(Text != GenerateEntryText())
			{
				StreetName = Text;
				FiasGuid = null;
				StreetTypeName = null;
				StreetTypeNameShort = null;
				StreetDistrict = null;
			}

			return base.OnFocusOutEvent(evnt);
		}

		protected virtual void OnStreetSelected()
		{
			StreetSelected?.Invoke(null, EventArgs.Empty);
		}

		protected override void OnDestroyed()
		{
			if(StreetsDataLoader != null)
			{
				StreetsDataLoader.StreetLoaded -= StreetLoaded;
			}

			base.OnDestroyed();
		}

		public IStreetsDataLoader StreetsDataLoader
		{
			get => _streetsDataLoader;
			set => ChangeDataLoader(_streetsDataLoader, value);
		}

		public BindingControler<StreetEntry> Binding { get; }

		public event EventHandler StreetSelected;

		public Guid? CityGuid { get; set; }

		public string StreetName
		{
			get => _streetName;
			set
			{
				_streetName = value;
				Binding.FireChange(w => w.StreetName);
			}
		}

		public Guid? FiasGuid
		{
			get => _fiasGuid;
			private set
			{
				_fiasGuid = value;
				Binding.FireChange(w => w.FiasGuid);
			}
		}

		public string StreetTypeName
		{
			get => _streetTypeName;
			set
			{
				_streetTypeName = value;
				Binding.FireChange(w => w.StreetTypeName);
			}
		}

		public string StreetTypeNameShort
		{
			get => _streetTypeNameShort;
			set
			{
				_streetTypeNameShort = value;
				Binding.FireChange(w => w.StreetTypeNameShort);
			}
		}

		public string StreetDistrict
		{
			get => _streetDistrict;
			set
			{
				_streetDistrict = value;
				Binding.FireChange(w => w.StreetDistrict);
			}
		}

		public void FireStreetChange()
		{
			UpdateText();
			OnStreetSelected();
		}

		private enum columns
		{
			FiasGuid,
			StreetName,
			StreetTypeName,
			StreetTypeNameShort,
			StreeDistrict
		}
	}
}
