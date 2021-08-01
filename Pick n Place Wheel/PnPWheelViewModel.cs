using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pick_n_Place_Wheel
{
    class PnPWheelViewModel : INotifyPropertyChanged
    {
        private static readonly PnPWheelViewModel instance = new PnPWheelViewModel();
        public static PnPWheelViewModel Instance { get { return instance; } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static ObservableCollection<string> _csvcolumn = new ObservableCollection<string>();

        public ObservableCollection<string> csvcolumn
        {
            get { return _csvcolumn; }
            set
            {
                _csvcolumn = value;

                OnPropertyChanged();
            }
        }


        public static ObservableCollection<string> wheel1poscount { get; set; }


        public static ObservableCollection<string> wheel2poscount { get; set; }


        private string _wheel1color = "White";
        public string wheel1color
        {
            get { return _wheel1color; }

            set
            {
                _wheel1color = value;
                PnPWheelViewModel.Instance.OnPropertyChanged();
            }
        }
        private string _wheel2color = "White";
        public string wheel2color
        {
            get { return _wheel2color; }

            set
            {
                _wheel2color = value;
                PnPWheelViewModel.Instance.OnPropertyChanged();
            }
        }


        public List<string> colors { get; set; }
        public List<string> w1slotcount { get; set; }
        public List<string> w2slotcount { get; set; }

        public List<string> units { get; set; }


        private void conStateChanged()
        {
            if (constate == 1)
            {
                connectionstatecolor = "Lime";
                conbuttontext = "Disconnect";
                connectionstate = "Connected to PnPWheel";
            }
            else if (constate == 2)
            {
                connectionstatecolor = "Red";
                conbuttontext = "Connect";
                connectionstate = "Not Connected";
            }
            else if (constate == 3)
            {
                connectionstatecolor = "Blue";
                conbuttontext = "Disconnect";
                connectionstate = "Connected (no project is loaded)";
            }
            else if (constate == 0)
            {
                connectionstatecolor = "Gold";
                conbuttontext = "Query";
            }
        }

        public int _constate = 0;
        public int constate
        {
            get { return _constate; }
            set { _constate = value; conStateChanged(); }
        }

        public string _conbuttontext = "Query";
        public string conbuttontext
        {
            get { return _conbuttontext; }
            set { _conbuttontext = value; PnPWheelViewModel.Instance.OnPropertyChanged(); }
        }

        public string _connectionstate;
        public string connectionstate
        {
            get { return _connectionstate; }
            set { _connectionstate = value; PnPWheelViewModel.Instance.OnPropertyChanged(); }
        }
        public string _connectionstatecolor;
        public string connectionstatecolor
        {
            get { return _connectionstatecolor; }
            set { _connectionstatecolor = value; PnPWheelViewModel.Instance.OnPropertyChanged(); }
        }

        public PnPWheelViewModel()
        {
            colors = new List<string>
            {
             "null","AliceBlue","AntiqueWhite","Aqua","Aquamarine","Azure","Beige","Bisque","Black","BlanchedAlmond","Blue","BlueViolet","Brown","BurlyWood","CadetBlue","Chartreuse","Chocolate","Coral","CornflowerBlue","Cornsilk","Crimson","Cyan","DarkBlue","DarkCyan","DarkGoldenrod","DarkGray","DarkGreen","DarkKhaki","DarkMagenta","DarkOliveGreen","DarkOrange","DarkOrchid","DarkRed","DarkSalmon","DarkSeaGreen","DarkSlateBlue","DarkSlateGray","DarkTurquoise","DarkViolet","DeepPink","DeepSkyBlue","DimGray","DodgerBlue","Firebrick","FloralWhite","ForestGreen","Fuchsia","Gainsboro","GhostWhite","Gold","Goldenrod","Gray","Green","GreenYellow","Honeydew","HotPink","IndianRed","Indigo","Ivory","Khaki","Lavender","LavenderBlush","LawnGreen","LemonChiffon","LightBlue","LightCoral","LightCyan","LightGoldenrodYellow","LightGray","LightGreen","LightPink","LightSalmon","LightSeaGreen","LightSkyBlue","LightSlateGray","LightSteelBlue","LightYellow","Lime","LimeGreen","Linen","Magenta","Maroon","MediumAquamarine","MediumBlue","MediumOrchid","MediumPurple","MediumSeaGreen","MediumSlateBlue","MediumSpringGreen","MediumTurquoise","MediumVioletRed","MidnightBlue","MintCream","MistyRose","Moccasin","NavajoWhite","Navy","OldLace","Olive","OliveDrab","Orange","OrangeRed","Orchid","PaleGoldenrod","PaleGreen","PaleTurquoise","PaleVioletRed","PapayaWhip","PeachPuff","Peru","Pink","Plum","PowderBlue","Purple","Red","RosyBrown","RoyalBlue","SaddleBrown","Salmon","SandyBrown","SeaGreen","SeaShell","Sienna","Silver","SkyBlue","SlateBlue","SlateGray","Snow","SpringGreen","SteelBlue","Tan","Teal","Thistle","Tomato","Transparent","Turquoise","Violet","Wheat","White","WhiteSmoke","Yellow","YellowGreen"
            };

            w1slotcount = new List<string>
            {
             "48","16"
            };
            w2slotcount = new List<string>
            {
             "24","8"
            };

            units = new List<string>
            {
                "mm",
                "inch"
            };

            wheel1poscount = new ObservableCollection<string>
            {
                "001","002","003","004","005","006","007","008","009","010"
                ,"011","012","013","014","015","016","017","018","019","020"
                ,"021","022","023","024","025","026","027","028","029","030"
                ,"031","032","033","034","035","036","037","038","039","040"
                ,"041","042","043","044","045","046","047","048","049","050"
                ,"051","052","053","054","055","056","057","058","059","060"
            };
            wheel2poscount = new ObservableCollection<string>
            {
               "001","002","003","004","005","006","007","008","009","010"
                ,"011","012","013","014","015","016","017","018","019","020"
                ,"021","022","023","024","025","026","027","028","029","030"
                ,"031","032","033","034","035","036","037","038","039","040"
                ,"041","042","043","044","045","046","047","048","049","050"
                ,"051","052","053","054","055","056","057","058","059","060"
            };
        }
    }
}
