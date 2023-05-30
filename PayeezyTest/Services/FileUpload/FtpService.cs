using Microsoft.Extensions.Options;
using PayeezyTest.Dto;
using PayeezyTest.Models;
using System.Net;

namespace PayeezyTest.Services
{
    public class FtpService : IFtpService
    {
        private readonly FtpSettings _ftpSettings;
        private string PaidCsvFileName = "PaymentDetails_DateTimeStamp.csv";

        public FtpService(IOptions<FtpSettings> ftpSettings)
        {
            _ftpSettings = ftpSettings.Value;
        }

        public async Task<bool> UploadFileToFTP()
        {
            FtpWebRequest request = (FtpWebRequest) WebRequest.Create(_ftpSettings.HostUrl + "/" + PaidCsvFileName);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(_ftpSettings.Username, _ftpSettings.Password);

            byte[] fileContents = File.ReadAllBytes(PaidCsvFileName);

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
            return true;

        }

        public void AppendWriteCSV(string referralId, string paidDate, string paidAmount, string transactionId)
        {
            string detail = referralId + "," + paidDate + "," + paidAmount + "," + transactionId + Environment.NewLine;


            if (!System.IO.File.Exists(PaidCsvFileName))
            {
                string header = "Appointment_ReferralID" + "," + "date paid" + "," + "paid amount" + "," + "transaction id" + Environment.NewLine;

                System.IO.File.WriteAllText(PaidCsvFileName, header);
            }

            System.IO.File.AppendAllText(PaidCsvFileName, detail);
        }

        public void Dispose()
        {

        }
    }
}
