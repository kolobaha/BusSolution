using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BusSolOnDB.Models;

namespace BusSolOnDB
{
    public partial class Form1 : Form
    {
        BusFleet TaskBusFleet = new BusFleet();

        public Form1()
        {
            InitializeComponent();
            ShowBusesMoves();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TaskBusFleet.GetTestData();
        }

        public void ShowBusesMoves()
        {
            listBox1.Items.Clear();
            listBox3.Items.Clear();
            listBox1.Items.Add("Buses:");
            foreach (var bus in TaskBusFleet.Buses)
            {
                listBox1.Items.Add(bus.ToString());
            }
            listBox3.Items.Add("Ways:");
            foreach (var move in TaskBusFleet.Moves)
            {
                listBox3.Items.Add(move.ToString());
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        // Метод загрузки.
        private void DownloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StreamReader myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "c:\\Documents\\BusTask";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = new StreamReader(openFileDialog1.FileName)) != null)
                    {
                        using (myStream)
                        {
                            TaskBusFleet.ReadData(myStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            ShowBusesMoves();
        }

        // Генерируем транспортную карту переездов.
        private void Button1_Click(object sender, EventArgs e)
        {
            ShowBigMap(GetStartTime());
        }

        public void ShowBigMap(int startTime)
        {
            TaskBusFleet.InitializeBigTransportMap(startTime);
            ShowIt(TaskBusFleet.BigTransportMap);
        }

        private void EnterTestDataToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public void ShowIt(List<Transaction> transactionMap)
        {
            listBox2.Items.Clear();
            foreach (var t in transactionMap)
            {
                if (t != null)
                    listBox2.Items.Add(t.ToString());
            }
        }

        public int GetStartTime()
        {
            return Convert.ToInt32(startHour.Text) * Constans.MinutesInHour + Convert.ToInt32(startMinute.Text);
        }

        public int GetStartStation()
        {
            return Convert.ToInt32(startStTB.Text);
        }

        public int GetEndStation()
        {
            return Convert.ToInt32(endStTB.Text);
        }

        public void SolutionResult()
        {
            List<Transaction> res = TaskBusFleet.Solution(GetStartStation(), GetEndStation(), GetStartTime()).ToList();
            if (res == null || res.Count != 2)
            {
                MessageBox.Show("Решение ошибочно или его нет!");
                return;
            }

            string timeRes = "Самый быстрый маршрут : ";

            timeRes += res[0].Way + res[0].EndStation.ToString() ;
           
            timeRes += " Время прибытия : " + Constans.GetTimeFromMinutes(res[0].EndTime);

            string costRes = "Самый дешёвый маршрут : ";
          
            costRes += res[1].Way + res[1].EndStation.ToString();
         
            costRes += " Цена поездки : " + res[1].Cost;
            listBox2.Items.Add(costRes);
            listBox2.Items.Add(timeRes);

        }

        private void IterateBut_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            SolutionResult();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ShowIt(TaskBusFleet.GetTransportMap(TaskBusFleet.Buses, TaskBusFleet.Moves));
        }
    }
}
