using EvernoteClone.ViewModel;
using EvernoteClone.ViewModel.Helpers;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EvernoteClone.View {
    /// <summary>
    /// Interaction logic for NotesWindow.xaml
    /// </summary>
    public partial class NotesWindow : Window {

        NotesVM viewModel;
        public NotesWindow() {
            InitializeComponent();

            viewModel = Resources["vm"] as NotesVM;
            viewModel.OnSelectedNoteChanged += ViewModel_OnSelectedNoteChanged;

            var fontFamilies = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            fontFamilyComboBox.ItemsSource = fontFamilies;

            List<double> fontSizes = new List<double>() { 8, 9, 10, 11, 12, 13, 14, 15, 16, 18, 20, 24, 26, 28, 30, 32, 36, 40, 48 };
            fontSizeComboBox.ItemsSource = fontSizes;
        }

        private void ViewModel_OnSelectedNoteChanged(object? sender, EventArgs e) {

            contentRichTextBox.Document.Blocks.Clear();
            if (viewModel.SelectedNote != null) {
                if (!string.IsNullOrEmpty(viewModel.SelectedNote.FileLocation)) {

                    FileStream fileStream = new FileStream(viewModel.SelectedNote.FileLocation, FileMode.Open);
                    var contents = new TextRange(contentRichTextBox.Document.ContentStart, contentRichTextBox.Document.ContentEnd);
                    contents.Load(fileStream, DataFormats.Rtf);

                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {

            Application.Current.Shutdown();
        }

        private async void SpeechButton_Click(object sender, RoutedEventArgs e) {

            string region = "eastus";
            string key = "3GbP5AqHvN226mdp5AQUPcGN4iJTtYK6mfqhAbh34W9tPTNavIUyJQQJ99AKACYeBjFXJ3w3AAAYACOGBbiO";

            var speechConfig = SpeechConfig.FromSubscription(key, region);
            using (var audioConfig = AudioConfig.FromDefaultMicrophoneInput()) {

                using (var recognizer = new SpeechRecognizer(speechConfig, audioConfig)) {

                    var result = await recognizer.RecognizeOnceAsync();
                    contentRichTextBox.Document.Blocks.Add(new Paragraph(new Run(result.Text)));
                }
            }
        }

        private void contentRichTextBox_TextChanged(object sender, TextChangedEventArgs e) {

            int amountOfCharacters = (new TextRange(contentRichTextBox.Document.ContentStart, contentRichTextBox.Document.ContentEnd)).Text.Length;

            statusTextBlock.Text = $"Document length: {amountOfCharacters} characters";
        }

        private void boldButton_Click(object sender, RoutedEventArgs e) {

            bool isButtonChecked = (sender as ToggleButton).IsChecked ?? false;

            if (isButtonChecked) {

                contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Bold);
            } else {

                contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Normal);
            }
        }

        private void underlineButton_Click(object sender, RoutedEventArgs e) {

            bool isButtonChecked = (sender as ToggleButton).IsChecked ?? false;

            if (isButtonChecked) {

                contentRichTextBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            } else {

                contentRichTextBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
            }
        }

        private void italicButton_Click(object sender, RoutedEventArgs e) {

            bool isButtonChecked = (sender as ToggleButton).IsChecked ?? false;

            if (isButtonChecked) {

                contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Italic);
            } else {

                contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Normal);
            }
        }

        private void fontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (fontFamilyComboBox.SelectedItem != null) {

                contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, fontFamilyComboBox.SelectedItem);
            }
        }

        private void fontSizeComboBox_TextChanged(object sender, TextChangedEventArgs e) {

            if (double.TryParse(fontSizeComboBox.Text, out var fontSize)) {

                contentRichTextBox.Selection.ApplyPropertyValue(Inline.FontSizeProperty, fontSize);
            }
        }


        private void contentRichTextBox_SelectionChanged(object sender, RoutedEventArgs e) {

            var selectedWeight = contentRichTextBox.Selection.GetPropertyValue(FontWeightProperty);

            boldButton.IsChecked = (selectedWeight != DependencyProperty.UnsetValue) && selectedWeight.Equals(FontWeights.Bold);

            var selectedDecoration = contentRichTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);

            underlineButton.IsChecked = (selectedDecoration != DependencyProperty.UnsetValue) && selectedDecoration.Equals(TextDecorations.Underline);

            var selectedStyle = contentRichTextBox.Selection.GetPropertyValue(Inline.FontStyleProperty);

            italicButton.IsChecked = (selectedStyle != DependencyProperty.UnsetValue) && selectedStyle.Equals(FontStyles.Italic);

            fontFamilyComboBox.SelectedItem = contentRichTextBox.Selection.GetPropertyValue(Inline.FontFamilyProperty);

            fontSizeComboBox.Text = (contentRichTextBox.Selection.GetPropertyValue(Inline.FontSizeProperty)).ToString();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e) {


            string rtFile = System.IO.Path.Combine(Environment.CurrentDirectory, $"{viewModel.SelectedNote.Id}.rtf");
            viewModel.SelectedNote.FileLocation = rtFile;
            DatabaseHelper.Update(viewModel.SelectedNote);

            FileStream fileStream = new FileStream(rtFile, FileMode.Create);
            var contents = new TextRange(contentRichTextBox.Document.ContentStart, contentRichTextBox.Document.ContentEnd);
            contents.Save(fileStream, DataFormats.Rtf);
        }
    }
}
