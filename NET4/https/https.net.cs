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
using System.Security.Cryptography.X509Certificates;

public class https{
  public const string CL="Content-Length",CT="Content-Type", CD="Content-Disposition",
                      CC="Cache-Control: public, max-age=2300000\r\n",DI="index.html",
                      H1="HTTP/1.1 ",CLR="sys(2004)+'VFPclear.prg'";
  public const string OK=H1+"200 OK\r\n",CT_T=CT+": text/plain\r\n";
  public const  int q9=2147483647;
  public static int port=8443, st=888, qu=888, bu=16384, db=22, log9=10000, post=33554432,
                    le=524288, cp=Encoding.GetEncoding(0).CodePage, logi=0, i, k, maxVFP;
  public static string DocumentRoot="../www/", Folder, DirectoryIndex=DI,
                       Proc="cscript.exe", Args="", Ext="wsf",
                       logX="https.net.x.log", logY="https.net.y.log", logZ="",
                       DirectorySessions="Sessions", CerFile="a.kornienko.ru.pfx";
  public static Dictionary<string,byte[]> Files = new Dictionary<string,byte[]>();
  public static Type vfpa = Type.GetTypeFromProgID("VisualFoxPro.Application");
  public static Encoding UTF8 = Encoding.GetEncoding("UTF-8");
  public static X509Certificate2 Cert = null;
  public static StreamWriter logSW = null;
  public static FileStream logFS = null;
  public static Encoding Ewin = null;
  public static TextWriter TW = null;
  public static TextWriter TE = null;
  public static dynamic[] vfp = null;
  public static byte[] vfpb = null;
  public static bool notexit=true, VFPclr=false;
  public static long DTi;
  Socket Server = null;
  Session[] Session = null;
  
  public void RunServer(){
    if(Directory.Exists(DirectorySessions)) Directory.Delete(DirectorySessions,true);
    if(!File.Exists(CerFile)){
      CerFile=DocumentRoot+CerFile;
      if(!File.Exists(CerFile)) CerFile="";
    }
    if(CerFile==""){
      Console.WriteLine("The https-server cannot be started. Certificate was not found :(");
    }else{
      try{
        Cert = new X509Certificate2(CerFile);
      }catch(Exception){
        Console.WriteLine("The https-server cannot be started. Certificate error :((");
        Cert = null;
      }
      if(Cert!=null){
        Server = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        Session = new Session[st];
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
        Ewin = Encoding.GetEncoding(cp);
        vfp = new dynamic[db];
        vfpb = new byte[db];
        Server.Bind(ep);
        Server.Listen(qu);
        if(log9>0) log("\tThe https-server is running...");
        if(vfpa!=null){
          try{
            vfp[0] = Activator.CreateInstance(vfpa);
          }catch(Exception){
            vfpa = null;
          }
          if(vfpa!=null){
            vfpb[0]=1;
            maxVFP=(vfp[0].Eval("sys(17)")=="Pentium")? 16777184 : 67108832;
            VFPclr=vfp[0].Eval("file("+CLR+")");
          }
        }
        ThreadPool.GetMinThreads(out i, out k);
        if(st>i) ThreadPool.SetMinThreads(st,st);
        i=0;    // Задание начального индекса для создания переменной jt в Session
        try{
          Parallel.For(0,st,j => { Session[j] = new Session(Server); });
          if(log9>0) log("\t"+st.ToString()+" tasks are waiting for input requests...");
        }catch(Exception){
          if(log9>0) log("\t"+"There were problems when creating threads. Try updating Windows.");
          notexit=false;
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
      logFS = new FileStream(logZ,FileMode.Create,FileAccess.Write,FileShare.ReadWrite);
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
    }catch(ObjectDisposedException){
      log9=0;
    }catch(IOException){
      Thread.Sleep(23); log2(x+"");
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
      int k=x.IndexOf(Str);
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
    try{ z=int.Parse(x); }catch(Exception){ z=q9; }
    return z;
  }

  public static int find10(ref byte[] bytes, int i){
    if(i<bytes.Length && bytes[i]!=10) i=find10(ref bytes,i+1);
    return i;
  }

}

class Session{
  private int i,k,Content_Length;
  private string cont1, h1, reso, res, head, heads, Host, Content_Disposition,
                 QUERY_STRING, IP, jt, Port, x1;
  private byte[] bytes;
  private byte l, R, R1, R2;

  public Session(Socket Server){
    https.i++;
    jt = https.i.ToString();
    bytes = new Byte[https.bu];
    Accept(Server);
  }

  public async void Accept(Socket Server){
    while (https.notexit) await AcceptProc(await Server.AcceptAsync(), Server);
  }

  public async Task AcceptProc(Socket Client, Socket Server){
    SslStream Stream = null;
    try{
      Stream = new SslStream(new NetworkStream(Client,true),false);
      Stream.AuthenticateAsServer(https.Cert,false,
          System.Security.Authentication.SslProtocols.Tls12,false);
      R=R1=R2=0;
    }catch(Exception){
      R=6;    // Ошибки авторизации хакеров проигнорировать
    }
    if(R==0){
      string dt1=DateTime.UtcNow.ToString("R"), Content_T=https.CT_T;
      cont1=heads=head=h1=reso=Host=Content_Disposition=QUERY_STRING="";
      IPEndPoint Point = Client.RemoteEndPoint as IPEndPoint;
      IP=Point.Address.ToString();
      Port=Point.Port.ToString();
      k=Content_Length=0;
      x1=IP+" "+jt+"\t";
      i=https.bu;
      l=1;
      while (i>0 && l>0){
        if(k>0 && i>k){
          cont1=https.UTF8.GetString(bytes,k,i-k);
          k=0;
        }
        try{
          i = await Stream.ReadAsync(bytes, 0, bytes.Length);
        }catch(Exception){
          i = -1;
        }
        if(i>0){
          l = getHeaders(ref bytes, ref cont1, ref k, ref reso, ref Host,
                ref Content_Disposition, ref Content_Length, ref heads);
        }else{
          R2=1;
        }
      }

      if(i>=0) res=prepResource(ref reso, ref QUERY_STRING, ref Host, ref R, ref R1,
                                ref h1, ref Content_T);
      if(R>0){
        https.log2(x1+res);
        head="Date: "+dt1+"\r\n"+h1+Content_T;
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
              res=https.DocumentRoot+https.DI;
              if(!gzExists(ref res, ref head)) R=0;
            }
          }
          if(R==1) await type(Stream);
        }
      }
    }
    if(Stream != null) Stream.Close();
  }

