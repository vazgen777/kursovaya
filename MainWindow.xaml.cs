﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Koursach_Tri_v_Ryad
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int bSize = 60;
        BitmapImage[] typedpic = new BitmapImage[]
        {
            new BitmapImage(new Uri(@"pack://application:,,,/imgs/0.png", UriKind.Absolute)),
            new BitmapImage(new Uri(@"pack://application:,,,/imgs/1.png", UriKind.Absolute)),
            new BitmapImage(new Uri(@"pack://application:,,,/imgs/2.png", UriKind.Absolute)),
            new BitmapImage(new Uri(@"pack://application:,,,/imgs/3.png", UriKind.Absolute)),
            new BitmapImage(new Uri(@"pack://application:,,,/imgs/4.png", UriKind.Absolute)),
            new BitmapImage(new Uri(@"pack://application:,,,/imgs/5.png", UriKind.Absolute)),
        };

        Cell[,] FieldOfCells = new Cell[quantity, quantity]; //массив игровых ячеек
        GameLogic GameLog; //переменная класса игровой механики


        const int quantity = 8; //константа размера поля
        const int nulltipe = -99; //константа пустого типа ячеек
        const int missscore = 5 * ((quantity - 2) * 3 * quantity * 2); //константа погрешности при подсчете очков
        const int moves = 10; //константа числа ходов

        JsonSaveLoadProgress j = new JsonSaveLoadProgress(); // переменная класса сохранения и загрузки прогресса

        Player p; //переменная класса игрока
        List<Player> ratelist = new List<Player>(); //список рейтинга игроков

        bool gamestarted = false;

        //при запуске программы создается пустое поле
        public MainWindow()
        {
            ratelist.Clear();

            InitializeComponent();

            unigrid.Rows = quantity;
            unigrid.Columns = quantity;

            unigrid.Width = quantity * (bSize + 4);
            unigrid.Height = quantity * (bSize + 4);

            unigrid.Margin = new Thickness(5, 5, 5, 5);

            GameLog = new GameLogic(FieldOfCells);


        }

        //специальный метод для реализации автоматического падения
        private void Falled(object sender, EventArgs args)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                Update();
            });
        }

        //вызывается при каждом изменении данных о типе картинок в игровом поле и отображает эти изменения,
        //также отображает число очков и оставшиеся ходы
        //при истеченнии ходов очищает игровое поле
        void Update()
        {
            for (int i = 0; i < quantity; i++)
                for (int j = 0; j < quantity; j++)
                {
                    StackPanel stack = new StackPanel();

                    int typeel = FieldOfCells[i, j].typeofpic;
                    if (typeel != nulltipe)
                    {
                        BitmapImage image = typedpic[typeel];
                        stack = getPanel(image);

                    }

                    FieldOfCells[i, j].b.Content = stack;
                }

            totalscore.Content = Convert.ToString(GameLog.getScore() - missscore);
            Lefthod.Content = "Ходы: " + GameLog.leftmoves;

            if (GameLog.getLeftMoves() == 0)
            {
                unigrid.Children.Clear();
                gamestarted = false;
            }
        }

        StackPanel getPanel(BitmapImage picture)
        {
            StackPanel stackPanel = new StackPanel();
            Image image = new Image();
            image.Source = picture;
            stackPanel.Children.Add(image);
            stackPanel.Margin = new Thickness(1);

            return stackPanel;
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            int index = (int)(((Button)sender).Tag);

            int i = index % quantity;
            int j = index / quantity;

            GameLog.moveCell(i, j);

            totalscore.Content = Convert.ToString(GameLog.getScore() - missscore);
            Update();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {

            if (gamestarted == false)
            {
                string name = Convert.ToString(PlayerName.Content); //получает введенное имя игрока из лейбла имени 

                //если имя игрока присутсвует, то генерируется игровое поле 
                //иначе сообщается, что имя не было введено
                if (name != "Вы играете за: ")
                {
                    gamestarted = true;
                    for (int i = 0; i < quantity; i++)
                        for (int j = 0; j < quantity; j++)
                        {
                            FieldOfCells[i, j] = new Cell(nulltipe, i + j * quantity);
                            StackPanel stackPanel = new StackPanel();
                            stackPanel.Margin = new Thickness(1);
                            FieldOfCells[i, j].b.Click += Btn_Click;
                            unigrid.Children.Add(FieldOfCells[i, j].b);
                        }


                    GameLog.GameSetScore(0); //устанавливается начальный счет
                    GameLog.setMovesLeft(moves); //устанавливается начальное чилсо ходов
                    GameLog.Falled += Falled;
                    Update();
                    GameLog.StartFall();
                }
                else
                    MessageBox.Show("Введите имя");
            }
            else
            {
                MessageBox.Show("Игра уже начата");
            }
        }


        private void NameChange_Click(object sender, RoutedEventArgs e)
        {
            if (gamestarted == false)
            {
                bool check = true;
                AddName win2 = new AddName();
                if (win2.ShowDialog() == true)
                {

                    foreach (Player player in ratelist)
                        if (win2.Name.Text == player.name)
                        {
                            MessageBox.Show("Это имя занято");

                            check = false;
                        }
                    if (check == true)
                    {
                        GameLog.GameSetScore(0);
                        p = new Player(win2.Name.Text, 0);
                        PlayerName.Content = "Вы играете за: " + p.getName();
                    }
                }
            }
            else
            {
                MessageBox.Show("Игрок уже есть");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (gamestarted == false)
            {
                Rate.Items.Clear();
                p.setScore(Convert.ToInt32(totalscore.Content));

                ratelist.Add(p);

                var sortedPlayers = from r in ratelist
                                    orderby r.score descending
                                    select r;

                foreach (Player p in sortedPlayers)
                    Rate.Items.Add(p.name + ":     " + p.score);

                PlayerName.Content = "";
                totalscore.Content = "";

                j.SaveFile(ratelist);
            }
            else
            {
                MessageBox.Show("Дождитесь конца игры");
            }
 
        }

        private void Load_Click_1(object sender, RoutedEventArgs e)
        {
            if (gamestarted == false)
            {
                Rate.Items.Clear();

                ratelist = j.LoadFile();

                //сортировка списка рейтинга по убыванию числа набранных очков
                var sortedPlayers = from r in ratelist
                                    orderby r.score descending
                                    select r;

                foreach (Player p in sortedPlayers)
                    Rate.Items.Add(p.name + ":     " + p.score);
            }
            else
            {
                MessageBox.Show("Дождитесь конца игры");
            }

        }
    }
}
