using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;

namespace bd_
{
    public partial class auth : Form
    {
        private SqlConnection rsoConnection = null;
        Form1 fr1 = new Form1();
        registration rg = new registration();
        public string in_surname;
        public string in_name;
        public int in_id_squad;
        public string in_squad;
        public int in_id;
        public string in_status;

        public auth()
        {
            InitializeComponent();
        }

        private void auth_Load(object sender, EventArgs e)
        {
            //label6.Parent = pictureBox1;
            rsoConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["rso"].ConnectionString);
            rsoConnection.Open();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            in_surname = ""; in_name = ""; in_id_squad = 0; in_squad = ""; in_id = 0; in_status = "";
            SqlDataReader dr_auth; SqlDataReader dr_name;
            string chkurn = maskedTextBox1.Text; string chkpsw = maskedTextBox2.Text; int l = -1; int p = -1;
            if (maskedTextBox1.Text == "" || maskedTextBox2.Text == "" || maskedTextBox1.Text == " " || maskedTextBox2.Text == " ")
            { MessageBox.Show("Пустые поля?"); goto ot; }
            SqlCommand auth = new SqlCommand("select * from lp", rsoConnection);
            dr_auth = auth.ExecuteReader();
            while (dr_auth.Read())//cursor
            {
                if (Convert.ToString(dr_auth["username"]) == chkurn)
                {
                    l = 0;
                    in_id = Convert.ToInt32(dr_auth["Id_Member"]);
                    if (Convert.ToString(dr_auth["password"]) == chkpsw)
                    {
                        dr_auth.Close();
                        SqlCommand surn = new SqlCommand($"select m.surname, m.name from Members m, LP lp where m.Id_Member = lp.Id_Member and lp.Id_Member = @id", rsoConnection);
                        surn.Parameters.AddWithValue("id", in_id);
                        dr_name = surn.ExecuteReader();
                        while (dr_name.Read())
                        {
                            in_surname = Convert.ToString(dr_name["Surname"]);
                            in_name = Convert.ToString(dr_name["Name"]);
                        }
                        dr_name.Close();
                        MessageBox.Show("Добро пожаловать, " + in_surname + " " + in_name);
                        fr1.mainform = this;
                        this.Hide();
                        fr1.ShowDialog();
                        break;
                    }
                    else { MessageBox.Show("Неверный пароль!"); break; }
                }
                else l = 1;
            }
            if (l == 1)
                MessageBox.Show("Пользователя с таким логином не существует!");
            else
                if (p == 1) { MessageBox.Show("Неверный пароль."); }
            dr_auth.Close();
        ot:;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //регистрация
            rg.mainform = this;
            this.Hide();
            rg.ShowDialog();
        }

        private void maskedTextBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox1.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (maskedTextBox1.SelectionStart > maskedTextBox1.Text.Count())
                maskedTextBox1.SelectionStart = maskedTextBox1.Text.Count();
        }

        private void maskedTextBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox2.Text == "") 
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (maskedTextBox2.SelectionStart > maskedTextBox2.Text.Count())
                maskedTextBox2.SelectionStart = maskedTextBox2.Text.Count();
        }
    }
}
