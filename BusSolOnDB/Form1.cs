﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Колобаха\source\repos\BusSolOnDB\BusSolOnDB\BusFleet.mdf;Integrated Security=True";
        SqlConnection sqlConnection = new SqlConnection(connectionString); // Подключение к локальной базе данных
        List<Bus> Buses = new List<Bus>();
        List<Move> Moves = new List<Move>();
        List<Transaction> TransportMap = new List<Transaction>();
        List<Transaction> bigTransportMap = new List<Transaction>();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            GetTestData();
            listBox1.Items.Add("Buses:");
            foreach (var bus in Buses)
            {
                listBox1.Items.Add(bus.ToString());
            }
            listBox3.Items.Add("Ways:");
            foreach (var move in Moves)
            {
                listBox3.Items.Add(move.ToString());
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                sqlConnection.Close();
        }

        private void DownloadToolStripMenuItem_Click(object sender, EventArgs e)//Метод загрузки
        {
            StreamReader myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
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
                            EnterData(myStream);
                            MessageBox.Show(openFileDialog1.FileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }
        public void EnterData(StreamReader myStream)// Метод парсинга текстового файла для получения исходных данных к задаче
        {
            string lineFile;
            List<String> data = new List<String>();
            do
            {
                lineFile = myStream.ReadLine();
                data.Add(lineFile);
            }
            while (lineFile != null);
            int countBus = Convert.ToInt32(data[0]);
            int countStation = Convert.ToInt32(data[1]);
            string[] timesArrive = data[2].Split(' ');
            string[] costs = data[3].Split(' ');
        }

        private async void Button1_Click(object sender, EventArgs e)//Генерируем транспортную карту переездов 
        {
            int startTime = Convert.ToInt32(stTimeTB.Text);
            GetGlobalMap(startTime);


        }
        public void GetTestData()
        {
            Buses.Add(new Bus(Buses.Count() + 1, 10, 10 * 60));
            Buses.Add(new Bus(Buses.Count() + 1, 20, 12 * 60));
            Moves.Add(new Move(1, 1, 3, 5));
            Moves.Add(new Move(1, 3, 1, 7));
            Moves.Add(new Move(2, 1, 2, 10));
            Moves.Add(new Move(2, 2, 4, 5));
            Moves.Add(new Move(2, 4, 1, 20));

            foreach (var Bus in Buses)//Рассчитываем периоды движения для автобусов
            {
                Bus.Period = Moves.Where(c => c.BusId == Bus.Id).Sum(c => c.Time);
            }
            GetTransportMap(Buses, Moves);
        }
        private void EnterTestDataToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void GetTransportMap(List<Bus> buses, List<Move> moves)//Входные параметры на случай ухода от глобальных списков
        {
            int endTime = 0;
            foreach (var move in moves)
            {
                endTime = move.Time + Math.Max(TransportMap.Where(x => x.EndStation == move.StationFrom).Select(x => x.EndTime).FirstOrDefault(), buses.Where(x => x.Id == move.BusId).Select(x => x.StartTime).FirstOrDefault());
                if (endTime < 60 * 24)//Добавляем только те переезды, что осуществляюстя до 24:00
                {
                    TransportMap.Add(new Transaction(
                          move.BusId,
                          move.StationFrom,
                          Math.Max(TransportMap.
                          Where(x => x.EndStation == move.StationFrom).
                          Select(x => x.EndTime).FirstOrDefault(),
                            buses.Where(x => x.Id == move.BusId).
                            Select(x => x.StartTime).FirstOrDefault()),
                          move.StationTo,
                          move.Time,
                          buses.Where(x => x.Id == move.BusId).Select(x => x.Cost).FirstOrDefault()));
                    listBox2.Items.Add(TransportMap[TransportMap.Count() - 1].ToString());
                }
            }

        }
        public void GetGlobalMap(int startTime)//Генерация полной карты переездов
        {
            int period;
            int iteration;
            foreach (var transact in TransportMap)
            {
                iteration = 0;
                period = Buses.Where(x => x.Id == transact.BusId).Select(x => x.Period).FirstOrDefault();
                while (transact.EndTime + period * iteration <= 1440)
                {
                    if (transact.StartTime + period * iteration >= startTime)
                        bigTransportMap.Add(new Transaction(transact, period * iteration));
                    iteration++;
                }
            }
            bigTransportMap = bigTransportMap.OrderBy(x => x.StartTime).ToList();
            listBox2.Items.Clear();
            foreach (var transact in bigTransportMap)
            {
                listBox2.Items.Add(transact.ToString());
            }
        }
    }
}