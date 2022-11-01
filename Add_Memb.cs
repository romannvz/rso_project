using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;

namespace bd_
{
    public partial class Add_Memb : Form
    {
        private SqlConnection rsoConnection = null;
        public Form1 mainform;
        public List<string> name_sqd = new List<string>();
        public List<string> name_sqd_with_abr = new List<string>();
        public string name_inst = "";
        public int id_headq = -1;
        public string name_headq = "";
        public List<string> inst_to_head = new List<string>();
        public List<string[]> mem = new List<string[]>();

        public Add_Memb()
        {
            InitializeComponent();
            maskedTextBox1.Width = 30;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (maskedTextBox1.Text.Count() != 17) { MessageBox.Show("Введите телефон полностью!", "Ошибка в заполнении полей"); }
            else if (maskedTextBox1.Text[4].ToString() != "9") { MessageBox.Show("Телефон должен начинаться с '+79...' ", "Ошибка в заполнении полей"); }
            else if (maskedTextBox1.Text[4].ToString() == " " || maskedTextBox1.Text[5].ToString() == " " ||
                maskedTextBox1.Text[6].ToString() == " " || maskedTextBox1.Text[9].ToString() == " " ||
                maskedTextBox1.Text[10].ToString() == " " || maskedTextBox1.Text[11].ToString() == " " ||
                maskedTextBox1.Text[13].ToString() == " " || maskedTextBox1.Text[14].ToString() == " " ||
                maskedTextBox1.Text[15].ToString() == " " || maskedTextBox1.Text[16].ToString() == " ")
            { MessageBox.Show("В телефоне не может быть пробела!"); }
            else
            {
                int squad_id = -1;
                string result = ""; int stat_find = -1;
                SqlCommand check_available = new SqlCommand($"select phone from members", rsoConnection);
                SqlDataReader check_available_read = check_available.ExecuteReader();
                while (check_available_read.Read())
                {
                    if (check_available_read[0].ToString() == maskedTextBox1.Text) { stat_find = 1; break; }
                }
                check_available_read.Close();
                if (stat_find == 1)
                    MessageBox.Show("Боец с таким номером телефона уже есть в системе.", "Ошибка внесения.");
                else
                {
                    DialogResult dr = DialogResult.Cancel;
                    SqlCommand check_available1 = new SqlCommand($"select id_member from members where surname = @sn and name = @n and patronymic = @pt and birthday = @b", rsoConnection);
                    check_available1.Parameters.AddWithValue("sn", maskedTextBox2.Text);
                    check_available1.Parameters.AddWithValue("n", maskedTextBox3.Text);
                    check_available1.Parameters.AddWithValue("pt", maskedTextBox4.Text);
                    check_available1.Parameters.AddWithValue("b", dateTimePicker1.Value);
                    SqlDataReader check_available1_read = check_available1.ExecuteReader();
                    while (check_available1_read.Read())
                    {
                        result = Convert.ToString(check_available1_read[0]);
                        if (result != "")
                        {
                            stat_find = 1;
                            dr = MessageBox.Show("Боец с такими данными, но другим \nномером телефона уже зарегистрирован.\n\nПроверьте введённые данные, и если всё верно \nи нужно внести бойца с такими данными,\nнажмите кнопку 'Да'. Если хотите исправить данные,\nнажмите 'Нет'.", "Ошибка внесения", MessageBoxButtons.YesNo);
                            break;
                        }
                    }
                    check_available1_read.Close();
                    if (dr == DialogResult.Yes || dr == DialogResult.Cancel || stat_find != 1)
                    {
                        SqlCommand id_sq = new SqlCommand($"select Id_Squad from Squads where Name = @name", rsoConnection);
                        id_sq.Parameters.AddWithValue("Name", name_sqd_from_combobox1);
                        SqlDataReader sq;
                        sq = id_sq.ExecuteReader();
                        while (sq.Read())
                        {
                            squad_id = Convert.ToInt32(sq["Id_Squad"]);
                        }
                        sq.Close();
                        id_sq.Cancel();
                        SqlCommand command = new SqlCommand(
                            $"insert into members " +
                            $"(Surname, Name, Patronymic, Birthday, Phone, Registration,status, Id_Squad) " +
                            $"values " +
                            $"(@srn, @n, @pt, @bd, @ph, @reg,@st, @id_sq)",
                            rsoConnection);
                        command.Parameters.AddWithValue("srn", maskedTextBox2.Text);
                        command.Parameters.AddWithValue("n", maskedTextBox3.Text);
                        command.Parameters.AddWithValue("pt", maskedTextBox4.Text);
                        command.Parameters.AddWithValue("bd", dateTimePicker1.Value.Date);
                        command.Parameters.AddWithValue("ph", maskedTextBox1.Text);
                        command.Parameters.AddWithValue("reg", dateTimePicker2.Value.Date);
                        command.Parameters.AddWithValue("st", comboBox5.Text);
                        command.Parameters.AddWithValue("id_sq", squad_id);
                        MessageBox.Show("Успешно внесён " + command.ExecuteNonQuery().ToString() + " человек.");
                        //mainform.RefreshMembersToHeadSquad();
                    }
                }
                //string squad;
                //int squad_id = -1; int chmem_id = -1; string log = ""; int err = 0;
                //DateTime b; DateTime r; 

                //if (maskedTextBox1.Text == "" || maskedTextBox2.Text == "" || maskedTextBox3.Text == "" || maskedTextBox4.Text == "")
                //{ MessageBox.Show("Заполните все поля!", "Ошибка в заполнении полей"); }

                /*//проверка на правильность ввода данных
                else if (maskedTextBox2.Text.Contains(" ")) { MessageBox.Show("В фамилии не может быть пробела!", "Ошибка в заполнении полей"); }
                else if (maskedTextBox3.Text.Contains(" ")) { MessageBox.Show("В имени не может быть пробела!", "Ошибка в заполнении полей"); }
                //else if (maskedTextBox3.Text.Contains(" ")) { MessageBox.Show("В отчестве не может быть пробела!"); }
                else if (maskedTextBox1.Text[4].ToString() == " " || maskedTextBox1.Text[5].ToString() == " " ||
                    maskedTextBox1.Text[6].ToString() == " " || maskedTextBox1.Text[9].ToString() == " " ||
                    maskedTextBox1.Text[10].ToString() == " " || maskedTextBox1.Text[11].ToString() == " " ||
                    maskedTextBox1.Text[13].ToString() == " " || maskedTextBox1.Text[14].ToString() == " " ||
                    maskedTextBox1.Text[15].ToString() == " " || maskedTextBox1.Text[16].ToString() == " ")
                { MessageBox.Show("В телефоне не может быть пробела!"); }
                else if (maskedTextBox1.Text[4].ToString() != "9") { MessageBox.Show("Телефон должен начинаться с '+79...' ", "Ошибка в заполнении полей"); }
                else if (maskedTextBox1.Text.Count() != 17) { MessageBox.Show("Введите телефон полностью!", "Ошибка в заполнении полей"); }
                else
                {
                    //всё введено корректно. 
                    //проверяем на зарегистрированность такого человека уже в системе*/
                /*SqlCommand check_user = new SqlCommand("select Id_Member from members where surname=@srn and name=@n and patronymic=@pt and birthday=@bd and phone=@ph", rsoConnection);
                check_user.Parameters.AddWithValue("srn", maskedTextBox2.Text);
                check_user.Parameters.AddWithValue("n", maskedTextBox3.Text);
                check_user.Parameters.AddWithValue("pt", maskedTextBox4.Text);
                check_user.Parameters.AddWithValue("bd", dateTimePicker1.Value.Date);
                check_user.Parameters.AddWithValue("ph", maskedTextBox1.Text);
                //check_user.Parameters.AddWithValue("reg", dateTimePicker2.Value.Date);
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

                    //логин свободен, нет препятствий для регистрации. заносим данные
                    //заносим человека в таблицу Members

                    //вычислим id отряда


                    SqlCommand id_sq = new SqlCommand("select Id_Squad from Squads where Name = @name_sqd", rsoConnection);
                    string name_checked_sqd = "";
                    foreach (var i in name_sqd_with_abr)
                        if (i.Contains(comboBox1.Text.ToString()) == true)
                        {
                            //MessageBox.Show("Выбрано " + i);
                            foreach (var j in name_sqd)
                                if (i.Contains(j))
                                {
                                    //MessageBox.Show("Итог : " + j);
                                    name_checked_sqd = j;
                                }
                        }
                    id_sq.Parameters.AddWithValue("name_sqd", name_checked_sqd);
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
                        add_new_mem.Parameters.AddWithValue("srn", maskedTextBox2.Text);
                        add_new_mem.Parameters.AddWithValue("n", maskedTextBox3.Text);
                        add_new_mem.Parameters.AddWithValue("pt", maskedTextBox4.Text);
                        add_new_mem.Parameters.AddWithValue("bd", dateTimePicker1.Value.Date);
                        add_new_mem.Parameters.AddWithValue("ph", maskedTextBox1.Text);
                        add_new_mem.Parameters.AddWithValue("reg", dateTimePicker2.Value.Date);
                        add_new_mem.Parameters.AddWithValue("id_sq", squad_id);
                        MessageBox.Show("Информация об (" + add_new_mem.ExecuteNonQuery().ToString() + ") участнике занесена в систему.", "Занесение информации о человеке");
                    }
                }
                else
                {
                    if (err == 2) { MessageBox.Show("Этот человек уже зарегистрирован.\nЕго логин: " + log, "Попытка занесения существующих данных"); }
                }
            //}*/
                /*b = dateTimePicker1.Value.Date;
            r = dateTimePicker2.Value.Date;
            squad = comboBox1.Text.ToString();
            SqlCommand id_sq = new SqlCommand($"select Id_Squad from Squads where Name = @name", rsoConnection);
            id_sq.Parameters.AddWithValue("Name", squad);
            SqlDataReader sq;
            sq = id_sq.ExecuteReader();
            while (sq.Read())
            {
                squad_id = Convert.ToInt32(sq["Id_Squad"]);
            }
            sq.Close();
            id_sq.Cancel();
            SqlCommand command = new SqlCommand(
                $"insert into members " +
                $"(Surname, Name, Patronymic, Birthday, Phone, Registration, Id_Squad) " +
                $"values " +
                $"(@srn, @n, @pt, @bd, @ph, @reg, @id_sq)",
                rsoConnection);
            command.Parameters.AddWithValue("srn", maskedTextBox2.Text);
            command.Parameters.AddWithValue("n", maskedTextBox3.Text);
            command.Parameters.AddWithValue("pt", maskedTextBox4.Text);
            command.Parameters.AddWithValue("bd", b);
            command.Parameters.AddWithValue("ph", maskedTextBox1.Text);
            command.Parameters.AddWithValue("reg", r);
            command.Parameters.AddWithValue("id_sq", squad_id);
            MessageBox.Show("Успешно внесён " + command.ExecuteNonQuery().ToString() + " человек.");*/
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
            mainform.Show();
            maskedTextBox2.Clear(); maskedTextBox3.Clear();maskedTextBox4.Clear(); maskedTextBox1.Clear(); comboBox1.SelectedIndex = -1;
            maskedTextBox5.Clear(); comboBox2.SelectedIndex = -1; comboBox2.Enabled = false; dateTimePicker3.Enabled = false;
            comboBox3.SelectedIndex = -1; comboBox3.Enabled = false; button2.Enabled = false;
            comboBox4.SelectedIndex = -1; comboBox5.Enabled = false; radioButton1.Enabled = false; radioButton2.Enabled = false;
            button3.Enabled = false;
            comboBox2.SelectedIndex = -1;
            //comboBox3.Items.Clear(); comboBox4.Items.Clear();
        }

        private void Add_Memb_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "rsoDataSet1.Directions". При необходимости она может быть перемещена или удалена.
            this.directionsTableAdapter.Fill(this.rsoDataSet1.Directions);
            rsoConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["rso"].ConnectionString);
            rsoConnection.Open();

