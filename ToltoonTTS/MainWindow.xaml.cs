using System.Diagnostics;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using ToltoonTTS.Scripts;
using ToltoonTTS.Scripts.IndividualVoices;
using System.Windows.Media;
using ToltoonTTS.Scripts.Twitch;
using ToltoonTTS.Scripts.GoodGame;

namespace ToltoonTTS
{
    public partial class MainWindow : Window
    {   //Доп. окна
        IndividualVoices individualVoices;
        SpeechSynthesizer Synth = new SpeechSynthesizer();

        bool isDuplicate;

        bool ConnectToTwitch;

        public MainWindow()
        {
            TextToSpeech.CanRemoveEmoji = false;
            SaveContainers.CreateFiles();
            InitializeComponent();
            LoadContainers.ReadJsonProfiles(comboBoxProfileSelect);
            AddVoiceToPool.InstalledVoices = new List<string>();
            TwitchConnection.TwitchBlackList = new ListBox();
            TwitchConnection.LabelTwitchStatusMessage = LabelTwitchStatusMessage;
            GoodGameConnection.LabelGoodgameStatusMessage = LabelGoodgameStatusMessage;

            //загрузка данных в listbox
            ListBoxBlackList = LoadContainers.LoadBlackListUser(ListBoxBlackList, "DataForProgram/BlackList/BlackListUsers.json");
            ListBoxWhatToReplace = LoadContainers.LoadBlackListUser(ListBoxWhatToReplace, "DataForProgram/WordReplace/WhatToReplace.json");
            ListBoxWhatToReplaceWith = LoadContainers.LoadBlackListUser(ListBoxWhatToReplaceWith, "DataForProgram/WordReplace/WhatToReplaceWith.json");

            TextToSpeech.WhatToReplace = ListBoxWhatToReplace.Items.Cast<string>().ToList();
            TextToSpeech.WhatToReplaceWith = ListBoxWhatToReplaceWith.Items.Cast<string>().ToList();

            Zaglushka();
            foreach (var items in ListBoxBlackList.Items)
            {
                TwitchConnection.TwitchBlackList.Items.Add(items);
            }
        }
        //из-за отсутствия некоторых значений программа может не запуститься
        private void Zaglushka()
        {
            if (comboBoxInstalledVoices.SelectedIndex == -1)
                comboBoxInstalledVoices.SelectedIndex = 0;
            if (sliderTtsSpeedValue == null || sliderTtsSpeedValue.Value == 0)
                sliderTtsVolumeValue.Value = 50;
            //if (TextBoxTtsDoNotTtsIfStartWith.Text == "" || TextBoxTtsDoNotTtsIfStartWith.Text == null)
            //    TextBoxTtsDoNotTtsIfStartWith.Text = "!";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveEverythingOnClosing();
            Application.Current.Shutdown();
        }


        private void SaveEverythingOnClosing()
        {
            Properties.Settings.Default.TwitchApi = PasswordboxTwitchApi.Password;
            Properties.Settings.Default.TwitchID = TextboxTwitchClientId.Text;
            Properties.Settings.Default.TwitchNickname = TextboxTwitchNickName.Text;
            Properties.Settings.Default.RemoveEmoji = checkBoxRemoveEmoji.IsChecked ?? false;
            Properties.Settings.Default.InstalledVoiceSelect = comboBoxInstalledVoices.Text;
            Properties.Settings.Default.TtsVolumeValue = (int)sliderTtsVolumeValue.Value;
            Properties.Settings.Default.TtsSpeedValue = (int)sliderTtsSpeedValue.Value;
            Properties.Settings.Default.IndividualVoices = checkBoxVoiceIndividualVoices.IsChecked ?? false;
            Properties.Settings.Default.CheckboxConnectToTwitch = CheckBoxConnectToTwitch.IsChecked ?? false;
            Properties.Settings.Default.TextBoxDoNotTtsIfStartWith = TextBoxTtsDoNotTtsIfStartWith.Text;
            Properties.Settings.Default.TextBoxMessageSkip = TextBoxSkipMessage.Text;
            Properties.Settings.Default.TextBoxMessageSkipAll = TextBoxSkipMessageAll.Text;
            Properties.Settings.Default.TextBoxGoodgameLogin = TextBoxGoodgameLogin.Text;
            Properties.Settings.Default.CheckBoxConnectToGoodgame = CheckBoxConnectToGoodgame.IsChecked ?? false;
            Properties.Settings.Default.TextBoxIndividualVoicesChannelPoints = TextBoxIndividualVoicesChannelPoints.Text;
            Properties.Settings.Default.Save();

            //сохранение содержимого чёрного списка, замены слов и чёрного списка букв
            SaveContainers.SaveTextModifier(ListBoxBlackList, ListBoxWhatToReplace, ListBoxWhatToReplaceWith);
        }

