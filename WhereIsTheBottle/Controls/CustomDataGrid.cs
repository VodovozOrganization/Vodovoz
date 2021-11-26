using System.Windows;
using System.Windows.Controls;

namespace WhereIsTheBottle.Controls
{
	public class CustomDataGrid : DataGrid
	{
		public static readonly RoutedEvent SortedEvent = EventManager.RegisterRoutedEvent(
			nameof(Sorted), RoutingStrategy.Bubble, typeof(DataGridSortedEventHandler), typeof(CustomDataGrid));

		public event DataGridSortedEventHandler Sorted
		{
			add => AddHandler(SortedEvent, value);
			remove => RemoveHandler(SortedEvent, value);
		}

		void RaiseSortedEvent(DataGridSortingEventArgs dataGridSortingEventArgs)
		{
			var args = new DataGridSortedEventArgs(dataGridSortingEventArgs.Column);
			RaiseEvent(args);
		}

		protected override void OnSorting(DataGridSortingEventArgs eventArgs)
		{
			base.OnSorting(eventArgs);
			RaiseSortedEvent(eventArgs);
		}
	}

	public delegate void DataGridSortedEventHandler(object sender, DataGridSortedEventArgs args);

	public class DataGridSortedEventArgs : RoutedEventArgs
	{
		public DataGridSortedEventArgs(DataGridColumn column) : base(CustomDataGrid.SortedEvent)
		{
			Column = column;
		}

		public DataGridSortedEventArgs(DataGridColumn column, object source) : base(CustomDataGrid.SortedEvent, source)
		{
			Column = column;
		}

		public DataGridColumn Column { get; set; }
	}
}
