using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;

namespace CertAdmin
{
    /// <summary>
    /// https://blog.vavstech.com/2014/11/c-install-cert-to-remote-computer-and.html?m=1
    /// https://referencesource.microsoft.com/#system/security/system/security/cryptography/cryptoapi.cs,1c0aa5c2b5408ca6\
    /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/0657f168-af11-42bf-a9e3-301a9aa46a24/certopenstore-and-certenumcertificatesinstore-trying-to-get-certificates-on?forum=clr
    /// https://stackoverflow.com/questions/42124041/c-sharp-x509certificate2-added-on-a-remote-server-x509store-cannot-be-exported
    /// https://stackoverflow.com/questions/12337721/how-to-programmatically-install-a-certificate-using-c-sharp
    /// https://stackoverflow.com/questions/566570/how-can-i-install-a-certificate-into-the-local-machine-store-programmatically-us
    /// ---------------------------------------
    /// https://docs.microsoft.com/en-us/dotnet/framework/tools/certmgr-exe-certificate-manager-tool
    /// https://social.msdn.microsoft.com/Forums/en-US/e64438ed-0fd2-42c5-aecc-d00a8a9df01f/instal-certificate-programmatically?forum=netfxcompact
    /// https://www.codeproject.com/Questions/808781/How-to-install-certificate-using-csharp-to-Server
    /// https://jaredmeredith.com/2015/06/17/programmatically-install-a-root-ca-certificate-so-users-dont-have-to/
    /// https://gist.github.com/BrandonLWhite/235fa12247f6dc827051
    /// https://stackoverflow.com/questions/9951729/x509certificate-constructor-exception
    /// http://paulstovell.com/blog/x509certificate2
    /// https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/certutil#BKMK_installcert
    /// https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/certutil
    /// https://social.technet.microsoft.com/wiki/contents/articles/3063.certutil-examples-for-managing-active-directory-certificate-services-ad-cs-from-the-command-line.aspx
    /// https://docs.microsoft.com/en-us/previous-versions/orphan-topics/ws.10/cc772898(v=ws.10)
    /// https://docs.microsoft.com/en-us/windows/desktop/SecCertEnroll/using-the-included-samples
    /// 
    /// </summary>
    public enum CertStoreName
    {
        MY,
        ROOT,
        TRUST,
        CA
    }
    public class CertStoreReader
    {
        //public void instRemotelly()
        //{
        //    int CERT_STORE_PROV_SYSTEM_A = 9;
        //    int CERT_SYSTEM_STORE_USERS_ID = 6;
        //    int CERT_SYSTEM_STORE_LOCATION_SHIFT = 16;
        //    int CERT_STORE_READONLY_FLAG = 0x00008000;
        //    int CERT_STORE_OPEN_EXISTING_FLAG = 0x00004000;
        //    int CERT_SYSTEM_STORE_USERS = ((int)CERT_SYSTEM_STORE_USERS_ID << (int)CERT_SYSTEM_STORE_LOCATION_SHIFT);

        //    CertOpenStore(CERT_STORE_PROV_SYSTEM_A, 0,
        //    0,
        //    CERT_SYSTEM_STORE_USERS | CERT_STORE_READONLY_FLAG | CERT_STORE_OPEN_EXISTING_FLAG,
        //    "\\\\computerName\\user_SID\\MY");
        //}

        #region P/Invoke Interop

        private static int CERT_STORE_PROV_SYSTEM = 10;
        private static int CERT_SYSTEM_STORE_CURRENT_USER = (1 << 16);
        private static int CERT_SYSTEM_STORE_LOCAL_MACHINE = (2 << 16);

        #region dodato
        private static int CERT_STORE_PROV_SYSTEM_A = 9;
        private static int CERT_SYSTEM_STORE_USERS_ID = 6;
        private static int CERT_SYSTEM_STORE_LOCATION_SHIFT = 16;
        private static int CERT_STORE_READONLY_FLAG = 0x00008000;
        private static int CERT_STORE_OPEN_EXISTING_FLAG = 0x00004000;
        private static int CERT_SYSTEM_STORE_USERS = ((int)CERT_SYSTEM_STORE_USERS_ID << (int)CERT_SYSTEM_STORE_LOCATION_SHIFT);
        #endregion

