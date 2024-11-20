/////////////////////////////////////////////////////////
//   http.net сервер на C#.     Автор: A.Б.Корниенко   //
/////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Web;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

public class httpd{
  public const string CL="Content-Length",CT="Content-Type", CD="Content-Disposition",
                      CC="Cache-Control: public, max-age=2300000\r\n",DI="index.html",
                      H1="HTTP/1.1 ",UTF8="UTF-8",CLR="sys(2004)+'VFPclear.prg'",
                      Protocol="http",OK=H1+"200 OK\r\n",CT_T=CT+": text/plain\r\n",
                      ver="version 2.6.8",verD="November 2024";
  public const  int i9=2147483647;
  public static int port=8080, st=20, qu=600, bu=32768, db=20, log9=10000, post=33554432,
                    logi=0, i, k, maxVFP;
  public static string DocumentRoot="../www/", Folder, DirectoryIndex=DI,
                       Proc="cscript.exe", Args="", Ext="wsf",
                       logX="http.net.x.log", logY="http.net.y.log", logZ="",
                       DirectorySessions="Sessions";
  public static Type vfpa = Type.GetTypeFromProgID("VisualFoxPro.Application");
  public static StreamWriter logSW = null;
  public static FileStream logFS = null;
  public static TextWriter TW = null;
  public static TextWriter TE = null;
  public static dynamic[] vfp = null;
  public static Encoding vfpw = null;
  public static bool notexit=true, VFPclr=false;
  public static byte[] vfpb = null;
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
      log2("\tThe http.net server "+ver+" are waiting for input requests...");
    }catch(Exception){
      log2("\t\tThere were problems when creating threads. Try updating Windows.");
      notexit=false;
    }
  }

  public void StopServer(){
    notexit=false;
    if(vfpa != null) for(i=0; i<db; i++) if(vfpb[i]>0)
                     try{ vfp[i].Quit(); }catch(System.Runtime.InteropServices.COMException){ }
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
      logFS.Seek(0,SeekOrigin.Begin);
    }else{
      Interlocked.Increment(ref logi);
    }

    if(!(logFS!=null)){
      // Отправка вывода на консоль в файл:
      logZ=(File.GetLastWriteTime(logX)<=File.GetLastWriteTime(logY))? logX : logY;
      logFS = new FileStream(logZ,FileMode.Create,FileAccess.Write,FileShare.ReadWrite);
      TW = Console.Out;
      TE = Console.Error;
      logSW = new StreamWriter(logFS);
      Console.SetOut(logSW);
      Console.SetError(logSW);
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
  private string cont1, h1, reso, res, head, heads, Host, Content_Type,
                 Content_Disposition, QUERY_STRING, IP, jt, Port, x1, x2;
  private byte l, R, R1, R2;
  private byte[] bytes;
  private Encoding UTF;

  public Session(Socket Server){
    httpd.i++;
    jt = httpd.i.ToString();
    bytes = new Byte[httpd.bu];
    Accept(Server);
  }

  public async void Accept(Socket Server){
    DateTime dt1;
    string Content_T;
    NetworkStream Stream = null;
    Socket Client = null;
    while (httpd.notexit){
// httpd.log2("Stack Count: "+(new StackTrace()).FrameCount.ToString());
      Client = await Server.AcceptAsync();
      IPEndPoint Point = Client.RemoteEndPoint as IPEndPoint;
      IP=Point.Address.ToString();
      dt1=DateTime.UtcNow;
      x1=" "+IP+" "+jt+"\t";
      res="";
      try{ Stream = new NetworkStream(Client,true); }catch(Exception){ }
      if(Stream != null){
        Content_T=httpd.CT_T;
        cont1=heads=head=h1=reso=Host=Content_Type=Content_Disposition=QUERY_STRING="";
        UTF = Encoding.GetEncoding(httpd.UTF8);
        Port=Point.Port.ToString();
        k=Content_Length=0;
        i=httpd.bu;
        R=R1=R2=0;
        l=1;
        while (i>0 && l>0){
          if(k>0 && i>k){
            cont1=UTF.GetString(bytes,k,i-k);
            k=0;
          }
          try{
            i = await Stream.ReadAsync(bytes, 0, bytes.Length);
          }catch(Exception){
            i = -1;
          }
          if(i>0){
            l = getHeaders(UTF, ref bytes, ref cont1, ref k, ref reso, ref Host,
                ref Content_Type, ref Content_Disposition, ref Content_Length, ref heads);
            if(Host.Length>0){
              res=prepResource(ref reso, ref QUERY_STRING, ref Host, ref R, ref R1,
                  ref h1, ref Content_T);
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
              x2=httpd.valStr(ref Content_Type,"charset");
              if(x2.Length>0 && !String.Equals(x2,httpd.UTF8,StringComparison.CurrentCultureIgnoreCase)){
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
            if(!gzExists(ref res, ref head)){
              if(!File.Exists(res)){
                res=httpd.DocumentRoot+httpd.DI;
                if(!gzExists(ref res, ref head)){
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
      if(res.Length==0) res="400 Bad Request";
      httpd.log2("/"+(DateTime.UtcNow-dt1).ToString("fff")+x1+res);
    }
  }

  static void putCT(ref string c, string x){
    c=httpd.CT+": "+x+"\r\n";
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

  static string line1(Encoding UTF, ref byte[] bytes, ref string cont1, ref int k,
                      ref byte b){
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

  static byte getHeaders(Encoding UTF, ref byte[] bytes, ref string cont1, ref int k,
                ref string reso, ref string Host, ref string Content_Type,
                ref string Content_Disposition, ref int Content_Length,
                ref string heads){
    int i;
    byte b=0;
    string lin=line1(UTF, ref bytes, ref cont1, ref k, ref b),n,h;

    while (lin.Length>0){
// Console.WriteLine(lin);
// httpd.log2(lin);
      h=httpd.afterStr1(ref lin,":");
      h=httpd.ltri(ref h);
      if(h.Length>0){
        n=httpd.beforStr1(ref lin,":");
        switch(n){
        case "Host":
          Host=h;
          break;
        case httpd.CT:
          Content_Type=h;
          break;
        case httpd.CD:
          Content_Disposition=h;
          break;
        case httpd.CL:
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
            h=httpd.ltri(ref h);
            i=h.IndexOf(" ");
            if(i>0) reso=h.Substring(0,i);
          }
        }
      }
      lin=line1(UTF, ref bytes, ref cont1, ref k, ref b);
    }
    return b;
  }

  static string prepResource(ref string reso, ref string QUERY_STRING, ref string Host,
                             ref byte R, ref byte R1, ref string h1, ref string Content_T){
    string sub,res="",ext=".";
    if(reso.Length==0){
      R=0;
    }else{
      res=HttpUtility.UrlDecode(reso);
      QUERY_STRING=httpd.afterStr1(ref res,"?");
      res=httpd.beforStr1(ref res,"?");
      sub=httpd.beforStr1(ref Host,":");
      // ".." в запроах недопустимы в целях безопасности
      if(res.IndexOf("..")<0){
        if(res.EndsWith("/")) res+=httpd.DirectoryIndex;
        reso=httpd.afterStr9(ref res,"/");
        ext=httpd.afterStr9(ref reso,ext);
        if(ext.Length==0){
          reso=httpd.DocumentRoot+sub+res+".";
          if(File.Exists(reso+httpd.Ext)){
            R1=1;      // Случай API
            ext=httpd.Ext;
            res+="."+ext;
          }else if(File.Exists(reso+"prg")){
            R1=1;      // Случай API
            ext="prg";
          }else if(Directory.Exists(reso)){
            res+="/"+httpd.DirectoryIndex;
            ext=httpd.afterStr9(ref httpd.DirectoryIndex,".");
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
        h1=httpd.CC;
        break;
      case "svg":
        putCT(ref Content_T,"image/svg+xml");
        h1=httpd.CC;
        break;
      case "gif":
        putCT(ref Content_T,"image/gif");
        h1=httpd.CC;
        break;
      case "png":
        putCT(ref Content_T,"image/png");
        h1=httpd.CC;
        break;
      case "jpeg":
      case "jpg":
        putCT(ref Content_T,"image/jpeg");
        h1=httpd.CC;
        break;
      case "js":
        putCT(ref Content_T,"text/javascript");
        h1=httpd.CC;
        break;
      case "css":
        putCT(ref Content_T,"text/css");
        h1=httpd.CC;
        break;
      case "ico":
        putCT(ref Content_T,"image/x-icon");
        h1=httpd.CC;
        break;
      case "mp4":
        putCT(ref Content_T,"video/mp4");
        h1=httpd.CC;
        break;
      case "":
        Content_T=httpd.CT_T;
        break;
      default:
        Content_T="";
        if(ext==httpd.Ext){
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
      res=httpd.DocumentRoot+reso;
    }
    return res;
  }

  async Task failure(System.Net.Sockets.NetworkStream Stream, string z){
    byte[] bytes1=UTF.GetBytes(httpd.H1+z+"\r\n");
    await Stream.WriteAsync(bytes1,0,bytes1.Length);
  }

  async Task type(System.Net.Sockets.NetworkStream Stream){
    // Отправка файла
    int i,j,k;
    head=httpd.OK+head+httpd.CL+": ";
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

  async Task send_wsf(System.Net.Sockets.NetworkStream Stream){
    int N=0;
    byte[] bytes1;
    string dirname="", filename="";
    var wsf = new ProcessStartInfo();

    if(Content_Length>0){
      wsf.RedirectStandardInput = true;
      // Поставить разумное ограничение на размер потока
      filename=httpd.valStr(ref Content_Disposition,"filename");
      if(filename.Length>0 || Content_Length>httpd.post){
        dirname=httpd.DirectorySessions+"/"+IP+"_"+Port;
        if(filename.Length==0) filename=DateTime.Now.ToString("HHmmssfff");
        filename = dirname+"/"+HttpUtility.UrlDecode(filename);
      }
    }

    wsf.EnvironmentVariables["SCRIPT_FILENAME"] = httpd.fullres(ref res);
    wsf.EnvironmentVariables["SERVER_PROTOCOL"] = httpd.Protocol;
    wsf.EnvironmentVariables["QUERY_STRING"] = QUERY_STRING;
    wsf.EnvironmentVariables["POST_FILENAME"] = filename;
    wsf.EnvironmentVariables["HTTP_HEADERS"] = heads;
    wsf.EnvironmentVariables["REMOTE_ADDR"] = IP;
    wsf.RedirectStandardOutput = true;
    wsf.UseShellExecute = false;
    wsf.CreateNoWindow = true;
    wsf.FileName = httpd.Proc;
    wsf.Arguments = httpd.Args+" \""+res+"\"";
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
          i = await Stream.ReadAsync(bytes,k,bytes.Length);
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
              i = await Stream.ReadAsync(bytes,0,bytes.Length);
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
      if (swt != null) try{ await swt; }catch(Exception){}
      sw.Close();
    }

    // Вывод полученных данных wsf-скрипта
    if(R1==0){
      bytes1=UTF.GetBytes(httpd.OK+head+Proc.StandardOutput.ReadToEnd());
    }else{
      cont1=Proc.StandardOutput.ReadToEnd();
      R1=(byte)cont1[0];
      if (R1>53||R1<49){
        head=httpd.OK+head;
        i=0;
      }else{
        i=cont1.IndexOf("\n")+1;
        head=httpd.H1+cont1.Substring(0,i)+head;
      }
      bytes1=UTF.GetBytes(head+cont1.Substring(i));
    }
    await Stream.WriteAsync(bytes1,0,bytes1.Length);

    Proc.WaitForExit();
    // Освободить ресурсы
    Proc.Dispose();
  }

  async Task send_prg(System.Net.Sockets.NetworkStream Stream){
    int j=-1, N=0;
    byte[] bytes1;
    string fullprg=httpd.fullres(ref res),prg=httpd.afterStr9(ref res,"/"),
           dirname="", filename="";

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
      bytes1=UTF.GetBytes(httpd.OK+head+httpd.CT_T+
             "\r\nMS VFP is missing in the Windows registry");
    }else if(j<httpd.db){
      if(Content_Length>0){
        // Ограничение на размер потока определяется возможностями VFP на размер строки
        filename=httpd.valStr(ref Content_Disposition,"filename");
        if(filename.Length>0 || Content_Length>httpd.maxVFP){
          if(filename.Length==0) filename=DateTime.Now.ToString("HHmmssfff");
          dirname=httpd.DirectorySessions+"/"+IP+"_"+Port;
          filename = dirname+"/"+HttpUtility.UrlDecode(filename);
        }
      }

      try{
        httpd.vfp[j].SetVar("ERROR_MESS","");
      }catch(System.Runtime.InteropServices.COMException){
        httpd.vfp[j] = Activator.CreateInstance(httpd.vfpa);
      }
      httpd.vfp[j].DoCmd("on erro ERROR_MESS='ERROR: '+MESSAGE()+' IN: '+MESSAGE(1)");
      httpd.vfp[j].DoCmd("SET DEFA TO (\""+httpd.beforStr9(ref fullprg,"/")+"\")");
      httpd.vfp[j].SetVar("POST_FILENAME",filename.Length>0?httpd.Folder+"/"+filename:"");
      httpd.vfp[j].SetVar("SERVER_PROTOCOL",httpd.Protocol);
      httpd.vfp[j].SetVar("QUERY_STRING",QUERY_STRING);
      httpd.vfp[j].SetVar("SCRIPT_FILENAME",fullprg);
      httpd.vfp[j].SetVar("HTTP_HEADERS",heads);
      httpd.vfp[j].SetVar("REMOTE_ADDR",IP);

      if(Content_Length>0){
        Task ft = null;
        FileStream file = null;
        // R2==0 означает, что не все данные со входа Stream прочитаны
        if(k>0 && k<i){
          i = i-k;
        }else{
          k=0;
          try{
            i = await Stream.ReadAsync(bytes,k,bytes.Length);
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
                i = await Stream.ReadAsync(bytes,0,bytes.Length);
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
      httpd.vfp[j].SetVar("STD_INPUT",cont1);
      try{
        if(R1==0){
          bytes1=httpd.vfpw.GetBytes(httpd.OK+head+httpd.vfp[j].
                   Eval(httpd.beforStr9(ref prg,".prg")+"()"));
        }else{      // Случай API
          cont1=httpd.vfp[j].Eval(prg+"()");
          R1=(byte)cont1[0];
          if (R1>53||R1<49){
            head=httpd.OK+head;
            i=0;
          }else{
            i=cont1.IndexOf("\n")+1;
            head=httpd.H1+cont1.Substring(0,i)+head;
          }
          bytes1=httpd.vfpw.GetBytes(head+cont1.Substring(i));
        }
      }catch(Exception e){
        bytes1=UTF.GetBytes(httpd.OK+head+httpd.CT_T+"\r\nError in VFP: "+e.Message);
      }
      // Подготовим VFP к новым заданиям
      try{
        if(httpd.VFPclr){
          httpd.vfp[j].DoCmd("do ("+httpd.CLR+")");
        }else{
          httpd.vfp[j].DoCmd("clea even");
          httpd.vfp[j].DoCmd("clea prog");
          httpd.vfp[j].DoCmd("clea all");
          httpd.vfp[j].DoCmd("clos data all");
          httpd.vfp[j].DoCmd("clos all");
        }
        httpd.vfpb[j]=1;
      }catch(Exception){
        httpd.vfpb[j]=0;
      }

    }else{
      bytes1=UTF.GetBytes(httpd.OK+head+httpd.CT_T+"\r\nAll "+httpd.db.ToString()+
             " VFP processes are busy");
    }
    await Stream.WriteAsync(bytes1,0,bytes1.Length);
  }

}

class main{
  public static httpd httpd = null;

  static void Main(string[] Args){
    string Folder=System.IO.Path.GetDirectoryName(
           System.Reflection.Assembly.GetEntryAssembly().Location);
    Directory.SetCurrentDirectory(Folder);
    httpd = new httpd();
    httpd.Folder=Folder;
    if(getArgs(Args)){
      if(httpd.Args.Length==0){
        if(httpd.Proc.Substring(httpd.Proc.Length-11,11)=="cscript.exe" ||
           httpd.Proc.Substring(httpd.Proc.Length-7,7)=="cscript") httpd.Args="//Nologo";
      }else{
        httpd.Args+=" ";
      }
      httpd.RunServer();
      if(httpd.notexit){
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
        Console.TreatControlCAsInput=true;
        Console.ReadKey(true);
        httpd.StopServer();
      }
      Console.WriteLine("Server stoped :(((");
    }
  }

  static void CurrentDomain_ProcessExit(object sender, EventArgs e){
    // аварийное завершение или выключение ПК
    httpd.StopServer();
  }

  static bool getArgs(String[] Args){
    int i, k, b9=131072, db9=1000, p9=65535, q9=2147483647, s9=1000, post9=33554432, log1=80;
    bool l=true;
    // Разбор параметров
    for (i = 0; i < Args.Length; i++){
      switch (Args[i]){
      case "-p":
        i++;
        if(i < Args.Length){
          k=httpd.valInt(Args[i]);
          if(k > 0 && k <= p9) httpd.port=k;
        }
        break;
      case "-b":
        i++;
        if(i < Args.Length){
          k=httpd.valInt(Args[i]);
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
          k=httpd.valInt(Args[i]);
          httpd.qu=(k > 0 && k <= q9)? k : q9;
        }            
        break;
      case "-s":
        i++;
        if(i < Args.Length){
          k=httpd.valInt(Args[i]);
          httpd.st=(k > 0 && k <= s9)? k : s9;
        }            
        break;
      case "-db":
        i++;
        if(i < Args.Length){
          k=httpd.valInt(Args[i]);
          if(k >= 0 && k <= db9) httpd.db=k;
        }            
        break;
      case "-log":
        i++;
        if(i < Args.Length){
          k=httpd.valInt(Args[i]);
          httpd.log9=(k < log1)? 0 : k;
        }            
        break;
      case "-post":
        i++;
        if(i < Args.Length){
          k=httpd.valInt(Args[i]);
          httpd.post=(k > 0)? k : post9;
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
        Console.WriteLine("Multithreaded http.net server "+
             httpd.ver+", (C) a.kornienko.ru "+httpd.verD+@".

USAGE:
    http.net [Parameter1 Value1] [Parameter2 Value2] ...

    If necessary, Parameter and Value pairs are specified. If the value is text and contains
    spaces, then it must be enclosed in quotation marks.

Parameters:                                                                  Default values:
     -d      Folder containing the domains.                                      "+httpd.DocumentRoot+@"
     -i      Main document is in the folders. The main document in the           "+httpd.DirectoryIndex+@"
             folder specified by the -d parameter is used to display the page
             with the 404 code - file was not found. To compress traffic,
             files compressed using gzip method of the name.expansion.gz type
             are supported, for example - index.html.gz or library.js.gz etc.
     -p      Port that the server is listening on.                               "+httpd.port.ToString()+@"
     -b      Size of the read and write buffers.                                 "+httpd.bu.ToString()+@"
     -s      Number of requests being processed at the same time. Maximum        "+httpd.st.ToString()+@"
             value is 1000.
     -q      Number requests stored in the queue.                                "+httpd.qu.ToString()+@"
     -db     Maximum number of dynamically running MS VFP DBMS instances.        "+httpd.db.ToString()+@"
             Extending scripts to run VFP - prg. Processes are started as
             needed by simultaneous client requests to the set value. Maximum
             value is 1000.
     -log    Size of the query log in rows. The log consists of two              "+httpd.log9.ToString()+@"
             interleaved versions http.net.x.log and http.net.y.log. If the
             size is set to less than "+log1.ToString()+@", then the log is not kept.
     -post   Maximum size of the accepted request to transfer to the script      "+httpd.post.ToString()+@"
             file. If it is exceeded, the request is placed in a file,
             the name of which is passed to the script in the environment
             variable POST_FILENAME. Other generated environment variables -
             SERVER_PROTOCOL, SCRIPT_FILENAME, QUERY_STRING, HTTP_HEADERS,
             REMOTE_ADDR. If the form-... directive is missing from the
             request data, then incoming data stream will be placed entirely
             in a file. This feature can be used to transfer files to the
             server. In this case, the file name will be in the environment
             variable POST_FILENAME.
     -proc   Script handler used. If necessary, you must also include            "+httpd.Proc+@"
             the full path to the executable file. By default, the component
             built into Microsoft Windows OS is used, a very fast script
             server handler (WSH) using the JScript and VBScript languages.
     -args   Additional parameters of the handler startup command line. When
             using cscript.exe if no additional parameters are specified,
             the //Nologo parameter is used.
     -ext    Extension of the script files.                                      "+httpd.Ext);
        l=false;
        break;
      }
      if(!l) break;
    }
    return l;
  }
}
