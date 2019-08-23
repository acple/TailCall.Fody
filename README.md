# TailCall.Fody
This is a [Fody](https://github.com/Fody/Fody) add-in that is simply adding tailcall IL prefix.

## Install
Available on [NuGet](https://www.nuget.org/packages/TailCall.Fody)

Insert below section into your csproj:

```csproj
<ItemGroup>
  <PackageReference Include="Fody" Version="6.0.0" PrivateAssets="All" />
  <PackageReference Include="TailCall.Fody" Version="1.0.0" PrivateAssets="All" />
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

## License
MIT
