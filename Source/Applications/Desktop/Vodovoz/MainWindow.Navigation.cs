using Gtk;
using QS.Tdi;
using QS.Tdi.Gtk;
using Vodovoz.Core.Journal;
using Vodovoz.SidePanel.InfoProviders;

public partial class MainWindow
{
	public void OnTdiMainTabClosed(object sender, TabClosedEventArgs args)
	{
		switch(args.Tab)
		{
			case IInfoProvider dialogTab:
				infopanel.OnInfoProviderDisposed(dialogTab);
				break;
			case TdiSliderTab journalTab when journalTab.Journal is IInfoProvider journal:
				infopanel.OnInfoProviderDisposed(journal);
				break;
			case TdiSliderTab tdiSliderTab
				when(tdiSliderTab.Journal as MultipleEntityJournal)?.RepresentationModel is IInfoProvider provider:
				{
					infopanel.OnInfoProviderDisposed(provider);
					break;
				}
		}

		if(tdiMain.NPages == 0)
		{
			infopanel.SetInfoProvider(DefaultInfoProvider.Instance);
		}
	}

	public void OnTdiMainTabSwitched(object sender, TabSwitchedEventArgs args)
	{
		var currentTab = args.Tab;
		switch(currentTab)
		{
			case IInfoProvider provider:
				infopanel.SetInfoProvider(provider);
				break;
			case TdiSliderTab tdiSliderTab when tdiSliderTab.Journal is IInfoProvider provider:
				infopanel.SetInfoProvider(provider);
				break;
			case TdiSliderTab tdiSliderTab when
				(tdiSliderTab.Journal as MultipleEntityJournal)?.RepresentationModel is IInfoProvider provider:
				{
					infopanel.SetInfoProvider(provider);
					break;
				}
			default:
				infopanel.SetInfoProvider(DefaultInfoProvider.Instance);
				break;
		}
	}

	public void OnTdiMainTabAdded(object sender, TabAddedEventArgs args)
	{
		switch(args.Tab)
		{
			case IInfoProvider dialogTab:
				dialogTab.CurrentObjectChanged += infopanel.OnCurrentObjectChanged;
				break;
			case TdiSliderTab journalTab when journalTab.Journal is IInfoProvider journal:
				journal.CurrentObjectChanged += infopanel.OnCurrentObjectChanged;
				break;
			case TdiSliderTab tdiSliderTab
				when(tdiSliderTab.Journal as MultipleEntityJournal)?.RepresentationModel is IInfoProvider provider:
				{
					provider.CurrentObjectChanged += infopanel.OnCurrentObjectChanged;
					break;
				}
		}
	}
}
