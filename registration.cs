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
    public partial class registration : Form
    {
        private SqlConnection rsoConnection = null;
        public auth mainform;

        public registration()
        {
            InitializeComponent();
        }

        public void registration_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mainform != null) mainform.Show();
        }

        private void registration_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "rsoDataSet.Squads". При необходимости она может быть перемещена или удалена.
            this.squadsTableAdapter.Fill(this.rsoDataSet.Squads);

            //блокируем ввод некорректных данных в календаре
            dateTimePicker1.MinDate = DateTime.Today.AddYears(-100);
            dateTimePicker1.MaxDate = DateTime.Today.AddYears(-14);
            dateTimePicker2.MinDate = new DateTime(2004, 01, 01);
            dateTimePicker2.MaxDate = DateTime.Today;
            dateTimePicker1.Value = new DateTime(DateTime.Today.AddYears(-20).Year, 01, 01);
            dateTimePicker2.Value = DateTime.Today;

            rsoConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["rso"].ConnectionString);
            rsoConnection.Open();

            comboBox1.SelectedIndex = -1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
            mainform.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //проверка на пустоту полей
            int squad_id = -1; int mem_id = -1; string usrnm = ""; int chmem_id = -1; string log = ""; int err = 0; int stat_find = -1;
            if (maskedTextBox1.Text == "" || maskedTextBox2.Text == "" || maskedTextBox3.Text == "" || maskedTextBox4.Text == "" ||
                maskedTextBox5.Text == "" || maskedTextBox6.Text == "" || maskedTextBox7.Text == "")
            { MessageBox.Show("Заполните все поля!", "Ошибка в заполнении полей"); }

            //проверка на правильность ввода данных
            else if (maskedTextBox1.Text.Contains(" ")) { MessageBox.Show("В фамилии не может быть пробела!", "Ошибка в заполнении полей"); }
            else if (maskedTextBox2.Text.Contains(" ")) { MessageBox.Show("В имени не может быть пробела!", "Ошибка в заполнении полей"); }
            //else if (maskedTextBox3.Text.Contains(" ")) { MessageBox.Show("В отчестве не может быть пробела!"); }
            else if (maskedTextBox4.Text[4].ToString() ==" " || maskedTextBox4.Text[5].ToString()  == " " || 
                maskedTextBox4.Text[6].ToString() == " " || maskedTextBox4.Text[9].ToString() == " " ||
                maskedTextBox4.Text[10].ToString() == " " || maskedTextBox4.Text[11].ToString() == " " ||
                maskedTextBox4.Text[13].ToString() == " " || maskedTextBox4.Text[14].ToString() == " " ||
                maskedTextBox4.Text[15].ToString() == " " || maskedTextBox4.Text[16].ToString() == " ")
            { MessageBox.Show("В телефоне не может быть пробела!"); }
            else if (maskedTextBox4.Text[4].ToString() != "9") { MessageBox.Show("Телефон должен начинаться с '+79...' ", "Ошибка в заполнении полей"); }
            else if (maskedTextBox4.Text.Count() != 17) { MessageBox.Show("Введите телефон полностью!", "Ошибка в заполнении полей"); }
            else if (maskedTextBox5.Text.Count() < 8) { MessageBox.Show("Логин не может быть меньше 8 символов.", "Ошибка в заполнении полей"); }
            else if (maskedTextBox5.Text.Contains(" ")) { MessageBox.Show("Логин не может содержать пробел!", "Ошибка в заполнении полей"); }
            else if (maskedTextBox6.Text.Count() < 8) { MessageBox.Show("Пароль не может быть меньше 8 символов.", "Ошибка в заполнении полей"); }
            else if (maskedTextBox6.Text.Contains(" ")) { MessageBox.Show("Пароль не может содержать пробел!", "Ошибка в заполнении полей"); }
            else if (maskedTextBox6.Text != maskedTextBox7.Text) { MessageBox.Show("Пароли не совпадают!", "Ошибка в заполнении полей"); }
            else
            {
                //всё введено корректно. 
                //проверяем на зарегистрированность такого человека уже в системе
                SqlCommand check_available = new SqlCommand($"select phone from members", rsoConnection);
                SqlDataReader check_available_read = check_available.ExecuteReader();
                while (check_available_read.Read())
                {
                    if (check_available_read[0].ToString() == maskedTextBox4.Text) { stat_find = 1; break; }
                }
                check_available_read.Close();
                if (stat_find == 1)
                    MessageBox.Show("Боец с таким номером телефона уже есть в системе.", "Ошибка внесения.");
                else
                {
                    SqlCommand check_user = new SqlCommand("select Id_Member from members where surname=@srn and name=@n and patronymic=@pt and birthday=@bd and phone=@ph and registration=@reg"/*" and id_squad=@id_sq"*/, rsoConnection);
                    check_user.Parameters.AddWithValue("srn", maskedTextBox1.Text);
                    check_user.Parameters.AddWithValue("n", maskedTextBox2.Text);
                    check_user.Parameters.AddWithValue("pt", maskedTextBox3.Text);
                    check_user.Parameters.AddWithValue("bd", dateTimePicker1.Value.Date);
                    check_user.Parameters.AddWithValue("ph", maskedTextBox4.Text);
                    check_user.Parameters.AddWithValue("reg", dateTimePicker2.Value.Date);
                    //check_user.Parameters.AddWithValue("id_sq", squad_id);
                    SqlDataReader check_user_read;
                    check_user_read = check_user.ExecuteReader();
                    while (check_user_read.Read())
                    {
                        chmem_id = Convert.ToInt32(check_user_read["Id_Member"]);
                    }
                    check_user_read.Close();
                    SqlCommand output_login = new SqlCommand("select username from LP where Id_Member = @id", rsoConnection);
                    output_login.Parameters.AddWithValue("id", chmem_id);
                    SqlDataReader output_read;
                    output_read = output_login.ExecuteReader();
                    while (output_read.Read())
                    {
                        log = Convert.ToString(output_read["username"]);
                    }
                    output_read.Close();
                    if (chmem_id != -1 && log == "") { err = 1; } //значит есть в member но нет в lp
                    if (chmem_id != -1 && log != "") { err = 2; } //значить есть и в member, и в lp
                    if (chmem_id == -1 || err < 2)
                    {
                        //такой человек ещё не регался. пустим дальше и проверим наличие такого же логина



                        SqlCommand check_username = new SqlCommand("select username from lp where username = @un", rsoConnection);
                        check_username.Parameters.AddWithValue("un", maskedTextBox5.Text);
                        SqlDataReader check_read;
                        check_read = check_username.ExecuteReader();
                        while (check_read.Read())
                        {
                            usrnm = Convert.ToString(check_read["username"]);
                        }
                        check_read.Close();
                        if (usrnm != "") { MessageBox.Show("Этот логин уже занят! Придумайте другой.", "Вы что, самозванец?.."); }
                        else
                        {
                            //логин свободен, нет препятствий для регистрации. заносим данные
                            //заносим человека в таблицу Members

                            //вычислим id отряда
                            SqlCommand id_sq = new SqlCommand("select Id_Squad from Squads where Name = @name_sqd", rsoConnection);
                            id_sq.Parameters.AddWithValue("name_sqd", comboBox1.Text);
                            SqlDataReader id_sq_read;
                            id_sq_read = id_sq.ExecuteReader();
                            while (id_sq_read.Read())
                            {
                                squad_id = Convert.ToInt32(id_sq_read["Id_Squad"]);
                            }
                            id_sq_read.Close();

                            if (err == 1) { MessageBox.Show("Информация об этом участнике уже есть в системе."); }
                            else
                            {
                                SqlCommand add_new_mem = new SqlCommand("insert into members (Surname, Name, Patronymic, Birthday, Phone, Registration, Id_Squad) values (@srn, @n, @pt, @bd, @ph, @reg, @id_sq)", rsoConnection);
                                add_new_mem.Parameters.AddWithValue("srn", maskedTextBox1.Text);
                                add_new_mem.Parameters.AddWithValue("n", maskedTextBox2.Text);
                                add_new_mem.Parameters.AddWithValue("pt", maskedTextBox3.Text);
                                add_new_mem.Parameters.AddWithValue("bd", dateTimePicker1.Value.Date);
                                add_new_mem.Parameters.AddWithValue("ph", maskedTextBox4.Text);
                                add_new_mem.Parameters.AddWithValue("reg", dateTimePicker2.Value.Date);
                                add_new_mem.Parameters.AddWithValue("id_sq", squad_id);
                                MessageBox.Show("Информация об (" + add_new_mem.ExecuteNonQuery().ToString() + ") участнике занесена в систему.", "Занесение информации о человеке");
                            }

                            //человек занесён. теперь нужно вычислить его id в таблице Members чтобы занести этот же id в таблицу LP
                            SqlCommand ident_id_new_mem = new SqlCommand("select Id_Member from Members where surname=@srn and name=@n and patronymic=@pt and birthday=@bd and phone=@ph and registration=@reg and id_squad=@id_sq", rsoConnection);
                            ident_id_new_mem.Parameters.AddWithValue("srn", maskedTextBox1.Text);
                            ident_id_new_mem.Parameters.AddWithValue("n", maskedTextBox2.Text);
                            ident_id_new_mem.Parameters.AddWithValue("pt", maskedTextBox3.Text);
                            ident_id_new_mem.Parameters.AddWithValue("bd", dateTimePicker1.Value.Date);
                            ident_id_new_mem.Parameters.AddWithValue("ph", maskedTextBox4.Text);
                            ident_id_new_mem.Parameters.AddWithValue("reg", dateTimePicker2.Value.Date);
                            ident_id_new_mem.Parameters.AddWithValue("id_sq", squad_id);
                            ident_id_new_mem.Parameters.AddWithValue("name_sqd", comboBox1.Text);
                            SqlDataReader ident_id_read;
                            ident_id_read = ident_id_new_mem.ExecuteReader();
                            while (ident_id_read.Read())
                            {
                                mem_id = Convert.ToInt32(ident_id_read["Id_Member"]);
                            }
                            ident_id_read.Close();

                            if (mem_id == -1) { MessageBox.Show("Кажется, вы состоите в другом отряде (не " + comboBox1.Text + ")."); }
                            else
                            {
                                //нашли id. вносим сведения о логине и пароле в таблицу LP
                                SqlCommand add_new_lp = new SqlCommand("insert into lp values (@id, @un, @psw)", rsoConnection);
                                add_new_lp.Parameters.AddWithValue("id", mem_id);
                                add_new_lp.Parameters.AddWithValue("un", maskedTextBox5.Text);
                                add_new_lp.Parameters.AddWithValue("psw", maskedTextBox6.Text);
                                MessageBox.Show("Информация о паре логин-пароль успешно занесена в базу. (" + add_new_lp.ExecuteNonQuery().ToString() + ")", "Занесение данных логин-пароль");

                                //регистрация завершена успешно. теперь чистим все поля и выкидываем на главное меню
                                MessageBox.Show("Вы успешно зарегистрированы!\nДобро пожаловать в команду РСО!\n\nНе забудьте свои данные:\nЛогин: " + maskedTextBox5.Text + "\nПароль: " + maskedTextBox6.Text, "Успешная регистрация");
                                maskedTextBox1.Clear(); maskedTextBox2.Clear(); maskedTextBox3.Clear(); maskedTextBox4.Clear();
                                maskedTextBox5.Clear(); maskedTextBox6.Clear(); maskedTextBox7.Clear();
                                this.Close();
                                mainform.Show();
                            }
                        }
                    }
                    else
                    {
                        if (err == 2) { MessageBox.Show("Кажется, вы уже зарегистрированы.\nВаш логин: " + log, "Мы с вами раньше не встречались?"); }
                    }
                }
            }
        }

        private void maskedTextBox5_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox5.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (maskedTextBox5.SelectionStart > maskedTextBox5.Text.Count()) 
                maskedTextBox5.SelectionStart = maskedTextBox5.Text.Count();
        }

        private void maskedTextBox6_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox6.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (maskedTextBox6.SelectionStart > maskedTextBox6.Text.Count())
                maskedTextBox6.SelectionStart = maskedTextBox6.Text.Count();
        }

        private void maskedTextBox7_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox7.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (maskedTextBox7.SelectionStart > maskedTextBox7.Text.Count())
                maskedTextBox7.SelectionStart = maskedTextBox7.Text.Count();
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

        private void maskedTextBox3_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox3.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (maskedTextBox3.SelectionStart > maskedTextBox3.Text.Count())
                maskedTextBox3.SelectionStart = maskedTextBox3.Text.Count();
        }

        private void maskedTextBox4_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox4.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (maskedTextBox4.SelectionStart > maskedTextBox4.Text.Count())
                maskedTextBox4.SelectionStart = maskedTextBox4.Text.Count();
        }

        private void maskedTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox1.Text == "")
            {
                maskedTextBox2.Enabled = false; maskedTextBox3.Enabled = false; maskedTextBox4.Enabled = false;
                dateTimePicker1.Enabled = false; comboBox1.Enabled = false; dateTimePicker2.Enabled = false;
                maskedTextBox5.Enabled = false; maskedTextBox6.Enabled = false; maskedTextBox7.Enabled = false;
            }
            else
                maskedTextBox2.Enabled = true;
        }

        private void maskedTextBox2_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox2.Text == "")
            {
                maskedTextBox3.Enabled = false; maskedTextBox4.Enabled = false;
                dateTimePicker1.Enabled = false; comboBox1.Enabled = false; dateTimePicker2.Enabled = false;
                maskedTextBox5.Enabled = false; maskedTextBox6.Enabled = false; maskedTextBox7.Enabled = false;
            }
            else
                maskedTextBox3.Enabled = true;
        }

        private void maskedTextBox3_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox3.Text == "")
            {
                maskedTextBox4.Enabled = false;
                dateTimePicker1.Enabled = false; comboBox1.Enabled = false; dateTimePicker2.Enabled = false;
                maskedTextBox5.Enabled = false; maskedTextBox6.Enabled = false; maskedTextBox7.Enabled = false;
            }
            else
                maskedTextBox4.Enabled = true;
        }

        private void maskedTextBox4_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox4.Text == "")
            {
                dateTimePicker1.Enabled = false; comboBox1.Enabled = false; dateTimePicker2.Enabled = false;
                maskedTextBox5.Enabled = false; maskedTextBox6.Enabled = false; maskedTextBox7.Enabled = false;
            }
            else
            /*if (maskedTextBox4.Text.Count() == 17 && maskedTextBox4.Text != ""
                && maskedTextBox4.Text[4].ToString() != " " || maskedTextBox4.Text[5].ToString() != " " ||
                maskedTextBox4.Text[6].ToString() != " " || maskedTextBox4.Text[9].ToString() != " " ||
                maskedTextBox4.Text[10].ToString() != " " || maskedTextBox4.Text[11].ToString() != " " ||
                maskedTextBox4.Text[13].ToString() != " " || maskedTextBox4.Text[14].ToString() != " " ||
                maskedTextBox4.Text[15].ToString() != " " || maskedTextBox4.Text[16].ToString() != " ")*/
            { dateTimePicker1.Enabled = true; comboBox1.Enabled = true; }
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                SqlCommand dr_sq = new SqlCommand("select date_registration from squads where name = @name_sq", rsoConnection);
                dr_sq.Parameters.AddWithValue("name_sq", comboBox1.Text);
                SqlDataReader dr_sq_read = dr_sq.ExecuteReader();
                while (dr_sq_read.Read())
                    dateTimePicker2.MinDate = Convert.ToDateTime(dr_sq_read[0]).Date;
                dr_sq_read.Close();
                maskedTextBox5.Enabled = true; dateTimePicker2.Enabled = true;
            }
            else
            {
                dateTimePicker2.Enabled = false; maskedTextBox5.Enabled = false; 
                maskedTextBox6.Enabled = false; maskedTextBox7.Enabled = false;
            }
        }

        private void maskedTextBox5_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox5.Text == "")
            {
                maskedTextBox6.Enabled = false; maskedTextBox7.Enabled = false;
            }
            else
            {
                maskedTextBox6.Enabled = true; maskedTextBox7.Enabled = true;
            }
        }
    }
}
