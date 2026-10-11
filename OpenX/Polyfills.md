OpenX polyfills: where things live in each language
===

OpenX exists in C# (`dotnet/`, the reference), Python (`python/`) and Rust (`rust/`). Every type and member exists in all three, with C#'s names (camelCase members in Python and Rust); only where each is filed differs. Much of C#'s `OpenX.Poly` polyfills .NET libraries, so it's filed by .NET namespace. Python and Rust use their own libraries for the same jobs (`numpy`, `glam`, `half`, `std::io`) and file the rest by what it is. In Rust, code that extends another crate's types goes in a module named for that crate, as extension traits.

To find a type's counterpart in another language, look up its row. When something moves, update this table. (GameX ADR 0001 and 0002 record the reasoning.)

| C# `OpenX.Poly` | Python `openx.core.poly` | Rust `openx-poly` |
| -- | -- | -- |
| `System.Numerics/` `Vector2/3/4`, `Quaternion`, `Matrix4x4` (.NET types) | `numpy` `ndarray`, `quaternion` | `glam` types directly (`Vec3`, `Quat`, `Mat4`) |
| `System.Numerics/` extension methods (`IsZero`, `ConvertToTransformationMatrix`, ...) | | extension traits on glam types in `openx_poly::glam` |
| `System.Numerics/Matrix2x2`, `Matrix3x3`, `Matrix3x4`, `Matrix4x3` | | glam `Mat2`/`Mat3`; shapes glam lacks (3x4, 4x3) are structs in `openx_poly::glam` |
| `System.Numerics/Polyfill.cs` `Byte2`, `Int2`, `Byte3`, `Int3`, `Float2/3/4` | `poly.py` | `openx_poly::poly`: C#-named aliases of glam types (`Int3 = glam::IVec3`, `Float3 = glam::Vec3`, `Byte3 = glam::U8Vec3`) |
| `System.IO/` reader, writer and stream polyfills | `reader.py`, `writer.py`; streams in `openx.core.stream` | `openx_poly::reader` / `writer`: extension traits on `std::io::Read`/`Write`/`Seek` |
| `Poly2.1/` (`Half`, `HalfFloat`, `BitConverterX`: netstandard2.1 gaps) | `numpy.float16` (`reader.readHalf` not ported yet) | `half::f16` used directly, no alias or file (stable Rust has no `f16` yet); `BitConverterX` is std `from_le_bytes` plus `f16::from_bits` |
| `System/` (`MathX`, `ConvertX`, `UnsafeX`, `ThreadSafeRandom`, ...) | `system.py`, `unsafe.py` | `openx_poly::system` |
| `System.Collections.Generic/` (pools, coroutine queues, `KV`) | `pool.py` | `openx_poly::pool` |
| `System.Drawing/` (`Color`, `BoundingBox`, `Ray`, ...) | `openx.sys.drawing` | `openx_poly::drawing` |
| `Log.cs` | `log.py` | `openx_poly::log` |
| `ISource.cs` | `openx.core.core` | `openx_poly::core` |
| `System.Text/` `ValueStringBuilder`, `System.Security.Cryptography/`, `MathNet.Numerics/` | | Rust's `String`, RustCrypto crates, when something needs them |
