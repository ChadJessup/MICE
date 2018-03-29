using System;

namespace MICE.Nintendo
{
    public class NintendoStepArgs : EventArgs
    {
        public int CPUStepsOccurred { get; set; }
        public int PPUStepsOccurred { get; set; }
        public long TotalCPUSteps { get; set; }
        public long TotalPPUSteps { get; set; }
    }
}