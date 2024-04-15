using System;
using System.IO;
using System.Web;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class httpd{
  public static int port=8080, qu=8888, bu=16384, st=8888, db=22, log9=10000,
                    post=33554432, le=524288, cp=1251, logi=0, i, k;
  public static string DocumentRoot="../www/", DirectoryIndex="index.html",
                       Proc="cscript.exe", Args="", Ext="wsf",
                       logX="http.net.x.log", logY="http.net.y.log", logZ="",
                       DirectorySessions="Sessions";
  public static Dictionary<string,byte[]> Files = new Dictionary<string,byte[]>();
  public static Type vfpa = Type.GetTypeFromProgID("VisualFoxPro.Application");
  public static StreamWriter logSW = null;
  public static FileStream logFS = null;
  public static TextWriter TW = null;
  public static TextWriter TE = null;
  public static dynamic[] vfp = null;
  public static byte[] vfpb = null;
  public static bool notexit=true;
  public static long DTi;
  Socket Server = null;
  Session[] Session = null;
  
  public void RunServer(){
    if(Directory.Exists(DirectorySessions)) Directory.Delete(DirectorySessions,true);
    Server = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
    Session = new Session[st];
    IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
    vfp = new dynamic[db];
    vfpb = new byte[db];
    Server.Bind(ep);
    Server.Listen(qu);
    if(log9>0) log("\tThe http-server is running...");
    Thread.Sleep(23);
    ThreadPool.GetMinThreads(out i, out k);
    i*=8; k*=8;
    ThreadPool.SetMinThreads(i,k);
    Parallel.For(0,st,j => { Session[j] = new Session(Server,j); });
  }

  public void StopServer(){
    notexit=false;
    if(vfpa != null) for(i=0; i<db; i++) if(vfpb[i]>0) try{vfp[i].Quit();}catch(System.Runtime.InteropServices.COMException){}
    if(logFS!=null){
      // Восстановить вывод на консоль
      Console.SetError(TE);
      Console.SetOut(TW);
      logSW.Close();
    }
  }

  public static void log(object x){
    // Добавить сообщение в журнал с чередующимися версиями.
    // Сначала писать в X, затем в Y, затем снова в X и т.д.
    Interlocked.Exchange(ref DTi,DateTime.UtcNow.Ticks);

    // Нужно ли начать запись с начала журнала?
    if(logi>=log9 && logFS!=null){
      Interlocked.Exchange(ref logi,1);
      logFS.Seek(0,SeekOrigin.Begin);
    }else{
      Interlocked.Increment(ref logi);
    }

    if(!(logFS!=null)){
      // Отправка вывода на консоль в файл:
      logZ=(File.GetLastWriteTime(logX)<=File.GetLastWriteTime(logY))? logX : logY;
      logFS = new FileStream(logZ,FileMode.OpenOrCreate,FileAccess.Write,FileShare.ReadWrite);
      TW = Console.Out;
      TE = Console.Error;
      logSW = new StreamWriter(logFS);
      Console.SetOut(logSW);
      Console.SetError(logSW);
    }

    // Записать в файл
    try{
      Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff")+"\t"+x);
      Thread.Sleep(3000);
      if(DTi+30000000<DateTime.UtcNow.Ticks){
        logSW.Flush();
        logFS.Flush();
      }
    }catch(IOException){
      Thread.Sleep(23); log2(x+"");
    }
  }

  public static void log2(string x){
    Thread log2 = new Thread(log);
    log2.Priority = ThreadPriority.BelowNormal;
    log2.Start(x);
  }
}

class Session{
  const string CL="Content-Length",CT="Content-Type", CD="Content-Disposition",
               CC="Cache-Control: public, max-age=2300000\r\n";
  Encoding Edos = Encoding.GetEncoding(28591);
  Encoding Ewin = Encoding.GetEncoding(httpd.cp);
  private int i,k,Content_Length;
  private string cont1, h1, reso, res, head, Host, Content_Type, Content_Disposition, Cookie,
                 QUERY_STRING, User_Agent, Referer, Accept_Language, Origin, IP, Port, x1;
  private byte[] bytes;
  private byte l, R, R1, R2;
  private Task AddCache = null;

  public Session(Socket Server, int sn){
//    if(httpd.log9>0) httpd.log2("\tStream number "+sn.ToString()+" is running...");
    Accept(Server);
  }

