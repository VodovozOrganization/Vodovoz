using Autofac;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QSOrmProject;
using System;

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


}
