using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace PlayBook3DTSL.Utilities.Helpers
{
    [ExcludeFromCodeCoverage]
    public class AppConfiguration
    {
        public string VerificationToken { get; set; }

        public string ProviderId { get; set; }

        public string RedoxApiKey { get; set; }

        public string RedoxSecretKey { get; set; }

        public string DestinationId { get; set; }

        public string DestinationName { get; set; }
        public string BlobStorageConnection { get; set; }
        public string BlobStorageCotainer { get; set; }

        public int ResetPasswordDays { get; set; }
        public string sasToken { get; set; }
        public string ConnectionString { get; set; }

        public string DicomUrl { get; set; }
        public string DicomUserName { get; set; }
        public string DicomUserPassword { get; set; }


        public string AMAConsumerKey { get; set; }
        public string AMAConsumerSecret { get; set; }
        public string AMATokenUrl { get; set; }
        public string AMAReleaseUrl { get; set; }
        public string AMAFilesUrl { get; set; }

        public string FromEmailAddress { get; set; }
        public string EmailPassword { get; set; }
        public string SmtpServer { get; set; }
        public string SmtpPortNumber { get; set; }
        public string ToEmail { get; set; }
        public string CcEmail { get; set; }
        public string RxTxDeviceIPUrl { get; set; }
        public string RxTxDeviceUser { get; set; }
        public string RxTxDevicePassword { get; set; }



        public string MediaPath
        {
            get; set;
        }
        public AppConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();
            var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configBuilder.AddJsonFile(path, false);
            var root = configBuilder.Build();
            VerificationToken = root.GetSection("Redox:VerificationToken").Value;
            MediaPath = root.GetSection("AzureConfiguration:MediaPath").Value;
            RedoxApiKey = root.GetSection("Redox:ApiKey").Value;
            RedoxSecretKey = root.GetSection("Redox:SecretKey").Value;
            ProviderId = root.GetSection("Redox:ProviderId").Value;
            DestinationId = root.GetSection("Redox:DestinationId").Value;
            DestinationName = root.GetSection("Redox:DestinationName").Value;
            ResetPasswordDays = Convert.ToInt32(root.GetSection("PasswordSetting:ResetPasswordDays").Value);
            BlobStorageConnection = root.GetSection("AzureConfiguration:BlobStorageConnectionString").Value;
            BlobStorageCotainer = root.GetSection("AzureConfiguration:BlobStorageContainer").Value;
            sasToken = root.GetSection("AzureConfiguration:sasToken").Value;
            ConnectionString = root.GetSection("ConnectionStrings:DefaultConnection").Value;
            DicomUrl = root.GetSection("Diacom:ServerUrl").Value;
            DicomUserName = root.GetSection("Diacom:UserName").Value;
            DicomUserPassword = root.GetSection("Diacom:Password").Value;
            AMAConsumerKey = root.GetSection("AMASetting:ConsumerKey").Value;
            AMAConsumerSecret = root.GetSection("AMASetting:ConsumerSecret").Value;
            AMATokenUrl = root.GetSection("AMASetting:TokenUrl").Value;
            AMAReleaseUrl = root.GetSection("AMASetting:ReleaseUrl").Value;
            AMAFilesUrl = root.GetSection("AMASetting:FilesUrl").Value;
            FromEmailAddress = root.GetSection("EmailSettings:FromEmailAddress").Value;
            EmailPassword = root.GetSection("EmailSettings:EmailPassword").Value;
            SmtpServer = root.GetSection("EmailSettings:SmtpServer").Value;
            SmtpPortNumber = root.GetSection("EmailSettings:SmtpPortNumber").Value;
            ToEmail = root.GetSection("EmailSettings:ToEmail").Value;
            CcEmail = root.GetSection("EmailSettings:CcEmail").Value;
            RxTxDeviceIPUrl = root.GetSection("RxTxDeviceSettings:RxTxDeviceIPUrl").Value;
            RxTxDeviceUser = root.GetSection("RxTxDeviceSettings:RxTxDeviceUser").Value;
            RxTxDevicePassword = root.GetSection("RxTxDeviceSettings:RxTxDevicePassword").Value;
        }
    }
}