  static void putCT(ref string c, string x){
    c=https.CT+": "+x+"\r\n";
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

  static string zinc(ref string z, ref byte[] bytes, ref int k, int i){
    return z+https.UTF8.GetString(bytes,k,i);
  }

  static string line1(ref byte[] bytes, ref string cont1, ref int k, ref byte b){
    int i;
    string z=cont1;
    cont1="";
    i=https.find10(ref bytes,k);
    if(i<bytes.Length){
      if(i>0 && bytes[i-1]==13){
        z=zinc(ref z, ref bytes, ref k, i-k-1);
      }else{
        z=zinc(ref z, ref bytes, ref k, i-k);
      }
      k=i+1;
      b=0;
    }else{
      b=1;
    }
    return z;
  }

  static byte getHeaders(ref byte[] bytes, ref string cont1, ref int k, ref string reso,
                ref string Host, ref string Content_Disposition, ref int Content_Length,
                ref string heads){
    byte b=0;
    string lin=line1(ref bytes, ref cont1, ref k, ref b),n,h;

    while (lin.Length>0){
// Console.WriteLine("lin=|"+lin+"|");
// https.log(lin);
      h=https.afterStr1(ref lin,":");
      h=https.ltri(ref h);
      if(h.Length>0){
        n=https.beforStr1(ref lin,":");
        switch(n){
        case "Host":
          Host=h;
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
        h=https.afterStr1(ref lin," ");
        h=https.beforStr9(ref h," ");
        reso=https.ltri(ref h);
      }
      lin=line1(ref bytes, ref cont1, ref k, ref b);
    }
    return b;
  }

  static string prepResource(ref string reso, ref string QUERY_STRING, ref string Host,
                             ref byte R, ref byte R1, ref string h1, ref string Content_T){
    string sub,res="",ext=".";
    if(reso.Length>0 && Host.Length>0){
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
          reso=https.DocumentRoot+sub+res;
          if(Directory.Exists(reso)){
            res+="/"+https.DirectoryIndex;
            ext=https.afterStr9(ref https.DirectoryIndex,".");
          }else{
            R1=1;
            reso+=".";
            if(File.Exists(reso+https.Ext)){
              ext=https.Ext;
            }else if(File.Exists(reso+"prg")){
              ext="prg";
            }else if(File.Exists(reso)){
              ext="";
            }else{
              ext="html";
            }
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
    return res;
  }

  async Task type(SslStream Stream){
    // Отправка файла
    long NN = new System.IO.FileInfo(res).Length;
    string key = res+File.GetLastWriteTime(res).ToString("yyyyMMddHHmmssfff");

    byte found;
    int i,j,k,m=0;
    head=https.OK+head+https.CL+": ";
    if(NN > https.le){
      found=0;
    }else{
      if(https.Files.ContainsKey(key)){
        found = 1;
      }else{
        found = 7;
        try{ https.Files.Add(key, new byte[NN]); }catch (ArgumentException){ }
      }
    }
    if(found == 1){
      head+=NN+"\r\n\r\n";
      i=https.UTF8.GetBytes(head,0,head.Length,bytes,0);
      await Stream.WriteAsync(bytes,0,i);
      await Stream.WriteAsync(https.Files[key],0,https.Files[key].Length);
      await Stream.WriteAsync(bytes,i-2,2);
    }else{
      using (FileStream ts = File.OpenRead(res)){
        head+=ts.Length+"\r\n\r\n";
        k=https.UTF8.GetBytes(head,0,head.Length,bytes,0);
        j=bytes.Length-k;
        while ((i = await ts.ReadAsync(bytes,k,j)) > 0){
          if(found > 0) {
            Array.Copy(bytes,k,https.Files[key],m,i);
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
              await Stream.WriteAsync(bytes,0,i);
              i=0;
            }
            bytes[i]=13;
            i++;
            bytes[i]=10;
            i++;
          }
          await Stream.WriteAsync(bytes,0,i);
        }
        ts.Close();
      }
    }
  }

  async Task send_wsf(SslStream Stream){
    int N=0;
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
          i = await Stream.ReadAsync(bytes,k,bytes.Length);
        }catch(IOException){
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
            cont1+=https.UTF8.GetString(bytes,k,i);
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
            }catch(IOException){
              N=Content_Length;
            }
          }else{
            N=Content_Length;
          }
        }
      }
      if (ft != null) await ft;
      if (file != null && file.CanRead) file.Close();
      if (swt != null) try{ await swt; }catch(IOException){ }
      sw.Close();
    }

    // Вывод полученных данных wsf-скрипта
    if(R1==0){
      bytes1=https.Ewin.GetBytes(https.OK+head+Proc.StandardOutput.ReadToEnd());
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
      bytes1=https.Ewin.GetBytes(head+cont1.Substring(i));
    }

    try{
      await Stream.WriteAsync(bytes1,0,bytes1.Length);
    }catch(IOException){ }

    Proc.WaitForExit();
    // Освободить ресурсы
    Proc.Dispose();
  }

  async Task send_prg(SslStream Stream){
    int j=-1, N=0;
    byte[] bytes1;
    string fullprg=https.fullres(ref res),prg=https.afterStr9(ref res,"/"),
           dirprg=System.IO.Path.GetDirectoryName(
           System.Reflection.Assembly.GetEntryAssembly().Location),dirname="",
           filename="";

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
      bytes1=https.Ewin.GetBytes(https.OK+head+https.CT_T+
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
      https.vfp[j].SetVar("POST_FILENAME",filename.Length>0?https.Folder+"/"+filename:"");
      https.vfp[j].SetVar("SCRIPT_FILENAME",fullprg);
      https.vfp[j].SetVar("QUERY_STRING",QUERY_STRING);
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
            i = await Stream.ReadAsync(bytes,k,bytes.Length);
          }catch(IOException){
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
              cont1+=https.UTF8.GetString(bytes,k,i);
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
              }catch(IOException){
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
          bytes1=Encoding.GetEncoding(https.vfp[j].Eval("CPCURRENT()")).
              GetBytes(https.OK+head+https.vfp[j].Eval(https.beforStr9(ref prg,".prg")+"()"));
        }else{
          cont1=https.vfp[j].Eval(https.beforStr9(ref prg,".prg")+"()");
          R1=(byte)cont1[0];
          if (R1>53||R1<49){
            head=https.OK+head;
            i=0;
          }else{
            i=cont1.IndexOf("\n")+1;
            head=https.H1+cont1.Substring(0,i)+head;
          }
          bytes1=Encoding.GetEncoding(https.vfp[j].Eval("CPCURRENT()")).
              GetBytes(head+cont1.Substring(i));
        }
      }catch(Exception e){
        bytes1=https.UTF8.GetBytes(https.OK+head+https.CT_T+"\r\nError in VFP: "+e.Message);
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
      bytes1=https.Ewin.GetBytes(https.OK+head+https.CT_T+"\r\nAll "+https.db.ToString()+
             " VFP processes are busy");
    }
    try{
      await Stream.WriteAsync(bytes1,0,bytes1.Length);
    }catch(IOException){ }
  }

}

