using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;

namespace Vodovoz.Testing.Database
{
	/// <summary>
	/// Дистрибутив MariaDB: один раз скачивается, проверяется по SHA256 и лежит в кеше вне каталога сборки.
	/// </summary>
	public static class MariaDbDistribution
	{
		private const string _cacheVariable = "VODOVOZ_TEST_MARIADB_CACHE";
		private const string _mutexName = "VodovozTestMariaDbDistribution";
		private const int _downloadAttempts = 3;

		public static string CacheDirectory
		{
			get
			{
				var configured = Environment.GetEnvironmentVariable(_cacheVariable);
				var root = string.IsNullOrWhiteSpace(configured)
					? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vodovoz", "test-mariadb")
					: configured;

				return Path.Combine(root, MariaDbVersion.Version);
			}
		}

		public static string BinDirectory => Path.Combine(CacheDirectory, MariaDbVersion.ArchiveName, "bin");

		public static bool IsCached => File.Exists(Path.Combine(BinDirectory, "mysqld.exe"));

		/// <summary>
		/// Гарантирует наличие дистрибутива в кеше. Параллельные сборки не качают архив дважды.
		/// </summary>
		public static void EnsureCached()
		{
			if(IsCached)
			{
				return;
			}

			using(var mutex = new Mutex(false, _mutexName))
			{
				var acquired = false;

				try
				{
					try
					{
						acquired = mutex.WaitOne(TimeSpan.FromMinutes(30));
					}
					catch(AbandonedMutexException)
					{
						acquired = true;
					}

					if(!acquired)
					{
						throw new TimeoutException("Не дождались загрузки MariaDB другим процессом");
					}

					if(!IsCached)
					{
						Download();
					}
				}
				finally
				{
					if(acquired)
					{
						mutex.ReleaseMutex();
					}
				}
			}
		}

		private static void Download()
		{
			var parent = Directory.GetParent(CacheDirectory).FullName;
			Directory.CreateDirectory(parent);

			var workDirectory = Path.Combine(parent, $"download_{Guid.NewGuid():N}");
			Directory.CreateDirectory(workDirectory);

			try
			{
				var archivePath = Path.Combine(workDirectory, "mariadb.zip");
				DownloadWithRetries(archivePath);
				VerifySha256(archivePath);

				var extractPath = Path.Combine(workDirectory, "extracted");
				ZipFile.ExtractToDirectory(archivePath, extractPath);

				if(Directory.Exists(CacheDirectory))
				{
					Directory.Delete(CacheDirectory, true);
				}

				Directory.Move(extractPath, CacheDirectory);
			}
			finally
			{
				Directory.Delete(workDirectory, true);
			}
		}

		private static void DownloadWithRetries(string path)
		{
			using(var client = new HttpClient { Timeout = TimeSpan.FromMinutes(20) })
			{
				for(var attempt = 1; ; attempt++)
				{
					try
					{
						using(var response = client.GetAsync(MariaDbVersion.ArchiveUrl, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult())
						using(var file = File.Create(path))
						{
							response.EnsureSuccessStatusCode();
							response.Content.ReadAsStreamAsync().GetAwaiter().GetResult().CopyTo(file);
						}

						return;
					}
					catch(Exception) when(attempt < _downloadAttempts)
					{
						Thread.Sleep(TimeSpan.FromSeconds(attempt * 2));
					}
				}
			}
		}

		private static void VerifySha256(string path)
		{
			using(var file = File.OpenRead(path))
			using(var sha256 = SHA256.Create())
			{
				var actual = BitConverter.ToString(sha256.ComputeHash(file)).Replace("-", string.Empty);

				if(!string.Equals(actual, MariaDbVersion.ArchiveSha256, StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidOperationException(
						$"SHA256 архива MariaDB не совпадает: ожидался {MariaDbVersion.ArchiveSha256}, получен {actual}");
				}
			}
		}
	}
}
