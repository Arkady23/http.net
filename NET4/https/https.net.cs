/////////////////////////////////////////////////////////
//   https.net сервер на C#.    Автор: A.Б.Корниенко   //
/////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Web;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net.Security;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

public class https{
  public const string CL="Content-Length",CT="Content-Type", CD="Content-Disposition",
                      CC="Cache-Control: public, max-age=2300000\r\n",DI="index.html",
                      H1="HTTP/1.1 ",UTF8="UTF-8",CLR="sys(2004)+'VFPclear.prg'",
                      Protocol="https",OK=H1+"200 OK\r\n",CT_T=CT+": text/plain\r\n",
                      logX="https.net.x.log", logY="https.net.y.log",
                      ver="version 0.5.0",verD="December 2024";
  public const  int i9=2147483647;
  public static int port=8443, st=20, qu=600, bu=32768, db=20, log9=10000, post=33554432,
                    tw=10000, logi=0, i, k, maxVFP;
  public static string DocumentRoot="../www/", Folder, DirectoryIndex=DI,
                       Proc="cscript.exe", Ext="wsf", Args="", logZ="",
                       DirectorySessions="Sessions", CerFile="a.kornienko.ru.pfx";
  public static Type vfpa = Type.GetTypeFromProgID("VisualFoxPro.Application");
  public static X509Certificate2 Cert = null;
  public static StreamWriter logSW = null;
  public static FileStream logFS = null;
  public static TextWriter TW = null;
  public static TextWriter TE = null;
  public static dynamic[] vfp = null;
  public static Encoding vfpw = null;
  public static byte[] vfpb = null;
  public static bool notexit=true, VFPclr=false;
  Socket Server = null;
  Session[] Session = null;
  
  public void RunServer(){
    if(Directory.Exists(DirectorySessions)) Directory.Delete(DirectorySessions,true);
    if(!File.Exists(CerFile)){
      CerFile=DocumentRoot+CerFile;
      if(!File.Exists(CerFile)) CerFile="";
    }
    if(CerFile==""){
      Console.WriteLine("The https.net server cannot be started. Certificate was not found :(");
    }else{
      try{
        Cert = new X509Certificate2(CerFile);
      }catch(Exception){
        Console.WriteLine("The https.net server cannot be started. Certificate error :((");
        Cert = null;
      }
      if(Cert!=null){
        Server = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        Session = new Session[st];
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
        vfp = new dynamic[db];
        vfpb = new byte[db];

        // Проверим на случай, если уже запущен экземпляр сервера:
        try{
          Server.Bind(ep);
        }catch(Exception){
          // Запущен втоой экземпляр сервра на одном порту
          notexit=false;
        }

        if(notexit==true){
          Server.Listen(qu);
          if(vfpa!=null){
            try{
              vfp[0] = Activator.CreateInstance(vfpa);
            }catch(Exception){
              vfpa = null;
            }
            if(vfpa!=null){
              vfpb[0]=1;
              maxVFP=(vfp[0].Eval("sys(17)")=="Pentium")? 16777184 : 67108832;
              vfpw=Encoding.GetEncoding(vfp[0].Eval("CPCURRENT()"));
              VFPclr=vfp[0].Eval("file("+CLR+")");
            }
          }
          i= st<8?8:st;
          ThreadPool.SetMinThreads(i,i);
          i=0;    // Задание начального индекса для создания переменной jt в Session
          try{
            Parallel.For(0,st,j => { Session[j] = new Session(Server); });
            log2("\tThe https.net server "+ver+" are waiting for input requests...");
          }catch(Exception){
            log2("\t\tThere were problems when creating threads. Try updating Windows.");
            notexit=false;
          }
        }
      }
    }
  }

  public void StopServer(){
    notexit=false;
    if(vfpa != null) for(i=0; i<db; i++) if(vfpb[i]>0)
                        try{vfp[i].Quit();}catch(System.Runtime.InteropServices.COMException){ }
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

    // Нужно ли начать запись с начала журнала?
    if(logi>=log9 && logFS!=null){
      Interlocked.Exchange(ref logi,1);
      logZ = (logY==logZ)? logX:logY;
      logSW.Close();
      logFS.Close();
      log1();
    }else{
      Interlocked.Increment(ref logi);
    }

    if(!(logFS!=null)){
      // Сохранить старые назначения:
      TW = Console.Out;
      TE = Console.Error;

      // Отправка стандартного вывода на консоль в чередующиеся кешируемые файлы:
      logZ=(File.GetLastWriteTime(logX)<=File.GetLastWriteTime(logY))? logX : logY;
      log1();
    }

    // Записать в файл
    try{
      Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff")+x);
      logSW.Flush();
      logFS.Flush();
    }catch(ObjectDisposedException){
      log9=0;
    }catch(Exception){
      Thread.Sleep(23); log2(x+" *");
    }
  }

