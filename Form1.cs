using System;
using System.Data;
using System.Data.OracleClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace SendEmail2018
{
    public partial class Form1 : Form
    {
        OracleConnection ConnectionToOracle;
        private string connectionString; //Рядок підключення до БД
        public string time_1; //Даний час
        public string time_2; //Час який вводиться в програмі
        public string file; //sql файл
        public string file_sql;
        public string file_zagol;
        string file_name; //назва sql файлу
        private string MessTo; //на кого відправляти
        private string MessCopy; //кого в копію
        public string date;
        public string result_query;
        //Файл заголовку
        public string zagol_file_url;
        public string zagol_file;
        public string[] zagol_arr;
        public string[] array_querry;
        public string folder;
        public string zvit_file_name;
        public string[] settings;
        public string path_zvitu = "";
        public string OutLookLogin = "";
        public string OutLookPassword = "";

        public string Header = "";
        public string MyBody = "";
        public string Futer = "";

        public string ret_Header;
        public string ret_MyBody;
        public string ret_Futer;
        
        public string[] zagol_DataGrid;
        public DataTable table = new DataTable();

        //Перемикач для заповненняполя "Хто отримує" - 1, "Кого ставити в копію" - 2
        public int whoSend = 0;

        public string my_string = "";

        public Form1()
        {
            InitializeComponent();
            //Рядок підключення до БД
            connectionString = "Data Source=;Persist Security Info=True;User ID=;Password=;Unicode=True";
        }

        private void button1_Click(object sender, EventArgs e)
        {
          
        }

        private void Form1_Load(object sender, EventArgs e)
        {        
           
        }

        public void TimeForWork()
        {
            int time_ot = Convert.ToInt32(textBox4.Text);
            int time_do = Convert.ToInt32(textBox5.Text);
            int interval = Convert.ToInt32(textBox8.Text);
            int times = (time_do - time_ot);
            int p = time_ot;
            int count_interval = (time_do - time_ot) * 60 / interval;
            for (int i = 0; i < times; i++)
            {
                int t = 00;
                for (int j = 0; j < 60 / interval; j++)
                {
                    if (t == 0)
                    {
                        richTextBox1.Text += p + ":" + t + "0" + ";\n";
                        t = t + interval;
                    }
                    else if (t < 10)
                    {
                        richTextBox1.Text += p + ":" + "0" + t + ";\n";
                        t = t + interval;
                    }
                    else if(interval == 60)
                    {
                        richTextBox1.Text += p + ":00" + ";\n";
                    }
                    else
                    {
                        richTextBox1.Text += p + ":" + t + ";\n";
                        t = t + interval;
                    }

                }
                p++;
            }
            richTextBox1.Text += time_do + ":00";
        }


        private void button2_Click(object sender, EventArgs e)
        {
          
            if (OutLookLogin == "" && OutLookPassword == "")
            {
                MessageBox.Show("Укажіть дані для підключеня до пошти!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (textBox6.Text == "")
            {
                MessageBox.Show("Не заповнено поля з отримувачами!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (textBox3.Text == "")
            {
                MessageBox.Show("Не заповнено поле з темою повідомлення!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox3.BackColor = Color.Red;
            }
            else if (textBox4.Text == "")
            {
                MessageBox.Show("Не заповнено поле з часом початку виконання!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox4.BackColor = Color.Red;
            }
            else if (textBox5.Text == "")
            {
                MessageBox.Show("Не заповнено поле з часом закінчення виконання!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox5.BackColor = Color.Red;
            }
            else if (textBox8.Text == "")
            {
                MessageBox.Show("Не заповнено поле з часом інтервалом виконання!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox8.BackColor = Color.Red;
            }
            else
            {
                TimeForWork();

                //Зчитування часу та запис його в зміну
                for (int pp = 0; pp < richTextBox1.Lines.Length; pp++)
                {
                    time_2 += richTextBox1.Lines[pp];
                }

                //Якщо програму вже запущено, то повторно 
                if (label6.Text == "Відправку запущено")
                {
                    MessageBox.Show("Відправку вже запущено", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                //В іншому випадку запустити таймер виконання програми
                else
                {

                    int n = 0;
                    timer1.Dispose();
                    timer1.Interval = 60000;
                    timer1.Start();
                    label6.ForeColor = Color.Green;
                    label6.Text = "Відправку запущено";
                }
            }

        }

        private void checkDate()//object obj)
        {               
            string DayWeek = Convert.ToString(DateTime.Now.DayOfWeek);

            //if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday || DateTime.Now.Hour < Convert.ToInt32(textBox4.Text) || DateTime.Now.Hour > Convert.ToInt32(textBox5.Text))
            if (DayWeek != "Saturday" && DayWeek != "Sunday"  && (DateTime.Now.Hour >= Convert.ToInt32(textBox4.Text)) && DateTime.Now.Hour <= Convert.ToInt32(textBox5.Text))
            {
                Programm();
                label6.Text = "Працює";
            }       
            else if((DateTime.Now.Hour < Convert.ToInt32(textBox4.Text)) || DateTime.Now.Hour > Convert.ToInt32(textBox5.Text))
            {
                label6.Text = "Час поза діапазоном";
            }
            else
            {
                label6.Text = "Сьогодні вихідний";   
            }
        }

        private void Programm()
        {

            //------------------------------------------------------------------------------
            //К-ть рядків в файлі        
            int count = System.IO.File.ReadAllLines(@file_sql).Length;
            //Масив для запису запитів
            array_querry = new string[count];

            StreamReader SqlText = new StreamReader(@file_sql);
            for (int p = 0; p < count; p++)
            {
                array_querry[p] = SqlText.ReadLine();
            }
            //------------------------------------------------------------------------------
          
            //Запис поточного часу в зміну, специфіка програми, якщо час 16:05, то його програма записує як 16:5, в цьому випадку додається 0 
            if (DateTime.Now.Minute < 10)
            {
                time_1 = DateTime.Now.Hour + ":0" + DateTime.Now.Minute;
            }
            //В іншому випадку запис без 0 
            else
            {
                time_1 = DateTime.Now.Hour + ":" + DateTime.Now.Minute;
            }

            //Створення текстового масиву через зчитування рядку та розділення його по символу ";"
            string[] split = time_2.Split(new Char[] { ';', '\t' });
            foreach (string s in split)
            {
                if (s.Trim() != "")
                    Console.WriteLine(s);
            }

            for (int m = 0; m < split.Length; m++)
            {
                //Порівняння поточного часу та часу вказаного для виконання програми
                if (time_1 == split[m])
                {

                    File.Delete(@path_zvitu + "\\Temp_CorpLight.html");
                    System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", string.Empty);
                    //Відкриття підключення до Oracle
                    ConnectionToOracle = new OracleConnection(connectionString);
                    ConnectionToOracle.Open();

                    for (int k = 0; k < count; k++)
                    {
                        OracleCommand sqlq = new OracleCommand(array_querry[k], ConnectionToOracle);
                        OracleDataReader readerSql = sqlq.ExecuteReader();

                        //Зміна для запису результату запиту
                        result_query = "";

                        //Зчитування кількості полів в запиті SELECT
                        int i = readerSql.FieldCount;
                        //В залежності від цієї кількості по різному записується результат
                        if (i == 1)
                        {
                            zagol_DataGrid = new string[1];
                            int x = 0;

                            DataTable table = new DataTable();
                            table.Columns.Clear();
                            dataGridView1.DataSource = table;

                            while (readerSql.Read())
                            {
                                if (x == 0)
                                {
                                    zagol_DataGrid[0] = readerSql[0].ToString();
                                    for (int j = 0; j < zagol_DataGrid.Length; j++)
                                    {
                                        table.Columns.Add(zagol_DataGrid[j], typeof(string));
                                    }
                                }
                                else
                                {
                                    table.Rows.Add(readerSql[0].ToString());
                                    result_query += (readerSql[0].ToString() + "\t " + "\n");

                                }
                                x++;
                            }
                            MyBody += CenterHTML(table, k);
                            System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                            MyBody = "";
                        }

                        else if (i == 2)
                        {
                            zagol_DataGrid = new string[2];
                            int x = 0;

                            DataTable table = new DataTable();
                            table.Columns.Clear();
                            dataGridView1.DataSource = table;

                            while (readerSql.Read())
                            {
                                if (x == 0)
                                {
                                    zagol_DataGrid[0] = readerSql[0].ToString();
                                    zagol_DataGrid[1] = readerSql[1].ToString();
                                    for (int j = 0; j < zagol_DataGrid.Length; j++)
                                    {
                                        table.Columns.Add(zagol_DataGrid[j], typeof(string));
                                    }
                                }
                                else
                                {
                                    table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString());
                                    result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + " \n");

                                }
                                x++;
                            }
                            MyBody += CenterHTML(table, k);
                            System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                            MyBody = "";
                        }

                        else if (i == 3)
                        {
                            zagol_DataGrid = new string[3];
                            int x = 0;

                            DataTable table = new DataTable();
                            table.Columns.Clear();
                            dataGridView1.DataSource = table;

                            while (readerSql.Read())
                            {
                                if (x == 0)
                                {
                                    zagol_DataGrid[0] = readerSql[0].ToString();
                                    zagol_DataGrid[1] = readerSql[1].ToString();
                                    zagol_DataGrid[2] = readerSql[2].ToString();
                                    for (int j = 0; j < zagol_DataGrid.Length; j++)
                                    {
                                        table.Columns.Add(zagol_DataGrid[j], typeof(string));
                                    }
                                }
                                else
                                {
                                    table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString());
                                    result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + " \n");

                                }
                                x++;
                            }
                            MyBody += CenterHTML(table, k);
                            System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                            MyBody = "";
                        }

                        else if (i == 4)
                        {
                            zagol_DataGrid = new string[4];
                            int x = 0;

                            DataTable table = new DataTable();
                            table.Columns.Clear();
                            dataGridView1.DataSource = table;

                            while (readerSql.Read())
                            {
                                if (x == 0)
                                {
                                    zagol_DataGrid[0] = readerSql[0].ToString();
                                    zagol_DataGrid[1] = readerSql[1].ToString();
                                    zagol_DataGrid[2] = readerSql[2].ToString();
                                    zagol_DataGrid[3] = readerSql[3].ToString();
                                    for (int j = 0; j < zagol_DataGrid.Length; j++)
                                    {
                                        table.Columns.Add(zagol_DataGrid[j], typeof(string));
                                    }
                                }
                                else
                                {
                                    table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString());
                                    result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + readerSql[3].ToString() + "\t " + " \n");
                                }
                                x++;
                            }
                            MyBody += CenterHTML(table, k);
                            System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                            MyBody = "";
                        }

                        else if (i == 5)
                        {
                            zagol_DataGrid = new string[5];
                            int x = 0;

                            DataTable table = new DataTable();
                            table.Columns.Clear();
                            dataGridView1.DataSource = table;

                            while (readerSql.Read())
                            {
                                if (x == 0)
                                {
                                    zagol_DataGrid[0] = readerSql[0].ToString();
                                    zagol_DataGrid[1] = readerSql[1].ToString();
                                    zagol_DataGrid[2] = readerSql[2].ToString();
                                    zagol_DataGrid[3] = readerSql[3].ToString();
                                    zagol_DataGrid[4] = readerSql[4].ToString();
                                    for (int j = 0; j < zagol_DataGrid.Length; j++)
                                    {
                                        table.Columns.Add(zagol_DataGrid[j], typeof(string));
                                    }
                                }
                                else
                                {
                                    table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString(), readerSql[4].ToString());
                                    result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + readerSql[3].ToString() + "\t " + readerSql[4].ToString() + "\t " + " \n");
                                }
                                x++;
                            }
                            MyBody += CenterHTML(table, k);
                            System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                            MyBody = "";
                        }

                        else if (i == 6)
                        {
                            zagol_DataGrid = new string[6];
                            int x = 0;

                            DataTable table = new DataTable();
                            table.Columns.Clear();
                            dataGridView1.DataSource = table;

                            while (readerSql.Read())
                            {
                                if (x == 0)
                                {
                                    zagol_DataGrid[0] = readerSql[0].ToString();
                                    zagol_DataGrid[1] = readerSql[1].ToString();
                                    zagol_DataGrid[2] = readerSql[2].ToString();
                                    zagol_DataGrid[3] = readerSql[3].ToString();
                                    zagol_DataGrid[4] = readerSql[4].ToString();
                                    zagol_DataGrid[5] = readerSql[5].ToString();
                                    for (int j = 0; j < zagol_DataGrid.Length; j++)
                                    {
                                        table.Columns.Add(zagol_DataGrid[j], typeof(string));
                                    }
                                }
                                else
                                {
                                    table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString(), readerSql[4].ToString(), readerSql[5].ToString());
                                }
                                x++;
                            }
                            MyBody += CenterHTML(table, k);
                            System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                            MyBody = "";
                        }

                        else if (i == 7)
                        {
                            zagol_DataGrid = new string[7];
                            int x = 0;

                            DataTable table = new DataTable();
                            table.Columns.Clear();
                            dataGridView1.DataSource = table;

                            while (readerSql.Read())
                            {
                                if (x == 0)
                                {
                                    zagol_DataGrid[0] = readerSql[0].ToString();
                                    zagol_DataGrid[1] = readerSql[1].ToString();
                                    zagol_DataGrid[2] = readerSql[2].ToString();
                                    zagol_DataGrid[3] = readerSql[3].ToString();
                                    zagol_DataGrid[4] = readerSql[4].ToString();
                                    zagol_DataGrid[5] = readerSql[5].ToString();
                                    zagol_DataGrid[6] = readerSql[6].ToString();
                                    for (int j = 0; j < zagol_DataGrid.Length; j++)
                                    {
                                        table.Columns.Add(zagol_DataGrid[j], typeof(string));
                                    }
                                }
                                else
                                {
                                    table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString(), readerSql[4].ToString(), readerSql[5].ToString(), readerSql[6].ToString());
                                    result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + readerSql[3].ToString() + "\t " + readerSql[4].ToString() + "\t " + readerSql[5].ToString() + "\t " + readerSql[6].ToString() + "\t " + " \n");
                                }
                                x++;
                            }
                            MyBody += CenterHTML(table, k);
                            System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                            MyBody = "";
                        }

                        else if (i == 8)
                        {
                            zagol_DataGrid = new string[8];
                            int x = 0;

                            DataTable table = new DataTable();
                            table.Columns.Clear();
                            dataGridView1.DataSource = table;

                            while (readerSql.Read())
                            {
                                if (x == 0)
                                {
                                    zagol_DataGrid[0] = readerSql[0].ToString();
                                    zagol_DataGrid[1] = readerSql[1].ToString();
                                    zagol_DataGrid[2] = readerSql[2].ToString();
                                    zagol_DataGrid[3] = readerSql[3].ToString();
                                    zagol_DataGrid[4] = readerSql[4].ToString();
                                    zagol_DataGrid[5] = readerSql[5].ToString();
                                    zagol_DataGrid[6] = readerSql[6].ToString();
                                    zagol_DataGrid[7] = readerSql[7].ToString();
                                    for (int j = 0; j < zagol_DataGrid.Length; j++)
                                    {
                                        table.Columns.Add(zagol_DataGrid[j], typeof(string));
                                    }
                                }
                                else
                                {
                                    table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString(), readerSql[4].ToString(), readerSql[5].ToString(), readerSql[6].ToString(), readerSql[7].ToString());
                                    result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + readerSql[3].ToString() + "\t " + readerSql[4].ToString() + "\t " + readerSql[5].ToString() + "\t " + readerSql[6].ToString() + "\t " + readerSql[7].ToString() + "\t " + " \n");
                                }
                                x++;
                            }
                            MyBody += CenterHTML(table, k);
                            System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                            MyBody = "";
                        }

                        else if (i == 9)
                        {
                            zagol_DataGrid = new string[9];
                            int x = 0;

                            DataTable table = new DataTable();
                            table.Columns.Clear();
                            dataGridView1.DataSource = table;

                            while (readerSql.Read())
                            {
                                if (x == 0)
                                {
                                    zagol_DataGrid[0] = readerSql[0].ToString();
                                    zagol_DataGrid[1] = readerSql[1].ToString();
                                    zagol_DataGrid[2] = readerSql[2].ToString();
                                    zagol_DataGrid[3] = readerSql[3].ToString();
                                    zagol_DataGrid[4] = readerSql[4].ToString();
                                    zagol_DataGrid[5] = readerSql[5].ToString();
                                    zagol_DataGrid[6] = readerSql[6].ToString();
                                    zagol_DataGrid[7] = readerSql[7].ToString();
                                    zagol_DataGrid[8] = readerSql[8].ToString();
                                    for (int j = 0; j < zagol_DataGrid.Length; j++)
                                    {
                                        table.Columns.Add(zagol_DataGrid[j], typeof(string));
                                    }
                                }
                                else
                                {
                                    table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString(), readerSql[4].ToString(), readerSql[5].ToString(), readerSql[6].ToString(), readerSql[7].ToString(), readerSql[8].ToString());
                                    result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + readerSql[3].ToString() + "\t " + readerSql[4].ToString() + "\t " + readerSql[5].ToString() + "\t " + readerSql[6].ToString() + "\t " + readerSql[7].ToString() + "\t " + readerSql[8].ToString() + "\t " + " \n");
                                }
                                x++;
                            }
                            MyBody += CenterHTML(table, k);
                            System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                            MyBody = "";
                        }

                        else if (i == 10)
                        {
                            zagol_DataGrid = new string[10];
                            int x = 0;

                            DataTable table = new DataTable();
                            table.Columns.Clear();
                            dataGridView1.DataSource = table;

                            while (readerSql.Read())
                            {
                                if (x == 0)
                                {
                                    zagol_DataGrid[0] = readerSql[0].ToString();
                                    zagol_DataGrid[1] = readerSql[1].ToString();
                                    zagol_DataGrid[2] = readerSql[2].ToString();
                                    zagol_DataGrid[3] = readerSql[3].ToString();
                                    zagol_DataGrid[4] = readerSql[4].ToString();
                                    zagol_DataGrid[5] = readerSql[5].ToString();
                                    zagol_DataGrid[6] = readerSql[6].ToString();
                                    zagol_DataGrid[7] = readerSql[7].ToString();
                                    zagol_DataGrid[8] = readerSql[8].ToString();
                                    zagol_DataGrid[9] = readerSql[9].ToString();
                                    for (int j = 0; j < zagol_DataGrid.Length; j++)
                                    {
                                        table.Columns.Add(zagol_DataGrid[j], typeof(string));
                                    }
                                }
                                else
                                {
                                    table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString(), readerSql[4].ToString(), readerSql[5].ToString(), readerSql[6].ToString(), readerSql[7].ToString(), readerSql[8].ToString(), readerSql[9].ToString());
                                    result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + readerSql[3].ToString() + "\t " + readerSql[4].ToString() + "\t " + readerSql[5].ToString() + "\t " + readerSql[6].ToString() + "\t " + readerSql[7].ToString() + "\t " + readerSql[8].ToString() + "\t " + " \n");
                                }
                                x++;
                            }
                            MyBody += CenterHTML(table, k);
                            System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                            MyBody = "";
                        }


                        //Назва файлу логу
                        zvit_file_name = DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + "_CorpLight.txt";

                        string path = path_zvitu + "\\" + zvit_file_name;

                        //Запис в файл логу
                        using (StreamWriter sw = File.AppendText(path))
                        {
                            sw.WriteLine("---------------------------------------------------------------------------------------------");
                            sw.WriteLine(zagol_arr[k] + "          Дата й час: " + DateTime.Now);
                            sw.WriteLine("---------------------------------------------------------------------------------------------");
                            sw.WriteLine(result_query);
                        }


                    }

                    //Відправка на електронну пошту
                    SmtpClient Smtp = new SmtpClient("mail.oschadbank.ua", 25);
                    Smtp.Credentials = new NetworkCredential(OutLookLogin, OutLookPassword);
                    MailMessage Message = new MailMessage();
                    Message.IsBodyHtml = true;
                    string Login_email = OutLookLogin;
                    Message.From = new MailAddress(Login_email);

                    //Зміна для запису адрес кому відправити листа
                    MessTo = textBox6.Text;
                    //Зчитування рядку з адресами кому потрібно відправити лист, розділення їх по символу ";"
                    foreach (var adress in MessTo.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        Message.To.Add(adress);
                    }

                    //Зміна для запису адрес кого в копію листа
                    MessCopy = textBox7.Text;
                    //Зчитування рядку з адресами кого поставити в копію листа, розділення їх по символу ";"
                    if
                        (textBox7.Text != "")
                    {
                        foreach (var adress_copy in MessCopy.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            Message.CC.Add(adress_copy);
                        }
                    }

                    //Тема листа
                    Message.Subject = textBox3.Text;
                    //Тіло повідомлення, основний текст

                    string body_texts = "";

                    //К-ть рядків в файлі        
                    int count_doc_bodyes = System.IO.File.ReadAllLines(@path_zvitu + "\\Temp_CorpLight.html").Length;

                    StreamReader BodyTexT = new StreamReader(@path_zvitu + "\\Temp_CorpLight.html");
                    for (int p = 0; p < count_doc_bodyes; p++)
                    {
                        body_texts += BodyTexT.ReadLine() + "\n";
                    }

                    BodyTexT.Close();
                    BodyTexT.Dispose();

                    Message.Body += body_texts;

                    //Спроба відправити повідомлення
                    try
                    {
                        Smtp.Send(Message);
                        Thread.Sleep(1000);
                    }
                    //В іншому випадку вивести повідомлення про помилку
                    catch (SmtpException exp)
                    {
                        MessageBox.Show(exp.ToString(), "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
            }
        }
                
        private void button4_Click(object sender, EventArgs e)
        {
            //Вибір файлу через діалогове вікно.
            file = "";
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                file = ofd.FileName;
                file_name = Path.GetFileNameWithoutExtension(ofd.FileName);
                //Вивід на форму шляху обраного sql файлу
                label12.Text = "Файл налаштувань - " + file;

                int counter = System.IO.File.ReadAllLines(file).Length;
                settings = new string[counter/2];
                StreamReader SqlText = new StreamReader(@file);
                int p = 0;
                while(SqlText.ReadLine() != null)
                {
                    settings[p] = SqlText.ReadLine();
                    p++;
                }

                for (int coc = 0; coc < settings.Length; coc++)
                {
                    richTextBox3.Text += settings[coc] + "\n";
                }

                textBox4.Text = settings[0]; //З
                textBox5.Text = settings[1]; //По
                textBox8.Text = settings[2]; //Інтервал
                textBox3.Text = settings[3]; //Тема листа
                file_sql = settings[4];      //Файл sql запитів
                file_zagol = settings[5];    //Файл заголовків
                textBox6.Text = settings[6]; //Отримувачі
                textBox7.Text = settings[7]; //Кого в копію
                path_zvitu = settings[8];    //Шлях звіту
                OutLookLogin = settings[9];
                OutLookPassword = settings[10];
                label16.Text = "Шлях звіту: " + path_zvitu;

                int count_zag_file = System.IO.File.ReadAllLines(@file_zagol).Length;
                zagol_arr = new string[count_zag_file];
                StreamReader ZagolText = new StreamReader(@file_zagol, Encoding.GetEncoding(1251));
                for (int i = 0; i < count_zag_file; i++)
                {
                    zagol_arr[i] = ZagolText.ReadLine();
                }

                int count = System.IO.File.ReadAllLines(@file_sql).Length;
                //Масив для запису запитів
                array_querry = new string[count];
                StreamReader SqlText1 = new StreamReader(@file_sql);
                for (int p1 = 0; p1 < count; p1++)
                {
                    array_querry[p1] = SqlText1.ReadLine();
                }
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            //Очищення текстових полів
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
            //Зміна ширини вікна на 460px
            richTextBox1.Clear();
            this.Width = 460;
            label12.Text = "Файл налаштувань не обрано";
            file = "";
            file_name = "";
            zagol_file = "";
            zagol_file_url = "";
            
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            checkDate();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            
            //MessageBox.Show("Повідолмення відправлено примусово", "Увага!", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if(MessageBox.Show("Ви бажаєте відправити звіт примусово?","Увага",MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                MessageBox.Show("Звіт відправлено примусово", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                PrimSend();
            }
            else
            {
                MessageBox.Show("Звіт не було відправлено", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        public void PrimSend()
        {
            Header = HeaderHTML();
            Futer = FuterHTML();
            //------------------------------------------------------------------------------
            //К-ть рядків в файлі        
            int count = System.IO.File.ReadAllLines(@file_sql).Length;
            //Масив для запису запитів
            string[] array_querry = new string[count];

            StreamReader SqlText = new StreamReader(@file_sql);
            for (int p = 0; p < count; p++)
            {
                array_querry[p] = SqlText.ReadLine();
            }
            //------------------------------------------------------------------------------

            File.Delete(@path_zvitu + "\\Temp_CorpLight.html");         
            //Відкриття підключення до Oracle
            ConnectionToOracle = new OracleConnection(connectionString);
                ConnectionToOracle.Open();

            for (int k = 0; k < count; k++)
            {
                OracleCommand sqlq1 = new OracleCommand(array_querry[k], ConnectionToOracle);
                OracleDataReader readerSql = sqlq1.ExecuteReader();

                //Зміна для запису результату запиту
                result_query = "";

                //Зчитування кількості полів в запиті SELECT
                int i = readerSql.FieldCount;
                //В залежності від цієї кількості по різному записується результат
                if (i == 1)
                {
                    zagol_DataGrid = new string[1];
                    int x = 0;

                    DataTable table = new DataTable();
                    table.Columns.Clear();
                    dataGridView1.DataSource = table;

                    while (readerSql.Read())
                    {
                        if (x == 0)
                        {
                            zagol_DataGrid[0] = readerSql[0].ToString();
                            for (int j = 0; j < zagol_DataGrid.Length; j++)
                            {
                                table.Columns.Add(zagol_DataGrid[j], typeof(string));
                            }
                        }
                        else
                        {
                            table.Rows.Add(readerSql[0].ToString());
                            result_query += (readerSql[0].ToString() + "\t " + "\n");

                        }
                        x++;
                    }
                    MyBody += CenterHTML(table, k);
                    System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                    MyBody = "";
                }

                else if (i == 2)
                {
                    zagol_DataGrid = new string[2];
                    int x = 0;

                    DataTable table = new DataTable();
                    table.Columns.Clear();
                    dataGridView1.DataSource = table;

                    while (readerSql.Read())
                    {
                        if (x == 0)
                        {
                            zagol_DataGrid[0] = readerSql[0].ToString();
                            zagol_DataGrid[1] = readerSql[1].ToString();
                            for (int j = 0; j < zagol_DataGrid.Length; j++)
                            {
                                table.Columns.Add(zagol_DataGrid[j], typeof(string));
                            }
                        }
                        else
                        {
                            table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString());
                            result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + " \n");

                        }
                        x++;
                    }
                    MyBody += CenterHTML(table, k);
                    System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                    MyBody = "";
                }

                else if (i == 3)
                {
                    zagol_DataGrid = new string[3];
                    int x = 0;

                    DataTable table = new DataTable();
                    table.Columns.Clear();
                    dataGridView1.DataSource = table;

                    while (readerSql.Read())
                    {
                        if (x == 0)
                        {
                            zagol_DataGrid[0] = readerSql[0].ToString();
                            zagol_DataGrid[1] = readerSql[1].ToString();
                            zagol_DataGrid[2] = readerSql[2].ToString();
                            for (int j = 0; j < zagol_DataGrid.Length; j++)
                            {
                                table.Columns.Add(zagol_DataGrid[j], typeof(string));
                            }
                        }
                        else
                        {
                            table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString());
                            result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + " \n");

                        }
                        x++;
                    }
                    MyBody += CenterHTML(table, k);
                    System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                    MyBody = "";
                }

                else if (i == 4)
                {
                    zagol_DataGrid = new string[4];
                    int x = 0;

                    DataTable table = new DataTable();
                    table.Columns.Clear();
                    dataGridView1.DataSource = table;

                    while (readerSql.Read())
                    {
                        if (x == 0)
                        {
                            zagol_DataGrid[0] = readerSql[0].ToString();
                            zagol_DataGrid[1] = readerSql[1].ToString();
                            zagol_DataGrid[2] = readerSql[2].ToString();
                            zagol_DataGrid[3] = readerSql[3].ToString();
                            for (int j = 0; j < zagol_DataGrid.Length; j++)
                            {
                                table.Columns.Add(zagol_DataGrid[j], typeof(string));
                            }
                        }
                        else
                        {
                            table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString());
                            result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + readerSql[3].ToString() + "\t " + " \n");
                        }
                        x++;
                    }
                    MyBody += CenterHTML(table, k);
                    System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                    MyBody = "";
                }

                else if (i == 5)
                {
                    zagol_DataGrid = new string[5];
                    int x = 0;

                    DataTable table = new DataTable();
                    table.Columns.Clear();
                    dataGridView1.DataSource = table;

                    while (readerSql.Read())
                    {
                        if (x == 0)
                        {
                            zagol_DataGrid[0] = readerSql[0].ToString();
                            zagol_DataGrid[1] = readerSql[1].ToString();
                            zagol_DataGrid[2] = readerSql[2].ToString();
                            zagol_DataGrid[3] = readerSql[3].ToString();
                            zagol_DataGrid[4] = readerSql[4].ToString();
                            for (int j = 0; j < zagol_DataGrid.Length; j++)
                            {
                                table.Columns.Add(zagol_DataGrid[j], typeof(string));
                            }
                        }
                        else
                        {
                            table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString(), readerSql[4].ToString());
                            result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + readerSql[3].ToString() + "\t " + readerSql[4].ToString() + "\t " + " \n");
                        }
                        x++;
                    }
                    MyBody += CenterHTML(table, k);
                    System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                    MyBody = "";
                }

                else if (i == 6)
                {
                    zagol_DataGrid = new string[6];
                    int x = 0;

                    DataTable table = new DataTable();
                    table.Columns.Clear();
                    dataGridView1.DataSource = table;

                    while (readerSql.Read())
                    {
                        if (x == 0)
                        {
                            zagol_DataGrid[0] = readerSql[0].ToString();
                            zagol_DataGrid[1] = readerSql[1].ToString();
                            zagol_DataGrid[2] = readerSql[2].ToString();
                            zagol_DataGrid[3] = readerSql[3].ToString();
                            zagol_DataGrid[4] = readerSql[4].ToString();
                            zagol_DataGrid[5] = readerSql[5].ToString();
                            for (int j = 0; j < zagol_DataGrid.Length; j++)
                            {
                                table.Columns.Add(zagol_DataGrid[j], typeof(string));
                            }
                        }
                        else
                        {
                            table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString(), readerSql[4].ToString(), readerSql[5].ToString());
                        }
                        x++;                       
                    }
                    MyBody += CenterHTML(table, k);
                    System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                    MyBody = "";
                }

                else if (i == 7)
                {
                    zagol_DataGrid = new string[7];
                    int x = 0;

                    DataTable table = new DataTable();
                    table.Columns.Clear();
                    dataGridView1.DataSource = table;

                    while (readerSql.Read())
                    {
                        if (x == 0)
                        {
                            zagol_DataGrid[0] = readerSql[0].ToString();
                            zagol_DataGrid[1] = readerSql[1].ToString();
                            zagol_DataGrid[2] = readerSql[2].ToString();
                            zagol_DataGrid[3] = readerSql[3].ToString();
                            zagol_DataGrid[4] = readerSql[4].ToString();
                            zagol_DataGrid[5] = readerSql[5].ToString();
                            zagol_DataGrid[6] = readerSql[6].ToString();
                            for (int j = 0; j < zagol_DataGrid.Length; j++)
                            {
                                table.Columns.Add(zagol_DataGrid[j], typeof(string));
                            }
                        }
                        else
                        {
                            table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString(), readerSql[4].ToString(), readerSql[5].ToString(), readerSql[6].ToString());
                            result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + readerSql[3].ToString() + "\t " + readerSql[4].ToString() + "\t " + readerSql[5].ToString() + "\t " + readerSql[6].ToString() + "\t " + " \n");
                        }
                        x++;
                    }
                    MyBody += CenterHTML(table, k);
                    System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                    MyBody = "";
                }

                else if (i == 8)
                {
                    zagol_DataGrid = new string[8];
                    int x = 0;

                    DataTable table = new DataTable();
                    table.Columns.Clear();
                    dataGridView1.DataSource = table;

                    while (readerSql.Read())
                    {
                        if (x == 0)
                        {
                            zagol_DataGrid[0] = readerSql[0].ToString();
                            zagol_DataGrid[1] = readerSql[1].ToString();
                            zagol_DataGrid[2] = readerSql[2].ToString();
                            zagol_DataGrid[3] = readerSql[3].ToString();
                            zagol_DataGrid[4] = readerSql[4].ToString();
                            zagol_DataGrid[5] = readerSql[5].ToString();
                            zagol_DataGrid[6] = readerSql[6].ToString();
                            zagol_DataGrid[7] = readerSql[7].ToString();
                            for (int j = 0; j < zagol_DataGrid.Length; j++)
                            {
                                table.Columns.Add(zagol_DataGrid[j], typeof(string));
                            }
                        }
                        else
                        {
                            table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString(), readerSql[4].ToString(), readerSql[5].ToString(), readerSql[6].ToString(), readerSql[7].ToString());
                            result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + readerSql[3].ToString() + "\t " + readerSql[4].ToString() + "\t " + readerSql[5].ToString() + "\t " + readerSql[6].ToString() + "\t " + readerSql[7].ToString() + "\t " + " \n");
                        }
                        x++;
                    }
                    MyBody += CenterHTML(table, k);
                    System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                    MyBody = "";
                }

                else if (i == 9)
                {
                    zagol_DataGrid = new string[9];
                    int x = 0;

                    DataTable table = new DataTable();
                    table.Columns.Clear();
                    dataGridView1.DataSource = table;

                    while (readerSql.Read())
                    {
                        if (x == 0)
                        {
                            zagol_DataGrid[0] = readerSql[0].ToString();
                            zagol_DataGrid[1] = readerSql[1].ToString();
                            zagol_DataGrid[2] = readerSql[2].ToString();
                            zagol_DataGrid[3] = readerSql[3].ToString();
                            zagol_DataGrid[4] = readerSql[4].ToString();
                            zagol_DataGrid[5] = readerSql[5].ToString();
                            zagol_DataGrid[6] = readerSql[6].ToString();
                            zagol_DataGrid[7] = readerSql[7].ToString();
                            zagol_DataGrid[8] = readerSql[8].ToString();
                            for (int j = 0; j < zagol_DataGrid.Length; j++)
                            {
                                table.Columns.Add(zagol_DataGrid[j], typeof(string));
                            }
                        }
                        else
                        {
                            table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString(), readerSql[4].ToString(), readerSql[5].ToString(), readerSql[6].ToString(), readerSql[7].ToString(), readerSql[8].ToString());
                            result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + readerSql[3].ToString() + "\t " + readerSql[4].ToString() + "\t " + readerSql[5].ToString() + "\t " + readerSql[6].ToString() + "\t " + readerSql[7].ToString() + "\t " + readerSql[8].ToString() + "\t " + " \n");
                        }
                        x++;
                    }
                    MyBody += CenterHTML(table, k);
                    System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                    MyBody = "";
                }

                else if (i == 10)
                {
                    zagol_DataGrid = new string[10];
                    int x = 0;

                    DataTable table = new DataTable();
                    table.Columns.Clear();
                    dataGridView1.DataSource = table;

                    while (readerSql.Read())
                    {
                        if (x == 0)
                        {
                            zagol_DataGrid[0] = readerSql[0].ToString();
                            zagol_DataGrid[1] = readerSql[1].ToString();
                            zagol_DataGrid[2] = readerSql[2].ToString();
                            zagol_DataGrid[3] = readerSql[3].ToString();
                            zagol_DataGrid[4] = readerSql[4].ToString();
                            zagol_DataGrid[5] = readerSql[5].ToString();
                            zagol_DataGrid[6] = readerSql[6].ToString();
                            zagol_DataGrid[7] = readerSql[7].ToString();
                            zagol_DataGrid[8] = readerSql[8].ToString();
                            zagol_DataGrid[9] = readerSql[9].ToString();
                            for (int j = 0; j < zagol_DataGrid.Length; j++)
                            {
                                table.Columns.Add(zagol_DataGrid[j], typeof(string));
                            }
                        }
                        else
                        {
                            table.Rows.Add(readerSql[0].ToString(), readerSql[1].ToString(), readerSql[2].ToString(), readerSql[3].ToString(), readerSql[4].ToString(), readerSql[5].ToString(), readerSql[6].ToString(), readerSql[7].ToString(), readerSql[8].ToString(), readerSql[9].ToString());
                            result_query += (readerSql[0].ToString() + "\t " + readerSql[1].ToString() + "\t " + readerSql[2].ToString() + "\t " + readerSql[3].ToString() + "\t " + readerSql[4].ToString() + "\t " + readerSql[5].ToString() + "\t " + readerSql[6].ToString() + "\t " + readerSql[7].ToString() + "\t " + readerSql[8].ToString() + "\t " + " \n");
                        }
                        x++;
                    }
                    MyBody += CenterHTML(table, k);
                    System.IO.File.WriteAllText(@path_zvitu + "\\Temp_CorpLight.html", Header + MyBody + Futer);
                    MyBody = "";
                }
              

                //Назва файлу логу
                zvit_file_name = DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + "_CorpLight.txt";

                string path = path_zvitu + "\\" + zvit_file_name;

                //Запис в файл логу
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine("---------------------------------------------------------------------------------------------");
                    sw.WriteLine(zagol_arr[k] + "          Дата й час: " + DateTime.Now);
                    sw.WriteLine("---------------------------------------------------------------------------------------------");
                    sw.WriteLine(result_query);
                }               
            }
        
            //Відправка на електронну пошту
            SmtpClient Smtp = new SmtpClient("mail.oschadbank.ua", 25);
            Smtp.Credentials = new NetworkCredential(OutLookLogin, OutLookPassword);
            MailMessage Message = new MailMessage();
            Message.IsBodyHtml = true;
            string Login_email = OutLookLogin;
            Message.From = new MailAddress(Login_email);

            //Зміна для запису адрес кому відправити листа
            MessTo = textBox6.Text;
            //Зчитування рядку з адресами кому потрібно відправити лист, розділення їх по символу ";"
            foreach (var adress in MessTo.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                Message.To.Add(adress);
            }

            //Зміна для запису адрес кого в копію листа
            MessCopy = textBox7.Text;
            //Зчитування рядку з адресами кого поставити в копію листа, розділення їх по символу ";"
            if
                (textBox7.Text != "")
            {
                foreach (var adress_copy in MessCopy.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Message.CC.Add(adress_copy);
                }
            }

            //Тема листа
            Message.Subject = textBox3.Text;
            //Тіло повідомлення, основний текст

            string body_texts = "";

            //К-ть рядків в файлі        
            int count_doc_bodyes = System.IO.File.ReadAllLines(@path_zvitu + "\\Temp_CorpLight.html").Length;

            StreamReader BodyTexT = new StreamReader(@path_zvitu + "\\Temp_CorpLight.html");
            for (int p = 0; p < count_doc_bodyes; p++)
            {
                body_texts += BodyTexT.ReadLine() + "\n";
            }

            BodyTexT.Close();
            BodyTexT.Dispose();

            Message.Body += body_texts;

            //Спроба відправити повідомлення
            try
            {
                Smtp.Send(Message);
                Thread.Sleep(1000);

                File.Delete(@path_zvitu + "\\Temp_CorpLight.html");
                result_query = "";
            }
            //В іншому випадку вивести повідомлення про помилку
            catch (SmtpException exp)
            {
                MessageBox.Show(exp.ToString(), "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            //Вибір файлу через діалогове вікно.
            zagol_file_url = "";
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                zagol_file_url = ofd.FileName;
                zagol_file = Path.GetFileNameWithoutExtension(ofd.FileName);

                int count_zag_file = System.IO.File.ReadAllLines(@zagol_file_url).Length;
                zagol_arr = new string[count_zag_file];
                StreamReader ZagolText = new StreamReader(zagol_file_url, Encoding.GetEncoding(1251));

                for (int i=0;i<count_zag_file;i++)
                {
                    zagol_arr[i] = ZagolText.ReadLine();
                }

            }
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            textBox8.BackColor = Color.White;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            this.Height = 584;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.Height = 392;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            textBox3.BackColor = Color.White;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            textBox4.BackColor = Color.White;
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            textBox6.BackColor = Color.White;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            textBox5.BackColor = Color.White;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Ви дійсно бажаєте зупинити роботу програми?","Увага",MessageBoxButtons.OKCancel,MessageBoxIcon.Question) == DialogResult.OK)
            {           
                timer1.Stop();
                timer1.Dispose();
                timer1.Enabled = false;

                label6.ForeColor = Color.Red;
                label6.Text = "Відправку не запущено";
                MessageBox.Show("Роботу програми успішно зупинено", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //Очищення текстових полів
                textBox3.Text = "";
                textBox4.Text = "";
                textBox5.Text = "";
                textBox6.Text = "";
                textBox7.Text = "";
                textBox8.Text = "";
                //Зміна ширини вікна на 460px
                this.Width = 460;
                label12.Text = "Файл налаштувань не обрано";
                file = "";
                file_name = "";
                zagol_file = "";
                zagol_file_url = "";
                MessCopy = "";
                MessTo = "";
                time_2 = "";              
            }
            else
            {
               
            }
        }

        protected string HeaderHTML()
        {
            StringBuilder HeaderHTML = new StringBuilder();
            HeaderHTML.Append("<html>");
            HeaderHTML.Append("<head>");
            HeaderHTML.Append("</head>");
            HeaderHTML.Append("<body>");
            string HtmlText = HeaderHTML.ToString();
            return HtmlText;
        }

        protected string CenterHTML(DataTable td, int num)
        {
            StringBuilder CenterHTML = new StringBuilder();
            CenterHTML.Append("<br>");
            CenterHTML.Append("<b><font color='DarkRed'>" + zagol_arr[num] + "</b></font>");
            CenterHTML.Append("<table border='1px' cellpadding='1' cellspacing='1'  bgcolor='lightyellow' style='border: 1px solid black; text-align: left; padding: 5px; border-collapse: collapse;'>");
            CenterHTML.Append("<tr >");
            CenterHTML.Append("<th>Times</th>");
            CenterHTML.Append("<th>Count</th>");
            CenterHTML.Append("<th>Sum</th>");
            CenterHTML.Append("</tr>");
            CenterHTML.Append("<tr >");
            foreach (DataColumn myColumn in td.Columns)
            {
                CenterHTML.Append("<td style='border: 1px solid black;'>");
                CenterHTML.Append(myColumn.ColumnName);
                CenterHTML.Append("</td>");
            }
            CenterHTML.Append("</tr>");
            foreach (DataRow myRow in td.Rows)
            {
                CenterHTML.Append("<tr >");
                foreach (DataColumn myColumn in td.Columns)
                {
                    CenterHTML.Append("<td style='border: 1px solid black;'>");
                    CenterHTML.Append(myRow[myColumn.ColumnName].ToString());
                    CenterHTML.Append("</td>");
                }
                CenterHTML.Append("</tr>");
            }

            CenterHTML.Append("</table>");
            CenterHTML.Append("<br>");
            string Centr = CenterHTML.ToString();
            return Centr;
        }

        protected string FuterHTML()
        {
            StringBuilder FuterHTML = new StringBuilder();
            FuterHTML.Append("</body>");
            FuterHTML.Append("</html>");
            string Futer = FuterHTML.ToString();
            return Futer;
        }
     
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
