# TailCall.Fody
This is a [Fody](https://github.com/Fody/Fody) add-in that is simply adding tailcall IL prefix.

## Install
Available on [NuGet](https://www.nuget.org/packages/TailCall.Fody)

Insert below section into your csproj:

```csproj
<ItemGroup>
  <PackageReference Include="Fody" Version="6.0.0" PrivateAssets="All" />
  <PackageReference Include="TailCall.Fody" Version="1.0.3" PrivateAssets="All" />
</ItemGroup>
```

## Usage
Add <TailCall /> to your `FodyWeavers.xml`

```xml
<Weavers>
  <TailCall />
</Weavers>
```

then all functions called at tail will be prefixed.

## Limitations
* Does not treat ValueType instance method calls
* Does not treat generic type instance's method calls without constraints `where T : class`
* Does not treat a method that has any byreference parameters

## License
MIT
