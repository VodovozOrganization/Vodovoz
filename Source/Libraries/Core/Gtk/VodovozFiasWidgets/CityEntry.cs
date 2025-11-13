using Gamma.Binding.Core;
using Gdk;
using GLib;
using Gtk;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Fias.Client.Loaders;
using Fias.Search.DTO;
using Key = Gdk.Key;

namespace VodovozFiasWidgets
{
	[ToolboxItem(true)]
	[Category("Vodovoz Fias Widgets")]
	public class CityEntry : Entry
	{
		private ICitiesDataLoader _citiesDataLoader;
		private Guid? _fiasGuid;
		private string _cityName;
		private string _cityTypeName;
		private string _cityTypeNameShort;
		private ListStore _completionListStore;

		public CityEntry()
		{
			Binding = new BindingControler<CityEntry>(this, new Expression<Func<CityEntry, object>>[]
			{
				w => w.FiasGuid,  w => w.CityTypeName, w => w.CityTypeNameShort, w => w.CityName
			});

			Completion = new EntryCompletion();
			Completion.MinimumKeyLength = 0;
			Completion.MatchSelected += Completion_MatchSelected;
			Completion.MatchFunc = (completion, key, iter) => true;
			var cell = new CellRendererText();
			Completion.PackStart(cell, true);
			Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);

			FocusOutEvent += OnFocusOutEvent;
		}

		private void UpdateText()
		{
			string cityText = string.Empty;

			if(!string.IsNullOrWhiteSpace(CityTypeName))
			{
				cityText += CityTypeName;
			}

			if(!string.IsNullOrWhiteSpace(CityName))
			{
				if(cityText != string.Empty)
				{
					cityText += " ";
				}

				cityText += CityName;
			}

			Text = cityText;
		}

		private void UpdateFromFias()
		{
			var city = string.IsNullOrWhiteSpace(_cityName) ? null : _citiesDataLoader.GetCity(_cityName);

			if(city == null)
			{
				FiasGuid = null;
				CityTypeName = null;
				CityTypeNameShort = null;
			}
			else
			{
				FiasGuid = city.FiasGuid;
				CityName = city.Name;
				CityTypeName = city.TypeName;
				CityTypeNameShort = city.TypeShortName;
			}
		}

		private void OnCellLayoutDataFunc(CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var cityName = (string)tree_model.GetValue(iter, (int)Columns.Name);
			var typeName = (string)tree_model.GetValue(iter, (int)Columns.TypeName);
			var pattern = $"\\b{Regex.Escape(Text.ToLower())}";
			cityName = Regex.Replace(cityName, pattern, match => $"<b>{ match.Value }</b>", RegexOptions.IgnoreCase);
			((CellRendererText)cell).Markup = $"{typeName} {cityName}";
		}

		[ConnectBefore]
		private void Completion_MatchSelected(object o, MatchSelectedArgs args)
		{
			CityName = args.Model.GetValue(args.Iter, (int)Columns.Name).ToString();
			CityTypeName = args.Model.GetValue(args.Iter, (int)Columns.TypeName).ToString();
			CityTypeNameShort = args.Model.GetValue(args.Iter, (int)Columns.TypeNameShort).ToString();
			FiasGuid = (Guid)args.Model.GetValue(args.Iter, (int)Columns.FiasGuid);
			FireCityChange();
			args.RetVal = true;
		}

		private void OnFocusOutEvent(object o, FocusOutEventArgs args)
		{
			if(AcceptUnknownCities
			   && (
				   !Text.ToLower().Contains(CityName.ToLower())
			       || string.IsNullOrWhiteSpace(CityName)
			       || FiasGuid is null
				   )
			   )
			{
				CityName = Text;
				UpdateFromFias();
				FireCityChange();
			}
		}

		private void EntryTextChanges(object o, TextInsertedArgs args)
		{
			_citiesDataLoader.LoadCities(Text);
		}

		private void EntryTextChanges(object o, TextDeletedArgs args)
		{
			EntryTextChanges(o, EventArgs.Empty as TextInsertedArgs);
		}

		private void ChangeDataLoader(ICitiesDataLoader oldValue, ICitiesDataLoader newValue)
		{
			if(oldValue == newValue)
			{
				return;
			}

			if(oldValue != null)
			{
				oldValue.CitiesLoaded -= CitiesLoaded;
				TextInserted -= EntryTextChanges;
				TextDeleted -= EntryTextChanges;
			}

			_citiesDataLoader = newValue;

			if(CitiesDataLoader == null)
			{
				return;
			}

			CitiesDataLoader.CitiesLoaded += CitiesLoaded;
			TextInserted += EntryTextChanges;
			TextDeleted += EntryTextChanges;
		}

		private void CitiesLoaded()
		{
			Application.Invoke((senderObject, eventArgs) =>
			{
				var cities = _citiesDataLoader.GetCities();
				_completionListStore = new ListStore(typeof(Guid), typeof(string), typeof(string), typeof(string));

				foreach(var city in cities)
				{
					_completionListStore.AppendValues(
						city.FiasGuid,
						city.TypeName,
						city.TypeShortName,
						city.Name
					);
				}

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

		private enum Columns
		{
			FiasGuid,
			TypeName,
			TypeNameShort,
			Name
		}

		protected virtual void OnCitySelected()
		{
			CitySelected?.Invoke(null, EventArgs.Empty);
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

		protected override void OnDestroyed()
		{
			if(CitiesDataLoader != null)
			{
				_citiesDataLoader.CitiesLoaded -= CitiesLoaded;
			}

			base.OnDestroyed();
		}

		public bool AcceptUnknownCities { get; set; }
		public BindingControler<CityEntry> Binding { get; }
		public event EventHandler CitySelected;

		public ICitiesDataLoader CitiesDataLoader
		{
			get => _citiesDataLoader;
			set => ChangeDataLoader(_citiesDataLoader, value);
		}

		public Guid? FiasGuid
		{
			get => _fiasGuid;
			set
			{
				_fiasGuid = value;
				Binding.FireChange(w => w.FiasGuid);
			}
		}

		public string CityName
		{
			get => _cityName;
			set
			{
				_cityName = value;
				Binding.FireChange(w => w.CityName);
			}
		}

		public string CityTypeName
		{
			get => _cityTypeName;
			set
			{
				_cityTypeName = value;
				Binding.FireChange(w => w.CityTypeName);
			}
		}

		public string CityTypeNameShort
		{
			get => _cityTypeNameShort;
			set
			{
				_cityTypeNameShort = value;
				Binding.FireChange(w => w.CityTypeNameShort);
			}
		}

		public void FireCityChange()
		{
			UpdateText();
			if(FiasGuid == null)
			{
				UpdateFromFias();
			}
			OnCitySelected();
		}
	}
}