class main{
  public static https https = null;

  static void Main(string[] Args){
    string Folder=System.IO.Path.GetDirectoryName(
           System.Reflection.Assembly.GetEntryAssembly().Location);
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
          AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
          Console.TreatControlCAsInput=true;
          Console.ReadKey(true);
          https.StopServer();
        }
        Console.WriteLine("Server stoped :(((");
      }
    }
  }

  static void CurrentDomain_ProcessExit(object sender, EventArgs e){
    // аварийное завершение или выключение ПК
    https.StopServer();
  }

  static bool getArgs(String[] Args){
    int i, k, b9=131072, db9=80, p9=65535, s9=15383, post9=33554432, less9=524288, log1=80;
    bool l=true;
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
          https.qu=(k > 0 && k <= https.q9)? k : https.q9;
        }            
        break;
      case "-s":
        i++;
        if(i < Args.Length){
          k=https.valInt(Args[i]);
          https.st=(k > 0 && k <= s9)? k : s9;
        }            
        break;
      case "-cp":
        i++;
        if(i < Args.Length){
          k=https.valInt(Args[i]);
          if(k > 0 && k <= 65535) https.cp=k;
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
      case "-less":
        i++;
        if(i < Args.Length){
          k=https.valInt(Args[i]);
          https.le=(k > 0)? k : less9;
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
        Console.WriteLine(@"Multithreaded https.net server version 0.3.0, (C) kornienko.ru October 2024.

USAGE:
    https.net [Parameter1 Value1] [Parameter2 Value2] ...

    If necessary, Parameter and Value pairs are specified. If the value is text and contains
    spaces, then it must be enclosed in quotation marks.

Parameters:                                                                  Default values:
     -d      Folder containing the domains.                                      "+https.DocumentRoot+@"
     -i      Main document is in the folders. The main document in the           "+https.DirectoryIndex+@"
             folder specified by the -d parameter is used to display the page
             with the 404 code - file was not found. To compress traffic,
             files compressed using gzip method of the name.expansion.gz type
             are supported, for example - index.html.gz or library.js.gz etc.
     -c      Name of the file containing the self-signed certificate for the     "+https.CerFile+@"
             Tls 1.2 protocol without a password. If path is not specified,
             the certificate is searched in the https-server location folder
             and in the root folder containing the domains.
     -p      Port that the server is listening on.                               "+https.port.ToString()+@"
     -b      Size of the read and write buffers.                                 "+https.bu.ToString()+@"
     -s      Number of requests being processed at the same time.                "+https.st.ToString()+@"
             The maximum number is limited by processor performance, RAM size
             and Windows settings.
     -q      Number of additional requests stored in the queue if the number     "+https.qu.ToString()+@"
             of simultaneous requests specified by the -s parameter is
             exceeded. If the amount of requests processed and pending in the
             queue is exceeded, a denial of service is sent to the client.
     -cp     Code page number used for text transfer.                            "+https.cp.ToString()+@"
     -db     Maximum number of dynamically running MS VFP DBMS instances.        "+https.db.ToString()+@"
             Extending scripts to run VFP - prg. Pprocesses are started as
             needed by simultaneous client requests to the set value.
     -log    Size of the query log in rows. The log consists of two              "+https.log9.ToString()+@"
             interleaved versions http.net.x.log and http.net.y.log. If the
             size is set to less than "+log1.ToString()+@", then the log is not kept.
     -less   Maximum size of small files that should be cached. All such         "+https.le.ToString()+@"
             files will be stored in RAM to improve performance.
     -post   Maximum size of the accepted request to transfer to the script      "+https.post.ToString()+@"
             file. If it is exceeded, the request is placed in a file,
             the name of which is passed to the script in the environment
             variable POST_FILENAME. Other generated environment variables -
             SCRIPT_FILENAME, QUERY_STRING, HTTP_HEADERS, REMOTE_ADDR. If
             the form-... directive is missing from the request data, then
             incoming data stream will be placed entirely in a file. This
             feature can be used to transfer files to the server. In this
             case, the file name will be in the environment variable
             POST_FILENAME.
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
