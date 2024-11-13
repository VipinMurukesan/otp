using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OTP.Module
{
    public class Email_OTP_Module
    {
        // Status Codes
        public const int STATUS_EMAIL_OK = 0;
        public const int STATUS_EMAIL_FAIL = 1;
        public const int STATUS_EMAIL_INVALID = 2;
        public const int STATUS_OTP_OK = 3;
        public const int STATUS_OTP_FAIL = 4;
        public const int STATUS_OTP_TIMEOUT = 5;

        private const string VALID_EMAIL_DOMAIN = ".dso.org.sg";
        private const int OTP_VALIDITY_DURATION_SECONDS = 60;
        private const int OTP_MAX_ATTEMPTS = 10;


        private string? currentOtp;
        private DateTime otpGenerationTimeUtc;
        private int attempts = 0;

        private readonly Func<string> otpGenerator;
        public Email_OTP_Module(Func<string> otpGenerator = null)
        {
            // Use provided generator or default            
            this.otpGenerator = otpGenerator ?? GenerateOtp;
        }

        public void Start() { /* Initialize resources if needed */ }
        public void Close() { /* Clean up resources if needed */ }

        public int GenerateOtpEmail(string userEmail)
        {
            if (!IsValidEmail(userEmail))
            {
                return STATUS_EMAIL_INVALID;
            }

            currentOtp = otpGenerator();
            otpGenerationTimeUtc = DateTime.UtcNow;

            string emailBody = $"Your OTP Code is {currentOtp}. The code is valid for 1 minute.";
            bool emailSent = SendEmail(userEmail, emailBody);

            return emailSent ? STATUS_EMAIL_OK : STATUS_EMAIL_FAIL;
        }

        public int CheckOtp(Stream input)
        {

            DateTime expiryTimeUtc = otpGenerationTimeUtc.AddSeconds(OTP_VALIDITY_DURATION_SECONDS);

            using (StreamReader reader = new StreamReader(input, Encoding.UTF8))
            {
                using (CancellationTokenSource cts = new CancellationTokenSource())
                {
                    while (attempts < OTP_MAX_ATTEMPTS)
                    {
                        // Check if timeout has occurred
                        if (DateTime.UtcNow > expiryTimeUtc)
                        {
                            return STATUS_OTP_TIMEOUT;
                        }

                        // Start a task to read the line asynchronously
                        var readTask = Task.Run(() =>
                        {
                            return reader.ReadLine();
                        }, cts.Token);

                        string enteredOtp = null;

                        // Wait for the task to complete or for the timeout to occur
                        if (Task.WhenAny(readTask, Task.Delay(expiryTimeUtc - DateTime.UtcNow)).Result == readTask)
                        {
                            // If the read task completes within the timeout, return the result
                            enteredOtp = readTask.Result;
                        }
                        else
                        {
                            // If the timeout occurs, cancel the read task and return null
                            cts.Cancel();
                            enteredOtp = null;
                        }

                        if (enteredOtp == null)
                        {
                            return STATUS_OTP_TIMEOUT; // Timeout occurred
                        }

                        // Check if the entered OTP is correct
                        if (enteredOtp == currentOtp)
                        {
                            return STATUS_OTP_OK;
                        }

                        attempts++;
                    }
                }
            }

            // Return STATUS_OTP_FAIL after OTP_MAX_ATTEMPTS if no correct OTP was entered
            return STATUS_OTP_FAIL;
        }


        private bool IsValidEmail(string email)
        {
            // Check if email ends with the required domain
            if (!email.EndsWith(VALID_EMAIL_DOMAIN))
            {
                return false;

            }
            else
            {
                return true;
            }

        }

        private string GenerateOtp()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString("D6");
        }                   
              

        private bool SendEmail(string emailAddress, string emailBody)
        {
            // Assumes an external function to send email
            return true;
        }
    }
}