  internal static void log1(){
    logFS = new FileStream(logZ,FileMode.Create,FileAccess.Write,FileShare.ReadWrite);
    logSW = new StreamWriter(logFS);
    Console.SetError(logSW);
    Console.SetOut(logSW);
  }

  public static void log2(string x){
    if(log9>0){
      Thread log2 = new Thread(log);
      log2.Priority = ThreadPriority.BelowNormal;
      log2.Start(x);
    }
  }

  public static string ltri(ref string x){
    return x.TrimStart('\t',' ');
  }

  public static string fullres(ref string x){
    return Path.GetFullPath(x).Replace("\\","/");
  }

  public static string beforStr1(ref string x, string Str){
    int k=0;
    if(Str.Length>0) k=x.IndexOf(Str);
    return k<0?x:(k>0?x.Substring(0,k):"");
  }

  public static string afterStr1(ref string x, string Str){
    if(Str.Length>0){
      int k=x.IndexOf(Str,StringComparison.OrdinalIgnoreCase);
      return k<0?"":x.Substring(k+Str.Length);
    }else{
      return x;
    }
  }

  public static string beforStr9(ref string x, string Str){
    if(Str.Length>0){
       int k=x.LastIndexOf(Str);
       return k<0?x:(k>0?x.Substring(0,k):"");
    }else{
       return x;
    }
  }

  public static string afterStr9(ref string x, string Str){
    int k= -1;
    if(Str.Length>0) k=x.LastIndexOf(Str);
    return k<0?"":x.Substring(k+Str.Length);
  }

