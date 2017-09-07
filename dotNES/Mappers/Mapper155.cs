namespace dotNES.Mappers
{
    [MapperDef(Id = 155, Description = "MMC1A")]
    class Mapper155 : MMC1
    {
        // Mapper for games requiring MMC1A
        public Mapper155(Emulator emulator) : base(emulator, ChipType.MMC1A)
        {
        }
    }
}
