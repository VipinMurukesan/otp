# Email OTP Module

This project implements a secure OTP (One-Time Password) module for email-based authentication in an enterprise application. The module generates and validates OTPs, allowing only emails with specific domains to receive OTPs and enforcing time and attempt restrictions during OTP validation.

## Features

- **Domain-Restricted OTP Delivery**: Only allows OTP emails to addresses with the `.dso.org.sg` domain.
- **6-Digit OTP Generation**: Generates a random 6-digit OTP valid for 1 minute.
- **User Verification**: Allows up to 10 attempts to enter the OTP, with a timeout if the OTP is not validated within the specified period.

## Solution Overview

### `Email_OTP_Module` Class

The `Email_OTP_Module` class provides methods to initialize, generate, and verify OTPs.

### Status Codes

The following status codes are used throughout the module:

- `STATUS_EMAIL_OK`: Email containing OTP was sent successfully.
- `STATUS_EMAIL_FAIL`: Failed to send OTP email.
- `STATUS_EMAIL_INVALID`: Invalid email address.
- `STATUS_OTP_OK`: OTP was verified successfully.
- `STATUS_OTP_FAIL`: OTP verification failed after max attempts.
- `STATUS_OTP_TIMEOUT`: OTP timed out.

### Methods

#### `GenerateOtpEmail(string userEmail)`

- **Description**: Validates the email domain and sends a 6-digit OTP if valid.
- **Parameters**:
  - `userEmail` (string): Email address for OTP delivery.
- **Return Values**:
  - Returns `STATUS_EMAIL_OK`, `STATUS_EMAIL_FAIL`, or `STATUS_EMAIL_INVALID`.

#### `CheckOtp(Stream input)`

- **Description**: Reads user OTP input and validates it against the generated OTP, with a 1-minute timeout and a maximum of 10 attempts.
- **Parameters**:
  - `input` (Stream): Input stream for reading user-entered OTPs.
- **Return Values**:
  - Returns `STATUS_OTP_OK`, `STATUS_OTP_FAIL`, or `STATUS_OTP_TIMEOUT`.

#### `Start()` and `Close()`

- **Description**: Optional methods to initialize or release resources as needed.

### Helper Methods

- `IsValidEmail(string email)`: Verifies that the email ends with `.dso.org.sg`.
- `GenerateOtp()`: Generates a random 6-digit OTP.
- `SendEmail(string emailAddress, string emailBody)`: Placeholder for email-sending function, assumed to return `true`.

## Testing

Tests are implemented in `Email_OTP_ModuleTests` using xUnit to validate module functionality.

### Key Test Cases

1. **Email Validation**:
   - Tests that an invalid domain email returns `STATUS_EMAIL_INVALID`.
   - Tests that a valid domain email returns `STATUS_EMAIL_OK`.

2. **OTP Verification**:
   - **Valid OTP**: Verifies that a correct OTP within attempts returns `STATUS_OTP_OK`.
   - **Invalid OTP**: Tests incorrect OTP inputs up to max attempts, expecting `STATUS_OTP_FAIL`.
   - **Timeout**: Simulates a timeout and expects `STATUS_OTP_TIMEOUT`.