            // TODO: данная строка кода позволяет загрузить данные в таблицу "rsoDataSet.Squads". При необходимости она может быть перемещена или удалена.
            //this.squadsTableAdapter.Fill(this.rsoDataSet1.Squads);

            dateTimePicker1.MinDate = DateTime.Today.AddYears(-100);
            dateTimePicker1.MaxDate = DateTime.Today.AddYears(-14);
            dateTimePicker2.MinDate = new DateTime(2004, 01, 01);
            dateTimePicker2.MaxDate = DateTime.Today;
            dateTimePicker1.Value = new DateTime(DateTime.Today.AddYears(-20).Year, 01, 01);
            dateTimePicker2.Value = DateTime.Today;
            dateTimePicker3.MinDate = new DateTime(2004, 01, 01);
            dateTimePicker3.MaxDate = DateTime.Today;

            textBox1.Text = name_headq;
        }

        public void add_names_squads_to_head()
        {
            foreach (var s in name_sqd_with_abr)
            { if(!comboBox1.Items.Contains(s))
                    comboBox1.Items.Add(s); 
                if(!comboBox4.Items.Contains(s))
                    comboBox4.Items.Add(s);
                if (!comboBox6.Items.Contains(s))
                    comboBox6.Items.Add(s);
            }
        }

