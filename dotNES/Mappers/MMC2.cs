namespace dotNES.Mappers
{
    [MapperDef(Id = 9, Description = "Mike Tyson's Punch-Out!!")]
    class MMC2 : MMC4
    {
        public MMC2(Emulator emulator) : base(emulator)
        {

        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            base.InitializeMemoryMap(cpu);

            cpu.MapReadHandler(0x8000, 0xBFFF, addr => _prgROM[_prgBankOffset + (addr - 0x8000)]);
            cpu.MapReadHandler(0xA000, 0xFFFF, addr => _prgROM[_prgROM.Length - 0x4000 - 0x2000 + (addr - 0xA000)]);

            cpu.MapWriteHandler(0xA000, 0xAFFF, (addr, val) => _prgBankOffset = (val & 0xF) * 0x2000);
        }

        protected override void GetLatch(uint addr, out uint latch, out bool? on)
        {
            base.GetLatch(addr, out latch, out on);

            // For MMC2, only 0xFD8 and 0xFE8 trigger the latch,
            // not the whole range like in MMC4
            if (latch == 0 && (addr & 0x3) != 0)
                on = null;
        }
    }
}
