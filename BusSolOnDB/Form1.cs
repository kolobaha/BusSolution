using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BusSolOnDB.Models;
//Есть N автобусов, каждый автобус ездит по заранее известному
//циклическому маршруту.Известна стоимость проезда (оплачивается при
//входе в автобус) и время движения между остановками.Автобусы выходят
//на маршрут(появляются на первой остановке) в определенное время.
//Необходимо написать программу, которая будет находить два пути: самый
//дешевый путь и самый быстрый путь.Интерфейс программы должен
//позволять загружать файл с маршрутами автобусов, выбирать начальную и
//конечную точки и время отправления из начальной точки.

//Формат входного файла:
//{N число автобусов}
//{K число остановок}
//{время отправления 1 автобуса} {время отправления 2 автобуса} ...
//{время отправления N автобуса}
//{стоимость проезда на 1 автобусе} {стоимость проезда на 2 автобусе}
//... {стоимость проезда на N автобусе}
//{число остановок на маршруте 1 автобуса} {номер 1 остановки} {номер 2
//остановки} ... {номер последней остановки} {время в пути между 1 и 2
//остановкой} {время в пути между 2 и 3 остановкой} ... {время в пути
//между X и 1 остановкой}
//... маршруты остальных автобусов...

//Пример:
//2
//4
//10:00 12:00
//10 20
//2 1 3 5 7
//3 1 2 4 10 5 20

//Факты:
//1. Остановки пронумерованы подряд от 1 до K.
//2. Время пути между остановками задается в минутах целым числом.
//3. Стоимость проезда задается в рублях целым числом.
//4. Автобусы друг другу не мешают.
//5. Автобус не тратит время на остановке (стоит 0 минут).
//6. Входной файл не содержит ошибок.
//7. Все автобусы пропадают в 00:00, т.е.все расчеты проходят до полуночи.

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

        private void DownloadToolStripMenuItem_Click(object sender, EventArgs e)//Метод загрузки
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


        private void Button1_Click(object sender, EventArgs e)//Генерируем транспортную карту переездов 
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