        public void clear_names_squads_to_head()
        {
            comboBox1.Items.Clear(); comboBox4.Items.Clear(); comboBox6.Items.Clear();
            name_sqd_with_abr.Clear();
        }

        public void add_institutions_to_head()
        {
            foreach (var s in inst_to_head)
                if(!comboBox3.Items.Contains(s))
                    comboBox3.Items.Add(s);
        }

        public void clear_institutions_to_head()
        {
            comboBox3.Items.Clear();
        }

        private void maskedTextBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox2.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (((MaskedTextBox)sender).SelectionStart > ((MaskedTextBox)sender).Text.Count())
                ((MaskedTextBox)sender).SelectionStart = ((MaskedTextBox)sender).Text.Count();
        }

        private void maskedTextBox3_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox3.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (((MaskedTextBox)sender).SelectionStart > ((MaskedTextBox)sender).Text.Count())
                ((MaskedTextBox)sender).SelectionStart = ((MaskedTextBox)sender).Text.Count();
        }

        private void maskedTextBox4_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox4.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (((MaskedTextBox)sender).SelectionStart > ((MaskedTextBox)sender).Text.Count())
                ((MaskedTextBox)sender).SelectionStart = ((MaskedTextBox)sender).Text.Count();
        }

        private void maskedTextBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox1.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (((MaskedTextBox)sender).SelectionStart > ((MaskedTextBox)sender).Text.Count())
                ((MaskedTextBox)sender).SelectionStart = ((MaskedTextBox)sender).Text.Count();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            dateTimePicker3.Enabled = true; comboBox3.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //string name_new_squad = maskedTextBox5.Text.ToString(); int id_direct = (comboBox2.SelectedIndex)+1;
            //DateTime date_reg = dateTimePicker3.Value.Date; string inst = comboBox3.Text.ToString();
            string s = ""; int status_find = -1; //int status_insert = -1;
            //MessageBox.Show(name_new_squad+"\n"+id_direct+"\n"+date_reg+"\n"+inst);
            SqlCommand check_sq_in_inst = new SqlCommand("select name from Squads where Name = @name_sq and id_institution = (select id_institution from institutions where name = @nm_i)", rsoConnection);
            check_sq_in_inst.Parameters.AddWithValue("name_sq", maskedTextBox5.Text.ToString());
            //check_sq.Parameters.AddWithValue("id_d", id_direct);
            //check_sq.Parameters.AddWithValue("dr", date_reg);
            check_sq_in_inst.Parameters.AddWithValue("nm_i", comboBox3.Text.ToString());
            SqlDataReader check_sq_in_inst_read = check_sq_in_inst.ExecuteReader();
            while (check_sq_in_inst_read.Read())
            {
                s = check_sq_in_inst_read[0].ToString();
                if (s != "")
                    if (s.ToLower() == maskedTextBox5.Text.ToString().ToLower())
                    { 
                        MessageBox.Show("Отряд '" + s + "' уже существует на базе\nучебного заведения "
                            + comboBox3.Text.ToString() + ". \nУкажите другое название."); 
                        status_find = 0;
                        maskedTextBox5.Focus();
                        maskedTextBox5.Select(0, maskedTextBox5.Text.Count());
                        break; 
                    }
            }
            check_sq_in_inst_read.Close();
            if (status_find != 0)
            {
                SqlCommand check_sq_everywhere = new SqlCommand("select s.name, i.name from squads s, institutions i where s.name = @name_sq and s.id_institution = i.id_institution", rsoConnection);
                check_sq_everywhere.Parameters.AddWithValue("name_sq", maskedTextBox5.Text.ToString());
                SqlDataReader check_sq_everywhere_read = check_sq_everywhere.ExecuteReader();
                while (check_sq_everywhere_read.Read())
                {
                    string sqd = check_sq_everywhere_read[0].ToString(); string instit = check_sq_everywhere_read[1].ToString();
                    if (sqd.ToLower() == maskedTextBox5.Text.ToString().ToLower())
                    {
                        DialogResult dr = MessageBox.Show("Отряд '" + sqd + "' уже существует на базе\nучебного заведения " 
                            + instit + ". \nВсё равно хотите создать отряд с таким названием?","Предупреждение", MessageBoxButtons.YesNo);
                        if (dr == DialogResult.Yes)
                            status_find = 1;
                        else if (dr == DialogResult.No)
                        {
                            maskedTextBox5.Focus();
                            maskedTextBox5.Select(0, maskedTextBox5.Text.Count());
                        }
                        break; 
                    }
                }
                check_sq_everywhere_read.Close();
            }
            if (status_find == -1 || status_find > 
                0)
            {
                DialogResult dr1 = MessageBox.Show("Проверьте введённые данные:\nНазвание отряда: " + maskedTextBox5.Text +
                    "\nНаправление отряда: " + comboBox2.Text + "\nДата создания: " + dateTimePicker3.Value.Date + "" +
                    "\nУчебное заведение: " + comboBox3.Text + "\n\nЕсли всё верно, нажмите кнопку 'ОК'."
                    + " В противном случае, нажмите кнопку 'Отмена', исправьте данные и повторите попытку.","Подтверждение информации",
                    MessageBoxButtons.OKCancel);
                if (dr1 == DialogResult.OK)
                {
                    int id_i = -1;
                    SqlCommand find_id_i = new SqlCommand("select id_institution from institutions where name = @name",rsoConnection);
                    find_id_i.Parameters.AddWithValue("name", comboBox3.Text);
                    SqlDataReader find_id_i_read = find_id_i.ExecuteReader();
                    while (find_id_i_read.Read())
                    { id_i = Convert.ToInt32(find_id_i_read["id_institution"]); }
                    find_id_i_read.Close();

                    SqlCommand insert_new_sq = new SqlCommand($"insert into squads(name,id_direction,date_registration,id_institution," +
                    "id_headquarters) values (@name,@id_d,@dr,@id_i,@id_h)", rsoConnection);
                    insert_new_sq.Parameters.AddWithValue("name", maskedTextBox5.Text.ToString());
                    insert_new_sq.Parameters.AddWithValue("id_d", (comboBox2.SelectedIndex) + 1);
                    insert_new_sq.Parameters.AddWithValue("dr", dateTimePicker3.Value.Date);
                    insert_new_sq.Parameters.AddWithValue("id_i", id_i);
                    insert_new_sq.Parameters.AddWithValue("id_h", id_headq);
                    MessageBox.Show("Успешно создан " + insert_new_sq.ExecuteNonQuery().ToString() + " отряд.");
                    maskedTextBox5.Clear();comboBox2.Enabled = false;comboBox2.SelectedIndex = -1;
                    dateTimePicker3.Enabled = false; comboBox3.Enabled = false;comboBox3.SelectedIndex = -1; button2.Enabled = false;
                }    
                else if (dr1 == DialogResult.Cancel)
                {
                    maskedTextBox5.Focus();
                    maskedTextBox5.Select(0, maskedTextBox5.Text.Count());
                }
            }
            mainform.RefreshSquadsToHeadQ();

        }

        private void maskedTextBox5_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox5.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (((MaskedTextBox)sender).SelectionStart > ((MaskedTextBox)sender).Text.Count())
                ((MaskedTextBox)sender).SelectionStart = ((MaskedTextBox)sender).Text.Count();
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            dateTimePicker2.Enabled = true;button1.Enabled = true;
        }

        string name_sqd_from_combobox1 = "";

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            string s = ""; string name_checked_sqd = "";
            foreach (var i in name_sqd_with_abr)
                if (i.Contains(comboBox1.Text.ToString()) == true)
                {
                    //MessageBox.Show("Выбрано " + i);
                    foreach (var j in name_sqd)
                        if (i.Contains(j))
                        {
                            //MessageBox.Show("Итог : " + j);
                            name_checked_sqd = j;
                            name_sqd_from_combobox1 = j;
                        }
                        
                }
                    
            SqlCommand dr_sq = new SqlCommand("select date_registration from squads where name = @name_sq", rsoConnection);
            dr_sq.Parameters.AddWithValue("name_sq", name_checked_sqd);
            //check_sq.Parameters.AddWithValue("id_d", id_direct);
            //check_sq.Parameters.AddWithValue("dr", date_reg);
            //check_sq.Parameters.AddWithValue("nm_i", inst);
            SqlDataReader dr_sq_read = dr_sq.ExecuteReader();
            while (dr_sq_read.Read())
            {
                s = Convert.ToString(dr_sq_read[0]);
            }
            dr_sq_read.Close();
            dateTimePicker2.MinDate = Convert.ToDateTime(s).Date;
        }

        private void maskedTextBox5_TextChanged(object sender, EventArgs e)
        {
            comboBox2.Enabled = true;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = true;
        }

        private void maskedTextBox2_TextChanged(object sender, EventArgs e)
        {
            maskedTextBox3.Enabled = true;
        }

        private void maskedTextBox3_TextChanged(object sender, EventArgs e)
        {
            maskedTextBox4.Enabled = true;
        }

        private void maskedTextBox4_TextChanged(object sender, EventArgs e)
        {
            maskedTextBox1.Enabled = true;
        }

        private void maskedTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox1.Text.Count() == 17)
            { dateTimePicker1.Enabled = true; comboBox1.Enabled = true; }
            else
            { dateTimePicker1.Enabled = false; comboBox1.Enabled = false; }
        }

        public List<string> full_names_for_show = new List<string>();
        public List<string> surnames = new List<string>();
        public List<string> birthdays = new List<string>();
        public List<string> phones = new List<string>();


        private void comboBox4_TextChanged(object sender, EventArgs e)
        {
            full_names_for_show.Clear(); comboBox5.Items.Clear();
            if (comboBox4.SelectedIndex != -1)
                comboBox5.Enabled = true;
            string name_checked_sqd = "";
            foreach (var i in name_sqd_with_abr)
                if (i.Contains(comboBox4.Text.ToString()) == true)
                    foreach (var j in name_sqd)
                        if (i.Contains(j))
                            name_checked_sqd = j;
            SqlCommand insert_people_ch = new SqlCommand("select surname,name,patronymic,birthday,phone from members where id_squad = (select id_squad from squads where name = @name_sq) order by surname,name,patronymic", rsoConnection);
            insert_people_ch.Parameters.AddWithValue("name_sq", name_checked_sqd);
            SqlDataReader insert_people_ch_read = insert_people_ch.ExecuteReader();
            while (insert_people_ch_read.Read())
            {
                DateTime dt = Convert.ToDateTime(insert_people_ch_read[3]);
                comboBox5.Items.Add(Convert.ToString(insert_people_ch_read[0] + " " + insert_people_ch_read[1] + " " + insert_people_ch_read[2] + ", "+
                    dt+ " гр, моб. тел. " + insert_people_ch_read[4]));
                //listBox1.Items.Add(Convert.ToString(insert_people_ch_read[0] + " " + insert_people_ch_read[1] + " " + insert_people_ch_read[2] + ", " +
                    //dt + " гр, моб. тел. " + insert_people_ch_read[4]));
                full_names_for_show.Add(Convert.ToString(insert_people_ch_read[0] + " " + insert_people_ch_read[1] + " " + insert_people_ch_read[2] + ", " +
                    dt + " гр, моб. тел. " + insert_people_ch_read[4]));
                surnames.Add(insert_people_ch_read[0].ToString() +" "+ insert_people_ch_read[1].ToString()+" "+ insert_people_ch_read[2].ToString());
                //names.Add(insert_people_ch_read[1].ToString());
                //patronymics.Add(insert_people_ch_read[2].ToString());
                birthdays.Add(insert_people_ch_read[3].ToString()); 
                phones.Add(insert_people_ch_read[4].ToString());
            }
            insert_people_ch_read.Close();
            int stat_h = 0;
            SqlCommand insert_head = new SqlCommand("select m.surname,m.name,m.patronymic from members m, squads s, institutions i where m.id_squad = (select id_squad from squads where name = @name_sq) and s.id_squad = m.id_squad and m.id_member = s.id_head", rsoConnection);
            insert_head.Parameters.AddWithValue("name_sq", name_checked_sqd);
            insert_head.Parameters.AddWithValue("name_headq", name_headq);
            SqlDataReader insert_head_read = insert_head.ExecuteReader();
            while (insert_head_read.Read())
            {
                if (insert_head_read[0].ToString() != "" && insert_head_read[1].ToString() != "" )
                    //|| insert_head_read[0].ToString() != " " && insert_head_read[1].ToString() != " "
                    //|| insert_head_read[0].ToString() != null && insert_head_read[1].ToString() != null)
                    //|| insert_head_read[0].ToString() != "" && insert_head_read[1].ToString() != ""  && insert_head_read[2].ToString() != "")
                    textBox2.Text = insert_head_read[0].ToString() + " " + insert_head_read[1] + " " + insert_head_read[2];
                else
                    textBox2.Text = "--не назначен--";
                stat_h = 1;
            }
            if (stat_h == 0)
                textBox2.Text = "--не назначен--";
            insert_head_read.Close();
            int stat_v = 0;
            SqlCommand insert_vice = new SqlCommand("select m.surname,m.name,m.patronymic from members m, squads s where m.id_squad = (select id_squad from squads where name = @name_sq) and s.id_squad = m.id_squad and m.id_member = s.id_vice", rsoConnection);
            insert_vice.Parameters.AddWithValue("name_sq", name_checked_sqd);
            SqlDataReader insert_vice_read = insert_vice.ExecuteReader();
            while (insert_vice_read.Read())
            {
                if (insert_vice_read[0].ToString() != "" && insert_vice_read[1].ToString() != "")
                    //insert_vice_read[0].ToString() != "" && insert_vice_read[1].ToString() != "" && insert_vice_read[2].ToString() != "")
                    textBox3.Text = insert_vice_read[0].ToString() + " " + insert_vice_read[1] + " " + insert_vice_read[2];

                else
                    textBox3.Text = "--не назначен--";
                stat_v = 1;
            }
            if (stat_v == 0)
                textBox3.Text = "--не назначен--";
            insert_vice_read.Close();
            comboBox5.Items.Add("-----------------------из других отрядов-----------------------");
            SqlCommand insert_people_unch = new SqlCommand("select m.surname,m.name,m.patronymic,m.birthday,m.phone from members m, squads s where s.id_headquarters = (select id_headquarters from headquarters where name = @name_headq) and m.id_squad != (select id_squad from squads where name = @name_sq) and m.Id_Squad = s.Id_Squad order by m.surname,m.name,m.patronymic", rsoConnection);
            insert_people_unch.Parameters.AddWithValue("name_sq", name_checked_sqd);
            insert_people_unch.Parameters.AddWithValue("name_headq", name_headq);
            //MessageBox.Show("NameSq = " + name_checked_sqd + " ; NameHeadQ = " + name_headq);
            SqlDataReader insert_people_unch_read = insert_people_unch.ExecuteReader();
            while (insert_people_unch_read.Read())
            {
                comboBox5.Items.Add(Convert.ToString(insert_people_unch_read[0] + " " + insert_people_unch_read[1] + " " + insert_people_unch_read[2] + ", " + insert_people_unch_read[3] + " гр, моб. тел. " + insert_people_unch_read[4]));
                full_names_for_show.Add(Convert.ToString(insert_people_unch_read[0] + " " + insert_people_unch_read[1] + " " + insert_people_unch_read[2] + ", " +
                    insert_people_unch_read[3] + " гр, моб. тел. " + insert_people_unch_read[4]));
                surnames.Add(insert_people_unch_read[0].ToString() + " "+insert_people_unch_read[1].ToString()+" "+ insert_people_unch_read[2].ToString());
                //names.Add(insert_people_unch_read[1].ToString());
                //patronymics.Add(insert_people_unch_read[2].ToString());
                birthdays.Add(insert_people_unch_read[3].ToString());
                phones.Add(insert_people_unch_read[4].ToString());
            }
            insert_people_unch_read.Close();
        }

        private void comboBox5_TextChanged(object sender, EventArgs e)
        {
            radioButton1.Enabled = true; radioButton2.Enabled = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //foreach (var i in full_names_for_search)
                //listBox2.Items.Add(i);
            string name_checked_sqd = ""; //string row = "";
            foreach (var i in name_sqd_with_abr)
                if (i.Contains(comboBox4.Text.ToString()) == true)
                    foreach (var j in name_sqd)
                        if (i.Contains(j))
                            name_checked_sqd = j;
            //MessageBox.Show(comboBox5.Text);
            string fullname = ""; string sur = ""; string nm = ""; string ptr = ""; string brth = ""; string ph = "";
            foreach (var i in full_names_for_show)
                if (i.Contains(comboBox5.Text))
                {
                    MessageBox.Show(i);
                    foreach (var s in surnames)
                    {
                        //MessageBox.Show(s);
                        if (i.Contains(s))
                        {
                            fullname = s;
                            //foreach (var n in names)
                            //if(i.Contains(n))
                            //{
                            //nm = n;
                            //foreach(var patr in patronymics)
                            //if (i.Contains(patr))
                            //{
                            //ptr = patr;
                            foreach (var b in birthdays)
                                if (i.Contains(b))
                                {
                                    brth = b;
                                    foreach (var p in phones)
                                        if (i.Contains(p))
                                        {
                                            ph = p;
                                            break;
                                        }
                                    break;
                                }
                            break;
                            //  }//
                            break;
                            // }//
                            break;
                        }
                    }    break;
                }
            MessageBox.Show(fullname /*+ " " + nm + " " + ptr*/ + " " + brth + " " + ph+" - это фулнейм. дальше по отдельности каждая переменная");
            int id_m = -1; int k = 0;
            for(int i = 0; i < fullname.Length; i++)
            {
                if (fullname[i].ToString() != " ")
                    if (k == 0)
                        sur += fullname[i].ToString();
                    else if (k == 1)
                        nm += fullname[i].ToString();
                    else if (k == 2)
                        ptr += fullname[i].ToString();
                    else { }
                else k++;
            }
            //MessageBox.Show(sur);
            //MessageBox.Show(nm);
            //MessageBox.Show(ptr);
            if(ptr!="" && ptr!=" " && ph!="" && ph!=" ")
            {
                SqlCommand find_id = new SqlCommand("select id_member from members where surname=@sn and name=@nm and patronymic=@pt and birthday=@bd and phone=@ph", rsoConnection);
                find_id.Parameters.AddWithValue("sn", sur);
                find_id.Parameters.AddWithValue("nm", nm);
                find_id.Parameters.AddWithValue("pt", ptr);
                find_id.Parameters.AddWithValue("bd", Convert.ToDateTime(brth));
                find_id.Parameters.AddWithValue("ph", ph);
                SqlDataReader find_id_read = find_id.ExecuteReader();
                while (find_id_read.Read())
                {
                    id_m = Convert.ToInt32(find_id_read[0]);
                }
                find_id_read.Close();
                MessageBox.Show("ID MEMBER - " + id_m);
            }
            else if (ptr=="" || ptr == " ")
            {
                SqlCommand find_id = new SqlCommand("select id_member from members where surname=@sn and name=@nm and birthday=@bd and phone=@ph", rsoConnection);
                find_id.Parameters.AddWithValue("sn", sur);
                find_id.Parameters.AddWithValue("nm", nm);
                //find_id.Parameters.AddWithValue("pt", ptr);
                find_id.Parameters.AddWithValue("bd", Convert.ToDateTime(brth));
                find_id.Parameters.AddWithValue("ph", ph);
                SqlDataReader find_id_read = find_id.ExecuteReader();
                while (find_id_read.Read())
                {
                    id_m = Convert.ToInt32(find_id_read[0]);
                }
                find_id_read.Close();
                MessageBox.Show("ID MEMBER - " + id_m);
            }
            else if (ph=="" || ph==" ")
            {
                SqlCommand find_id = new SqlCommand("select id_member from members where surname=@sn and name=@nm and patronymic=@pt and birthday=@bd", rsoConnection);
                find_id.Parameters.AddWithValue("sn", sur);
                find_id.Parameters.AddWithValue("nm", nm);
                find_id.Parameters.AddWithValue("pt", ptr);
                find_id.Parameters.AddWithValue("bd", Convert.ToDateTime(brth));
                //find_id.Parameters.AddWithValue("ph", ph);
                SqlDataReader find_id_read = find_id.ExecuteReader();
                while (find_id_read.Read())
                {
                    id_m = Convert.ToInt32(find_id_read[0]);
                }
                find_id_read.Close();
                MessageBox.Show("ID MEMBER - " + id_m);
            }
            SqlCommand upd = new SqlCommand("update members set id_squad = (select id_squad from squads where name = @nms) where id_member = @id", rsoConnection);
            upd.Parameters.AddWithValue("nms", name_checked_sqd);
            upd.Parameters.AddWithValue("id", id_m);
            MessageBox.Show("Изменен айди сквада: " + upd.ExecuteNonQuery().ToString());
            if (radioButton1.Checked)
            {
                SqlCommand upd_head = new SqlCommand("update squads set id_head = @id_head where id_squad = (select id_squad from squads where name = @nm)", rsoConnection);
                upd_head.Parameters.AddWithValue("nm", name_checked_sqd);
                upd_head.Parameters.AddWithValue("id_head", id_m);
                MessageBox.Show("Изменен айди командира: " + upd_head.ExecuteNonQuery().ToString());
            }
            if(radioButton2.Checked)
            {
                SqlCommand upd_vice = new SqlCommand("update squads set id_vice = @id_vice where id_squad = (select id_squad from squads where name = @nm)", rsoConnection);
                upd_vice.Parameters.AddWithValue("nm", name_checked_sqd);
                upd_vice.Parameters.AddWithValue("id_vice", id_m);
                MessageBox.Show("Изменен айди комиссара: " + upd_vice.ExecuteNonQuery().ToString());
            }
            
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
        }

        private void comboBox6_TextChanged(object sender, EventArgs e)
        {
            label16.Text = ""; listView1.Items.Clear(); mem.Clear();
            //if (comboBox6.SelectedIndex != -1)
                //comboBox5.Enabled = true;
            string name_checked_sqd = "";
            foreach (var i in name_sqd_with_abr)
                if (i.Contains(comboBox6.Text.ToString()) == true)
                    foreach (var j in name_sqd)
                        if (i.Contains(j))
                            name_checked_sqd = j;
            SqlCommand inf_sqd = new SqlCommand("select s.name, d.name, s.date_registration, m.surname, m.name, m.patronymic, m.phone, i.name from members m, squads s, directions d, institutions i, headquarters h where i.id_region = h.id_region_authority and h.id_headquarter = s.id_headquarters and s.id_head = m.id_member and s.id_institution = i.id_institution and d.id_direction = s.id_direction and h.name = @nh and s.name = @ns", rsoConnection);
            inf_sqd.Parameters.AddWithValue("nh", name_headq);
            inf_sqd.Parameters.AddWithValue("ns", name_checked_sqd);
            SqlDataReader inf_sqd_read = inf_sqd.ExecuteReader();
            while (inf_sqd_read.Read())
            {
                label16.Text = "ЛСО: " + inf_sqd_read[1].ToString() + " '" + inf_sqd_read[0].ToString() + "', основан "
                    + Convert.ToDateTime(inf_sqd_read[2]).ToShortDateString() + " на базе учебного заведения " +
                    inf_sqd_read[7].ToString()+ ". \nКомандир: ";
                if (inf_sqd_read[3] != null)
                    label16.Text += inf_sqd_read[3].ToString() + " " +
                        inf_sqd_read[4] + " " + inf_sqd_read[5] + ", тел. " + inf_sqd_read[6].ToString()+"\n";
                else
                    label16.Text += "не назначен.\n";
            }
            inf_sqd_read.Close();
            string[] row;
            SqlCommand inf_mem = new SqlCommand("select m.surname, m.name, m.patronymic, m.birthday, m.phone, m.registration, m.status from squads s, members m where s.name = @ns and s.id_squad = m.id_squad order by m.surname, m.name, m.patronymic", rsoConnection);
            inf_mem.Parameters.AddWithValue("ns", name_checked_sqd);
            SqlDataReader inf_mem_read = inf_mem.ExecuteReader();
            while (inf_mem_read.Read())
            {
                row = new string[] { inf_mem_read[0].ToString(), inf_mem_read[1].ToString(),inf_mem_read[2].ToString(),
                    Convert.ToDateTime(inf_mem_read[3]).ToShortDateString(), inf_mem_read[4].ToString(),
                    Convert.ToDateTime(inf_mem_read[5]).ToShortDateString(), inf_mem_read[6].ToString() };
                mem.Add(row);
            }
            inf_mem_read.Close();
            foreach (var i in mem)
                listView1.Items.Add(new ListViewItem(i));
            label16.Text += "Количество бойцов: "+listView1.Items.Count.ToString();

        }

        private void maskedTextBox6_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox6.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (((MaskedTextBox)sender).SelectionStart > ((MaskedTextBox)sender).Text.Count())
                ((MaskedTextBox)sender).SelectionStart = ((MaskedTextBox)sender).Text.Count();
        }

        private void maskedTextBox7_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox7.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (((MaskedTextBox)sender).SelectionStart > ((MaskedTextBox)sender).Text.Count())
                ((MaskedTextBox)sender).SelectionStart = ((MaskedTextBox)sender).Text.Count();
        }

        private void maskedTextBox8_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox8.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (((MaskedTextBox)sender).SelectionStart > ((MaskedTextBox)sender).Text.Count())
                ((MaskedTextBox)sender).SelectionStart = ((MaskedTextBox)sender).Text.Count();
        }

        private void maskedTextBox6_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox6.Text == "")
            { maskedTextBox7.Enabled = false; maskedTextBox8.Enabled = false; }
            else
                maskedTextBox7.Enabled = true;
        }

        private void maskedTextBox7_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox7.Text == "")
                maskedTextBox8.Enabled = false; 
            else
                maskedTextBox8.Enabled = true;
        }

        private void maskedTextBox8_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox8.Text == "")
                button4.Enabled = false;
            else
                button4.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int stat = -1; int reg = -1;
            SqlCommand inst_available = new SqlCommand("select i.name, i.city, i.head, i.id_region from institutions i, headquarters h where i.id_region = h.id_region_authority and h.name = @nh", rsoConnection);
            inst_available.Parameters.AddWithValue("nh", name_headq);
            SqlDataReader inst_available_read = inst_available.ExecuteReader();
            while (inst_available_read.Read())
            {
                if (inst_available_read[0].ToString() == maskedTextBox6.Text &&
                    inst_available_read[1].ToString() == maskedTextBox7.Text &&
                    inst_available_read[2].ToString() == maskedTextBox8.Text)
                { MessageBox.Show("Такое учебное заведение уже есть в базе.", "Ошибка внесения"); stat = 1; break; }
                else if (inst_available_read[0].ToString() == maskedTextBox6.Text)
                { MessageBox.Show("В вашем регионе уже есть заведение с подобным названием.", "Ошибка внесения"); stat = 1; break; }
                else if (inst_available_read[2].ToString() == maskedTextBox8.Text)
                { MessageBox.Show("Этот руководитель управляет другим учебным заведением.", "Ошибка внесения"); stat = 1; break; }
                else
                { stat = 0; reg = Convert.ToInt32(inst_available_read[3]); }
            }
            inst_available_read.Close();
            if (stat == 0)
            {
                SqlCommand inst_insert = new SqlCommand("insert into institutions (name, id_region, city, head) values (@nm,@id_r,@c,@h)", rsoConnection);
                inst_insert.Parameters.AddWithValue("nm", maskedTextBox6.Text);
                inst_insert.Parameters.AddWithValue("id_r", reg);
                inst_insert.Parameters.AddWithValue("c", maskedTextBox7.Text);
                inst_insert.Parameters.AddWithValue("h", maskedTextBox8.Text);
                MessageBox.Show("Успешно внесено " + inst_insert.ExecuteNonQuery().ToString() + " учебное заведение.");
            }
        }
    }
}
