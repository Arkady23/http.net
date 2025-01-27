//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//!!                                                     !!
//!!   http.net сервер на C#.      Автор: A.Б.Корниенко  !!
//!!   Серверный движок            версия от 27.01.2025  !!
//!!                                                     !!
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

// Проблемы с движком от Microsoft:
// 1. Невозможность использовать BufferManager для использования SSL потока.
//    Возможно эту проблему можно решить используя нестандартные неописанные в 
//    документации от Microsoft способы.
// 2. Значительно усложняется логика сервера и значительно увеличивается объем
//    текста.
//
// В результате принято решение отказаться от событийного движка в пользу
// чистого потокового. Однако прием использованных Microsoft объекта типа
// Stack<int> (в тексте ниже freeClientsPool) и объекта типа Semaphore (в тексте
// maxNumberAcceptedClients) сохранен для реализации поддержки фиксированного
// количества сессий в бесконечном цикле.

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
       for (int j=0; j<f.st; j++) {
         if (t[j] != null) f.session[j].Stop();
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