        private void StackPanel_Initialized(object sender, EventArgs e)
        {
            PasswordboxTwitchApi.Password = Properties.Settings.Default.TwitchApi;
            TextboxTwitchClientId.Text = Properties.Settings.Default.TwitchID;
            TextboxTwitchNickName.Text = Properties.Settings.Default.TwitchNickname;
            checkBoxRemoveEmoji.IsChecked = Properties.Settings.Default.RemoveEmoji;
            comboBoxInstalledVoices.SelectedValue = Properties.Settings.Default.InstalledVoiceSelect;
            sliderTtsVolumeValue.Value = Properties.Settings.Default.TtsVolumeValue;
            sliderTtsSpeedValue.Value = Properties.Settings.Default.TtsSpeedValue;
            checkBoxVoiceIndividualVoices.IsChecked = Properties.Settings.Default.IndividualVoices;
            CheckBoxConnectToTwitch.IsChecked = Properties.Settings.Default.CheckboxConnectToTwitch;
            TextBoxTtsDoNotTtsIfStartWith.Text = Properties.Settings.Default.TextBoxDoNotTtsIfStartWith;
            TextBoxSkipMessage.Text = Properties.Settings.Default.TextBoxMessageSkip;
            TextBoxSkipMessage.Text = Properties.Settings.Default.TextBoxMessageSkip;
            TextBoxSkipMessageAll.Text = Properties.Settings.Default.TextBoxMessageSkipAll;
            TextBoxGoodgameLogin.Text = Properties.Settings.Default.TextBoxGoodgameLogin;
            CheckBoxConnectToGoodgame.IsChecked = Properties.Settings.Default.CheckBoxConnectToGoodgame;
            TextBoxIndividualVoicesChannelPoints.Text = Properties.Settings.Default.TextBoxIndividualVoicesChannelPoints;
        }

