using PCSC;
using PCSC.Iso7816;

namespace aprt
{
    internal class ADPUBuilder
    {

        public static CommandApdu SelectMF(SCardProtocol protocol)
        {
            return new CommandApdu(IsoCase.Case4Short, protocol)
            {
                CLA = 0xA0,
                Instruction = InstructionCode.SelectFile,
                P1 = 0x00,
                P2 = 0x00,
                Data = new byte[] { 0x3F, 0x00 }
            };
        }
        public static CommandApdu SelectICCID(SCardProtocol protocol)
        {
            return new CommandApdu(IsoCase.Case4Short, protocol)
            {
                CLA = 0xA0,
                Instruction = InstructionCode.SelectFile,
                P1 = 0x00,
                P2 = 0x00,
                Data = new byte[] { 0x2F, 0xE2 } // DF Telecom, EF ICCID
            };
        }
        public static CommandApdu ReadICCID(SCardProtocol protocol)
        {
            return new CommandApdu(IsoCase.Case2Short, protocol)
            {
                CLA = 0xA0,
                Instruction = InstructionCode.ReadBinary,
                P1 = 0x00,
                P2 = 0x00,
                Le = 0x0A // ICCID are 10 bytes
            };
        }

        public static CommandApdu DisablePIN(SCardProtocol protocol, string pin)
        {
            byte[] pinBytes = System.Text.Encoding.ASCII.GetBytes(pin);
            const byte disableCommand = 0x26;

            return new CommandApdu(IsoCase.Case3Short, protocol)
            {
                CLA = 0xA0, // Class
                INS = disableCommand,
                P1 = 0x00, // Parameter 1
                P2 = 0x01, // Parameter 2 (PIN1)
                Data = pinBytes.Concat(new byte[8 - pinBytes.Length].Select(b => (byte)0xFF)).ToArray()
            };
        }

        public static CommandApdu EnablePIN(SCardProtocol protocol, string pin)
        {
            byte[] pinBytes = System.Text.Encoding.ASCII.GetBytes(pin);
            const byte enableCommand = 0x28;

            return new CommandApdu(IsoCase.Case3Short, protocol)
            {
                CLA = 0xA0, // Class
                INS = enableCommand,
                P1 = 0x00, // Parameter 1
                P2 = 0x01, // Parameter 2 (PIN1)
                Data = pinBytes.Concat(new byte[8 - pinBytes.Length].Select(b => (byte)0xFF)).ToArray()
            };
        }

    }
}
