using System;
using System.IO;
using System.Linq;
using System.Timers;
using NLog;

namespace VodovozOSMService
{
    public class RequestCounter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public RequestCounter(string filePath = null)
        {
            logger.Info("Запуск счётчика запросов...");
            if(filePath == null)
                path = "/var/log/VodovozOSMService/RequestCounter.txt";
            
            try {
                using (var fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite)) { }
                var lastLine = File.ReadAllLines(path).LastOrDefault(x => x.StartsWith(DateTime.Now.Date.ToString("d")));
                if(lastLine != null) {
                    var data = lastLine.Split(' ').ToList();

                    City = int.Parse(data[data.IndexOf(nameof(City)) + 1]);
                    CityByCriteria = int.Parse(data[data.IndexOf(nameof(CityByCriteria)) + 1]);
                    Street = int.Parse(data[data.IndexOf(nameof(Street)) + 1]);
                    StreetByCriteria = int.Parse(data[data.IndexOf(nameof(StreetByCriteria)) + 1]);
                    CityId = int.Parse(data[data.IndexOf(nameof(CityId)) + 1]);
                    PointRegion = int.Parse(data[data.IndexOf(nameof(PointRegion)) + 1]);
                    HouseNumbers = int.Parse(data[data.IndexOf(nameof(HouseNumbers)) + 1]);
                    BuildingCountInCity = int.Parse(data[data.IndexOf(nameof(BuildingCountInCity)) + 1]);
                    BuildingCountInRegion = int.Parse(data[data.IndexOf(nameof(BuildingCountInRegion)) + 1]);
                }
            }
            catch (Exception ex) {
                logger.Error(ex, "Ошибка при запуске счётчика запросов");
                return;
            }
            
            var timer = new Timer(30000);
            timer.Elapsed += TimerOnElapsed;
            timer.Enabled = true;
            logger.Info("Счётчик запросов запущен");
        }

        private readonly string path;
        public int City { get; set; }
        public int CityByCriteria { get; set; }
        public int Street { get; set; }
        public int StreetByCriteria { get; set; }
        public int CityId { get; set; }
        public int PointRegion { get; set; }
        public int HouseNumbers { get; set; }
        public int BuildingCountInCity { get; set; }
        public int BuildingCountInRegion { get; set; }
        
        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var requestSum = City + CityByCriteria + Street + StreetByCriteria + CityId + PointRegion + HouseNumbers + BuildingCountInCity + BuildingCountInRegion;
            string stringToSave = $"{DateTime.Now.Date:d} "
                + $"{nameof(City)} {City} "
                + $"{nameof(CityByCriteria)} {CityByCriteria} "
                + $"{nameof(Street)} {Street} "
                + $"{nameof(StreetByCriteria)} {StreetByCriteria} "
                + $"{nameof(CityId)} {CityId} "
                + $"{nameof(PointRegion)} {PointRegion} "
                + $"{nameof(HouseNumbers)} {HouseNumbers} "
                + $"{nameof(BuildingCountInCity)} {BuildingCountInCity} "
                + $"{nameof(BuildingCountInRegion)} {BuildingCountInRegion} "
                + $"Сумма: {requestSum}";
            logger.Info($"Сохранение данных счётчика в файл. Строка: {stringToSave}");
            
            try {            
                var allLines = File.ReadAllLines(path).ToList();
                var line = allLines.FirstOrDefault(x => x.StartsWith(DateTime.Now.Date.ToString("d")));
                if(line != null) {
                    allLines[allLines.IndexOf(line)] = stringToSave;
                    File.WriteAllLines(path, allLines);
                }
                else
                    File.AppendAllText(path, stringToSave + "\n");
            }
            catch (Exception ex) {
                logger.Error(ex, "Ошибка при сохранении данных счётчика в файл");
                return;
            }
            
            logger.Info("Сохранение завершено");
        }
    }
}