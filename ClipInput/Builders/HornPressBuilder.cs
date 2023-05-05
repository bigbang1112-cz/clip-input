using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class HornPressBuilder : GenericKeyBuilder<Horn>
{
    private readonly ClipInputConfig config;

    public HornPressBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.config = config;
    }

    protected override bool Enable => config.EnableHorn;

    protected override CGameCtnMediaBlock Initiate(TimeSingle time, bool pressed) => config.Design.InitiateHorn(time, pressed);
}
