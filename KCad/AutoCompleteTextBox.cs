using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace KCad
{
    public class AutoCompleteTextBox : TextBox
    {
        public class TextEventArgs : EventArgs
        {
            public string Text;
        }

        public delegate void TextEventHandler(object sender, TextEventArgs e);

        private Popup CandidatePopup;
        private ListBox CandidateListBox;


        public IEnumerable CandidateList
        {
            get;
            set;
        }

        public bool IsDropDownOpen
        {
            get
            {
                if (CandidatePopup == null)
                {
                    return false;
                }

                return CandidatePopup.IsOpen;
            }
        }

        public event TextEventHandler Determine;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            CandidatePopup = new Popup();
            CandidateListBox = new ListBox();

            CandidatePopup.Child = CandidateListBox;

            CandidateListBox.Background = Brushes.Black;
            CandidateListBox.Foreground = Brushes.White;

            CandidateListBox.MouseUp += CandidateListBox_MouseUp; ;
            CandidateListBox.PreviewKeyDown += CandidateListBox_PreviewKeyDown; ;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (CandidatePopup.IsOpen)
            {
                if (e.Key == Key.Down)
                {
                    CandidateListBox.Focus();
                }
            }
            else
            {
                if (e.Key == Key.Enter)
                {
                    TextEventArgs ea = new TextEventArgs();

                    ea.Text = Text;

                    Determine(this, ea);
                }
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (Comp(Text))
            {
                CandidatePopup.PlacementTarget = this;
                CandidatePopup.IsOpen = true;
            }
            else
            {
                CandidatePopup.IsOpen = false;
            }
        }

        private void CandidateListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SetTextFromListBox();
            ClosePopup();
        }

        private void CandidateListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (CandidatePopup.IsOpen)
            {
                if (e.Key == Key.Enter)
                {
                    SetTextFromListBox();
                    ClosePopup();
                }
            }
        }

        private void ClosePopup()
        {
            CandidatePopup.IsOpen = false;
            Focus();
            CaretIndex = Text.Length;
        }

        private bool SetTextFromListBox()
        {
            if (CandidateListBox.SelectedItem == null)
            {
                return false;
            }

            string s = CandidateListBox.SelectedItem.ToString();

            Text = s;

            return true;
        }

        private bool Comp(string currentText)
        {
            if (string.IsNullOrEmpty(currentText))
            {
                return false;
            }

            if (CandidateList == null)
            {
                return false;
            }


            CandidateListBox.Items.Clear();

            foreach (var item in CandidateList)
            {
                string text = item as String;
                if (text.Contains(currentText))
                {
                    CandidateListBox.Items.Add(text);
                }
            }

            return CandidateListBox.Items.Count > 0;
        }
    }
}
