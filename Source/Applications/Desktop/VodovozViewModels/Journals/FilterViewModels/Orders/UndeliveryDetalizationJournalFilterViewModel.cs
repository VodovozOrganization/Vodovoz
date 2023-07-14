using QS.Project.Filter;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Orders
{
	public class UndeliveryDetalizationJournalFilterViewModel
		: FilterViewModelBase<UndeliveryDetalizationJournalFilterViewModel>
	{
		private UndeliveryObject _undeliveryObject;
		private UndeliveryKind _undeliveryKind;
		private bool _hideArchive;
		private IList<UndeliveryKind> _allUndeliveryKinds;
		private IList<UndeliveryKind> _undeliveryKinds;
		private IList<UndeliveryObject> _undeliveryObjects;

		public UndeliveryDetalizationJournalFilterViewModel()
		{
			UndeliveryObjects = UoW.Session.QueryOver<UndeliveryObject>().List();

			UndeliveryKinds = _allUndeliveryKinds = UoW.Session.QueryOver<UndeliveryKind>().List();
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
						UndeliveryKinds = _allUndeliveryKinds;
						UndeliveryKind = null;
					}
					else
					{
						UndeliveryKinds = _allUndeliveryKinds
							.Where(k => k.UndeliveryObject.Id == value.Id).ToList();
					}
				}
			}
		}

		public UndeliveryKind UndeliveryKind
		{
			get => _undeliveryKind;
			set => UpdateFilterField(ref _undeliveryKind, value);
		}

		public IList<UndeliveryObject> UndeliveryObjects
		{
			get => _undeliveryObjects;
			set => _undeliveryObjects = value;
		}

		public IList<UndeliveryKind> UndeliveryKinds
		{
			get => _undeliveryKinds;
			set => SetField(ref _undeliveryKinds, value);
		}

		public bool HideArchive
		{
			get => _hideArchive;
			set => UpdateFilterField(ref _hideArchive, value);
		}
	}
}
