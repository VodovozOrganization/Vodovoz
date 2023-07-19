using Autofac;
using Gtk;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.Tdi;
using QS.Tdi.Gtk;
using QSOrmProject;
using System;
using Vodovoz.Core.Journal;
using Vodovoz.SidePanel.InfoProviders;

public partial class MainWindow
{
	#region Obsolete methods

	[Obsolete("Старые диалоги, по достижению ссылок 0 - удалить")]
	private void OpenDialog<TDlg>()
	where TDlg : TdiTabBase
	{
		var localScope = autofacScope.BeginLifetimeScope();

		var tab = tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<TDlg>(),
			() => localScope.Resolve<TDlg>());

		tab.TabClosed += (s, e) =>
		{
			localScope.Dispose();
			localScope = null;
		};
	}

	[Obsolete("Очень старые диалоги, по достижению ссылок 0 - удалить")]
	private void OpenOrmReference<TDomainObject>()
	where TDomainObject : IDomainObject =>
	tdiMain.AddTab(new OrmReference(typeof(TDomainObject)));

	[Obsolete("Очень старые диалоги, по достижению ссылок 0 - удалить")]
	private void OpenHashedOrmReference<TDomainObject>()
	where TDomainObject : IDomainObject =>
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<TDomainObject>(),
			() => new OrmReference(typeof(TDomainObject)));

	#endregion Obsolete methods

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

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		if(tdiMain.CloseAllTabs())
		{
			a.RetVal = false;
			autofacScope.Dispose();
			Application.Quit();
		}
		else
		{
			a.RetVal = true;
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
