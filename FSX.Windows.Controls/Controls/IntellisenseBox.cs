using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;

namespace FSX.Windows.Controls
{
    //http://www.codeproject.com/Articles/180311/Rich-Text-Box-With-Intellisense-Ability
    public class IntellisenseBox : Control
    {
        public IntelliRichTextBox RichTextBoxEx { get; private set; }

        static IntellisenseBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IntellisenseBox), new FrameworkPropertyMetadata(typeof(IntellisenseBox)));
        }

        public override void OnApplyTemplate()
        {
            // searching to resolve RichTextBoxEx control in the the child controls
            RichTextBoxEx = FindVisualChild<IntelliRichTextBox>(this);
            // if RichTextBoxEx is not found, raise error
            Debug.Assert(RichTextBoxEx != null);
            base.OnApplyTemplate();
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(IntellisenseBox), new UIPropertyMetadata(TextPropertyChangedHandler));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static void TextPropertyChangedHandler(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        public static readonly DependencyProperty IntellisenseProperty =
            DependencyProperty.Register("Intellisense", typeof(Intellisense), typeof(IntellisenseBox), new PropertyMetadata(IntellisensePropertyChangedHandler));
        
        public Intellisense Intellisense
        {
            get { return (Intellisense)GetValue(IntellisenseProperty); }
            set { SetValue(IntellisenseProperty, value); }
        }

        public static void IntellisensePropertyChangedHandler(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var intellisenseBox = ((IntellisenseBox)sender);
            intellisenseBox.Intellisense = (Intellisense)e.NewValue;
            intellisenseBox.RichTextBoxEx.FeedDataForIntellisense((Intellisense)e.NewValue);
        }

        /// <summary>
        /// Search for an element of a certain type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="visual">The parent element.</param>
        /// <returns></returns>
        public T FindVisualChild<T>(Visual visual) where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null)
                {
                    T correctlyTyped = child as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }
                    T descendent = FindVisualChild<T>(child);
                    if (descendent != null)
                    {
                        return descendent;
                    }
                }
            }
            return null;
        }
    }
}