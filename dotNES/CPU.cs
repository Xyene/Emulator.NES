using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    class CPU : IAddressable
    {
        public class CPUFlags
        {
            public bool Negative;
            public bool Overflow;
            public bool IRQ;
            public bool DecimalMode;
            public bool InterruptsDisabled;
            public bool Zero;
            public bool Carry;
        }

        private Emulator emulator;
        private byte[] ram = new byte[0x800];
        public byte A { get; private set; }
        public byte X { get; private set; }
        public byte Y { get; private set; }
        public byte S { get; private set; }
        public int PC { get; private set; }
        public long cycle { get; private set; }

        public readonly CPUFlags flags = new CPUFlags();

        /**
         * 7  bit  0
         * ---- ----
         * NVss DIZC
         * |||| ||||
         * |||| |||+- Carry: 1 if last addition or shift resulted in a carry, or if
         * |||| |||     last subtraction resulted in no borrow
         * |||| ||+-- Zero: 1 if last operation resulted in a 0 value
         * |||| |+--- Interrupt: Interrupt inhibit
         * |||| |       (0: /IRQ and /NMI get through; 1: only /NMI gets through)
         * |||| +---- Decimal: 1 to make ADC and SBC use binary-coded decimal arithmetic
         * ||||         (ignored on second-source 6502 like that in the NES)
         * ||++------ s: No effect, used by the stack copy, see note below
         * |+-------- Overflow: 1 if last ADC or SBC resulted in signed overflow,
         * |            or D6 from last BIT
         * +--------- Negative: Set to bit 7 of the last operation
         */
        public byte P
        {
            get
            {
                return (byte)((Convert.ToByte(flags.Carry) << 0) |
                                (Convert.ToByte(flags.Zero) << 1) |
                                (Convert.ToByte(flags.InterruptsDisabled) << 2) |
                                (Convert.ToByte(flags.DecimalMode) << 3) |
                                (Convert.ToByte(flags.IRQ) << 4) |
                                (1 << 5) |
                                (Convert.ToByte(flags.Overflow) << 6) |
                                (Convert.ToByte(flags.Negative) << 7));
            }
            set
            {
                flags.Carry = (value & 0x1) > 0;
                flags.Zero = (value & 0x2) > 0;
                flags.InterruptsDisabled = (value & 0x4) > 0;
                flags.DecimalMode = (value & 0x8) > 0;
                flags.IRQ = (value & 0x10) > 0;
                flags.Overflow = (value & 0x40) > 0;
                flags.Negative = (value & 0x80) > 0;
            }
        }

        public CPU(Emulator emulator)
        {
            this.emulator = emulator;
            Initialize();
        }

        public void Initialize()
        {
            A = 0;
            X = 0;
            Y = 0;
            S = 0xFD;
            P = 0x24;

            PC = 0xC000;
        }

        public void Reset()
        {
            S -= 3;
            flags.IRQ = true;
        }

        public void Execute()
        {
            for (int i = 0; i < 5; i++)
                _Execute();
        }

        private byte NextByte()
        {
            return emulator.Mapper.ReadAddress((ushort)PC++);
        }

        public void _Execute()
        {
            int instruction = NextByte();
            Console.WriteLine($"{(PC - 1).ToString("X")}\t{instruction.ToString("X")}\t\t\t\tA:{A.ToString("X")} X:{X.ToString("X")} Y:{Y.ToString("X")} P:{P.ToString("X")} SP:{S.ToString("X")} CYC:\t{cycle}");

            switch (instruction)
            {
                case 0x4C: // JMP
                    PC = NextByte() | (NextByte() << 8);
                    break;
                case 0xA2: // LDX
                    X = NextByte();
                    break;
                case 0x86: // STX
                    WriteAddress(NextByte(), X);
                    break;
                default:
                    throw new ArgumentException(instruction.ToString("X"));
            }
        }

        public byte ReadAddress(ushort addr)
        {
            /*
             * Address range 	Size 	Device
             * $0000-$07FF 	    $0800 	2KB internal RAM
             * $0800-$0FFF 	    $0800 	|
             * $1000-$17FF 	    $0800   | Mirrors of $0000-$07FF
             * $1800-$1FFF 	    $0800   |
             * $2000-$2007 	    $0008 	NES PPU registers
             * $2008-$3FFF 	    $1FF8 	Mirrors of $2000-2007 (repeats every 8 bytes)
             * $4000-$4017 	    $0018 	NES APU and I/O registers
             * $4018-$401F 	    $0008 	APU and I/O functionality that is normally disabled. See CPU Test Mode.
             * $4020-$FFFF 	    $BFE0 	Cartridge space: PRG ROM, PRG RAM, and mapper registers (See Note)
             * 
             * https://wiki.nesdev.com/w/index.php/CPU_memory_map
             */
            switch (addr & 0xF000)
            {
                case 0x0000:
                case 0x1000:
                    // Wrap every 7FFh bytes
                    return ram[addr & 0x07FF];
                case 0x2000:
                case 0x3000:
                    // Wrap every 7h bytes
                    int reg = (addr & 0x7) - 0x2000;
                    return emulator.PPU.ReadRegister(reg);
                case 0x4000:
                    if (addr <= 0x401F)
                    {
                        reg = addr - 0x4000;
                        return ReadRegister(reg);
                    }
                    goto default;
                default:
                    return emulator.Mapper.ReadAddress(addr);
            }

            throw new ArgumentOutOfRangeException();
        }

        public void WriteAddress(ushort addr, byte val)
        {
            switch (addr & 0xF000)
            {
                case 0x0000:
                case 0x1000:
                    // Wrap every 7FFh bytes
                    ram[addr & 0x07FF] = val;
                    return;
                case 0x2000:
                case 0x3000:
                    // Wrap every 7h bytes
                    int reg = (addr & 0x7) - 0x2000;
                    emulator.PPU.WriteRegister(reg, val);
                    return;
                case 0x4000:
                    if (addr <= 0x401F)
                    {
                        reg = addr - 0x4000;
                        WriteRegister(reg, val);
                        return;
                    }
                    goto default;
                default:
                    emulator.Mapper.WriteAddress(addr, val);
                    return;
            }
        }

        public void WriteRegister(int reg, byte val)
        {
            throw new NotImplementedException();
        }

        public byte ReadRegister(int reg)
        {
            throw new NotImplementedException();
        }
    }
}
