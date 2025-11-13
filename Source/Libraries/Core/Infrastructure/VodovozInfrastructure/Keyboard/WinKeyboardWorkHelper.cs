using System;
using System.Runtime.InteropServices;

namespace VodovozInfrastructure.Keyboard
{
	public static class WinKeyboardWorkHelper
	{
		private const int ENGLISH_LOCALE_ID = 1033;
		private const uint KLF_SETFORPROCESS = 0x00000100;

		// Константы для клавиши CapsLock
		private const int VK_CAPITAL = 0x14; // Код клавиши CapsLock
		private const int KEYEVENTF_EXTENDEDKEY = 0x1;
		private const int KEYEVENTF_KEYUP = 0x2;

		private static void SetKeyboardLayout(int localeId)
		{
			var pwszKlid = localeId.ToString("x8");
			var hkl = NativeMethods.LoadKeyboardLayout(pwszKlid, KLF_SETFORPROCESS);
			NativeMethods.ActivateKeyboardLayout(hkl, KLF_SETFORPROCESS);
		}

		public static void SwitchToEnglish()
		{
			SetKeyboardLayout(ENGLISH_LOCALE_ID);
		}

		public static void TurnOffCapsLock()
		{
			// Имитируем нажатие и отпускание клавиши CapsLock
			NativeMethods.keybd_event(VK_CAPITAL, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
			NativeMethods.keybd_event(VK_CAPITAL, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
		}

		public static bool IsEnKeyboardLayot()
		{
			IntPtr foregroundWindow = NativeMethods.GetForegroundWindow();
			uint process = NativeMethods.GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
			var keyboardLayout = NativeMethods.GetKeyboardLayout(process).ToInt32() & 0xFFFF;
			return keyboardLayout == ENGLISH_LOCALE_ID;
		}

		public static bool IsWindowsOs => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		public static bool IsCapsLockEnabled => NativeMethods.GetKeyState(VK_CAPITAL) != 0;

		private class NativeMethods
		{
			// Импорт функций из user32.dll

			[DllImport("user32.dll")]
			internal static extern IntPtr GetForegroundWindow();

			[DllImport("user32.dll")]
			internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);

			[DllImport("user32.dll")]
			internal static extern IntPtr GetKeyboardLayout(uint idThread);

			[DllImport("user32.dll")]
			internal static extern uint LoadKeyboardLayout(string pwszKLID, uint Flags);

			[DllImport("user32.dll")]
			internal static extern short GetKeyState(int keyCode);

			[DllImport("user32.dll")]
			internal static extern uint ActivateKeyboardLayout(uint hkl, uint Flags);

			[DllImport("user32.dll")]
			internal static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
		}
	}
}
