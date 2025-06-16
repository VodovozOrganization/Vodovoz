using QS.Commands;
using QS.Extensions.Observable.Collections.List;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public abstract class DualTreeViewNodesTransferViewModel : WidgetViewModelBase
	{
		public IObservableList LeftItems { get; protected set; }

		public IObservableList RightItems { get; protected set; }

		public IEnumerable<object> SelectedLeftItems { get; set; }

		public IEnumerable<object> SelectedRightItems { get; set; }

		public Func<object, string> ItemLeftDisplayFunc { get; protected set; }
			= item => item.ToString();

		public Func<object, string> ItemRightDisplayFunc { get; protected set; }
			= item => item.ToString();

		public string SearchText { get; set; }

		public bool IsSearchVisible { get; protected set; }

		public bool ShowAllButtons { get; protected set; } = false;

		public bool SkipTreeViewConfig { get; protected set; } = false;

		public Expression<Func<string, object, bool>> SearchRightPredicate { get; protected set; }
		public Expression<Func<string, object, bool>> SearchLeftPredicate { get; protected set; }

		public DelegateCommand MoveToLeftCommand { get; protected set; }

		public DelegateCommand MoveToRightCommand { get; protected set; }

		public DelegateCommand MoveAllToLeftCommand { get; protected set; }

		public DelegateCommand MoveAllToRightCommand { get; protected set; }

		public DelegateCommand SearchCommand { get; protected set; }
	}

	public class DualTreeViewNodesTransferViewModel<T> : DualTreeViewNodesTransferViewModel
	{
		protected DualTreeViewNodesTransferViewModel()
		{
			var leftItems = new ObservableList<T>();
			base.LeftItems = leftItems;
			LeftItems = leftItems;

			var rightItems = new ObservableList<T>();
			base.RightItems = rightItems;
			RightItems = rightItems;

			base.SelectedLeftItems = Enumerable.Empty<object>();
			base.SelectedRightItems = Enumerable.Empty<object>();

			SelectedLeftItems = Enumerable.Empty<T>();
			SelectedRightItems = Enumerable.Empty<T>();

			MoveToLeftCommand = new DelegateCommand(MoveSelectedItemsToLeft);
			MoveToRightCommand = new DelegateCommand(MoveSelectedItemsToRight);
			MoveAllToLeftCommand = new DelegateCommand(MoveAllToLeft);
			MoveAllToRightCommand = new DelegateCommand(MoveAllToRight);
		}

		public new IObservableList<T> LeftItems { get; }

		public new IObservableList<T> RightItems { get; }

		public new IEnumerable<T> SelectedLeftItems
		{
			get => base.SelectedLeftItems.OfType<T>();
			set => base.SelectedLeftItems = value.Cast<object>();
		}

		public new IEnumerable<T> SelectedRightItems
		{
			get => base.SelectedRightItems.OfType<T>();
			set => base.SelectedRightItems = value.Cast<object>();
		}

		public new Expression<Func<string, T, bool>> SearchRightPredicate
		{
			get => base.SearchLeftPredicate as Expression<Func<string, T, bool>>;
			set => base.SearchLeftPredicate = value as Expression<Func<string, object, bool>>;
		}

		public new Expression<Func<string, T, bool>> SearchLeftPredicate
		{
			get => base.SearchLeftPredicate as Expression<Func<string, T, bool>>;
			set => base.SearchLeftPredicate = value as Expression<Func<string, object, bool>>;
		}

		public new Func<T, string> ItemLeftDisplayFunc
		{
			get => base.ItemLeftDisplayFunc as Func<T, string>;
			set => base.ItemLeftDisplayFunc = value as Func<object, string>;
		}

		public new Func<T, string> ItemRightDisplayFunc
		{
			get => base.ItemRightDisplayFunc as Func<T, string>;
			set => base.ItemRightDisplayFunc = value as Func<object, string>;
		}

		private void MoveSelectedItemsToLeft()
		{
			var selectedItemsToMove = SelectedRightItems.ToArray();

			foreach(var item in selectedItemsToMove)
			{
				RightItems.Remove(item);
				LeftItems.Add(item);
			}
		}

		public void MoveSelectedItemsToRight()
		{
			var selectedItemsToMove = SelectedLeftItems.ToArray();

			foreach(var item in selectedItemsToMove)
			{
				LeftItems.Remove(item);
				RightItems.Add(item);
			}
		}

		public void MoveAllToLeft()
		{
			foreach(var item in RightItems.ToArray())
			{
				RightItems.Remove(item);
				LeftItems.Add(item);
			}
		}

		public void MoveAllToRight()
		{
			foreach(var item in LeftItems.ToArray())
			{
				LeftItems.Remove(item);
				RightItems.Add(item);
			}
		}

		public static DualTreeViewNodesTransferViewModel<T> Create(
			IEnumerable<T> leftItems = null,
			IEnumerable<T> rightItems = null,
			bool isSearchVisible = true,
			Expression<Func<string, T, bool>> searchPredicate = null,
			Func<T, string> itemDisplayFunc = null)
		{
			var result = new DualTreeViewNodesTransferViewModel<T>();

			if(leftItems != null)
			{
				foreach(var item in leftItems)
				{
					result.LeftItems.Add(item);
				}
			}

			if(rightItems != null)
			{
				foreach(var item in rightItems)
				{
					result.RightItems.Add(item);
				}
			}

			result.IsSearchVisible = isSearchVisible;

			if(searchPredicate != null)
			{
				result.SearchLeftPredicate = searchPredicate;
				result.SearchRightPredicate = searchPredicate;
			}

			if(itemDisplayFunc != null)
			{
				result.ItemRightDisplayFunc = itemDisplayFunc;
				result.ItemLeftDisplayFunc = itemDisplayFunc;
			}

			return result;
		}
	}

	public class DualTreeViewNodesTransferViewModel<T, U>
		: DualTreeViewNodesTransferViewModel
	{
		protected DualTreeViewNodesTransferViewModel(
			IEnumerable<T> leftItems = null,
			IEnumerable<U> rightItems = null,
			bool isSearchVisible = true,
			bool showAllButtons = true,
			Expression<Func<string, T, bool>> searchLeftPredicate = null,
			Expression<Func<string, U, bool>> searchRightPredicate = null,
			Func<T, string> itemLeftDisplayFunc = null,
			Func<U, string> itemRightDisplayFunc = null)
		{
			SkipTreeViewConfig = true;

			ShowAllButtons = showAllButtons;

			var leftItemsList = leftItems == null
				? new ObservableList<T>()
				: new ObservableList<T>(leftItems);

			base.LeftItems = leftItemsList;
			LeftItems = leftItemsList;

			var rightItemsList = rightItems == null
				? new ObservableList<U>()
				: new ObservableList<U>(rightItems);

			base.RightItems = rightItemsList;
			RightItems = rightItemsList;

			SelectedLeftItems = Enumerable.Empty<T>();
			SelectedRightItems = Enumerable.Empty<U>();

			IsSearchVisible = isSearchVisible;

			if(searchLeftPredicate != null)
			{
				SearchLeftPredicate = searchLeftPredicate;
			}

			if(searchRightPredicate != null)
			{
				SearchRightPredicate = searchRightPredicate;
			}

			if(itemLeftDisplayFunc != null)
			{
				ItemLeftDisplayFunc = itemLeftDisplayFunc;
			}

			if(itemRightDisplayFunc != null)
			{
				ItemRightDisplayFunc = itemRightDisplayFunc;
			}

			MoveToLeftCommand = new DelegateCommand(MoveSelectedItemsToLeft);
			MoveToRightCommand = new DelegateCommand(MoveSelectedItemsToRight);
			MoveAllToLeftCommand = new DelegateCommand(MoveAllToLeft);
			MoveAllToRightCommand = new DelegateCommand(MoveAllToRight);
		}

		public new IObservableList<T> LeftItems { get; }

		public new IObservableList<U> RightItems { get; }

		public new IEnumerable<T> SelectedLeftItems
		{
			get => base.SelectedLeftItems.OfType<T>();
			set => base.SelectedLeftItems = value.Cast<object>();
		}

		public new IEnumerable<U> SelectedRightItems
		{
			get => base.SelectedRightItems.OfType<U>();
			set => base.SelectedRightItems = value.Cast<object>();
		}

		public new Expression<Func<string, T, bool>> SearchLeftPredicate
		{
			get => base.SearchLeftPredicate as Expression<Func<string, T, bool>>;
			set => base.SearchLeftPredicate = value as Expression<Func<string, object, bool>>;
		}

		public new Expression<Func<string, U, bool>> SearchRightPredicate
		{
			get => base.SearchLeftPredicate as Expression<Func<string, U, bool>>;
			set => base.SearchLeftPredicate = value as Expression<Func<string, object, bool>>;
		}

		public new Func<T, string> ItemLeftDisplayFunc
		{
			get => base.ItemLeftDisplayFunc as Func<T, string>;
			set => base.ItemLeftDisplayFunc = value as Func<object, string>;
		}

		public new Func<U, string> ItemRightDisplayFunc
		{
			get => base.ItemRightDisplayFunc as Func<U, string>;
			set => base.ItemRightDisplayFunc = value as Func<object, string>;
		}

		protected virtual void MoveAllToLeft()
		{
			throw new NotImplementedException("Метод должен переопределяться в наследуемом классе.");
		}

		protected virtual void MoveAllToRight()
		{
			throw new NotImplementedException("Метод должен переопределяться в наследуемом классе.");
		}

		protected virtual void MoveSelectedItemsToLeft()
		{
			throw new NotImplementedException("Метод должен переопределяться в наследуемом классе.");
		}

		protected virtual void MoveSelectedItemsToRight()
		{
			throw new NotImplementedException("Метод должен переопределяться в наследуемом классе.");
		}
	}
}
