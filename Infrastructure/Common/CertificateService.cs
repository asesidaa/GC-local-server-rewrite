using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CertificateManager;
using CertificateManager.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Common;

public class CertificateService
{
    private const string ENHANCED_KEY_USAGE_OID = "2.5.29.37";
    
    private const X509KeyUsageFlags ROOT_CA_X509_KEY_USAGE_FLAGS = X509KeyUsageFlags.KeyCertSign |
                                                                   X509KeyUsageFlags.CrlSign;

    private const X509KeyStorageFlags X509_KEY_STORAGE_FLAGS_MACHINE = X509KeyStorageFlags.PersistKeySet |
                                                                       X509KeyStorageFlags.MachineKeySet |
                                                                       X509KeyStorageFlags.Exportable;

    private const X509KeyUsageFlags CERT_X509_KEY_USAGE_FLAGS = X509KeyUsageFlags.DataEncipherment |
                                                                X509KeyUsageFlags.KeyEncipherment |
                                                                X509KeyUsageFlags.DigitalSignature;

    private const string ROOT_CA_CN = "Taito Arcade Machine CA";
    private const string CERT_CN = "GC local server";
    private const string CERT_CN2 = "nesys";
    private const string CERT_DIR = "Certificates";
    private const string CERT_FILE_NAME = "cert.pfx";
    private const string ROOT_CERT_FILE_NAME = "root.pfx";
    private static readonly string CERT_PATH = Path.Combine(CERT_DIR, CERT_FILE_NAME);
    private static readonly string ROOT_CERT_PATH = Path.Combine(CERT_DIR, ROOT_CERT_FILE_NAME);

    private ILogger logger;

    private static readonly DistinguishedName ROOT_CA_DISTINGUISHED_NAME = new()
    {
        CommonName = ROOT_CA_CN
    };

    private static readonly DistinguishedName CERT_DISTINGUISHED_NAME = new()
    {
        CommonName = CERT_CN
    };

    private static readonly BasicConstraints ROOT_CA_BASIC_CONSTRAINTS = new()
    {
        CertificateAuthority = true,
        HasPathLengthConstraint = true,
        PathLengthConstraint = 3,
        Critical = true
    };

    public static readonly BasicConstraints CERT_BASIC_CONSTRAINTS = new()
    {
        CertificateAuthority = false,
        HasPathLengthConstraint = false,
        PathLengthConstraint = 0,
        Critical = true,
    };

    private readonly SubjectAlternativeName subjectAlternativeName;

    private static readonly ValidityPeriod VALIDITY_PERIOD = new()
    {
        ValidFrom = DateTime.UtcNow,
        ValidTo = DateTime.UtcNow.AddYears(1)
    };

    private static readonly OidCollection OID_COLLECTION = new()
    {
        OidLookup.ServerAuthentication,
        OidLookup.AnyPurpose
    };

    public CertificateService(string serverIp, ILogger logger)
    {
        this.logger = logger;
        subjectAlternativeName = new SubjectAlternativeName
        {
            DnsName = new List<string>
            {
                "localhost",
                "cert.nesys.jp",
                "nesys.taito.co.jp",
                "fjm170920zero.nesica.net"
            },
            IpAddress = System.Net.IPAddress.Parse(serverIp)
        };
    }

    public X509Certificate2 InitializeCertificate()
    {
        return Environment.OSVersion.Platform == PlatformID.Win32NT
            ? InitializeCertificateWindows()
            : InitializeCertificateOthers();
    }

    private X509Certificate2 InitializeCertificateOthers()
    {
        if (CertificateExists())
        {
            return new X509Certificate2(CERT_PATH);
        }

        logger.LogInformation("Existing certs not found! Removing old certificates and genrate new ones...");
        
        File.Delete(CERT_PATH);
        File.Delete(ROOT_CERT_PATH);

        return GenerateCertificate();
    }

    private X509Certificate2 InitializeCertificateWindows()
    {
        if (CertificateExists())
        {
            var existingCert = GetCertificate(StoreName.My, StoreLocation.LocalMachine, CERT_CN);

            if (existingCert is not null)
            {
                return existingCert;
            }
            
            logger.LogInformation("First try not found, changing CN to nesys");
            
            existingCert = GetCertificate(StoreName.My, StoreLocation.LocalMachine, CERT_CN2);
            if (existingCert is not null)
            {
                return existingCert;
            }

            logger.LogInformation("Existing certs not found or are not valid! " +
                                  "Removing old certificates and genrate new ones...");
        }

        RemovePreviousCert(StoreName.My, StoreLocation.LocalMachine);
        RemovePreviousCert(StoreName.Root, StoreLocation.LocalMachine);
        RemovePreviousCert(StoreName.Root, StoreLocation.CurrentUser);

        return GenerateCertificate();
    }

