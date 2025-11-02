using QS.Project.Filter;
using QS.Project.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Complaints
{
	public class ComplaintDetalizationJournalFilterViewModel
		: FilterViewModelBase<ComplaintDetalizationJournalFilterViewModel>
	{
		private ComplaintObject _complaintObject;
		private ComplaintKind _complaintOKind;
		private IEnumerable<ComplaintKind> _visibleComplaintKinds;
		private ComplaintKind _restrictComplaintKind;
		private ComplaintObject _restrictComplaintObject;
		private bool _hideArchieve = false;

		public ComplaintDetalizationJournalFilterViewModel(Action<ComplaintDetalizationJournalFilterViewModel> filterParams = null)
		{
			ComplaintObjects = UoW.Session.QueryOver<ComplaintObject>()
				.Where(co => !co.IsArchive)
				.List();

			ComplaintKinds = UoW.Session.QueryOver<ComplaintKind>().List();
			VisibleComplaintKinds = Enumerable.Empty<ComplaintKind>();

			if(filterParams != null)
			{
				SetAndRefilterAtOnce(filterParams);
			}
		}

		public ComplaintObject ComplaintObject
		{
			get => _complaintObject;
			set
			{
				if(UpdateFilterField(ref _complaintObject, value))
				{
					if(value is null)
					{
						VisibleComplaintKinds = Enumerable.Empty<ComplaintKind>();
						ComplaintKind = null;
					}
					else
					{
						VisibleComplaintKinds = ComplaintKinds
							.Where(ck => ck.ComplaintObject == value);
					}
					OnPropertyChanged(nameof(CanChangeComplaintKind));
				}
			}
		}

		public ComplaintKind ComplaintKind
		{
			get => _complaintOKind;
			set
			{
				UpdateFilterField(ref _complaintOKind, value);
				OnPropertyChanged(nameof(CanChangeComplaintKind));
			}
		}

		public IList<ComplaintObject> ComplaintObjects { get; }

		public IList<ComplaintKind> ComplaintKinds { get; }

		public bool HideArchieve
		{
			get => _hideArchieve;
			set => UpdateFilterField(ref _hideArchieve, value);
		}

		public ComplaintKind RestrictComplaintKind
		{
			get => _restrictComplaintKind;
			set
			{
				if(UpdateFilterField(ref _restrictComplaintKind, value))
				{
					/*Т.к. идет множественный вызов и при обработке FirstOrDefault получается уже реальный объект,
					 а не прокси, то делаем дополнительные проверки и устанавливаем параметр RestrictComplaintObject в тихую,
					 если значения ComplaintObject и RestrictComplaintObject равны, дабы не вызвать не нужную перенастройку
					 VisibleComplaintKinds
					 */
					if(value is null)
					{
						RestrictComplaintObject = null;
					}
					else
					{
						if(value.ComplaintObject?.Id != ComplaintObject?.Id)
						{
							RestrictComplaintObject = ComplaintObjects
								.FirstOrDefault(x => x.Id == value.ComplaintObject?.Id);
						}
						else
						{
							_restrictComplaintObject = ComplaintObject;
						}
					}
					
					ComplaintKind = value;
				}
			}
		}

		public ComplaintObject RestrictComplaintObject
		{
			get => _restrictComplaintObject;
			set
			{
				if(UpdateFilterField(ref _restrictComplaintObject, value))
				{
					ComplaintObject = value;
				}
			}
		}

		public bool CanChangeComplaintObject => RestrictComplaintObject is null;

		public bool CanChangeComplaintKind => RestrictComplaintKind is null
			&& ComplaintObject != null;

		public IEnumerable<ComplaintKind> VisibleComplaintKinds
		{
			get => _visibleComplaintKinds;
			private set => UpdateFilterField(ref _visibleComplaintKinds, value);
		}

		public override bool IsShow { get; set; } = true;
	}
}
