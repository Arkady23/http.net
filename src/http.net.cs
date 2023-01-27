using System;
using System.IO;
using System.Web;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class httpd{
  public static int port=8080, qu=80, bu=16384, st=80, log9=524200, post=33554432;
  public static string DocumentRoot="../www/", DirectoryIndex="index.html",
                       Proc="cscript.exe", Args="", Ext="wsf",
                       logX="http.net.x.log", logY="http.net.y.log", logZ="",
                       DirectorySessions="Sessions";
  public static FileStream log = null;
  public static Task logt = null;
  public static Task logf;
  Socket Server = null;
  Session[] Session = new Session[st];

  public void RunServer(){
    int i;
    if(Directory.Exists(DirectorySessions)) Directory.Delete(DirectorySessions,true);
    Server = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
    IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
    Server.Bind(ep);
    Server.Listen(qu);
    for(i = 0; i < st; i++) Session[i] = new Session(Server);
  }
  public void StopServer(){
    Server.Close(8888);
    if (logt != null){
      logt.Wait();
      log.Close();
    }
  }
}

class Session{
  const string CL="Content-Length",CT="Content-Type", CD="Content-Disposition",
               CC="Cache-Control: public, max-age=2300000\r\n";
  Encoding Edos = System.Text.Encoding.GetEncoding(866);
  private int i,k,Content_Length;
  private string bytes1, h1, reso, res, head, Host, Content_Type, Content_Disposition, Cookie,
                 QUERY_STRING, User_Agent, Referer, Accept_Language, Origin, IP, Port, x1;
  private byte[] bytes;
  private byte l, R, R1, R2;

  public Session(Socket Server){
    Accept(Server);
  }
  public async void Accept(Socket Server){
    await AcceptProc(await Server.AcceptAsync(), Server);
  }
  public async Task AcceptProc(Socket Client, Socket Server){
    using(var Stream = new NetworkStream(Client,true)){
      IPEndPoint Point = Client.RemoteEndPoint as IPEndPoint;
      string dt1=DateTime.UtcNow.ToString("R"),
             dt=DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff"),
             Content_T=CT+": text/plain\r\n";
      l=1;
      R=R1=R2=0;
      i=httpd.bu;
      k=Content_Length=0;
      IP=Point.Address.ToString();
      Port=Point.Port.ToString();
      x1=dt+"\t"+IP+" "+Port+"\t";
      bytes = new Byte[i];
      bytes1=head=h1=reso=Host=Content_Type=Content_Disposition=QUERY_STRING=Cookie=
             Referer=Origin=User_Agent=Accept_Language="";

      while (i>0 && l>0){
        if(k>0){
          bytes1=Edos.GetString(bytes,k,i-k);
          k=0;
        }
        i = await Stream.ReadAsync(bytes, 0, bytes.Length);

        if(i>0){
          l = getHeaders(Edos, ref bytes, ref bytes1, ref k, ref reso, ref Host,
                         ref User_Agent, ref Referer, ref Accept_Language, ref Origin,
                         ref Cookie, ref Content_Type, ref Content_Disposition,
                         ref Content_Length);
        }else{
          R2=1;
        }
      }

      res=prepResource(ref reso, ref QUERY_STRING, ref Host, ref R, ref h1, ref Content_T);
      R1=log(x1+res);

      if(R>0){
        head="HTTP/1.1 200 OK\r\nDate: "+dt1+"\r\n"+h1+Content_T;
        if(R==2){
          if(File.Exists(res)){
            await send_wsf(Stream);
          }else{
            R=1;
          }
        }
        if(R==1){
          if(!gzExists(ref res, ref head)){
            if(!File.Exists(res)){
              res=httpd.DocumentRoot+httpd.DirectoryIndex;
              gzExists(ref res, ref head);
            }
          }
          await type(Stream);
        }
      }
      Stream.Close();
      while(R1!=0) if((R1=log(x1+res)) !=0) Thread.Sleep(123);
    }
    Task RantAccept = Task.Run(()=>Accept(Server));
  }

