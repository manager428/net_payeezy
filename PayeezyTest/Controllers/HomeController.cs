using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PayeezyTest.Dto;
using PayeezyTest.Models;
using PayeezyTest.Services;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PayeezyTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private PayeezySettings _payeezySettings { get; }
		private readonly FtpSettings _ftpSettings;
		private readonly IPatientService _patientService;
		private readonly IFtpService _ftpService;
		private readonly IEmailSender _emailSender;
		private Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);

		public HomeController(
			ILogger<HomeController> logger, 
			IPatientService patientService,
			IFtpService ftpService,
			IEmailSender emailSender,
			IOptions<FtpSettings> ftpSettings,
			IOptions<PayeezySettings> payeezySettings)
        {
            _logger = logger;
            _payeezySettings = payeezySettings.Value;
			_patientService = patientService;
			_ftpService = ftpService;
			_emailSender = emailSender;
			_ftpSettings = ftpSettings.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

		[HttpGet]
		[Route("patient/{referalID:int}")]

		public async Task<IActionResult> PatientPayPage(int referalID)
        {
			var patientData = await _patientService.GetPatientCoPayData(referalID);

			return View(new PaymentDetailDto
			{
				PatientCoPay = patientData,
				Amount = patientData == null ? 0 : patientData.AmountDue,
				ReferalId = patientData == null ? 0 : patientData.Appointment_ReferralID
			});
		}


		[HttpGet]
		[Route("patient/TriggerFTPLookup")]

		public async Task<IActionResult> TriggerFTPLookup()
		{
			var result = await InsertCoPayData();

			return new OkObjectResult(result);
		}

		[HttpPost]
        public async Task<IActionResult> PayTransaction(PaymentDetailDto dto)
        {
			var paymentCard = new
			{
				type = "visa",
				cardholder_name = dto.CardHolderName,
				card_number = dto.CardNumber.Replace(" ", ""),
				exp_date = dto.ExpireDate.Replace("/", ""),
				cvv = dto.Cvv
			};

			var payload = new
			{
				merchant_ref = "WEBFORMS TEST",
				transaction_type = "authorize",
				method = "credit_card",
				amount = dto.Amount.ToString(),
				currency_code = "USD",
				credit_card = paymentCard
			};

			string payloadJson = JsonConvert.SerializeObject(payload);

			Random random = new Random();
			string nonce = (random.Next(0, 1000000)).ToString();

			string time = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();


			string token = _payeezySettings.MerchantToken; //Merchant token
			string apiKey = _payeezySettings.ApiKey; //apikey
			string apiSecret = _payeezySettings.ApiSecret; //API secret
			string hashData = apiKey + nonce + time + token + payloadJson;

			string base64Hash = Convert.ToBase64String(CalculateHMAC(hashData, apiSecret));

			string url = "https://api-cert.payeezy.com/v1/transactions";

			//prior to .NET 4.6, TLS1.2 is not default, the following will work for .NET 4.0 and above
			ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
			//begin HttpWebRequest
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

			webRequest.Method = "POST";
			webRequest.Accept = "*/*";
			webRequest.Headers.Add("timestamp", time);
			webRequest.Headers.Add("nonce", nonce);
			webRequest.Headers.Add("token", token);
			webRequest.Headers.Add("apikey", apiKey);
			webRequest.Headers.Add("Authorization", base64Hash);
			webRequest.ContentLength = payloadJson.Length;
			webRequest.ContentType = "application/json";

			StreamWriter writer = null;
			writer = new StreamWriter(webRequest.GetRequestStream());
			writer.Write(payloadJson);
			writer.Close();

			string responseString;
			try
			{
				using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
				{
					using (StreamReader responseStream = new StreamReader(webResponse.GetResponseStream()))
					{
						responseString = responseStream.ReadToEnd();
						dynamic jsonSuccess = JsonConvert.DeserializeObject(responseString);
						await _patientService.UpdatePatientCoPaymentPaid(dto.ReferalId, "success", $"{jsonSuccess.transaction_id}", dto.Amount);
						
						_ftpService.AppendWriteCSV(dto.ReferalId.ToString(), DateTime.Now.ToString(), dto.Amount.ToString(), $"{jsonSuccess.transaction_id}");
						await _ftpService.UploadFileToFTP();
						await _emailSender.SendEmailAsync("", "Payment success", $"Payment Details Appointment_ReferralID : {dto.ReferalId}, Amount: {dto.Amount.ToString("C", new CultureInfo("en-US"))}, Transaction ID: {jsonSuccess.transaction_id}");
						return Content($"<div class='alert alert-success alert-dismissible' role='alert' style='background-color:#59ca7c;word-break: break-word;'><button type='button' class='close' data-dismiss='alert' aria-label='Close'><span aria-hidden='true'>×</span></button><strong>Success!!!</strong> Successfully paid <br/>Transaction status: {jsonSuccess.transaction_status}, Transaction ID: {jsonSuccess.transaction_id}</div>");
					}
				}
			}
			catch (WebException ex)
			{
				if (ex.Response != null)
				{
					if (ex.Response is FtpWebResponse)
                    {
						return Content($"<div class='alert alert-danger alert-dismissible' role='alert'><button type='button' class='close' data-dismiss='alert' aria-label='Close'><span aria-hidden='true'>×</span></button><strong>Oops!!!</strong> {ex.Message} {ex.StackTrace}</div>");
					} else
                    {
						using (HttpWebResponse errorResponse = (HttpWebResponse)ex.Response)
						{
							using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream()))
							{
								string remoteEx = reader.ReadToEnd();
								dynamic jsonError = JsonConvert.DeserializeObject(remoteEx);

								string errorMsg = "";
								if (jsonError != null && jsonError?.Error != null)
								{
									errorMsg = "<strong>Oops!!!</strong><br/>";
									foreach (var error in jsonError?.Error?.messages)
									{
										errorMsg += error.description + "<br/>";
									}
								}
								else
								{
									errorMsg = "<strong>Oops!!!</strong>Something error";
								}
								await _patientService.UpdatePatientCoPaymentPaid(dto.ReferalId, "failed", "", 0);
								return Content($"<div class='alert alert-danger alert-dismissible' role='alert'><button type='button' class='close' data-dismiss='alert' aria-label='Close'><span aria-hidden='true'>×</span></button>{errorMsg}</div>");
							}
						}
					}
					
				}
			}
			return Content($"<div class='alert alert-danger alert-dismissible' role='alert'><button type='button' class='close' data-dismiss='alert' aria-label='Close'><span aria-hidden='true'>×</span></button><strong>Oops!!!</strong> Something error</div>");
		}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private byte[] CalculateHMAC(string data, string secret)
        {
            HMAC hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] hmac2Hex = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(data));

            string hex = BitConverter.ToString(hmac2Hex);
            hex = hex.Replace("-", "").ToLower();
            byte[] hexArray = Encoding.UTF8.GetBytes(hex);
            return hexArray;
        }

		private async Task<string> InsertCoPayData()
		{
			Console.WriteLine("Read Csv file from FTP server");
			
			FtpWebRequest request = (FtpWebRequest)WebRequest.Create(_ftpSettings.HostUrl + "/" + _ftpSettings.CsvFileName);
			request.Method = WebRequestMethods.Ftp.DownloadFile;

			request.Credentials = new NetworkCredential(_ftpSettings.Username, _ftpSettings.Password);

			FtpWebResponse response = (FtpWebResponse)request.GetResponse();
			//use the response like below
			Stream responseStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(responseStream);
			string[] allLines = reader.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			int createdCount = 0;
			Console.WriteLine("Inserting Records to database");

			foreach (string line in allLines)
			{
				string[] cells = SplitCSV(line);

				if (cells.Length > 0 && int.TryParse(cells[0], out _))
				{
					try
					{
						DateTime aDate = DateTime.ParseExact($"{cells[13]} {cells[14]}", "ddd MM/dd/yyyy h:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);

						string[] dobStrings = cells[5].Split("/");
						string dobString = cells[5];
						if (dobStrings.Length == 3 && dobStrings[2].Length == 2)
						{
							int year;
							int.TryParse(dobStrings[2], out year);
							if (year < 22)
							{
								dobString = dobStrings[0] + "/" + dobStrings[1] + "/20" + dobStrings[2];
							}
							else
							{
								dobString = dobStrings[0] + "/" + dobStrings[1] + "/19" + dobStrings[2];
							}

						}
						DateTime dobDate = DateTime.ParseExact(dobString, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
						int referalId, patientID;
						int.TryParse(cells[0], out referalId);
						int.TryParse(cells[1], out patientID);
						double amountDue;
						double.TryParse(cells[7].Replace("$", ""), out amountDue);
						PatientCoPay dto = new PatientCoPay
						{
							Appointment_ReferralID = referalId,
							PatientID = patientID,
							PatientLN = cells[2].ToString(),
							PatientFN = cells[3].ToString(),
							Email = cells[4].ToString(),
							DOB = dobDate,
							InsuranceCoName = cells[6].ToString(),
							AmountDue = amountDue,
							LocationName = cells[8].ToString(),
							Address = cells[9],
							City = cells[10],
							State = cells[11],
							ZipCode = cells[12],
							ADate = aDate
						};
						var isCreated = await _patientService.CreatePatientCoPay(dto);
						if (isCreated) createdCount++;
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Failed Creating record {ex.Message}: {cells[0]}");
					}

				}
			}
			Console.WriteLine($"Created Records to database {createdCount} of {allLines.Length - 1}");
			return $"Created Records to database {createdCount} of {allLines.Length - 1}";
		}

		private string[] SplitCSV(string input)
		{
			
			List<string> list = new List<string>();
			string curr = null;
			foreach (Match match in csvSplit.Matches(input))
			{
				curr = match.Value;
				if (0 == curr.Length)
				{
					list.Add("");
				}

				list.Add(curr.TrimStart(','));
			}

			return list.ToArray();
		}
	}
}