        [DllImport("CRYPT32", EntryPoint = "CertOpenStore", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CertOpenStore(int storeProvider, int encodingType, int hcryptProv, int flags, string pvPara);

        [DllImport("CRYPT32", EntryPoint = "CertEnumCertificatesInStore", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CertEnumCertificatesInStore(IntPtr storeProvider, IntPtr prevCertContext);

        [DllImport("CRYPT32", EntryPoint = "CertCloseStore", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CertCloseStore(IntPtr storeProvider, int flags);

        [DllImport("crypt32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CertAddCertificateContextToStore([In] IntPtr hCertStore, [In] IntPtr pCertContext, [In] uint dwAddDisposition, [In, Out] IntPtr ppStoreContext);

        [DllImport("Crypt32.dll", SetLastError = true)]
        public static extern IntPtr CertCreateCertificateContext(
            Int32 dwCertEncodingType,
            Byte[] pbCertEncoded,
            Int32 cbCertEncoded
        );

        public const Int32 X509_ASN_ENCODING = 0x00000001;

        public const Int32 PKCS_7_ASN_ENCODING = 0x00010000;

        public const Int32 MY_TYPE = PKCS_7_ASN_ENCODING | X509_ASN_ENCODING;

        const uint CERT_STORE_ADD_ALWAYS = 4;

        #endregion

        public string ComputerName { get; set; }

        private readonly bool isLocalMachine;
        public CertStoreReader(string machineName)
        {
            ComputerName = machineName;
            if (machineName == string.Empty)
            {
                isLocalMachine = true;
            }
            else
            {
                isLocalMachine = string.Compare(ComputerName, Environment.MachineName, true) == 0 ? true : false;
            }
        }

        //
        // !!! NOVO !!!
        //
        // da bi ovo radilo, user pod kojim je startovan aplikacija mora na remote racunaru imati administratorske privilegije
        // ovo se testira iz CertificateLocalComp.msc konzole tako sto se proba zakaciti na taj racuna da se proveri da li postoji pristup certificate storu
        // ako pristup ne postoji nece raditi
        public void InstallCert(CertStoreName storeName, string certFilePath)
        {
            /*
            // Create a collection object and populate it using the PFX file
            X509Certificate2Collection collection = new X509Certificate2Collection();
            collection.Import(certFilePath, "1234", X509KeyStorageFlags.PersistKeySet);
            foreach (X509Certificate2 cert in collection)
            {
                var test = cert.Export(X509ContentType.Cert);
                int a = 1;
            }
            */

            //string fileName = "C:\\temp\\Test2.cer";
            string fileName = certFilePath;

            //var certificate = new X509Certificate(fileName);
            X509Certificate certificate = new X509Certificate(fileName,"1234");
            byte[] certificateBytes = certificate.Export(X509ContentType.Cert);
            var certContextHandle = CertCreateCertificateContext(X509_ASN_ENCODING, certificateBytes, certificateBytes.Length);

            // \\D140252.DMS.LOCAL\Personal

            var givenStoreName = GetStoreName(storeName);

            if (givenStoreName == string.Empty)
                throw new Exception("Invalid Store Name");

            IntPtr storeHandle = IntPtr.Zero;
            try
            {
                // remote comp
                storeHandle = CertOpenStore(
                    CERT_STORE_PROV_SYSTEM,
                    0,
                    0,
                    CERT_SYSTEM_STORE_LOCAL_MACHINE,
                    string.Format(@"\\{0}\{1}",ComputerName,givenStoreName));

                if (storeHandle == IntPtr.Zero)
                {
                    //throw new Exception(string.Format("Cannot connect to remote machine: {0}", ComputerName));
                    Console.WriteLine(string.Format("Cannot connect to remote machine: {0}", ComputerName));
                    return;
                }

                CertAddCertificateContextToStore(storeHandle, certContextHandle, CERT_STORE_ADD_ALWAYS, IntPtr.Zero);
                CertCloseStore(storeHandle, 0);
            }
            catch (Exception ex)
            {
                throw new Exception("Error opening Certificate Store", ex);
            }
            finally
            {
                if (storeHandle != IntPtr.Zero)
                    CertCloseStore(storeHandle, 0);
            }
        }

        public X509Certificate2Collection GetCertificates(CertStoreName storeName)
        {

            X509Certificate2Collection collectionToReturn = null;
            string givenStoreName = GetStoreName(storeName);

            if (givenStoreName == string.Empty)
            {
                throw new Exception("Invalid Store Name");
            }

            IntPtr storeHandle = IntPtr.Zero;

            try
            {
                storeHandle = CertOpenStore(CERT_STORE_PROV_SYSTEM, 0, 0, CERT_SYSTEM_STORE_LOCAL_MACHINE, string.Format(@"\\{0}\{1}", ComputerName, givenStoreName));
                if (storeHandle == IntPtr.Zero)
                {
                    throw new Exception(string.Format("Cannot connect to remote machine: {0}", ComputerName));
                }


                IntPtr currentCertContext = IntPtr.Zero;
                collectionToReturn = new X509Certificate2Collection();
                do
                {
                    currentCertContext = CertEnumCertificatesInStore(storeHandle, currentCertContext);
                    if (currentCertContext != IntPtr.Zero)
                    {
                        collectionToReturn.Add(new X509Certificate2(currentCertContext));
                    }
                }
                while (currentCertContext != (IntPtr)0);


            }
            catch (Exception ex)
            {
                throw new Exception("Error opening Certificate Store", ex);
            }
            finally
            {
                if (storeHandle != IntPtr.Zero)
                    CertCloseStore(storeHandle, 0);
            }

            return collectionToReturn;
        }

        private static string GetStoreName(CertStoreName certStoreName)
        {
            string storeName = string.Empty;
            switch (certStoreName)
            {
                case CertStoreName.MY:
                    storeName = "My";
                    break;

                case CertStoreName.ROOT:
                    storeName = "Root";
                    break;

                case CertStoreName.CA:
                    storeName = "CA";
                    break;

                case CertStoreName.TRUST:
                    storeName = "Trust";
                    break;
            }
            return storeName;
        }
    }
}