  static byte log(string x){
    // Добавить сообщение в журнал с чередующимися версиями.
    // Сначала писать в X, затем в Y, затем снова в X и т.д.

    if(httpd.log9>0){
      // Выбрать в какой файл писать logX или logY
      string old=httpd.logZ;
      if(File.Exists(httpd.logX)){
        if(new FileInfo(httpd.logX).Length<httpd.log9){
          httpd.logZ=httpd.logX;
        }else{
          if(File.Exists(httpd.logY)){
            if(new FileInfo(httpd.logY).Length<httpd.log9){
              httpd.logZ=httpd.logY;
            }else{
              httpd.logZ=(File.GetLastWriteTime(httpd.logX)<File.GetLastWriteTime(httpd.logY))?
                          httpd.logX : httpd.logY;
            }
          }else{
            httpd.logZ=httpd.logY;
          }
        }
      }else{
        httpd.logZ=httpd.logX;
      }

      // Нужно ли переключение на другую часть?
      if(old.Length>0 && httpd.logZ!=old){
        if(httpd.logt != null) httpd.logt.Wait();
        httpd.log.Close();
        httpd.log=null;
      }

      // Теперь можно записать в файл logZ
      if(!(httpd.log!=null))
           httpd.log = File.Open(httpd.logZ,FileMode.Append,FileAccess.Write,FileShare.ReadWrite);
      byte[] b = System.Text.Encoding.UTF8.GetBytes(x+"\r\n");
      if(httpd.logt != null){
        httpd.logt.Wait();
        httpd.logf.Wait();
      }
      try{
        httpd.logt = httpd.log.WriteAsync(b,0,b.Length);
      }catch(IOException){
        return 1;
      }
    }
    httpd.logf=httpd.log.FlushAsync();
    return 0;
  }

  static string ltri(ref string x){
    return x.TrimStart('\t',' ');
  }

  static string beforStr1(ref string x, string Str){
    int k=0;
    if(Str.Length>0) k=x.IndexOf(Str);
    return k<0?x:(k>0?x.Substring(0,k):"");
  }

  static string afterStr1(ref string x, string Str){
    if(Str.Length>0){
      int k=x.IndexOf(Str);
      return k<0?"":x.Substring(k+Str.Length);
    }else{
      return x;
    }
  }

  static string beforStr9(ref string x, string Str){
    if(Str.Length>0){
       int k=x.LastIndexOf(Str);
       return k<0?x:(k>0?x.Substring(0,k):"");
    }else{
       return x;
    }
  }

  static string afterStr9(ref string x, string Str){
    int k= -1;
    if(Str.Length>0) k=x.LastIndexOf(Str);
    return k<0?"":x.Substring(k+Str.Length);
  }

  static void putCT(ref string c, string x){
    c=CT+": "+x+"\r\n";
  }

  static void putSl(ref string x){
    if(!x.EndsWith("/")) x+="/";
  }

  static string valStr(ref string x, string Str){
    string z="";
    if(x.Length>0){
      z=afterStr1(ref x," "+Str+"=");
      if(z.Length>0){
        if(z.Substring(0,1)=="\""){
          z=z.Substring(1);
          z=beforStr1(ref z,"\"");
        }else{
          z=beforStr1(ref z,";");
        }
      }
    }
    return z;
  }

  static bool gzExists(ref string res, ref string head){
    string gz=res+".gz";
    bool L=File.Exists(gz);
    if(L){
      res=gz;
      head+="Content-Encoding: gzip\r\n";
    }
    return L;
  }

  static string line1(Encoding Edos, ref byte[] bytes, ref string bytes1, ref int k, ref byte b){
    int i;
    string z=bytes1;
    bytes1="";
    b=1;

    for (i = k; i < bytes.Length; i++){
      if(bytes[i]==13){
        if(i+1 < bytes.Length){
          if(bytes[i+1]==10){
            z+=Edos.GetString(bytes,k,i-k);
            k+=i-k+2;
            b=0;
            break;
          }
        }
      }else if(bytes[i]==10){
        z+=Edos.GetString(bytes,k,i-k);
        k+=i-k+1;
        b=0;
        break;
      }
    }
    return z;
  }

