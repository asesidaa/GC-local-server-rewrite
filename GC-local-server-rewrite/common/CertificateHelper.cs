using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CertificateManager;
using CertificateManager.Models;
using Microsoft.Extensions.DependencyInjection;
using Swan;
using Swan.Logging;

namespace GCLocalServerRewrite.common;

public static class CertificateHelper
{
    private const X509KeyUsageFlags ROOT_CA_X509_KEY_USAGE_FLAGS = X509KeyUsageFlags.KeyCertSign |
                                                                   X509KeyUsageFlags.DataEncipherment |
                                                                   X509KeyUsageFlags.KeyEncipherment |
                                                                   X509KeyUsageFlags.DigitalSignature;

    private const X509KeyStorageFlags X509_KEY_STORAGE_FLAGS_MACHINE = X509KeyStorageFlags.PersistKeySet |
                                                                       X509KeyStorageFlags.MachineKeySet | 
                                                                       X509KeyStorageFlags.Exportable;

    private const X509KeyUsageFlags CERT_X509_KEY_USAGE_FLAGS = X509KeyUsageFlags.DataEncipherment |
                                                                X509KeyUsageFlags.KeyEncipherment |
                                                                X509KeyUsageFlags.DigitalSignature;

    private static readonly DistinguishedName ROOT_CA_DISTINGUISHED_NAME = new()
    {
        CommonName = Configs.ROOT_CA_CN
    };

    private static readonly DistinguishedName CERT_DISTINGUISHED_NAME = new()
    {
        CommonName = Configs.CERT_CN
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

    private static readonly SubjectAlternativeName SUBJECT_ALTERNATIVE_NAME = new()
    {
        DnsName = Configs.DOMAINS,
        IpAddress = System.Net.IPAddress.Parse(Configs.SETTINGS.ServerIp)
    };

    private static readonly ValidityPeriod VALIDITY_PERIOD = new()
    {
        ValidFrom = DateTime.UtcNow,
        ValidTo = DateTime.UtcNow.AddYears(3)
    };

    private static readonly OidCollection OID_COLLECTION = new()
    {
        OidLookup.AnyPurpose
    };

    public static X509Certificate2 InitializeCertificate()
    {
        if (CertificateExists())
        {
            var existingCert = GetCertificate(StoreName.My, StoreLocation.LocalMachine, Configs.CERT_CN);

            if (existingCert != null)
            {
                return existingCert;
            }
            "Existing CN not found! Removing old certificates and genrate new ones...".Info();
        }

        RemovePreviousCert(StoreName.My, StoreLocation.LocalMachine);
        RemovePreviousCert(StoreName.Root, StoreLocation.LocalMachine);

        var serviceProvider = new ServiceCollection()
            .AddCertificateManager().BuildServiceProvider();

        var createCertificates = serviceProvider.GetService<CreateCertificates>();

        if (createCertificates == null)
        {
            throw SelfCheck.Failure("Cannot initialize CreateCertificates service!");
        }

        var rootCa = createCertificates.NewRsaSelfSignedCertificate(
            ROOT_CA_DISTINGUISHED_NAME,
            ROOT_CA_BASIC_CONSTRAINTS,
            VALIDITY_PERIOD,
            SUBJECT_ALTERNATIVE_NAME,
            OID_COLLECTION,
            ROOT_CA_X509_KEY_USAGE_FLAGS,
            new RsaConfiguration()
        );

        var cert = createCertificates.NewRsaChainedCertificate(
            CERT_DISTINGUISHED_NAME,
            CERT_BASIC_CONSTRAINTS,
            VALIDITY_PERIOD,
            SUBJECT_ALTERNATIVE_NAME,
            rootCa,
            OID_COLLECTION,
            CERT_X509_KEY_USAGE_FLAGS,
            new RsaConfiguration()
        );

        var exportService = serviceProvider.GetService<ImportExportCertificate>();

        if (exportService == null)
        {
            throw SelfCheck.Failure("Cannot initialize ImportExportCertificate service!");
        }

        var rootCaPfxBytes = exportService.ExportRootPfx(null, rootCa);
        var certPfxBytes = exportService.ExportChainedCertificatePfx(null, cert, rootCa);

        var rootCaWithPrivateKey = new X509Certificate2(rootCaPfxBytes, (string)null!,
            X509_KEY_STORAGE_FLAGS_MACHINE);

        var certWithPrivateKey = new X509Certificate2(certPfxBytes, (string)null!,
            X509_KEY_STORAGE_FLAGS_MACHINE);


        AddCertToStore(rootCaWithPrivateKey, StoreName.My, StoreLocation.LocalMachine);
        AddCertToStore(rootCaWithPrivateKey, StoreName.Root, StoreLocation.LocalMachine);
        AddCertToStore(certWithPrivateKey, StoreName.My, StoreLocation.LocalMachine);

        Directory.CreateDirectory(PathHelper.CertRootPath);

        File.WriteAllBytes(Path.Combine(PathHelper.CertRootPath, "root.pfx"), rootCaWithPrivateKey.Export(X509ContentType.Pfx));
        File.WriteAllBytes(Path.Combine(PathHelper.CertRootPath, "cert.pfx"), certWithPrivateKey.Export(X509ContentType.Pfx));

        return certWithPrivateKey;
    }

    private static void AddCertToStore(X509Certificate2 cert, StoreName storeName, StoreLocation storeLocation)
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
            e.Error(e.Source ?? "", e.Message);
        }
    }

    private static void RemovePreviousCert(StoreName storeName, StoreLocation storeLocation)
    {
        try
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            var result = store.Certificates.Find(X509FindType.FindByIssuerName, Configs.ROOT_CA_CN, true);

            if (result.Any())
            {
                store.RemoveRange(result);
                "Removed previous certs!".Info();
            }

            store.Close();
        }
        catch (Exception e)
        {
            e.Error(e.Source ?? "", e.Message);
        }
    }

    private static bool CertificateExists()
    {
        try
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var result = store.Certificates.Find(X509FindType.FindByIssuerName, Configs.ROOT_CA_CN, true);

            if (result.Count == 2)
            {
                "Certificate exists!".Info();

                return true;
            }

            store.Close();
            "Certificate not found! Will generate new certs...".Info();

            return false;
        }
        catch (Exception e)
        {
            e.Error(e.Source ?? "", e.Message);

            return false;
        }
    }

    private static X509Certificate2? GetCertificate(StoreName storeName, StoreLocation storeLocation, string commonName)
    {
        try
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);
            var result = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName,
                $"CN={commonName}", true);

            if (result.Any())
            {
                $"Certificate CN={commonName} found!".Info();

                return result.First();
            }

            store.Close();

            return null;
        }
        catch (Exception e)
        {
            e.Error(e.Source ?? "", e.Message);

            return null;
        }
    }
}