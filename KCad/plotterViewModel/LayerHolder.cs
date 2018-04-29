using System.ComponentModel;

namespace Plotter
{
    public class LayerHolder : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private CadLayer mLayer;

        public uint ID
        {
            get { return mLayer.ID; }
        }

        public bool Locked
        {
            set
            {
                mLayer.Locked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Locked)));
            }

            get { return mLayer.Locked; }
        }

        public bool Visible
        {
            set
            {
                mLayer.Visible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Visible)));
            }

            get { return mLayer.Visible; }
        }

        public string Name
        {
            set
            {
                mLayer.Name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }

            get { return mLayer.Name; }
        }

        public LayerHolder(CadLayer layer)
        {
            mLayer = layer;
        }
    }
}
