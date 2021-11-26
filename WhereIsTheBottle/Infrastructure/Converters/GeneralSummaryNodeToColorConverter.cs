using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WhereIsTheBottle.Models.MainContent.Nodes;

namespace WhereIsTheBottle.Infrastructure
{
	public class GeneralSummaryNodeToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is not GeneralSummaryNode node)
			{
				throw new ArgumentException("Invalid value argument", nameof(value));
			}

			if(node.AssetByMorning < node.MinimalAsset)
			{
				return Brushes.Red;
			}
			if(node.AssetByMorning > node.NecessaryAsset)
			{
				return Brushes.ForestGreen;
			}
			return Brushes.Black;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
