using System;
using WhereIsTheBottle.ViewModels;

namespace WhereIsTheBottle.Views
{
	public partial class BottleAnalyticsView
	{
		public BottleAnalyticsView(BottleAnalyticsViewModel bottleAnalyticsViewModel)
		{
			BottleAnalyticsViewModel = bottleAnalyticsViewModel ?? throw new ArgumentNullException(nameof(bottleAnalyticsViewModel));
			InitializeComponent();
			DataContext = bottleAnalyticsViewModel;
		}

		public BottleAnalyticsViewModel BottleAnalyticsViewModel { get; }
	}
}
