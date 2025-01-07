# http.net and https.net
Multithreaded http.net and https.net C# servers using the dotNet v4 framework included in Windows 10/11 by default.  
Многопоточный http.net и https.net серверы на C# с использованием фреймворка dotNet v4, входящего в Windows 10/11 по умолчанию.  

The root folder for domains (by default www) must contain folders that match the domain name and subdomain of the requested resource. For example, if the request looks like http://a.kornienko.ru or https://a.kornienko.ru, then there should be a folder named in the root folder for domains a.kornienko.ru. If you need to provide aliases with other names, then you can create a folder in the root folder as a symbolic link to another folder.  

The number of threads should not be set immediately to the maximum possible. If the values are very high, the operating system may block the server operation. In my case, the maximum value at which the server is running steadily turned out to be exactly 1000. The default is 888. You can check how quickly threads are initialized by using the log, which indicates the timestamps of the server startup and the readiness of tasks to receive requests.  

Processing of wsf scripts with a handler is provided cscript.exe. In the http server parameters, you can replace this script extension and handler with any other one. It also provides processing of prg scripts via COM MS technology with VFP 9/10(Advanced) DBMS, not CGI. COM objects are created as requests from simultaneously accessing clients are made to the maximum value specified in the server parameters. By default, the visual error output of the VFP 9/10(Advanced) DBMS is disabled. In case of an error in the prg, the description of this error is returned to the script in the ERROR_MESS variable. Below is an example of a prg file and the result of its work. And also the result of a similar prg file, but with an error (the last line break ";" is missing).
```
PS D:\> D:\work\httpd\http.net.exe /?
Multithreaded http.net server version 3.0.0, (C) a.kornienko.ru January 2025.

USAGE:
    http.net [Parameter1 Value1] [Parameter2 Value2] ...
    http.net @filename

    If necessary, Parameter and Value pairs are specified. If the value is text and contains
    spaces, then it must be enclosed in quotation marks. You can specify @filename which
    contains the entire line with parameters.

Parameters:                                                                  Default values:
     -d      Folder containing the domains.                                      ../www/
     -i      Main document is in the folders. The main document in the           index.html
             folder specified by the -d parameter is used to display the page
             with the 404 code - file was not found. To compress traffic,
             files compressed using gzip method of the name.expansion.gz type
             are supported, for example - index.html.gz or library.js.gz etc.
     -p      Port that the server is listening on.                               8080
     -b      Size of read/write buffers.                                         8192
     -s      Number of requests being processed at the same time. Maximum        400
             value is 1000.
     -q      Number requests stored in the queue.                                600
     -db     Maximum number of dynamically running MS VFP DBMS instances.        20
             Extending scripts to run VFP - prg. Processes are started as
             needed by simultaneous client requests to the set value. Maximum
             value is 1000.
     -log    Size of the query log in rows. The log consists of two              10000
             interleaved versions http.net.x.log and http.net.y.log. If the
             size is set to less than 80, then the log is not kept.
     -post   Maximum size of the accepted request to transfer to the script      33554432
             file. If it is exceeded, the request is placed in a file,
             the name of which is passed to the script in the environment
             variable POST_FILENAME. Other generated environment variables -
             SERVER_PROTOCOL, SCRIPT_FILENAME, QUERY_STRING, HTTP_HEADERS,
             REMOTE_ADDR. If the form-... directive is missing from the
             request data, then incoming data stream will be placed entirely
             in a file. This feature can be used to transfer files to the
             server. In this case, the file name will be in the environment
             variable POST_FILENAME.
     -proc   Script handler used. If necessary, you must also include            cscript.exe
             the full path to the executable file. By default, the component
             built into Microsoft Windows OS is used, a very fast script
             server handler (WSH) using the JScript and VBScript languages.
     -args   Additional parameters of the handler startup command line. When
             using cscript.exe if no additional parameters are specified,
             the //Nologo parameter is used.
     -ext    Extension of the script files.                                      wsf
```
Корневая папка для доменов (по умолчанию www) должна содержать папки, соответствующие доменному имени и поддомену запрашиваемого ресурса. Например, если запрос выглядит как http://a.kornienko.ru или https://a.kornienko.ru, то в корневой папке для доменов должна быть папка с именем a.kornienko.ru. Если вам нужно предоставить псевдонимам другие имена, вы можете создать папку в корневой папке в качестве символической ссылки на другую папку.  

Число потоков не следут задавать сразу максимально возможным. При очень больших значениях операционная система может заблокировать работу сервера. В моем случае максимальное значение при котором сервер работает устойчиво оказалось ровно 1000. По умолчанию — 20. Проверить, как быстро иницилизируются потоки можно по журналу, в котором указываются метки времени запуска сервера и готовности задач к приему запросов.  