  // Узнать значение поля в заголовке (может понадобиться при разборе заголовков)
  public static string valStr(ref string x, string Str){
    string z="";
    if(x.Length>0){
      z=afterStr1(ref x," "+Str+"=");
      if(z.Length==0) z=afterStr1(ref x,";"+Str+"=");
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

  public static int valInt(string x){
    int z;
    try{ z=int.Parse(x); }catch(Exception){ z=i9; }
    return z;
  }

}

class Session{
  private int i,k,Content_Length;
  private string cont1, h1, reso, res, head, heads, Host, Content_Type, Content_T,
                 Content_Disposition, QUERY_STRING, IP, jt, Port, x1, x2;
  private byte l, R, R1, R2;
  private byte[] bytes;
  private Encoding UTF;

  public Session(Socket Server){
    https.i++;
    jt = https.i.ToString();
    bytes = new Byte[https.bu];
    Accept(Server);
  }

  async void Accept(Socket Server){
    Task<int> ti;
    DateTime dt1;
    double n;
    SslStream Stream = null;
    Socket Client = null;
    while (https.notexit){
      Client = await Server.AcceptAsync();
      IPEndPoint Point = Client.RemoteEndPoint as IPEndPoint;
      IP=Point.Address.ToString();
      dt1=DateTime.UtcNow;
      x1=" "+IP+" "+jt+"\t";
      res="";
      try{
        Stream = new SslStream(new NetworkStream(Client,true),false);
        if (! Stream.AuthenticateAsServerAsync(https.Cert,false,
            System.Security.Authentication.SslProtocols.Tls12,false).Wait(200)){
          Stream.Close();
          Stream = null;
        }
      }catch(Exception){
        if(Stream != null){
          Stream.Close();
          Stream = null;
        }
      }
      if(Stream != null){
        Content_T=https.CT_T;
        dt1=DateTime.UtcNow;
        cont1=heads=head=h1=reso=Host=Content_Type=Content_Disposition=QUERY_STRING="";
        UTF = Encoding.GetEncoding(https.UTF8);
        Port=Point.Port.ToString();
        k=Content_Length=0;
        i=https.bu;
        R=R1=R2=0;
        l=1;
        while (i>0 && l>0){
          if(k>0 && i>k){
            cont1=UTF.GetString(bytes,k,i-k);
            k=0;
          }
          try{
            ti = Stream.ReadAsync(bytes, 0, bytes.Length);
            i = ti.Wait(https.tw)? await ti : -1;
          }catch(Exception){
            i = -1;
          }
          if(i>0){
            l = getHeaders();
            if(Host.Length>0){
              prepResource();
              if(R==0) l=R;    // Дальше читать бессмысленно
            }
          }else{
            R2=1;
          }
        }
        if(R>0){
          head="Date: "+dt1.ToString("R")+"\r\n"+h1+Content_T;
          if(R>1){
            if(R1>0 || File.Exists(res)){
              x2=https.valStr(ref Content_Type,"charset");
              if(x2.Length>0 && !String.Equals(x1,https.UTF8,StringComparison.CurrentCultureIgnoreCase)){
                try{ UTF = Encoding.GetEncoding(x2); }catch(Exception){ }
              }
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
            if(!gzExists()){
              if(!File.Exists(res)){
                res=https.DocumentRoot+https.DI;
                if(!gzExists()){
                  if(!File.Exists(res)){
                    R=0;
                    await failure(Stream,"404 Not Found");
                  }
                }
              }
            }
            if(R==1){
              await type(Stream);
            }
          }
        }else{
          if(res.Length>0){
            res+=" -";
            await failure(Stream,"403 Forbidden");
          }
        }
        Stream.Close();
      }else{
        res="500 Internal Server Error";
      }
      Client = null;
      Stream = null;
      if(res.Length==0) res="-";
      n=DateTime.UtcNow.Subtract(dt1).TotalMilliseconds;
      https.log2("/"+(n>9999?"****":n.ToString("0000"))+x1+res);
    }
  }

  void putCT(ref string c, string x){
    c=https.CT+": "+x+"\r\n";
  }

  bool gzExists(){
    string gz=res+".gz";
    bool L=File.Exists(gz);
    if(L){
      res=gz;
      head+="Content-Encoding: gzip\r\n";
    }
    return L;
  }

  string line1(ref byte b){
    int i;
    string z=cont1;
    cont1="";
    i=Array.IndexOf(bytes,(byte)10,k);
    if(i<0){
      b=1;
    }else{
      if(i>0 && bytes[i-1]==13){
        z+=UTF.GetString(bytes,k,i-k-1);
      }else{
        z+=UTF.GetString(bytes,k,i-k);
      }
      k=i+1;
      b=0;
    }
    return z;
  }

  byte getHeaders(){
    int i;
    byte b=0;
    string lin=line1(ref b),n,h;

    while (lin.Length>0){
// Console.WriteLine(lin);
// https.log2(lin);
      h=https.afterStr1(ref lin,":");
      h=https.ltri(ref h);
      if(h.Length>0){
        n=https.beforStr1(ref lin,":");
        switch(n){
        case "Host":
          Host=h;
          break;
        case https.CT:
          Content_Type=h;
          break;
        case https.CD:
          Content_Disposition=h;
          break;
        case https.CL:
          try{ Content_Length=int.Parse(h); }catch(Exception){ Content_Length=0; }
          break;
        }
        heads+=lin+"\r\n";
      }else{
        i=lin.IndexOf(" ");
        if(i>0){
          n=lin.Substring(0,i);
          if(n=="GET" || n=="POST" || n=="PUT"){
            h=lin.Substring(i+1);
            h=https.ltri(ref h);
            i=h.IndexOf(" ");
            if(i>0) reso=h.Substring(0,i);
          }
        }
      }
      lin=line1(ref b);
    }
    return b;
  }

  void prepResource(){
    string sub,ext=".";
    if(reso.Length==0){
      R=0;
    }else{
      res=HttpUtility.UrlDecode(reso);
      QUERY_STRING=https.afterStr1(ref res,"?");
      res=https.beforStr1(ref res,"?");
      sub=https.beforStr1(ref Host,":");
      // ".." в запроах недопустимы в целях безопасности
      if(res.IndexOf("..")<0){
        if(res.EndsWith("/")) res+=https.DirectoryIndex;
        reso=https.afterStr9(ref res,"/");
        ext=https.afterStr9(ref reso,ext);
        if(ext.Length==0){
          reso=https.DocumentRoot+sub+res+".";
          if(File.Exists(reso+https.Ext)){
            R1=1;      // Случай API
            ext=https.Ext;
            res+="."+ext;
          }else if(File.Exists(reso+"prg")){
            R1=1;      // Случай API
            ext="prg";
          }else if(Directory.Exists(reso)){
            res+="/"+https.DirectoryIndex;
            ext=https.afterStr9(ref https.DirectoryIndex,".");
          }else if(! File.Exists(reso)){
            ext="html";
            res+="."+ext;
          }
        }
      }
      R=1;
      switch(ext){
      case "html":
        putCT(ref Content_T,"text/html");
        h1=https.CC;
        break;
      case "svg":
        putCT(ref Content_T,"image/svg+xml");
        h1=https.CC;
        break;
      case "gif":
        putCT(ref Content_T,"image/gif");
        h1=https.CC;
        break;
      case "png":
        putCT(ref Content_T,"image/png");
        h1=https.CC;
        break;
      case "jpeg":
      case "jpg":
        putCT(ref Content_T,"image/jpeg");
        h1=https.CC;
        break;
      case "js":
        putCT(ref Content_T,"text/javascript");
        h1=https.CC;
        break;
      case "css":
        putCT(ref Content_T,"text/css");
        h1=https.CC;
        break;
      case "ico":
        putCT(ref Content_T,"image/x-icon");
        h1=https.CC;
        break;
      case "mp4":
        putCT(ref Content_T,"video/mp4");
        h1=https.CC;
        break;
      case "":
        Content_T=https.CT_T;
        break;
      default:
        Content_T="";
        if(ext==https.Ext){
          R=2;
        }else if(ext=="prg"){
          R=3;
        }else{
          // Все другие расширения недопустимы в целях безопасности
          R=0;
        }
        break;
      }
      reso=sub+res;
      res=https.DocumentRoot+reso;
    }
  }

  async Task failure(SslStream Stream, string z){
    byte[] bytes1=UTF.GetBytes(https.H1+z+"\r\n");
    await Stream.WriteAsync(bytes1,0,bytes1.Length);
  }

  async Task type(SslStream Stream){
    // Отправка файла
    int i,j,k;
    head=https.OK+head+https.CL+": ";
    using (FileStream ts = File.OpenRead(res)){
      head+=ts.Length+"\r\n\r\n";
      k=UTF.GetBytes(head,0,head.Length,bytes,0);
      j=bytes.Length-k;
      while ((i = await ts.ReadAsync(bytes,k,j)) > 0){
        if(k>0){
          i+=k;
          j=bytes.Length;
          k=0;
        }
        await Stream.WriteAsync(bytes,0,i);
      }
      ts.Close();
    }
  }

  async Task send_wsf(SslStream Stream){
    int N=0;
    Task<int> ti;
    byte[] bytes1;
    string dirname="", filename="";
    var wsf = new ProcessStartInfo();

    if(Content_Length>0){
      wsf.RedirectStandardInput = true;
      // Поставить разумное ограничение на размер потока
      filename=https.valStr(ref Content_Disposition,"filename");
      if(filename.Length>0 || Content_Length>https.post){
        dirname=https.DirectorySessions+"/"+IP+"_"+Port;
        if(filename.Length==0) filename=DateTime.Now.ToString("HHmmssfff");
        filename = dirname+"/"+HttpUtility.UrlDecode(filename);
      }
    }

    wsf.EnvironmentVariables["SCRIPT_FILENAME"] = https.fullres(ref res);
    wsf.EnvironmentVariables["SERVER_PROTOCOL"] = https.Protocol;
    wsf.EnvironmentVariables["QUERY_STRING"] = QUERY_STRING;
    wsf.EnvironmentVariables["POST_FILENAME"] = filename;
    wsf.EnvironmentVariables["HTTP_HEADERS"] = heads;
    wsf.EnvironmentVariables["REMOTE_ADDR"] = IP;
    wsf.RedirectStandardOutput = true;
    wsf.UseShellExecute = false;
    wsf.CreateNoWindow = true;
    wsf.FileName = https.Proc;
    wsf.Arguments = https.Args+" \""+res+"\"";
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
        try{
          ti = Stream.ReadAsync(bytes,k,bytes.Length);
          if(ti.Wait(https.tw)){
            i= await ti;
          }else{
            N=Content_Length;
          }
        }catch(Exception){
          N=Content_Length;
        }
      }
      if(filename.Length>0){
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
            cont1+=UTF.GetString(bytes,k,i);
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
            try{
              ti = Stream.ReadAsync(bytes,0,bytes.Length);
              if(ti.Wait(https.tw)){
                i= await ti;
              }else{
                N=Content_Length;
              }
            }catch(Exception){
              N=Content_Length;
            }
          }else{
            N=Content_Length;
          }
        }
      }
      if (ft != null) await ft;
      if (file != null && file.CanRead) file.Close();
      if (swt != null) try{ await swt; }catch(Exception){ }
      sw.Close();
    }

