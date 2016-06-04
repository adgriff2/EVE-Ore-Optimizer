using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.OrTools.LinearSolver;
using System.Data.SQLite;

namespace EVE_Ore_Optimizer
{
    public partial class Form1 : Form
    {
        SQLiteConnection m_dbConnection;
        List<ManufacturableItem> items;
        List<TypeMaterials> typeMaterialsList;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_dbConnection = new SQLiteConnection("Data Source=BPO_Mats.db;Version=3;");
            m_dbConnection.Open();
            string sql = "select typeID,materialTypeID,quantity from invTypeMaterials";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            typeMaterialsList = new List<TypeMaterials>();
            while (reader.Read())
            {
                typeMaterialsList.Add(new TypeMaterials(
                    (long)reader["typeID"],
                    (long)reader["materialTypeID"],
                    (long)reader["quantity"]));
            }

            List<long> manufacturableTypeIDs = typeMaterialsList.GroupBy(x => x.typeID).Select(y => y.First()).Select(z => z.typeID).ToList(); ;
            List<KeyValuePair<long, string>> Names = new List<KeyValuePair<long, string>>();
            sql = "select typeID,typeName from invTypes";
            command = new SQLiteCommand(sql, m_dbConnection);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Names.Add(new KeyValuePair<long, string>(
                    (long)reader["typeID"],
                    (string)reader["typeName"]
                    ));
            }

