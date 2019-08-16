using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AdmAssist.Views.Controls
{
    public class HighlightingTextBlock : TextBlock
    {
        public string HighlightedText
        {
            get { return (string)GetValue(HighlightedTextProperty); }
            set { SetValue(HighlightedTextProperty, value); }
        }

        public static readonly DependencyProperty HighlightedTextProperty =
            DependencyProperty.Register("HighlightedText", typeof(string), typeof(HighlightingTextBlock), new UIPropertyMetadata(string.Empty, UpdateHighlightEffect));

        public static readonly DependencyProperty OriginalTextProperty = DependencyProperty.Register(
            "OriginalText", typeof(string), typeof(HighlightingTextBlock), new PropertyMetadata(default(string), OnOriginalTextChanged));

        private static void OnOriginalTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var block = ((HighlightingTextBlock)obj);
            block.Text = block.OriginalText;
            block.UpdateHighlightEffect();
        }

        public string OriginalText
        {
            get { return (string)GetValue(OriginalTextProperty); }
            set { SetValue(OriginalTextProperty, value); }
        }

        private static void UpdateHighlightEffect(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(string.IsNullOrEmpty(e.NewValue as string) && string.IsNullOrEmpty(e.OldValue as string)))
                ((HighlightingTextBlock)sender).UpdateHighlightEffect();
        }

        private void UpdateHighlightEffect()
        {
            if (string.IsNullOrEmpty(HighlightedText)) return;

            var allText = GetCompleteText();

            Inlines.Clear();

            var indexOfHighlightString = allText.IndexOf(HighlightedText, StringComparison.InvariantCultureIgnoreCase);

            if (indexOfHighlightString < 0)
            {
                Inlines.Add(allText);
            }
            else
            {
                Inlines.Add(allText.Substring(0, indexOfHighlightString));
                Inlines.Add(new Run
                {
                    Text = allText.Substring(indexOfHighlightString, HighlightedText.Length),
                    Foreground = Brushes.White,
                    Background = Brushes.OrangeRed
                });
                Inlines.Add(allText.Substring(indexOfHighlightString + HighlightedText.Length));
            }

        }

        private string GetCompleteText()
        {
            var allText = Inlines.OfType<Run>().Aggregate(new StringBuilder(), (sb, run) => sb.Append(run.Text), sb => sb.ToString());
            return allText;
        }
    }
}