using Gdk;
using Gtk;
using Vodovoz.Extensions;

namespace Vodovoz.Infrastructure
{
	/// <summary>
	/// Text - цвет текста Entry, Label, Treeview и других текстовых элементов
	/// Base - цвет заливки Entry, Label, Treeview и других текстовых элементов
	/// FG - цвет текста Button и других не текстовых элементов
	/// BG - цвет заливки Button и других не текстовых элементов
	/// </summary>
	public static class GdkColors
	{
		private static readonly Widget _defaultWidget = new Label();

		public static bool IsDark { get; } = Rc.GetStyle(_defaultWidget).IsDark();
		public static bool IsLight { get; } = Rc.GetStyle(_defaultWidget).IsLight();

		#region Primary

		public static Color PrimaryBG { get; } = Rc.GetStyle(_defaultWidget).Background(StateType.Normal);
		public static Color PrimaryFG { get; } = Rc.GetStyle(_defaultWidget).Foreground(StateType.Normal);
		public static Color PrimaryText { get; } = Rc.GetStyle(_defaultWidget).Text(StateType.Normal);
		public static Color PrimaryBase { get; } = Rc.GetStyle(_defaultWidget).Base(StateType.Normal);

		#endregion Primary

		#region Active

		public static Color ActiveBG { get; } = Rc.GetStyle(_defaultWidget).Background(StateType.Active);
		public static Color ActiveFG { get; } = Rc.GetStyle(_defaultWidget).Foreground(StateType.Active);
		public static Color ActiveText { get; } = Rc.GetStyle(_defaultWidget).Text(StateType.Active);
		public static Color ActiveBase { get; } = Rc.GetStyle(_defaultWidget).Base(StateType.Active);

		#endregion Active

		#region Success

		public static Color SuccessText { get; } = new Color(14, 135, 14);

		public static Color SuccessBase { get; } = IsLight
			? new Color(0xc0, 0xff, 0xc0)
			: new Color(47, 109, 47);

		#endregion Success

		#region Danger

		public static Color DangerText { get; } = IsLight ? new Color(255, 0, 0) : new Color(209, 90, 90);

		public static Color DangerBase { get; } = IsLight
			? new Color(0xff, 0x66, 0x66)
			: new Color(126, 50, 50);

		#endregion Danger

		#region Warning

		public static Color WarningText { get; } = new Color(255, 255, 40);

		public static Color WarningBase { get; } = IsLight ? new Color(0xe1, 0xd6, 0x70) : new Color(123, 116, 60);

		#endregion Warning

		#region Info

		public static Color InfoText { get; } = IsLight ? new Color(0x00, 0x18, 0xf9) : new Color(70, 90, 204);

		public static Color InfoBase { get; } = IsLight ? new Color(0xbb, 0xbb, 0xff) : new Color(52, 67, 153);

		#endregion Info

		#region Insensitive

		public static Color InsensitiveBG { get; } = Rc.GetStyle(_defaultWidget).Background(StateType.Insensitive);
		public static Color InsensitiveFG { get; } = Rc.GetStyle(_defaultWidget).Foreground(StateType.Insensitive);
		public static Color InsensitiveText { get; } = Rc.GetStyle(_defaultWidget).Text(StateType.Insensitive);
		public static Color InsensitiveBase { get; } = Rc.GetStyle(_defaultWidget).Base(StateType.Insensitive);

		#endregion Insensitive

		#region AdditionalColors

		public static Color Red2 { get; } = new Color(0xfe, 0x5c, 0x5c);
		public static Color DarkRed { get; } = new Color(110, 19, 0);
		public static Color Pink { get; } = IsLight ? new Color(0xff, 0xc0, 0xc0) : new Color(164, 123, 123);
		public static Color LightYellow2 { get; } = new Color(255, 243, 199);
		public static Color YellowMustard { get; } = IsLight ? LightYellow2: new Color(193, 157, 39);
		public static Color Orange { get; } = new Color(0xfc, 0x66, 0x00);
		public static Color DarkGreen { get; } = new Color(32, 100, 17);
		public static Color CadetBlue { get; } = IsLight ? new Color(95, 158, 160) : new Color(140, 178, 179);
		public static Color BabyBlue { get; } = new Color(0x89, 0xcf, 0xef);
		public static Color LightPurple { get; } = new Color(199, 206, 255);
		public static Color LightCoral { get; } = new Color(240, 128, 128);
		public static Color Turquoise { get; } = new Color(64, 224, 208);
		public static Color DarkBlue { get; } = new Color(0, 12, 123);
		public static Color DarkViolet { get; } = new Color(88, 19, 94);
		public static Color DarkMustard { get; } = IsLight ? new Color(0xb3, 0xb3, 0x00) : new Color(255, 219, 88);
		public static Color CashFlowTotalColor { get; } = IsLight ? new Color(249, 191, 143) : new Color(233, 84, 32);
		public static Color DiscussionCommentBase { get; } = IsLight ? new Color(230, 230, 245) : new Color(62, 62, 65);
		public static Color CarMonitoringNewbieDriversBase { get; } = IsLight ? new Color(250, 130, 130) : new Color(178, 102, 25);

		#endregion AdditionalColors
	}
}
