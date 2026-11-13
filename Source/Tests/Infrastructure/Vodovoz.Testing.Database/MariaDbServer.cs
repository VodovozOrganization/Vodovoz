using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Vodovoz.Testing.Database
{
	/// <summary>
	/// Портативная MariaDB, которую запускают сами тесты: один сервер на процесс, случайный порт на 127.0.0.1,
	/// настройки как на проде, данные во временной папке. Других подключений у тестов нет.
	/// </summary>
	public sealed class MariaDbServer
	{
		private const string _host = "127.0.0.1";
		private const string _dataDirectoryPrefix = "test_mariadb_";
		private static readonly TimeSpan _startTimeout = TimeSpan.FromSeconds(120);
		private static readonly TimeSpan _staleDataAge = TimeSpan.FromDays(1);

		private static readonly Lazy<MariaDbServer> _instance = new Lazy<MariaDbServer>(() => new MariaDbServer());
		private static readonly Lazy<string> _unavailableReason = new Lazy<string>(FindUnavailableReason);

		private readonly object _startLock = new object();
		private readonly StringBuilder _serverOutput = new StringBuilder();
		private Process _process;
		private IntPtr _job;
		private string _dataDirectory;
		private string _password;
		private bool _started;

		private MariaDbServer()
		{
		}

		public static MariaDbServer Instance => _instance.Value;

		/// <summary>
		/// Сборка на сервере CI: отсутствие тестового сервера там ошибка, а не пропуск теста.
		/// </summary>
		public static bool IsCi =>
			string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase);

		/// <summary>
		/// Быстрая проверка без запуска сервера. null — сервер можно запустить.
		/// </summary>
		public static string UnavailableReason => _unavailableReason.Value;

		public int Port { get; private set; }

		public IList<string> Warnings { get; private set; } = new List<string>();

		public void EnsureStarted()
		{
			lock(_startLock)
			{
				if(_started)
				{
					return;
				}

				MariaDbDistribution.EnsureCached();
				CleanStaleDataDirectories();
				Start();
				_started = true;
			}
		}

		/// <summary>
		/// Строка подключения к запущенному серверу. Проходит проверку <see cref="SafetyGuard"/>.
		/// </summary>
		/// <param name="database">Имя базы или null для служебного подключения</param>
		public string GetConnectionString(string database = null)
		{
			EnsureStarted();

			var builder = new MySqlConnectionStringBuilder
			{
				Server = _host,
				Port = (uint)Port,
				UserID = "root",
				Password = _password,
				SslMode = MySqlSslMode.None,
				AllowUserVariables = true
			};

			if(database != null)
			{
				builder.Database = database;
			}

			SafetyGuard.EnsureAllowed(builder.ConnectionString, Port);

			return builder.ConnectionString;
		}

		private static string FindUnavailableReason()
		{
			if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				return "Портативная MariaDB для тестов есть только для Windows";
			}

			if(MariaDbDistribution.IsCached)
			{
				return null;
			}

			try
			{
				Dns.GetHostAddresses(MariaDbVersion.ArchiveHost);
				return null;
			}
			catch(Exception ex)
			{
				return $"MariaDB нет в кеше {MariaDbDistribution.CacheDirectory}, а {MariaDbVersion.ArchiveHost} недоступен: {ex.Message}";
			}
		}

		private void Start()
		{
			_password = Guid.NewGuid().ToString("N");
			_dataDirectory = Path.Combine(Path.GetTempPath(), _dataDirectoryPrefix + Guid.NewGuid().ToString("N"));

			var settings = ProductionServerSettings.Load();

			InitializeDataDirectory();

			Port = FindFreePort();

			var defaultsFile = _dataDirectory + ".cnf";
			WriteDefaultsFile(defaultsFile, settings);

			_process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = Path.Combine(MariaDbDistribution.BinDirectory, "mysqld.exe"),
					Arguments = $"--defaults-file=\"{defaultsFile}\" --console",
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardError = true,
					RedirectStandardOutput = true
				}
			};
			_process.OutputDataReceived += (s, e) => AppendOutput(e.Data);
			_process.ErrorDataReceived += (s, e) => AppendOutput(e.Data);
			_process.Start();
			_process.BeginOutputReadLine();
			_process.BeginErrorReadLine();

			BindToCurrentProcess(_process);
			AppDomain.CurrentDomain.ProcessExit += (s, e) => Stop();

			using(var connection = WaitForServer())
			{
				Warnings = settings.Verify(connection);
			}

			foreach(var warning in Warnings)
			{
				Trace.TraceWarning(warning);
				Console.WriteLine(warning);
			}
		}

		private void InitializeDataDirectory()
		{
			var install = new ProcessStartInfo
			{
				FileName = Path.Combine(MariaDbDistribution.BinDirectory, "mariadb-install-db.exe"),
				Arguments = $"--datadir=\"{_dataDirectory}\" --password={_password}",
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardError = true,
				RedirectStandardOutput = true
			};

			using(var process = Process.Start(install))
			{
				var error = process.StandardError.ReadToEndAsync();
				var output = process.StandardOutput.ReadToEndAsync();

				if(!process.WaitForExit((int)_startTimeout.TotalMilliseconds) || process.ExitCode != 0)
				{
					throw new InvalidOperationException(
						$"Не удалось инициализировать каталог данных MariaDB. Если mysqld не запускается на сервере, проверьте наличие Visual C++ Redistributable. Вывод: {output.Result}{error.Result}");
				}
			}
		}

		private void WriteDefaultsFile(string path, ProductionServerSettings settings)
		{
			var technical = new Dictionary<string, string>
			{
				["basedir"] = Path.GetDirectoryName(MariaDbDistribution.BinDirectory).Replace('\\', '/'),
				["datadir"] = _dataDirectory.Replace('\\', '/'),
				["port"] = Port.ToString(),
				["bind_address"] = _host
			};
			// Ускоряют тесты и не влияют на результат SQL
			var performance = new Dictionary<string, string>
			{
				["innodb_flush_log_at_trx_commit"] = "2",
				["sync_binlog"] = "0",
				["innodb_doublewrite"] = "0",
				["innodb_buffer_pool_size"] = "268435456"
			};

			var overridden = settings.StartupVariables
				.Select(x => x.Key)
				.Intersect(technical.Keys.Concat(performance.Keys), StringComparer.OrdinalIgnoreCase)
				.ToList();

			if(overridden.Count > 0)
			{
				throw new InvalidOperationException($"Технические параметры не могут заменять настройки прода: {string.Join(", ", overridden)}");
			}

			var lines = new List<string> { "[mysqld]" };
			lines.AddRange(technical.Concat(performance).Concat(settings.StartupVariables).Select(x => $"{x.Key} = {x.Value}"));

			File.WriteAllLines(path, lines);
		}

		private MySqlConnection WaitForServer()
		{
			var builder = new MySqlConnectionStringBuilder
			{
				Server = _host,
				Port = (uint)Port,
				UserID = "root",
				Password = _password,
				SslMode = MySqlSslMode.None,
				Pooling = false
			};
			SafetyGuard.EnsureAllowed(builder.ConnectionString, Port);

			var stopwatch = Stopwatch.StartNew();

			while(true)
			{
				var connection = new MySqlConnection(builder.ConnectionString);

				try
				{
					connection.Open();
					return connection;
				}
				catch(MySqlException) when(stopwatch.Elapsed < _startTimeout && !_process.HasExited)
				{
					connection.Dispose();
					Thread.Sleep(200);
				}
				catch(MySqlException ex)
				{
					connection.Dispose();
					throw new InvalidOperationException($"Тестовая MariaDB не запустилась. Вывод mysqld: {_serverOutput}", ex);
				}
			}
		}

		private void AppendOutput(string line)
		{
			if(line != null)
			{
				lock(_serverOutput)
				{
					_serverOutput.AppendLine(line);
				}
			}
		}

		private void Stop()
		{
			try
			{
				if(_process != null && !_process.HasExited)
				{
					_process.Kill();
					_process.WaitForExit(5000);
				}

				DeleteDataDirectory(_dataDirectory);
			}
			catch(Exception ex) when(ex is IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException || ex is Win32Exception)
			{
				// Папка будет удалена при следующем запуске как устаревшая
			}
		}

		private static void DeleteDataDirectory(string path)
		{
			if(Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}

			var defaultsFile = path + ".cnf";

			if(File.Exists(defaultsFile))
			{
				File.Delete(defaultsFile);
			}
		}

		private static void CleanStaleDataDirectories()
		{
			foreach(var directory in Directory.GetDirectories(Path.GetTempPath(), _dataDirectoryPrefix + "*"))
			{
				if(DateTime.UtcNow - Directory.GetLastWriteTimeUtc(directory) < _staleDataAge)
				{
					continue;
				}

				try
				{
					DeleteDataDirectory(directory);
				}
				catch(Exception ex) when(ex is IOException || ex is UnauthorizedAccessException)
				{
					// Папка ещё занята другим процессом
				}
			}
		}

		private static int FindFreePort()
		{
			var listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();

			try
			{
				return ((IPEndPoint)listener.LocalEndpoint).Port;
			}
			finally
			{
				listener.Stop();
			}
		}

		/// <summary>
		/// Привязывает сервер к процессу тестов через Job Object: при обрыве тестов он завершается вместе с ними.
		/// </summary>
		private void BindToCurrentProcess(Process process)
		{
			_job = NativeMethods.CreateJobObject(IntPtr.Zero, null);

			var information = new NativeMethods.JobObjectExtendedLimitInformation
			{
				BasicLimitInformation = new NativeMethods.JobObjectBasicLimitInformation
				{
					LimitFlags = NativeMethods.JobObjectLimitKillOnJobClose
				}
			};

			var size = Marshal.SizeOf(typeof(NativeMethods.JobObjectExtendedLimitInformation));
			var pointer = Marshal.AllocHGlobal(size);

			try
			{
				Marshal.StructureToPtr(information, pointer, false);

				if(!NativeMethods.SetInformationJobObject(_job, NativeMethods.JobObjectExtendedLimitInformationClass, pointer, (uint)size)
					|| !NativeMethods.AssignProcessToJobObject(_job, process.Handle))
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
			finally
			{
				Marshal.FreeHGlobal(pointer);
			}
		}

		private static class NativeMethods
		{
			public const int JobObjectExtendedLimitInformationClass = 9;
			public const uint JobObjectLimitKillOnJobClose = 0x2000;

			[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			public static extern IntPtr CreateJobObject(IntPtr attributes, string name);

			[DllImport("kernel32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool SetInformationJobObject(IntPtr job, int informationClass, IntPtr information, uint length);

			[DllImport("kernel32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

			[StructLayout(LayoutKind.Sequential)]
			public struct JobObjectBasicLimitInformation
			{
				public long PerProcessUserTimeLimit;
				public long PerJobUserTimeLimit;
				public uint LimitFlags;
				public UIntPtr MinimumWorkingSetSize;
				public UIntPtr MaximumWorkingSetSize;
				public uint ActiveProcessLimit;
				public UIntPtr Affinity;
				public uint PriorityClass;
				public uint SchedulingClass;
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct IoCounters
			{
				public ulong ReadOperationCount;
				public ulong WriteOperationCount;
				public ulong OtherOperationCount;
				public ulong ReadTransferCount;
				public ulong WriteTransferCount;
				public ulong OtherTransferCount;
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct JobObjectExtendedLimitInformation
			{
				public JobObjectBasicLimitInformation BasicLimitInformation;
				public IoCounters IoInfo;
				public UIntPtr ProcessMemoryLimit;
				public UIntPtr JobMemoryLimit;
				public UIntPtr PeakProcessMemoryUsed;
				public UIntPtr PeakJobMemoryUsed;
			}
		}
	}
}
