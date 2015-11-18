using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using Xceed.Wpf.Toolkit;

namespace FSX.Windows.Controls
{    
    public class IntelliRichTextBox : PlainRichTextBox
    {
        private ListBox IntellisenseList = new ListBox();
        
        #region Dependency Properties

        public static readonly DependencyProperty AutoMoveIntellisenseListProperty =
            DependencyProperty.Register("AutoMoveIntellisenseListProperty", typeof(bool), typeof(IntelliRichTextBox), new UIPropertyMetadata(false));

        public bool AutoMoveIntellisenseList
        {
            get { return (bool)GetValue(AutoMoveIntellisenseListProperty); }
            set { SetValue(AutoMoveIntellisenseListProperty, value); }
        }

        public static readonly DependencyProperty ContentAssistSourceProperty =
            DependencyProperty.Register("ContentAssistSource", typeof(IList<String>), typeof(IntelliRichTextBox), new UIPropertyMetadata(new List<string>()));

        public IList<String> ContentAssistSource
        {
            get { return (IList<String>)GetValue(ContentAssistSourceProperty); }
            set { SetValue(ContentAssistSourceProperty, value); }
        }

        public static readonly DependencyProperty ContentAssistTriggersProperty =
            DependencyProperty.Register("ContentAssistTriggers", typeof(IList<char>), typeof(IntelliRichTextBox), new UIPropertyMetadata(new List<char>()));

        public IList<char> ContentAssistTriggers
        {
            get { return (IList<char>)GetValue(ContentAssistTriggersProperty); }
            set { SetValue(ContentAssistTriggersProperty, value); }
        }

        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register("Level", typeof(int), typeof(IntelliRichTextBox), new UIPropertyMetadata(-1));

        public int Level
        {
            get { return (int)GetValue(LevelProperty); }
            set
            {
                //// [FSX] as soon as level changes, reset the intellisense list location to that point and update the contents as per the level
                if ((int)GetValue(LevelProperty) != value)
                {
                    SetValue(LevelProperty, value);
                    UpdateIntellisenseContent();
                    if (!IsLevelIncreased)
                        IntellisenseList.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        public static readonly DependencyProperty IsLevelIncreasedProperty =
            DependencyProperty.Register("IsLevelIncreased", typeof(bool), typeof(IntelliRichTextBox), new UIPropertyMetadata(false));

        public bool IsLevelIncreased
        {
            get { return (bool)GetValue(IsLevelIncreasedProperty); }
            set { SetValue(IsLevelIncreasedProperty, value); }
        }
        
        public static readonly DependencyProperty LastWordProperty =
            DependencyProperty.Register("LastWord", typeof(string), typeof(IntelliRichTextBox), new UIPropertyMetadata(String.Empty));

        public string LastWord
        {
            get { return (string)GetValue(LastWordProperty); }
            set { SetValue(LastWordProperty, value); }
        }

        public static readonly DependencyProperty CompleteWordProperty =
            DependencyProperty.Register("CompleteWord", typeof(string), typeof(IntelliRichTextBox), new UIPropertyMetadata(String.Empty));

        public string CompleteWord
        {
            get { return (string)GetValue(CompleteWordProperty); }
            set
            {
                //// [FSX] as soon as complete word changes, set the last word and level
                SetValue(CompleteWordProperty, value);
                SetLastWord();
            }
        }

        #endregion

        public void SetCompleteWord(string appendStr = "")
        {
            var text = this.CaretPosition.GetTextInRun(LogicalDirection.Backward) + appendStr;
            var index = text.LastIndexOf(' ');
            if (index >= 0)
            {
                index++;
                text = text.Substring(index, text.Length - index);
            }
            CompleteWord = text;
        }

        public void SetLastWord()
        {
            var text = CompleteWord;
            var index = text.LastIndexOf(' ');
            if (index >= 0)
            {
                index++;
                text = text.Substring(index, text.Length - index);
            }
            var splitText = text.Split(new char[] { '.' });
            LastWord = splitText[splitText.Length - 1];
        }

        public void SetLevel()
        {
            Level = CompleteWord.Count(x => x == '.');
        }

        #region .ctor

        public IntelliRichTextBox()
        {
            this.Loaded += new RoutedEventHandler(RichTextBoxEx_Loaded);
        }

        void RichTextBoxEx_Loaded(object sender, RoutedEventArgs e)
        {
            if (IntellisenseList.Parent == null)
            {
                (this.Parent as Grid).Children.Add(IntellisenseList);
                //Create the style with Margin=0 for the Paragraph
                Style style = new Style { TargetType = typeof(Paragraph) };
                style.Setters.Add(new Setter(Paragraph.MarginProperty, new Thickness(0)));
                //Add the style as resource.            
                this.Resources.Add(typeof(Paragraph), style);
            }
            IntellisenseList.SetValue(Canvas.ZIndexProperty, 1000);
            if (this.Parent.GetType() != typeof(Grid))
            {
                throw new Exception("This control must be inside Grid control.");
            }
            if (ContentAssistTriggers.Count == 0)
            {
                ContentAssistTriggers.Add('.');
            }
            this.BorderThickness = new Thickness(1);
            this.BorderBrush = Brushes.Black;
            IntellisenseList.MaxHeight = 100;
            IntellisenseList.MinWidth = 100;
            IntellisenseList.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            IntellisenseList.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            IntellisenseList.Visibility = System.Windows.Visibility.Collapsed;
            IntellisenseList.BorderThickness = new Thickness(1);
            IntellisenseList.MouseDoubleClick += new MouseButtonEventHandler(AssistListBox_MouseDoubleClick);
            IntellisenseList.PreviewKeyDown += new KeyEventHandler(AssistListBox_PreviewKeyDown);
            UpdateInfo();
        }

        private void AssistListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.Space)
            {
                InsertAssistWord();
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                // Backspace key is pressed, set focus to richtext box
                //if (sbLastWords.Length >= 1)
                //{
                //    sbLastWords.Remove(sbLastWords.Length - 1, 1);
                //}
                this.Focus();
            }
        }

        private void AssistListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            InsertAssistWord();
        }

