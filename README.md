# TailCall.Fody
This is a [Fody](https://github.com/Fody/Fody) add-in that is simply adding tailcall IL prefix.

## Install
(not released yet)

## Usage
Add <TailCall /> to your `FodyWeavers.xml`

```xml
<Weavers>
  <TailCall />
</Weavers>
```

then all functions called at tail will be prefixed.
