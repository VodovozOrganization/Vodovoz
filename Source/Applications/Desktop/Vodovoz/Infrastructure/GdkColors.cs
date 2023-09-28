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
		private static Widget _dwfaultWidget => new Label();

		public static bool IsDark => Rc.GetStyle(_dwfaultWidget).IsDark();
		public static bool IsLight => Rc.GetStyle(_dwfaultWidget).IsLight();

		#region Primary

		public static Color PrimaryBG => Rc.GetStyle(_dwfaultWidget).Background(StateType.Normal);
		public static Color PrimaryFG => Rc.GetStyle(_dwfaultWidget).Foreground(StateType.Normal);
		public static Color PrimaryText => Rc.GetStyle(_dwfaultWidget).Text(StateType.Normal);
		public static Color PrimaryBase => Rc.GetStyle(_dwfaultWidget).Base(StateType.Normal);

		#endregion Primary

		#region Active

		public static Color ActiveBG => Rc.GetStyle(_dwfaultWidget).Background(StateType.Active);
		public static Color ActiveFG => Rc.GetStyle(_dwfaultWidget).Foreground(StateType.Active);
		public static Color ActiveText => Rc.GetStyle(_dwfaultWidget).Text(StateType.Active);
		public static Color ActiveBase => Rc.GetStyle(_dwfaultWidget).Base(StateType.Active);

		#endregion Active

		#region Success

		public static Color SuccessText { get; } = new Color(14, 135, 14);

		public static Color SuccessBase { get; } = IsLight ? new Color(0xc0, 0xff, 0xc0) : new Color(69, 161, 69);

		#endregion Success

		#region Danger

		public static Color DangerText { get; } = IsLight ? new Color(255, 0, 0) : new Color(209, 90, 90);

		public static Color DangerBase { get; } = new Color(0xff, 0x66, 0x66);

		#endregion Danger

		#region Warning

		public static Color WarningText { get; } = new Color(255, 255, 40);

		public static Color WarningBase { get; } = new Color(0xe1, 0xd6, 0x70);

		#endregion Warning

		#region Info

		public static Color InfoText { get; } = IsLight ? new Color(0x00, 0x18, 0xf9) : new Color(70, 90, 204);

		public static Color InfoBase { get; } = IsLight ? new Color(0xbb, 0xbb, 0xff) : new Color(52, 67, 153);

		#endregion Info

		#region Insensitive

		public static Color InsensitiveBG => Rc.GetStyle(_dwfaultWidget).Background(StateType.Insensitive);
		public static Color InsensitiveFG => Rc.GetStyle(_dwfaultWidget).Foreground(StateType.Insensitive);
		public static Color InsensitiveText => Rc.GetStyle(_dwfaultWidget).Text(StateType.Insensitive);
		public static Color InsensitiveBase => Rc.GetStyle(_dwfaultWidget).Base(StateType.Insensitive);

		#endregion Insensitive

		#region AdditionalColors

		public static Color Red2 { get; } = new Color(0xfe, 0x5c, 0x5c);
		public static Color DarkRed { get; } = new Color(110, 19, 0);
		public static Color Pink { get; } = IsLight ? new Color(0xff, 0xc0, 0xc0) : new Color(164, 123, 123);
		public static Color LightYellow2 { get; } = new Color(255, 243, 199);
		public static Color Orange { get; } = new Color(0xfc, 0x66, 0x00);
		public static Color DarkGreen { get; } = new Color(32, 100, 17);
		public static Color CadetBlue { get; } = IsLight ? new Color(95, 158, 160) : new Color(140, 178, 179);
		public static Color BabyBlue { get; } = new Color(0x89, 0xcf, 0xef);
		public static Color LightPurple { get; } = new Color(199, 206, 255);
		public static Color LightCoral { get; } = new Color(240, 128, 128);
		public static Color Turquoise { get; } = new Color(64, 224, 208);
		public static Color DarkMustard { get; } = IsLight ? new Color(0xb3, 0xb3, 0x00) : new Color(255, 219, 88);
		public static Color CashFlowTotalColor { get; } = IsLight ? new Color(249, 191, 143) : new Color(233, 84, 32);
		public static Color ComplaintDiscussionCommentBase { get; } = IsLight ? new Color(230, 230, 245) : new Color(62, 62, 65);

		#endregion AdditionalColors
	}
}
