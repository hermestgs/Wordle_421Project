using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Wordle
{
    public partial class MainWindow : Window
    {
        // Global Variables
        int CurrentRow = 0; // CurrentRow helps in keeping track of the row in which user is currently typing in
        int CurrentColumn = 0; // CurrentColumn helps in keeping track of the column or letter block on which user currently is
        string CurrentWinningWord = ""; // CurrentWinningWord is the word that user needs to guess

        public MainWindow()
        {
            InitializeComponent();
        }

        // MainWindow_Loaded function starts when the main window loads to setup functions
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetWinningWord();
        }

        // SetWinningWord function questions the user about the their preference about the word
        private void SetWinningWord()
        {
            RandomWordORCustomWordDialogBox.Visibility = Visibility.Visible;
        }

        // HandleKeyPress functions listens to the key presses on the window and fills the columns and rows accordingly
        private async void HandleKeyPress(object sender, KeyEventArgs e)
        {
            string PressedKey = e.Key.ToString();
            bool IsAplhaBet = (e.Key >= Key.A && e.Key <= Key.Z);

            // Checking if pressed key is alphanumeric and if current column is the last column of the row
            if (IsAplhaBet && CurrentColumn != 5)
            {
                // Looping through the MyGrid to get the correct column in the correct row, and then assigning the correct block with the pressed aplhabet
                MyGrid.Children.Cast<UIElement>().ToList().ForEach(e =>
                {
                    if (e is TextBlock block && Grid.GetColumn(e) == CurrentColumn && Grid.GetRow(e) == CurrentRow)
                    {
                        block.Text = PressedKey;
                    }
                });

                // Incrementing the CurrentColumn index
                CurrentColumn++;
                return;
            }

            if (e.Key == Key.Back)
            {
                // Checking if CurrentColumn is more than 0, this is to prevent user to glitch the system by making CurrentColumn index into negative integers
                if (CurrentColumn > 0)
                {
                    CurrentColumn--;
                }

                // Looping through the MyGrid to get the correct column in the correct row, and then assigning clearing the text inside the column
                MyGrid.Children.Cast<UIElement>().ToList().ForEach(e =>
                {
                    if (e is TextBlock block && Grid.GetColumn(e) == CurrentColumn && Grid.GetRow(e) == CurrentRow)
                    {
                        block.Text = "";
                    }
                });
                return;
            }

            if (e.Key == Key.Return)
            {
                // Checking if user is trying to submit the word before filling the whole row
                if (CurrentColumn == 5)
                {
                    await SubmitWord();
                }
                else
                {
                    ErrorsLabel.Content = "Not enough letters";
                    await Task.Delay(1000);
                    ErrorsLabel.Content = "";
                }
                return;
            }
        }

        // CheckIsWordValid function checks if given word exists in the word list, and returns a boolean value
        private static bool CheckIsWordValid(string Word)
        {
            // Loops through each line
            foreach (string line in File.ReadLines("./valid-words.txt"))
            {
                // return true, if given word matches a word in the file
                if (Word.ToLower().Trim() == line.ToLower().Trim())
                {
                    return true;
                };
            }
            
            return false;
        }

        // ColorTheColumns function changes the Foreground, BorderBrush, and Background of columns in a row
        private void ColorTheColumns(string InputWord, string CorrectWord)
        {
            int index = 0;
         
            MyGrid.Children.Cast<UIElement>().ToList().ForEach(e =>
            {
                // If a UIElement in MyGrid is TextBlock in the current row, then change it's foreground to white
                if (e is TextBlock block && Grid.GetRow(e) == CurrentRow)
                {
                    block.Foreground = new SolidColorBrush(Colors.White);
                }

                if (e is Border border && Grid.GetRow(e) == CurrentRow)
                {
                    // Check if a letter is in correct position if it is then change it's color to green
                    if (InputWord[index].ToString().ToLower() == CorrectWord[index].ToString().ToLower())
                    {
                        border.BorderBrush = new SolidColorBrush(Color.FromRgb(90, 212, 24));
                        border.Background = new SolidColorBrush(Color.FromRgb(90, 212, 24));

                        // Remove the letter for the CorrectWord to avoid duplicates and misleading information to the user
                        var regex = new Regex(Regex.Escape(InputWord[index].ToString().ToLower()));
                        // To avoid problem with index we simply change the letter with space
                        CorrectWord = regex.Replace(CorrectWord, " ", 1);
                    }
                    // If letter is not in correct place, but it does exists in the winning word then change it's color to yellow
                    else if (CorrectWord.ToLower().Contains(InputWord[index].ToString().ToLower()))
                    {
                        border.BorderBrush = new SolidColorBrush(Colors.Goldenrod);
                        border.Background = new SolidColorBrush(Colors.Goldenrod);

                        // Remove the letter for the CorrectWord to avoid duplicates and misleading information to the user
                        var regex = new Regex(Regex.Escape(InputWord[index].ToString().ToLower()));
                        // To avoid problem with index we simply change the letter with space
                        CorrectWord = regex.Replace(CorrectWord, " ", 1);
                    }
                    // If letters dones't passes the above conditions then change it's color to gray
                    else
                    {
                        border.BorderBrush = new SolidColorBrush(Colors.Gray);
                        border.Background = new SolidColorBrush(Colors.Gray);
                    }
                    // Incrementing the index to move on to the next letter
                    index++;
                }
            });
        }

        // SubmitWord function validates the word and checkes the state of game
        private async Task SubmitWord()
        {
            string InputWord = "";
            // CorrectWord is a dulicate of CurrentWinningWord
            string CorrectWord = CurrentWinningWord;

            // Looping through MyGrid, and if a element is TextBlock in CurrentRow then add it's text to InputWord
            MyGrid.Children.Cast<UIElement>().ToList().ForEach(e =>
            {
                if (e is TextBlock block && Grid.GetRow(e) == CurrentRow)
                {
                    InputWord += block.Text;
                }
            });

            // Checking if word exists in the word list
            if (!CheckIsWordValid(InputWord))
            {
                ErrorsLabel.Content = "Not in word list";
                await Task.Delay(1000);
                ErrorsLabel.Content = "";
                return;
            }

            ColorTheColumns(InputWord, CorrectWord);

            // If InputWord matches the CurrentWinningWord then change the text and color of ErrorsLabel, and at last calls RestartGame().
            if (InputWord.ToLower().Trim() == CurrentWinningWord.ToLower().Trim())
            {
                ErrorsLabel.Foreground = new SolidColorBrush(Color.FromRgb(90, 212, 24));
                ErrorsLabel.Content = "You Won!";
                RestartGame();
                return;
            }

            // If all 5 rows are filled, but no correct answer then user losses, it text changes and at last calls RestartGame().
            if (CurrentRow == 4)
            {
                ErrorsLabel.Content = "You Lost!";
                RestartGame();
                return;
            }

            // If above statements dones't becomes true then increment the CurrentRow and set CurrentColumn to 0
            CurrentColumn = 0;
            CurrentRow++;
        }

        // RestartGame function asks the user whether to quite the game or to restart, if user chooses retry. Then it resets the values of variables and elements.
        private void RestartGame()
        {
            // Preparing a MessageBox to ask user about whether they want to restart the game or not.
            string MessageBoxText = "Do you want to restart the game?";
            string Title = ErrorsLabel.Content.ToString();
            MessageBoxButton Buttons = MessageBoxButton.YesNo;
            MessageBoxImage Icon = MessageBoxImage.Question;
            MessageBoxResult DialogResult;
            DialogResult = MessageBox.Show(MessageBoxText, Title, Buttons, Icon, MessageBoxResult.Yes);

            if (DialogResult == MessageBoxResult.Yes)
            {
                // Looping through every element in MyGrid
                MyGrid.Children.Cast<UIElement>().ToList().ForEach(e =>
                {
                    // Setting element's appearances back to their default appearances
                    if (e is TextBlock block)
                    {
                        block.Text = "";
                        block.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    if (e is Border border)
                    {
                        border.BorderBrush = new SolidColorBrush(Colors.LightGray);
                        border.Background = new SolidColorBrush(Colors.White);
                    }
                });

                CurrentColumn = 0;
                CurrentRow = 0;
                ErrorsLabel.Content = "";
                CustomWordInputBox.Text = "";
                ErrorsLabel.Foreground = new SolidColorBrush(Colors.Red);
                
                // Removing key listener, to prevent a bug from listening key presses while user types for custom word in TextBox
                var window = Window.GetWindow(this);
                window.KeyDown -= HandleKeyPress;

                SetWinningWord();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        // RandomWordButton_Click is function selects a random word from the word list as CurrentWinningWord, and hiddes the RandomWordORCustomWordDialogBox to Collapsed, and at last also adds the keylistener to the window
        private void RandomWordButton_Click(object sender, RoutedEventArgs e)
        {
            string[] lines = File.ReadAllLines("./valid-words.txt");
            Random random = new Random();

            // Using two to account for the empty next line at the end of the text file
            int randomLineNumber = random.Next(0, lines.Length - 2);
            CurrentWinningWord = lines[randomLineNumber].ToLower().Trim();
            
            RandomWordORCustomWordDialogBox.Visibility = Visibility.Collapsed;

            var window = Window.GetWindow(this);
            window.KeyDown += HandleKeyPress;
        }

        // CustomWordButton_Click function changes the RandomWordORCustomWordDialogBox visibility to collapsed and CustomWordDialogBox visibility to visible
        private void CustomWordButton_Click(object sender, RoutedEventArgs e)
        {
            RandomWordORCustomWordDialogBox.Visibility = Visibility.Collapsed;
            CustomWordDialogBox.Visibility = Visibility.Visible;
        }

        // SaveCustomWordButton_Click saves the custom word accordingly to user's preference
        private async void SaveCustomWordButton_Click(object sender, RoutedEventArgs e)
        {
            // Cleaning the string from the CustomWordInputBox and saving it into CustomWord
            string CustomWord = CustomWordInputBox.Text.ToLower().Trim().ToString();

            // If CustomWord contains more than aplhabets like spaces or numbers then warn the user.
            if (!Regex.IsMatch(CustomWord, "^[a-zA-Z]+$"))
            {
                CustomWordDialogErrorsLabel.Content = "Only letters";
                await Task.Delay(2000);
                CustomWordDialogErrorsLabel.Content = "";
            }
            else if (CustomWord.Length == 5)
            {
                if (SaveInTheListRadioButton.IsChecked == true)
                {

                    using StreamWriter file = new("./valid-words.txt", append: true);
                    await file.WriteLineAsync(CustomWord);

                    CurrentWinningWord = CustomWord;
                }
                else
                {
                    // If user chooses the "Only use for once" then just save it into the variable
                    CurrentWinningWord = CustomWord;
                }
                CustomWordDialogBox.Visibility = Visibility.Collapsed;
                var window = Window.GetWindow(this);
                window.KeyDown += HandleKeyPress;
            }
            else{
                // We are only accounting for the less words because we are having max character limit in the TextBox
                CustomWordDialogErrorsLabel.Content = "Not enough letters";
                await Task.Delay(2000);
                CustomWordDialogErrorsLabel.Content = "";
            }
        }
    }
}