  static byte getHeaders(Encoding Edos, ref byte[] bytes, ref string bytes1, ref int k, ref string reso,
                 ref string Host, ref string User_Agent, ref string Referer,
                 ref string Accept_Language, ref string Origin, ref string Cookie,
                 ref string Content_Type, ref string Content_Disposition,
                 ref int Content_Length){
    byte b=0;
    string lin=line1(Edos, ref bytes, ref bytes1, ref k, ref b),n,h;

    while (lin.Length>0){
//      Console.WriteLine("|"+lin+"|");
//      log(lin);
      h=afterStr1(ref lin,":");
      h=ltri(ref h);
      if(h.Length>0){
        n=beforStr1(ref lin,":");
        switch(n){
        case "Host":
          Host=h;
          break;
        case "User-Agent":
          User_Agent=h;
          break;
        case "Referer":
          Referer=h;
          break;
        case "Accept-Language":
          Accept_Language=h;
          break;
        case "Origin":
          Origin=h;
          break;
        case "Cookie":
          Cookie=h;
          break;
        case CT:
          Content_Type=h;
          break;
        case CD:
          Content_Disposition=h;
          break;
        case CL:
          Content_Length=Int32.Parse(h);
          break;
        }
      }else{
        h=afterStr1(ref lin," ");
        h=beforStr9(ref h," ");
        reso=ltri(ref h);
      }
      lin=line1(Edos, ref bytes, ref bytes1, ref k, ref b);
    }
    return b;
  }

  static string prepResource(ref string reso, ref string QUERY_STRING, ref string Host,
                             ref byte R, ref string h1, ref string Content_T){
    string ext,sub,res="";
    if(reso.Length>0 && Host.Length>0){
      res=HttpUtility.UrlDecode(reso);
      QUERY_STRING=afterStr1(ref res,"?");
      res=beforStr1(ref res,"?");
      sub=beforStr1(ref Host,":");
      ext=afterStr9(ref res,".");
      if(ext.Length>0){
        R=1;
        switch(ext){
        case "html":
          putCT(ref Content_T,"text/html");
          h1=CC;
          break;
        case "svg":
          putCT(ref Content_T,"image/svg+xml");
          h1=CC;
          break;
        case "png":
          putCT(ref Content_T,"image/png");
          h1=CC;
          break;
        case "jpeg":
        case "jpg":
          putCT(ref Content_T,"image/jpeg");
          h1=CC;
          break;
        case "js":
          putCT(ref Content_T,"text/javascript");
          h1=CC;
          break;
        case "css":
          putCT(ref Content_T,"text/css");
          h1=CC;
          break;
        case "ico":
          putCT(ref Content_T,"image/x-icon");
          h1=CC;
          break;
        case "mp4":
          putCT(ref Content_T,"video/mp4");
          h1=CC;
          break;
        default:
          if(ext==httpd.Ext){
            R=2;
            putCT(ref Content_T,"text/html");
          }
          break;
        }
      }else{
        R=1;
        putSl(ref res);
        putCT(ref Content_T,"text/html");
        res=res+httpd.DirectoryIndex;
      }
      putSl(ref httpd.DocumentRoot);
      res=httpd.DocumentRoot+sub+res;
    }
    return res;
  }

  async Task type(System.Net.Sockets.NetworkStream Stream){
    // Загрузка файла побайтно через TextStream с использованием буфера 100 тыс. байт
    using (FileStream ts = File.OpenRead(res)){
      head+=CL+": "+ts.Length+"\r\n\r\n";
      byte[] b = System.Text.Encoding.UTF8.GetBytes(head);
      int i=httpd.bu, k=0;

      if(b.Length<i){
        k=b.Length;
        Array.Resize(ref b, i);
      }else{
//        Stream.Write(b,k,b.Length);
        await Stream.WriteAsync(b,k,b.Length);
      }

      while ((i = await ts.ReadAsync(b,k,b.Length-k)) > 0){
        if(ts.Length==ts.Position){
          // Добавляем в конец перенос строки
          k+=2;
          Array.Resize(ref b, i+k);
          b[b.Length-1]=10;
          b[b.Length-2]=13;
        }
        await Stream.WriteAsync(b,0,i+k);
        if(k>0) k=0;
      }
      ts.Close();
    }
  }