    // Вывод полученных данных wsf-скрипта
    if(R1==0){
      bytes1=UTF.GetBytes(https.OK+head+Proc.StandardOutput.ReadToEnd());
    }else{
      cont1=Proc.StandardOutput.ReadToEnd();
      R1=(byte)cont1[0];
      if (R1>53||R1<49){
        head=https.OK+head;
        i=0;
      }else{
        i=cont1.IndexOf("\n")+1;
        head=https.H1+cont1.Substring(0,i)+head;
      }
      bytes1=UTF.GetBytes(head+cont1.Substring(i));
    }
    await Stream.WriteAsync(bytes1,0,bytes1.Length);

    Proc.WaitForExit();
    // Освободить ресурсы
    Proc.Dispose();
  }

  async Task send_prg(SslStream Stream){
    Task<int> ti;
    int j=-1, N=0;
    byte[] bytes1;
    string fullprg=https.fullres(ref res),prg=https.afterStr9(ref res,"/"),
           dirname="", filename="";

    if(https.vfpa!=null){
      for(j=0; j<https.db; j++){
        if(https.vfpb[j]==0){
          https.vfpb[j]=2;
          https.vfp[j] = Activator.CreateInstance(https.vfpa);
          break;
        }else if(https.vfpb[j]==1){
          https.vfpb[j]=2;
          break;
        }
      }
    }

    if(j<0){
      bytes1=UTF.GetBytes(https.OK+head+https.CT_T+
             "\r\nMS VFP is missing in the Windows registry");
    }else if(j<https.db){
      if(Content_Length>0){
        // Ограничение на размер потока определяется возможностями VFP на размер строки
        filename=https.valStr(ref Content_Disposition,"filename");
        if(filename.Length>0 || Content_Length>https.maxVFP){
          if(filename.Length==0) filename=DateTime.Now.ToString("HHmmssfff");
          dirname=https.DirectorySessions+"/"+IP+"_"+Port;
          filename = dirname+"/"+HttpUtility.UrlDecode(filename);
        }
      }

      try{
        https.vfp[j].SetVar("ERROR_MESS","");
      }catch(System.Runtime.InteropServices.COMException){
        https.vfp[j] = Activator.CreateInstance(https.vfpa);
      }
      https.vfp[j].DoCmd("on erro ERROR_MESS='ERROR: '+MESSAGE()+' IN: '+MESSAGE(1)");
      https.vfp[j].DoCmd("SET DEFA TO (\""+https.beforStr9(ref fullprg,"/")+"\")");
      https.vfp[j].SetVar("POST_FILENAME",filename.Length>0?https.Folder+filename:"");
      https.vfp[j].SetVar("SERVER_PROTOCOL",https.Protocol);
      https.vfp[j].SetVar("QUERY_STRING",QUERY_STRING);
      https.vfp[j].SetVar("SCRIPT_FILENAME",fullprg);
      https.vfp[j].SetVar("HTTP_HEADERS",heads);
      https.vfp[j].SetVar("REMOTE_ADDR",IP);

      if(Content_Length>0){
        Task ft = null;
        FileStream file = null;
        // R2==0 означает, что не все данные со входа Stream прочитаны
        if(k>0 && k<i){
          i = i-k;
        }else{
          k=0;
          try{
            ti = Stream.ReadAsync(bytes,k,bytes.Length);
            if(ti.Wait(https.tw)){
              i= await ti;
            }else{
              N=Content_Length;
            }
          }catch(Exception){
            N=Content_Length;
          }
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
              cont1+=UTF.GetString(bytes,k,i);
            }else{
              R2=1;
            }
          }
          N+=i;
          if(N<Content_Length){
            if(R2==0){
              if (ft != null) await ft;
              try{
                ti = Stream.ReadAsync(bytes,0,bytes.Length);
                if(ti.Wait(https.tw)){
                  i= await ti;
                }else{
                  N=Content_Length;
                }
              }catch(Exception){
                N=Content_Length;
              }
            }else{
              N=Content_Length;
            }
          }
        }
        if (ft != null) await ft;
        if (file != null && file.CanRead) file.Close();
      }
      https.vfp[j].SetVar("STD_INPUT",cont1);
      try{
        if(R1==0){
          bytes1=https.vfpw.GetBytes(https.OK+head+https.vfp[j].
                            Eval(https.beforStr9(ref prg,".prg")+"()"));
        }else{      // Случай API
          cont1=https.vfp[j].Eval(prg+"()");
          R1=(byte)cont1[0];
          if (R1>53||R1<49){
            head=https.OK+head;
            i=0;
          }else{
            i=cont1.IndexOf("\n")+1;
            head=https.H1+cont1.Substring(0,i)+head;
          }
          bytes1=https.vfpw.GetBytes(head+cont1.Substring(i));
        }
      }catch(Exception e){
        bytes1=UTF.GetBytes(https.OK+head+https.CT_T+"\r\nError in VFP: "+e.Message);
      }
      // Подготовим VFP к новым заданиям
      try{
        if(https.VFPclr){
          https.vfp[j].DoCmd("do ("+https.CLR+")");
        }else{
          https.vfp[j].DoCmd("clea even");
          https.vfp[j].DoCmd("clea prog");
          https.vfp[j].DoCmd("clea all");
          https.vfp[j].DoCmd("clos data all");
          https.vfp[j].DoCmd("clos all");
        }
        https.vfpb[j]=1;
      }catch(Exception){
        https.vfpb[j]=0;
      }

    }else{
      bytes1=UTF.GetBytes(https.OK+head+https.CT_T+"\r\nAll "+https.db.ToString()+
             " VFP processes are busy");
    }
    await Stream.WriteAsync(bytes1,0,bytes1.Length);
  }

}

