using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace HQXChatServer
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        private byte[] _buffer = new byte[1024];//verinin boyutu
        public List<SocketT2h> __ClientSockets { get; set; }
        List<string> _names = new List<string>();
        private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private void SetupServer()
        {
            Label1.Text = "sunucu başlatıldı . . .";
            //serverSocket.Bind(new IPEndPoint(IPAddress.Parse("176.53.69.151"), 100));//IPAddress.Any=127.0.0.1,100 portunu dinliyor
            _serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 100));//127.0.0.1,100 portunu dinliyor
            _serverSocket.Listen(1);//dinliyor

            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null); //Async=asenkron yani eş zamansız Callback=Geri arama
            Console.WriteLine("dinliyor");
            /*
             Asenkron Callback modelinde,
             işlemlerin sonuçlanmasının hemen ardından devreye 
             girecek olan metod Callback metodu olarak adlandırılır.
             Bu metodu çalışma zamanında işaret edebilmek için özel
             AsyncCallback temsilci (delegate) tipinden faydalanılır.
              */
        }
        private void AppceptCallback(IAsyncResult ar)//kabul edilen soket buraya gelıyor
        {
            Console.WriteLine("tekrardan buradayım");
            Socket socket = _serverSocket.EndAccept(ar);//
            __ClientSockets.Add(new SocketT2h(socket));//SocketT2h sinifi en aşşağıda -> List<SocketT2h> __ClientSockets = new List<SocketT2h>();
            ListBox1.Items.Add(socket.RemoteEndPoint.ToString());
            Console.WriteLine("bağlanan soket = " + socket.RemoteEndPoint.ToString());
            //  label2.Text = "clienttt: " + __ClientSockets.Count.ToString();
            Label1.Text = "Client bağlı. . .";
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);//AsyncCallback demek thread gibi bişey eş zamansız yani bu kodu çalıştırırken diğerleride çalışıyor
            Console.WriteLine("ReceiveCallback metodu çalıştı");
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);//AppceptCallback ı tekrardan çağırıyor 
            Console.WriteLine("AppceptCallback metodu recursıve oldu");
        }
        static string sonlanan_clien = "";

        private void ReceiveCallback(IAsyncResult ar)
        {

            Socket socket = (Socket)ar.AsyncState;//geri dönüş yapılacak soket

            if (socket.Connected)
            {
                int received;//alınan 
                try
                {
                    received = socket.EndReceive(ar);//mesajı aldık eğer burada bı hata olursa client kapatılmıştır
                }
                catch (Exception)//hatayı buraya taşıdık
                {
                    // 
                    for (int i = 0; i < __ClientSockets.Count; i++)//bağlı soketler kadar dolaş
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))//soket listemdeki soket ile sonlanan soketi karşılaştır ve onu listeden çıkar
                        {

                            sonlanan_clien = __ClientSockets[i]._Name.Substring(1, __ClientSockets[i]._Name.Length - 1);
                            Console.WriteLine("client sonlandı " + sonlanan_clien);
                            __ClientSockets.RemoveAt(i);//soket listemden kaldır
                            //  label2.Text = "clientt: " + __ClientSockets.Count.ToString();
                            for (int j = 0; j < ListBox1.Items.Count; j++)//list boxtanda kaldır
                            {
                                if (ListBox1.Items[j].Equals(sonlanan_clien))
                                {
                                    ListBox1.Items.RemoveAt(j);

                                }
                            }
                        }
                    }
                    clientlerden_sil(sonlanan_clien);
                    // 
                    return;
                }
                if (received != 0)
                {
                    byte[] dataBuf = new byte[received];//mesajı byte çevirdik

                    Array.Copy(_buffer, dataBuf, received);//dataBuf'u _buffer e de kopyaladık


                    string text = Encoding.ASCII.GetString(dataBuf);// dataBuf'u texte çevirdik
                    //  label1.Text = "alinan mesaj : " + text;
                    Console.WriteLine("alinan mesaj " + text);
                    string reponse = string.Empty;

                    if (text.Contains("@@"))//eğer text in içerisinde @@ varsa bu client yeni kaydolacak
                    {
                        for (int i = 0; i < ListBox1.Items.Count; i++)//listboxtaki elementler kadar don
                        {
                            if (socket.RemoteEndPoint.ToString().Equals(__ClientSockets[i]._Socket.RemoteEndPoint.ToString()))//bu soket daha onceden varsa
                            {
                                ListBox1.Items.RemoveAt(i);//onu listeden sil
                                ListBox1.Items.Insert(i, text.Substring(1, text.Length - 1));//tekrardan ekle
                                __ClientSockets[i]._Name = text;//ismini tekrardan ver
                                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);//BeginReceive=  almaya başla recursıve=ReceiveCallback
                                isimleri_gonder();

                                return;
                            }
                        }
                    }
                    int index = text.IndexOf(" ");
                    string cli = text.Substring(0, index);

                    string mesaj = "";
                    int uzunluk = (text.Length) - (index + 2);
                    index = index + 2;
                    mesaj = text.Substring(index, uzunluk);
                    gonder_gelen_mesaji(cli, text, mesaj);//gelen mesajı parçaladım ilgili cliente gondermek için
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (socket.RemoteEndPoint.ToString().Equals(__ClientSockets[i]._Socket.RemoteEndPoint.ToString()))//soket ile listemdeki soketi karşılaştırıyorum eşit ise o soketten gelen leri yazıom
                        {

                        }
                    }



                    //if (text.Contains("cik"))
                    //{
                    //    return;
                    //}
                    // reponse = "serverdan  :  " + text;
                    //  Sendata(socket, reponse);

                }//received==0 sa client çıkış yapmıştır onu listeden sil
                else
                {
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            __ClientSockets.RemoveAt(i);
                            Console.WriteLine("çıktıı");


                            // label2.Text = "clientt: " + __ClientSockets.Count.ToString();
                        }
                    }
                }
            }
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);//almaya başla soketten ReceiveCallback recursive
        }
        public void clientlerden_sil(string sonlanan_client)
        {
            string sil = "sil*" + sonlanan_clien;
            for (int j = 0; j < __ClientSockets.Count; j++)//soket sayısı kadar dön
            {
                if (__ClientSockets[j]._Socket.Connected)//soket baglıysa
                {



                    Sendata(__ClientSockets[j]._Socket, sil);//client isimlerini gonderıoz cunku tum clientleri bilsin diye clientler

                    Thread.Sleep(20);//burasını clientlere ->clientleri gonderırken hata olmasın dıye


                }
            }
        }
        public void gonder_gelen_mesaji(string cli, string text, string mesaj)
        {
            //gelen=@@aa
            //__ClientSockets[i]._Name=@@aa
            string parcc = text.Substring(2, 2);
            Console.WriteLine("bu benım aradıgım " + parcc);
            cli = "@" + cli;

            Console.WriteLine("gelen_cli = " + cli + "\n cli_ismi" + __ClientSockets[0]._Name + "\n text :" + text);
            if (cli.Equals(__ClientSockets[0]._Name))
            {
                Console.WriteLine("oldu bu iş");
            }


            Console.WriteLine("mesajj " + mesaj);
            int ind__ = (mesaj.IndexOf("*") + 1);
            string parcalanm = mesaj.Substring(ind__, mesaj.Length - ind__);
            string mess = mesaj.Substring(0, (ind__ - 1));
            string gonder_ = parcalanm + ": " + mess;
            Console.WriteLine("gonderecek " + gonder_);
            try
            {
                for (int j = 0; j < __ClientSockets.Count; j++)
                {
                    if (__ClientSockets[j]._Socket.Connected)
                    {
                        if (__ClientSockets[j]._Name.Equals(cli))//a clienti b clientine mesaj gonderırken
                        {

                            Sendata(__ClientSockets[j]._Socket, gonder_);//a clienti b clientine mesaj gonderırken
                            Thread.Sleep(20);//burasını clientlere ->clientleri gonderırken hata olmasın dıye
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("gonder_gelen_mesaji() hata " + e.Message);
            }
        }
        void Sendata(Socket socket, string mesajj)//soket ve mesajı geldı
        {
            byte[] data = Encoding.ASCII.GetBytes(mesajj);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);//sokete verıyı gonder
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        private void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            __ClientSockets = new List<SocketT2h>();
            SetupServer();
        }
        public void isimleri_gonder()
        {
            for (int j = 0; j < __ClientSockets.Count; j++)//soket sayısı kadar dön
            {
                if (__ClientSockets[j]._Socket.Connected)//soket baglıysa
                {
                    for (int i = 0; i < ListBox1.Items.Count; i++)//listedeki isimler kadar
                    {


                        Sendata(__ClientSockets[j]._Socket, ListBox1.Items[i].ToString());//client isimlerini gonderıoz cunku tum clientleri bilsin diye clientler

                        Thread.Sleep(20);//burasını clientlere ->clientleri gonderırken hata olmasın dıye
                    }

                }
            }
        }
        public class SocketT2h
        {
            public Socket _Socket { get; set; }
            public string _Name { get; set; }
            public SocketT2h(Socket socket)
            {
                this._Socket = socket;
            }
        }
    }
}