    private X509Certificate2 GenerateCertificate()
    {
        var serviceProvider = new ServiceCollection()
            .AddCertificateManager().BuildServiceProvider();

        var createCertificates = serviceProvider.GetService<CreateCertificates>();

        if (createCertificates == null)
        {
            logger.LogError("Cannot initialize CreateCertificates service!");
            throw new Exception();
        }

        var rootCa = createCertificates.NewRsaSelfSignedCertificate(
            ROOT_CA_DISTINGUISHED_NAME,
            ROOT_CA_BASIC_CONSTRAINTS,
            VALIDITY_PERIOD,
            new SubjectAlternativeName(),
            OID_COLLECTION,
            ROOT_CA_X509_KEY_USAGE_FLAGS,
            new RsaConfiguration()
        );

        var cert = createCertificates.NewRsaChainedCertificate(
            CERT_DISTINGUISHED_NAME,
            new BasicConstraints(),
            VALIDITY_PERIOD,
            subjectAlternativeName,
            rootCa,
            OID_COLLECTION,
            CERT_X509_KEY_USAGE_FLAGS,
            new RsaConfiguration()
        );

        var exportService = serviceProvider.GetService<ImportExportCertificate>();

        if (exportService == null)
        {
            logger.LogError("Cannot initialize ImportExportCertificate service!");
            throw new Exception();
        }

        var rootCaPfxBytes = exportService.ExportRootPfx(null, rootCa);
        var certPfxBytes = exportService.ExportChainedCertificatePfx(null, cert, rootCa);

        var rootCaWithPrivateKey = new X509Certificate2(rootCaPfxBytes, (string)null!,
            X509_KEY_STORAGE_FLAGS_MACHINE);

        var certWithPrivateKey = new X509Certificate2(certPfxBytes, (string)null!,
            X509_KEY_STORAGE_FLAGS_MACHINE);


        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            AddCertToStore(rootCaWithPrivateKey, StoreName.My, StoreLocation.LocalMachine);
            AddCertToStore(rootCaWithPrivateKey, StoreName.Root, StoreLocation.LocalMachine);
            AddCertToStore(certWithPrivateKey, StoreName.My, StoreLocation.LocalMachine);
            AddCertToStore(certWithPrivateKey, StoreName.My, StoreLocation.CurrentUser);
            logger.LogInformation("Added new certs to store!");
        }

        Directory.CreateDirectory(CERT_DIR);

        File.WriteAllBytes(ROOT_CERT_PATH, rootCaWithPrivateKey.Export(X509ContentType.Pfx));
        File.WriteAllBytes(CERT_PATH, certWithPrivateKey.Export(X509ContentType.Pfx));
        logger.LogInformation("New certs saved!");

        return certWithPrivateKey;
    }
    
    private void AddCertToStore(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation)
    {
        try
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);

            store.Close();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An exception occurs when adding certificate");
        }
    }

    private void RemovePreviousCert(StoreName storeName, StoreLocation storeLocation)
    {
        try
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            var result = store.Certificates.Find(X509FindType.FindByIssuerName, ROOT_CA_CN, false);

            if (result.Any())
            {
                store.RemoveRange(result);
                logger.LogInformation("Removed previous certificates!");
            }

            store.Close();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An exception occurs when removing previous certificates");
        }
    }

    private bool CertificateExists()
    {
        bool certificateExists;
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            // Non windows just use generated certificate file
            certificateExists = Path.Exists(Path.Combine(CERT_DIR, "cert.pfx")) &&
                                Path.Exists(Path.Combine(CERT_DIR, "root.pfx"));
        }
        else
        {
            try
            {
                var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                var result = store.Certificates.Find(X509FindType.FindByIssuerName, ROOT_CA_CN, true);

                certificateExists = result.Count != 0;

                store.Close();
            }
            catch (Exception e)
            {
                logger.LogError(e, "An exception occurs when checking certificates");

                return false;
            }
        }

        if (certificateExists)
        {
            logger.LogInformation("Certificate exists!");
        }
        else
        {
            logger.LogInformation("Certificate not found! Will generate new certs...");
        }

        return certificateExists;
    }

    private X509Certificate2? GetCertificate(StoreName storeName, StoreLocation storeLocation, string commonName)
    {
        try
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);
            var result = store.Certificates.Find(X509FindType.FindBySubjectName,
                $"{commonName}", true);

            if (result.Any())
            {
                logger.LogInformation("Certificate CN={CommonName} found!", commonName);

                var cert = result.First();
                var extensions = cert.Extensions;
                var enhancedUsage = extensions.FirstOrDefault(extension => ENHANCED_KEY_USAGE_OID.Equals(extension.Oid?.Value));
                if (enhancedUsage is X509EnhancedKeyUsageExtension usages)
                {
                    foreach (var usage in usages.EnhancedKeyUsages)
                    {
                        if (OidLookup.ServerAuthentication.Value!.Equals(usage.Value))
                        {
                            return cert;
                        }
                    }
                }
                logger.LogInformation("Certificate CN={CommonName} does not include server authentication!", commonName);
                return null;
            }

            store.Close();

            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An exception occurs when getting certificate {CommonName}", commonName);

            return null;
        }
    }
}