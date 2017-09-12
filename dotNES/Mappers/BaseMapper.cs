using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace dotNES.Mappers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MapperDef : Attribute
    {
        public int Id;
        public string Name;
        public string Description;

        public MapperDef()
        {

        }

        public MapperDef(int id)
        {
            Id = id;
        }
    }

    abstract class BaseMapper
    {
        protected readonly Emulator _emulator;
        protected readonly byte[] _prgROM;
        protected readonly byte[] _prgRAM = new byte[0x2000];
        protected readonly byte[] _chrROM;
        protected readonly uint _lastBankOffset;

        public BaseMapper(Emulator emulator)
        {
            _emulator = emulator;
            var cart = emulator.Cartridge;
            _prgROM = cart.PRGROM;
            _chrROM = cart.CHRROM;
            _lastBankOffset = (uint) _prgROM.Length - 0x4000;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual uint ReadBytePPU(uint addr)
        {
            if (addr < 0x2000)
            {
                return _chrROM[addr];
            }
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void WriteBytePPU(uint addr, uint val)
        {
            if (addr < 0x2000)
            {
                _chrROM[addr] = (byte) val;
            }
            else throw new NotImplementedException();
        }

        public virtual void InitializeMaps(CPU cpu)
        {

        }

        public virtual void ProcessCycle(int scanline, int cycle)
        {

        }

        public virtual void Save(Stream os)
        {
            os.Write(_prgRAM, 0, _prgRAM.Length);
        }

        public virtual void Load(Stream os)
        {
            using (BinaryReader binaryReader = new BinaryReader(os))
            {
                byte[] ram = binaryReader.ReadBytes((int)os.Length);
                Array.Copy(ram, _prgRAM, ram.Length);
            }
        }
    }
}