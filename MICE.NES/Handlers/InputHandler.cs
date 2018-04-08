using System;
using MICE.Common.Misc;
using MICE.Nintendo.Components;
using MICE.Nintendo.Interfaces;

namespace MICE.Nintendo.Handlers
{
    public class InputHandler
    {
        public INESInput Controller1 { get; private set; }
        public INESInput Controller2 { get; private set; }

        public INESInput GetController1(int min, int max, string name)
        {
            this.Controller1 = new KeyboardController(new Range(min, max), name);
            return this.Controller1;
        }

        public INESInput GetController2(int min, int max, string name)
        {
            this.Controller2 = new KeyboardController(new Range(min, max), name);
            return this.Controller2;
        }

        public void InputChanged(NESInputs inputs)
        {
            // TODO: Controller 2...
            this.Controller1.InputsChanged(inputs);
        }
    }
}
