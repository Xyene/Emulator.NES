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
        public byte SP { get; private set; }
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
            SP = 0xFD;
            P = 0x24;

            PC = 0xC000;
        }

        public void Reset()
        {
            SP -= 3;
            flags.IRQ = true;
        }

        private byte NextByte()
        {
            return emulator.Mapper.ReadAddress((ushort)PC++);
        }

        private void Push(byte what)
        {
            SP--;
            WriteAddress(SP, what);
        }

        private byte Pop()
        {
            byte val = ReadAddress(SP);
            SP++;
            return val;
        }

        private void PushWord(int what)
        {
            SP -= 2;

            WriteAddress(SP, (byte)(what & 0x00FF));
            WriteAddress((ushort)(SP + 1), (byte)((what & 0xFF00) >> 8));
        }

        private int PopWord()
        {
            int val = ReadAddress(SP) | ((ReadAddress((ushort)(SP + 1)) << 8));
            SP += 2;
            return val;
        }

        public void Execute()
        {
            for (int i = 0; i < 300; i++)
                _Execute();
        }

        public void _Execute()
        {
            int instruction = NextByte();
            Console.WriteLine($"{(PC - 1).ToString("X4")}\t{instruction.ToString("X2")}\t\t\t\tA:{A.ToString("X2")} X:{X.ToString("X2")} Y:{Y.ToString("X2")} P:{P.ToString("X2")} SP:{SP.ToString("X2")} CYC:\t{cycle}");

            switch (instruction)
            {
                case 0x4C: // JMP
                    PC = NextByte() | (NextByte() << 8);
                    break;
                case 0xA9: // LDA
                    A = NextByte();
                    flags.Zero = A == 0;
                    flags.Negative = (A & 0x80) > 0;
                    break;
                case 0xA0: // LDY
                    Y = NextByte();
                    flags.Zero = Y == 0;
                    flags.Negative = (Y & 0x80) > 0;
                    break;
                case 0xA2: // LDX
                    X = NextByte();
                    flags.Zero = X == 0;
                    flags.Negative = (X & 0x80) > 0;
                    break;
                case 0x86: // STX
                    WriteAddress(NextByte(), X);
                    break;
                case 0x84: // STY
                    WriteAddress(NextByte(), Y);
                    break;
                case 0x85: // STA
                    WriteAddress(NextByte(), A);
                    break;
                case 0x20: // JSR
                    int nPC = NextByte() | (NextByte() << 8);
                    PushWord(PC);
                    PC = nPC;
                    break;
                case 0x60: // RTS
                    PC = PopWord();
                    break;
                case 0xEA: // NOP
                    break;
                case 0x18: // CLC
                    flags.Carry = false;
                    break;
                case 0x38: // SEC
                    flags.Carry = true;
                    break;
                case 0x58: // CLI
                    flags.InterruptsDisabled = false;
                    break;
                case 0x78: // SEI
                    flags.InterruptsDisabled = true;
                    break;
                case 0xB8: // CLV
                    flags.Overflow = false;
                    break;
                case 0xD8: // CLD
                    flags.DecimalMode = false;
                    break;
                case 0xF8: // SED
                    flags.DecimalMode = true;
                    break;
                case 0xB0: // BCS
                    nPC = PC + NextByte() + 1;
                    if (flags.Carry)
                        PC = nPC;
                    break;
                case 0x90: // BCC
                    nPC = PC + NextByte() + 1;
                    if (!flags.Carry)
                        PC = nPC;
                    break;
                case 0xF0: // BEQ
                    nPC = PC + NextByte() + 1;
                    if (flags.Zero)
                        PC = nPC;
                    break;
                case 0xD0: // BNE
                    nPC = PC + NextByte() + 1;
                    if (!flags.Zero)
                        PC = nPC;
                    break;
                case 0x70: // BVS
                    nPC = PC + NextByte() + 1;
                    if (flags.Overflow)
                        PC = nPC;
                    break;
                case 0x50: // BVC
                    nPC = PC + NextByte() + 1;
                    if (!flags.Overflow)
                        PC = nPC;
                    break;
                case 0x10: // BPL
                    nPC = PC + NextByte() + 1;
                    if (!flags.Negative)
                        PC = nPC;
                    break;
                case 0x30: // BMI
                    nPC = PC + NextByte() + 1;
                    if (flags.Negative)
                        PC = nPC;
                    break;
                case 0x24: // BIT
                    // BIT sets the Z flag as though the value in the address tested were ANDed with the accumulator.
                    // The S and V flags are set to match bits 7 and 6 respectively in the value stored at the tested address.
                    byte val = ReadAddress(NextByte());
                    flags.Zero = (val & A) == 0;
                    flags.Negative = (val & 0x80) > 0;
                    flags.Overflow = (val & 0x40) > 0;
                    break;
                case 0x08: // PHP
                    bool irq = flags.IRQ;
                    flags.IRQ = true;
                    Push(P);
                    flags.IRQ = irq;
                    break;
                case 0x48: // PHA
                    Push(A);
                    break;
                case 0x28: // PLP
                    P = Pop();
                    flags.IRQ = false;
                    break;
                case 0x68: // PLA
                    A = Pop();
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x29: // AND
                    A = (byte)(A & NextByte());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x09: // OR
                    A = (byte)(A | NextByte());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x49: // EOR
                    A = (byte)(A ^ NextByte());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x69: // ADC
                    val = NextByte();
                    int nA = (sbyte)A + (sbyte)val + (sbyte)(flags.Carry ? 1 : 0);
                    flags.Overflow = nA < -128 || nA > 127;
                    flags.Carry = (A + val + (flags.Carry ? 1 : 0)) > 0xFF;
                    flags.Negative = (nA & 0x80) > 0;
                    flags.Zero = (nA & 0xFF) == 0;
                    A = (byte) (nA & 0xFF);
                    break;
                case 0xC9: // CMP
                    byte M = NextByte();
                    flags.Zero = A == M;
                    flags.Negative = A < M;
                    flags.Carry = A >= M;
                    break;
                default:
                    throw new ArgumentException(instruction.ToString("X2"));
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
            // Console.WriteLine($"Write to {addr.ToString("X")} = {val}");
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
