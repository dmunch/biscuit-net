namespace biscuit_net.Parser;

public static class HexConvert
{
    public static byte[] FromHexString(String hexadecimalString)
    {
#if NET6_0_OR_GREATER
        return Convert.FromHexString(hexadecimalString);
#else
        int length = hexadecimalString.Length;
        byte[] byteArray = new byte[length / 2];
        for (int i = 0; i < length; i += 2){
            byteArray[i / 2] = Convert.ToByte(hexadecimalString.Substring(i, 2), 16);
        }
        return byteArray;
#endif
    }
}