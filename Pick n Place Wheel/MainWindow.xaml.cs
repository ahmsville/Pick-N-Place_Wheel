using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pick_n_Place_Wheel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        Ellipse ellipse = new Ellipse();

        int wheel1Slot = 0;
        int wheel2Slot = 0;

        Point boardsize = new Point();

        int pointerDotDia = 15;

        bool ack = false;
        int acktimeout = 50;

        string pnpProjectPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\PickNPlace Wheel\";
        string defaultpnpProjectPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\PickNPlace Wheel\";
        string csvpath;
        string toplayerimagepath;
        string buttomlayerimagepath;

        string configpath;
        PnPWheelViewModel PnPWheelVM = new PnPWheelViewModel();

        string boardCompDesignator;
        string boardCompName;
        string boardCompFootprint;
        string boardCompXcenter;
        string boardCompYcenter;
        string boardCompValue;
        string boardCompLayer;
        string boardunit;
        string boardwheel1color;
        string boardwheel2color;
        string boardwheelpos;

        bool wheel1needsreset = true;
        bool wheel2needsreset = true;



        List<List<string>> activeBoardPickNPlaceData;
        struct activeComponentInfo
        {
            public string name { get; set; }
            public string value { get; set; }
            public string footprint { get; set; }
            public string designator { get; set; }
            public Point pos { get; set; }
            public Ellipse pointer { get; set; }
        }

        List<activeComponentInfo> wheel1_activeComponents = new List<activeComponentInfo>();
        List<activeComponentInfo> wheel2_activeComponents = new List<activeComponentInfo>();
        List<activeComponentInfo> unassigned_activeComponents = new List<activeComponentInfo>();
        public MainWindow()
        {
            DataContext = new PnPWheelViewModel();
            PnPWheelViewModel.Instance.constate = 2;
            Blinktimer();
            InitializeComponent();
            createprojectfolder();
            
        }

        private void createprojectfolder()
        {
            if (!Directory.Exists(defaultpnpProjectPath))
            {
                Directory.CreateDirectory(defaultpnpProjectPath);
            }
        }
        private void StackPanel_MouseMove(object sender, MouseEventArgs e)//
        {

            Point point = e.GetPosition(imageviewer);

            // mouseposition.Text = imageviewer.ActualWidth.ToString() + "  ,  " + imageviewer.ActualHeight.ToString() + "\n" + point.X.ToString() + "  ,  " + point.Y.ToString();

            //mouseposition.RenderTransform = new TranslateTransform(point.X, point.Y);
            //pointer.RenderTransform = new TranslateTransform(point.X - 10, point.Y - 10);

            // activeComponentInfo ggg;
            //ggg.name = "";
            //ggg.designator = "";
            //ggg.value = "";
            //ggg.footprint = "";
            //ggg.posX = 1;
            //ggg.posY = 5;
            //ggg.pointer = new Ellipse();
            //ggg.pointer.
            //wheel1_activeComponents.Add(ggg);

            //wheel1_activeComponents.ElementAt(0);



            if (boardimage.Source != null)
            {
                imageviewer.Children.Remove(ellipse);
                ellipse.Height = pointerDotDia;
                ellipse.Width = pointerDotDia;
                ellipse.Fill = Brushes.DarkCyan;
                ellipse.StrokeThickness = 2;
                ellipse.Stroke = Brushes.Black;
                //imageviewer.Children.Add(ellipse);
                ellipse.HorizontalAlignment = HorizontalAlignment.Left;
                ellipse.VerticalAlignment = VerticalAlignment.Top;
                //ellipse.RenderTransform = new TranslateTransform(point.X - pointerDotDia / 2, point.Y - pointerDotDia / 2);
            }


        }

        public void get_activeComponentsInfoFromSlots()
        {
            //clear active component list

            //locate column for slot id 
            //locate active slot
            //get component value and footprint for active slot
            //scan csv for other components with the same value and footprint 
            //get and save position info and create pointer for components

        }
        public Point get_ActualCoordinates(Point compPos)
        {
            Point imagesize = new Point(imageviewer.ActualWidth, imageviewer.ActualHeight);
            Point scalingfactor = new Point(imagesize.X / boardsize.X, imagesize.Y / boardsize.Y);
            if (activatetoplayer.IsChecked == true)
            {
                compPos.X = compPos.X * scalingfactor.X;
                compPos.Y = imagesize.Y - (compPos.Y * scalingfactor.Y);
            }
            else if (activatebuttomlayer.IsChecked == true)
            {
                compPos.X = imagesize.X - (compPos.X * scalingfactor.X);
                compPos.Y = imagesize.Y - (compPos.Y * scalingfactor.Y);
            }

            //do conversion magic

            return compPos;
        }

        public void readCSV()
        {
            //locate and store column names
            var csvdata = new StreamReader(csvpath);

            int[] compareColumnCount = { 1, 2, 3 };

            int cnt = 0;

            while (!csvdata.EndOfStream)
            {
                string line = csvdata.ReadLine();
                line = line.Replace(",", ";");
                line = line.Replace("\"", "");
                compareColumnCount[cnt] = line.Count(x => x == ';');

                if (cnt < 2)
                {
                    cnt++;
                }
                else
                {
                    cnt = 0;
                }

                if (Array.TrueForAll(compareColumnCount, x => x == compareColumnCount[cnt]) && compareColumnCount[cnt] != 0)
                {
                    csvdata.ReadToEnd();
                }

            }
            csvdata.Close();

            int columnCount = compareColumnCount[0] + 1;

            //csv content array of lists
            List<List<string>> csvcontent = new List<List<string>>();

            for (int i = 0; i < columnCount; i++)
            {
                csvcontent.Add(new List<string>());
            }


            string[] csvread = System.IO.File.ReadAllLines(csvpath);
            var file = new System.IO.StreamWriter(csvpath);
            bool PnPWheelColumnCreated = false;
            bool isfirsttimeCSV = true;
            for (int i = 0; i < csvread.Count(); i++)
            {
                csvread[i] = csvread[i].Replace("\"", "");
                //first time csv
                if (isfirsttimeCSV)
                {
                    if (csvread[i].Contains(",,,,")) //remove consecutive commas
                    {
                            csvread[i] = csvread[i].Replace(",","");    
                    }
                    if (Regex.Matches(csvread[i], ",").Count == (columnCount - 1))
                    {
                        if (!PnPWheelColumnCreated && !csvread[i].Contains("PnPWheel Position"))
                        {
                            csvread[i] = csvread[i] + ",PnPWheel Position";
                            PnPWheelColumnCreated = true;
                            csvcontent.Add(new List<string>());
                            file.WriteLine(csvread[i]);
                        }
                        else if (PnPWheelColumnCreated && isfirsttimeCSV)
                        {
                            
                                csvread[i] = csvread[i] + ",unloaded";
                           
                            file.WriteLine(csvread[i]);
                        }
                        else
                        {
                            file.WriteLine(csvread[i]);
                            isfirsttimeCSV = false;
                        }

                        var value = csvread[i].Split(new string[] { "," }, StringSplitOptions.None);
                        for (int j = 0; j < value.Count(); j++)
                        {
                            //value[j] = value[j].Replace("\"", "");
                            csvcontent.ElementAt(j).Add(value[j]);
                            //csvcontent[0].ElementAt(0);
                        }

                    }
                    else
                    {
                        file.WriteLine(csvread[i]);
                    }
                }
                else
                {
                    if (Regex.Matches(csvread[i], ",").Count == (columnCount - 1))
                    {
                        if (csvread[i].Contains(",,,,")) //remove consecutive commas
                        {
                            //csvread[i] = csvread[i].Replace(",", "");
                        }
                        file.WriteLine(csvread[i]);
                        var value = csvread[i].Split(new string[] { "," }, StringSplitOptions.None);
                        for (int j = 0; j < value.Count(); j++)
                        {
                           // value[j] = value[j].Replace("\"", "");
                            csvcontent.ElementAt(j).Add(value[j]);
                            //csvcontent[0].ElementAt(0);
                        }
                    }
                }

            }
            file.Close();
            //modify csv file




            activeBoardPickNPlaceData = csvcontent;
            //load columns
            PnPWheelVM.csvcolumn.Clear();
            foreach (var col in activeBoardPickNPlaceData)
            {
                PnPWheelVM.csvcolumn.Add(col.ElementAt(0));
            }

        }



        private void loadboard_Click(object sender, RoutedEventArgs e)
        {

            //load board files
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = pnpProjectPath;
            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                pnpProjectPath = System.IO.Path.GetDirectoryName(dlg.FileName);

                //get csv
                string[] csvfiles = Directory.GetFiles(pnpProjectPath, "*.csv");
                if (csvfiles.Length == 1)
                {
                    csvpath = csvfiles[0];
                    boardfiles.Children.Clear();
                    Label label = new Label();
                    label.Content = ">> " + csvpath.Substring(csvpath.LastIndexOf(@"\") + 1);
                    boardfiles.Children.Add(label);
                    readCSV();

                }
                else
                {
                    MessageBox.Show("Multiple CSVs found");
                }

                //get board image
                toplayerIMG.Items.Clear();
                buttomlayerIMG.Items.Clear();
                string[] imagefiles = { "" };
                if (Directory.GetFiles(pnpProjectPath, "*.png") != null)
                {
                    imagefiles = Directory.GetFiles(pnpProjectPath, "*.png");

                }
                else
                {
                    imagefiles = Directory.GetFiles(pnpProjectPath, "*.jpg");
                }
                if (imagefiles != null)
                {
                    if (imagefiles.Count() == 2)
                    {
                        foreach (string img in imagefiles)
                        {

                            ComboBoxItem temp = new ComboBoxItem();
                            temp.Content = img;
                            toplayerIMG.Items.Add(temp);
                            ComboBoxItem temp2 = new ComboBoxItem();
                            temp2.Content = img;
                            buttomlayerIMG.Items.Add(temp2);

                            Label label = new Label();
                            label.Content = ">> " + img.Substring(img.LastIndexOf(@"\") + 1);
                            boardfiles.Children.Add(label);
                        }

                    }
                    else if (imagefiles.Count() == 1)
                    {
                        toplayerimagepath = imagefiles[0];
                        activatetoplayer.IsChecked = true;
                        boardimage.Source = new BitmapImage(new Uri(toplayerimagepath));
                        ComboBoxItem temp = new ComboBoxItem();
                        temp.Content = toplayerimagepath;
                        toplayerIMG.Items.Add(temp);

                        ComboBoxItem temp2 = new ComboBoxItem();
                        temp2.Content = "null";
                        buttomlayerIMG.Items.Add(temp2);

                        Label label = new Label();
                        label.Content = ">> " + toplayerimagepath.Substring(toplayerimagepath.LastIndexOf(@"\") + 1);
                        boardfiles.Children.Add(label);

                    }
                }


                //get config txt
                get_savedConfig();

                //populate update combobox
                populateUpdateCombobox();

            }

        }

        private void loadproject(string projectfolder)
        {
            //load board files

            pnpProjectPath = projectfolder;
            //get csv
            string[] csvfiles = Directory.GetFiles(pnpProjectPath, "*.csv");
            if (csvfiles.Length == 1)
            {
                csvpath = csvfiles[0];
                boardfiles.Children.Clear();
                Label label = new Label();
                label.Content = ">> " + csvpath.Substring(csvpath.LastIndexOf(@"\") + 1);
                boardfiles.Children.Add(label);
                readCSV();

            }
            else
            {
                MessageBox.Show("Multiple CSVs found");
            }

            //get board image
            toplayerIMG.Items.Clear();
            buttomlayerIMG.Items.Clear();
            string[] imagefiles = { "" };
            if (Directory.GetFiles(pnpProjectPath, "*.png") != null)
            {
                imagefiles = Directory.GetFiles(pnpProjectPath, "*.png");

            }
            else
            {
                imagefiles = Directory.GetFiles(pnpProjectPath, "*.jpg");
            }
            if (imagefiles != null)
            {
                if (imagefiles.Count() == 2)
                {
                    foreach (string img in imagefiles)
                    {

                        ComboBoxItem temp = new ComboBoxItem();
                        temp.Content = img;
                        toplayerIMG.Items.Add(temp);
                        ComboBoxItem temp2 = new ComboBoxItem();
                        temp2.Content = img;
                        buttomlayerIMG.Items.Add(temp2);

                        Label label = new Label();
                        label.Content = ">> " + img.Substring(img.LastIndexOf(@"\") + 1);
                        boardfiles.Children.Add(label);
                    }

                }
                else if (imagefiles.Count() == 1)
                {
                    toplayerimagepath = imagefiles[0];
                    activatetoplayer.IsChecked = true;
                    boardimage.Source = new BitmapImage(new Uri(toplayerimagepath));
                    ComboBoxItem temp = new ComboBoxItem();
                    temp.Content = toplayerimagepath;
                    toplayerIMG.Items.Add(temp);

                    ComboBoxItem temp2 = new ComboBoxItem();
                    temp2.Content = "null";
                    buttomlayerIMG.Items.Add(temp2);

                    Label label = new Label();
                    label.Content = ">> " + toplayerimagepath.Substring(toplayerimagepath.LastIndexOf(@"\") + 1);
                    boardfiles.Children.Add(label);

                }
            }


            //get config txt
            get_savedConfig();

            //populate update combobox
            populateUpdateCombobox();
        }
        private void get_savedConfig()
        {

            string[] configfiles = Directory.GetFiles(pnpProjectPath, "*.txt");
            if (configfiles.Length == 1)
            {
                configpath = configfiles[0];
                using (var reader = new StreamReader(configpath))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        string cline = line.Substring(0, (line.IndexOf("="))); //column name
                        string selection = line.Replace(cline + "=", ""); //combobox selection
                        switch (cline)
                        {
                            case "boardname":
                                boardprojectname.Text = selection;

                                break;
                            case "compname":
                                compname.SelectedValue = selection;
                                boardCompName = selection;
                                break;
                            case "compvalue":
                                compvalue.SelectedValue = selection;
                                boardCompValue = selection;
                                break;
                            case "compfootprint":
                                compfootprint.SelectedValue = selection;
                                boardCompFootprint = selection;
                                break;
                            case "compXaxis":
                                compXaxis.SelectedValue = selection;
                                boardCompXcenter = selection;
                                break;
                            case "compYaxis":
                                compYaxis.SelectedValue = selection;
                                boardCompYcenter = selection;
                                break;
                            case "boardwidth":
                                boardwidth.Text = selection;
                                boardsize.X = double.Parse(selection);
                                break;
                            case "boardheight":
                                boardheight.Text = selection;
                                boardsize.Y = double.Parse(selection);
                                break;
                            case "unit":
                                dimensionUnit.SelectedValue = selection;
                                boardunit = selection;
                                break;
                            case "boardDesignator":
                                compdesignator.SelectedValue = selection;
                                boardCompDesignator = selection;
                                break;
                            case "boardLayer":
                                complayer.SelectedValue = selection;
                                boardCompLayer = selection;
                                break;
                            case "toplayerimage":
                                toplayerIMG.SelectedIndex = Int32.Parse(selection);

                                break;
                            case "buttomlayerimage":
                                buttomlayerIMG.SelectedIndex = Int32.Parse(selection);

                                break;
                            case "layertag1":
                                activatetoplayer.Content = selection;
                                activatetoplayer.Tag = selection;
                                break;
                            case "layertag2":
                                activatebuttomlayer.Content = selection;
                                activatebuttomlayer.Tag = selection;
                                break;
                            case "wheel1slots":
                                wheel1cnt.SelectedValue = selection;
                                break;
                            case "wheel1color":
                                wheel1color.SelectedValue = selection;
                                boardwheel1color = selection;
                                break;
                            case "wheel2slots":
                                wheel2cnt.SelectedValue = selection;
                                break;
                            case "wheel2color":
                                wheel2color.SelectedValue = selection;
                                boardwheel2color = selection;
                                break;
                            case "wheelpos":
                                wheelpos.SelectedValue = selection;
                                boardwheelpos = selection;
                                break;

                        }
                    }
                }
            }
        }

        private void saveprojectsettings_Click(object sender, RoutedEventArgs e)
        {

            if (checkSelections())
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "PnPWheel Config File|*.txt";
                saveFileDialog1.Title = "Save a PnPWheel Config File";
                saveFileDialog1.InitialDirectory = pnpProjectPath;
                saveFileDialog1.ShowDialog();

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(saveFileDialog1.OpenFile()))//
                {
                    try
                    {
                        file.WriteLine("boardname=" + boardprojectname.Text);
                        file.WriteLine("compname=" + compname.SelectedValue.ToString());
                        file.WriteLine("compvalue=" + compvalue.SelectedValue.ToString());
                        file.WriteLine("compfootprint=" + compfootprint.SelectedValue.ToString());
                        file.WriteLine("compXaxis=" + compXaxis.SelectedValue.ToString());
                        file.WriteLine("compYaxis=" + compYaxis.SelectedValue.ToString());
                        file.WriteLine("boardwidth=" + boardwidth.Text);
                        file.WriteLine("boardheight=" + boardheight.Text);
                        file.WriteLine("unit=" + dimensionUnit.SelectedValue.ToString());
                        file.WriteLine("boardDesignator=" + compdesignator.SelectedValue.ToString());
                        file.WriteLine("boardLayer=" + complayer.SelectedValue.ToString());
                        file.WriteLine("toplayerimage=" + toplayerIMG.SelectedIndex.ToString());
                        file.WriteLine("buttomlayerimage=" + buttomlayerIMG.SelectedIndex.ToString());
                        file.WriteLine("layertag1=" + activatetoplayer.Tag.ToString());
                        file.WriteLine("layertag2=" + activatebuttomlayer.Tag.ToString());
                        file.WriteLine("wheel1slots=" + wheel1cnt.SelectedValue.ToString());
                        file.WriteLine("wheel1color=" + wheel1color.SelectedValue.ToString());
                        file.WriteLine("wheel2slots=" + wheel2cnt.SelectedValue.ToString());
                        file.WriteLine("wheel2color=" + wheel2color.SelectedValue.ToString());
                        file.WriteLine("wheelpos=" + wheelpos.SelectedValue.ToString());

                        MessageBox.Show("Configuration Saved");
                    }
                    catch (Exception)
                    {


                    }

                }


                boardCompName = compname.SelectedValue.ToString();
                boardCompDesignator = compdesignator.SelectedValue.ToString();
                boardCompValue = compvalue.SelectedValue.ToString();
                boardCompFootprint = compfootprint.SelectedValue.ToString();
                boardCompXcenter = compXaxis.SelectedValue.ToString();
                boardCompYcenter = compYaxis.SelectedValue.ToString();
                boardCompLayer = complayer.SelectedValue.ToString();
                boardunit = dimensionUnit.SelectedValue.ToString();
                boardwheel1color = wheel1color.SelectedValue.ToString();
                boardwheel2color = wheel2color.SelectedValue.ToString();
                boardwheelpos = wheelpos.SelectedValue.ToString();


                get_savedConfig();
                populateUpdateCombobox();
            }
            else
            {
                MessageBox.Show("Invalid Selections");
            }
        }

        private bool checkSelections()
        {
            if (compname.SelectedValue != null
                && compvalue.SelectedValue != null
                 && compfootprint.SelectedValue != null
                  && compXaxis.SelectedValue != null
                   && compYaxis.SelectedValue != null
                    && isdouble(boardwidth.Text)
                     && isdouble(boardheight.Text)
                      && dimensionUnit.SelectedValue != null
                       && compdesignator.SelectedValue != null
                        && complayer.SelectedValue != null
                         && toplayerIMG.SelectedValue != null
                          && buttomlayerIMG.SelectedValue != null
                           && activatetoplayer.Tag != null
                            && activatebuttomlayer.Tag != null
                            && wheel1cnt.SelectedValue != null
                            && wheel2cnt.SelectedValue != null
                            && wheel1color.SelectedValue != null
                            && wheel2color.SelectedValue != null
                            && wheelpos.Text == "PnPWheel Position"

                )
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool isdouble(string text)
        {
            bool ret = false;
            try
            {
                double dd = double.Parse(text);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;

            }

            return ret;
        }
        private bool updateWheelPosition(string c_name, string c_value, string c_footprint, string w_pos)
        {
            bool res = false;
            string[] csvread = System.IO.File.ReadAllLines(csvpath);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(csvpath))
            {
                foreach (string line in csvread)
                {
                    int passmark = 0;
                    try
                    {
                        if (Regex.Matches(line, ",").Count == (activeBoardPickNPlaceData.Count - 1))//COLUMNS MATCH
                        {
                            var splt = line.Split(new string[] { "," }, StringSplitOptions.None);
                            for (int i = 0; i < splt.Length; i++)
                            {
                                if (splt[i] == c_name)
                                {
                                    passmark = passmark + 1;
                                }
                                if (splt[i] == c_value)
                                {
                                    passmark = passmark + 1;
                                }
                                if (splt[i] == c_footprint)
                                {
                                    passmark = passmark + 1;
                                }
                            }
                        }
                        
                        if (passmark == 3)
                        {
                           
                           
                                int lineindex = Array.FindIndex(csvread, x => x == line);
                                //string lineplusnext = csvread[lineindex] + csvread[lineindex + 1];
                                int ggg = line.LastIndexOf(",") + 1;
                                string sub = line.Substring(ggg);
                                string hhh = line.Replace(sub, w_pos);
                                file.WriteLine(hhh);
                            
                        }
                        else
                        {
                            file.WriteLine(line);
                        }
                        res = true;

                    }
                    catch (Exception)
                    {

                        throw;

                    }

                }
            }
            readCSV();
            get_savedConfig();
            populateUpdateCombobox();
            return res;
        }

        private void populateUpdateCombobox()
        {
            //populate update combobox

            valueandfootprint.Items.Clear();
            for (int i = 1; i < activeBoardPickNPlaceData.ElementAt(0).Count; i++)
            {
                if (boardCompValue != null)
                {
                    string naMe = activeBoardPickNPlaceData.ElementAt(compname.Items.IndexOf(boardCompName)).ElementAt(i);
                    string vaLue = activeBoardPickNPlaceData.ElementAt(compname.Items.IndexOf(boardCompValue)).ElementAt(i);
                    string footPrint = activeBoardPickNPlaceData.ElementAt(compname.Items.IndexOf(boardCompFootprint)).ElementAt(i);
                    string wheelPos = activeBoardPickNPlaceData.ElementAt(compname.Items.IndexOf(boardwheelpos)).ElementAt(i);
                    if (wheelPos != "unloaded")
                    {
                        if (wheelPos.Contains("w1"))
                        {

                            ComboBoxItem temp = new ComboBoxItem();
                            //temp.Name = vaLue + " , " + footPrint;
                            temp.Content = naMe + " , " + vaLue + " , " + footPrint;
                            temp.Tag = i.ToString();
                            temp.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("lime");
                            valueandfootprint.Items.Add(temp);

                            //wheelPos = wheelPos.Replace("w1_", "");
                            //newpos_wheel1.SelectedValue = wheelPos;

                        }
                        else if (wheelPos.Contains("w2"))
                        {
                            ComboBoxItem temp = new ComboBoxItem();
                            // temp.Name = vaLue + " , " + footPrint;
                            temp.Content = naMe + " , " + vaLue + " , " + footPrint;
                            temp.Tag = i.ToString();
                            temp.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("lime");
                            valueandfootprint.Items.Add(temp);

                            //wheelPos = wheelPos.Replace("w2_", "");
                            //newpos_wheel2.SelectedValue = wheelPos;

                        }
                    }
                    else
                    {
                        ComboBoxItem temp = new ComboBoxItem();
                        //temp.Name = vaLue + " , " + footPrint;
                        temp.Content = naMe + " , " + vaLue + " , " + footPrint;
                        temp.Tag = i.ToString();
                        temp.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("OrangeRed");
                        valueandfootprint.Items.Add(temp);
                    }
                    //PnPWheelVM.compvalue_compfootprint.Add(vaLue + " , " + footPrint);
                }


            }
            if (!valueandfootprint.Items.IsEmpty && PnPWheelViewModel.Instance.constate == 3)
            {
                PnPWheelViewModel.Instance.constate = 1;
            }


        }

        private void updateCompPosition_Click(object sender, RoutedEventArgs e)
        {
            int selectedindex = valueandfootprint.SelectedIndex;
            string Wpos = "";
            if (newpos_wheel1.SelectedValue != null && newpos_wheel2.SelectedValue != null)
            {
                MessageBox.Show("Select only one wheel slot");
                newpos_wheel1.SelectedValue = null;
                newpos_wheel2.SelectedValue = null;
            }
            else if (newpos_wheel1.SelectedValue == null && newpos_wheel2.SelectedValue == null)
            {
                MessageBox.Show("no slot selected");
            }
            else
            {
                if (newpos_wheel1.SelectedValue != null)
                {
                    Wpos = "w1_" + newpos_wheel1.Text;
                }
                else if (newpos_wheel2.SelectedValue != null)
                {
                    Wpos = "w2_" + newpos_wheel2.Text;
                }

                var splt = valueandfootprint.Text.Split(new string[] { " , " }, StringSplitOptions.None);

                if (updateWheelPosition(splt[0], splt[1], splt[2], Wpos))
                {
                    MessageBox.Show("new position added");
                }
            }
            valueandfootprint.SelectedIndex = selectedindex;
        }

        int activenewposcombobox = 2;
        private void newpos_wheel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (activenewposcombobox == 2)
            {
                activenewposcombobox = 0;
            }
            ComboBox combo = e.Source as ComboBox;
            if (combo.Name == "newpos_wheel1")
            {
                if (activenewposcombobox == 0)
                {
                    if (newpos_wheel2.SelectedValue == null)
                    {
                        activenewposcombobox = 0;
                    }
                    else
                    {
                        activenewposcombobox = 1;

                        newpos_wheel2.SelectedValue = null;
                    }

                }
                else
                {
                    activenewposcombobox = 0;
                }

            }
            else if (combo.Name == "newpos_wheel2")
            {
                if (activenewposcombobox == 0)
                {
                    if (newpos_wheel1.SelectedValue == null)
                    {
                        activenewposcombobox = 0;
                    }
                    else
                    {
                        activenewposcombobox = 1;

                        newpos_wheel1.SelectedValue = null;
                    }

                }
                else
                {
                    activenewposcombobox = 0;
                }
            }

        }

        private void valueandfootprint_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (valueandfootprint.SelectedIndex >= 0)
            {
                //int locationinboarddatalist = compname.Items.IndexOf(valueandfootprint.SelectedValue);

                //get list of designators that match the selection
                List<int> matchedDesignators = new List<int>();

                //filter by component name
                int namelistindex = compname.Items.IndexOf(boardCompName);
                string nametomatch = activeBoardPickNPlaceData.ElementAt(namelistindex).ElementAt(valueandfootprint.SelectedIndex + 1);
                for (int i = 0; i < activeBoardPickNPlaceData.ElementAt(namelistindex).Count; i++)
                {
                    if (activeBoardPickNPlaceData.ElementAt(namelistindex).ElementAt(i).Equals(nametomatch))
                    {
                        matchedDesignators.Add(i);
                    }
                }
                //filter by component value
                int valuelistindex = compname.Items.IndexOf(boardCompValue);
                string valuetomatch = activeBoardPickNPlaceData.ElementAt(valuelistindex).ElementAt(valueandfootprint.SelectedIndex + 1);
                for (int i = 0; i < activeBoardPickNPlaceData.ElementAt(valuelistindex).Count; i++)
                {
                    if (!activeBoardPickNPlaceData.ElementAt(valuelistindex).ElementAt(i).Equals(valuetomatch))
                    {
                        matchedDesignators.RemoveAll(x => x == i);
                    }
                }
                //filter by component footprint
                int footprintlistindex = compname.Items.IndexOf(boardCompFootprint);
                string footprinttomatch = activeBoardPickNPlaceData.ElementAt(footprintlistindex).ElementAt(valueandfootprint.SelectedIndex + 1);
                for (int i = 0; i < activeBoardPickNPlaceData.ElementAt(footprintlistindex).Count; i++)
                {
                    if (!activeBoardPickNPlaceData.ElementAt(footprintlistindex).ElementAt(i).Equals(footprinttomatch))
                    {
                        matchedDesignators.RemoveAll(x => x == i);
                    }
                }
                //filter by component layer
                int layerlistindex = compname.Items.IndexOf(boardCompLayer);
                string layertomatch = activeBoardPickNPlaceData.ElementAt(layerlistindex).ElementAt(valueandfootprint.SelectedIndex + 1);
                for (int i = 0; i < activeBoardPickNPlaceData.ElementAt(layerlistindex).Count; i++)
                {
                    if (!activeBoardPickNPlaceData.ElementAt(layerlistindex).ElementAt(i).Equals(layertomatch))
                    {
                        matchedDesignators.RemoveAll(x => x == i);
                    }
                }

                //select appropriate layer image
                if (activatetoplayer.Tag.ToString() == layertomatch)
                {
                    activatetoplayer.IsChecked = true;
                    boardimage.UpdateLayout();
                }
                else if (activatebuttomlayer.Tag.ToString() == layertomatch)
                {
                    activatebuttomlayer.IsChecked = true;
                    boardimage.UpdateLayout();
                }


                int desinatorlistindex = compname.Items.IndexOf(boardCompDesignator);
                W1_activecompdesignators.Content = "";
                W2_activecompdesignators.Content = "";
                //get containing wheel slot
                int wheelposlistindex = compname.Items.IndexOf(boardwheelpos);
                string wheelpos = activeBoardPickNPlaceData.ElementAt(wheelposlistindex).ElementAt(valueandfootprint.SelectedIndex + 1);

                if (wheelpos.Contains("w1"))
                {
                    newpos_wheel1.SelectedValue = wheelpos.Replace("w1_", "");
                    W1_activecompdesc.Content = nametomatch;
                    W1_activecompvalue.Content = valuetomatch;
                    W1_activecompfootprint.Content = footprinttomatch;
                    W1_activeslot.Content = wheelpos.Replace("w1_", "");
                    W1_activecomplayer.Content = layertomatch;
                    wheelone.IsEnabled = true;
                    wheeltwo.IsEnabled = false;

                }
                else if (wheelpos.Contains("w2"))
                {
                    newpos_wheel2.SelectedValue = wheelpos.Replace("w2_", "");
                    W2_activecompdesc.Content = nametomatch;
                    W2_activecompvalue.Content = valuetomatch;
                    W2_activecompfootprint.Content = footprinttomatch;
                    W2_activeslot.Content = wheelpos.Replace("w2_", "");
                    W2_activecomplayer.Content = layertomatch;
                    wheeltwo.IsEnabled = true;
                    wheelone.IsEnabled = false;
                }


                int xposlistindex = compname.Items.IndexOf(boardCompXcenter);
                int yposlistindex = compname.Items.IndexOf(boardCompYcenter);

                foreach (var child in wheel1_activeComponents)
                {
                    imageviewer.Children.Remove(child.pointer);
                }
                foreach (var child in wheel2_activeComponents)
                {
                    imageviewer.Children.Remove(child.pointer);
                }
                foreach (var child in unassigned_activeComponents)
                {
                    imageviewer.Children.Remove(child.pointer);
                }
                wheel1_activeComponents.Clear();
                wheel2_activeComponents.Clear();
                unassigned_activeComponents.Clear();
                foreach (var ln in matchedDesignators)
                {


                    string posX = activeBoardPickNPlaceData.ElementAt(xposlistindex).ElementAt(ln);
                    string posY = activeBoardPickNPlaceData.ElementAt(yposlistindex).ElementAt(ln);

                    //create pointers for component  
                    if (wheelpos.Contains("w1"))
                    {
                        W1_activecompdesignators.Content = W1_activecompdesignators.Content + " , " + (activeBoardPickNPlaceData.ElementAt(desinatorlistindex).ElementAt(ln));

                        activeComponentInfo active = new activeComponentInfo();
                        active.name = nametomatch;
                        active.value = valuetomatch;
                        active.footprint = footprinttomatch;
                        active.pos = new Point(double.Parse(posX, System.Globalization.CultureInfo.InvariantCulture), double.Parse(posY, System.Globalization.CultureInfo.InvariantCulture));

                        Ellipse dot = new Ellipse();
                        dot.Height = pointerDotDia;
                        dot.Width = pointerDotDia;
                        dot.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(wheel1color.Text);
                        dot.StrokeThickness = 2;
                        dot.Stroke = Brushes.Black;
                        dot.HorizontalAlignment = HorizontalAlignment.Left;
                        dot.VerticalAlignment = VerticalAlignment.Top;

                        active.pointer = dot;

                        wheel1_activeComponents.Add(active);

                    }
                    else if (wheelpos.Contains("w2"))
                    {
                        W2_activecompdesignators.Content = W2_activecompdesignators.Content + " , " + (activeBoardPickNPlaceData.ElementAt(desinatorlistindex).ElementAt(ln));

                        activeComponentInfo active = new activeComponentInfo();
                        active.name = nametomatch;
                        active.value = valuetomatch;
                        active.footprint = footprinttomatch;
                        active.pos = new Point(double.Parse(posX, System.Globalization.CultureInfo.InvariantCulture), double.Parse(posY, System.Globalization.CultureInfo.InvariantCulture));

                        Ellipse dot = new Ellipse();
                        dot.Height = pointerDotDia;
                        dot.Width = pointerDotDia;
                        dot.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(wheel2color.Text);
                        dot.StrokeThickness = 2;
                        dot.Stroke = Brushes.Black;
                        dot.HorizontalAlignment = HorizontalAlignment.Left;
                        dot.VerticalAlignment = VerticalAlignment.Top;

                        active.pointer = dot;

                        wheel2_activeComponents.Add(active);
                    }
                    else
                    {
                        activeComponentInfo active = new activeComponentInfo();
                        active.name = nametomatch;
                        active.value = valuetomatch;
                        active.footprint = footprinttomatch;
                        active.pos = new Point(double.Parse(posX, System.Globalization.CultureInfo.InvariantCulture), double.Parse(posY, System.Globalization.CultureInfo.InvariantCulture));

                        Ellipse dot = new Ellipse();
                        dot.Height = pointerDotDia;
                        dot.Width = pointerDotDia;
                        dot.Fill = Brushes.White;
                        dot.StrokeThickness = 2;
                        dot.Stroke = Brushes.Red;
                        dot.HorizontalAlignment = HorizontalAlignment.Left;
                        dot.VerticalAlignment = VerticalAlignment.Top;

                        active.pointer = dot;
                        unassigned_activeComponents.Add(active);
                    }



                }





                drawPointers();
                /*
                foreach (var ln in matchedDesignators)
                {
                    if (wheelpos.Contains("w1"))
                    {
                        W1_activecompdesignators.Content = W1_activecompdesignators.Content + " , " + (activeBoardPickNPlaceData.ElementAt(desinatorlistindex).ElementAt(ln));
                    }
                    else if (wheelpos.Contains("w2"))
                    {
                        W2_activecompdesignators.Content = W2_activecompdesignators.Content + " , " + (activeBoardPickNPlaceData.ElementAt(desinatorlistindex).ElementAt(ln));
                    }


                }
                */


                // activeBoardPickNPlaceData.ElementAt((activeBoardPickNPlaceData.Count - 1)).ElementAt(locationinboarddatalist+1);
            }

        }

        private void drawPointers()
        {

            foreach (var actcp in wheel1_activeComponents)
            {
                if (!imageviewer.Children.Contains(actcp.pointer))
                {
                    imageviewer.Children.Add(actcp.pointer);
                    Point correctedpos = get_ActualCoordinates(actcp.pos);
                    actcp.pointer.RenderTransform = new TranslateTransform(correctedpos.X - pointerDotDia / 2, correctedpos.Y - pointerDotDia / 2);
                }

            }
            foreach (var actcp in wheel2_activeComponents)
            {
                if (!imageviewer.Children.Contains(actcp.pointer))
                {
                    imageviewer.Children.Add(actcp.pointer);
                    Point correctedpos = get_ActualCoordinates(actcp.pos);
                    actcp.pointer.RenderTransform = new TranslateTransform(correctedpos.X - pointerDotDia / 2, correctedpos.Y - pointerDotDia / 2);
                }
            }
            foreach (var actcp in unassigned_activeComponents)
            {
                if (!imageviewer.Children.Contains(actcp.pointer))
                {
                    imageviewer.Children.Add(actcp.pointer);
                    Point correctedpos = get_ActualCoordinates(actcp.pos);
                    actcp.pointer.RenderTransform = new TranslateTransform(correctedpos.X - pointerDotDia / 2, correctedpos.Y - pointerDotDia / 2);
                }
            }
        }

        private void complayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (complayer.Items.Count != 0)
            {
                if (complayer.SelectedItem != null)
                {
                    var layerlist = activeBoardPickNPlaceData.ElementAt(complayer.SelectedIndex);
                    List<string> layers = new List<string>();
                    foreach (var dt in layerlist)
                    {
                        if (!layers.Contains(dt))
                        {
                            if (layerlist.IndexOf(dt) != 0)
                            {
                                layers.Add(dt);
                            }

                        }
                    }
                    if (layers.Count() == 2)
                    {
                        foreach (string lay in layers)
                        {
                            if (lay.IndexOf("op", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                activatetoplayer.Content = lay;
                                activatetoplayer.Tag = lay;
                                activatetoplayer.IsEnabled = true;
                            }
                            else if (lay.IndexOf("om", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                activatebuttomlayer.Content = layers.ElementAt(1);
                                activatebuttomlayer.Tag = layers.ElementAt(1);
                                activatebuttomlayer.IsEnabled = true;
                            }
                        }


                    }
                    else if (layers.Count() == 1)
                    {
                        activatetoplayer.Content = layers.ElementAt(0);
                        activatetoplayer.Tag = layers.ElementAt(0);
                        activatebuttomlayer.Content = "null";
                        activatebuttomlayer.Tag = "null";
                        activatebuttomlayer.IsEnabled = false;
                    }
                    else
                    {
                        MessageBox.Show("Invalid BoardLayer column selected");
                        complayer.SelectedItem = null;
                    }
                }
                
            }


        }

        private void activatelayer_Checked(object sender, RoutedEventArgs e)
        {
            if (activatetoplayer.IsChecked == true)
            {
                boardimage.Source = new BitmapImage(new Uri(toplayerimagepath));
            }
            else if (activatebuttomlayer.IsChecked == true)
            {
                boardimage.Source = new BitmapImage(new Uri(buttomlayerimagepath));
            }
        }

        private void toplayerIMG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).Items.Count != 0)
            {
                toplayerimagepath = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string;
                boardimage.Source = new BitmapImage(new Uri(toplayerimagepath));

            }

        }

        private void buttomlayerIMG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).Items.Count != 0)
            {
                buttomlayerimagepath = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string;
            }
        }

        private void boardprojectname_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            boardprojectname.IsReadOnly = false;
        }

        private void boardprojectname_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                boardprojectname.IsReadOnly = true;
            }
        }

        private void gotoprevproject_Click(object sender, RoutedEventArgs e)
        {
            string[] projectsfolders = Directory.GetDirectories(defaultpnpProjectPath);
            string prevproject = "";
            foreach (var folder in projectsfolders)
            {
                if (pnpProjectPath.Contains(folder))
                {
                    int previndex = Array.FindIndex(projectsfolders, x => x == folder);
                    if (previndex == 0)
                    {
                        previndex = projectsfolders.Count() - 1;
                        prevproject = projectsfolders[previndex];
                    }
                    else
                    {
                        previndex = (Array.FindIndex(projectsfolders, x => x == folder)) - 1;
                        prevproject = projectsfolders[previndex];
                    }
                }
            }
            if (prevproject != "")
            {
                foreach (var child in wheel1_activeComponents)
                {
                    imageviewer.Children.Remove(child.pointer);
                }
                foreach (var child in wheel2_activeComponents)
                {
                    imageviewer.Children.Remove(child.pointer);
                }
                foreach (var child in unassigned_activeComponents)
                {
                    imageviewer.Children.Remove(child.pointer);
                }
                resetControls();
                loadproject(prevproject);
            }

        }

        private void gotonextproject_Click(object sender, RoutedEventArgs e)
        {
            string[] projectsfolders = Directory.GetDirectories(defaultpnpProjectPath);
            string nextproject = "";
            foreach (var folder in projectsfolders)
            {
                if (pnpProjectPath.Contains(folder))
                {
                    int previndex = Array.FindIndex(projectsfolders, x => x == folder);
                    if (previndex == projectsfolders.Count() - 1)
                    {
                        previndex = 0;
                        nextproject = projectsfolders[previndex];
                    }
                    else
                    {
                        previndex = (Array.FindIndex(projectsfolders, x => x == folder)) + 1;
                        nextproject = projectsfolders[previndex];
                    }
                }
            }
            if (nextproject != "")
            {
                foreach (var child in wheel1_activeComponents)
                {
                    imageviewer.Children.Remove(child.pointer);
                }
                foreach (var child in wheel2_activeComponents)
                {
                    imageviewer.Children.Remove(child.pointer);
                }
                foreach (var child in unassigned_activeComponents)
                {
                    imageviewer.Children.Remove(child.pointer);
                }
                resetControls();
                loadproject(nextproject);
            }
        }

        private void resetControls()
        {
            newpos_wheel1.SelectedValue = null;
            W1_activecompdesc.Content = "Description";
            W1_activecompvalue.Content = "Value";
            W1_activecompfootprint.Content = "Footprint";
            W1_activeslot.Content = "0";
            W1_activecomplayer.Content = "Layer";
            W1_activecompdesignators.Content = "Designators";

            newpos_wheel2.SelectedValue = null;
            W2_activecompdesc.Content = "Description";
            W2_activecompvalue.Content = "Value";
            W2_activecompfootprint.Content = "Footprint";
            W2_activeslot.Content = "0";
            W2_activecomplayer.Content = "Layer";
            W2_activecompdesignators.Content = "Designators";


            wheeltwo.IsEnabled = true;
            wheelone.IsEnabled = true;
        }

        static SerialPort _serialPort;
        string inputstring = "";
        private int con_try = 5;
        private string[] ports;
        string pnpSerialPort = "";
        bool connected = false;
        private Timer blinkTimer;
        private bool blinkerbool = false;

        public Timer COMportQTimer { get; private set; }

        public void connectToSerialDevice(string portname)
        {

            _serialPort = new SerialPort();
            _serialPort.PortName = portname;
            _serialPort.BaudRate = 250000;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.DataReceived += DataReceivedHandler;

            //open port
            try
            {
                int tryturn = 0;
                while (!_serialPort.IsOpen && tryturn < con_try)
                {
                    _serialPort.Open();
                    setDTR();
                    tryturn++;

                }
            }
            catch (Exception)
            {
                //MessageBox.Show("Failed to connect to PnPWheel");
                PnPWheelViewModel.Instance.constate = 2;
            }
            if (_serialPort.IsOpen)
            {
                if (valueandfootprint.Items.IsEmpty)
                {
                    //MessageBox.Show("connected to PnPWheel");
                    PnPWheelViewModel.Instance.constate = 3;

                }
                else
                {
                    //MessageBox.Show("connected to PnPWheel");
                    PnPWheelViewModel.Instance.constate = 1;
                    connected = true;
                    updatewheelslotcount(1);
                    updatewheelslotcount(2);
                }
            }


        }


        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {

            char[] inChar = new char[6];

            while (_serialPort.BytesToRead > 0)
            {
                try
                {
                    _serialPort.Read(inChar, 0, 6);
                    inputstring = new string(inChar);


                }
                catch (Exception)
                {

                }
                if (inputstring.StartsWith("w1"))
                {
                    // MessageBox.Show(inputstring);
                    this.Dispatcher.Invoke(() =>
                    {
                        processWheel1(inputstring);
                    });


                }
                else if (inputstring.StartsWith("w2"))
                {
                    // MessageBox.Show(inputstring);
                    this.Dispatcher.Invoke(() =>
                    {
                        processWheel2(inputstring);
                    });
                }
                else if (inputstring.Contains("ack111"))
                {
                    ack = true;
                }
            }

        }

        private void processWheel1(string slot)
        {
            if (!valueandfootprint.Items.IsEmpty)
            {
                if (slot.Contains("x"))
                {
                    wheel1status.Text = "Requires Position Reset";
                    wheel1needsreset = true;
                    blinkTimer.Enabled = true;
                }
                else
                {
                    wheel1needsreset = false;
                    wheel1status.Text = "is Good To Go";
                    //get containing wheel slot
                    int wheelposlistindex = compname.Items.IndexOf(boardwheelpos);
                    if (activeBoardPickNPlaceData.ElementAt(wheelposlistindex).Contains(slot))//slot is filled
                    {
                        int slotindexinlist = activeBoardPickNPlaceData.ElementAt(wheelposlistindex).IndexOf(slot);
                        valueandfootprint.SelectedIndex = slotindexinlist - 1;
                        //get name of component in slot
                        int namelistindex = compname.Items.IndexOf(boardCompName);
                        string name = activeBoardPickNPlaceData.ElementAt(namelistindex).ElementAt(slotindexinlist);
                        //get name of component in slot
                        int valuelistindex = compname.Items.IndexOf(boardCompValue);
                        string value = activeBoardPickNPlaceData.ElementAt(valuelistindex).ElementAt(slotindexinlist);
                        //get name of component in slot
                        int footprintlistindex = compname.Items.IndexOf(boardCompFootprint);
                        string footprint = activeBoardPickNPlaceData.ElementAt(footprintlistindex).ElementAt(slotindexinlist);

                        wheel1slotcomponent.Text = name + " , " + value + " , " + footprint;
                        wheel1activeslot.Text = slot.Replace("w1_", "");
                        newpos_wheel1.SelectedValue = wheel1activeslot.Text;


                        //get list of designators that match the selection
                        List<int> matchedDesignators = new List<int>();

                        //filter by component name

                        for (int i = 0; i < activeBoardPickNPlaceData.ElementAt(namelistindex).Count; i++)
                        {
                            if (activeBoardPickNPlaceData.ElementAt(namelistindex).ElementAt(i).Equals(name))
                            {
                                matchedDesignators.Add(i);
                            }
                        }
                        //filter by component value

                        for (int i = 0; i < activeBoardPickNPlaceData.ElementAt(valuelistindex).Count; i++)
                        {
                            if (!activeBoardPickNPlaceData.ElementAt(valuelistindex).ElementAt(i).Equals(value))
                            {
                                matchedDesignators.RemoveAll(x => x == i);
                            }
                        }
                        //filter by component footprint

                        for (int i = 0; i < activeBoardPickNPlaceData.ElementAt(footprintlistindex).Count; i++)
                        {
                            if (!activeBoardPickNPlaceData.ElementAt(footprintlistindex).ElementAt(i).Equals(footprint))
                            {
                                matchedDesignators.RemoveAll(x => x == i);
                            }
                        }
                        //filter by component layer
                        int layerlistindex = compname.Items.IndexOf(boardCompLayer);
                        string layer = activeBoardPickNPlaceData.ElementAt(layerlistindex).ElementAt(slotindexinlist);
                        for (int i = 0; i < activeBoardPickNPlaceData.ElementAt(layerlistindex).Count; i++)
                        {
                            if (!activeBoardPickNPlaceData.ElementAt(layerlistindex).ElementAt(i).Equals(layer))
                            {
                                matchedDesignators.RemoveAll(x => x == i);
                            }
                        }

                        //select appropriate layer image
                        if (activatetoplayer.Tag.ToString() == layer)
                        {
                            activatetoplayer.IsChecked = true;
                            boardimage.UpdateLayout();
                        }
                        else if (activatebuttomlayer.Tag.ToString() == layer)
                        {
                            activatebuttomlayer.IsChecked = true;
                            boardimage.UpdateLayout();
                        }



                        int xposlistindex = compname.Items.IndexOf(boardCompXcenter);
                        int yposlistindex = compname.Items.IndexOf(boardCompYcenter);

                        foreach (var child in wheel1_activeComponents)
                        {
                            imageviewer.Children.Remove(child.pointer);
                        }
                        wheel1_activeComponents.Clear();

                        foreach (var child in unassigned_activeComponents)
                        {
                            imageviewer.Children.Remove(child.pointer);
                        }
                        unassigned_activeComponents.Clear();

                        foreach (var ln in matchedDesignators)
                        {


                            string posX = activeBoardPickNPlaceData.ElementAt(xposlistindex).ElementAt(ln);
                            string posY = activeBoardPickNPlaceData.ElementAt(yposlistindex).ElementAt(ln);

                            //create pointers for component  

                            activeComponentInfo active1 = new activeComponentInfo();
                            active1.name = name;
                            active1.value = value;
                            active1.footprint = footprint;
                            active1.pos = new Point(double.Parse(posX, System.Globalization.CultureInfo.InvariantCulture), double.Parse(posY, System.Globalization.CultureInfo.InvariantCulture));

                            Ellipse dot = new Ellipse();
                            dot.Uid = "w1dot" + ln.ToString();
                            dot.Height = pointerDotDia;
                            dot.Width = pointerDotDia;
                            dot.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(wheel1color.Text);
                            PnPWheelViewModel.Instance.wheel1color = wheel1color.Text;
                            dot.StrokeThickness = 2;
                            dot.Stroke = Brushes.Black;
                            dot.HorizontalAlignment = HorizontalAlignment.Left;
                            dot.VerticalAlignment = VerticalAlignment.Top;

                            active1.pointer = dot;

                            wheel1_activeComponents.Add(active1);


                        }
                        drawPointers();
                    }
                    else //slot is empty
                    {
                        foreach (var child in wheel1_activeComponents)
                        {
                            imageviewer.Children.Remove(child.pointer);
                        }
                        wheel1_activeComponents.Clear();
                        wheel1slotcomponent.Text = "Slot Empty";
                        wheel1activeslot.Text = slot.Replace("w1_", "");
                        newpos_wheel1.SelectedValue = wheel1activeslot.Text;

                    }

                }
            }

        }
        private void processWheel2(string slot)
        {
            if (!valueandfootprint.Items.IsEmpty)
            {
                if (slot.Contains("x"))
                {
                    wheel2status.Text = "Requires Position Reset";
                    wheel2needsreset = true;
                    blinkTimer.Enabled = true;
                }
                else
                {
                    wheel2needsreset = false;
                    wheel2status.Text = "is Good To Go";
                    //get containing wheel slot
                    int wheelposlistindex = compname.Items.IndexOf(boardwheelpos);
                    if (activeBoardPickNPlaceData.ElementAt(wheelposlistindex).Contains(slot))//slot is filled
                    {
                        int slotindexinlist = activeBoardPickNPlaceData.ElementAt(wheelposlistindex).IndexOf(slot);
                        valueandfootprint.SelectedIndex = slotindexinlist - 1;
                        //get name of component in slot
                        int namelistindex = compname.Items.IndexOf(boardCompName);
                        string name = activeBoardPickNPlaceData.ElementAt(namelistindex).ElementAt(slotindexinlist);
                        //get name of component in slot
                        int valuelistindex = compname.Items.IndexOf(boardCompValue);
                        string value = activeBoardPickNPlaceData.ElementAt(valuelistindex).ElementAt(slotindexinlist);
                        //get name of component in slot
                        int footprintlistindex = compname.Items.IndexOf(boardCompFootprint);
                        string footprint = activeBoardPickNPlaceData.ElementAt(footprintlistindex).ElementAt(slotindexinlist);

                        wheel2slotcomponent.Text = name + " , " + value + " , " + footprint;
                        wheel2activeslot.Text = slot.Replace("w2_", "");
                        newpos_wheel2.SelectedValue = wheel2activeslot.Text;


                        //get list of designators that match the selection
                        List<int> matchedDesignators = new List<int>();

                        //filter by component name

                        for (int i = 0; i < activeBoardPickNPlaceData.ElementAt(namelistindex).Count; i++)
                        {
                            if (activeBoardPickNPlaceData.ElementAt(namelistindex).ElementAt(i).Equals(name))
                            {
                                matchedDesignators.Add(i);
                            }
                        }
                        //filter by component value

                        for (int i = 0; i < activeBoardPickNPlaceData.ElementAt(valuelistindex).Count; i++)
                        {
                            if (!activeBoardPickNPlaceData.ElementAt(valuelistindex).ElementAt(i).Equals(value))
                            {
                                matchedDesignators.RemoveAll(x => x == i);
                            }
                        }
                        //filter by component footprint

                        for (int i = 0; i < activeBoardPickNPlaceData.ElementAt(footprintlistindex).Count; i++)
                        {
                            if (!activeBoardPickNPlaceData.ElementAt(footprintlistindex).ElementAt(i).Equals(footprint))
                            {
                                matchedDesignators.RemoveAll(x => x == i);
                            }
                        }
                        //filter by component layer
                        int layerlistindex = compname.Items.IndexOf(boardCompLayer);
                        string layer = activeBoardPickNPlaceData.ElementAt(layerlistindex).ElementAt(slotindexinlist);
                        for (int i = 0; i < activeBoardPickNPlaceData.ElementAt(layerlistindex).Count; i++)
                        {
                            if (!activeBoardPickNPlaceData.ElementAt(layerlistindex).ElementAt(i).Equals(layer))
                            {
                                matchedDesignators.RemoveAll(x => x == i);
                            }
                        }

                        //select appropriate layer image
                        if (activatetoplayer.Tag.ToString() == layer)
                        {
                            activatetoplayer.IsChecked = true;
                        }
                        else if (activatebuttomlayer.Tag.ToString() == layer)
                        {
                            activatebuttomlayer.IsChecked = true;
                        }


                        int xposlistindex = compname.Items.IndexOf(boardCompXcenter);
                        int yposlistindex = compname.Items.IndexOf(boardCompYcenter);

                        foreach (var child in wheel2_activeComponents)
                        {
                            imageviewer.Children.Remove(child.pointer);
                        }
                        wheel2_activeComponents.Clear();
                        foreach (var child in unassigned_activeComponents)
                        {
                            imageviewer.Children.Remove(child.pointer);
                        }
                        unassigned_activeComponents.Clear();
                        foreach (var ln in matchedDesignators)
                        {


                            string posX = activeBoardPickNPlaceData.ElementAt(xposlistindex).ElementAt(ln);
                            string posY = activeBoardPickNPlaceData.ElementAt(yposlistindex).ElementAt(ln);

                            //create pointers for component  

                            activeComponentInfo active2 = new activeComponentInfo();
                            active2.name = name;
                            active2.value = value;
                            active2.footprint = footprint;
                            active2.pos = new Point(double.Parse(posX, System.Globalization.CultureInfo.InvariantCulture), double.Parse(posY, System.Globalization.CultureInfo.InvariantCulture));

                            Ellipse dot = new Ellipse();
                            dot.Uid = "w2dot" + ln.ToString();
                            dot.Height = pointerDotDia;
                            dot.Width = pointerDotDia;
                            dot.Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(wheel2color.Text);
                            PnPWheelViewModel.Instance.wheel2color = wheel2color.Text;
                            dot.StrokeThickness = 2;
                            dot.Stroke = Brushes.Black;
                            dot.HorizontalAlignment = HorizontalAlignment.Left;
                            dot.VerticalAlignment = VerticalAlignment.Top;

                            active2.pointer = dot;

                            wheel2_activeComponents.Add(active2);


                        }
                        drawPointers();
                    }
                    else //slot is empty
                    {

                        foreach (var child in wheel2_activeComponents)
                        {
                            imageviewer.Children.Remove(child.pointer);
                        }
                        wheel2_activeComponents.Clear();
                        wheel2slotcomponent.Text = "Slot Empty";
                        wheel2activeslot.Text = slot.Replace("w2_", "");
                        newpos_wheel2.SelectedValue = wheel2activeslot.Text;

                    }

                }
            }

        }
        private void connectbutton_Click(object sender, RoutedEventArgs e)
        {
            if (PnPWheelViewModel.Instance.constate != 2)
            {
                //close port
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        try
                        {
                            int tryturn = 0;
                            while (_serialPort.IsOpen && tryturn < con_try)
                            {
                                _serialPort.Close();
                                tryturn++;
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                    if (!_serialPort.IsOpen)
                    {
                        PnPWheelViewModel.Instance.constate = 2;
                    }
                }
            }
            else
            {
                List<string> tomatch = new List<string>();
                tomatch.Add("ALPnPW");
                Action action = getSerialDevicesCallback;

                if (GetSerialDevices._getSerialDevices.getDevices(tomatch, 6, "p*", action))
                {
                    // MessageBox.Show("released");
                }
            }


        }

        public void getSerialDevicesCallback()
        {
            if (GetSerialDevices.serialdevicelist.Count != 0)
            {
                if (GetSerialDevices.serialdevicelist.Count == 1)
                {
                    var splitcominfo = GetSerialDevices.serialdevicelist.ElementAt(0).Split(';');
                    ComboBoxItem boxItem = new ComboBoxItem();
                    boxItem.Content = GetSerialDevices.serialdevicelist.ElementAt(0);
                    connectedDevices.Items.Add(boxItem);
                    connectedDevices.SelectedIndex = 0;
                    connectToSerialDevice(splitcominfo[0]);

                }
                else
                {
                    foreach (var item in GetSerialDevices.serialdevicelist)
                    {
                        ComboBoxItem boxItem = new ComboBoxItem();
                        boxItem.Content = item;
                        connectedDevices.Items.Add(boxItem);
                    }
                }

            }
            else
            {
                connectedDevices.Items.Clear();
            }


        }

        public void setDTR()
        {
            _serialPort.DtrEnable = true;
            _serialPort.RtsEnable = true;
        }
        private void resetwheel1_Click(object sender, RoutedEventArgs e)
        {
            var answer = MessageBox.Show("Rotate WHEEL 1 until the open slot aligns with the tiny arrow \n Click on OK once the wheel is in position", "PnPWheel", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (answer == MessageBoxResult.OK)
            {
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        try
                        {
                            _serialPort.Write("rsw1*");
                            setDTR();
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }
                }

            }

        }

        private void resetwheel2_Click(object sender, RoutedEventArgs e)
        {
            var answer = MessageBox.Show("Rotate WHEEL 2 until the open slot aligns with the tiny arrow \n Click on OK once the wheel is in position", "PnPWheel", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (answer == MessageBoxResult.OK)
            {
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        try
                        {
                            _serialPort.Write("rsw2*");
                            setDTR();
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                    }
                }

            }
        }

        private void Blinktimer()
        {
            // Create a timer for com port query
            blinkTimer = new System.Timers.Timer(200);
            blinkTimer.AutoReset = true;
            blinkTimer.Enabled = false;
            // Hook up the Elapsed event for the timer. 
            blinkTimer.Elapsed += blinkQTimerEvent;

        }

        private void blinkQTimerEvent(Object source, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (wheel1needsreset)
                {
                    if (blinkerbool)
                    {
                        wheel1panel.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(wheel1color.Text);

                    }
                    else
                    {
                        wheel1panel.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("White");
                       
                    }
                }
                if (wheel2needsreset)
                {
                    if (blinkerbool)
                    {
                        wheel2panel.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(wheel2color.Text);
                    }
                    else
                    {
                        wheel2panel.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("White");

                    }
                }
                blinkerbool = !blinkerbool;
            });
            if (!wheel1needsreset && !wheel2needsreset)
            {
                blinkTimer.Enabled = false;
            }
           
        }
        private void wheel1cnt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updatewheelslotcount(1);

        }

        private void wheel2cnt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            updatewheelslotcount(2);
        }
        private void updatewheelslotcount(int wheelno)
        {
            if (wheelno == 1)
            {
                if (wheel1cnt.SelectedValue != null)
                {
                    int scnt = Int32.Parse(wheel1cnt.SelectedValue.ToString());
                    newpos_wheel1.Items.Clear();
                    for (int i = 1; i <= scnt; i++)
                    {
                        if (i < 10)
                        {
                            newpos_wheel1.Items.Add("00" + i.ToString());
                        }
                        else if (i < 100 && i >= 10)
                        {
                            newpos_wheel1.Items.Add("0" + i.ToString());
                        }
                    }
                    //update pnp wheel too
                    if (_serialPort != null)
                    {
                        if (_serialPort.IsOpen)
                        {

                            try
                            {
                                _serialPort.Write("w1=" + scnt.ToString() + "xx*");
                                setDTR();

                            }
                            catch (Exception)
                            {

                                throw;
                            }
                        }
                    }
                }
            }
            else if (wheelno == 2)
            {
                if (wheel2cnt.SelectedValue != null)
                {
                    int scnt = Int32.Parse(wheel2cnt.SelectedValue.ToString());
                    newpos_wheel2.Items.Clear();
                    for (int i = 1; i <= scnt; i++)
                    {
                        if (i < 10)
                        {
                            newpos_wheel2.Items.Add("00" + i.ToString());
                        }
                        else if (i < 100 && i >= 10)
                        {
                            newpos_wheel2.Items.Add("0" + i.ToString());
                        }
                    }
                    //update pnp wheel too
                    if (_serialPort != null)
                    {
                        if (_serialPort.IsOpen)
                        {

                            try
                            {
                                _serialPort.Write("w2=" + scnt.ToString() + "xx*");
                                setDTR();
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                        }
                    }
                }
            }


        }

        private void importCompPositionFromExisting_Click(object sender, RoutedEventArgs e)
        {
            if (activeBoardPickNPlaceData != null)// no project loaded yet
            {
                List<string> knownslots = new List<string>();
                //load board files
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.InitialDirectory = pnpProjectPath;
                // Display OpenFileDialog by calling ShowDialog method 
                Nullable<bool> result = dlg.ShowDialog();
                // Get the selected file name and display in a TextBox 
                if (result == true)
                {
                    string choosenpnpProjectPath = System.IO.Path.GetDirectoryName(dlg.FileName);

                    //get csv
                    string[] csvfiles = Directory.GetFiles(choosenpnpProjectPath, "*.csv");
                    if (csvfiles.Length == 1)
                    {
                        string pnpwcsvpath = csvfiles[0];
                        /*****************************************************************/
                        //locate and store column names

                        //if choosen csv has pnpwheel position column 
                        var csvdata = new StreamReader(pnpwcsvpath);
                        bool PnPWheelColumnCreated = false;
                        while (!csvdata.EndOfStream)
                        {
                            string line = csvdata.ReadLine();
                            if (line.Contains("PnPWheel Position"))
                            {
                                PnPWheelColumnCreated = true;
                            }

                        }
                        csvdata.Close();

                        if (PnPWheelColumnCreated)
                        {
                            
                            /// string[] csvread = System.IO.File.ReadAllLines(csvpath);
                            // var file = new System.IO.StreamWriter(csvpath);


                            //filter by component info
                            int namelistindex = compname.Items.IndexOf(boardCompName);
                            int valuelistindex = compname.Items.IndexOf(boardCompValue);
                            int footprintlistindex = compname.Items.IndexOf(boardCompFootprint);
                            int wheelposlistindex = compname.Items.IndexOf(boardwheelpos);
                            int rowcount = activeBoardPickNPlaceData.ElementAt(namelistindex).Count;
                            List<int> linestoremove = new List<int>();
                            for (int i = 1; i < rowcount; i++) //start from the first component
                            {
                                string[] csvreadchoosen = System.IO.File.ReadAllLines(pnpwcsvpath);
                                linestoremove.Clear();
                                string nametomatch = activeBoardPickNPlaceData.ElementAt(namelistindex).ElementAt(i);  //component name
                                for (int j = 0; j < csvreadchoosen.Count(); j++)
                                {
                                    if (!csvreadchoosen[j].Contains(nametomatch))
                                    {
                                        if (!linestoremove.Contains(j))
                                        {
                                            linestoremove.Add(j);
                                        }
                                       
                                    }
                                }
                                string valuetomatch = activeBoardPickNPlaceData.ElementAt(valuelistindex).ElementAt(i); //component value
                                for (int j = 0; j < csvreadchoosen.Count(); j++)
                                {
                                    if (!csvreadchoosen[j].Contains(valuetomatch))
                                    {
                                        if (!linestoremove.Contains(j))
                                        {
                                            linestoremove.Add(j);
                                        }
                                    }
                                }
                                string footprinttomatch = activeBoardPickNPlaceData.ElementAt(footprintlistindex).ElementAt(i); //component footprint
                                for (int j = 0; j < csvreadchoosen.Count(); j++)
                                {
                                    if (!csvreadchoosen[j].Contains(footprinttomatch))
                                    {
                                        if (!linestoremove.Contains(j))
                                        {
                                            linestoremove.Add(j);
                                        }
                                    }
                                }
                                //remove unqualified lines
                                foreach (var ln in linestoremove)
                                {
                                    csvreadchoosen[ln] = "";
                                    
                                }
                                csvreadchoosen = csvreadchoosen.Where(val => val != "").ToArray();
                                if (csvreadchoosen.Count() != 0) //csvreadchoosen is not empty
                                {
                                    //check if wheel position is the same for all qualified lines
                                    int ggg = csvreadchoosen[0].LastIndexOf(",") + 1;
                                    string wheelpositionstring = csvreadchoosen[0].Substring(ggg);
                                    bool wheelpositionstringIsIvnalid = false;
                                    for (int j = 0; j < csvreadchoosen.Count(); j++)
                                    {
                                        if (!csvreadchoosen[j].Contains(wheelpositionstring))
                                        {
                                            wheelpositionstringIsIvnalid = true;
                                        }
                                    }
                                    if (!wheelpositionstringIsIvnalid) //is valid
                                    {
                                        string[] csvread = System.IO.File.ReadAllLines(csvpath);
                                        var file = new System.IO.StreamWriter(csvpath);
                                        for (int j = 0; j < csvread.Count(); j++)
                                        {
                                            if (csvread[j].Contains(nametomatch) && csvread[j].Contains(valuetomatch) && csvread[j].Contains(footprinttomatch))
                                            {
                                                if (!knownslots.Contains(wheelpositionstring))
                                                {
                                                    knownslots.Add(wheelpositionstring);
                                                }
                                                int fff = csvread[j].LastIndexOf(",") + 1;
                                                string kkk = csvread[j].Substring(fff);
                                                csvread[j] = csvread[j].Replace(kkk, wheelpositionstring);
                                            }
                                            file.WriteLine(csvread[j]);
                                        }
                                        file.Close();
                                        
                                    }
                                }

                            }
                        }


                    }
                }
                readCSV();
                get_savedConfig();
                populateUpdateCombobox();
                MessageBox.Show(knownslots.Count + "  Known PnPWheel Positions Successfully Imported");
            }
            else
            {
                MessageBox.Show("No Project Loaded");
            }
        }

      
    }
}
