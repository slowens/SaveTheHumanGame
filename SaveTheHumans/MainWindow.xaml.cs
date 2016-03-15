using System;
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

using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SaveTheHumans
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random random = new Random();                           //generator losowych liczb
        DispatcherTimer enemyTimer = new DispatcherTimer();     // obiekt zarządzający czasem wrogów
        DispatcherTimer targetTimer = new DispatcherTimer();    // obiekt zarządzający czasem gry
        bool humanCaptured = false;                             // zmienna boolowska określająca złapanie człowikea przez myszą

        public MainWindow()
        {
            InitializeComponent();

            enemyTimer.Tick += enemyTimer_Tick;
            enemyTimer.Interval = TimeSpan.FromSeconds(2);      // timer wrogów ustawiony na 2 sek czyli dodajemy ich co 2 sek

            targetTimer.Tick += targetTimer_Tick;
            targetTimer.Interval = TimeSpan.FromSeconds(0.1);   // timer gry ustawiony na 10 sek  / timer trwa z wartoscia 1 100 sek



        }

        void targetTimer_Tick(object sender, EventArgs e)       // metoda zapełniająca pasek z czasem gry
        {
            progressBar.Value += 1;                             // dodajemy do paska po 1
            if(progressBar.Value >= progressBar.Maximum)        // jesli pasek się zapełni  wywołujemy koniec gry
            {
                EndTheGame();
            }
        }

        private void EndTheGame()                               // metoda zakonczenia gry
        {

            if (!playArea.Children.Contains(gameOverText))      // jeśli na ekranie nie widać gameover
            {
                enemyTimer.Stop();                              // zastopuj timer wrogów
                targetTimer.Stop();                             // zastopuj timer gry
                humanCaptured = false;                          // zresetowanie zmiennej human caputred na false czyli nie złapany przez mysz
                startButton.Visibility = Visibility.Visible;    // pokazanie klawisza startu aby można zagrać jeszcze raz   
                playArea.Children.Add(gameOverText);            // Wyswietlenie napisu game over
            }

        }

        private void enemyTimer_Tick(object sender, EventArgs e) // zadeklarowanie metody timera obcego
        {
            AddEnemy();                                         // wywołanie metody dodania obcego
        }

        private void startButton_Click(object sender, RoutedEventArgs e)    // metoda dla zdarzenia klikniecia na przycisk start
        {
            StartGame();                                        // wywołanie metody rozpoczęcie gry
        }

        private void StartGame()                                // deklaracja metody rozpoczęcia gry
        {

            human.IsHitTestVisible = true;                      // pozycjonowanie człowieka na ekranie oznaczone jako mozliwe
            humanCaptured = false;                              // ustawienie człowieka na niezłapany przez mysz
            progressBar.Value = 0;                              // wyzerowanie paska czasu gry
            startButton.Visibility = Visibility.Collapsed;      // wyłącznie widoczności przycisku start
            playArea.Children.Clear();                          // wyczyszczenie pola gry z wszystkiego
            playArea.Children.Add(target);                      // wstawienie wyjscia
            playArea.Children.Add(human);                       // dodanie człowieka
            enemyTimer.Start();                                 // uruchomienie timera obcych
            targetTimer.Start();                                // uruchomienie timera gry

        }

        private void AddEnemy()                                 // metoda dodania wrogów
        {
         ContentControl enemy = new ContentControl();           // utworzenie nowego obiektu kontrolującego zawartość pola gry
            enemy.Template = Resources["EnemyTemplate"] as ControlTemplate;             // przypisanie wartości szablonu do obiektu contentcontrol
            AnimateEnemy(enemy, 0, playArea.ActualWidth - 100, "(Canvas.Left)");        // pozycjonowanie pojawienia się obcego       
            AnimateEnemy(enemy, random.Next((int)playArea.ActualHeight - 100), 
                random.Next((int)playArea.ActualHeight - 100), "(Canvas.Top)");
            playArea.Children.Add(enemy);                                               // dodanie obcego do pola gry 

            enemy.MouseEnter += Enemy_MouseEnter;               // najechanie człowiekiem na wroga powoduje koniec gry 

        }

        private void Enemy_MouseEnter(object sender, MouseEventArgs e)
        {
            if (humanCaptured)
            {
                EndTheGame();
            }
        }

        private void AnimateEnemy(ContentControl enemy, double from, double to, string propertyToAnimate)
        {
            Storyboard storyboard = new Storyboard() { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(random.Next(4, 6))),
            };

            Storyboard.SetTarget(animation, enemy);
            Storyboard.SetTargetProperty(animation, new PropertyPath(propertyToAnimate));
            storyboard.Children.Add(animation);
            storyboard.Begin();


        }

        private void human_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (enemyTimer.IsEnabled)
            {
                humanCaptured = true;                               // zmienna wskazuje że trzymamy człowieka /złapanie człowieka 
                human.IsHitTestVisible = false;
            }
        }

        private void target_MouseEnter(object sender, MouseEventArgs e)
        {
            if(targetTimer.IsEnabled && humanCaptured)
            {
                progressBar.Value = 0;
                Canvas.SetLeft(target, random.Next(100, (int)playArea.ActualWidth - 100));
                Canvas.SetTop(target, random.Next(100, (int)playArea.ActualHeight - 100));
                Canvas.SetLeft(human, random.Next(100, (int)playArea.ActualWidth - 100));
                Canvas.SetTop(human, random.Next(100, (int)playArea.ActualHeight - 100));
                humanCaptured = false;
                human.IsHitTestVisible = true;
            }
        }

        private void playArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (humanCaptured)
            {

                Point pointerPosition = e.GetPosition(null);
                Point relativePosition = grid.TransformToVisual(playArea).Transform(pointerPosition);
                if ((Math.Abs(relativePosition.X - Canvas.GetLeft(human)) > human.ActualWidth * 3) || 
                    (Math.Abs(relativePosition.Y - Canvas.GetTop(human)) > human.ActualHeight * 3))
                {
                    humanCaptured = false;
                    human.IsHitTestVisible = true;
                }
                else
                {
                    Canvas.SetLeft(human, relativePosition.X - human.ActualWidth / 2);          //odpowiada za zmianę pozycji postaci w pionie
                    Canvas.SetTop(human, relativePosition.Y - human.ActualHeight / 2);          //odpowiada za zmianę pozycji postaci w poziomie
                }

            }
        }

        private void playArea_MouseLeave(object sender, MouseEventArgs e)
        {
            if (humanCaptured)
            {
                EndTheGame();
            }
        }
    }
}
