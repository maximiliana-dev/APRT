using CsvHelper;
using CsvHelper.Configuration;
using PCSC;
using PCSC.Iso7816;
using System.Globalization;
using System.Reflection;

namespace aprt
{
    internal class Program
    {


    static List<PinRecord>? records;

    static List<PinRecord>? ReadCsvFile(string filePath)
    {
        try {

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
            return new List<PinRecord>(csv.GetRecords<PinRecord>());
     }
     catch{
        return null;
     }
    }
        private static void Main(string[] args)
        {

            string? version = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString();
            Console.WriteLine($"APRT (Assisted PIN Remove Tool) PC/SC {version} by aeri\n\n");
            
            var csvFilePath = "pins.csv";
            records = ReadCsvFile(csvFilePath);

            if (records != null){
                Console.WriteLine($"Loaded {records.Count} PIN records");
            } 
            
            try
            {
                using SCardContext context = new();
                context.Establish(SCardScope.System);

                string[] readerNames = context.GetReaders();
                while (readerNames == null || readerNames.Length == 0)
                {
                    Console.WriteLine("ERROR: No readers available.");
                    ConsoleKeyInfo pressedKey = Console.ReadKey();
                    readerNames = context.GetReaders();
                }

                string? readerName = null;

                if (readerNames.Length > 1)
                {
                    for (int i = 0; i < readerNames.Length; i++)
                    {
                        Console.WriteLine($"{i}: {readerNames[i]}");
                    }
                    while (readerName == null)
                    {
                        Console.Write("Select a reader -> ");
                        string? reader = Console.ReadLine();

                        if (int.TryParse(reader, out int index))
                        {
                            if (index < readerNames.Length)
                            {
                                readerName = readerNames[index];
                            }
                        }
                    }
                }
                else
                {
                    readerName = readerNames[0];
                }

                using SCardReader rfidReader = new(context);

                while (true)
                {
                    Console.WriteLine("Ensure a card is insterted inside a reader and press enter.");

                    ConsoleKeyInfo pressedKey = Console.ReadKey();
                    SCardError connectResult = rfidReader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);

                    while (!rfidReader.IsConnected)
                    {
                        Console.WriteLine($"ERROR: Cannot read card because {connectResult}");
                        Console.WriteLine("Ensure a card is insterted inside a reader and press enter. ->\n");
                        pressedKey = Console.ReadKey();
                        connectResult = rfidReader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                    }

                    //-----------------------------

                    CommandApdu selectFileCommand = ADPUBuilder.SelectICCID(rfidReader.ActiveProtocol);
                    ResponseApdu responseSelect = Transmit(rfidReader, selectFileCommand);

                    if (responseSelect.SW1 == 0x9F && responseSelect.SW2 == 0x0F)
                    {
                        CommandApdu readBinaryCommand = ADPUBuilder.ReadICCID(rfidReader.ActiveProtocol);

                        ResponseApdu responseRead = Transmit(rfidReader, readBinaryCommand);

                        if (responseRead.SW1 == 0x90 && responseRead.SW2 == 0x00)
                        {

                            byte[] rawIccid = responseRead.GetData();
                            string iccid = BitConverter.ToString(Utils.ReverseEndianness(rawIccid)).Replace("-", string.Empty);

                            Console.WriteLine($"ICCID: {iccid}");

                            PinRecord? record = null;
                            string? pin = null;

                            record = records?.Find( x => x.ICC == iccid.Remove(iccid.Length-1) );
                         

                            if (record?.PIN == null){

                                Console.Write($"Enter current PIN -> ");

                                pin = Console.ReadLine();
                                while (pin == null || !pin.All(char.IsDigit))
                                {
                                    Console.Write($"Enter a valid PIN -> ");
                                    pin = Console.ReadLine();
                                }
                            }
                            else{
                                pin = record.PIN;
                                Console.WriteLine($"Found PIN {pin} in file.");
                            }



                            CommandApdu apduCommand = ADPUBuilder.DisablePIN(rfidReader.ActiveProtocol, pin);

                            nint sendPci = SCardPCI.GetPci(rfidReader.ActiveProtocol);
                            SCardPCI receivePci = new();

                            byte[] receiveBuffer = new byte[256];
                            int receiveBufferSize = receiveBuffer.Length;

                            rfidReader.Transmit(sendPci, apduCommand.ToArray(), receivePci, ref receiveBuffer);

                            ResponseApdu responseApdu = new(receiveBuffer, IsoCase.Case2Short, rfidReader.ActiveProtocol);

                            if (responseApdu.SW1 == 0x90 && responseApdu.SW2 == 0x00)
                            {
                                Console.WriteLine("OK: PIN removed successfully.\n");
                            }
                            else if (responseApdu.SW1 == 0x98 && responseApdu.SW2 == 0x08)
                            {
                                Console.WriteLine("ERROR: PIN already removed.\n");
                            }
                            else
                            {
                                Console.WriteLine("ERROR: Incorrect PIN.\n");
                            }
                        }
                        else
                        {
                            Console.WriteLine("ERROR: Cannot read ICCID.\n");
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR Cannot select EF ICCID file.\n");
                    }
                    rfidReader.Disconnect(SCardReaderDisposition.Eject);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                ConsoleKeyInfo pressedKey = Console.ReadKey();
            }
        }

        private static ResponseApdu Transmit(SCardReader reader, CommandApdu command)
        {
            nint sendPci = SCardPCI.GetPci(reader.ActiveProtocol); // Protocol Control Information (PCI)
            SCardPCI receivePci = new();

            byte[] receiveBuffer = new byte[256];
            reader.Transmit(sendPci, command.ToArray(), receivePci, ref receiveBuffer);

            return new ResponseApdu(receiveBuffer, IsoCase.Case2Short, reader.ActiveProtocol);

        }
    }
}