  async Task send_wsf(System.Net.Sockets.NetworkStream Stream){
    int i,N=0;
    string cont, dirname="", filename="";
    var wsf = new ProcessStartInfo();

    wsf.EnvironmentVariables["QUERY_STRING"] = QUERY_STRING;
    wsf.EnvironmentVariables["HTTP_COOKIE"] = Cookie;
    wsf.EnvironmentVariables["REMOTE_ADDR"] = IP;
    if(Content_Length>0){
      dirname=httpd.DirectorySessions+"/"+IP+"_"+Port;
      wsf.RedirectStandardInput = true;
      // Поставить разумное ограничение на размер потока
      if(Content_Type.LastIndexOf("form-")<0 || Content_Length>httpd.post){
        filename=dirname+"/"+DateTime.Now.ToString("HHmmssfff");
        wsf.EnvironmentVariables["POST_DATA"] = filename;
      }
    }
    wsf.RedirectStandardOutput = true;
    wsf.UseShellExecute = false;
    wsf.CreateNoWindow = true;
    wsf.FileName = httpd.Proc;
    wsf.Arguments = httpd.Args+"\""+res+"\"";
    Process Proc = Process.Start(wsf);
    if(Content_Length>0){
      Task ft = null;
      Task swt = null;
      FileStream file = null;
      StreamWriter sw = Proc.StandardInput;
      // R2==0 означает, что не все данные со входа Stream прочитаны
      if(k>0 && k<bytes.Length){
        i = bytes.Length-k;
      }else{
        k=0;
        i = await Stream.ReadAsync(bytes,k,bytes.Length);
      }
      // Записать данные из буфера bytes
      // выделить файлы отдельно, а переменные в поток в соответсвии с примером:
/*
POST /test.html HTTP/1.1
Host: example.org
Content-Type: multipart/form-data;boundary="boundary"

--boundary
Content-Disposition: form-data; name="field1"

value1
--boundary
Content-Disposition: form-data; name="field2"; filename="example.txt"

value2
--boundary--
*/
      // Пока не реализовано, да и не пользуюсь стандартом form-data :(((((

      while (N<Content_Length){
        if(filename.Length>0){
          // Всё записывать в файл
          if (!(file != null)){
            // Открыть файл, если он не открыт
            if (File.Exists(filename)){
              File.Delete(filename);
            }else if(!Directory.Exists(dirname)){
              Directory.CreateDirectory(dirname);
            }
            file = new FileStream(filename,FileMode.Create);
          }
          if(bytes1.Length>0){
            // Это записать в поток, не в файл
            if (swt != null) await swt;
            swt = sw.WriteAsync(bytes1);
            bytes1="";
          }
          if (ft != null) await ft;
          ft = file.WriteAsync(bytes,k,i);
          N+=i;
        }else{
          // Всё записывать в поток
          if(i>0){
            bytes1+=Edos.GetString(bytes,k,i);
          }else{
            N=Content_Length;
          }
          if(bytes1.Length>0){
            N+=bytes1.Length;
            if(N>Content_Length) bytes1=bytes1.Substring(0,N-Content_Length);
            if (swt != null) await swt;
            swt = sw.WriteAsync(bytes1);
            bytes1="";
          }
        }
        if(N<Content_Length && R2==0){
          i = await Stream.ReadAsync(bytes,0,bytes.Length);
        }
        if(R2>0) N=Content_Length;
      }
      if (ft != null) await ft;
      if (file != null && file.CanRead) file.Close();
      if (swt != null) await swt;
      sw.Close();
    }

    // Вывод полученных данных wsf-скрипта
    cont=Proc.StandardOutput.ReadToEnd();
    byte[] cont1 = Encoding.UTF8.GetBytes(head+CL+": "+cont.Length+"\r\n\r\n"+
               Encoding.UTF8.GetString(Edos.GetBytes(cont))+"\r\n");
    await Stream.WriteAsync(cont1,0,cont1.Length);

    Proc.WaitForExit();
    // Освободить резурсы
    Proc.Dispose();
  }

}

