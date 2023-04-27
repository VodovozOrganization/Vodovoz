namespace Vodovoz.SidePanel.InfoProviders
{
	public interface ICustomWidthInfoProvider : IInfoProvider
	{
		int? WidthRequest { get; }
	}
}