        #endregion

        #region Insert Text

        public void InsertText(string text)
        {
            Focus();
            CaretPosition.InsertTextInRun(text);
            TextPointer pointer = CaretPosition.GetPositionAtOffset(text.Length);
            if (pointer != null)
            {
                CaretPosition = pointer;
            }
        }

        #endregion

        #region Helper

        private void UpdateIntellisenseContent()
        {
            if (Intellisense != null)
            {
                if (Level < 0)
                {
                    ContentAssistSource = Intellisense.Select(x => x.Name).ToList();
                }
                else
                {
                    var split = CompleteWord.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var contents = GetContent(Intellisense, split, 0);
                    if (contents != null)
                        ContentAssistSource = contents;
                    else
                        ContentAssistSource.Clear();
                }
            }
        }

        private List<string> GetContent(Intellisense intellisense, List<string> split, int level)
        {
            if (level == Level)
            {
                return intellisense.Select(x => x.Name).ToList();
            }
            else
            {
                if (split.Count > level)
                {
                    var intellisenseItem = intellisense.Where(x => x.Name == split[level]).FirstOrDefault();
                    if (intellisenseItem != null)
                    {
                        return GetContent(intellisenseItem.Children, split, ++level);
                    }
                }
                return null;
            }
        }

        #endregion

        #region Content Assist

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            IsLevelIncreased = false;
            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.Control) // when ctrl + space is pressed
            {
                var filteredCount = FilterIntellisenseWithCount();
                if (filteredCount == 1)
                {
                    InsertAssistWord();
                }
                else if (filteredCount > 1)
                {
                    ShowIntellisense();
                }
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                IntellisenseList.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (e.Key == System.Windows.Input.Key.OemPeriod)
            {
                IsLevelIncreased = true;
                UpdateInfo(".");
                ShowIntellisense();
            }
            else if (e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Tab)
            {
                if (IntellisenseList.Visibility == System.Windows.Visibility.Visible)
                {
                    if (InsertAssistWord())
                    {
                        e.Handled = true;
                    }
                }
            }
            else if (e.Key == Key.Down)
            {
                IntellisenseList.Focus();
            }
            base.OnPreviewKeyDown(e);
        }

