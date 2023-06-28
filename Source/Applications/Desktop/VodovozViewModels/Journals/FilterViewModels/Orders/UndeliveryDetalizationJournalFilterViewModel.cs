using System;
using System.Collections.Generic;
using System.Linq;
using QS.Project.Filter;
using QS.Project.Journal;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Orders
{
	public class UndeliveryDetalizationJournalFilterViewModel
		: FilterViewModelBase<UndeliveryDetalizationJournalFilterViewModel>, IJournalFilterViewModel
	{
		private UndeliveryObject _undeliveryObject;
		private UndeliveryKind _undeliveryOKind;
		private IEnumerable<UndeliveryKind> _visibleUndeliveryKinds;
		private UndeliveryKind _restrictUndeliveryKind;
		private UndeliveryObject _restrictUndeliveryObject;
		private bool _hideArchieve = false;

		public UndeliveryDetalizationJournalFilterViewModel(Action<UndeliveryDetalizationJournalFilterViewModel> filterParams = null)
		{
			UndeliveryObjects = UoW.Session.QueryOver<UndeliveryObject>()
				.Where(co => !co.IsArchive)
				.List();

			UndeliveryKinds = UoW.Session.QueryOver<UndeliveryKind>().List();
			VisibleUndeliveryKinds = Enumerable.Empty<UndeliveryKind>();

			if(filterParams != null)
			{
				SetAndRefilterAtOnce(filterParams);
			}
		}

		public UndeliveryObject UndeliveryObject
		{
			get => _undeliveryObject;
			set
			{
				if(UpdateFilterField(ref _undeliveryObject, value))
				{
					if(value is null)
					{
						VisibleUndeliveryKinds = Enumerable.Empty<UndeliveryKind>();
						UndeliveryKind = null;
					}
					else
					{
						VisibleUndeliveryKinds = UndeliveryKinds
							.Where(ck => ck.UndeliveryObject == value);
					}
					OnPropertyChanged(nameof(CanChangeUndeliveryKind));
				}
			}
		}

		public UndeliveryKind UndeliveryKind
		{
			get => _undeliveryOKind;
			set
			{
				UpdateFilterField(ref _undeliveryOKind, value);
				OnPropertyChanged(nameof(CanChangeUndeliveryKind));
			}
		}

		public IList<UndeliveryObject> UndeliveryObjects { get; }

		public IList<UndeliveryKind> UndeliveryKinds { get; }

		public bool HideArchieve
		{
			get => _hideArchieve;
			set => UpdateFilterField(ref _hideArchieve, value);
		}

		public UndeliveryKind RestrictUndeliveryKind
		{
			get => _restrictUndeliveryKind;
			set
			{
				if(UpdateFilterField(ref _restrictUndeliveryKind, value))
				{
					UndeliveryKind = value;
					RestrictUndeliveryObject = UndeliveryObjects
						.Where(x => x.Id == value?.UndeliveryObject?.Id)
						.FirstOrDefault();
				}
			}
		}

		public UndeliveryObject RestrictUndeliveryObject
		{
			get => _restrictUndeliveryObject;
			set
			{
				if(UpdateFilterField(ref _restrictUndeliveryObject, value))
				{
					UndeliveryObject = value;
				}
			}
		}

		public bool CanChangeUndeliveryObject => RestrictUndeliveryObject is null;

		public bool CanChangeUndeliveryKind => RestrictUndeliveryKind is null
			&& UndeliveryObject != null;

		public IEnumerable<UndeliveryKind> VisibleUndeliveryKinds
		{
			get => _visibleUndeliveryKinds;
			private set => UpdateFilterField(ref _visibleUndeliveryKinds, value);
		}

		public bool IsShow { get; set; }
	}
}
