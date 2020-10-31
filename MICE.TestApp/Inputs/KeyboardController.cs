using MICE.Common.Misc;
using MICE.Components.Memory;
using MICE.Nintendo.Components;
using MICE.Nintendo.Handlers;
using MICE.Nintendo.Interfaces;
using SharpDX.DirectInput;
using System;
using System.Threading.Tasks;
using Range = MICE.Common.Misc.Range;

namespace MICE.TestApp
{
    public class KeyboardController : MemorySegment, INESInput
    {
        private int readCount;
        private Task keyboardPollTask;
        private readonly Keyboard keyboard;
        private readonly InputHandler inputHandler;

        public KeyboardController(InputHandler inputHandler, Range memoryRange, string name)
            : base(memoryRange, name)
        {
            var directInput = new DirectInput();
            this.keyboard = new Keyboard(directInput);

            this.inputHandler = inputHandler;

            this.keyboard.Properties.BufferSize = 128;
            this.keyboard.Acquire();

            this.keyboardPollTask = this.PollKeyboard();
        }

        public NESInputs Inputs { get; private set; }
        public bool IsReadingInputs { get; private set; }
        public bool IsStrobing { get; private set; }

        public override byte ReadByte(int index)
        {
            if (this.IsStrobing)
            {
                return BuildReturnValue(this.GetInput(NESInputs.A));
            }

            byte returnValue = 0x0;

            returnValue = (this.readCount++) switch
            {
                0 => BuildReturnValue(this.GetInput(NESInputs.A)),
                1 => BuildReturnValue(this.GetInput(NESInputs.B)),
                2 => BuildReturnValue(this.GetInput(NESInputs.Select)),
                3 => BuildReturnValue(this.GetInput(NESInputs.Start)),
                4 => BuildReturnValue(this.GetInput(NESInputs.Up)),
                5 => BuildReturnValue(this.GetInput(NESInputs.Down)),
                6 => BuildReturnValue(this.GetInput(NESInputs.Left)),
                7 => BuildReturnValue(this.GetInput(NESInputs.Right)),
                _ => BuildReturnValue(isPressed: true),
            };
            return returnValue;
        }

        private static byte BuildReturnValue(bool isPressed) => (byte)(0b01000000 | (isPressed ? 1 : 0));
        private bool GetInput(NESInputs input) => this.Inputs.HasFlag(input);
        public void InputsChanged(NESInputs inputs) => this.Inputs = inputs;

        public override void Write(int index, byte value)
        {
            this.IsStrobing = (value & 1) == 1;
            this.IsReadingInputs = !this.IsStrobing;
            this.readCount = 0;
        }

        private Task PollKeyboard() => Task.Factory.StartNew(() =>
        {
            while (true)
            {
                this.keyboard.Poll();
                var datas = keyboard.GetBufferedData();

                foreach (var state in datas)
                {
                    switch (state.Key)
                    {
                        case Key.Z:
                            this.SetInput(NESInputs.B, state.IsPressed);
                            break;
                        case Key.X:
                            this.SetInput(NESInputs.A, state.IsPressed);
                            break;
                        case Key.Tab:
                            this.SetInput(NESInputs.Select, state.IsPressed);
                            break;
                        case Key.Return:
                            this.SetInput(NESInputs.Start, state.IsPressed);
                            break;
                        case Key.Up:
                            this.SetInput(NESInputs.Up, state.IsPressed);
                            break;
                        case Key.Down:
                            this.SetInput(NESInputs.Down, state.IsPressed);
                            break;
                        case Key.Left:
                            this.SetInput(NESInputs.Left, state.IsPressed);
                            break;
                        case Key.Right:
                            this.SetInput(NESInputs.Right, state.IsPressed);
                            break;
                    }

                    this.inputHandler.InputChanged(this.Inputs);
                }
            }
        });

        private void SetInput(NESInputs input, bool wasPressed)
        {
            if (wasPressed)
            {
                this.Inputs |= input;
            }
            else
            {
                this.Inputs &= ~input;
            }
        }

        public override byte[] GetBytes() => throw new NotImplementedException();
        public override ushort ReadShort(int index) => throw new NotImplementedException();
        public override void Write(int index, ushort value) => throw new NotImplementedException();
        public override void CopyBytes(ushort startAddress, Span<byte> destination, int destinationIndex, int length) => throw new NotImplementedException();
    }
}
