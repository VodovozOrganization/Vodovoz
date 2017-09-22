

# Linux
## Для работы оптимизации маршрута, рядом с приложением должны лежать файлы:
Google.OrTools.dll
Google.Protobuf.dll
libGoogle.OrTools.so

В системной /usr/lib64 папке должна быть установлена библиотека libortools.so
Или приложение должно запускаться так LD_LIBRARY_PATH=lib: mono Vodovoz.exe (где lib путь до папки с библиотекой)

# Сервер
## Запустить службу рассчета расстояний (OSRM) на сервере можно следующим способом.
1. Заходим на сервер под пользователем admin
1. Убедимся что запущена служба докера 
  `sudo systemctl status docker.service`
2. Переходим в папку с файлами данных службы OSRM
  `cd osrm/`
3. Запускаем сам контейнер
`sudo docker run -t -i -p 5000:5000 -v $(pwd):/osrm osrm/osrm-backend osrm-routed /osrm/RU-LEN.osrm`
