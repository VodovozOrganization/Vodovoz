using Microsoft.AspNetCore.Http;
using MoreLinq.Extensions;
using QS.Project.Filter;
using QS.Utilities.Enums;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.Journals.JournalNodes.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.FilterViewModels.Employees
{
	public class FineFilterViewModel : FilterViewModelBase<FineFilterViewModel>
	{
		private bool _canEditSubdivision;
		private Subdivision _subdivision;
		private bool _canEditFineDate;
		private DateTime? _fineDateStart;
		private DateTime? _fineDateEnd;
		private bool _canEditRouteListDate;
		private DateTime? _routeListDateStart;
		private DateTime? _routeListDateEnd;
		private int[] _excludedIds;
		private int[] _findFinesWithIds;
		private FinesJournalViewModel _journalViewModel;
		private bool _canEditFilter;
		private Employee _author;
		private bool _canEditAuthor;

		private List<EmployeeFineCategoryNode> _fineTypeNodes = EnumHelper.GetValuesList<FineTypes>().Select(x => new EmployeeFineCategoryNode(x) { Selected = true }).ToList();


		public FineFilterViewModel()
		{
			CanEditFilter = true;
		}

		public FinesJournalViewModel JournalViewModel
		{
			get => _journalViewModel;
			set
			{
				_journalViewModel = value;

				var subdivisionViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<FineFilterViewModel>(value, this, UoW, _journalViewModel.NavigationManager, _journalViewModel.Scope);

				SubdivisionViewModel = subdivisionViewModelEntryViewModelBuilder
					.ForProperty(x => x.Subdivision)
					.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
					.UseViewModelDialog<SubdivisionViewModel>()
					.Finish();

				SubdivisionViewModel.IsEditable = CanEditSubdivision;

				var authorViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<FineFilterViewModel>(value, this, UoW, _journalViewModel.NavigationManager, _journalViewModel.Scope);

				AuthorViewModel = authorViewModelEntryViewModelBuilder
					.ForProperty(x => x.Author)
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
					.UseViewModelDialog<EmployeeViewModel>()
					.Finish();

				AuthorViewModel.IsEditable = CanEditAuthor;
			}
		}

		public IEntityEntryViewModel SubdivisionViewModel { get; private set; }

		public IEntityEntryViewModel AuthorViewModel { get; private set; }

		public bool CanEditFilter
		{
			get => _canEditFilter;
			set
			{
				if(SetField(ref _canEditFilter, value))
				{
					CanEditSubdivision = value;
					CanEditFineDate = value;
					CanEditRouteListDate = value;
					CanEditAuthor = value;
				}
			}
		}

		public virtual bool CanEditSubdivision
		{
			get => _canEditSubdivision;
			set => SetField(ref _canEditSubdivision, value);
		}

		public virtual Subdivision Subdivision
		{
			get => _subdivision;
			set => UpdateFilterField(ref _subdivision, value);
		}

		public virtual bool CanEditFineDate
		{
			get => _canEditFineDate;
			set => SetField(ref _canEditFineDate, value);
		}

		public virtual DateTime? FineDateStart
		{
			get => _fineDateStart;
			set => UpdateFilterField(ref _fineDateStart, value);
		}

		public virtual DateTime? FineDateEnd
		{
			get => _fineDateEnd;
			set => UpdateFilterField(ref _fineDateEnd, value);
		}

		public virtual bool CanEditRouteListDate
		{
			get => _canEditRouteListDate;
			set => SetField(ref _canEditRouteListDate, value);
		}

		public virtual DateTime? RouteListDateStart
		{
			get => _routeListDateStart;
			set => UpdateFilterField(ref _routeListDateStart, value);
		}

		public virtual DateTime? RouteListDateEnd
		{
			get => _routeListDateEnd;
			set => UpdateFilterField(ref _routeListDateEnd, value);
		}

		public virtual int[] ExcludedIds
		{
			get => _excludedIds;
			set => UpdateFilterField(ref _excludedIds, value);
		}

		public virtual int[] FindFinesWithIds
		{
			get => _findFinesWithIds;
			set => UpdateFilterField(ref _findFinesWithIds, value);
		}

		public virtual bool CanEditAuthor
		{
			get => _canEditAuthor;
			set => SetField(ref _canEditAuthor, value);
		}


		public virtual Employee Author
		{
			get => _author;
			set => UpdateFilterField(ref _author, value);
		}
		public virtual FineTypes[] SelectedFineCategories
		{
			get => FineCategoryNodes.Where(x => x.Selected)
				.Select(x => x.FineCategory)
				.ToArray();
			set
			{
				foreach(var category in _fineTypeNodes.Where(x => value.Contains(x.FineCategory)))
				{
					category.Selected = true;
				}
				OnPropertyChanged(nameof(SelectedFineCategories));
			}
		}

		public List<EmployeeFineCategoryNode> FineCategoryNodes
		{
			get => _fineTypeNodes;
			set
			{
				UnsubscribeOnFineCategoryChanged();
				_fineTypeNodes = value;
				SubscribeOnFineCategoryChanged();
			}
		}

		private void SubscribeOnFineCategoryChanged()
		{
			foreach(var node in FineCategoryNodes)
			{
				node.PropertyChanged += OnCategoryCheckChanged;
			}
		}

		private void UnsubscribeOnFineCategoryChanged()
		{
			foreach(var node in FineCategoryNodes)
			{
				node.PropertyChanged -= OnCategoryCheckChanged;
			}
		}

		private void OnCategoryCheckChanged(object sender, PropertyChangedEventArgs e)
		{
			Update();
		}

		public void SelectAllFineCategories()
		{
			_fineTypeNodes.ForEach(x => x.Selected = true);
			Update();
		}

		public void DeselectAllFineCategories()
		{
			_fineTypeNodes.ForEach(x => x.Selected = false);
			Update();
		}


		public override void Dispose()
		{
			_journalViewModel = null;
			base.Dispose();
		}
	}
}
