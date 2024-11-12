using System;
using System.IO;
using Xunit;
using OTP.Module;
using System.Net.NetworkInformation;

namespace OTPTest
{


    public class Email_OTP_ModuleTests
    {
        private readonly Email_OTP_Module _otpModule;

        public Email_OTP_ModuleTests()
        {
            _otpModule = new Email_OTP_Module();
            _otpModule.Start();
        }

        [Fact]
        public void GenerateOtpEmail_InvalidEmailFormat_ReturnsStatusEmailInvalid()
        {
            int status = _otpModule.GenerateOtpEmail("invalidemail@wrongdomain.com");
            Assert.Equal(Email_OTP_Module.STATUS_EMAIL_INVALID, status);
        }

        [Fact]
        public void GenerateOtpEmail_ValidEmail_ReturnsStatusEmailOk()
        {
            int status = _otpModule.GenerateOtpEmail("testuser@domain.dso.org.sg");
            Assert.Equal(Email_OTP_Module.STATUS_EMAIL_OK, status);
        }

        [Fact]
        public void CheckOtp_ValidOtpWithinAttempts_ReturnsStatusOtpOk()
        {
            // Inject a fixed OTP generator that always returns "123456"
            var otpModule = new Email_OTP_Module(() => "123456");
            otpModule.GenerateOtpEmail("testuser@domain.dso.org.sg");

            // Mock OTP input stream with the known OTP "123456"
            var mockInput = new MemoryStream();
            var writer = new StreamWriter(mockInput);
            writer.WriteLine("123456");
            writer.Flush();
            mockInput.Position = 0;

            int status = otpModule.CheckOtp(mockInput);
            Assert.Equal(Email_OTP_Module.STATUS_OTP_OK, status);
        }

        [Fact]
        public void CheckOtp_InvalidOtpAfterMaxAttempts_ReturnsStatusOtpFail()
        {
            var otpModule = new Email_OTP_Module(() => "123456");
            otpModule.GenerateOtpEmail("testuser@domain.dso.org.sg");

            // Create a MemoryStream for the incorrect OTP inputs
            var mockInput = new MemoryStream();
            var writer = new StreamWriter(mockInput);
            for (int i = 0; i < 15; i++)  // Write incorrect OTP 15 times
            {
                writer.WriteLine("654321");  // Incorrect OTP
            }
            writer.Flush();
            mockInput.Position = 0;  // Reset stream position to the beginning



            // Call CheckOtp and expect STATUS_OTP_FAIL due to max attempts being reached
            int status = otpModule.CheckOtp(mockInput);
            Assert.Equal(Email_OTP_Module.STATUS_OTP_FAIL, status);

        }


        [Fact]
        public void CheckOtp_TimeoutBeforeInput_ReturnsStatusOtpTimeout()
        {
            _otpModule.GenerateOtpEmail("testuser@dso.org.sg");

            // Simulate delay and timeout
            System.Threading.Thread.Sleep(61000); // 61 seconds, longer than OTP validity period
            var mockInput = new MemoryStream();

            int status = _otpModule.CheckOtp(mockInput);
            Assert.Equal(Email_OTP_Module.STATUS_OTP_TIMEOUT, status);
        }
    }

}