class catnet{
  static void Main(string[] Args){
    Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(
              System.Reflection.Assembly.GetEntryAssembly().Location));
    httpd httpd = new httpd();
    if(getArgs(Args)){
      if(httpd.Args.Length==0){
        if(httpd.Proc.Substring(httpd.Proc.Length-11,11)=="cscript.exe" ||
           httpd.Proc.Substring(httpd.Proc.Length-7,7)=="cscript") httpd.Args="//Nologo ";
      }else{
        httpd.Args+=" ";
      }
      httpd.RunServer();
      Console.TreatControlCAsInput=true;
      Console.ReadKey(true);
      Console.WriteLine("Finalizing...");
      httpd.StopServer();
      Console.WriteLine("Server stoped :(((");
    }
  }
  static bool getArgs(String[] Args){
    int i, k, b9=131072, p9=65535, q9=2147483647, s9=15383, post9=33554432, log1=1024;
    bool l=true;
    // Разбор параметров
    for (i = 0; i < Args.Length; i++){
      switch (Args[i]){
      case "-p":
        i++;
        if(i < Args.Length){
          k=int.Parse(Args[i]);
          if(k > 0 && k <= p9) httpd.port=k;
        }
        break;
      case "-b":
        i++;
        if(i < Args.Length){
          k=int.Parse(Args[i]);
          httpd.bu=(k > 0 && k <= b9)? k : b9;
        }            
        break;
      case "-q":
        i++;
        if(i < Args.Length){
          k=int.Parse(Args[i]);
          httpd.qu=(k > 0 && k <= q9)? k : q9;
        }            
        break;
      case "-s":
        i++;
        if(i < Args.Length){
          k=int.Parse(Args[i]);
          httpd.st=(k > 0 && k <= s9)? k : s9;
        }            
        break;
      case "-log":
        i++;
        if(i < Args.Length){
          k=int.Parse(Args[i]);
          httpd.log9=(k < log1)? 0 : k;
        }            
        break;
      case "-post":
        i++;
        if(i < Args.Length){
          k=int.Parse(Args[i]);
          httpd.post=(k > 0)? k : post9;
        }            
        break;
      case "-d":
        i++;
        if(i < Args.Length) httpd.DocumentRoot=Args[i];
        break;
      case "-i":
        i++;
        if(i < Args.Length) httpd.DirectoryIndex=Args[i];
        break;
      case "-proc":
        i++;
        if(i < Args.Length) httpd.Proc=Args[i];
        break;
      case "-args":
        i++;
        if(i < Args.Length) httpd.Args=Args[i];
        break;
      case "-ext":
        i++;
        if(i < Args.Length) httpd.Ext=Args[i];
        break;
      default:
        Console.Write(@"Многопоточный http.net сервер версия 1.0, (C) kornienko.ru январь 2023.

ИСПОЛЬЗОВАНИЕ:
    http.net [Параметр1 Значение1] [Параметр2 Значение2] ...

    При необходимости указываются пары Параметр и Значение. Если значение текст и содержит
    пробелы, то его необходимо заключать в кавычки.

Параметры:                                                          Значения по умолчанию:
     -d      Папка, содержащая поддомены.                                     "+httpd.DocumentRoot+@"
     -i      Главный документ в папках. Главный документ в папке, заданной    "+httpd.DirectoryIndex+@"
             параметром -d используется для отображения страницы с кодом
             404 - файл не найден. Для сжатия трафика поддерживаются файлы,
             сжатые методом gzip вида имя.расширение.gz, например -
             index.html.gz или library.js.gz и т.д.
     -p      Порт, который прослушивает сервер.                               "+httpd.port.ToString()+@"
     -b      Размер буферов чтения и записи.                                  "+httpd.bu.ToString()+@"
     -s      Количество одновременно обрабатываемых запросов. Максимальное    "+httpd.st.ToString()+@"
             число ограничивается только производительностью процессора и
             размером оперативной памяти.
     -q      Количество дополнительных запросов, хранящихся в очереди,        "+httpd.qu.ToString()+@"
             если превышено количество поступивших одновременно запросов,
             заданных параметром -s. Если сумма обрабатываемых и ожидающих
             в очереди запросов будет превышена, то клиенту посылается
             отказ в обслуживании.
     -log    Размер журнала регистрации запросов. Журнал состоит из двух      "+httpd.log9.ToString()+@"
             чередующихся версий http.net.x.log и http.net.y.log. Если
             задан размер менее 1024, то журнал не ведётся.
     -post   Максимальный размер принимаемого запроса для передачи            "+httpd.post.ToString()+@"
             файлу-скрипту. Если он будет превышен, то запрос помещается в
             файл, имя которого передается скрипту в переменной окружения
             POST_DATA. Другие формируемые переменные окружения -
             QUERY_STRING, HTTP_COOKIE, REMOTE_ADDR. Если в данных запроса
             отсутствует директива form-..., то входящий поток данных
             целиком будет помещен в файл. Эта особенность может
             использоваться для передачи серверу файлов. При этом имя файла
             будет находиться в переменной окружения POST_DATA.
     -proc   Используемый оброботчик скриптов. Если нобходимо, то нужно       "+httpd.Proc+@"
             также включить полный путь к исполняемому файлу. По умолчанию
             используется встроенный в ОС Microsoft Windows компонент,
             очень быстрый обработчик - сервер сценариев (WSH), использующий
             языки JScript и VBScript.
     -args   Дополнительные параметры командной строки запуска оброботчика.
             При использовании cscript.exe в случае, если дополнительные
             параметры не заданы, используется параметр //Nologo.
     -ext    Расширение файлов-скриптов.                                      "+httpd.Ext+@"
");
        l=false;
        break;
      }
    }
    return l;
  }
}
