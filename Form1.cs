using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HorseRaceGame
{
    public partial class Form1 : Form
    {
        RiderActions[] riders; // Массив с наездниками
        RadioButton[] radioButtons; // Массив с RadioButtons
        ProgressBar[] progressBars; // Массив с ProgressBar
        Random rnd = new Random(); // Генератор случайных чисел
        List<int>[] coveredDistance; // Массив из списков, содержащих пройденное расстояние для каждого наездника

        int cntRiders = 5; // Количество наездников = 5
        int cntRaces = 5; // Количество заездов = 5
        int currentRace = 0; // Текущий заезд (изначально 0)
        int distance = 50; // Дистанция заезда - 50 км (по умолчанию 50)
        int[] arrNumbers; // Массив с случайными числами (номера наездников)
        int choice; // Выбранный наездник
        bool correctChoice = false; // Был ли выбранный наездник победителем

        // Путь к текстовому файлу "history.txt" в текущей рабочей директории
        string historyPath = $"{Directory.GetCurrentDirectory()}\\history.txt";
        // Путь к текстовому файлу "readme.txt" в текущей рабочей директории
        string instructionPath = $"{Directory.GetCurrentDirectory()}\\readme.txt";

        int fileLineCount = 1; // Количество строк в текстовом файле для history.txt (изначально 1)

        public Form1()
        {
            InitializeComponent();

            // Задаем импровизированную дистанцию (max значение для всех progressBar)
            progressBars = new ProgressBar[]
            { progressBar1, progressBar2, progressBar3, progressBar4, progressBar5 };
            foreach (ProgressBar progressBar in progressBars)
            {
                progressBar.Minimum = 0;
                progressBar.Maximum = distance;
            }

            // Создаём массив с {кол-во наездников} случайными значениями от 1 до 30
            arrNumbers = new int[cntRiders];
            for (int i = 0; i < arrNumbers.Length; i++)
            {
                int num;
                do
                {
                    num = rnd.Next(1, 31);
                }
                while (Array.IndexOf(arrNumbers, num) != -1);
                arrNumbers[i] = num;
            }

            // Создаем наездников и задаем им номера
            riders = new RiderActions[cntRiders];
            for (int i = 0; i < riders.Length; i++) riders[i] = new RiderActions(arrNumbers[i]);
            int riderIndex = 0;
            foreach (RiderActions rider in riders)
            {
                rider.Number = arrNumbers[riderIndex++];
            }

            // Задаем номера в RadioButton
            radioButtons = new RadioButton[]
            { radioButton1, radioButton2, radioButton3, radioButton4, radioButton5 };
            int buttonIndex = 0;
            foreach (RadioButton radioButton in radioButtons)
            {
                radioButton.Text = $"#{arrNumbers[buttonIndex++]}";
            }

            // Создаем/Перезаписываем текстовый документ с историей
            File.WriteAllText(historyPath, String.Empty);

            // Обновляем таблицу лидеров
            labelLeaderboardPoints.Text = ShowRating(riders);

            // Блокируем кнопку "Следующий заезд"
            buttonNextRound.Enabled = false;
        }

        private static string ShowRating(RiderActions[] riders)
        {
            var sortedRiders = riders.OrderByDescending(r => r.Points);
            string text = "";
            foreach (RiderActions rider in sortedRiders)
            {
                text += $"#{rider.Number} - {rider.Points} б.\n";
            }
            return text;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            // Отключаем кнопку "Старт" и "Новая игра"
            buttonStart.Enabled = false;
            buttonNewGame.Enabled = false;

            // Отключаем возможность выбора наездника
            foreach (RadioButton radioButton in radioButtons)
            {
                radioButton.Enabled = false;
            }

            // Включаем кнопку "Следующий заезд"
            buttonNextRound.Enabled = true;

            // Выбираем наездника
            foreach (RadioButton radioButton in radioButtons)
            {
                if (radioButton.Checked)
                {
                    choice = int.Parse(radioButton.Text.Substring(1));
                    break;
                }
            }

            // Вывод сообщения о начале игры с информацией о выбранном наезднике
            string textStart = $"Вы выбрали наездника #{choice}";
            MessageBox.Show(textStart, "Начало игры");

            // Создание экземпляров наездников и обновление таблицы лидеров
            for (int i = 0; i < riders.Length; i++) riders[i] = new RiderActions(arrNumbers[i]);
            labelLeaderboardPoints.Text = ShowRating(riders);

            coveredDistance = new List<int>[cntRiders]; // Массив из {кол-во наездников} списков
        }

        private void buttonNextRound_Click(object sender, EventArgs e)
        {
            if (currentRace < cntRaces)
            {
                if (currentRace == cntRaces - 1)
                {
                    buttonNextRound.Text = "Подвести итоги";
                }
                else buttonNextRound.Text = "Следующий заезд";

                currentRace++;
                labelRoundNum.Text = $"Номер заезда: {currentRace}";

                for (int i = 0; i < riders.Length; i++) // Проходимся по каждому наезднику
                {
                    coveredDistance[i] = new List<int>(); // Обнуляем список
                    riders[i].Position = 0; // Обнуляем позицию наездника
                    while (riders[i].Position < distance) // Пока наездник не пройдёт {distance}
                    {
                        riders[i].Move(rnd); // Перемещаем наездника
                        coveredDistance[i].Add(riders[i].Position); // Добавляем текущую позицию в список
                    }

                    // Если дистанция >{distance}, то приравниваем к {distance}.
                    coveredDistance[i][coveredDistance[i].Count - 1] -= (coveredDistance[i][coveredDistance[i].Count - 1] % distance);

                    // Обновляем позицию наездника, чтобы избежать ошибки, когда значение ProgressBar выходит за пределы дистанции
                    riders[i].Position = coveredDistance[i][coveredDistance[i].Count - 1];
                }

                // Узнаем минимальное количество ходов победителя.
                int minLength = coveredDistance.Min(list => list.Count);

                // Выдаем баллы победителям заезда
                int riderIndex = 0;
                foreach (RiderActions rider in riders)
                {
                    if (coveredDistance[riderIndex].Count() == minLength)
                    {
                        riders[riderIndex].UpdatePoints();
                    }
                    riderIndex++;
                }

                // Заполняем progressBars результатами заезда
                for (int i = 0; i < minLength; i++)
                {
                    for (int j = 0; j < progressBars.Length; j++)
                    {
                        progressBars[j].Value = coveredDistance[j][i];
                    }
                }

                // Обновляем текстовое поле с рейтингом наездников.
                labelLeaderboardPoints.Text = ShowRating(riders);
            }
            else
            {
                // Находим максимальное количество очков среди всех наездников.
                int maxPoints = riders.Max(rider => rider.Points);

                // Устанавливаем IsWinner в true у всех, у кого количество очков равно максимальному
                foreach (RiderActions rider in riders.Where(rider => rider.Points == maxPoints))
                {
                    if (rider.Number == choice) correctChoice = true;
                    rider.IsWinner = true;
                }

                // Возвращаем текст кнопки обратно
                buttonNextRound.Text = "Начать заезд";

                // Вывод списка победителей
                string listOfWinners = $"Вы поставили ставку на #{choice}\n" + 
                                       $"Список победителей:\n";
                foreach (RiderActions rider in riders)
                {
                    if (rider.IsWinner)
                    {
                        listOfWinners += $"#{rider.Number}\n";
                    }
                }

                // Формируем и выводим сообщение в MessageBox, добавляем строку в файл с историей
                string textGameOver;
                string title;
                if (correctChoice)
                {
                    title = "Победа";
                    textGameOver = $"{listOfWinners}\nВаш наездник победил!";
                }
                else
                {
                    title = "Поражение";
                    textGameOver = $"{listOfWinners}\nВаш наездник проиграл!";
                }
                MessageBox.Show(textGameOver, title);
                File.AppendAllText(historyPath, $"{fileLineCount++}. {title}\n");

                currentRace = 0;
                labelRoundNum.Text = $"Номер заезда: {currentRace}";

                // Включаем кнопку "Старт" и "Новая игра"
                buttonStart.Enabled = true;
                buttonNewGame.Enabled = true;

                // Отключаем кнопку "Следующий заезд";
                buttonNextRound.Enabled = false;

                // Возвращаем значение correctChoice на false
                correctChoice = false;

                // Включаем возможность выбора наездника
                foreach (RadioButton radioButton in radioButtons)
                {
                    radioButton.Enabled = true;
                }

                // Сбрасываем значения в progressBar
                foreach (ProgressBar progressBar in progressBars)
                {
                    progressBar.Value = 0;
                }

                // Обнуляем очки и обновляем таблицу участников
                foreach (RiderActions rider in riders)
                {
                    rider.Points = 0;
                }
                labelLeaderboardPoints.Text = ShowRating(riders);
            }
        }

        private void buttonNewGame_Click(object sender, EventArgs e)
        {
            // Обновляем массив с {кол-во наездников} случайными значениями от 1 до 30
            for (int i = 0; i < arrNumbers.Length; i++)
            {
                int num;
                do
                {
                    num = rnd.Next(1, 31);
                }
                while (Array.IndexOf(arrNumbers, num) != -1);
                arrNumbers[i] = num;
            }

            // Задаем наездникам новые номера
            int riderIndex = 0;
            foreach (RiderActions rider in riders)
            {
                rider.Number = arrNumbers[riderIndex++];
            }

            // Задаем новые номера в RadioButton
            int buttonIndex = 0;
            foreach (RadioButton radioButton in radioButtons)
            {
                radioButton.Text = $"#{arrNumbers[buttonIndex++]}";
            }

            // Обновляем таблицу участников
            labelLeaderboardPoints.Text = ShowRating(riders);
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            // Окно с инструкцией
            string instructionText = File.ReadAllText(instructionPath);
            MessageBox.Show(instructionText, "Инструкция", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void buttonHistory_Click(object sender, EventArgs e)
        {
            // Окно с историей игр
            MessageBox.Show(File.ReadAllText(historyPath), "История игр");
        }
    }

    class Rider
    {
        public int Number;
        public int Position;
        public int Points;
        public bool IsWinner;

        public Rider(int number)
        {
            Number = number;
            Position = 0;
            Points = 0;
            IsWinner = false;
        }
    }

    class RiderActions : Rider
    {
        public RiderActions(int number) : base(number)
        {
            // Инициализация полей из базового класса Rider
        }

        public void Move(Random rnd)
        {
            Position += rnd.Next(1, 6); // Кол-во км за один ход (от 1 до 5)
        }

        public void UpdatePoints()
        {
            Points += 5;
        }
    }
}
