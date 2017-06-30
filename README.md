

# Linux
## Для работы оптимизации маршрута, рядом с приложением должны лежать файлы:
Google.OrTools.dll
Google.Protobuf.dll
libGoogle.OrTools.so

В системной /usr/lib64 папке должна быть установлена библиотека libortools.so
Или приложение должно запускаться так LD_LIBRARY_PATH=lib: mono Vodovoz.exe (где lib путь до папки с библиотекой)
