using System;
using System.Collections.Generic;
using System.Linq;
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
        private const int OTP_VALIDITY_DURATION_SECONDS = 6000;
        private const int MAX_ATTEMPTS = 10;

        private string? currentOtp;
        private DateTime otpGenerationTimeUtc;
        private int attempts = 0;

        private readonly Func<string> otpGenerator;
        public Email_OTP_Module(Func<string> otpGenerator = null)
        {
            this.otpGenerator = otpGenerator ?? GenerateOtp; // Use provided generator or default
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

            while (attempts < MAX_ATTEMPTS)
            {
                // Check if timeout has occurred
                if (DateTime.UtcNow > expiryTimeUtc)
                {
                    return STATUS_OTP_TIMEOUT;
                }

                // Read the OTP from the input stream
                string enteredOtp = ReadOtpWithTimeout1(input, expiryTimeUtc - DateTime.UtcNow);
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

            // Return STATUS_OTP_FAIL after MAX_ATTEMPTS if no correct OTP was entered
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


        public string ReadOtpWithTimeout1(Stream stream, TimeSpan timeout)
        {
            // Create a StreamReader to read from the stream
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                // Create a cancellation token source to handle timeout
                using (CancellationTokenSource cts = new CancellationTokenSource())
                {
                    // Start a task to read the line asynchronously
                    var readTask = Task.Run(() =>
                    {
                        return reader.ReadLine();
                    }, cts.Token);

                    // Wait for the task to complete or for the timeout to occur
                    if (Task.WhenAny(readTask, Task.Delay(timeout)).Result == readTask)
                    {
                        // If the read task completes within the timeout, return the result
                        return readTask.Result;
                    }
                    else
                    {
                        // If the timeout occurs, cancel the read task and return null
                        cts.Cancel();
                        return null;
                    }
                }
            }
        }

        private string ReadOtpWithTimeout(Stream input, TimeSpan timeout)
        {
            string otp = null;

            using (var cts = new CancellationTokenSource(timeout))
            {
                try
                {
                    otp = new StreamReader(input).ReadLine();
                    return otp;
                }
                catch (OperationCanceledException)
                {
                    otp = null;
                    return otp; // Timeout occurred
                }
            }
        }

        private bool SendEmail(string emailAddress, string emailBody)
        {
            // Assumes an external function to send email
            return true;
        }
    }
}
