namespace MICE.Common.Interfaces
{
    public interface ILoader : IMICEComponent
    {
        TCartridge Load<TCartridge>(string path) where TCartridge : ICartridge;
    }
}
