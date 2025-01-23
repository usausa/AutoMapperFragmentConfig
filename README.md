# AutoMapperFragmentConfig

[![NuGet](https://img.shields.io/nuget/v/AutoMapperFragmentConfig.svg)](https://www.nuget.org/packages/AutoMapperFragmentConfig)

## What is this?

Configure AutoMapper locally.

## Usage

```xml
  <ItemGroup>
    <PackageReference Include="AutoMapperFragmentConfig" Version="1.0.0" />
  </ItemGroup>
```

## ASP.NET Core Sample

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class ExampleController : ControllerBase
{
    private readonly IMapper mapper;

    // AutoMapper settings used only in the Controller can be configured locally without creating Profile class.
    [MapConfig]
    public static void ConfigureMapping(IProfileExpression config, IServiceProvider provider)
    {
        var calc = provider.GetRequiredService<ICalc>();
        config.CreateMap<Request, Response>()
            .ForMember(d => d.Value, o => o.MapFrom(s => calc.Calc(s.Value)));
    }

    public ExampleController(IMapper mapper)
    {
        this.mapper = mapper;
    }

    [HttpPost]
    public IActionResult Execute([FromBody] Request request)
    {
        return Ok(mapper.Map<Request, Response>(request));
    }
}
```

```csharp
public static partial class AutoMapperExtensions
{
    // Extension method generator for fragment config.
    [MapConfigExtension]
    public static partial void AddFragmentProfile(this IMapperConfigurationExpression expression, IServiceProvider provider);
}
```

```csharp
builder.Services.AddSingleton<ICalc, IncrementCalc>();

// Add AutoMapper with fragment config.
builder.Services.AddSingleton<IMapper>(static p => new Mapper(new MapperConfiguration(c => c.AddFragmentProfile(p)), p.GetService));
```

```csharp
public class Request
{
    public int Value { get; set; }
}

public class Response
{
    public string Value { get; set; } = default!;
}

public interface ICalc
{
    int Calc(int value);
}

public class IncrementCalc : ICalc
{
    public int Calc(int value) => value + 1;
}
```
