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

namespace HQXChat
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            con = new MySqlConnection("Server=sql11.freesqldatabase.com;Database=sql11493111;user=sql11493111;Pwd=UAZsakuyuQ");
        }
        MySqlConnection con;
        MySqlCommand cmd;
        MySqlDataReader dr;
        public static string usr;
        public static string pwd;
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            usr = guna2TextBox2.Text;
            pwd = guna2TextBox1.Text;
            cmd = new MySqlCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "SELECT * FROM Uyeler where Kadi='" + guna2TextBox2.Text + "' AND Sifre='" + guna2TextBox1.Text + "'";
            dr = cmd.ExecuteReader();
            if (guna2TextBox2.Text == "" && guna2TextBox1.Text == "")
            {
                label2.Visible = true;
                label2.Text = "Lüffen Kullanıcı Adı Ve Şifre Giriniz";
            }
            else if (dr.Read())
            {
                Main main = new Main();
                this.Hide();
                main.Show();
            }
            else
            {
                label2.Visible=true;
                label2.Text = "Hatalı Kullanıcı Adı veya Şifre Girdiniz";
            }
            con.Close();
        }

        private void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            guna2TextBox1.Text = "";
            guna2TextBox2.Text = "";
            guna2TextBox3.Text = "";
            label2.Text = "";
            label4.Text = "";
            if (guna2ToggleSwitch1.Checked == false)
            {
                guna2TextBox3.Visible = false;
                guna2Button2.Visible = false;
                guna2Button1.Enabled = true;
            }
            else if (guna2ToggleSwitch1.Checked == true)
            {
                guna2TextBox3.Visible = true;
                guna2Button2.Visible = true;
                guna2Button1.Enabled = false;
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch1.Checked == false)
            {
                guna2TextBox3.Visible = false;
                guna2Button2.Visible = false;
                guna2Button1.Enabled = true;
            }
            try
            {
                con.Open();
                label3.ForeColor = Color.Black;
                label3.Text = "Bağlantı başarılı";
                con.Close();
            }
            catch (Exception ex)
            {
                label3.ForeColor = Color.Red;
                label3.Text = $"{ ex.Message} Hatası Oluştuğu Için Bağlantı Başarısız!";
                guna2Button1.Enabled=false;
                guna2Button2.Enabled=false;
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (guna2TextBox2.Text == "" && guna2TextBox1.Text == "" && guna2TextBox3.Text == "")
            {
                label2.Visible = true;
                label2.Text = "Lütfen Tüm Alanları Doldurunuz";
            }
            string ekle = "insert into Uyeler(Kadi,Sifre) values('" + guna2TextBox2.Text + "','" + guna2TextBox1.Text + "')";
            con.Open();
            MySqlCommand cmd = new MySqlCommand(ekle, con);
            try
            {
                if (cmd.ExecuteNonQuery() == 1)
                {
                    guna2MessageDialog1.Show("Kayıt Eklendi");
                    guna2ToggleSwitch1.Checked = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            con.Close();
        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch1.Checked == true)
            {
                cmd = new MySqlCommand();
                con.Open();
                cmd.Connection = con;
                cmd.CommandText = "SELECT * FROM Uyeler where Kadi='" + guna2TextBox2.Text + "'";
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    label2.Visible=true;
                    label2.Text = "Bu İsim Zaten Kullanılıyor";
                    guna2Button2.Enabled = false;
                }
                else
                {
                    label2.Visible=false; 
                    guna2Button2.Enabled=true;
                }
                con.Close();
            }
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch1.Checked == true)
            {
                if (guna2TextBox1.TextLength <= 6)
                {
                    label2.Visible = true;
                    label2.Text = "Şifre uzunluğu 6'dan Kısa Olamaz";
                    guna2Button2.Enabled = false;
                }
                else
                {
                    label2.Visible = false;
                    guna2Button2.Enabled=true;
                }
                if (guna2TextBox3.Text != guna2TextBox1.Text)
                {
                    label4.Visible = true;
                    label4.ForeColor = Color.Red;
                    label4.Text = "Şifreler Uyuşmuyor";
                    guna2Button2.Enabled = false;
                }
                else
                {
                    label4.ForeColor = Color.Green;
                    label4.Text = "Şifreler Uyuşuyor";
                    guna2Button2.Enabled = true;
                }
            }
        }

        private void guna2TextBox3_TextChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch1.Checked == true)
            {
                if (guna2TextBox3.Text != guna2TextBox1.Text)
                {
                    label4.Visible = true;
                    label4.ForeColor = Color.Red;
                    label4.Text = "Şifreler Uyuşmuyor";
                    guna2Button2.Enabled = false;
                }
                else
                {
                    label4.ForeColor = Color.Green;
                    label4.Text = "Şifreler Uyuşuyor";
                    guna2Button2.Enabled = true;
                }
            }
        }
    }
}