        private void UpdateInfo(string appendStr = "")
        {
            SetCompleteWord(appendStr);
            SetLevel();
            ResetIntellisenseLocation();
            FilterIntellisenseWithCount();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            UpdateInfo();
        }

        private bool InsertAssistWord()
        {
            bool isInserted = false;
            if (IntellisenseList.SelectedIndex != -1)
            {
                TextPointer pointer = this.CaretPosition;
                var text = pointer.GetTextInRun(LogicalDirection.Backward);
                var offset = LastWord.Length * -1;
                this.CaretPosition = pointer.GetPositionAtOffset(offset, LogicalDirection.Backward);
                var wordToInsert = IntellisenseList.SelectedItem.ToString();
                this.CaretPosition.DeleteTextInRun(LastWord.Length);
                this.InsertText(wordToInsert);
                isInserted = true;
            }
            IntellisenseList.Visibility = System.Windows.Visibility.Collapsed;
            return isInserted;
        }

        private void ResetIntellisenseLocation()
        {
            var pointer = this.CaretPosition;
            Rect rect = pointer.GetCharacterRect(LogicalDirection.Forward);
            var text = pointer.GetTextInRun(LogicalDirection.Backward);
            var index = text.LastIndexOfAny(new char[] { '.', ' ' });
            int offset = text.Length * -1;
            if (index >= 0)
            {
                offset = (text.Length - index - 1) * -1;
            }
            pointer = pointer.GetPositionAtOffset(offset, LogicalDirection.Backward);
            rect = pointer.GetCharacterRect(LogicalDirection.Forward);
            double left = rect.X >= 0 ? rect.X : 0;
            double top = rect.Y >= 0 ? rect.Y + 20 : 0;
            left += this.Padding.Left;
            top += this.Padding.Top;
            IntellisenseList.SetCurrentValue(ListBox.MarginProperty, new Thickness(left, top, 0, 0));
        }

        private int FilterIntellisenseWithCount()
        {
            IEnumerable<string> filtered = ContentAssistSource.Where(s => s.ToUpper().StartsWith(LastWord.ToString().ToUpper()));
            IntellisenseList.ItemsSource = filtered;
            IntellisenseList.SelectedIndex = 0;
            if (filtered.Count() == 0)
            {
                IntellisenseList.Visibility = System.Windows.Visibility.Collapsed;
            }
            return filtered.Count();
        }

        private void ShowIntellisense()
        {
            //// [FSX] if there is any contents then only display the intellisense
            if (ContentAssistSource.Count() > 0 && FilterIntellisenseWithCount() > 0)
            {
                IntellisenseList.Visibility = System.Windows.Visibility.Visible;
            }
        }

        #endregion

        public Intellisense Intellisense;

        public void FeedDataForIntellisense(Intellisense intellisense)
        {
            Intellisense = intellisense;
            UpdateIntellisenseContent();
        }
    }

    public class IntelliRichTextBoxFormatter : ITextFormatter
    {
        public string GetText(System.Windows.Documents.FlowDocument document)
        {
            return ReplaceLastOccurrence(new TextRange(document.ContentStart, document.ContentEnd).Text, Environment.NewLine, String.Empty);
        }

        public void SetText(System.Windows.Documents.FlowDocument document, string text)
        {
            new TextRange(document.ContentStart, document.ContentEnd).Text = text;
        }

        public static string ReplaceLastOccurrence(string text, string find, string replace)
        {
            var result = text;
            int index = text.LastIndexOf(find);
            if (index >= 0)
                result = text.Remove(index, find.Length).Insert(index, replace);
            return result;
        }
    }
}
