namespace MICE.Nintendo
{
    public struct NintendoStepArgs
    {
        public int CPUStepsOccurred { get; set; }
        public int PPUStepsOccurred { get; set; }
        public long TotalCPUSteps { get; set; }
        public long TotalPPUSteps { get; set; }
    }
}