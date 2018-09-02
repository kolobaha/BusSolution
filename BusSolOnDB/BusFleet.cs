using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusSolOnDB
{
    public class BusFleet
    {
        public int StationCount { get; set; }
        public int BusesCount { get; set; }
        public List<Bus> Buses { get; set; }
        public List<Move> Moves { get; set; }
        public List<Transaction> TransportMap { get; set; }//Оперативная матрица переездов на текущий момент
        public List<Transaction> BigTransportMap { get; set; } //Все возможные переезды
        public BusFleet()
        {
            Buses = new List<Bus>();
            Moves = new List<Move>();
            TransportMap = new List<Transaction>();
            BigTransportMap = new List<Transaction>();
            GetTestData();
            InitializeBigTransportMap(0);
        }
        public void GetTestData()
        {
            Buses.Clear();
            Moves.Clear();
            Buses.Add(new Bus(Buses.Count() + 1, 10, 10 * Constans.MinutesInHour));
            Buses.Add(new Bus(Buses.Count() + 1, 20, 12 * Constans.MinutesInHour));
            Moves.Add(new Move(1, 1, 3, 5));
            Moves.Add(new Move(1, 3, 1, 7));
            Moves.Add(new Move(2, 1, 2, 10));
            Moves.Add(new Move(2, 2, 4, 5));
            Moves.Add(new Move(2, 4, 1, 20));
            SetPeriods();
            StationCount = 5;
        }
        public void SetPeriods()
        {
            foreach (var Bus in Buses)//Рассчитываем периоды движения для автобусов
            {
                Bus.Period = Moves.Where(c => c.BusId == Bus.Id).Sum(c => c.Time);
            }
        }
        public bool IsWaysExist(List<Transaction> transactionMap, int startStation, int endStation, int startTime)
        {
            if (transactionMap.Where(x => x.StartStation == startStation).Count() <= 0)
            {
                //("Автобусы от заданной станции не ходят.");
                return false;
            }
            if (transactionMap.Where(x => x.EndStation == endStation).Count() <= 0)
            {
                // MessageBox.Show("Доехать до станции назначения невозможно.");
                return false;
            }
            if (transactionMap.Where(x => x.StartTime >= startTime).Count() <= 0)
            {
                //  MessageBox.Show("Так поздно автобусы не ходят.");
                return false;
            }
            return true;
        }
        public void InitializeBigTransportMap(int startTime)//Генерация полной карты переездов
        {
            BigTransportMap = new List<Transaction>();
            var transportMap = GetTransportMap(Buses, Moves);
            int period;
            int iteration;
            foreach (var transact in transportMap)
            {
                iteration = 0;
                period = Buses.Where(x => x.Id == transact.BusId).Select(x => x.Period).FirstOrDefault();
                while (transact.EndTime + period * iteration <= 1440)
                {
                    if ((transact.StartTime + period * iteration) >= startTime)
                        BigTransportMap.Add(new Transaction(transact, period * iteration));
                    iteration++;
                }
            }
            BigTransportMap = BigTransportMap.OrderBy(x => x.StartTime).ToList();
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
        public void ReadData(StreamReader myStream)// Метод парсинга текстового файла для получения исходных данных к задаче
        {
            string lineFile;
            List<String> data = new List<String>();
            do
            {
                lineFile = myStream.ReadLine();
                data.Add(lineFile);
            }
            while (lineFile != null);
            BusesCount = Convert.ToInt32(data[0]);
            StationCount = Convert.ToInt32(data[1]);
            Moves.Clear();
            Buses.Clear();
            var busesStarts = data[2].Split(' ').Select(x => Convert.ToInt32(Convert.ToDateTime(x).Hour * Constans.MinutesInHour + Convert.ToDateTime(x).Minute)).ToList();
            var costs = data[3].Split(' ').Select(x => Convert.ToInt32(x)).ToList();
            for (int i = 1; i <= BusesCount; i++)
            {
                Buses.Add(new Bus(i, costs[i - 1], busesStarts[i - 1]));
            }
            int currentBus = 1;
            for (int i = 4; i < BusesCount + 4; i++)
            {
                var moves = data[i].Split(' ');
                int count = Convert.ToInt32(moves[0]);
                List<int> stations = new List<int>();
                for (int j = 1; j < count + 1; j++)//Станции, которые входят в конкретный маршрут
                {
                    stations.Add(Convert.ToInt32(moves[j]));
                }
                List<int> times = new List<int>();
                for (int k = count + 1; k < moves.Count(); k++)//Время переездов между станциями
                {
                    times.Add(Convert.ToInt32(moves[k]));
                }
                for (int u = 0; u < count - 1; u++)
                {
                    Moves.Add(new Move(currentBus, stations[u], stations[u + 1], times[u]));
                }
                Moves.Add(new Move(currentBus, stations[stations.Count - 1], stations[0], times[times.Count - 1]));
                currentBus++;
            }
            SetPeriods();
        }
        public void InitializeMarks(ref Transaction LenghtMark, ref Transaction CostMark)
        {
            CostMark = new Transaction();
            CostMark.Cost = int.MaxValue;
            LenghtMark = new Transaction();
            if (StationCount > 0)
            {
                for (int i = 1; i <= StationCount; i++)
                {
                    LenghtMark.PassedStations.Add(i);
                }
            }
            LenghtMark.PassedStations.Add(StationCount + 1);
        }
        public List<Transaction> Solution(int startStation, int endStation, int startTime)
        {
            if (!IsWaysExist(BigTransportMap, startStation, endStation, startTime))
            {
                return new List<Transaction>() ;
            }
            List<Transaction> resultMatrix = new List<Transaction>();//После всех проверок генерируем карту переездов от станции отправления.
            var p = BigTransportMap.Where(x => x.StartStation == startStation).GroupBy(x => x.BusId);//Отбираем транзакции по следующей станции прибытия
            foreach (var potentialMove in p)//Берём следующий автобус по возможным передвижениям
            {
                resultMatrix.Add(potentialMove.FirstOrDefault());
            }
            Transaction resultOnCostTransaction = new Transaction();
            Transaction resultOnLenghtTransaction = new Transaction(); // Для данной оценки задать начальное значение, как список станций + 1 
            InitializeMarks(ref resultOnLenghtTransaction, ref resultOnCostTransaction);
            SolutionIteration(resultMatrix, endStation, ref resultOnLenghtTransaction, ref resultOnCostTransaction);
            resultMatrix.Clear();
            resultMatrix.Add(resultOnLenghtTransaction);
            resultMatrix.Add(resultOnCostTransaction);
            return resultMatrix;
        }
        private void SolutionIteration(List<Transaction> oldTransactionMatrix, int endStation, ref Transaction TimeMark, ref Transaction CostMark)
        {
            // Проверка можно ли прямо сейчас доехать до станции назначения ? Если да , то записи на оценку и удалить
            var preResultTransactions = oldTransactionMatrix.Where(x => x.EndStation == endStation).ToList();

            foreach (var transact in preResultTransactions)
            {
                if (transact.PassedStations.Count <= TimeMark.PassedStations.Count)
                {
                    TimeMark = transact;
                }
                if (transact.Cost <= CostMark.Cost)
                {
                    CostMark = transact;
                }
            }
            oldTransactionMatrix.RemoveAll(x => x.EndStation == endStation);//Избавляемся от тех, что доехали до конца.
            if (oldTransactionMatrix.Count < 1) return; //Повторяем проверку, что можно уехать далее
            List<Transaction> newTransactionMatrix = new List<Transaction>();
            foreach (var oldTransact in oldTransactionMatrix) //Строим матрицу следующих переездов
            {
                var p = BigTransportMap.Where(x => x.StartStation == oldTransact.EndStation && x.StartTime >= oldTransact.EndTime).GroupBy(x => x.BusId);//Отбираем транзакции по следующей станции прибытия
                foreach (var potentialMove in p)//Берём следующий автобус по возможным передвижениям
                {
                    Transaction potentialTransaction = potentialMove.FirstOrDefault();
                    if (potentialTransaction == null) continue;
                    if (!(oldTransact.PassedStations.Contains(potentialTransaction.StartStation)))//Если таких станций не было
                        if (!(oldTransact.PassedBuses.Contains(potentialTransaction.BusId) || potentialTransaction.BusId == oldTransact.PassedBuses.LastOrDefault()))//Если мы не садимся на автобус, на котором катались в прошлом 
                        {
                            int Cost = oldTransact.Cost;
                            if (potentialTransaction.BusId != oldTransact.BusId) Cost += potentialTransaction.Cost;
                            potentialTransaction.PassedBuses.AddRange(oldTransact.PassedBuses);
                            potentialTransaction.PassedBuses.Add(oldTransact.BusId);
                            potentialTransaction.PassedBuses.Distinct();
                            potentialTransaction.Cost = Cost;
                            potentialTransaction.PassedStations.AddRange(oldTransact.PassedStations);
                            potentialTransaction.PassedStations.Add(oldTransact.StartStation);
                            int bus = oldTransact.BusId;
                            potentialTransaction.Buses.Add(bus);
                            newTransactionMatrix.Add(potentialTransaction);//Добавляем данную транзакцию ToDo: Проверка на новый автобус!
                        }
                }
            }
            if (newTransactionMatrix.Count > 0) SolutionIteration(newTransactionMatrix, endStation, ref TimeMark, ref CostMark);//Выполняем новую итерацию.
        }

    }
}
