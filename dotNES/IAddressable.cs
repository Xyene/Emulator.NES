namespace dotNES
{
    interface IAddressable
    {
        byte ReadByte(int addr);

        void WriteByte(int addr, int val);
    }
}
