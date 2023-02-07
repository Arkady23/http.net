# http.net
Multithreaded http.net server on C#
```
D:\work\httpd>http.net /?
Многопоточный http.net сервер версия 1.0, (C) kornienko.ru январь 2023.

ИСПОЛЬЗОВАНИЕ:
    http.net [Параметр1 Значение1] [Параметр2 Значение2] ...

    При необходимости указываются пары Параметр и Значение. Если значение текст и содержит
    пробелы, то его необходимо заключать в кавычки.

Параметры:                                                          Значения по умолчанию:
     -d      Папка, содержащая домены.                                        ../www/
     -i      Главный документ в папках. Главный документ в папке, заданной    index.html
             параметром -d используется для отображения страницы с кодом
             404 - файл не найден. Для сжатия трафика поддерживаются файлы,
             сжатые методом gzip вида имя.расширение.gz, например -
             index.html.gz или library.js.gz и т.д.
     -p      Порт, который прослушивает сервер.                               8080
     -b      Размер буферов чтения и записи.                                  16384
     -s      Количество одновременно обрабатываемых запросов. Максимальное    888
             число ограничивается только производительностью процессора и
             размером оперативной памяти.
     -q      Количество дополнительных запросов, хранящихся в очереди,        888
             если превышено количество поступивших одновременно запросов,
             заданных параметром -s. Если сумма обрабатываемых и ожидающих
             в очереди запросов будет превышена, то клиенту посылается
             отказ в обслуживании.
     -log    Размер журнала регистрации запросов в строках. Журнал состоит    10000
             из двух чередующихся версий http.net.x.log и http.net.y. Если
             задан размер менее 80, то журнал не ведётся.
     -post   Максимальный размер принимаемого запроса для передачи            33554432
             файлу-скрипту. Если он будет превышен, то запрос помещается в
             файл, имя которого передается скрипту в переменной окружения
             POST_DATA. Другие формируемые переменные окружения -
             QUERY_STRING, HTTP_COOKIE, REMOTE_ADDR. Если в данных запроса
             отсутствует директива form-..., то входящий поток данных
             целиком будет помещен в файл. Эта особенность может
             использоваться для передачи серверу файлов. При этом имя файла
             будет находиться в переменной окружения POST_DATA.
     -proc   Используемый оброботчик скриптов. Если нобходимо, то нужно       cscript.exe
             также включить полный путь к исполняемому файлу. По умолчанию
             используется встроенный в ОС Microsoft Windows компонент,
             очень быстрый обработчик - сервер сценариев (WSH), использующий
             языки JScript и VBScript.
     -args   Дополнительные параметры командной строки запуска оброботчика.
             При использовании cscript.exe в случае, если дополнительные
             параметры не заданы, используется параметр //Nologo.
     -ext    Расширение файлов-скриптов.                                      wsf

D:\work\httpd>
```
### История версий
В папке httpd всегда можно скачать последнюю рабочую версию: http.net.exe.  
1.0. Январь 2023. Первая версия.  
1.1. Февраль 2023. По рекомендации MS Corp. убраны массивы с динамически изменяющимися размерами в целях уменьшения нагрузки на системный сборщик мусора в ОП. В разработке...