            items = new List<ManufacturableItem>();
            List<long> mineralTypes = new List<long> { 11396, 34, 35, 36, 37, 38, 39, 40 };
            comboBox17.Items.Clear();
            foreach (long typeID in manufacturableTypeIDs)
            {
                if (!Names.Any(x => x.Key == typeID)) { continue; }
                List<long> m_typeIDs = typeMaterialsList.FindAll(x => x.typeID == typeID).Select(y => y.materialTypeID).ToList();
                List<long> m_Qtys = typeMaterialsList.FindAll(x => x.typeID == typeID).Select(y => y.quantity).ToList();

                Dictionary<long, long> dic = m_typeIDs.Zip(m_Qtys, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
                string name = Names.First(x => x.Key == typeID).Value;
                if (!m_typeIDs.Except(mineralTypes).Any())
                {
                    items.Add(new ManufacturableItem(
                        typeID,
                        name,
                        dic));
                    
                    comboBox17.Items.Add(name);
                }
            }



            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
            comboBox6.SelectedIndex = 0;
            comboBox7.SelectedIndex = 0;
            comboBox8.SelectedIndex = 0;
            comboBox9.SelectedIndex = 0;
            comboBox10.SelectedIndex = 0;
            comboBox11.SelectedIndex = 0;
            comboBox12.SelectedIndex = 0;
            comboBox13.SelectedIndex = 0;
            comboBox14.SelectedIndex = 0;
            comboBox15.SelectedIndex = 0;
            comboBox16.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            double[,] ore_mineral_matrix = new double[8, 16] {
                { 415,  346,    351,    107,    800,    134,    0,      2200,   0,      0,      10000,  56000,  21000,  0,      22000,  0   },
                { 0,    173,    25,     213,    100,    0,      0,      0,      1000,   2200,   0,      12050,  0,      12000,  0,      0   },
                { 0,    0,      50,     107,    0,      267,    350,    0,      0,      2400,   0,      2100,   0,      0,      2500,   0   },
                { 0,    0,      0,      0,      85,     134,    0,      100,    200,    300,    1600,   450,    0,      0,      0,      0   },
                { 0,    0,      5,      0,      0,      0,      75,     120,    100,    0,      120,    0,      760,    0,      0,      0   },
                { 0,    0,      0,      0,      0,      0,      8,      15,     19,     0,      0,      0,      135,    450,    0,      0   },
                { 0,    0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      100,    320,    0   },
                { 0,    0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      0,      300 }
            };

            double[] ore_volume_matrix = new double[16] { 0.15, 0.19, 0.16, 0.15, 0.3, 0.19, 0.15, 0.47, 0.86, 1.8, 4.2, 28, 7.81, 4.4, 8.8, 0.1 };

            double[] ore_cost_matrix = new double[16] { 1943.99, 2999.96, 6399.94, 8499.99, 13756.95, 25420.18, 59299.00, 109998.98, 77108.10, 173077.98, 202247.96, 529993.55, 514889.99, 609999.95, 588999.81, 4089999.92 };

            double[] desired_minerals = { 2555556, 666667, 194444, 22222, 17222, 8444, 1778, 0 };

            bool failed = false;

            failed = (!double.TryParse(textBox1.Text, out ore_cost_matrix[0]) || failed);
            failed = (!double.TryParse(textBox2.Text, out ore_cost_matrix[1]) || failed);
            failed = (!double.TryParse(textBox3.Text, out ore_cost_matrix[2]) || failed);
            failed = (!double.TryParse(textBox4.Text, out ore_cost_matrix[3]) || failed);
            failed = (!double.TryParse(textBox5.Text, out ore_cost_matrix[4]) || failed);
            failed = (!double.TryParse(textBox6.Text, out ore_cost_matrix[5]) || failed);
            failed = (!double.TryParse(textBox7.Text, out ore_cost_matrix[6]) || failed);
            failed = (!double.TryParse(textBox8.Text, out ore_cost_matrix[7]) || failed);
            failed = (!double.TryParse(textBox9.Text, out ore_cost_matrix[8]) || failed);
            failed = (!double.TryParse(textBox10.Text, out ore_cost_matrix[9]) || failed);
            failed = (!double.TryParse(textBox11.Text, out ore_cost_matrix[10]) || failed);
            failed = (!double.TryParse(textBox12.Text, out ore_cost_matrix[11]) || failed);
            failed = (!double.TryParse(textBox13.Text, out ore_cost_matrix[12]) || failed);
            failed = (!double.TryParse(textBox14.Text, out ore_cost_matrix[13]) || failed);
            failed = (!double.TryParse(textBox15.Text, out ore_cost_matrix[14]) || failed);
            failed = (!double.TryParse(textBox16.Text, out ore_cost_matrix[15]) || failed);

            failed = (!double.TryParse(textBox24.Text, out desired_minerals[0]) || failed);
            failed = (!double.TryParse(textBox23.Text, out desired_minerals[1]) || failed);
            failed = (!double.TryParse(textBox22.Text, out desired_minerals[2]) || failed);
            failed = (!double.TryParse(textBox21.Text, out desired_minerals[3]) || failed);
            failed = (!double.TryParse(textBox20.Text, out desired_minerals[4]) || failed);
            failed = (!double.TryParse(textBox19.Text, out desired_minerals[5]) || failed);
            failed = (!double.TryParse(textBox18.Text, out desired_minerals[6]) || failed);
            failed = (!double.TryParse(textBox17.Text, out desired_minerals[7]) || failed);

            double refining_eff;
            double BPO_ME;

            failed = (!double.TryParse(textBox26.Text, out refining_eff) || failed);
            failed = (!double.TryParse(textBox25.Text, out BPO_ME) || failed);

            if (failed)
            {
                MessageBox.Show("Failed parsing something into a number, check your typing");
                return;
            }

            double[] quality_opts = { 1.00, 1.05, 1.10 };
            double[] quality = new double[16];
            quality[0] = quality_opts[comboBox1.SelectedIndex];
            quality[1] = quality_opts[comboBox2.SelectedIndex];
            quality[2] = quality_opts[comboBox3.SelectedIndex];
            quality[3] = quality_opts[comboBox4.SelectedIndex];
            quality[4] = quality_opts[comboBox5.SelectedIndex];
            quality[5] = quality_opts[comboBox6.SelectedIndex];
            quality[6] = quality_opts[comboBox7.SelectedIndex];
            quality[7] = quality_opts[comboBox8.SelectedIndex];
            quality[8] = quality_opts[comboBox9.SelectedIndex];
            quality[9] = quality_opts[comboBox10.SelectedIndex];
            quality[10] = quality_opts[comboBox11.SelectedIndex];
            quality[11] = quality_opts[comboBox12.SelectedIndex];
            quality[12] = quality_opts[comboBox13.SelectedIndex];
            quality[13] = quality_opts[comboBox14.SelectedIndex];
            quality[14] = quality_opts[comboBox15.SelectedIndex];
            quality[15] = quality_opts[comboBox16.SelectedIndex];

            Solver solver = Solver.CreateSolver("IntegerProgramming", "GLOP_LINEAR_PROGRAMMING");
            Variable ore0 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Veldspar");
            Variable ore1 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Scordite");
            Variable ore2 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Pyroxeres");
            Variable ore3 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Plagioclase");
            Variable ore4 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Omber");
            Variable ore5 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Kernite");
            Variable ore6 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Jaspet");
            Variable ore7 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Hedbergite");
            Variable ore8 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Hemorphite");
            Variable ore9 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Gneiss");
            Variable ore10 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Dark Ochre");
            Variable ore11 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Spodumain");
            Variable ore12 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Crokite");
            Variable ore13 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Bistot");
            Variable ore14 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Arkonor");
            Variable ore15 = solver.MakeNumVar(0.0, double.PositiveInfinity, "Mercoxit");

            if (radioButton1.Checked)
            {
                solver.Minimize(
                    ore_cost_matrix[0] * ore0 +
                    ore_cost_matrix[1] * ore1 +
                    ore_cost_matrix[2] * ore2 +
                    ore_cost_matrix[3] * ore3 +
                    ore_cost_matrix[4] * ore4 +
                    ore_cost_matrix[5] * ore5 +
                    ore_cost_matrix[6] * ore6 +
                    ore_cost_matrix[7] * ore7 +
                    ore_cost_matrix[8] * ore8 +
                    ore_cost_matrix[9] * ore9 +
                    ore_cost_matrix[10] * ore10 +
                    ore_cost_matrix[11] * ore11 +
                    ore_cost_matrix[12] * ore12 +
                    ore_cost_matrix[13] * ore13 +
                    ore_cost_matrix[14] * ore14 +
                    ore_cost_matrix[15] * ore15
                );
            }
            else
            {
                solver.Minimize(
                    ore_volume_matrix[0] * ore0 +
                    ore_volume_matrix[1] * ore1 +
                    ore_volume_matrix[2] * ore2 +
                    ore_volume_matrix[3] * ore3 +
                    ore_volume_matrix[4] * ore4 +
                    ore_volume_matrix[5] * ore5 +
                    ore_volume_matrix[6] * ore6 +
                    ore_volume_matrix[7] * ore7 +
                    ore_volume_matrix[8] * ore8 +
                    ore_volume_matrix[9] * ore9 +
                    ore_volume_matrix[10] * ore10 +
                    ore_volume_matrix[11] * ore11 +
                    ore_volume_matrix[12] * ore12 +
                    ore_volume_matrix[13] * ore13 +
                    ore_volume_matrix[14] * ore14 +
                    ore_volume_matrix[15] * ore15
                );
            }

            for (int i = 0; i<8; i++)
            {
                desired_minerals[i] = desired_minerals[i] * (1 - BPO_ME / 100);
                for (int j = 0; j<16; j++)
                {
                    ore_mineral_matrix[i, j] = ore_mineral_matrix[i, j] * quality[j] * refining_eff / 100;
                }
            }

            Constraint mineral0 = solver.Add(
                ore_mineral_matrix[0, 0] * ore0 +
                ore_mineral_matrix[0, 1] * ore1 +
                ore_mineral_matrix[0, 2] * ore2 +
                ore_mineral_matrix[0, 3] * ore3 +
                ore_mineral_matrix[0, 4] * ore4 +
                ore_mineral_matrix[0, 5] * ore5 +
                ore_mineral_matrix[0, 6] * ore6 +
                ore_mineral_matrix[0, 7] * ore7 +
                ore_mineral_matrix[0, 8] * ore8 +
                ore_mineral_matrix[0, 9] * ore9 +
                ore_mineral_matrix[0, 10] * ore10 +
                ore_mineral_matrix[0, 11] * ore11 +
                ore_mineral_matrix[0, 12] * ore12 +
                ore_mineral_matrix[0, 13] * ore13 +
                ore_mineral_matrix[0, 14] * ore14 +
                ore_mineral_matrix[0, 15] * ore15
                >= desired_minerals[0]);

            Constraint mineral1 = solver.Add(
                ore_mineral_matrix[1, 0] * ore0 +
                ore_mineral_matrix[1, 1] * ore1 +
                ore_mineral_matrix[1, 2] * ore2 +
                ore_mineral_matrix[1, 3] * ore3 +
                ore_mineral_matrix[1, 4] * ore4 +
                ore_mineral_matrix[1, 5] * ore5 +
                ore_mineral_matrix[1, 6] * ore6 +
                ore_mineral_matrix[1, 7] * ore7 +
                ore_mineral_matrix[1, 8] * ore8 +
                ore_mineral_matrix[1, 9] * ore9 +
                ore_mineral_matrix[1, 10] * ore10 +
                ore_mineral_matrix[1, 11] * ore11 +
                ore_mineral_matrix[1, 12] * ore12 +
                ore_mineral_matrix[1, 13] * ore13 +
                ore_mineral_matrix[1, 14] * ore14 +
                ore_mineral_matrix[1, 15] * ore15
                >= desired_minerals[1]);

            Constraint mineral2 = solver.Add(
                ore_mineral_matrix[2, 0] * ore0 +
                ore_mineral_matrix[2, 1] * ore1 +
                ore_mineral_matrix[2, 2] * ore2 +
                ore_mineral_matrix[2, 3] * ore3 +
                ore_mineral_matrix[2, 4] * ore4 +
                ore_mineral_matrix[2, 5] * ore5 +
                ore_mineral_matrix[2, 6] * ore6 +
                ore_mineral_matrix[2, 7] * ore7 +
                ore_mineral_matrix[2, 8] * ore8 +
                ore_mineral_matrix[2, 9] * ore9 +
                ore_mineral_matrix[2, 10] * ore10 +
                ore_mineral_matrix[2, 11] * ore11 +
                ore_mineral_matrix[2, 12] * ore12 +
                ore_mineral_matrix[2, 13] * ore13 +
                ore_mineral_matrix[2, 14] * ore14 +
                ore_mineral_matrix[2, 15] * ore15
                >= desired_minerals[2]);

            Constraint mineral3 = solver.Add(
                ore_mineral_matrix[3, 0] * ore0 +
                ore_mineral_matrix[3, 1] * ore1 +
                ore_mineral_matrix[3, 2] * ore2 +
                ore_mineral_matrix[3, 3] * ore3 +
                ore_mineral_matrix[3, 4] * ore4 +
                ore_mineral_matrix[3, 5] * ore5 +
                ore_mineral_matrix[3, 6] * ore6 +
                ore_mineral_matrix[3, 7] * ore7 +
                ore_mineral_matrix[3, 8] * ore8 +
                ore_mineral_matrix[3, 9] * ore9 +
                ore_mineral_matrix[3, 10] * ore10 +
                ore_mineral_matrix[3, 11] * ore11 +
                ore_mineral_matrix[3, 12] * ore12 +
                ore_mineral_matrix[3, 13] * ore13 +
                ore_mineral_matrix[3, 14] * ore14 +
                ore_mineral_matrix[3, 15] * ore15
                >= desired_minerals[3]);

            Constraint mineral4 = solver.Add(
                ore_mineral_matrix[4, 0] * ore0 +
                ore_mineral_matrix[4, 1] * ore1 +
                ore_mineral_matrix[4, 2] * ore2 +
                ore_mineral_matrix[4, 3] * ore3 +
                ore_mineral_matrix[4, 4] * ore4 +
                ore_mineral_matrix[4, 5] * ore5 +
                ore_mineral_matrix[4, 6] * ore6 +
                ore_mineral_matrix[4, 7] * ore7 +
                ore_mineral_matrix[4, 8] * ore8 +
                ore_mineral_matrix[4, 9] * ore9 +
                ore_mineral_matrix[4, 10] * ore10 +
                ore_mineral_matrix[4, 11] * ore11 +
                ore_mineral_matrix[4, 12] * ore12 +
                ore_mineral_matrix[4, 13] * ore13 +
                ore_mineral_matrix[4, 14] * ore14 +
                ore_mineral_matrix[4, 15] * ore15
                >= desired_minerals[4]);

            Constraint mineral5 = solver.Add(
                ore_mineral_matrix[5, 0] * ore0 +
                ore_mineral_matrix[5, 1] * ore1 +
                ore_mineral_matrix[5, 2] * ore2 +
                ore_mineral_matrix[5, 3] * ore3 +
                ore_mineral_matrix[5, 4] * ore4 +
                ore_mineral_matrix[5, 5] * ore5 +
                ore_mineral_matrix[5, 6] * ore6 +
                ore_mineral_matrix[5, 7] * ore7 +
                ore_mineral_matrix[5, 8] * ore8 +
                ore_mineral_matrix[5, 9] * ore9 +
                ore_mineral_matrix[5, 10] * ore10 +
                ore_mineral_matrix[5, 11] * ore11 +
                ore_mineral_matrix[5, 12] * ore12 +
                ore_mineral_matrix[5, 13] * ore13 +
                ore_mineral_matrix[5, 14] * ore14 +
                ore_mineral_matrix[5, 15] * ore15
                >= desired_minerals[5]);

            Constraint mineral6 = solver.Add(
                ore_mineral_matrix[6, 0] * ore0 +
                ore_mineral_matrix[6, 1] * ore1 +
                ore_mineral_matrix[6, 2] * ore2 +
                ore_mineral_matrix[6, 3] * ore3 +
                ore_mineral_matrix[6, 4] * ore4 +
                ore_mineral_matrix[6, 5] * ore5 +
                ore_mineral_matrix[6, 6] * ore6 +
                ore_mineral_matrix[6, 7] * ore7 +
                ore_mineral_matrix[6, 8] * ore8 +
                ore_mineral_matrix[6, 9] * ore9 +
                ore_mineral_matrix[6, 10] * ore10 +
                ore_mineral_matrix[6, 11] * ore11 +
                ore_mineral_matrix[6, 12] * ore12 +
                ore_mineral_matrix[6, 13] * ore13 +
                ore_mineral_matrix[6, 14] * ore14 +
                ore_mineral_matrix[6, 15] * ore15
                >= desired_minerals[6]);

            Constraint mineral7 = solver.Add(
                ore_mineral_matrix[7, 0] * ore0 +
                ore_mineral_matrix[7, 1] * ore1 +
                ore_mineral_matrix[7, 2] * ore2 +
                ore_mineral_matrix[7, 3] * ore3 +
                ore_mineral_matrix[7, 4] * ore4 +
                ore_mineral_matrix[7, 5] * ore5 +
                ore_mineral_matrix[7, 6] * ore6 +
                ore_mineral_matrix[7, 7] * ore7 +
                ore_mineral_matrix[7, 8] * ore8 +
                ore_mineral_matrix[7, 9] * ore9 +
                ore_mineral_matrix[7, 10] * ore10 +
                ore_mineral_matrix[7, 11] * ore11 +
                ore_mineral_matrix[7, 12] * ore12 +
                ore_mineral_matrix[7, 13] * ore13 +
                ore_mineral_matrix[7, 14] * ore14 +
                ore_mineral_matrix[7, 15] * ore15
                >= desired_minerals[7]);

            Constraint minore0 = solver.Add(ore0 >= 0);
            Constraint minore1 = solver.Add(ore1 >= 0);
            Constraint minore2 = solver.Add(ore2 >= 0);
            Constraint minore3 = solver.Add(ore3 >= 0);
            Constraint minore4 = solver.Add(ore4 >= 0);
            Constraint minore5 = solver.Add(ore5 >= 0);
            Constraint minore6 = solver.Add(ore6 >= 0);
            Constraint minore7 = solver.Add(ore7 >= 0);
            Constraint minore8 = solver.Add(ore8 >= 0);
            Constraint minore9 = solver.Add(ore9 >= 0);
            Constraint minore10 = solver.Add(ore10 >= 0);
            Constraint minore11 = solver.Add(ore11 >= 0);
            Constraint minore12 = solver.Add(ore12 >= 0);
            Constraint minore13 = solver.Add(ore13 >= 0);
            Constraint minore14 = solver.Add(ore14 >= 0);
            Constraint minore15 = solver.Add(ore15 >= 0);

            int resultStatus = solver.Solve();
            if (resultStatus != Solver.OPTIMAL)
            {
                MessageBox.Show("The problem does not have an optimal solution!");
                return;
            }

            double cost = ore_cost_matrix[0] * ore0.SolutionValue() +
                    ore_cost_matrix[1] * ore1.SolutionValue() +
                    ore_cost_matrix[2] * ore2.SolutionValue() +
                    ore_cost_matrix[3] * ore3.SolutionValue() +
                    ore_cost_matrix[4] * ore4.SolutionValue() +
                    ore_cost_matrix[5] * ore5.SolutionValue() +
                    ore_cost_matrix[6] * ore6.SolutionValue() +
                    ore_cost_matrix[7] * ore7.SolutionValue() +
                    ore_cost_matrix[8] * ore8.SolutionValue() +
                    ore_cost_matrix[9] * ore9.SolutionValue() +
                    ore_cost_matrix[10] * ore10.SolutionValue() +
                    ore_cost_matrix[11] * ore11.SolutionValue() +
                    ore_cost_matrix[12] * ore12.SolutionValue() +
                    ore_cost_matrix[13] * ore13.SolutionValue() +
                    ore_cost_matrix[14] * ore14.SolutionValue() +
                    ore_cost_matrix[15] * ore15.SolutionValue();

            double volume = ore_volume_matrix[0] * ore0.SolutionValue() +
                    ore_volume_matrix[1] * ore1.SolutionValue() +
                    ore_volume_matrix[2] * ore2.SolutionValue() +
                    ore_volume_matrix[3] * ore3.SolutionValue() +
                    ore_volume_matrix[4] * ore4.SolutionValue() +
                    ore_volume_matrix[5] * ore5.SolutionValue() +
                    ore_volume_matrix[6] * ore6.SolutionValue() +
                    ore_volume_matrix[7] * ore7.SolutionValue() +
                    ore_volume_matrix[8] * ore8.SolutionValue() +
                    ore_volume_matrix[9] * ore9.SolutionValue() +
                    ore_volume_matrix[10] * ore10.SolutionValue() +
                    ore_volume_matrix[11] * ore11.SolutionValue() +
                    ore_volume_matrix[12] * ore12.SolutionValue() +
                    ore_volume_matrix[13] * ore13.SolutionValue() +
                    ore_volume_matrix[14] * ore14.SolutionValue() +
                    ore_volume_matrix[15] * ore15.SolutionValue();

            label34.Text = ore0.SolutionValue().ToString("N0");
            label33.Text = ore1.SolutionValue().ToString("N0");
            label32.Text = ore2.SolutionValue().ToString("N0");
            label31.Text = ore3.SolutionValue().ToString("N0");
            label30.Text = ore4.SolutionValue().ToString("N0");
            label29.Text = ore5.SolutionValue().ToString("N0");
            label28.Text = ore6.SolutionValue().ToString("N0");
            label27.Text = ore7.SolutionValue().ToString("N0");
            label26.Text = ore8.SolutionValue().ToString("N0");
            label25.Text = ore9.SolutionValue().ToString("N0");
            label24.Text = ore10.SolutionValue().ToString("N0");
            label23.Text = ore11.SolutionValue().ToString("N0");
            label22.Text = ore12.SolutionValue().ToString("N0");
            label21.Text = ore13.SolutionValue().ToString("N0");
            label20.Text = ore14.SolutionValue().ToString("N0");
            label19.Text = ore15.SolutionValue().ToString("N0");

            label37.Text = cost.ToString("N2");
            label38.Text = volume.ToString("N2");
        }

        private void comboBox17_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox24.Text = "0";
            textBox23.Text = "0";
            textBox22.Text = "0";
            textBox21.Text = "0";
            textBox20.Text = "0";
            textBox19.Text = "0";
            textBox18.Text = "0";
            textBox17.Text = "0";

            string name = comboBox17.SelectedItem.ToString();

            Dictionary<long, long> temp = items.First(x => x.name == name).mats;

            if (temp.ContainsKey(34))
            {
                textBox24.Text = temp[34].ToString();
            }
            if (temp.ContainsKey(35))
            {
                textBox23.Text = temp[35].ToString();
            }
            if (temp.ContainsKey(36))
            {
                textBox22.Text = temp[36].ToString();
            }
            if (temp.ContainsKey(37))
            {
                textBox21.Text = temp[37].ToString();
            }
            if (temp.ContainsKey(38))
            {
                textBox20.Text = temp[38].ToString();
            }
            if (temp.ContainsKey(39))
            {
                textBox19.Text = temp[39].ToString();
            }
            if (temp.ContainsKey(40))
            {
                textBox18.Text = temp[40].ToString();
            }
            if (temp.ContainsKey(11396))
            {
                textBox17.Text = temp[11396].ToString();
            }
        }
    }
}