        private async void buttonTwitchConnect_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxConnectToTwitch.IsChecked == true)
            {
                //синглтон подключения твича
                TwitchConnection TClient = TwitchConnection.Instance;
            }
            if (CheckBoxConnectToGoodgame.IsChecked == true)
            {
                GoodGameConnection.GoodGameConnect(TextBoxGoodgameLogin.Text);
            }
                



        }

        private void checkBoxRemoveEmoji_Checked(object sender, RoutedEventArgs e)
        {
            TextToSpeech.CanRemoveEmoji = (bool)checkBoxRemoveEmoji.IsChecked;
            SaveContainers.JsonSaveRemoveEmoji = (bool)checkBoxRemoveEmoji.IsChecked;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            checkBoxRemoveEmoji.IsChecked = TextToSpeech.CanRemoveEmoji;

            foreach (InstalledVoice voice in Synth.GetInstalledVoices())
            {
                comboBoxInstalledVoices.Items.Add(voice.VoiceInfo.Name);
                AddVoiceToPool.InstalledVoices.Add(voice.VoiceInfo.Name);

            }
            individualVoices = new IndividualVoices();
        }

        private async void comboBoxInstalledVoices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //without delay the method will get previous voice name
            await Task.Delay(1);
            comboBoxInstalledVoices.ToolTip = comboBoxInstalledVoices.Text;
            TextToSpeech.TtsVoiceName = comboBoxInstalledVoices.Text;
            SaveContainers.JsonSaveInstalledVoicesSelect = comboBoxInstalledVoices.Text;
        }

        private void sliderTtsVolumeValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            labelVolumeValue.Content = sliderTtsVolumeValue.Value;
            TextToSpeech.TtsVolume = (int)sliderTtsVolumeValue.Value;
            SaveContainers.JsonSaveTtsVolumeValue = (int)sliderTtsVolumeValue.Value;
        }

        private void sliderTtsSpeedValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            labelTtsSpeedValue.Content = sliderTtsSpeedValue.Value;
            TextToSpeech.TtsSpeed = (int)sliderTtsSpeedValue.Value;
            SaveContainers.JsonSaveTtsSpeedValue = (int)sliderTtsSpeedValue.Value;
        }

        private void buttonSaveJsonProfile_Click(object sender, RoutedEventArgs e)
        {
            SaveContainers.JsonSaveTwitchApi = PasswordboxTwitchApi.Password;
            SaveContainers.SaveJsonFileMainWindow(textboxJsonProfileNameToSave.Text, comboBoxProfileSelect);

        }

        private void TextboxTwitchClientId_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            
            SaveContainers.JsonSaveTwitchId = TextboxTwitchClientId.Text;
            TwitchConnection.twitchId = TextboxTwitchClientId.Text;
        }

        private void TextboxTwitchNickName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            SaveContainers.JsonSaveTwitchNick = TextboxTwitchNickName.Text;
            TwitchConnection.twitchNick = TextboxTwitchNickName.Text;
        }

        private void buttonLoadJsonProfile_Click(object sender, RoutedEventArgs e)
        {
            Newtonsoft.Json.Linq.JObject jsonFile = LoadContainers.LoadProfile(comboBoxProfileSelect.Text);
            if (jsonFile != null)
            {
                try
                {
                    PasswordboxTwitchApi.Password = jsonFile["twitchApi"].ToString();
                    TextboxTwitchClientId.Text = jsonFile["twitchId"].ToString();
                    TextboxTwitchNickName.Text = jsonFile["twitchNick"].ToString();
                    checkBoxRemoveEmoji.IsChecked = (bool)jsonFile["removeEmoji"];
                    comboBoxInstalledVoices.Text = jsonFile["installedVoicesSelect"].ToString();
                    sliderTtsVolumeValue.Value = (int)jsonFile["ttsVolumeValue"];
                    sliderTtsSpeedValue.Value = (int)jsonFile["ttsSpeedValue"];
                    TextBoxTtsDoNotTtsIfStartWith.Text = jsonFile["DoNotTtsIfStartWith"].ToString();
                    TextBoxSkipMessage.Text = jsonFile["MessageSkip"].ToString();
                    TextBoxSkipMessageAll.Text = jsonFile["MessageSkipAll"].ToString();
                    TextBoxGoodgameLogin.Text = jsonFile["goodgameLogin"].ToString();
                    CheckBoxConnectToGoodgame.IsChecked = (bool)jsonFile["checkboxConnectToGoodgame"];
                    TextBoxIndividualVoicesChannelPoints.Text = jsonFile["twitchChannelPointsIndividualVoiceChange"].ToString();
                }
                catch { }

            }
        }

        private void PasswordboxTwitchApi_PasswordChanged(object sender, RoutedEventArgs e)
        {
            TwitchConnection.twitchApi = PasswordboxTwitchApi.Password;
        }

        private void comboBoxProfileSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ButtonIndividualVoices_Click(object sender, RoutedEventArgs e)
        {
            individualVoices.Show();
        }

        private void ButtonOpenProgramData_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "DataForProgram");
        }

        private void ButtonBlackListAdd_Click(object sender, RoutedEventArgs e)
        {
            bool isDuplicate = false;

            // Проверяем, есть ли элементы в ListBox
            if (ListBoxBlackList.Items.Count == 0)
            {
                // Добавляем элемент, если ListBox пустой
                ListBoxBlackList.Items.Add(TextBoxBlackList.Text);
                TwitchConnection.TwitchBlackList.Items.Add(TextBoxBlackList.Text.ToLower());
            }
            else
            {
                // Проходим по всем элементам и проверяем наличие дубликатов
                foreach (var item in ListBoxBlackList.Items)
                {
                    if(item is string itemString)
                    {
                        if (itemString.ToLower() == TextBoxBlackList.Text.ToLower())
                        {
                            isDuplicate = true;
                            break; // Прерываем цикл, если найден дубликат
                        }
                    }

                }

                // Если дубликатов нет, добавляем новый элемент
                if (!isDuplicate && !string.IsNullOrWhiteSpace(TextBoxBlackList.Text))
                {
                    ListBoxBlackList.Items.Add(TextBoxBlackList.Text);
                    TwitchConnection.TwitchBlackList.Items.Add(TextBoxBlackList.Text.ToLower());
                }
            }
        }

        private void ButtonBlackListDelete_Click(object sender, RoutedEventArgs e)
        {
            if(ListBoxBlackList.SelectedIndex != -1)
            {
                int a = ListBoxBlackList.SelectedIndex;
                ListBoxBlackList.Items.Remove(ListBoxBlackList.SelectedItem);
                TwitchConnection.TwitchBlackList.Items.RemoveAt(a);
            }
        }

        private void buttonTwitchDisconnect_Click(object sender, RoutedEventArgs e)
        {
            TwitchConnection.Instance.Disconnect();
        }

        // Флаги для отключения синхронизации
        private bool isSyncing = false;

        //замена слов
        private void ListBoxWhatToReplace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isSyncing) return;
            isSyncing = true;
            ListBoxWhatToReplaceWith.SelectedIndex = ListBoxWhatToReplace.SelectedIndex;
            ListBoxWhatToReplaceWith.ScrollIntoView(ListBoxWhatToReplaceWith.SelectedItem);

            isSyncing = false;
        }

        private void ListBoxWhatToReplaceWith_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isSyncing) return;
            isSyncing = true;
            ListBoxWhatToReplace.SelectedIndex = ListBoxWhatToReplaceWith.SelectedIndex;
            ListBoxWhatToReplace.ScrollIntoView(ListBoxWhatToReplace.SelectedItem);
            isSyncing = false;
        }

        private void ButtonReplaceWordAdd_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxWhatToReplace.Items.Count == 0)
            {
                TextToSpeech.WhatToReplace.Add(TextBoxWhatToReplace.Text);
                TextToSpeech.WhatToReplaceWith.Add(TextBoxWhatToReplaceWith.Text);
                ListBoxWhatToReplace.Items.Add(TextBoxWhatToReplace.Text);
                ListBoxWhatToReplaceWith.Items.Add((string)TextBoxWhatToReplaceWith.Text);
            }
            else
            {
                bool isDuplicate = false;
                foreach (string item in ListBoxWhatToReplace.Items)
                {
                    if (TextBoxWhatToReplace.Text.ToLower() == item.ToLower())
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                if (!isDuplicate)
                {
                    TextToSpeech.WhatToReplace.Add(TextBoxWhatToReplace.Text);
                    TextToSpeech.WhatToReplaceWith.Add(TextBoxWhatToReplaceWith.Text);
                    ListBoxWhatToReplace.Items.Add(TextBoxWhatToReplace.Text);
                    ListBoxWhatToReplaceWith.Items.Add(TextBoxWhatToReplaceWith.Text);
                }
            }

        }

        private void ButtonReplaceWordDelete_Click(object sender, RoutedEventArgs e)
        {
            // есть ли элемент вообще
            int selectedIndex = ListBoxWhatToReplace.SelectedIndex;

            if (selectedIndex >= 0)
            {
                // Удаляем элементы из коллекций TextToSpeech
                TextToSpeech.WhatToReplace.Remove(ListBoxWhatToReplace.SelectedItem.ToString());
                TextToSpeech.WhatToReplaceWith.Remove(ListBoxWhatToReplaceWith.SelectedItem.ToString());

                // Отключаем синхронизацию во время удаления
                isSyncing = true;

                // Удаляем элементы из ListBox
                ListBoxWhatToReplace.Items.RemoveAt(selectedIndex);
                ListBoxWhatToReplaceWith.Items.RemoveAt(selectedIndex);

                // Включаем синхронизацию обратно
                isSyncing = false;
            }
        }

        private void CheckBoxConnectToTwitch_Checked(object sender, RoutedEventArgs e)
        {
            ConnectToTwitch = CheckBoxConnectToTwitch.IsChecked ?? false;
        }

        private void buttonSaveJsonProfile_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            textboxJsonProfileNameToSave.Background = Brushes.LightPink;
        }

        private void buttonSaveJsonProfile_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            textboxJsonProfileNameToSave.Background = Brushes.White;
        }

        private void buttonLoadJsonProfile_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            comboBoxProfileSelect.Foreground = Brushes.Red;
        }

        private void buttonLoadJsonProfile_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            comboBoxProfileSelect.Foreground = Brushes.Black;
        }

        private void ButtonDeleteProfile_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            comboBoxProfileSelect.Foreground = Brushes.Red;
        }

        private void ButtonDeleteProfile_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            comboBoxProfileSelect.Foreground = Brushes.Black;
        }

        private void ButtonDeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            SaveContainers.DeleteProfile(comboBoxProfileSelect);
        }

        private void checkBoxVoiceIndividualVoices_Checked(object sender, RoutedEventArgs e)
        {
            TextToSpeech.IndividualVoiceForAll = checkBoxVoiceIndividualVoices.IsChecked ?? false;
            SaveContainers.JsonIndividualVoices = checkBoxVoiceIndividualVoices.IsChecked ?? false;
        }

        private void TextBoxTtsDoNotTtsIfStartWith_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveContainers.JsonDoNotTtsIfStartWith = TextBoxTtsDoNotTtsIfStartWith.Text;
            TextToSpeech.DoNotTtsIfStartWith = TextBoxTtsDoNotTtsIfStartWith.Text;
        }

        private void TextBoxSkipMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveContainers.JsonMessageSkip = TextBoxSkipMessage.Text;
            TextToSpeech.MessageSkip = TextBoxSkipMessage.Text;
        }

        private void TextBoxSkipMessageAll_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveContainers.JsonMessageSkipAll = TextBoxSkipMessageAll.Text;
            TextToSpeech.MessageSkipAll = TextBoxSkipMessageAll.Text;
        }

        private void TextBoxGoodgameLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveContainers.JsonTextBoxGoodgameLogin = TextBoxGoodgameLogin.Text;
        }

        private void CheckBoxConnectToGoodgame_Checked(object sender, RoutedEventArgs e)
        {
            SaveContainers.JsonCheckBoxConnectToGoodgame = CheckBoxConnectToTwitch.IsChecked ?? false;
        }
        private void HyperLinkTwitchGetTokens(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer","https://twitchtokengenerator.com/");
        }

        private void TextBoxIndividualVoicesChannelPoints_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveContainers.JsonTextBoxIndividualVoicesChannelPoints = TextBoxIndividualVoicesChannelPoints.Text;
            TwitchConnection.ChangeVoiceChannelPointsRewardName = TextBoxIndividualVoicesChannelPoints.Text;
        }
    }
}