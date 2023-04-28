//using CommonLibrary;
using Dragablz;
using CertAdmin.Forms;
using CertAdmin.functions;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Data;
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
//using Technewlogic.WpfDialogManagement.Contracts;

namespace CertAdmin
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class IntroductionPage : UserControl
    {

        public IntroductionPage()
        {
            InitializeComponent();
            Functions.fn_LoadSettings();

            MySqlConnection conStrMysql = new MySqlConnection();
            DataSet DTEmpDet = new DataSet();
            conStrMysql.ConnectionString = MainWindow.dbConnectioString;
            

            try
            {
                conStrMysql.Open();
                DTEmpDet = MySqlHelper.ExecuteDataset(conStrMysql, String.Format("SELECT cr.ID,cr.JMENO,cr.PRIJMENI,cr.TELEFON,cr.EMAIL,cr.LOKACE,cr.CERTIFIKAT_PLATNOST,cr.CERTIFIKAT,cr.CERTIFIKAT_KLIC,cr.CERTIFIKAT_CRS,cr.CERTIFIKAT_TM FROM ciselnik_ridici cr WHERE cr.AKTIVNI = 1 AND cr.ID <> 1 ORDER BY ID DESC"));
                driverList.DataContext = DTEmpDet.Tables[0];
                conStrMysql.Close();
            }
            catch (Exception ex)
            {
                conStrMysql.Close();
            }
        }


        public void LoadDbData()
        {
            MySqlConnection conStrMysql = new MySqlConnection();
            DataSet DTEmpDet = new DataSet();
            conStrMysql.ConnectionString = MainWindow.dbConnectioString;
            DataGrid resultData = new DataGrid();
            try
            {
                conStrMysql.Open();
                DTEmpDet = MySqlHelper.ExecuteDataset(conStrMysql, String.Format("SELECT cr.ID,cr.JMENO,cr.PRIJMENI,cr.TELEFON,cr.EMAIL,cr.LOKACE,cr.CERTIFIKAT_PLATNOST,cr.CERTIFIKAT,cr.CERTIFIKAT_KLIC,cr.CERTIFIKAT_CRS,cr.CERTIFIKAT_TM FROM ciselnik_ridici cr WHERE cr.AKTIVNI = 1 AND cr.ID <> 1"));
                driverList.DataContext = DTEmpDet.Tables[0];
                conStrMysql.Close();
             
            }
            catch (Exception ex)
            {
                conStrMysql.Close();
            }
           // return DTEmpDet;
        }

        private void DriverList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string header0 = "Vygenerovat novou žádost";
            IEnumerable<TabablzControl> tctrl;
            tctrl = TabablzControl.GetLoadedInstances();
            TabablzControl lastTabablzControl = tctrl.Last();
            MainWindow.loadedUserForm = new CreateRequest();

            IEnumerable<DragablzItem> orderedDragablzItem = lastTabablzControl.GetOrderedHeaders();
            DragablzItem lastTab = orderedDragablzItem.Last();
            TabContent tc1 = new TabContent(header0, MainWindow.loadedUserForm);

            // FILL DATA ExAMPLE
            ((CertAdmin.Forms.CreateRequest)tc1.Content).tbOrganization.Text = "T-Mobile Czech Republic a.s.";
            ((CertAdmin.Forms.CreateRequest)tc1.Content).tbCountryCode.Text = "CZ";
            ((CertAdmin.Forms.CreateRequest)tc1.Content).tbLocalityName.Text = "Prague";
            ((CertAdmin.Forms.CreateRequest)tc1.Content).tbStateOrProvince.Text = "Czech Republic";
            ((CertAdmin.Forms.CreateRequest)tc1.Content).tbEmail.Text = "epodatelna@t-mobile.cz";
            ((CertAdmin.Forms.CreateRequest)tc1.Content).tbPathName.Text = MainWindow.Setting.CertificateSavingPath;
            ((CertAdmin.Forms.CreateRequest)tc1.Content).tbRequestName.Text = "ESign-"+ new string('0', 8 - ((System.Data.DataRowView)driverList.CurrentItem).Row.ItemArray[0].ToString().Length)+ ((System.Data.DataRowView)driverList.CurrentItem).Row.ItemArray[0].ToString();
            ((CertAdmin.Forms.CreateRequest)tc1.Content).tbPrivateKeyName.Text = "ESignPrivateKey-"+ new string('0', 8 - ((System.Data.DataRowView)driverList.CurrentItem).Row.ItemArray[0].ToString().Length) + ((System.Data.DataRowView)driverList.CurrentItem).Row.ItemArray[0].ToString();
            ((CertAdmin.Forms.CreateRequest)tc1.Content).tbDomainName.Text = "ESign-" + new string('0', 8 - ((System.Data.DataRowView)driverList.CurrentItem).Row.ItemArray[0].ToString().Length) + ((System.Data.DataRowView)driverList.CurrentItem).Row.ItemArray[0].ToString();

            TabablzControl.AddItem(tc1, lastTab.DataContext, AddLocationHint.After);
            TabablzControl.SelectItem(tc1);
        }

        private void DriverList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (driverList.SelectedItems.Count >= 1)
            {
                MainWindow.dataGridSelectedId = Int64.Parse(((System.Data.DataRowView)driverList.CurrentItem).Row.ItemArray[0].ToString());
                MainWindow.DataGridSelected = true;
                if (((System.Data.DataRowView)driverList.CurrentCell.Item).Row.ItemArray[7].GetType().Name == "Byte[]") { MainWindow.PFXSelected = true; } else { MainWindow.PFXSelected = false; }
                if (((System.Data.DataRowView)driverList.CurrentCell.Item).Row.ItemArray[8].GetType().Name == "Byte[]") { MainWindow.PrivKeySelected = true;  } else { MainWindow.PrivKeySelected = false;  }
                if (((System.Data.DataRowView)driverList.CurrentCell.Item).Row.ItemArray[10].GetType().Name == "Byte[]") {MainWindow.TMSelected = true;}else { MainWindow.TMSelected = false; }
            }
            else
            {
                MainWindow.dataGridSelectedId = 0;
                MainWindow.DataGridSelected = false;
                MainWindow.TMSelected = false;
                MainWindow.PrivKeySelected = false;
                MainWindow.PFXSelected = false;
            }
            
        }
    }
}