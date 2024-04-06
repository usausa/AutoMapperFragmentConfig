namespace AutoMapperFragmentConfig;

using AutoMapper;

using Microsoft.Extensions.DependencyInjection;

public class ToStringTest
{
    [Fact]
    public void Test1()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICalc, AddCalc>();
        services.AddSingleton<IMapper>(static p => new Mapper(new MapperConfiguration(c => c.AddFragmentProfile(p))));

        var provider = services.BuildServiceProvider();

        var mapper = provider.GetRequiredService<IMapper>();

        var destination = mapper.Map<Source, Destination>(new Source { Value = 1 });

        Assert.Equal("2", destination.Value);
    }
}

public static partial class Extensions
{
    [MapExtension]
    public static partial void AddFragmentProfile(this IMapperConfigurationExpression expression, IServiceProvider provider);
}

public sealed class Controller
{
    [MapConfig]
    public static void ConfigureMapping(IProfileExpression config, IServiceProvider provider)
    {
        var calc = provider.GetRequiredService<ICalc>();
        config.CreateMap<Source, Destination>()
            .ForMember(d => d.Value, o => o.MapFrom(s => calc.Calc(s.Value)));
    }
}

public class Source
{
    public int Value { get; set; }
}

public class Destination
{
    public string Value { get; set; } = default!;
}

public interface ICalc
{
    int Calc(int value);
}

public class AddCalc : ICalc
{
    public int Calc(int value) => value + 1;
}