class main{
  public static https https = null;

  static void Main(string[] Args){
    string Folder=Thread.GetDomain().BaseDirectory;
    Directory.SetCurrentDirectory(Folder);
    https = new https();
    https.Folder=Folder;
    if(getArgs(Args)){
      if(https.Args.Length==0){
        if(https.Proc.Substring(https.Proc.Length-11,11)=="cscript.exe" ||
           https.Proc.Substring(https.Proc.Length-7,7)=="cscript") https.Args="//Nologo";
      }else{
        https.Args+=" ";
      }
      https.RunServer();
      if(https.Cert!=null){
        if(https.notexit){

          // Перехват ошибок и запись их в журнал
          AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
          {
            https.log2(" "+((Exception)eventArgs.ExceptionObject).ToString());
            https.StopServer();
          };

          // Считывание клавиши Esc для выхода
          ConsoleKeyInfo cki;
          Console.WriteLine("Press Esc to exit.");
          do { cki = Console.ReadKey(true); } while (cki.Key != ConsoleKey.Escape);
          https.StopServer();
          Console.WriteLine("Server stoped :(((");
        }
      }
    }
  }

  static bool getArgs(String[] Args){
    int b9=131072, db9=1000, p9=65535, q9=2147483647, s9=1000, t9=20, post9=33554432, log1=80,
        i, k;
    bool l=true;
    if(Args.Length>0){
      if((byte)Args[0][0]==64){
        string fn=Args[0].Substring(1).Trim();
        if(File.Exists(fn))
           Args = File.ReadAllText(fn).Replace("\t", " ").Replace("\r",string.Empty).
                  Replace("\n",string.Empty).Trim().Split();
      }
    }

    // Разбор параметров
    for (i = 0; i < Args.Length; i++){
      switch (Args[i]){
      case "-p":
        i++;
        if(i < Args.Length){
          k=https.valInt(Args[i]);
          if(k > 0 && k <= p9) https.port=k;
        }
        break;
      case "-b":
        i++;
        if(i < Args.Length){
          k=https.valInt(Args[i]);
          if(k<256){
            https.bu=256;
          }else{
            https.bu=(k <= b9)? k : b9;
          }
        }            
        break;
      case "-q":
        i++;
        if(i < Args.Length){
          k=https.valInt(Args[i]);
          https.qu=(k > 0 && k <= q9)? k : q9;
        }            
        break;
      case "-s":
        i++;
        if(i < Args.Length){
          k=https.valInt(Args[i]);
          https.st=(k > 0 && k <= s9)? k : s9;
        }            
        break;
        case "-w":
          i++;
          if(i < Args.Length){
            k=https.valInt(Args[i]);
            https.tw=((k > 0 && k <= t9)? k : t9)*1000;
          }            
          break;
      case "-db":
        i++;
        if(i < Args.Length){
          k=https.valInt(Args[i]);
          if(k >= 0 && k <= db9) https.db=k;
        }            
        break;
      case "-log":
        i++;
        if(i < Args.Length){
          k=https.valInt(Args[i]);
          https.log9=(k < log1)? 0 : k;
        }            
        break;
      case "-post":
        i++;
        if(i < Args.Length){
          k=https.valInt(Args[i]);
          https.post=(k > 0)? k : post9;
        }            
        break;
      case "-d":
        i++;
        if(i < Args.Length) https.DocumentRoot=
          (Args[i].EndsWith("/")||Args[i].EndsWith("\\"))?Args[i]:Args[i]+"/";
        break;
      case "-i":
        i++;
        if(i < Args.Length) https.DirectoryIndex=Args[i];
        break;
      case "-c":
        i++;
        if(i < Args.Length) https.CerFile=Args[i];
        break;
      case "-proc":
        i++;
        if(i < Args.Length) https.Proc=Args[i];
        break;
      case "-args":
        i++;
        if(i < Args.Length) https.Args=Args[i];
        break;
      case "-ext":
        i++;
        if(i < Args.Length) https.Ext=Args[i];
        break;
      default:
        Console.WriteLine("Multithreaded https.net server "+
             https.ver+", (C) a.kornienko.ru "+https.verD+@".

USAGE:
    https.net [Parameter1 Value1] [Parameter2 Value2] ...
    https.net @filename

    If necessary, Parameter and Value pairs are specified. If the value is text and contains
    spaces, then it must be enclosed in quotation marks. You can specify @filename which
    contains the entire line with parameters.

Parameters:                                                                  Default values:
     -d      Folder containing the domains.                                      "+https.DocumentRoot+@"
     -i      Main document is in the folders. The main document in the           "+https.DirectoryIndex+@"
             folder specified by the -d parameter is used to display the page
             with the 404 code - file was not found. To compress traffic,
             files compressed using gzip method of the name.expansion.gz type
             are supported, for example - index.html.gz or library.js.gz etc.
     -c      Name of the file containing the self-signed certificate for the     "+https.CerFile+@"
             Tls 1.2 protocol without a password. If path is not specified,
             the certificate is searched in the https.net server location
             folder and in the root folder containing the domains.
     -p      Port that the server is listening on.                               "+https.port.ToString()+@"
     -b      Size of the read and write buffers.                                 "+https.bu.ToString()+@"
     -s      Number of requests being processed at the same time. Maximum        "+https.st.ToString()+@"
             value is 1000.
     -q      Number requests stored in the queue.                                "+https.qu.ToString()+@"
     -w      Allowed time to reserve an open channel for request that did not    "+(https.tw/1000).ToString()+@"
             started. From 1 to 20 seconds.
     -db     Maximum number of dynamically running MS VFP DBMS instances.        "+https.db.ToString()+@"
             Extending scripts to run VFP - prg. Processes are started as
             needed by simultaneous client requests to the set value. Maximum
             value is 1000.
     -log    Size of the query log in rows. The log consists of two              "+https.log9.ToString()+@"
             interleaved versions https.net.x.log and https.net.y.log. If the
             size is set to less than "+log1.ToString()+@", then the log is not kept.
     -post   Maximum size of the accepted request to transfer to the script      "+https.post.ToString()+@"
             file. If it is exceeded, the request is placed in a file,
             the name of which is passed to the script in the environment
             variable POST_FILENAME. Other generated environment variables -
             SERVER_PROTOCOL, SCRIPT_FILENAME, QUERY_STRING, HTTP_HEADERS,
             REMOTE_ADDR. If the form-... directive is missing from the
             request data, then incoming data stream will be placed entirely
             in a file. This feature can be used to transfer files to the
             server. In this case, the file name will be in the environment
             variable POST_FILENAME.
     -proc   Script handler used. If necessary, you must also include            "+https.Proc+@"
             the full path to the executable file. By default, the component
             built into Microsoft Windows OS is used, a very fast script
             server handler (WSH) using the JScript and VBScript languages.
     -args   Additional parameters of the handler startup command line. When
             using cscript.exe if no additional parameters are specified,
             the //Nologo parameter is used.
     -ext    Extension of the script files.                                      "+https.Ext);
        l=false;
        break;
      }
      if(!l) break;
    }
    return l;
  }
}
