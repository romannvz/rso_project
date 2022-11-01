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
    public partial class Form1 : Form
    {
        private SqlConnection rsoConnection = null;
        public auth mainform;
        Add_Memb adm = new Add_Memb();
        private List<string[]> rows = new List<string[]>();
        private List<string[]> names_for_changing_to_comotr = new List<string[]>();
        private List<string[]> squads_to_head = new List<string[]>();
        private List<string[]> filteredList = null;
        private List<string[]> ss = new List<string[]>();
        int status_head_headq = -1;
        int id_headq = -1; int id_region = -1;
        //string[] names_for_changing_to_comotr;

        public Form1()
        {
            InitializeComponent();
        }

        public void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mainform != null) mainform.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "rsoDataSet.Squads". При необходимости она может быть перемещена или удалена.
            this.squadsTableAdapter.Fill(this.rsoDataSet.Squads);

            rsoConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["rso"].ConnectionString);
            rsoConnection.Open();
            if (rsoConnection.State == ConnectionState.Open)
            {
                MessageBox.Show("Подключение к БД установлено.", "Статус подключения");
            }

            //заполнение инфы об авторизованном
            textBox3.Text = mainform.in_surname;
            textBox4.Text = mainform.in_name;
            DateTime inf_db = DateTime.Now; DateTime inf_dr = DateTime.Now; int id_sqd = -1; string nmsqd = "";
            SqlCommand fill_inf = new SqlCommand($"select patronymic," +
                $" birthday, phone, registration, id_squad from members where surname = @sn and name = @n", rsoConnection);
            SqlCommand name_squad_by_id = new SqlCommand($"select name from squads where id_squad=@id", rsoConnection);
            fill_inf.Parameters.AddWithValue("sn", textBox3.Text);
            fill_inf.Parameters.AddWithValue("n", textBox4.Text);
            SqlDataReader fli;
            fli = fill_inf.ExecuteReader();
            while (fli.Read())
            {
                inf_db = Convert.ToDateTime(fli["Birthday"]);
                inf_dr = Convert.ToDateTime(fli["Registration"]);
                id_sqd = Convert.ToInt32(fli["Id_Squad"]);
                textBox17.Text = Convert.ToString(fli["Patronymic"]);
                maskedTextBox5.Text = Convert.ToString(fli["Phone"]);
            }
            fli.Close();
            name_squad_by_id.Parameters.AddWithValue("id", id_sqd);
            SqlDataReader idsqd;
            idsqd = name_squad_by_id.ExecuteReader();
            while (idsqd.Read())
            {
                nmsqd = Convert.ToString(idsqd["Name"]);
            }
            idsqd.Close();
            textBox5.Text = inf_db.ToString("dd/MM/yyyy");
            textBox6.Text = inf_dr.ToString("dd/MM/yyyy");
            textBox7.Text = nmsqd;
            mainform.in_id_squad = id_sqd;
            mainform.in_squad = nmsqd;

            //определение командир отряда

            SqlCommand chk_head = new SqlCommand($"select id_head from squads where id_squad = @id_s", rsoConnection);
            chk_head.Parameters.AddWithValue("id_s", id_sqd);
            SqlDataReader chk_head_rd  = chk_head.ExecuteReader();
            while (chk_head_rd.Read())
            {
                if (chk_head_rd[0].ToString() == "")
                { MessageBox.Show("В этом отряде командир ещё не назначен"); break; }
                else if (Convert.ToInt32(chk_head_rd["id_head"]) == mainform.in_id)
                {
                    mainform.in_status = "head_sq"; status_head_headq = 0;
                    textBox1.Text = mainform.in_squad;
                    break;
                }
            }
            chk_head_rd.Close();

            //командир штаба 
            id_headq = -1; id_region = -1;
            SqlCommand chk_head_headq = new SqlCommand($"select name, id_head, id_headquarter, id_region_authority from headquarters", rsoConnection);
            SqlDataReader chk_head_headq_rd = chk_head_headq.ExecuteReader();
            while (chk_head_headq_rd.Read())
            {
                if (Convert.ToInt32(chk_head_headq_rd["id_head"]) == mainform.in_id)
                {
                    if (mainform.in_status == "head_sq")
                    { status_head_headq = 2; mainform.in_status = "head_sq_h"; }
                    else { status_head_headq = 1; mainform.in_status = "head_h"; }
                    id_headq = Convert.ToInt32(chk_head_headq_rd["id_headquarter"]);
                    adm.id_headq = id_headq;
                    adm.name_headq = Convert.ToString(chk_head_headq_rd["name"]);
                    id_region = Convert.ToInt32(chk_head_headq_rd["id_region_authority"]);
                    break;
                }
            }
            chk_head_headq_rd.Close();
            if (mainform.in_status != "head_sq" && mainform.in_status != "head_h" && mainform.in_status != "head_sq_h")
            { mainform.in_status = "average"; button5.Visible = false; button7.Visible = true; }

            if (mainform.in_status == "head_sq_h") { button5.Visible = true; button8.Visible = true; button7.Visible = true; }
            else if (mainform.in_status == "head_h") { button5.Visible = false; button8.Visible = true; button7.Visible = true; }
            else if (mainform.in_status == "head_sq") { button5.Visible = true; button8.Visible = false; button7.Visible = true; }
            if (status_head_headq == 1 || status_head_headq == 2) 
            {
                RefreshSquadsToHeadQ();
                SqlCommand inst_to_headq = new SqlCommand($"select i.name from headquarters h, institutions i where i.id_region = @id_i and i.id_region = h.id_region_authority", rsoConnection);
                inst_to_headq.Parameters.AddWithValue("id_i", id_region);
                SqlDataReader inst_to_headq_rd = inst_to_headq.ExecuteReader();
                while (inst_to_headq_rd.Read())
                {
                    if (adm.inst_to_head.Contains(inst_to_headq_rd[0].ToString()) == false)
                        adm.inst_to_head.Add(inst_to_headq_rd[0].ToString());
                }
                inst_to_headq_rd.Close();
                adm.add_institutions_to_head();
                adm.add_names_squads_to_head();
            }

            if (status_head_headq == 0 || status_head_headq == 2)
            {
                //занесение инфы о заказах бойцовок
                listView2.Items.Clear();
                string[] s;
                SqlCommand orders = new SqlCommand("select m.surname, m.name, m.patronymic, m.birthday, m.phone, m.status, u.date_of_order, u.size, u.status from orders_uniform u, members m, squads s where m.id_member = u.id_customer and m.id_squad = s.id_squad and m.id_squad = @id", rsoConnection);
                orders.Parameters.AddWithValue("id", mainform.in_id_squad);
                SqlDataReader orders_rd = orders.ExecuteReader();
                while (orders_rd.Read())
                {
                    s = new string[] {Convert.ToString(orders_rd[0]), Convert.ToString(orders_rd[1]), Convert.ToString(orders_rd[2]),
                    Convert.ToDateTime(orders_rd[3]).ToShortDateString(),Convert.ToString(orders_rd[4]),Convert.ToString(orders_rd[5]),
                    Convert.ToDateTime(orders_rd[6]).ToShortDateString(),Convert.ToString(orders_rd[7]),
                        Convert.ToString(orders_rd[8])};
                    ss.Add(s);
                }
                orders_rd.Close();
                foreach (var i in ss)
                    listView2.Items.Add(new ListViewItem(i));
                RefreshMembersToHeadSquad();
            }

            //заполнение инфы об отряде авторизованного
            rows.Clear(); names_for_changing_to_comotr.Clear();
            RefreshMembersToHeadSquad();
            /*SqlDataReader squad_inner = null;
            string[] row; 
            SqlCommand inf_squad = new SqlCommand($"select surname, name, patronymic, birthday, phone, status from members where id_squad = @id order by surname,name,patronymic", rsoConnection);
            inf_squad.Parameters.AddWithValue("id", mainform.in_id_squad);
            squad_inner = inf_squad.ExecuteReader();
            while (squad_inner.Read())
            {
                
                DateTime dbc = Convert.ToDateTime(squad_inner["Birthday"]);

                row = new string[] { Convert.ToString(squad_inner["surname"]), Convert.ToString(squad_inner["name"]), 
                    Convert.ToString(squad_inner["patronymic"]), dbc.ToShortDateString(), 
                    Convert.ToString(squad_inner["phone"]), Convert.ToString(squad_inner["status"]) };
                rows.Add(row);
                if (mainform.in_status == "head")
                {
                    row = new string[] { Convert.ToString(squad_inner["surname"]) + " " + Convert.ToString(squad_inner["name"])
                        + " " + Convert.ToString(squad_inner["patronymic"])};
                    //names_for_changing_to_comotr.Add(row);
                    comboBox1.Items.AddRange(row);
                }
                //item = new ListViewItem(new string[] {  });
                //listView1.Items.Add(item);
            }
            squad_inner.Close();
            RefreshList(rows);*/
            if (status_head_headq == 0 || status_head_headq == 2)
            {
                string[] row = new string[] { "фамилия", "имя", "отчество", "дата рождения", "телефон", "дата регистрации", "статус" };
                comboBox4.Items.AddRange(row);
            }

            //повторное заполнение
            SqlCommand flo = new SqlCommand($"select d.name as 'dn', h.name as 'hn', i.name as 'in', i.head as 'ih' " +
                $"from Squads s, Members m, Headquarters h, Institutions i, Directions d " +
                $"where s.Id_Squad = @id and s.Id_Head = m.Id_Member and s.Id_Institution = i.Id_Institution " +
                $"and s.Id_Headquarters = h.Id_Headquarter and s.Id_Direction = d.Id_Direction",rsoConnection); //поиск всего без имён
            flo.Parameters.AddWithValue("id", mainform.in_id_squad);
            SqlDataReader flo_rd = null;
            flo_rd = flo.ExecuteReader();
            while (flo_rd.Read())
            {
                textBox9.Text = Convert.ToString(flo_rd["dn"]);
                textBox11.Text = Convert.ToString(flo_rd["hn"]);
                textBox13.Text = Convert.ToString(flo_rd["in"]);
                textBox14.Text = Convert.ToString(flo_rd["ih"]);
            }
            flo_rd.Close();

            //заполнение оставшихся имён
            SqlCommand head_sq = new SqlCommand($"select m.surname, m.name, m.phone from squads s, members m where s.id_squad = @id and s.id_head = m.id_member", rsoConnection);
            SqlCommand vice_sq = new SqlCommand($"select m.surname, m.name from squads s, members m where s.id_squad = @id and s.id_vice = m.id_member", rsoConnection);
            SqlCommand head_hq = new SqlCommand($"select m.surname, m.name, m.phone from squads s, headquarters h, members m where s.id_squad = @id and s.id_headquarters = h.id_headquarter and h.id_head = m.id_member", rsoConnection);

            head_sq.Parameters.AddWithValue("id", mainform.in_id_squad);
            vice_sq.Parameters.AddWithValue("id", mainform.in_id_squad);
            head_hq.Parameters.AddWithValue("id", mainform.in_id_squad);

            SqlDataReader head_sq_rd;
            head_sq_rd = head_sq.ExecuteReader();
            while (head_sq_rd.Read()) { textBox10.Text = Convert.ToString(head_sq_rd["surname"]);
                textBox10.Text += " " + Convert.ToString(head_sq_rd["name"]);
                textBox15.Text = Convert.ToString(head_sq_rd["phone"]);
            }
            head_sq_rd.Close();
            SqlDataReader vice_sq_rd;
            vice_sq_rd = vice_sq.ExecuteReader();
            while (vice_sq_rd.Read()) { textBox8.Text = Convert.ToString(vice_sq_rd["surname"]); 
                textBox8.Text += " " + Convert.ToString(vice_sq_rd["name"]);
            }
            vice_sq_rd.Close();
            SqlDataReader head_hq_rd;
            head_hq_rd = head_hq.ExecuteReader();
            while (head_hq_rd.Read()) { textBox12.Text = Convert.ToString(head_hq_rd["surname"]);
                textBox12.Text += " " + Convert.ToString(head_hq_rd["name"]);
                textBox16.Text = Convert.ToString(head_hq_rd["phone"]);
            }
            head_hq_rd.Close();
            dateTimePicker1.MinDate = DateTime.Today.AddYears(-100);
            dateTimePicker1.MaxDate = DateTime.Today.AddYears(-14);
            dateTimePicker2.MinDate = new DateTime(2004, 01, 01);
            dateTimePicker2.MaxDate = DateTime.Today;
            dateTimePicker1.Value = new DateTime(DateTime.Today.AddYears(-20).Year, 01, 01);
            dateTimePicker2.Value = DateTime.Today;
            SqlCommand dr_sq = new SqlCommand("select date_registration from squads where name = @name_sq", rsoConnection);
            dr_sq.Parameters.AddWithValue("name_sq", textBox1.Text);
            SqlDataReader dr_sq_read = dr_sq.ExecuteReader();
            while (dr_sq_read.Read())
            {
                dateTimePicker2.MinDate = Convert.ToDateTime(dr_sq_read[0]).Date; 
            }
            dr_sq_read.Close();
        }

        public void RefreshSquadsToHeadQ()
        {
            adm.name_sqd_with_abr.Clear(); adm.name_sqd.Clear();
            string squads_to_headq;
            int id_sqd_to_headq = -1;
            SqlCommand squads_in_headq = new SqlCommand($"select s.name, d.name, i.name, s.id_squad from headquarters h, squads s, directions d, institutions i where s.id_headquarters = @id_h and i.id_region = @id_i and s.Id_Direction = d.Id_Direction and s.id_institution = i.id_institution and i.id_region = h.id_region_authority", rsoConnection);
            squads_in_headq.Parameters.AddWithValue("id_h", id_headq);
            squads_in_headq.Parameters.AddWithValue("id_i", id_region);
            SqlDataReader squads_in_headq_rd = squads_in_headq.ExecuteReader();
            while (squads_in_headq_rd.Read())
            {
                squads_to_headq = Convert.ToString(squads_in_headq_rd[1]) + " '" + Convert.ToString(squads_in_headq_rd[0]) +
                    "', " + Convert.ToString(squads_in_headq_rd[2]);
                if (adm.name_sqd.Contains(Convert.ToString(squads_in_headq_rd[0])) == false)
                    adm.name_sqd.Add(Convert.ToString(squads_in_headq_rd[0]));
                if (adm.name_sqd_with_abr.Contains(squads_to_headq) == false)
                    adm.name_sqd_with_abr.Add(squads_to_headq);
                id_sqd_to_headq = Convert.ToInt32(squads_in_headq_rd[3]);
            }
            squads_in_headq_rd.Close();
            adm.add_names_squads_to_head();
        }

        private void RefreshList(List<string[]> list)
        {
            listView1.Items.Clear();
            foreach(string[] s in list)
            {
                listView1.Items.Add(new ListViewItem(s));
            }
        }

        public void RefreshMembersToHeadSquad()
        {
            rows.Clear(); names_for_changing_to_comotr.Clear(); comboBox1.Items.Clear();
            string[] row;
            SqlCommand inf_squad = new SqlCommand($"select surname, name, patronymic, birthday, phone, status from members where id_squad = @id order by surname,name,patronymic", rsoConnection);
            inf_squad.Parameters.AddWithValue("id", mainform.in_id_squad);
            SqlDataReader squad_inner = inf_squad.ExecuteReader();
            while (squad_inner.Read())
            {
                DateTime dbc = Convert.ToDateTime(squad_inner["Birthday"]);

                row = new string[] { Convert.ToString(squad_inner[0]), Convert.ToString(squad_inner[1]),
                    Convert.ToString(squad_inner[2]), Convert.ToDateTime(squad_inner[3]).ToShortDateString(),
                    Convert.ToString(squad_inner[4]), Convert.ToString(squad_inner[5]) };
                rows.Add(row);
                row = new string[] { Convert.ToString(squad_inner[0])+" "+Convert.ToString(squad_inner[1])+" "+
                    Convert.ToString(squad_inner[2])+" "+Convert.ToDateTime(squad_inner[3]).ToShortDateString()+" "+
                    Convert.ToString(squad_inner[4])+" "+Convert.ToString(squad_inner[5]) };
                comboBox1.Items.AddRange(row);
            }
            squad_inner.Close();
            RefreshList(rows);
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
                        //MessageBox.Show(result);
                        if (result != "")
                        {
                            stat_find = 1;
                            dr = MessageBox.Show("Боец с такими данными, но другим \nномером телефона уже зарегистрирован.\n\nПроверьте введённые данные, и если всё верно \nи нужно внести бойца с такими данными,\nнажмите кнопку 'Да'. Если хотите исправить данные,\nнажмите 'Нет'.", "Ошибка внесения", MessageBoxButtons.YesNo);
                            break;
                        }
                    }
                    check_available1_read.Close();
                    if (dr == DialogResult.Yes || dr==DialogResult.Cancel ||  stat_find != 1)
                    {/*
                        DialogResult dr = MessageBox.Show("Боец с такими данными, но другим \nномером телефона уже зарегистрирован.\n\nПроверьте введённые данные, и если всё верно \nи нужно внести бойца с такими данными,\nнажмите кнопку 'Да'. Если хотите исправить данные,\nнажмите 'Нет'.", "Ошибка внесения", MessageBoxButtons.YesNo);
                        
                    }
                    else if (st < 1)
                    {*/
                        SqlCommand id_sq = new SqlCommand($"select Id_Squad from Squads where Name = @name", rsoConnection);
                        id_sq.Parameters.AddWithValue("Name", textBox1.Text);
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
                        RefreshMembersToHeadSquad();
                    }
                }
            }
        }

        private void textbox2_TextChanged(object sender, EventArgs e)
        {
            if(comboBox3.SelectedIndex == 0)
            {
                filteredList = rows.Where((item) =>
                    item[0].ToLower().Contains(textBox2.Text.ToLower())).ToList();
                RefreshList(filteredList);
            }
            else if (comboBox3.SelectedIndex == 1)
            {
                filteredList = rows.Where((item) =>
                    item[1].ToLower().Contains(textBox2.Text.ToLower())).ToList();
                RefreshList(filteredList);
            }
            else if (comboBox3.SelectedIndex == 2)
            {
                filteredList = rows.Where((item) =>
                    item[2].ToLower().Contains(textBox2.Text.ToLower())).ToList();
                RefreshList(filteredList);
            }
            else if (comboBox3.SelectedIndex == 3)
            {
                filteredList = rows.Where((item) =>
                    item[3].ToLower().Contains(textBox2.Text.ToLower())).ToList();
                RefreshList(filteredList);
            }
            else if (comboBox3.SelectedIndex == 4)
            {
                filteredList = rows.Where((item) =>
                    item[4].ToLower().Contains(textBox2.Text.ToLower())).ToList();
                RefreshList(filteredList);
            }
            else if (comboBox3.SelectedIndex == 5)
            {
                filteredList = rows.Where((item) =>
                    item[5].ToLower().Contains(textBox2.Text.ToLower())).ToList();
                RefreshList(filteredList);
            }
            else { RefreshList(rows); }
            
        }

        private void button2_Click(object sender, EventArgs e) //form closing
        {
            this.Close();
            mainform.Show();
            textBox13.Text = ""; textBox14.Text = ""; textBox2.Text = ""; button5.Visible = false; button7.Visible = false;
            listView1.Items.Clear(); comboBox3.SelectedIndex = -1;  button8.Visible = false; status_head_headq = -1;
            groupBox1.Visible = true; groupBox2.Visible = false; label22.Visible = true; comboBox3.Visible = true; 
            button5.Visible = true; tabControl1.Visible = false; mainform.in_status = "";
            comboBox1.Items.Clear(); //comboBox3.Items.Clear(); 
            adm.clear_names_squads_to_head(); adm.inst_to_head.Clear();  adm.name_sqd_with_abr.Clear();
            button11.Visible = false; button10.Visible = true; label22.Visible = false; comboBox3.Visible = false; textBox2.Visible = false;
        }

        string in_ph = ""; string log = ""; string pas = "";

        private void button3_Click(object sender, EventArgs e)
        {
            maskedTextBox5.ReadOnly = false; in_ph = maskedTextBox5.Text;
            button3.Visible = false; button4.Visible = true; button12.Visible = true;
            label7.Visible = false; label6.Visible = false; label23.Visible = false; label24.Visible = false; label8.Visible = false;
            label9.Visible = false; label10.Visible = false; label12.Visible = false; label16.Visible = false; label17.Visible = false;
            label18.Visible = false; label13.Visible = false; label14.Visible = false; label19.Visible = false; label11.Visible = false;
            label15.Visible = false; textBox3.Visible = false; textBox4.Visible = false; textBox17.Visible = false;
            textBox5.Visible = false; textBox6.Visible = false; textBox7.Visible = false; textBox9.Visible = false; textBox10.Visible = false;
            textBox15.Visible = false; textBox8.Visible = false; textBox11.Visible = false; textBox12.Visible = false; textBox16.Visible = false;
            textBox13.Visible = false; textBox14.Visible = false;
            maskedTextBox8.Visible = true; maskedTextBox9.Visible = true; maskedTextBox10.Visible = true;
            label30.Visible = true; label31.Visible = true; label32.Visible = true;
            SqlCommand lp_in = new SqlCommand($"select username,password from lp where id_member = @id", rsoConnection);
            lp_in.Parameters.AddWithValue("id", mainform.in_id);
            SqlDataReader lp_in_rd = lp_in.ExecuteReader();
            while (lp_in_rd.Read())
            {
                log = Convert.ToString(lp_in_rd[0]);
                pas = Convert.ToString(lp_in_rd[1]);
            }
            lp_in_rd.Close();
            maskedTextBox8.Text = log;
            maskedTextBox9.Text = pas;
            maskedTextBox10.Text = pas;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int stat_chek = -1; int upd_p = -1;
            if (in_ph == maskedTextBox5.Text && maskedTextBox8.Text == log && maskedTextBox9.Text == pas && maskedTextBox10.Text==pas)
            { MessageBox.Show("Разве что-то изменилось?..", "Изменения не найдены"); stat_chek = 1; }
            else if (in_ph != maskedTextBox5.Text && maskedTextBox5.Text.Count() != 17)
            { MessageBox.Show("Кажется, не хватает цифр...", "Ошибка ввода"); stat_chek = 1; }
            else if (maskedTextBox5.Text[4].ToString() == " " || maskedTextBox5.Text[5].ToString() == " " ||
                maskedTextBox5.Text[6].ToString() == " " || maskedTextBox5.Text[9].ToString() == " " ||
                maskedTextBox5.Text[10].ToString() == " " || maskedTextBox5.Text[11].ToString() == " " ||
                maskedTextBox5.Text[13].ToString() == " " || maskedTextBox5.Text[14].ToString() == " " ||
                maskedTextBox5.Text[15].ToString() == " " || maskedTextBox5.Text[16].ToString() == " ")
            { MessageBox.Show("В телефоне не может быть пробела!", "Ошибка ввода"); stat_chek = 1; }
            else
            {
                if (maskedTextBox5.Text != in_ph)
                {
                    SqlCommand upd_ph = new SqlCommand($"update members set Phone = @phn where Id_Member = @id", rsoConnection);
                    upd_ph.Parameters.AddWithValue("phn", maskedTextBox5.Text.ToString());
                    upd_ph.Parameters.AddWithValue("id", mainform.in_id);
                    MessageBox.Show("Обновлено записей в базе: " + upd_ph.ExecuteNonQuery().ToString() + ". Номер телефона успешно изменён на " + maskedTextBox5.Text, "Успешное обновление");
                    upd_p = 1;
                }
            }
            if (maskedTextBox8.Text != log && maskedTextBox8.Text != "" && maskedTextBox8.Text != " ")
            {
                SqlCommand check_username = new SqlCommand("select username from lp where username = @un", rsoConnection);
                check_username.Parameters.AddWithValue("un", maskedTextBox8.Text);
                SqlDataReader check_read = check_username.ExecuteReader();
                while (check_read.Read())
                {
                    if (maskedTextBox8.Text.ToString() == Convert.ToString(check_read["username"]))
                    { MessageBox.Show("Этот логин уже занят! Придумайте другой.", "Вы что, самозванец?.."); stat_chek = 1; break; }
                }
                check_read.Close();
            }
            if (maskedTextBox9.Text != maskedTextBox10.Text)
            { MessageBox.Show("Пароли не совпадают!"); stat_chek = 1; }
            if (stat_chek != 1)
            {
                if (maskedTextBox8.Text != log) 
                { 
                    SqlCommand upd_lp = new SqlCommand($"update lp set username = @usrn where id_member = @id", rsoConnection);
                    upd_lp.Parameters.AddWithValue("usrn", maskedTextBox8.Text);
                    upd_lp.Parameters.AddWithValue("id", mainform.in_id);
                    MessageBox.Show("Обновлен " + upd_lp.ExecuteNonQuery().ToString() + " логин на '" + maskedTextBox8.Text + "'.", "Успешное обновление");
                }
                else { MessageBox.Show("Логин без имененений"); }
                if (maskedTextBox9.Text != pas)
                {
                    SqlCommand upd_lp1 = new SqlCommand($"update lp set password = @psw where id_member = @id", rsoConnection);
                    upd_lp1.Parameters.AddWithValue("psw", maskedTextBox9.Text);
                    upd_lp1.Parameters.AddWithValue("id", mainform.in_id);
                    MessageBox.Show("Обновлен " + upd_lp1.ExecuteNonQuery().ToString() + " пароль для аккаунта '" + maskedTextBox8.Text + "' \nна '" + maskedTextBox9.Text + "'. Не забудьте его!", "Успешное обновление");
                }
                else { MessageBox.Show("Пароль без имененений"); }
                if (upd_p != 1 && maskedTextBox5.Text != in_ph)
                {
                    SqlCommand upd_ph = new SqlCommand($"update members set Phone = @phn where Id_Member = @id", rsoConnection);
                    upd_ph.Parameters.AddWithValue("phn", maskedTextBox5.Text.ToString());
                    upd_ph.Parameters.AddWithValue("id", mainform.in_id);
                    MessageBox.Show("Обновлено записей в базе: " + upd_ph.ExecuteNonQuery().ToString() + ". Номер телефона успешно изменён на " + maskedTextBox5.Text, "Успешное обновление");
                }
                else if(upd_p==-1){ MessageBox.Show("Телефон без имененений"); }
                maskedTextBox5.ReadOnly = true; button3.Visible = true; button4.Visible = false; button12.Visible = false;
                    label7.Visible = true; label6.Visible = true; label23.Visible = true; label24.Visible = true; label8.Visible = true;
                    label9.Visible = true; label10.Visible = true; label12.Visible = true; label16.Visible = true; label17.Visible = true;
                    label18.Visible = true; label13.Visible = true; label14.Visible = true; label19.Visible = true; label11.Visible = true;
                    label15.Visible = true; textBox3.Visible = true; textBox4.Visible = true; textBox17.Visible = true;
                    textBox5.Visible = true; textBox6.Visible = true; textBox7.Visible = true; textBox9.Visible = true; textBox10.Visible = true;
                    textBox15.Visible = true; textBox8.Visible = true; textBox11.Visible = true; textBox12.Visible = true; textBox16.Visible = true;
                    textBox13.Visible = true; textBox14.Visible = true;
                    maskedTextBox8.Visible = false; maskedTextBox9.Visible = false; maskedTextBox10.Visible = false;
                    label30.Visible = false; label31.Visible = false; label32.Visible = false;
                }
            //}
        }

        private void button5_Click(object sender, EventArgs e)
        {
            tabControl1.Visible = true; button6.Visible = true; button7.Visible = false;
            groupBox1.Visible = false; groupBox2.Visible = false;
            button5.Visible = false; button9.Enabled = false;
            label22.Visible = false; comboBox3.Visible = false; textBox2.Visible = false;
            if (status_head_headq == 1)
                button8.Visible = false; 
            if(status_head_headq==0)
                button10.Visible = false;
            if(status_head_headq==2)
            { button8.Visible = false; button10.Visible = false; }
            //comboBox1.Items.Add(names_for_changing_to_comotr);
            //добавление информации о людях из отряда.начинаем с фамилии
            /*SqlCommand add_surnm = new SqlCommand($"select surname from members where Id_Squad = @id", rsoConnection);
            add_surnm.Parameters.AddWithValue("id", mainform.in_id_squad);
            SqlDataReader add_surnm_read = add_surnm.ExecuteReader();
            int i = 0; 
            while (add_surnm_read.Read())
            {
                names[i] = Convert.ToString(add_surnm_read["surname"]); i++;
            }
            add_surnm_read.Close();
            //теперь имя
            SqlCommand add_name = new SqlCommand($"select name from members where Id_Squad = @id", rsoConnection);
            add_surnm.Parameters.AddWithValue("id", mainform.in_id_squad);
            SqlDataReader add_name_read = add_name.ExecuteReader();
            i = 0;
            while (add_name_read.Read())
            {
                names[i] += Convert.ToString(add_surnm_read["surname"]); i++;
            }
            add_surnm_read.Close();*/


        }

        private void button6_Click(object sender, EventArgs e)
        {
            tabControl1.Visible = false; button6.Visible = false; groupBox1.Visible = true; button5.Visible = true;
            comboBox1.SelectedIndex = -1; comboBox4.SelectedIndex = -1; maskedTextBox6.Clear(); maskedTextBox7.Clear();
            tabControl1.SelectedIndex = 0; maskedTextBox1.Clear();maskedTextBox2.Clear();maskedTextBox3.Clear();maskedTextBox4.Clear();
            comboBox6.Visible = false; comboBox5.SelectedIndex = -1; button7.Visible = true;
            if (status_head_headq == 1)
                button8.Visible = true; 
            if(status_head_headq==0)
                button10.Visible = true;
            if(status_head_headq==2)
            { button8.Visible = true;button10.Visible = true; }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = false; groupBox2.Visible = false; groupBox3.Visible = true;
            button7.Visible = false; button14.Visible = true; button10.Visible = false;
            if (status_head_headq == 0)
                button5.Visible = false;
            if (status_head_headq == 1)
                button8.Visible = false;
            if(status_head_headq==2)
            { button5.Visible = false;button8.Visible = false; }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            adm.mainform = this;
            this.Hide();
            adm.ShowDialog();
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            comboBox4.Enabled = true;
        }

        private void comboBox4_SelectionChangeCommitted(object sender, EventArgs e)
        {
            maskedTextBox6.Mask = "AAAAAAAAAA";
            string checked_string = null; string chel = null;
            maskedTextBox6.Enabled = true;  maskedTextBox6.Visible = true;
            maskedTextBox6.BringToFront(); maskedTextBox7.BringToFront();
            comboBox6.BringToFront();
            if (comboBox4.SelectedIndex == 0)
            { checked_string = "surname"; maskedTextBox6.Mask = "???????????????????????"; comboBox6.Visible = false; }
            else if (comboBox4.SelectedIndex == 1)
            { checked_string = "name"; maskedTextBox6.Mask = "???????????????????????"; comboBox6.Visible = false; }
            else if (comboBox4.SelectedIndex == 2)
            { checked_string = "patronymic"; maskedTextBox6.Mask = "???????????????????????"; comboBox6.Visible = false; }
            else if (comboBox4.SelectedIndex == 3)
            { checked_string = "birthday"; maskedTextBox6.Mask = ""; comboBox6.Visible = false; }
            else if (comboBox4.SelectedIndex == 4)
            { checked_string = "phone"; maskedTextBox6.Mask = "+7 (000) 000-0000"; comboBox6.Visible = false; }
            else if (comboBox4.SelectedIndex == 5)
            { checked_string = "registration"; maskedTextBox6.Mask = ""; comboBox6.Visible = false; }
            else if (comboBox4.SelectedIndex == 6)
            { checked_string = "status"; comboBox6.Visible = true; maskedTextBox6.Visible = false; }
            for (int i = 0; i < comboBox1.Text.Count(); i++)
            {
                if (comboBox1.Text[i].ToString() != " ")
                    chel += comboBox1.Text[i];
                else break;
            }
            
            SqlCommand find_old = new SqlCommand($"select * from members where surname = @text", rsoConnection);
            find_old.Parameters.AddWithValue("text", chel);
            SqlDataReader find_old_rd = null;
            find_old_rd = find_old.ExecuteReader();
            while (find_old_rd.Read())
            {
                maskedTextBox7.Text = Convert.ToString(find_old_rd[checked_string]);
            }
            find_old_rd.Close();
            if (checked_string == "birthday" || checked_string == "registration")
            {
                dateTimePicker3.Visible = true; dateTimePicker4.Visible = true;
                dateTimePicker3.Enabled = true; dateTimePicker3.Value = Convert.ToDateTime(maskedTextBox7.Text);
                dateTimePicker4.Value = Convert.ToDateTime(maskedTextBox7.Text);
                maskedTextBox6.Visible = false; maskedTextBox7.Visible = false;
            }
            else
            {
                dateTimePicker3.Visible = false; dateTimePicker4.Visible = false;
                dateTimePicker3.Enabled = false;
                maskedTextBox6.Visible = true; maskedTextBox7.Visible = true;
                maskedTextBox6.Clear();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (comboBox6.SelectedIndex==-1 && maskedTextBox6.Text == "")
                MessageBox.Show("Заполните все поля!", "Ошибка в заполнении полей");
            else 
            {
                string chel = null;
                for (int i = 0; i < comboBox1.Text.Count(); i++)
                {
                    if (comboBox1.Text[i].ToString() != " ")
                        chel += comboBox1.Text[i];
                    else break;
                }
                if (comboBox4.SelectedIndex == 0)
                {
                    if (maskedTextBox6.Text.Contains(" ")) { MessageBox.Show("В фамилии не может быть пробела!", "Ошибка в заполнении полей"); }
                    else
                    {
                        SqlCommand add_new_inf = new SqlCommand($"update members set surname = @text_change where surname = @text", rsoConnection);
                        add_new_inf.Parameters.AddWithValue("text", chel);
                        add_new_inf.Parameters.AddWithValue("text_change", maskedTextBox6.Text);
                        MessageBox.Show("Обновлено записей: " + add_new_inf.ExecuteNonQuery().ToString() + ".\nВы изменили фамилию у выбранного члена на '" + maskedTextBox6.Text + "'.");
                        RefreshMembersToHeadSquad();
                    }
                }
                else if (comboBox4.SelectedIndex == 1)
                {
                    if (maskedTextBox2.Text.Contains(" ")) { MessageBox.Show("В имени не может быть пробела!", "Ошибка в заполнении полей"); }
                    else
                    {
                        SqlCommand add_new_inf = new SqlCommand($"update members set name = @text_change where surname = @text", rsoConnection);
                        add_new_inf.Parameters.AddWithValue("text", chel);
                        add_new_inf.Parameters.AddWithValue("text_change", maskedTextBox6.Text);
                        MessageBox.Show("Обновлено записей: " + add_new_inf.ExecuteNonQuery().ToString() + ".\nВы изменили имя у выбранного члена на '" + maskedTextBox6.Text + "'.");
                        RefreshMembersToHeadSquad();
                    }
                }
                else if (comboBox4.SelectedIndex == 2)
                {
                    SqlCommand add_new_inf = new SqlCommand($"update members set patronymic = @text_change where surname = @text", rsoConnection);
                    add_new_inf.Parameters.AddWithValue("text", chel);
                    add_new_inf.Parameters.AddWithValue("text_change", maskedTextBox6.Text);
                    MessageBox.Show("Обновлено записей: " + add_new_inf.ExecuteNonQuery().ToString() + ".\nВы изменили отчество у выбранного члена на '" + maskedTextBox6.Text + "'.");
                    RefreshMembersToHeadSquad();
                }
                else if (comboBox4.SelectedIndex == 3)
                {
                    SqlCommand add_new_inf = new SqlCommand($"update members set birthday = @text_change where surname = @text", rsoConnection);
                    add_new_inf.Parameters.AddWithValue("text", chel);
                    add_new_inf.Parameters.AddWithValue("text_change", dateTimePicker3.Value.Date);
                    MessageBox.Show("Обновлено записей: " + add_new_inf.ExecuteNonQuery().ToString() + ".\nВы изменили дату рождения у выбранного члена на '" + maskedTextBox6.Text + "'.");
                    RefreshMembersToHeadSquad();
                }
                else if (comboBox4.SelectedIndex == 4)
                {
                    if (maskedTextBox6.Text[4].ToString() == " " || maskedTextBox6.Text[5].ToString() == " " ||
                    maskedTextBox6.Text[6].ToString() == " " || maskedTextBox6.Text[9].ToString() == " " ||
                    maskedTextBox6.Text[10].ToString() == " " || maskedTextBox6.Text[11].ToString() == " " ||
                    maskedTextBox6.Text[13].ToString() == " " || maskedTextBox6.Text[14].ToString() == " " ||
                    maskedTextBox6.Text[15].ToString() == " " || maskedTextBox6.Text[16].ToString() == " ")
                    { MessageBox.Show("В телефоне не может быть пробела!"); }
                    else if (maskedTextBox6.Text[4].ToString() != "9") { MessageBox.Show("Телефон должен начинаться с '+79...' ", "Ошибка в заполнении полей"); }
                    else if (maskedTextBox6.Text.Count() != 17) { MessageBox.Show("Введите телефон полностью!", "Ошибка в заполнении полей"); }
                    else
                    {
                        SqlCommand add_new_inf = new SqlCommand($"update members set phone = @text_change where surname = @text", rsoConnection);
                        add_new_inf.Parameters.AddWithValue("text", chel);
                        add_new_inf.Parameters.AddWithValue("text_change", maskedTextBox6.Text);
                        MessageBox.Show("Обновлено записей: " + add_new_inf.ExecuteNonQuery().ToString() + ".\nВы изменили телефон у выбранного члена на '" + maskedTextBox6.Text + "'.");
                        RefreshMembersToHeadSquad();
                    }
                }
                else if (comboBox4.SelectedIndex == 5)
                {
                    SqlCommand add_new_inf = new SqlCommand($"update members set registration = @text_change where surname = @text", rsoConnection);
                    add_new_inf.Parameters.AddWithValue("text", chel);
                    add_new_inf.Parameters.AddWithValue("text_change", dateTimePicker3.Value.Date);
                    MessageBox.Show("Обновлено записей: " + add_new_inf.ExecuteNonQuery().ToString() + ".\nВы изменили дату регистрации у выбранного члена на '" + maskedTextBox6.Text + "'.");
                    RefreshMembersToHeadSquad();
                }
                else if (comboBox4.SelectedIndex == 6)
                {
                    SqlCommand add_new_inf = new SqlCommand($"update members set status = @text_change where surname = @text", rsoConnection);
                    add_new_inf.Parameters.AddWithValue("text", chel);
                    add_new_inf.Parameters.AddWithValue("text_change", comboBox6.Text);
                    MessageBox.Show("Обновлено записей: " + add_new_inf.ExecuteNonQuery().ToString() + ".\nВы изменили статус у выбранного члена на '" + comboBox6.Text + "'.");
                    RefreshMembersToHeadSquad();
                    comboBox6.SelectedIndex = -1;
                }
            }
        }

        private void dateTimePicker4_ValueChanged(object sender, EventArgs e)
        {
            maskedTextBox7.Text = dateTimePicker4.Value.ToString();
        }

        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            maskedTextBox6.Text = dateTimePicker3.Value.ToString();
        }

        private void maskedTextBox6_MouseClick(object sender, MouseEventArgs e)
        {
            if (maskedTextBox6.Text == "")
                ((MaskedTextBox)sender).SelectionStart = 0;
            else if (maskedTextBox6.SelectionStart > maskedTextBox6.Text.Count())
                    maskedTextBox6.SelectionStart = maskedTextBox6.Text.Count(); 
        }

        private void maskedTextBox6_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox6.Text != "" || comboBox6.SelectedIndex!=-1) 
                button9.Enabled = true;
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

        private void maskedTextBox2_TextChanged(object sender, EventArgs e)
        {
            if(maskedTextBox2.Text == "")
            {
                maskedTextBox3.Enabled = false; maskedTextBox4.Enabled = false;
                maskedTextBox1.Enabled = false; dateTimePicker1.Enabled = false; 
                dateTimePicker2.Enabled = false; button1.Enabled = false;
            }
            else 
                maskedTextBox3.Enabled = true;
        }

        private void maskedTextBox3_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox3.Text == "")
            {
                maskedTextBox4.Enabled = false;
                maskedTextBox1.Enabled = false; dateTimePicker1.Enabled = false;
                dateTimePicker2.Enabled = false; button1.Enabled = false;
            }
            else
                maskedTextBox4.Enabled = true;

        }

        private void maskedTextBox4_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox4.Text == "")
            {
                maskedTextBox1.Enabled = false; dateTimePicker1.Enabled = false;
                dateTimePicker2.Enabled = false; button1.Enabled = false;
            }
            else
                maskedTextBox1.Enabled = true;
        }

        private void maskedTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBox1.Text.Count() == 17)
            {
                dateTimePicker1.Enabled = true;
                dateTimePicker2.Enabled = true;
                button1.Enabled = true;
            }
            else
            {
                dateTimePicker1.Enabled = false;
                dateTimePicker2.Enabled = false;
                button1.Enabled = false;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            groupBox2.Visible = true; label22.Visible = true; comboBox3.Visible = true; textBox2.Visible = true; 
            groupBox1.Visible = false; button10.Visible = false; button11.Visible = true; button7.Visible = false;
            if (status_head_headq == 0)
                button5.Visible = false;
            if(status_head_headq == 1) 
                button8.Visible = false;
            if(status_head_headq==2)
            { button5.Visible = false;button8.Visible = false; }    
        }

        private void button11_Click(object sender, EventArgs e)
        {
            groupBox2.Visible = false; label22.Visible = false; comboBox3.Visible = false; textBox2.Visible = false; 
            groupBox1.Visible = true; button11.Visible = false; button10.Visible = true; button7.Visible = true;
            if(status_head_headq == 0)
                button5.Visible = true;
            if (status_head_headq == 1)
                button8.Visible = true;
            if(status_head_headq==2)
            { button5.Visible = true;button8.Visible = true; }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            button12.Visible = false; button4.Visible = false; button3.Visible = true; maskedTextBox5.ReadOnly = true;
            label7.Visible = true; label6.Visible = true; label23.Visible = true; label24.Visible = true; label8.Visible = true;
            label9.Visible = true; label10.Visible = true; label12.Visible = true; label16.Visible = true; label17.Visible = true;
            label18.Visible = true; label13.Visible = true; label14.Visible = true; label19.Visible = true; label11.Visible = true;
            label15.Visible = true; textBox3.Visible = true; textBox4.Visible = true; textBox17.Visible = true;
            textBox5.Visible = true; textBox6.Visible = true; textBox7.Visible = true; textBox9.Visible = true; textBox10.Visible = true;
            textBox15.Visible = true; textBox8.Visible = true; textBox11.Visible = true; textBox12.Visible = true; textBox16.Visible = true;
            textBox13.Visible = true; textBox14.Visible = true;
            maskedTextBox8.Visible = false; maskedTextBox9.Visible = false; maskedTextBox10.Visible = false;
            label30.Visible = false; label31.Visible = false; label32.Visible = false;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == -1)
                MessageBox.Show("Не выбран размер", "Ошибка заказа");
            else
            {
                DialogResult dr = DialogResult.Cancel;
                SqlCommand check_order = new SqlCommand("select status,date_of_order,size from orders_uniform where id_customer=@id_c", rsoConnection);
                check_order.Parameters.AddWithValue("id_c", mainform.in_id);
                SqlDataReader check_order_rd = check_order.ExecuteReader();
                while (check_order_rd.Read())
                {
                    if (check_order_rd[0].ToString() == label34.Text)
                    { dr = MessageBox.Show("Твой предыдущий заказ от " + Convert.ToDateTime(check_order_rd[1]).ToShortDateString() + " на бойцовку размера " + check_order_rd[2] + "\nещё не обработан командиром. Не переживай, про твой заказ никто не забудет!\nКак только он будет готов, твой командир свяжется с тобой.", "Невозможно заказать", MessageBoxButtons.OK); break; }
                }
                check_order_rd.Close();
                if (dr != DialogResult.OK && dr == DialogResult.Cancel)
                {
                    SqlCommand add_order = new SqlCommand(
                                $"insert into orders_uniform " +
                                $"(id_customer, date_of_order, size, status) " +
                                $"values " +
                                $"(@id_c, @dt, @size, @status)", rsoConnection);
                    add_order.Parameters.AddWithValue("id_c", mainform.in_id);
                    add_order.Parameters.AddWithValue("dt", new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day));
                    add_order.Parameters.AddWithValue("size", Convert.ToInt32(comboBox2.Text));
                    add_order.Parameters.AddWithValue("status", label34.Text.ToString());
                    MessageBox.Show(add_order.ExecuteNonQuery().ToString() + " заказ успешно размещён! Твой командир обязательно закажет\nтебе бойцовку и уведомит тебя о дате, времени \nи месте получения. ");
                    comboBox2.SelectedIndex = -1; groupBox1.Visible = true; groupBox3.Visible = false; button14.Visible = false; button7.Visible = true;
                }
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            comboBox2.SelectedIndex = -1; button10.Visible = true;
            groupBox1.Visible = true; groupBox3.Visible = false;
            button7.Visible = true; button14.Visible = false;
            if (status_head_headq == 0)
                button5.Visible = true;
            if (status_head_headq == 1)
                button8.Visible = true;
            if(status_head_headq==2)
            { button5.Visible = true;button8.Visible = true; }
        }
    }
}
