using MICE.Nintendo.Components;
using MICE.Nintendo.Interfaces;
using System;

namespace MICE.Nintendo.Handlers
{
    public class InputHandler
    {
        public INESInput Controller1 { get; private set; }
        public INESInput Controller2 { get; private set; }

        public INESInput GetController1() => this.Controller1;
        public INESInput GetController2() => this.Controller2;

        public void SetController1(INESInput input) => this.SetController(input, ControllerId.Controller1);
        public void SetController2(INESInput input) => this.SetController(input, ControllerId.Controller2);

        public void SetController(INESInput input, ControllerId controllerId)
        {
            ControllerChangedArgs args;

            switch (controllerId)
            {
                case ControllerId.Controller1:
                    this.Controller1 = input;
                    break;
                case ControllerId.Controller2:
                    this.Controller2 = input;
                    break;
            }

            args = input == null
                ? new ControllerChangedArgs(null, wasInserted: false, controllerId: controllerId)
                : new ControllerChangedArgs(input, wasInserted: true, controllerId: controllerId);

            this.ControllerChanged?.Invoke(this, args);
        }

        public EventHandler<ControllerChangedArgs> ControllerChanged;

        public void InputChanged(NESInputs inputs)
        {
            // TODO: Controller 2...
            this.Controller1.InputsChanged(inputs);
        }
    }

    public class ControllerChangedArgs
    {
        public ControllerChangedArgs(INESInput controller, bool wasInserted, ControllerId controllerId)
        {
            this.Controller = controller;
            this.WasInserted = wasInserted;
            this.ControllerId = controllerId;
        }

        public INESInput Controller { get; }
        public ControllerId ControllerId { get; }
        public bool WasInserted { get; }
    }

    [Flags]
    public enum ControllerId
    {
        Unknown     = 1 << 0,
        Controller1 = 1 << 1,
        Controller2 = 1 << 2,
    }
}
