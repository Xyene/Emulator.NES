namespace dotNES
{
    abstract class Memory : IAddressable
    {
        protected Emulator Emulator;

        public Memory(Emulator emulator)
        {
            this.Emulator = emulator;
        }

        public abstract byte ReadByte(int addr);

        public abstract void WriteByte(int addr, int val);
    }
}
