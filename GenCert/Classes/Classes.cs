using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CertAdmin.Classes
{
    public class App_Settings
    {
        internal string WSDLServer1 { get; set; }
        internal string CertPassword { get; set; }
        internal string MySQLServer { get; set; }
        internal string MySQLPort { get; set; }
        internal string MySQLLoginName { get; set; }
        internal string MySQLLoginPassword { get; set; }
        internal string MySQLDbName { get; set; }
        internal string WriteToLog { get; set; }
        internal string CertificateSavingPath { get; set; }
        internal string openSslVersion { get; set; }
        
    }

  
}



