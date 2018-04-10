using MICE.Common.Misc;
using MICE.Components.Memory;
using MICE.Nintendo.Components;
using MICE.Nintendo.Handlers;
using MICE.Nintendo.Interfaces;
using SharpDX.DirectInput;
using System;
using System.Threading.Tasks;

namespace MICE.TestApp
{
    public class KeyboardController : MemorySegment, INESInput
    {
        private int readCount = 0;
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
            byte returnValue = 0x0;

            if (this.IsStrobing)
            {
                returnValue = this.BuildReturnValue(this.GetInput(NESInputs.A));

                return returnValue;
            }

            switch (this.readCount++)
            {
                case 0:
                    returnValue = this.BuildReturnValue(this.GetInput(NESInputs.A));
                    break;
                case 1:
                    returnValue = this.BuildReturnValue(this.GetInput(NESInputs.B));
                    break;
                case 2:
                    returnValue = this.BuildReturnValue(this.GetInput(NESInputs.Select));
                    break;
                case 3:
                    returnValue = this.BuildReturnValue(this.GetInput(NESInputs.Start));
                    break;
                case 4:
                    returnValue = this.BuildReturnValue(this.GetInput(NESInputs.Up));
                    break;
                case 5:
                    returnValue = this.BuildReturnValue(this.GetInput(NESInputs.Down));
                    break;
                case 6:
                    returnValue = this.BuildReturnValue(this.GetInput(NESInputs.Left));
                    break;
                case 7:
                    returnValue = this.BuildReturnValue(this.GetInput(NESInputs.Right));
                    break;
                default:
                    returnValue = this.BuildReturnValue(isPressed: true);
                    break;
            }

            return returnValue;
        }

        private byte BuildReturnValue(bool isPressed) => (byte)(0b01000000 | (isPressed ? 1 : 0));
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

                    Console.WriteLine(state);
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
