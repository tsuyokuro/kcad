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
using System.Text.RegularExpressions;
using Plotter;

namespace KCad
{
    public class AutoCompleteTextBox : TextBox
    {
        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
        }

        public class TextEventArgs : EventArgs
        {
            public string Text;
        }

        private Popup CandidatePopup;
        private ListBox CandidateListBox;

        private bool DisableCandidateList = false;

        public IEnumerable CandidateList
        {
            get;
            set;
        } = new List<string>();

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

        public Brush CandidateListBackground
        {
            get
            {
                return CandidateListBox.Background;
            }
            set
            {
                CandidateListBox.Background = value;
            }
        }

        public Brush CandidateListForeground
        {
            get
            {
                return CandidateListBox.Foreground;
            }
            set
            {
                CandidateListBox.Foreground = value;
            }
        }

        public Brush CandidateListBorder
        {
            get
            {
                return CandidateListBox.BorderBrush;
            }
            set
            {
                CandidateListBox.BorderBrush = value;
            }
        }


        public event Action<object, TextEventArgs> Determine;

        public AutoCompleteTextBox()
        {
            CandidatePopup = new Popup();
            CandidateListBox = new ListBox();

            CandidatePopup.Child = CandidateListBox;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            CandidateListBox.MouseUp += CandidateListBox_MouseUp;
            CandidateListBox.PreviewKeyDown += CandidateListBox_PreviewKeyDown;

            CandidateListBox.PreviewLostKeyboardFocus += CandidateListBox_PreviewLostKeyboardFocus;

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                Application.Current.MainWindow.Deactivated += MainWindow_Deactivated;
            }
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
                else if (e.Key == Key.Up)
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
                        DisableCandidateList = true;
                        Text = s;
                    }
                    else if (e.Key == Key.Down)
                    {
                        string s = History.Forward();
                        DisableCandidateList = true;
                        Text = s;
                    }
                }
            }
        }

        private void NotifyDetermine()
        {
            TextEventArgs ea = new TextEventArgs();

            string v = Text.Trim('\r', '\n', ' ', '\t');

            if (v.Length == 0)
            {
                return;
            }

            ea.Text = v;

            History.Add(Text);

            Determine(this, ea);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (Check())
            {
                if (DisableCandidateList)
                {
                    DisableCandidateList = false;
                    return;
                }

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

            string s = (string)(item.Content);

            string currentText = Text;

            currentText = currentText.Remove(mReplacePos, mReplaceLen);
            currentText = currentText.Insert(mReplacePos, s);

            Text = currentText;

            return true;
        }

        Regex WordPtn = new Regex("[a-zA-Z_0-9]+");

        int mReplacePos = -1;

        int mReplaceLen = 0;

        private bool Check()
        {
            string currentText = Text;
            int cpos = CaretIndex;

            if (string.IsNullOrEmpty(currentText))
            {
                return false;
            }

            if (CandidateList == null)
            {
                return false;
            }

            string s = null;

            MatchCollection mc = WordPtn.Matches(currentText);
            
            foreach(Match m in mc)
            {
                if (cpos >= m.Index && cpos <= m.Index + m.Length)
                {
                    mReplacePos = m.Index;
                    mReplaceLen = m.Length;
                    s = m.Value;

                    break;
                }
            }

            if (s == null)
            {
                s = currentText;
            }

            CandidateListBox.Items.Clear();

            foreach (var str in CandidateList)
            {
                string text = str as String;

                if (text.Contains(s))
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