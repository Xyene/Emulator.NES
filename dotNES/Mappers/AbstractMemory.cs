namespace dotNES.Mappers
{
    abstract class AbstractMemory : IAddressable
    {
        protected readonly Emulator _emulator;
        protected readonly byte[] _PRGROM;
        protected readonly int _lastBankOffset;

        public AbstractMemory(Emulator emulator)
        {
            _emulator = emulator;
            var cart = emulator.Cartridge;
            _PRGROM = cart.PRGROM;
            _lastBankOffset = cart.PRGROM.Length - 0x4000;
        }

        public abstract uint ReadByte(uint addr);

        public abstract void WriteByte(uint addr, uint val);
    }
}
