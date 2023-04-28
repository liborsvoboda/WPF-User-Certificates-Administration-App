using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Controls.Primitives;
using System.Windows.Automation;
using CertAdmin.Forms;
using System.IO;
using Dragablz;
using CertAdmin.functions;
using System.ComponentModel;
using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Cryptography.X509Certificates;


namespace CertAdmin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        //novo
        private readonly MainWindowViewModel _viewModel;

        private bool _shutdown;
        internal static bool isAdmin = false;
        internal static bool isTest = false;

        internal static string appVersion = "1.5.2.0";
        #region version description
        #endregion

        private static bool _hackyIsFirstWindow = true;
        public static UserControl loadedUserForm = null;

        //LOG4NET - Here is the once-per-class call to initialize the log object
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Help Added
        private DependencyObject CurrentHelpDO { get; set; }
        private Popup CurrentHelpPopup { get; set; }
        private bool HelpActive { get; set; }
        private MouseEventHandler _helpHandler = null;
        static bool isHelpMode = false;
        #endregion

        MainWindowViewModel mwm;

        internal string iniFilePath = System.Environment.CurrentDirectory + "\\config.ini";
        internal string certFriendlyName = "Cert Friendly Name";

        internal static string setting_folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), System.Reflection.Assembly.GetEntryAssembly().GetName().FullName.Split(',')[0]);
        internal static string logFile = "Application.log";
        internal static string settingFile = "config.json";
        internal static Classes.App_Settings Setting = new Classes.App_Settings();
        internal static string dbConnectioString = null;

        public static bool dataGridSelected = false;
        public static Int64 dataGridSelectedId = 0;
        public static event EventHandler DataGridSelectedChanged = delegate { };
        public static bool tmSelected = false;
        public static event EventHandler TMSelectedChanged = delegate { };
        public static bool pfxSelected = false;
        public static event EventHandler PFXSelectedChanged = delegate { };
        public static bool privKeySelected = false;
        public static event EventHandler PrivKeySelectedChanged = delegate { };
        public static bool DataGridSelected
        {
            get => dataGridSelected;
            set
            {
                dataGridSelected = value;
                DataGridSelectedChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public static bool PrivKeySelected
        {
            get => privKeySelected;
            set
            {
                privKeySelected = value;
                PrivKeySelectedChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public static bool TMSelected
        {
            get => tmSelected;
            set
            {
                tmSelected = value;
                TMSelectedChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool PFXSelected
        {
            get => pfxSelected;
            set
            {
                pfxSelected = value;
                PFXSelectedChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            #region ini
            IniFile ini = new IniFile(iniFilePath);
            if (!File.Exists(iniFilePath))
            {
                ini.IniWriteValue("Configuration", "FriendlyName", "Certificate Friendly Name");
            }
            certFriendlyName = ini.IniReadValue("Configuration", "FriendlyName");
            #endregion

            if (_hackyIsFirstWindow)
            {
                // tooltip to stay on the screen
                ToolTipService.ShowDurationProperty.OverrideMetadata(
                    typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));

                //Programmatically change Taskbar icon in WPF
                Application.Current.MainWindow.Icon = IconMaker.Icon(System.Windows.Media.Colors.White);

                #region log4net
                //Log4Net messages
                log.Debug("CertAdmin started ...");
                log.Info("CertAdmin started ...");
                log.Warn("CertAdmin started ...");
                log.Error("CertAdmin started ...");
                log.Fatal("CertAdmin started ...");
                #endregion

                mainWindow.Height = Properties.Settings.Default.Height;
                mainWindow.Width = Properties.Settings.Default.Width;
                mainWindow.Left = Properties.Settings.Default.Left;
                mainWindow.Top = Properties.Settings.Default.Top;

                mwm = MainWindowViewModel.CreateWithSamples();
                DataContext = mwm;
            }

            _hackyIsFirstWindow = false;
        }


        private async void MetroWindowClosing(object sender, CancelEventArgs e)
        {
            MainWindow mw = (MainWindow)sender;
            // Close button only close app on main form
            if (mw.DataContext == null)
            {
                return;
            }

            if (e.Cancel) return;
            e.Cancel = !_shutdown; // && _viewModel.QuitConfirmationEnabled;
            if (_shutdown) return;

            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Ukončit",
                NegativeButtonText = "Zrušit",
                AnimateShow = true,
                AnimateHide = false
            };

            MessageDialogResult result = await this.ShowMessageAsync("Ukončit Aplikaci?",
                "Opravdu chcete ukončit aplikaci?",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);


            _shutdown = result == MessageDialogResult.Affirmative;

            if (_shutdown)
            {
                MainWindowViewModel.SaveTheme();
                Application.Current.Shutdown();
            }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isHelpMode)
            {
                e.Handled = true;
                isHelpMode = false;
                Mouse.OverrideCursor = null;

                if (Help.MyHelpCommand.CanExecute(null, this))
                {
                    Help.MyHelpCommand.Execute(null, this);
                }
            }
        }

        private void winMain_MouseMove(object sender, MouseEventArgs e)
        {
            // You can check the HelpActive property if desired, however 
            // the listener should not be hooked up so this should not be firing
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(((Visual)sender), e.GetPosition(this));

            if (hitTestResult.VisualHit != null && CurrentHelpDO != hitTestResult.VisualHit)
            {
                // Walk up the tree in case a parent element has help defined
                DependencyObject checkHelpDO = hitTestResult.VisualHit;
                string helpText = AutomationProperties.GetHelpText(checkHelpDO);

                while (String.IsNullOrEmpty(helpText) && checkHelpDO != null && checkHelpDO != mainWindow)
                {
                    checkHelpDO = VisualTreeHelper.GetParent(checkHelpDO);
                    helpText = AutomationProperties.GetHelpText(checkHelpDO);
                }

                if (String.IsNullOrEmpty(helpText) && CurrentHelpPopup != null)
                {
                    CurrentHelpPopup.IsOpen = false;
                    CurrentHelpDO = null;
                }
                else if (!String.IsNullOrEmpty(helpText) && CurrentHelpDO != checkHelpDO)
                {
                    CurrentHelpDO = checkHelpDO;
                    // New visual "stack" hit, close old popup, if any
                    if (CurrentHelpPopup != null)
                    {
                        CurrentHelpPopup.IsOpen = false;
                    }

                    // Obviously you can make the popup look anyway you want it to look 
                    // with any number of options. I chose a simple tooltip look-and-feel.
                    CurrentHelpPopup = new Popup()
                    {
                        //AllowsTransparency = true,
                        PopupAnimation = PopupAnimation.Scroll,
                        PlacementTarget = (UIElement)hitTestResult.VisualHit,
                        IsOpen = true,
                        Child = new Border()
                        {
                            CornerRadius = new CornerRadius(10),
                            BorderBrush = new SolidColorBrush(Colors.Goldenrod),
                            BorderThickness = new Thickness(2),
                            Background = new SolidColorBrush(Colors.LightYellow),
                            Child = new TextBlock()
                            {
                                Margin = new Thickness(10),
                                Text = helpText.Replace("\\r\\n", "\r\n"),
                                FontSize = 14,
                                FontWeight = FontWeights.Normal
                            }
                        }
                    };
                    CurrentHelpPopup.IsOpen = true;
                }
            }
        }

        private void btnHelp_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isHelpMode)
            {
                isHelpMode = false;
                Mouse.OverrideCursor = null;
            }
        }

        private async void About(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync("Řidiči Správce certifikátů ",
                "Generátor CSR,KEY,...\n\n" +
                "Verze :" + appVersion + "\n" +
                "Autor : Libor Svoboda",
                MessageDialogStyle.Affirmative);
        }

        private void LaunchHelp(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Help.ShowHelp(null, "GenCert.chm");
        }

        private void winMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1 && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                ToggleHelp();
            }
            else if (Keyboard.IsKeyDown(Key.Q) && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                ExitMenuItem_Click(null, null);

                isHelpMode = false;
                Mouse.OverrideCursor = null;
                e.Handled = true;
            }
            else if (Keyboard.IsKeyDown(Key.R) && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                GenerateCertRequest_Click(null, null);

                isHelpMode = false;
                Mouse.OverrideCursor = null;
                e.Handled = true;
            }
            else if (Keyboard.IsKeyDown(Key.C) && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                GenerateCreateSignCert_Click(null, null);

                isHelpMode = false;
                Mouse.OverrideCursor = null;
                e.Handled = true;
            }
        }

        private void GenerateCertRequest_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.DataContext = new CreateRequest();
        }

        private void GenerateCreateSignCert_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.DataContext = new CreateRequest();
        }

        private void ToggleHelp()
        {
            // Turn the current help off
            CurrentHelpDO = null;
            if (CurrentHelpPopup != null)
            {
                CurrentHelpPopup.IsOpen = false;
            }

            // Toggle current state; add/remove mouse handler
            HelpActive = !HelpActive;

            if (_helpHandler == null)
            {
                _helpHandler = new MouseEventHandler(winMain_MouseMove);
            }

            if (HelpActive)
            {
                mainWindow.MouseMove += _helpHandler;
            }
            else
            {
                mainWindow.MouseMove -= _helpHandler;
            }

            // Start recursive toggle at visual root
            ToggleHelp(mainWindow);
        }

        private void ToggleHelp(DependencyObject dependObj)
        {
            // Continue recursive toggle. Using the VisualTreeHelper works nicely.
            for (int x = 0; x < VisualTreeHelper.GetChildrenCount(dependObj); x++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependObj, x);
                ToggleHelp(child);
            }

            // BitmapEffect is defined on UIElement so our DependencyObject 
            // must be a UIElement also
            if (dependObj is UIElement)
            {
                UIElement element = (UIElement)dependObj;

                if (HelpActive)
                {
                    string helpText = AutomationProperties.GetHelpText(element);

                    if (!String.IsNullOrEmpty(helpText))
                    {
                    }
                }
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AppQuit();
        }

        public async void AppQuit()
        {
            var settings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No",
            };
            MetroWindow mw = (MetroWindow)App.Current.MainWindow;
            var result = await mw.ShowMessageAsync("Quit application?", "Sure you want to quit application?",
                MessageDialogStyle.AffirmativeAndNegative, settings);

            if (result == MessageDialogResult.Affirmative)
            {
                MainWindowViewModel.SaveTheme();
                Application.Current.Shutdown();
            }
        }

        public void GetTabablzData(out string header0, out IEnumerable<TabablzControl> tctrl)
        {
            MetroWindow wnd = (MetroWindow)App.Current.MainWindow;
            TabablzControl tc = (TabablzControl)wnd.FindName("InitialTabablzControl");

            TabContent itc0 = (TabContent)tc.SelectedItem;
            header0 = itc0.Header;
            tctrl = TabablzControl.GetLoadedInstances();
        }

        public void AddTabablzData(string header0, IEnumerable<TabablzControl> tctrl, TabContent tc1)
        {
            TabablzControl lastTabablzControl = tctrl.Last();

            // adds a new tab after the last right tab
            IEnumerable<DragablzItem> orderedDragablzItem = lastTabablzControl.GetOrderedHeaders();
            DragablzItem lastTab = orderedDragablzItem.Last();
            TabablzControl.AddItem(tc1, lastTab.DataContext, AddLocationHint.After);

            TabablzControl.SelectItem(tc1);
        }

        internal void OpenDriverList_Click(object sender, RoutedEventArgs e)
        {
            string header0 = "Přehled Řidičů";
            IEnumerable<TabablzControl> tctrl;
            GetTabablzData(out header0, out tctrl);
            header0 = "Přehled Řidičů";

            CreateRequest gr = new CreateRequest();
            if (loadedUserForm == null || loadedUserForm != gr)
            {
                loadedUserForm = gr;
            }

            TabContent tc1 = new TabContent(header0, loadedUserForm);
            AddTabablzData(header0, tctrl, tc1);

            sbiSelectedMenuOption.Content = header0;
        }

        public void OpenCreateReguestForm_Click(object sender, RoutedEventArgs e)
        {
            string header0 = "Create Request";
            IEnumerable<TabablzControl> tctrl;
            GetTabablzData(out header0, out tctrl);
            header0 = "Create Request";

            CreateRequest gr = new CreateRequest();
            if (loadedUserForm == null || loadedUserForm != gr)
            {
                loadedUserForm = gr;
            }

            TabContent tc1 = new TabContent(header0, loadedUserForm);
            AddTabablzData(header0, tctrl, tc1);

            sbiSelectedMenuOption.Content = header0;
        }

        internal void OpenSignCertForm_Click(object sender, RoutedEventArgs e)
        {
            string header0 = "Create Certificate";
            IEnumerable<TabablzControl> tctrl;
            GetTabablzData(out header0, out tctrl);
            header0 = "Create Certificate";

            CreatePFX gr = new CreatePFX();
            if (loadedUserForm == null || loadedUserForm != gr)
            {
                loadedUserForm = gr;
            }

            TabContent tc1 = new TabContent(header0, loadedUserForm);
            AddTabablzData(header0, tctrl, tc1);

            sbiSelectedMenuOption.Content = header0;
        }

        internal void OpenSelfSignCertForm_Click(object sender, RoutedEventArgs e)
        {
            string header0 = "Create SelfSign Cert.";
            IEnumerable<TabablzControl> tctrl;
            GetTabablzData(out header0, out tctrl);
            header0 = "Create SelfSign Cert.";

            CreateSelfSign gr = new CreateSelfSign();
            if (loadedUserForm == null || loadedUserForm != gr)
            {
                loadedUserForm = gr;
            }

            TabContent tc1 = new TabContent(header0, loadedUserForm);
            AddTabablzData(header0, tctrl, tc1);

            sbiSelectedMenuOption.Content = header0;
        }

        internal void OpenSignRequestForm_Click(object sender, RoutedEventArgs e)
        {
            string header0 = "Issue Certificate";
            IEnumerable<TabablzControl> tctrl;
            GetTabablzData(out header0, out tctrl);
            header0 = "Issue Certificate";

            IssueCert gr = new IssueCert();
            if (loadedUserForm == null || loadedUserForm != gr)
            {
                loadedUserForm = gr;
            }

            TabContent tc1 = new TabContent(header0, loadedUserForm);
            AddTabablzData(header0, tctrl, tc1);

            sbiSelectedMenuOption.Content = header0;
        }

        private void CACertificatesForm_Click(object sender, RoutedEventArgs e)
        {
            string header0 = "CA Certificate";
            IEnumerable<TabablzControl> tctrl;
            GetTabablzData(out header0, out tctrl);
            header0 = "CA Certificate";

            CreateCA gr = new CreateCA();
            if (loadedUserForm == null || loadedUserForm != gr)
            {
                loadedUserForm = gr;
            }

            TabContent tc1 = new TabContent(header0, loadedUserForm);
            AddTabablzData(header0, tctrl, tc1);

            sbiSelectedMenuOption.Content = header0;
        }

        private void RefreshDriverList_Click(object sender, RoutedEventArgs e)
        {
            var result = new MainWindowViewModel();
            TabContent tc = new TabContent("Seznam Řidičů", new IntroductionPage());
            result.TabContents.Add(tc);
            mwm = result;
            DataContext = mwm;
        }

        private void ImportTM_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".cer";
            dlg.Filter = "CER (*.cer)|*.cer|PEM (*.pem)|*.pem|CRT (*.crt)|*.crt|All Files|*";
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                MySqlConnection conStrMysql = new MySqlConnection();
                try
                {

                    System.Security.Cryptography.X509Certificates.X509Certificate cert = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromCertFile(filename);
                    string expiry = cert.GetExpirationDateString();

                    //  ukladani TMKey do databaze
                    conStrMysql.ConnectionString = MainWindow.dbConnectioString;
                    List<MySqlParameter> arrParameters = new List<MySqlParameter>();
                    var certData = File.ReadAllBytes(filename);
                    arrParameters.Add(new MySqlParameter("@TMKeyData", certData));
                    arrParameters.Add(new MySqlParameter("@TMKeyExpiry", DateTime.Parse(expiry)));
                    conStrMysql.Open();
                    IDataReader Reader = MySqlHelper.ExecuteReader(conStrMysql, String.Format("START TRANSACTION;UPDATE ciselnik_ridici cr SET cr.CERTIFIKAT_TM = @TMKeyData,cr.CERTIFIKAT_PLATNOST =@TMKeyExpiry   WHERE cr.ID = " + dataGridSelectedId.ToString() + ";COMMIT;"), arrParameters.ToArray());
                    conStrMysql.Close();
                    // konec ukladani 

                    Functions.resetSelection();
                    var resultWindow = new MainWindowViewModel();
                    TabContent tc = new TabContent("Seznam Řidičů", new IntroductionPage());
                    resultWindow.TabContents.Add(tc);
                    mwm = resultWindow;
                    DataContext = mwm;
                }
                catch (Exception ex)
                {
                    conStrMysql.Close();
                    Functions.fn_WriteToFile(System.IO.Path.Combine(setting_folder, logFile), "ImportTM error: " + ex.Message);
                }


            }
        }

        private void ExportTM_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".cer";
            dlg.Filter = "CER (*.cer)|*.cer|PEM (*.pem)|*.pem|CRT (*.crt)|*.crt|All Files|*";
            dlg.FileName = new string('0', 8 - dataGridSelectedId.ToString().Length) + dataGridSelectedId.ToString();

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                MySqlConnection conStrMysql = new MySqlConnection();
                try
                {
                    //  ukladani TMKey do souboru
                    conStrMysql.ConnectionString = MainWindow.dbConnectioString;
                    conStrMysql.Open();
                    IDataReader Reader = MySqlHelper.ExecuteReader(conStrMysql, String.Format("START TRANSACTION;SELECT cr.CERTIFIKAT_TM FROM ciselnik_ridici cr WHERE cr.ID = " + dataGridSelectedId.ToString() + ";COMMIT;"));
                    Reader.Read();
                    byte[] certData = (byte[])Reader.GetValue(0);
                    File.WriteAllBytes(filename, certData);
                    conStrMysql.Close();
                    // konec ukladani 
                }
                catch (Exception ex)
                {
                    conStrMysql.Close();
                    Functions.fn_WriteToFile(System.IO.Path.Combine(setting_folder, logFile), "ExportTM error: " + ex.Message);
                }
            }
        }

        private void ExportPK_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".csr";
            dlg.Filter = "CSR (*.csr)|*.csr";
            dlg.FileName = new string('0', 8 - dataGridSelectedId.ToString().Length) + dataGridSelectedId.ToString();

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                MySqlConnection conStrMysql = new MySqlConnection();
                try
                {
                    //  ukladani PK + CSR do souboru
                    conStrMysql.ConnectionString = MainWindow.dbConnectioString;
                    conStrMysql.Open();
                    IDataReader Reader = MySqlHelper.ExecuteReader(conStrMysql, String.Format("START TRANSACTION;SELECT cr.CERTIFIKAT_CRS,cr.CERTIFIKAT_KLIC FROM ciselnik_ridici cr WHERE cr.ID = " + dataGridSelectedId.ToString() + ";COMMIT;"));
                    Reader.Read();
                    byte[] csrData = (byte[])Reader.GetValue(0);
                    byte[] keyData = (byte[])Reader.GetValue(1);
                    File.WriteAllBytes(dlg.FileName, csrData);
                    File.WriteAllBytes(System.IO.Path.ChangeExtension(dlg.FileName, "key"), keyData);
                    conStrMysql.Close();
                    // konec ukladani 
                }
                catch (Exception ex)
                {
                    conStrMysql.Close();
                    Functions.fn_WriteToFile(System.IO.Path.Combine(setting_folder, logFile), "ExportPK error: " + ex.Message);
                }
            }
        }

        private void GenerateCSR_Click(object sender, RoutedEventArgs e)
        {
            if (TMSelected || PrivKeySelected) {
                MessageBoxResult resultQuestion = MessageBox.Show("Privátní klíč nebo TM certifikát již existuje, opravdu chcete vygenerovat novou žádost?\nStávající certifikáty budou odebrány.", "Existující TM Certifikát", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (resultQuestion == MessageBoxResult.Yes)
                {
                    GenerateCSR();
                }
            } else {
                GenerateCSR();
            }
        }


        private void GenerateCSR()
        {
            string header0 = "Vygenerovat novou žádost";
            IEnumerable<TabablzControl> tctrl;
            tctrl = TabablzControl.GetLoadedInstances();
            TabablzControl lastTabablzControl = tctrl.Last();
            loadedUserForm = new CreateRequest();

            IEnumerable<DragablzItem> orderedDragablzItem = lastTabablzControl.GetOrderedHeaders();
            DragablzItem lastTab = orderedDragablzItem.Last();
            TabContent tc1 = new TabContent(header0, loadedUserForm);

            ((CreateRequest)tc1.Content).tbOrganization.Text = "T-Mobile Czech Republic a.s.";
            ((CreateRequest)tc1.Content).tbCountryCode.Text = "CZ";
            ((CreateRequest)tc1.Content).tbLocalityName.Text = "Prague";
            ((CreateRequest)tc1.Content).tbStateOrProvince.Text = "Czech Republic";
            ((CreateRequest)tc1.Content).tbEmail.Text = "epodatelna@t-mobile.cz";
            ((CreateRequest)tc1.Content).tbPathName.Text = Setting.CertificateSavingPath;
            ((CreateRequest)tc1.Content).tbRequestName.Text = "ESign-" + new string('0', 8 - dataGridSelectedId.ToString().Length) + dataGridSelectedId.ToString();
            ((CreateRequest)tc1.Content).tbPrivateKeyName.Text = "ESignPrivateKey-" + new string('0', 8 - dataGridSelectedId.ToString().Length) + dataGridSelectedId.ToString();
            ((CreateRequest)tc1.Content).tbDomainName.Text = "ESign-" + new string('0', 8 - dataGridSelectedId.ToString().Length) + dataGridSelectedId.ToString();

            TabablzControl.AddItem(tc1, lastTab.DataContext, AddLocationHint.After);
            TabablzControl.SelectItem(tc1);
        }

        private void ExportPFX_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".pfx";
            dlg.Filter = "PFX (*.pfx)|*.pfx|P12 (*.p12)|*.p12|All Files|*";
            dlg.FileName = new string('0', 8 - dataGridSelectedId.ToString().Length) + dataGridSelectedId.ToString();

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                MySqlConnection conStrMysql = new MySqlConnection();
                try
                {
                    //  ukladani PFXKey do souboru
                    conStrMysql.ConnectionString = MainWindow.dbConnectioString;
                    conStrMysql.Open();
                    IDataReader Reader = MySqlHelper.ExecuteReader(conStrMysql, String.Format("START TRANSACTION;SELECT cr.CERTIFIKAT FROM ciselnik_ridici cr WHERE cr.ID = " + dataGridSelectedId.ToString() + ";COMMIT;"));
                    Reader.Read();
                    byte[] certData = (byte[])Reader.GetValue(0);
                    File.WriteAllBytes(filename, certData);
                    conStrMysql.Close();
                    // konec ukladani 
                }
                catch (Exception ex)
                {
                    conStrMysql.Close();
                    Functions.fn_WriteToFile(System.IO.Path.Combine(setting_folder, logFile), "ExportPFX error: " + ex.Message);
                }
            }
        }

        private void GeneratePFX_Click(object sender, RoutedEventArgs e)
        {
            string fileName = System.IO.Path.Combine(setting_folder, "Data", "Temp", new string('0', 8 - dataGridSelectedId.ToString().Length) + dataGridSelectedId.ToString() + ".cer");

            MySqlConnection conStrMysql = new MySqlConnection();
            try
            {
                //  ukladani PK + CER do souboru
                conStrMysql.ConnectionString = MainWindow.dbConnectioString;
                conStrMysql.Open();
                IDataReader Reader = MySqlHelper.ExecuteReader(conStrMysql, String.Format("START TRANSACTION;SELECT cr.CERTIFIKAT_TM,cr.CERTIFIKAT_KLIC FROM ciselnik_ridici cr WHERE cr.ID = " + dataGridSelectedId.ToString() + ";COMMIT;"));
                Reader.Read();
                byte[] cerData = (byte[])Reader.GetValue(0);
                byte[] keyData = (byte[])Reader.GetValue(1);
                File.WriteAllBytes(fileName, cerData);
                File.WriteAllBytes(System.IO.Path.ChangeExtension(fileName, "key"), keyData);
                conStrMysql.Close();
                // konec ukladani 

                if (Functions.fn_generatePfx(new string('0', 8 - dataGridSelectedId.ToString().Length) + dataGridSelectedId.ToString())) {
                    //  ukladani PFXCert do databaze
                    try
                    {
                        conStrMysql.ConnectionString = MainWindow.dbConnectioString;
                        List<MySqlParameter> arrParameters = new List<MySqlParameter>();
                        var pfxData = File.ReadAllBytes(System.IO.Path.ChangeExtension(fileName, "pfx"));
                        arrParameters.Add(new MySqlParameter("@PfxData", pfxData));
                        conStrMysql.Open();
                        Reader = MySqlHelper.ExecuteReader(conStrMysql, String.Format("START TRANSACTION;UPDATE ciselnik_ridici cr SET cr.CERTIFIKAT = @PfxData  WHERE cr.ID = " + dataGridSelectedId.ToString() + ";COMMIT;"), arrParameters.ToArray());
                        conStrMysql.Close();
                        // konec ukladani 

                        Functions.fn_delete_file(System.IO.Path.ChangeExtension(fileName, "cer"));
                        Functions.fn_delete_file(System.IO.Path.ChangeExtension(fileName, "key"));
                        Functions.fn_delete_file(System.IO.Path.ChangeExtension(fileName, "pfx"));

                        Functions.resetSelection();
                        var resultWindow = new MainWindowViewModel();
                        TabContent tc = new TabContent("Seznam Řidičů", new IntroductionPage());
                        resultWindow.TabContents.Add(tc);
                        mwm = resultWindow;
                        DataContext = mwm;
                    }
                    catch (Exception ex)
                    {
                        conStrMysql.Close();
                        Functions.fn_WriteToFile(System.IO.Path.Combine(setting_folder, logFile), "Save PFX cestificate error: " + ex.Message);

                        Functions.resetSelection();
                        var resultWindow = new MainWindowViewModel();
                        TabContent tc = new TabContent("Seznam Řidičů", new IntroductionPage());
                        resultWindow.TabContents.Add(tc);
                        mwm = resultWindow;
                        DataContext = mwm;
                    }
                } else
                {
                    MessageBox.Show("Generování certifikátu se nezdařilo.\nVygenerujte novou žádost.", "Chyba generování certifikátu", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }

            }
            catch (Exception ex)
            {
                conStrMysql.Close();
                Functions.fn_WriteToFile(System.IO.Path.Combine(setting_folder, logFile), "GeneratePFX error: " + ex.Message);
            }

        }
    }
}
