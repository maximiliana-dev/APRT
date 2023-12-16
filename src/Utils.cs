namespace aprt
{
    internal class Utils
    {
        public static byte[] ReverseEndianness(byte[] data)
        {
            byte[] swapped = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                swapped[i] = SwapDigits(data[i]);
            }

            return swapped;
        }
        private static byte SwapDigits(byte b)
        {
            int high = b >> 4;
            int low = b & 0x0F;
            return (byte)((low << 4) | high);
        }
    }
}
