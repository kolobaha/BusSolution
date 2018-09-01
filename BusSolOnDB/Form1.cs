using System;
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
        List<Bus> Buses = new List<Bus>();
        List<Move> Moves = new List<Move>();
        List<Transaction> TransportMap = new List<Transaction>();//Оперативная матрица переездов на текущий момент
        List<Transaction> BigTransportMap = new List<Transaction>();//Все возможные переезды

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

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

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
            BigTransportMap.Clear();
            int startTime = Convert.ToInt32(stTimeTB.Text);
            int startStation = Convert.ToInt32(startStTB.Text);
            TransportMap = GetTransportMap(Buses, Moves);
            InitializeGlobalMap(TransportMap, startTime);
        }
        public void GetTestData()
        {
            Buses.Add(new Bus(Buses.Count() + 1, 10, 10 * Constans.MinutesInHour));
            Buses.Add(new Bus(Buses.Count() + 1, 20, 12 * Constans.MinutesInHour));
            Moves.Add(new Move(1, 1, 3, 5));
            Moves.Add(new Move(1, 3, 1, 7));
            Moves.Add(new Move(2, 1, 2, 10));
            Moves.Add(new Move(2, 2, 4, 5));
            Moves.Add(new Move(2, 4, 1, 20));
            foreach (var Bus in Buses)//Рассчитываем периоды движения для автобусов
            {
                Bus.Period = Moves.Where(c => c.BusId == Bus.Id).Sum(c => c.Time);
            }
            // GetTransportMap(Buses, Moves);
        }
        private void EnterTestDataToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private List<Transaction> GetTransportMap(List<Bus> buses, List<Move> moves)//Входные параметры на случай ухода от глобальных списков
        {
            List<Transaction> initialTransportMap = new List<Transaction>();//Первичная карта переездов 
            int endTime = 0;
            foreach (var move in moves)
            {
                endTime = move.Time + Math.Max(initialTransportMap.Where(x => x.EndStation == move.StationFrom).Select(x => x.EndTime).FirstOrDefault(), buses.Where(x => x.Id == move.BusId).Select(x => x.StartTime).FirstOrDefault());
                if (endTime < Constans.MinutesInHour * Constans.HoursInDay)
                {
                    initialTransportMap.Add(new Transaction(
                          move.BusId,
                          move.StationFrom,
                          Math.Max(initialTransportMap.
                          Where(x => x.EndStation == move.StationFrom).
                          Select(x => x.EndTime).FirstOrDefault(),
                            buses.Where(x => x.Id == move.BusId).
                            Select(x => x.StartTime).FirstOrDefault()),
                          move.StationTo,
                          move.Time,
                          buses.Where(x => x.Id == move.BusId).Select(x => x.Cost).FirstOrDefault()));
                }
            }
            return initialTransportMap;
        }
        public void InitializeGlobalMap(List<Transaction> transportMap, int startTime)//Генерация полной карты переездов
        {
            int period;
            int iteration;
            foreach (var transact in transportMap)
            {
                iteration = 0;
                period = Buses.Where(x => x.Id == transact.BusId).Select(x => x.Period).FirstOrDefault();
                while (transact.EndTime + period * iteration <= 1440)
                {
                    if (transact.StartTime + period * iteration >= startTime)
                        BigTransportMap.Add(new Transaction(transact, period * iteration));
                    iteration++;
                }
            }
            BigTransportMap = BigTransportMap.OrderBy(x => x.StartTime).ToList();
            listBox2.Items.Clear();
            foreach (var transact in BigTransportMap)
            {
                listBox2.Items.Add(transact.ToString());
            }
        }
        //public List<Transaction> SolutionIteration(int startStation)
        //{
        //    List<Transaction> newTransportMap = new List<Transaction>();
        //    TransportMap = bigTransportMap.Where(x => x.StartStation == startStation).ToList();// Возможные пути отправления с указанной станции
        //    foreach (var transact in TransportMap)
        //    {
        //        var p = bigTransportMap.Where(x => x.StartStation == transact.EndStation).GroupBy(x => x.BusId);//Отбираем транзакции по следующей станции прибытия
        //        foreach (var potentialMove in p)//Берём следующий автобус по возможным передвижениям
        //        {
        //            newTransportMap.Add(potentialMove.FirstOrDefault(x => x.StartTime >= transact.EndTime));
        //        }
        //    }
        //    if (newTransportMap.Count == 0)
        //        MessageBox.Show("С данной станции в принципе не сущесвтует отправленя.");
        //    return newTransportMap;
        //}
        public void SolutionIteration(List<Transaction> oldTransportMap, int startStation)
        {
            List<Transaction> newTransportMap = new List<Transaction>();
            foreach (var transact in oldTransportMap.Where(x => x.StartStation == startStation))
            {
                var p = BigTransportMap.Where(x => x.StartStation == transact.EndStation).GroupBy(x => x.BusId);//Отбираем транзакции по следующей станции прибытия
                foreach (var potentialMove in p)//Берём следующий автобус по возможным передвижениям
                {
                    newTransportMap.Add(potentialMove.FirstOrDefault());
                }
            }
            oldTransportMap = newTransportMap;
        }
        public List<Transaction> Solution(int startStation, int endStation, int startTime)
        {
            if (BigTransportMap.Where(x => x.StartStation == startStation).Count() <= 0)
            {
                MessageBox.Show("Автобусы от заданной станции не ходят.");
                return null;
            }
            if (BigTransportMap.Where(x => x.EndStation == endStation).Count() <= 0)
            {
                MessageBox.Show("Доехать до станции назначения невозможно.");
                return null;
            }
            if (BigTransportMap.Where(x => x.StartTime >= startTime).Count() <= 0)
            {
                MessageBox.Show("Так поздно автобусы не ходят.");
                return null;
            }
            List<Transaction> transactionMatrix = new List<Transaction>();
            //После всех проверок генерируем карту переездов от станции отправления.
            var p = BigTransportMap.Where(x => x.StartStation == startStation).GroupBy(x => x.BusId);//Отбираем транзакции по следующей станции прибытия
            foreach (var potentialMove in p)//Берём следующий автобус по возможным передвижениям
            {
                transactionMatrix.Add(potentialMove.FirstOrDefault());
            }
            MessageBox.Show(transactionMatrix.Count().ToString());
            Transaction resultOnTimeTransaction = new Transaction() { Cost = int.MaxValue };
            Transaction resultOnCostTransaction = new Transaction() { CurrentWayLength = int.MaxValue };
            SolutionIteration(transactionMatrix, endStation, resultOnTimeTransaction, resultOnCostTransaction);
            transactionMatrix.Clear();
            transactionMatrix.Add(resultOnTimeTransaction);
            transactionMatrix.Add(resultOnCostTransaction);
            return transactionMatrix;
        }

        private void SolutionIteration(List<Transaction> oldTransactionMatrix, int endStation, Transaction CostMark, Transaction TimeMark)
        {
            // Проверка можно ли прямо сейчас доехать до станции назначения ? Если да , то записи на оценку и удалить
            var preResultTransactions = oldTransactionMatrix.Where(x => x.EndStation == endStation).ToList();
            Transaction newOnTimeTransaction = new Transaction() { Cost = int.MaxValue };
            Transaction newOnCostTransaction = new Transaction() { CurrentWayLength = int.MaxValue };
            foreach (var transact in preResultTransactions)
            {
                if (transact.PassedStations.Count <= TimeMark.CurrentWayLength)
                {
                    newOnTimeTransaction = transact;
                    newOnTimeTransaction.CurrentWayLength = transact.PassedStations.Count;//ToDo: Поменять логику работы с оценками. Плохо, что отдельным полем хранится то, что можно забрать со списка.
                }

                if (transact.Cost <= CostMark.Cost)
                {
                    newOnCostTransaction = transact;
                }

            }
            oldTransactionMatrix.RemoveAll(x => x.EndStation == endStation);
            if (oldTransactionMatrix.Count < 1) return; //Повторяем проверку, что можно уехать далее
            List<Transaction> newTransactionMatrix = new List<Transaction>();
            foreach (var oldTransact in oldTransactionMatrix) //Строим матрицу следующих переездов
            {
                var p = BigTransportMap.Where(x => x.StartStation == oldTransact.EndStation).GroupBy(x => x.BusId);//Отбираем транзакции по следующей станции прибытия
                foreach (var potentialMove in p)//Берём следующий автобус по возможным передвижениям
                {
                    Transaction potentialTransaction = potentialMove.FirstOrDefault();
                    if (potentialTransaction == null) continue;
                    var oldPassedStations = oldTransact.PassedStations;
                    var potentialPassedStations = potentialTransaction.PassedStations;
                    if (!(oldTransact.PassedStations.Contains(potentialTransaction.StartStation)))//Если таких станций не было
                        if (potentialTransaction.BusId != oldTransact.PassedBuses.GroupBy(x => x).LastOrDefault()?.Key)//Если мы не садимся на автобус, на котором ехали ранее
                        {
                            newTransactionMatrix.Add(potentialMove.FirstOrDefault());//Добавляем данную транзакцию ToDo: Проверка на новый автобус!
                            newTransactionMatrix[newTransactionMatrix.Count() - 1].PassedBuses.AddRange(oldTransact.PassedBuses);//Добавляем в нее данные о пройденных станциях и автобусах
                            newTransactionMatrix[newTransactionMatrix.Count() - 1].PassedBuses.Add(oldTransact.BusId);
                            if (newTransactionMatrix[newTransactionMatrix.Count() - 1].BusId != oldTransact.BusId)
                                newTransactionMatrix[newTransactionMatrix.Count() - 1].Cost += oldTransact.Cost;
                            newTransactionMatrix[newTransactionMatrix.Count() - 1].PassedStations.AddRange(oldTransact.PassedStations);//Добавляем в нее данные о пройденных станциях и автобусах
                            newTransactionMatrix[newTransactionMatrix.Count() - 1].PassedStations.Add(oldTransact.StartStation);

                        }
                }
            }

            //SolutionIteration(newTransactionMatrix, endStation, CostMark, TimeMark);//Выполняем новую итерацию.
        }

        public void SolutionIteration(int iteration)//На основании станций приезда формируем новую транспортную карту (Пока понимажаются оценки или пока их нет)
        {

            // Оценка записей о переездах, соответсвующие приезду в точку назначения.
            // На основании оставшихся записей строится новая матрица
            // Сделать проверку, чтобы переезды не пвторяли станции, на которых уже были через свойство PasssedStations
            // Каждой записи соотвествуют дальнейшие переезды ( с добавлением свойств PassedSt. и PassedBuses ) , а также с рачетом цены. 
            // Серии с конкретным автобусом должны быть единственны, т.е. ситуация A1,A1,A1,A2,A2,A1 - недопустима

            List<int> startStations = new List<int>();

            if (iteration != 1)
                startStations = TransportMap.Select(x => x.EndStation).ToList();
            else
            {
                startStations.Add(Convert.ToInt32(startStTB.Text));
            }

            List<Transaction> newIterationTransportMap = new List<Transaction>();
            foreach (var transact in TransportMap)
            {
                Transaction nextTransact;
                Transaction oldTransact;
                var p = BigTransportMap.Where(x => startStations.Contains(x.StartStation)).GroupBy(x => x.BusId);//Отбираем транзакции по следующей станции прибытия
                foreach (var potentialMove in p)//Берём следующий автобус по возможным передвижениям
                {
                    oldTransact = potentialMove.FirstOrDefault();
                    nextTransact = oldTransact;//Ошибка! Надо брать следующий переезд
                    nextTransact.PassedBuses.Add(oldTransact.BusId);
                    if (nextTransact.BusId != oldTransact.BusId)
                        nextTransact.Cost += oldTransact.Cost;
                    newIterationTransportMap.Add(nextTransact);
                }
            }
            TransportMap = newIterationTransportMap;
        }

        public void SolutionIteration(List<Transaction> oldTransportMap)
        {
            List<Transaction> newTransportMap = new List<Transaction>();
            Transaction newTransact = new Transaction();
            foreach (var transact in oldTransportMap)
            {
                var p = BigTransportMap.Where(x => x.StartStation == transact.EndStation).GroupBy(x => x.BusId);//Отбираем транзакции по следующей станции прибытия
                foreach (var potentialMove in p)//Берём следующий автобус по возможным передвижениям
                {
                    newTransportMap.Add(potentialMove.FirstOrDefault());
                }
            }
            oldTransportMap = newTransportMap;
        }
        public int GetStartTime()
        {
            return Convert.ToInt32(stTimeTB.Text);
        }
        public int GetStartStation()
        {
            return Convert.ToInt32(startStTB.Text);
        }
        public int GetEndStation()
        {
            return Convert.ToInt32(endStTB.Text);
        }
        private void IterateBut_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            foreach (var transact in Solution(GetStartStation(), GetEndStation(), GetStartTime()))
            {
                if (transact != null)
                    listBox2.Items.Add(transact.ToString());
            }
        }
    }
}
