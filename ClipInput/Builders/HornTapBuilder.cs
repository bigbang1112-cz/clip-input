using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using TmEssentials;

namespace ClipInput.Builders;

class HornTapBuilder : GenericKeyTapBuilder<Horn>
{
    private readonly ClipInputConfig config;

    public HornTapBuilder(IReadOnlyCollection<IInput> inputs, ClipInputConfig config) : base(inputs, config)
    {
        this.config = config;
    }

    protected override bool Enable => config.EnableHorn;

    protected override CGameCtnMediaBlock Initiate(TimeSingle time, bool pressed) => config.Design.InitiateHorn(time, pressed);
}
