namespace dotNES.Mappers
{
    abstract class AbstractMemory : IAddressable
    {
        protected Emulator Emulator;

        public AbstractMemory(Emulator emulator)
        {
            this.Emulator = emulator;
        }

        public abstract byte ReadByte(uint addr);

        public abstract void WriteByte(uint addr, uint val);
    }
}