Предусмотрена обработка wsf-скриптов с помощью обработчика cscript.exe. В параметрах http-сервера вы можете заменить это расширение скрипта и обработчик на любое другое. Он также обеспечивает обработку prg-скриптов по технологии COM MS с использованием СУБД VFP 9/10(Advanced), а не CGI. COM-объекты создаются по мере выполнения запросов от клиентов с одновременным доступом к максимальному значению, указанному в параметрах сервера. По умолчанию визуальный вывод ошибок в СУБД VFP 9/10(Advanced) отключен. В случае ошибки в prg описание этой ошибки возвращается скрипту в переменной ERROR_MESS. Ниже приведен пример файла prg и результат его работы. А также результат работы с аналогичным файлом prg, но с ошибкой (отсутствует разрыв последней строки ";").
```
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
*  Тест. Вывод переменных окружения.                   версия 07.01.2025  *
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
  c13 = chr(13) + chr(10)
  STD_INPUT = _Screen.STD_IO.value

* P.S. До этого присвоения информация из _Screen.STD_IO.value, находящаяся
* в качестве стандартного ввода, должна быть прочитана:
  _Screen.STD_IO.value = "Content-Type: text/html" + c13 + c13

  STD_Write("<h1>Привет мир из MS VFP!</h1>" + ;
     "<h3>Переменные окружения:</h3>" + ;
     "SCRIPT_FILENAME=" + SCRIPT_FILENAME + ";<br>" + c13 + ;
     "QUERY_STRING=" + QUERY_STRING + ";<br>" + c13+ ;
     "HTTP_COOKIE=" + STRE(HTTP_HEADERS,"Cookie:",c13) + ";<br>" + c13 + ;
     "REMOTE_ADDR=" + REMOTE_ADDR + ";<br>" + c13 + ;
     "STD_INPUT=" + STD_INPUT + ";<br>" + c13 + ;
     "POST_FILENAME=" + POST_FILENAME + ";<br>" + c13)
  STD_Write("ERROR_MESS=" + ERROR_MESS)
* P.S. Также при необходимости, если работают оба сервера (http.net и
* https.net) можно использовать переменную окружения m.SERVER_PROTOCOL.

func STD_Write(mess)
  _Screen.STD_IO.SelStart = len(_Screen.STD_IO.value)
  _Screen.STD_IO.SelText = mess
```
The visual result of the prg script:
![The visual result of the prg script](screenShots/2024-03-21.png)

If there is an error in the prg file:
![If there is an error in the prg file](screenShots/if-error-in-prg-file.png)
### История версий
В папках httpd и https всегда можно скачать последние рабочие версии http.net.exe и https.net.exe.  
  
