using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class StreamReaderWithTimeout
{
    public static async Task<string> ReadLineWithTimeout(Stream stream, TimeSpan timeout)
    {
        // Create a StreamReader to read from the stream
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true))
        {
            // Start reading asynchronously
            var readTask = reader.ReadLineAsync();

            // Wait for the task to complete or for the timeout to occur
            if (await Task.WhenAny(readTask, Task.Delay(timeout)) == readTask)
            {
                return await readTask; // If the task completes in time, return the line
            }
            else
            {
                // Timeout occurred before reading completed
                return null;
            }
        }
    }
}

public class Program
{
    public static async Task Main()
    {
        // Create a mock input stream and a StreamWriter
        var mockInput = new MemoryStream();
        var writer = new StreamWriter(mockInput);

        // Write incorrect OTP 15 times
        for (int i = 0; i < 15; i++)
        {
            writer.WriteLine("654321");  // Incorrect OTP
        }

        // Ensure that data is written to the stream and then flush
        writer.Flush();

        // Reset the position of the MemoryStream to the beginning for reading
        mockInput.Position = 0;

        // Set a timeout (e.g., 3 seconds per line)
        var timeout = TimeSpan.FromSeconds(3);

        // Loop to read all 15 lines with timeout handling
        int lineCount = 0;
        string line;
        while (lineCount < 15) // Ensure we attempt to read 15 lines
        {
            // Reset the stream position to the beginning before each read
            mockInput.Position = 0;

            line = await StreamReaderWithTimeout.ReadLineWithTimeout(mockInput, timeout);

            if (line != null)
            {
                lineCount++;
                Console.WriteLine($"Line {lineCount}: {line}");
            }
            else
            {
                // Timeout occurred or unable to read the line
                Console.WriteLine("Timed out or failed to read all lines.");
                break;
            }
        }

        // After the loop, check if we've read all lines or if it timed out
        if (lineCount == 15)
        {
            Console.WriteLine("Successfully read all lines.");
        }
    }
}
