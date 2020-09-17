using System;
using System.IO;
using System.Linq;

namespace SmsPaymentService
{
    public class FileProvider
    {
        public FileProvider(string filePath)
        {
            this.filePath = filePath;
        }

        private readonly string filePath;
        private readonly Object locker = new Object();

        public void WriteExternalIdToFile(int smsPaymentId, int externalId)
        {
            lock (locker) {
                if(!File.Exists(filePath))
                    using (File.Create(filePath)) { }

                using (StreamWriter sw = new StreamWriter(filePath)) {
                    sw.WriteLine($"{smsPaymentId},{externalId}");
                }
            }
        }

        public string[] GetAllLines()
        {
            lock (locker) {
                if(!File.Exists(filePath))
                    using (File.Create(filePath)) { }
                
                return File.ReadAllLines(filePath);
            }
        }

        public void RemoveLines(string[] linesToRemove)
        {
            lock (locker) {
                var lines = File.ReadAllLines(filePath);
                File.WriteAllLines(filePath, lines.Except(linesToRemove).ToArray());
            }
        }
    }
}