1.0. Январь 2023. Первая версия.  
1.1. Февраль 2023. По рекомендации MS Corp. убраны массивы с динамически изменяющимися размерами в целях уменьшения нагрузки на системный сборщик мусора в ОП. Добавлено кэширование небольших файлов с целью увеличения скорости работы сервера.  
1.2. Апрель 2023. Реализовано сохранение буферов журнала запросов при бездействии сервера в течении 3 с.  
1.3. Май 2023. Исправлена ошибка буферизации при кешировании небольших файлов.  
1.4. Ноябрь 2023. При отсылке результата скрипта теперь не отсылается заголовок длины этого результата и не закрывается блок заголовков в целях соблюдения приемственности известных http-серверов, например такого, как Apache.  
1.5. Январь 2024. Добавлена переменная окружения SCRIPT_FILENAME.  
1.6. Январь 2024. Исправлена ошибка кодировки при скачивании файлов через пользовательские скрипты.  
1.7. Февраль 2024. Устранено исключение, появляющееся при закрытии сервера.  
1.8. Март 2024. Добавлено чтение имени файла из заголовка вида Content-Disposition: attachment; filename="filename.jpg" (Используется при загрузке файлов методом POST).  
1.81. Март 2024. Ужесточена безопасность. Клиент сможет делать запросы к ресурсам только в пределах папки с доменом/поддоменом и только к файлам с разрешенными расширениями. Добавлена возможность использования СУБД MS Foxpro 9/10(Advanced) через COM технологию с расширением скриптов — prg.  
1.82. Март 2024. Убран концевой перевод строки после возвращаемых данных пользовательских скриптов.  
1.83. Март 2024. Добавлено использование кодовой страницы конфигурации СУБД VFP при возвращении результата prg файла.  
1.9. Апрель 2024. Добавлен параметр используемой сервером кодовой страницы -cp для обработки текстовых потоков.  
1.91. Апрель 2024. Добавлено немного больше асинхронности между сессиями.  
1.92. Апрель 2024. Увеличена стабильность существования потоков сессий.  
1.93. Апрель 2024. Заменён класс асинхронного цикла верхнего уровня, порождающего потоки, на класс параллельного выполнения.  
1.94. Апрель 2024. Увеличено число рабочих потоков. Запмсь в журнал переведена в отдельный поток с меньшим приоритетом.   
1.95. Апрель 2024. Число рабочих потоков увеличено до максимума, до значения, заданного параметром -s. При этом контроль правильности задания этого параметра ложится на пользователя путем анализа журнала, где первые две записи фиксируют время запуска сервера и время инициализации задач.  
2.0-2.1. Май-июнь 2024. Оптимизация кода.  
2.11-2.12. Июнь 2024. Обработка случая разрыва связи с клиентом.  
2.13-2.14. Июнь 2024. Теперь переменная окружения SCRIPT_FILENAME содержит полный путь к скрипту.  
2.15. Июнь 2024. Предусмотрен случай закрытия процесса VFP пользовательским prg.  
2.2. Июль 2024. Оптимизация кода. Добавление разрешения для проверки доменов сервера методом http-1 при получении сертификатов, необходимых для работы через протокол https.   
2.21-2.22. July 2024. The default code page is now taken from the Windows code page data. The help text has been translated into English.  
2.23. July 2024. Synchronization bug.  
2.24. July 2024. Put request bug.  
2.3. August 2024. When sending the script result, the "Content-Type: text/html" header is no longer added in order to maintain the continuity of well-known http servers, such as Apache. Just like when using Apache and others, user now has to generate this header himself.  
2.31. August 2024. Fixed a bug when using the -i command line option. Added automatic substitution of the extension, if it is missing. First, a resource without an extension is perceived as a folder, if such a folder is missing, then default script extension for the script is added, if the file is missing, then the server tries to execute the script with the prg extension, if this file is not found, then the html extension is substituted.    
2.32. August 2024. Added recognition of the POST length limit for different versions of MS VFP and VFPA.  
2.33. August 2024. The HTTP_COOKIE environment variable has been removed, and HTTP_HEADERS has been added.  
2.3.4. August 2024. Fixed VFP COM crash error. The version has been moved from the Debug state to Release.  
2.3.5. August 2024. After finishing the prg script, the "clear prog" command is added.  
2.3.6. August 2024. Message is available for the case of using outdated versions of Windows.  
2.3.7. August 2024. Bug of incorrect processing of command line parameters has been fixed.  
2.3.8. August 2024. Fixed an exception that is likely to occur when a connection is broken.  
2.3.9. August 2024. Caching of small files is now done asynchronously.  
2.3.10. August 2024. Asynchrony of extracting files from the cache has been removed, and asynchrony of transferring files over a socket has been reduced due to errors that occur.  
2.3.11. August 2024. Additional command line parameter control is set.  
2.4. September 2024. Added file name decoding command to download files with names in national Windows encodings. If a critical VFP error occurs when trying to execute the prg script, then now the error message is converted to UTF-8 encoding.  
2.4.1. September 2024. Added the ability to replace the standard set of VFP commands to clean up trash after the script is finished before executing the next script written in prg. The availability of the new set of commands is checked in the sys(2004)+'VFPclear.prg' file. If no such prg is found, then the standard set of commands is executed: clean even, clear prog, clear all, close data all and close all.  
2.4.2. September 2024. The request log contains the number of the task in which it is being processed.  
2.5.0. October 2024. The European encoding has been replaced with UTF-8. After switching the log to another file, all entries in this log are deleted for easy readability.  
2.5.1. October 2024. Now the encoding specified in the charset parameter in the Content-Type header is applied, if it is specified correctly. UTF-8 is used by default.  
2.5.2. October 2024. Fixed a flaw for redirecting to index.html.  
2.5.3. October 2024. Fixed the error of adding a small file to the dictionary.  
2.5.4. October 2024. Caching of small files has been removed due to the lack of effect. Default read/write buffer size has been increased.  
2.5.5-2.6.0. October-November 2024. Fixing the task stack overflow error.  
2.6.1. November 2024. Default values have been changed for more stable server functioning. The SERVER_PROTOCOL environment variable has been added.  
2.6.2-2.6.3. November 2024. Added syntactic control of the query method. The value of the response duration in ms has been added to the log, the symbol "-" when the server refuses to process the request, and some other changes in the log.  
2.6.4. November 2024. Minor corrections. Fixed bug blocking the SSL stream in https.net server.  
2.6.5-2.6.6. November 2024. If the file name with the default extension matches the existing directory name, then the request is now considered for an existing file.  
2.6.7. November 2024. Information about the server version has been added to the log.  
2.6.8. November 2024. Fixed a bug for the API variant.  
2.6.9. November 2024. It is set to cancel the request if the client, having established a connection, does not start sending headers within 10 seconds. Such requests could block the server. If execution error occurs, the message is now sent to the http.net.err.log file.  
2.6.10. November 2024. The logs are switched between each other when one of them is filled up to a set number of lines.  
2.7.0. Added an option to start the server with the command line parameters located in the file.  
3.0.0. January 2025. The server engine has been replaced with the Microsoft engine. The program appearance has been changed from a console application to a form. The icon has been added to tray. Now a single socket buffer is used to read a request and the same for writing a response. The _Screen.STD_IO object has been added to VFP, which is used as standard input/output. The Return operator is now used only in API mode to return the HTTP status code.  
