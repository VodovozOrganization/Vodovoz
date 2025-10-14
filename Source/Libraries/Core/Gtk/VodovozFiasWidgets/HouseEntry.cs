using Fias.Search.DTO;
using Gamma.Binding.Core;
using Gdk;
using GLib;
using Gtk;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Fias.Client.Loaders;
using Key = Gdk.Key;

namespace VodovozFiasWidgets
{
	[ToolboxItem(true)]
	[Category("Vodovoz Fias Widgets")]
	public class HouseEntry : Entry
	{
		private ListStore _completionListStore;
		private Guid? _fiasGuid;
		private IHousesDataLoader _housesDataLoader;

		public HouseEntry()
		{
			Binding = new BindingControler<HouseEntry>(this, new Expression<Func<HouseEntry, object>>[]
			{
				w => w.FiasGuid,
				w => w.BuildingName
			});

			Completion = new EntryCompletion();
			Completion.MinimumKeyLength = 0;
			Completion.MatchSelected += Completion_MatchSelected;
			Completion.MatchFunc = Completion_MatchFunc;
			var cell = new CellRendererText();
			Completion.PackStart(cell, true);
			Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);
		}

		private bool Completion_MatchFunc(EntryCompletion completion, string key, TreeIter iter)
		{
			var house = (HouseDTO)completion.Model.GetValue(iter, 0);
			if(house == null)
			{
				return false;
			}
			var houseName = house.ComplexNumber;
			return Regex.IsMatch(houseName, $"{Regex.Escape(Text)}", RegexOptions.IgnoreCase);
		}

		[ConnectBefore]
		private void Completion_MatchSelected(object o, MatchSelectedArgs args)
		{
			var house = (HouseDTO)args.Model.GetValue(args.Iter, 0);
			BuildingName = house.ComplexNumber;
			FiasGuid = house.FiasGuid;
			FiasHouse = house;
			args.RetVal = true;
		}


		private void OnCellLayoutDataFunc(CellLayout cellLayout, CellRenderer cell, TreeModel treeModel, TreeIter iter)
		{
			var house = (HouseDTO)treeModel.GetValue(iter, 0);
			var pattern = Regex.Escape(Text.ToLower());
			var houseFullName = house.ComplexNumber;
			houseFullName = Regex.Replace(houseFullName, pattern, match => $"<b>{ match.Value }</b>", RegexOptions.IgnoreCase);
			((CellRendererText)cell).Markup = houseFullName;
		}

		private void ChangeDataLoader(IHousesDataLoader oldValue, IHousesDataLoader newValue)
		{
			if(oldValue == newValue)
			{
				return;
			}

			if(oldValue != null)
			{
				oldValue.HousesLoaded -= HousesLoaded;
				TextInserted -= EntryTextChanges;
				TextDeleted -= EntryTextChanges;
			}

			_housesDataLoader = newValue;

			if(_housesDataLoader == null)
			{
				return;
			}

			_housesDataLoader.HousesLoaded += HousesLoaded;
			TextInserted += EntryTextChanges;
			TextDeleted += EntryTextChanges;
		}

		private void HousesLoaded()
		{
			Application.Invoke((sender, e) =>
			{
				var houses = HousesDataLoader.GetHouses();
				_completionListStore = new ListStore(typeof(HouseDTO));

				foreach(var house in houses)
				{
					_completionListStore.AppendValues(house);
				}

				if(Completion != null)
				{
					Completion.Model = _completionListStore;

					if(HasFocus)
					{
						Completion.Complete();
					}

					CompletionLoaded?.Invoke(null, EventArgs.Empty);
				}
			});
		}

		private void EntryTextChanges(object o, TextInsertedArgs args)
		{
			if(string.IsNullOrWhiteSpace(Text))
			{
				_completionListStore?.Clear();
				return;
			}

			if(StreetGuid != null)
			{
				HousesDataLoader.LoadHouses(Text, StreetGuid);
				return;
			}

			if(CityGuid != null)
			{
				HousesDataLoader.LoadHouses(Text, null, CityGuid);
			}
		}

		private void EntryTextChanges(object o, TextDeletedArgs args)
		{
			EntryTextChanges(o, EventArgs.Empty as TextInsertedArgs);
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
			var houseRow = _completionListStore?.Cast<object[]>().FirstOrDefault(row => ((HouseDTO)row[0]).ComplexNumber == BuildingName);

			if(houseRow == null)
			{
				FiasGuid = null;
				FiasHouse = null;
			}
			else
			{
				var house = ((HouseDTO)houseRow[0]);
				FiasGuid = house.FiasGuid;
				FiasHouse = house;
			}

			Binding.FireChange(w => w.BuildingName);

			return base.OnFocusOutEvent(evnt);
		}

		protected override void OnDestroyed()
		{
			if(HousesDataLoader != null)
			{
				HousesDataLoader.HousesLoaded -= HousesLoaded;
			}

			base.OnDestroyed();
		}

		public BindingControler<HouseEntry> Binding { get; }
		public HouseDTO FiasHouse { get; private set; }
		public event EventHandler CompletionLoaded;

		public IHousesDataLoader HousesDataLoader
		{
			get => _housesDataLoader;
			set => ChangeDataLoader(_housesDataLoader, value);
		}

		public bool? FiasCompletion
		{
			get
			{
				if(_completionListStore == null)
				{
					return null;
				}

				return _completionListStore.Cast<object[]>().Any(row => ((HouseDTO)row[0]).ComplexNumber == BuildingName);
			}
		}

		public string BuildingName
		{
			get => Text;
			set
			{
				if(Text == value)
				{
					return;
				}

				Text = value;
				Binding.FireChange(w => w.BuildingName);
			}
		}

		public Guid? StreetGuid { get; set; }
		
		public Guid? CityGuid { get; set; }

		public Guid? FiasGuid
		{
			get => _fiasGuid;
			set
			{
				_fiasGuid = value;
				Binding.FireChange(w => w.FiasGuid);
			}
		}

		public void GetCoordinates(out decimal? longitude, out decimal? latitude)
		{
			longitude = null;
			latitude = null;

			var houseRow = _completionListStore?.Cast<object[]>().FirstOrDefault(row => ((HouseDTO)row[0]).ComplexNumber == BuildingName);

			if(houseRow == null)
			{
				return;
			}

			var house = (HouseDTO)houseRow[0];

			var culture = CultureInfo.CreateSpecificCulture("ru-RU");
			culture.NumberFormat.NumberDecimalSeparator = ".";

			if(!string.IsNullOrEmpty(house.Longitude) && !string.IsNullOrEmpty(house.Latitude))
			{
				longitude = decimal.Parse(house.Longitude.Replace(",", "."), culture);
				latitude = decimal.Parse(house.Latitude.Replace(",", "."), culture);
			}
		}
	}
}
