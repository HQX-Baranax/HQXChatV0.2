using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HQXChat
{
    public partial class Main : Form
    {
        private Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Main()
        {
            InitializeComponent();
            con = new MySqlConnection("Server=sql11.freesqldatabase.com;Database=sql11493111;user=sql11493111;Pwd=UAZsakuyuQ");
        }
        byte[] receivedBuf = new byte[1024];//veri almak için yer ayırdık
        MySqlConnection con;
        MySqlCommand cmd;
        MySqlDataReader dr;
        bool usrpaneldgr = false;
        bool arkpanel = false;
        bool ayarpannel = false;
        bool duzenmod = false;

        //fonksiyonlar


        private void ReceiveData(IAsyncResult ar)//burası asenkron oldugu için hep çalışır thread gibi veriyi almak için
        {

            int listede_yok = 0;//yok
            try
            {

                Socket socket = (Socket)ar.AsyncState;//asenkron soketi alırız
                int received = socket.EndReceive(ar);//verinin toplam uzunlugu
                byte[] dataBuf = new byte[received];//verıyı byte cevırdık
                Array.Copy(receivedBuf, dataBuf, received);//dataBuf=receivedBuf ve received uzunluk
                string gelen = Encoding.ASCII.GetString(dataBuf).ToString();//serverdan gelen mesaj
                if (gelen.Contains("sil*"))
                {
                    string parcala = gelen.Substring(4, (gelen.Length - 4));
                    Console.WriteLine("degerim  " + parcala);
                    for (int j = 0; j < listBox1.Items.Count; j++)//list boxtanda kaldır
                    {
                        if (listBox1.Items[j].Equals(parcala))
                        {
                            listBox1.Items.RemoveAt(j);

                        }
                    }
                }
                else if (gelen.Contains("@"))//içerisinde @ içeriyorsa clienti listeye eklicez
                {

                    for (int i = 0; i < listBox1.Items.Count; i++)//listedeki itemler kadar dön
                    {
                        if (listBox1.Items[i].ToString().Equals(gelen))//listede varsa o client
                        {
                            listede_yok = 1;//var
                        }
                    }
                    if (listede_yok == 0)//yoksa  ekle clienti
                    {
                        string ben = "@" + loginkad;
                        if (ben.Equals(gelen))//kendimi ekleme
                        {

                        }
                        else
                        {
                            listBox1.Items.Add(gelen);
                        }
                    }

                }
                else
                {
                    //label3.Text = (gelen);
                    richTextBox1.AppendText(gelen + "\n");
                }


                //rb_chat.AppendText("\nServer: " + label3.Text);

                _clientSocket.BeginReceive(receivedBuf, 0, receivedBuf.Length, SocketFlags.None, new AsyncCallback(ReceiveData), _clientSocket);
                /*
          buffer=Türünde bir dizi Byte yani alınan veri için depolama konumu.
          offset=Sıfır tabanlı konumu buffer , alınan verileri depolamak parametre.
          size=Almak için bayt sayısı.
          socketFlags=Bit seviyesinde birleşimini SocketFlags değerleri.
          callback=Bir AsyncCallback işlemi tamamlandığında harekete geçirmek için bir yönteme başvuran bir temsilci.
          state=Alma işlemi hakkında bilgi içeren bir kullanıcı tanımlı nesne.Bu nesne için geçirilen EndReceive işlemi tamamlandığında temsilci.
                https://msdn.microsoft.com/tr-tr/library/dxkwh6zw(v=vs.110).aspx adresine bakabilirsin
                  */

            }
            catch (Exception e)
            {
                Console.WriteLine("ReceiveData() metodunda hata " + e.Message);
            }

        }
        private void SendLoop()
        {
            while (true)
            {
                //Console.WriteLine("Enter a request: ");
                //string req = Console.ReadLine();
                //byte[] buffer = Encoding.ASCII.GetBytes(req);
                //_clientSocket.Send(buffer);

                byte[] receivedBuf = new byte[1024];
                int rev = _clientSocket.Receive(receivedBuf);
                if (rev != 0)
                {
                    byte[] data = new byte[rev];
                    Array.Copy(receivedBuf, data, rev);
                    // label3.Text = ("Received: " + Encoding.ASCII.GetString(data));
                    richTextBox1.AppendText("\nServer: " + Encoding.ASCII.GetString(data) + "\n");
                }
                else _clientSocket.Close();

            }
        }

        private void LoopConnect()
        {
            int attempts = 0;
            while (!_clientSocket.Connected)//server çalışmıyorsa(çalışısaya kadar döngü döner)
            {
                try
                {
                    attempts++;
                    _clientSocket.Connect("127.0.0.1", 100);//127.0.0.1=IPAddress.Loopback demek 100 portuna bağlan
                }
                catch (SocketException)
                {
                    //Console.Clear();
                    //   label3.Text = ("bağlantılar: " + attempts.ToString());
                    Console.WriteLine("bağlantılar: " + attempts.ToString());
                }
            }
            // SendLoop();
            _clientSocket.BeginReceive(receivedBuf, 0, receivedBuf.Length, SocketFlags.None, new AsyncCallback(ReceiveData), _clientSocket);//AsyncCallback thread gibi asenkron eş zamansız çalışıyor
            byte[] buffer = Encoding.ASCII.GetBytes("@@" + loginkad);//ismimizin başına 2 tane @@ koydum belli olsun
            _clientSocket.Send(buffer);//veriyi gönderdim servera
            guna2Button1.Text = ("servere bağlandı!");//servere bağlandı
        }



        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            if (usrpaneldgr == false)
            {
                guna2Panel4.Visible = true;
                usrpaneldgr = true;
            }
            else if (usrpaneldgr == true)
            {
                guna2Panel4.Visible = false;
                usrpaneldgr = false;

            }
        }
        public static string loginkad;
        public static string loginsfr;
        private void Main_Load(object sender, EventArgs e)
        {
            loginkad = Login.usr;
            loginsfr = Login.pwd;
            CheckForIllegalCrossThreadCalls = false;
            this.Text = "Main | " + loginkad;
            guna2TextBox2.Text = loginkad;
            label1.Text = loginkad;
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT ID FROM Uyeler where Kadi='" + loginkad + "'", con);
            int Count = Convert.ToInt32(cmd.ExecuteScalar());
            if (Count != 0)
            {

                MySqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    label2.Text = "ID: " + dr["ID"].ToString();

                }

            }
            con.Close();
        }


        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (duzenmod == false)
            {
                duzenmod = true;
                guna2Button2.Text = "Bitti";
                guna2TextBox2.Enabled = true;
                guna2Button3.Enabled = false;
                guna2Button6.Enabled = false;
            }
            else if (duzenmod == true)
            {
                cmd = new MySqlCommand();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM Uyeler where Kadi='" + guna2TextBox2.Text + "'";
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    guna2TextBox2.BorderColor = Color.Red;
                    guna2TextBox2.FocusedState.BorderColor = Color.Red;
                    guna2MessageDialog1.Show("Bu Kullanıcı Adı Zaten Kullanılıyor");
                    guna2TextBox2.Text = loginkad;
                }
                else
                {
                    con.Close();
                    guna2TextBox2.BorderColor = Color.Green;
                    guna2TextBox2.FocusedState.BorderColor = Color.Green;
                    string gunc = "update Uyeler set Kadi='" + guna2TextBox2.Text + "' where Kadi='" + loginkad + "'";
                    MySqlCommand cmd = new MySqlCommand(gunc, con);
                    con.Open();
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        guna2MessageDialog1.Show("Kullanıcı Adı Deeğiştirildi!");
                        Login login = new Login();
                        login.Show();
                        this.Hide();
                    }
                    else
                    {
                        guna2MessageDialog1.Show("Kullanıcı Adı Deeğiştirilmedi!");
                        guna2TextBox2.Text = loginkad;
                        guna2Button3.Enabled = true;
                        guna2Button6.Enabled = true;
                    }
                }
                duzenmod = false;
                guna2TextBox2.Enabled = false;
                guna2Button2.Text = "Düzenle";
                con.Close();

            }
        }
        /*
        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {
            cmd = new MySqlCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "SELECT * FROM Uyeler where Kadi='" + guna2TextBox2.Text + "'";
            dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                guna2TextBox2.BorderColor = Color.Red;
                guna2TextBox2.FocusedState.BorderColor = Color.Red;
                guna2Button2.Enabled = false;
            }
            else
            {
                guna2TextBox2.BorderColor = Color.Green;
                guna2TextBox2.FocusedState.BorderColor = Color.Green;
                guna2Button2.Enabled = true;
            }
            con.Close();
        }
        */
        private void guna2TextBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2ImageButton3_Click(object sender, EventArgs e)
        {
            if (ayarpannel == false)
            {
                guna2GroupBox1.Visible = true;
                ayarpannel = true;
                if (guna2Panel13.Visible == true)
                {
                    guna2Panel13.Visible = false;
                }
            }
            else if (ayarpannel == true)
            {
                guna2GroupBox1.Visible = false;
                ayarpannel = false;
                guna2Panel13.Visible = true;
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = true;
            guna2Button3.Enabled = false;
            guna2Button2.Enabled = false;
            guna2Button6.Enabled = false;
        }

        private void guna2TextBox4_TextChanged(object sender, EventArgs e)
        {
            if (guna2TextBox4.Text == loginsfr)
            {
                guna2TextBox4.BorderColor = Color.Green;
                guna2TextBox4.FocusedState.BorderColor = Color.Green;
                guna2TextBox5.Enabled = true;
                guna2TextBox6.Enabled = true;
                guna2Button4.Enabled = true;
            }
            else if (guna2TextBox4.Text != loginsfr)
            {
                guna2TextBox4.BorderColor = Color.Red;
                guna2TextBox4.FocusedState.BorderColor = Color.Red;
                guna2TextBox5.Enabled = false;
                guna2TextBox6.Enabled = false;
                guna2Button4.Enabled = false;
            }
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            if (guna2TextBox4.Text == "" || guna2TextBox5.Text == "" || guna2TextBox6.Text == "")
            {
                guna2MessageDialog1.Show("Lütfen Tüm Alanları Doldurunuz!");
            }
            else
            {
                string yenisfr = "update Uyeler set Sifre='" + guna2TextBox6.Text + "' where Kadi='" + loginkad + "'  ";
                con.Open();
                MySqlCommand cmd = new MySqlCommand(yenisfr, con);
                if (cmd.ExecuteNonQuery() == 1)
                {
                    guna2MessageDialog1.Show("Şifre Değiştirildi!");
                    Login login = new Login();
                    login.Show();
                    this.Hide();
                    /* guna2Button2.Enabled =true;
                     guna2Button2.Enabled = true;
                     guna2TextBox5.Enabled = false;
                     guna2TextBox6.Enabled = false;
                     guna2Button4.Enabled = false;
                     groupBox1.Visible = false;
                     guna2Button3.Enabled = true;
                     guna2TextBox4.Text = "";
                     guna2TextBox5.Text = "";
                     guna2TextBox6.Text = "";*/
                }
                else
                {
                    guna2MessageDialog1.Show("Şifre Değiştirilemedi!");
                }
            }

        }

        private void guna2TextBox5_TextChanged(object sender, EventArgs e)
        {
            if (guna2TextBox5.Text.Length > 6 && guna2TextBox6.Text.Length > 6)
            {
                guna2Button4.Enabled = true;
                if (guna2TextBox5.Text == guna2TextBox6.Text)
                {
                    guna2TextBox5.BorderColor = Color.Green;
                    guna2TextBox5.FocusedState.BorderColor = Color.Green;
                    guna2TextBox6.BorderColor = Color.Green;
                    guna2TextBox6.FocusedState.BorderColor = Color.Green;
                    guna2Button4.Enabled = true;
                }
                else if (guna2TextBox5.Text != guna2TextBox6.Text)
                {
                    guna2TextBox5.BorderColor = Color.Red;
                    guna2TextBox5.FocusedState.BorderColor = Color.Red;
                    guna2TextBox6.BorderColor = Color.Red;
                    guna2TextBox6.FocusedState.BorderColor = Color.Red;
                    guna2Button4.Enabled = false;
                }
            }
            else
            {
                guna2TextBox5.BorderColor = Color.Red;
                guna2TextBox5.FocusedState.BorderColor = Color.Red;
                guna2TextBox6.BorderColor = Color.Red;
                guna2TextBox6.FocusedState.BorderColor = Color.Red;
                guna2Button4.Enabled = false;
            }
        }

        private void guna2TextBox6_TextChanged_1(object sender, EventArgs e)
        {
            if (guna2TextBox5.Text == guna2TextBox6.Text)
            {
                guna2TextBox5.BorderColor = Color.Green;
                guna2TextBox5.FocusedState.BorderColor = Color.Green;
                guna2TextBox6.BorderColor = Color.Green;
                guna2TextBox6.FocusedState.BorderColor = Color.Green;
                guna2Button4.Enabled = true;
            }
            else if (guna2TextBox5.Text != guna2TextBox6.Text)
            {
                guna2TextBox5.BorderColor = Color.Red;
                guna2TextBox5.FocusedState.BorderColor = Color.Red;
                guna2TextBox6.BorderColor = Color.Red;
                guna2TextBox6.FocusedState.BorderColor = Color.Red;
                guna2Button4.Enabled = false;
            }
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            DialogResult hspsilsoru = MessageBox.Show("Hesabını Silmek İstediğinden Emin misin?(Geri Dönüşü Yok)", "Hesabı Sil", MessageBoxButtons.YesNo);
            if (hspsilsoru == DialogResult.Yes)
            {
                string hsbsil = "delete from Uyeler where Kadi ='" + loginkad + "'";
                con.Open();
                MySqlCommand cmd = new MySqlCommand(hsbsil, con);
                if (cmd.ExecuteNonQuery() == 1)
                {
                    MessageBox.Show("Hesap Silidi");
                    Login login = new Login();
                    login.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Hesap Silinemedi");
                }
            }
        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void guna2ImageButton9_Click(object sender, EventArgs e)
        {
            if (_clientSocket.Connected)//client servera bağlı ise
            {
                string tmpStr = "";
                foreach (var item in listBox1.SelectedItems)//listboxtaki seçili itemlere
                {

                    tmpStr = listBox1.GetItemText(item);//isimlerini
                    byte[] buffer = Encoding.ASCII.GetBytes(tmpStr + " :" + guna2TextBox3.Text + "*" + loginkad);//byte çevir
                    _clientSocket.Send(buffer);//ve gönder ip ve porta
                    Thread.Sleep(20);//yapmasanda olur(fakat 4 cliente bırden mesaj gonderınce dıgerlerine gıtmeyebılir)

                }
                if (tmpStr.Equals(""))
                {
                    MessageBox.Show("lütfen listeden değer seçiniz");
                }
                else
                {
                    richTextBox1.AppendText(loginkad + ": " + guna2TextBox3.Text + "\n");
                }


            }
        }


        private void guna2Button1_Click(object sender, EventArgs e)
        {
            Thread t1 = new Thread(LoopConnect);
            t1.Start();
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Hide();
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            richTextBox1.ResetText();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            richTextBox1.ResetText();
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
