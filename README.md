<p align="center">
  <a href="https://archnet-plateform.fr"><img src="https://archnet-plateform.fr/ArchNetLogoWithBck.png" alt="ArchNet" width="240"></a>
</p>

# ArchNet · Library Core

Base of the ArchNet libraries: a ScriptableObject that stores values by int or enum key, with one shared inspector. Derive from KeyedLibrary<T> to make your own library.

- Unity **6000.0** or newer · No dependency.
- Part of [ArchNet](https://archnet-plateform.fr), free and open-source Unity packages

## Install

*Window > Package Manager > + > Install package from git URL…*

```
https://github.com/ArchNet-1/LibraryCore.git
```

## Use

```csharp
using ArchNet.Library;
using UnityEngine;

[CreateAssetMenu(menuName = "MyGame/Font Library")]
public class FontLibrary : KeyedLibrary<Font> { }
```

In the asset's inspector, pick **Key Type** (Enum or Int), type the enum's full name if needed, then **Add** one value per key. Read it with `Get(key)` or `TryGet(key, out value)`.

## License

MIT, see [LICENSE](LICENSE).
