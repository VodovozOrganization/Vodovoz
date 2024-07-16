namespace Vodovoz.Presentation.ViewModels.Common
{
	public class SearchStringChangedEventArgs
	{
		public SearchStringChangedEventArgs(string newSearchString)
		{
			NewSearchString = newSearchString;
		}

		public string NewSearchString { get; }
	}
}
