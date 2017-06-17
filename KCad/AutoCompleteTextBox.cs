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
using System.Windows;


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

        public TextHistory History
        {
            get;
            set;
        } = new TextHistory();

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

            CandidateListBox.MouseUp += CandidateListBox_MouseUp;
            CandidateListBox.PreviewKeyDown += CandidateListBox_PreviewKeyDown;

            CandidateListBox.PreviewLostKeyboardFocus += CandidateListBox_PreviewLostKeyboardFocus;

            Application.Current.MainWindow.Deactivated += MainWindow_Deactivated;
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            ClosePopup();
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
                else if (e.Key == Key.Escape)
                {
                    CandidatePopup.IsOpen = false;
                }
                else if (e.Key == Key.Enter)
                {
                    NotifyDetermine();
                }
            }
            else
            {
                if (e.Key == Key.Enter)
                {
                    NotifyDetermine();
                }

                if (History != null)
                {
                    if (e.Key == Key.Up)
                    {
                        string s = History.Rewind();
                        Text = s;
                    }
                    else if (e.Key == Key.Down)
                    {
                        string s = History.Forward();
                        Text = s;
                    }
                }
            }
        }

        private void NotifyDetermine()
        {
            TextEventArgs ea = new TextEventArgs();

            ea.Text = Text;

            Determine(this, ea);
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

        private void CandidateListBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!CandidatePopup.IsOpen)
            {
                return;
            }

            bool focus = false;

            foreach (ListBoxItem item in CandidateListBox.Items)
            {
                if (item.Equals(e.NewFocus))
                {
                    focus = true;
                }
            }

            Console.WriteLine("CandidateListBox_PreviewLostKeyboardFocus focus:" + focus.ToString());

            if (!focus)
            {
                ClosePopup();
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
                else if (e.Key == Key.Escape)
                {
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

            ListBoxItem item = (ListBoxItem)(CandidateListBox.SelectedItem);

            Text = (string)(item.Content);

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

            foreach (var str in CandidateList)
            {
                string text = str as String;

                if (text.Contains(currentText))
                {
                    ListBoxItem item = new ListBoxItem();

                    item.Content = str;

                    CandidateListBox.Items.Add(item);
                }
            }

            return CandidateListBox.Items.Count > 0;
        }


        public class TextHistory
        {
            private List<string> Data = new List<string>();
            int Pos = 0;

            private string empty = "";

            public void Add(string s)
            {
                Data.Add(s);
                Pos = Data.Count;
            }

            public string Rewind()
            {
                Pos--;

                if (Pos < 0)
                {
                    Pos = 0;
                    return empty;
                }

                return Data[Pos];
            }


            public string Forward()
            {
                Pos++;

                if (Pos >= Data.Count)
                {
                    Pos = Data.Count;
                    return empty;
                }

                return Data[Pos];
            }
        }
    }
}