  public async void Accept(Socket Server){
    while (httpd.notexit) await AcceptProc(await Server.AcceptAsync(), Server);
  }

  public async Task AcceptProc(Socket Client, Socket Server){
    using(var Stream = new NetworkStream(Client,true)){
      IPEndPoint Point = Client.RemoteEndPoint as IPEndPoint;
      string dt1=DateTime.UtcNow.ToString("R"),
             Content_T=CT+": text/plain\r\n";
      l=1;
      R=R1=R2=0;
      i=httpd.bu;
      k=Content_Length=0;
      IP=Point.Address.ToString();
      Port=Point.Port.ToString();
      x1=IP+" "+Port+"\t";
      bytes = new Byte[i];
      cont1=head=h1=reso=Host=Content_Type=Content_Disposition=QUERY_STRING=Cookie=
             Referer=Origin=User_Agent=Accept_Language="";

      while (i>0 && l>0){
        if(k>0){
          cont1=Edos.GetString(bytes,k,i-k);
          k=0;
        }
        i = await Stream.ReadAsync(bytes, 0, bytes.Length);

        if(i>0){
          l = getHeaders(Edos, ref bytes, ref cont1, ref k, ref reso, ref Host,
                         ref User_Agent, ref Referer, ref Accept_Language, ref Origin,
                         ref Cookie, ref Content_Type, ref Content_Disposition,
                         ref Content_Length);
        }else{
          R2=1;
        }
      }

      res=prepResource(ref reso, ref QUERY_STRING, ref Host, ref R, ref h1, ref Content_T);

      if(R>0){
        if(httpd.log9>0) httpd.log2(x1+res);
        head="HTTP/1.1 200 OK\r\nDate: "+dt1+"\r\n"+h1+Content_T;
        if(R>1){
          if(File.Exists(res)){
            if(R==2){
              await send_wsf(Stream);
            }else{
              await send_prg(Stream);
            }
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
    }
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

  // Узнать значение поля в заголовке (может понадобиться при разборе заголовков)
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

  static string line1(Encoding Edos, ref byte[] bytes, ref string cont1, ref int k, ref byte b){
    int i;
    string z=cont1;
    cont1="";
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

  static byte getHeaders(Encoding Edos, ref byte[] bytes, ref string cont1, ref int k, ref string reso,
                 ref string Host, ref string User_Agent, ref string Referer,
                 ref string Accept_Language, ref string Origin, ref string Cookie,
                 ref string Content_Type, ref string Content_Disposition,
                 ref int Content_Length){
    byte b=0;
    string lin=line1(Edos, ref bytes, ref cont1, ref k, ref b),n,h;

    while (lin.Length>0){
// Console.WriteLine("lin=|"+lin+"|");
// httpd.log(lin);
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
          Content_Length=int.Parse(h);
          break;
        }
      }else{
        h=afterStr1(ref lin," ");
        h=beforStr9(ref h," ");
        reso=ltri(ref h);
      }
      lin=line1(Edos, ref bytes, ref cont1, ref k, ref b);
    }
    return b;
  }

  static string prepResource(ref string reso, ref string QUERY_STRING, ref string Host,
                             ref byte R, ref string h1, ref string Content_T){
    string sub,res="",ext=".";
    if(reso.Length>0 && Host.Length>0){
      res=HttpUtility.UrlDecode(reso);
      QUERY_STRING=afterStr1(ref res,"?");
      res=beforStr1(ref res,"?");
      sub=beforStr1(ref Host,":");
      // ".." в запроах недопустимы в целях безопасности
      if(res.IndexOf("..")<0) ext=afterStr9(ref res,ext);
      
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
        case "gif":
          putCT(ref Content_T,"image/gif");
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
          if(ext==httpd.Ext || ext=="prg"){
            if(ext==httpd.Ext){R=2;}else{R=3;}
            putCT(ref Content_T,"text/html");
          }else{
            // Все другие расширения недопустимы в целях безопасности
            R=0;
          }
          break;
        }
      }else{
        R=1;
        if(!res.EndsWith("/")) res+="/";
        putCT(ref Content_T,"text/html");
        res=res+httpd.DirectoryIndex;
      }
      reso=sub+res;
      res=httpd.DocumentRoot+reso;
    }
    return res;
  }

  async Task type(System.Net.Sockets.NetworkStream Stream){
    // Отправка файла
    long NN = new System.IO.FileInfo(res).Length;
    string key = res+File.GetLastWriteTime(res).ToString("yyyyMMddHHmmssfff");

    Task tt = null;
    byte found;
    int i,j,k,m=0;
    if(NN > httpd.le){
      found=0;
    }else{
      if(AddCache!=null) await AddCache;
      if(httpd.Files.ContainsKey(key)){
        found = 1;
      }else{
        found = 7;
        AddCache = Task.Run(() =>{
          try{ httpd.Files.Add(key, new byte[NN]);
          }catch (ArgumentException){}
        });
      }
    }
    if(found == 1){
      head+=CL+": "+NN+"\r\n\r\n";
      i=Edos.GetBytes(head,0,head.Length,bytes,0);
      tt=Stream.WriteAsync(bytes,0,i);
      tt=Stream.WriteAsync(httpd.Files[key],0,httpd.Files[key].Length);
      tt=Stream.WriteAsync(bytes,i-2,2);
    }else{
      using (FileStream ts = File.OpenRead(res)){
        head+=CL+": "+ts.Length+"\r\n\r\n";
        k=Edos.GetBytes(head,0,head.Length,bytes,0);
        j=bytes.Length-k;
        while ((i = await ts.ReadAsync(bytes,k,j)) > 0){
          if(found > 0) {
            await AddCache;
            Array.Copy(bytes,k,httpd.Files[key],m,i);
            m+=i;
          }
          if(k>0){
            i+=k;
            j=bytes.Length;
            k=0;
          }
          if(ts.Length==ts.Position){
            // Добавляем в конец перенос строки
            if(i+2>=bytes.Length){
              if(tt!=null) await tt;
              tt=Stream.WriteAsync(bytes,0,i);
              i=0;
            }
            bytes[i]=10;
            i++;
            bytes[i]=13;
            i++;
          }
          tt=Stream.WriteAsync(bytes,0,i);
        }
        ts.Close();
      }
    }
    if(tt!=null) await tt;
  }

  async Task send_wsf(System.Net.Sockets.NetworkStream Stream){
    int N=0;
    byte[] bytes1;
    string cont, dirname="", filename="";
    var wsf = new ProcessStartInfo();

    wsf.EnvironmentVariables["SCRIPT_FILENAME"] = res;
    wsf.EnvironmentVariables["QUERY_STRING"] = QUERY_STRING;
    wsf.EnvironmentVariables["HTTP_COOKIE"] = Cookie;
    wsf.EnvironmentVariables["REMOTE_ADDR"] = IP;
    if(Content_Length>0){
      dirname=httpd.DirectorySessions+"/"+IP+"_"+Port;
      wsf.RedirectStandardInput = true;
      // Поставить разумное ограничение на размер потока
      if(Content_Type.LastIndexOf("form-")<0 || Content_Length>httpd.post){
        filename=valStr(ref Content_Disposition,"filename");
        if(filename.Length==0) filename=DateTime.Now.ToString("HHmmssfff");
        wsf.EnvironmentVariables["POST_FILENAME"] = filename = dirname+"/"+filename;
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
      if(k>0 && k<i){
        i = i-k;
      }else{
        k=0;
        i = await Stream.ReadAsync(bytes,k,bytes.Length);
      }
      if (filename.Length>0){
        // Открыть файл, если он не открыт
        if (File.Exists(filename)){
          File.Delete(filename);
        }else if(!Directory.Exists(dirname)){
          Directory.CreateDirectory(dirname);
        }
        file = new FileStream(filename,FileMode.Create);
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
      // т.е. разбор этого протокола ложится на сам скрипт

      while (N<Content_Length){
        if(filename.Length>0){
          if(i>0){
            ft = file.WriteAsync(bytes,k,i);
          }else{
            R2=1;
          }
        }else{
          // Всё записывать в поток
          if(i>0){
            cont1+=Edos.GetString(bytes,k,i);
          }else{
            R2=1;
          }
        }
        if(cont1.Length>0){
          // Это записать в поток, не в файл
          if (swt != null) await swt;
          swt = sw.WriteAsync(cont1);
          cont1="";
        }
        N+=i;
        if(N<Content_Length){
          if(R2==0){
            if (ft != null) await ft;
            i = await Stream.ReadAsync(bytes,0,bytes.Length);
          }else{
            N=Content_Length;
          }
        }
      }
      if (ft != null) await ft;
      if (file != null && file.CanRead) file.Close();
      if (swt != null) try{await swt;}catch(IOException){}
      sw.Close();
    }

    // Вывод полученных данных wsf-скрипта
    cont=Proc.StandardOutput.ReadToEnd();
    bytes1=Edos.GetBytes(head+cont);

    await Stream.WriteAsync(bytes1,0,bytes1.Length);

    Proc.WaitForExit();
    // Освободить ресурсы
    Proc.Dispose();
  }

  async Task send_prg(System.Net.Sockets.NetworkStream Stream){
    int j=-1, N=0;
    byte[] bytes1;
    string prg=afterStr9(ref res,"/"), dirprg=System.IO.Path.GetDirectoryName(
           System.Reflection.Assembly.GetEntryAssembly().Location),dirname="",
           filename="";

    if(httpd.vfpa!=null){
      for(j=0; j<httpd.db; j++){
        if(httpd.vfpb[j]==0){
          httpd.vfpb[j]=2;
          httpd.vfp[j] = Activator.CreateInstance(httpd.vfpa);
          break;
        }else if(httpd.vfpb[j]==1){
          httpd.vfpb[j]=2;
          break;
        }
      }
    }

    if(j<0){
      bytes1=Ewin.GetBytes(head+"\r\nMS VFP is missing in the Windows registry");
    }else if(j<httpd.db){
      if(Content_Length>0){
        dirname=httpd.DirectorySessions+"/"+IP+"_"+Port;
        // Ограничение на размер потока определяется возможностями VFP на размер строки
        if(Content_Type.LastIndexOf("form-")<0 || Content_Length>16777184){
          filename=valStr(ref Content_Disposition,"filename");
          if(filename.Length==0) filename=DateTime.Now.ToString("HHmmssfff");
          filename = dirname+"/"+filename;
        }
      }
      httpd.vfp[j].DoCmd("SET DEFA TO \""+dirprg+"\"");
      httpd.vfp[j].DoCmd("SET DEFA TO (FULLP(\""+beforStr9(ref res,"/")+"\"))");
      httpd.vfp[j].SetVar("SCRIPT_FILENAME",res);
      httpd.vfp[j].SetVar("QUERY_STRING",QUERY_STRING);
      httpd.vfp[j].SetVar("HTTP_COOKIE",Cookie);
      httpd.vfp[j].SetVar("REMOTE_ADDR",IP);
      httpd.vfp[j].SetVar("POST_FILENAME",filename);
      httpd.vfp[j].SetVar("ERROR_MESS","");
      httpd.vfp[j].DoCmd("on erro ERROR_MESS='ERROR: '+MESSAGE()+' IN: '+MESSAGE(1)");

      if(Content_Length>0){
        Task ft = null;
        FileStream file = null;
        // R2==0 означает, что не все данные со входа Stream прочитаны
        if(k>0 && k<i){
          i = i-k;
        }else{
          k=0;
          i = await Stream.ReadAsync(bytes,k,bytes.Length);
        }
        if (filename.Length>0){
          // Открыть файл, если он не открыт
          if (File.Exists(filename)){
            File.Delete(filename);
          }else if(!Directory.Exists(dirname)){
            Directory.CreateDirectory(dirname);
          }
          file = new FileStream(filename,FileMode.Create);
        }

        while (N<Content_Length){
          if(filename.Length>0){
            if(i>0){
              ft = file.WriteAsync(bytes,k,i);
            }else{
              R2=1;
            }
          }else{
            // Всё записывать в поток
            if(i>0){
              cont1+=Edos.GetString(bytes,k,i);
            }else{
              R2=1;
            }
          }
          N+=i;
          if(N<Content_Length){
            if(R2==0){
              if (ft != null) await ft;
              i = await Stream.ReadAsync(bytes,0,bytes.Length);
            }else{
              N=Content_Length;
            }
          }
        }
        if (ft != null) await ft;
        if (file != null && file.CanRead) file.Close();
      }
      httpd.vfp[j].SetVar("STD_INPUT",cont1);
      bytes1=Encoding.GetEncoding(httpd.vfp[j].Eval("CPCURRENT()")).
            GetBytes(head+httpd.vfp[j].Eval(beforStr9(ref prg,".prg")+"()"));

      // Подготовим VFP к новым заданиям
      httpd.vfp[j].DoCmd("on erro _box=_box");
      httpd.vfp[j].DoCmd("clea even");
      httpd.vfp[j].DoCmd("clea all");
      httpd.vfp[j].DoCmd("clos data all");
      httpd.vfp[j].DoCmd("clos all");
      httpd.vfp[j].DoCmd("on shut");
      httpd.vfpb[j]=1;
      cont1="";

    }else{
      bytes1=Ewin.GetBytes(head+"\r\nAll "+httpd.db.ToString()+" VFP processes are busy");
    }
    await Stream.WriteAsync(bytes1,0,bytes1.Length);
  }

}

class main{
  public static httpd httpd = null;

  static void Main(string[] Args){
    Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(
              System.Reflection.Assembly.GetEntryAssembly().Location));
    httpd = new httpd();
    if(getArgs(Args)){
      if(httpd.Args.Length==0){
        if(httpd.Proc.Substring(httpd.Proc.Length-11,11)=="cscript.exe" ||
           httpd.Proc.Substring(httpd.Proc.Length-7,7)=="cscript") httpd.Args="//Nologo ";
      }else{
        httpd.Args+=" ";
      }
      httpd.RunServer();
      AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
      Console.TreatControlCAsInput=true;
      Console.ReadKey(true);
      httpd.StopServer();
      Console.WriteLine("Server stoped :(((");
    }
  }

  static void CurrentDomain_ProcessExit(object sender, EventArgs e){
    // аварийное завершение или выключение ПК
    httpd.StopServer();
  }

  static bool getArgs(String[] Args){
    int i, k, b9=131072, db9=80, p9=65535, q9=2147483647, s9=15383, post9=33554432,
              less9=524288, log1=80;
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
          if(k<256){
            httpd.bu=256;
          }else{
            httpd.bu=(k <= b9)? k : b9;
          }
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
      case "-cp":
        i++;
        if(i < Args.Length){
          k=int.Parse(Args[i]);
          if(k > 0 && k <= 65535) httpd.cp=k;
        }            
        break;
      case "-db":
        i++;
        if(i < Args.Length){
          k=int.Parse(Args[i]);
          if(k >= 0 && k <= db9) httpd.db=k;
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
      case "-less":
        i++;
        if(i < Args.Length){
          k=int.Parse(Args[i]);
          httpd.le=(k > 0)? k : less9;
        }            
        break;
      case "-d":
        i++;
        if(i < Args.Length) httpd.DocumentRoot=
          (Args[i].EndsWith("/")||Args[i].EndsWith("\\"))?Args[i]:Args[i]+"/";
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
        Console.Write(@"Многопоточный http.net сервер версия 1.94, (C) kornienko.ru апрель 2024.

ИСПОЛЬЗОВАНИЕ:
    http.net [Параметр1 Значение1] [Параметр2 Значение2] ...

    При необходимости указываются пары Параметр и Значение. Если значение текст и содержит
    пробелы, то его необходимо заключать в кавычки.

Параметры:                                                          Значения по умолчанию:
     -d      Папка, содержащая домены.                                        "+httpd.DocumentRoot+@"
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
     -cp     Номер кодовой страницы, используемый для передачи текста.        "+httpd.cp.ToString()+@"
     -db     Максимальное количество динамически запускаемых экземпляров      "+httpd.db.ToString()+@"
             СУБД VFP. Расширение скриптов для запуска СУБД - prg. Процессы
             запускаются по мере одновременного обращения клиентов до
             заданного значения.
     -log    Размер журнала регистрации запросов в строках. Журнал состоит    "+httpd.log9.ToString()+@"
             из двух чередующихся версий http.net.x.log и http.net.y. Если
             задан размер менее "+log1.ToString()+@", то журнал не ведётся.
     -less   Максимальный размер небольших файлов, которые должны             "+httpd.le.ToString()+@"
             кешироваться. Все такие файлы при обращении к ним для повышения
             производительности будут сохраняться в оперативной памяти.
     -post   Максимальный размер принимаемого запроса для передачи            "+httpd.post.ToString()+@"
             файлу-скрипту. Если он будет превышен, то запрос помещается в
             файл, имя которого передается скрипту в переменной окружения
             POST_FILENAME. Другие формируемые переменные окружения -
             SCRIPT_FILENAME, QUERY_STRING, HTTP_COOKIE, REMOTE_ADDR. Если в
             данных запроса отсутствует директива form-..., то входящий
             поток данных целиком будет помещен в файл. Эта особенность
             может использоваться для передачи серверу файлов. При этом имя
             файла будет находиться в переменной окружения POST_FILENAME.
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
