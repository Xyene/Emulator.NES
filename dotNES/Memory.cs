namespace dotNES
{
    abstract class Memory
    {
        protected Emulator Emulator;

        public Memory(Emulator emulator)
        {
            this.Emulator = emulator;
        }

        public abstract byte ReadByte(uint addr);

        public abstract void WriteByte(uint addr, uint val);
    }
}
