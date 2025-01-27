//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//!!                                                     !!
//!!   http.net сервер на C#.      Автор: A.Б.Корниенко  !!
//!!   Серверный движок                                  !!
//!!                                                     !!
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace http1 {

  class Server {
    Socket listenSocket;            // the socket used to listen for incoming connection requests
    Task[] t;                       // Запуск сессий
    int i;

    public bool Start(IPEndPoint localEndPoint) {
      t = new Task[f.st];

      // create the socket which listens for incoming connections
      listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
      try { listenSocket.Bind(localEndPoint); } catch (Exception) { return false; }

      // start the server with a listen backlog of f.qu connections
      listenSocket.Listen(f.qu);

      //Console.WriteLine("Press any key to terminate the server process....");
      //Console.ReadKey();

      return true;
    }

    // Остановить сервер
    public void Stop() {

       // Закрыть все сессии
       for (i=0; i<f.st; i++) {
         if (t[i] != null) f.session[i].Stop();
       }

       // Закрыть прослушивание
       try { listenSocket.Shutdown(SocketShutdown.Both); } catch (Exception) { }
       listenSocket.Close();
    }

    public void StartAccept() {
       while (f.notExit) {
          f.maxNumberAcceptedClients.WaitOne();
          if (f.notExit) {
             i = f.freeClientsPool.Pop();
             t[i] = f.session[i].AcceptAsync(listenSocket.AcceptAsync());
          }
       }
    }
  }
}
