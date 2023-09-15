using Gdk;
using Gtk;
using Vodovoz.Extensions;

namespace Vodovoz.Infrastructure
{
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

		#region Insensitive

		public static Color InsensitiveBG => Rc.GetStyle(_dwfaultWidget).Background(StateType.Insensitive);
		public static Color InsensitiveFG => Rc.GetStyle(_dwfaultWidget).Foreground(StateType.Insensitive);
		public static Color InsensitiveText => Rc.GetStyle(_dwfaultWidget).Text(StateType.Insensitive);
		public static Color InsensitiveBase => Rc.GetStyle(_dwfaultWidget).Base(StateType.Insensitive);

		#endregion Insensitive

		#region AdditionalColors

		public static Color Black { get; } = new Color(0, 0, 0);
		public static Color Red { get; } = IsLight ? new Color(0xff, 0, 0) : new Color(204, 0, 0);
		public static Color Red2 { get; } = new Color(0xfe, 0x5c, 0x5c);
		public static Color LightRed { get; } = new Color(0xff, 0x66, 0x66);
		public static Color Pink { get; } = IsLight ? new Color(0xff, 0xc0, 0xc0) : new Color(164, 123, 123);
		public static Color Yellow { get; } = new Color(255, 255, 40);
		public static Color LightYellow { get; } = new Gdk.Color(0xe1, 0xd6, 0x70);
		public static Color LightYellow2 { get; } = new Color(255, 243, 199);
		public static Color White { get; } = new Color(0xff, 0xff, 0xff);
		public static Color Orange { get; } = new Color(0xfc, 0x66, 0x00);
		public static Color LightGray { get; } = new Color(0xcc, 0xcc, 0xcc);
		public static Color DarkGray { get; } = new Color(0x80, 0x80, 0x80);
		public static Color Green { get; } = new Color(14, 135, 14);
		public static Color LightGreen { get; } = new Color(0xc0, 0xff, 0xc0);
		public static Color Blue { get; } = IsLight ? new Color(0x00, 0x18, 0xf9) : new Color(0, 35, 164);
		public static Color BabyBlue { get; } = new Color(0x89, 0xcf, 0xef);
		public static Color LightPurple { get; } = new Color(199, 206, 255);
		public static Color LightCoral { get; } = new Color(240, 128, 128);
		public static Color Turquoise { get; } = new Color(64, 224, 208);

		#endregion AdditionalColors
	}
}
