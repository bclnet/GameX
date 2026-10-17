// PORT-SOURCE: Core/OpenX.PolyIO/TypeX.cs
// PORT-SHA: 74ce0c87f971da34
// PORT-STATUS: done
//
// Moved out of the parked tree (`OpenX/rust/_/core/polyio/type_x.rs`). The
// TODO named `OpenX/rust/core/poly/_/`; the type lived in the older `_` mirror,
// not in that folder. Only `TypeRegistry` came out. `GetAllProperties`,
// `GetAllFields` and `GetDefaultConstructor` have no call sites and stay parked.
//
// C# `TypeX` scans assemblies for `[RType]`. Nothing in the solution calls it.
// GameX's real `Type.GetType` is the spec type name (`archiveType`, `vfxType`,
// `clientType`, `explorerAppType`). `registerType` / `findType` store those
// factories on this registry.
//
// `register` rejects a duplicate r-type or l-type name. C# `ToDictionary`
// throws for r-types and keeps the first l-type. Both are errors here.
// `registerValue` overwrites, which is what GameX's map did.

use std::any::{Any, TypeId};
use std::collections::HashMap;
use std::sync::{Arc, OnceLock, RwLock};

/// Builds a fresh instance. C# `Activator.CreateInstance` via the parameterless constructor.
pub type Factory = fn() -> Box<dyn Any + Send + Sync>;

/// One registered type. C# derived this from `[RType]`.
#[derive(Clone, Copy)]
pub struct TypeRegistration {
    /// C# `RTypeAttribute.Name`.
    pub name: &'static str,
    /// C# l-type namespace. `None` is the flat r-type table.
    pub namespace: Option<&'static str>,
    pub typeId: fn() -> TypeId,
    pub rustName: &'static str,
    pub factory: Factory,
}

impl std::fmt::Debug for TypeRegistration {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("TypeRegistration").field("name", &self.name).field("namespace", &self.namespace).field("rustName", &self.rustName).finish()
    }
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum RegistryError {
    Duplicate { name: String, existing: &'static str, incoming: &'static str },
    NotFound { name: String },
    WrongType { name: String, requested: &'static str, actual: &'static str },
}

impl std::fmt::Display for RegistryError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            RegistryError::Duplicate { name, existing, incoming } => write!(f, "type name '{name}' registered twice: {existing} and {incoming}"),
            RegistryError::NotFound { name } => write!(f, "no type registered as '{name}'"),
            RegistryError::WrongType { name, requested, actual } => write!(f, "'{name}' is {actual}, not {requested}"),
        }
    }
}

impl std::error::Error for RegistryError {}

/// C# `TypeX` + the GameX type-name map.
#[derive(Default)]
pub struct TypeRegistry {
    /// C# `AssemblyTag.RTypes`.
    rTypes: HashMap<&'static str, TypeRegistration>,
    /// C# `AssemblyTag.LTypes`, namespace -> name -> type.
    lTypes: HashMap<&'static str, HashMap<&'static str, TypeRegistration>>,
    /// C# `AssemblyRedirects`.
    redirects: HashMap<String, String>,
    /// GameX `registerType` / `findType`: a factory stored under the C# type name.
    values: HashMap<String, Arc<dyn Any + Send + Sync>>,
}

impl TypeRegistry {
    pub fn new() -> Self { Self::default() }

    /// C# `ScanTypes`, one type at a time.
    pub fn register(&mut self, reg: TypeRegistration) -> Result<(), RegistryError> {
        let table = match reg.namespace {
            Some(ns) => self.lTypes.entry(ns).or_default(),
            None => &mut self.rTypes,
        };
        if let Some(existing) = table.get(reg.name) {
            return Err(RegistryError::Duplicate { name: reg.name.to_string(), existing: existing.rustName, incoming: reg.rustName });
        }
        table.insert(reg.name, reg);
        Ok(())
    }

    pub fn registerAll(&mut self, regs: impl IntoIterator<Item = TypeRegistration>) -> Result<(), RegistryError> {
        for r in regs { self.register(r)?; }
        Ok(())
    }

    /// C# `AssemblyRedirects`.
    pub fn redirect(&mut self, from: impl Into<String>, to: impl Into<String>) {
        self.redirects.insert(from.into(), to.into());
    }

    /// C# `GetRType`. A bare name, or `namespace.name`. Redirects apply first.
    pub fn resolve(&self, name: &str) -> Option<&TypeRegistration> {
        let name = self.redirects.get(name).map(String::as_str).unwrap_or(name);
        if let Some(r) = self.rTypes.get(name) { return Some(r); }
        let (ns, base) = name.rsplit_once('.')?;
        self.lTypes.get(ns)?.get(base)
    }

    /// C# `Activator.CreateInstance(GetRType(name))`.
    pub fn create(&self, name: &str) -> Result<Box<dyn Any + Send + Sync>, RegistryError> {
        let reg = self.resolve(name).ok_or_else(|| RegistryError::NotFound { name: name.to_string() })?;
        Ok((reg.factory)())
    }

    pub fn createAs<T: Any>(&self, name: &str) -> Result<Box<T>, RegistryError> {
        let reg = self.resolve(name).ok_or_else(|| RegistryError::NotFound { name: name.to_string() })?;
        if (reg.typeId)() != TypeId::of::<T>() {
            return Err(RegistryError::WrongType { name: name.to_string(), requested: std::any::type_name::<T>(), actual: reg.rustName });
        }
        (reg.factory)().downcast::<T>().map_err(|_| RegistryError::WrongType { name: name.to_string(), requested: std::any::type_name::<T>(), actual: reg.rustName })
    }

    /// GameX `registerType`. A later registration of the same name replaces the earlier one.
    pub fn registerValue<T: Any + Send + Sync>(&mut self, name: &str, value: T) {
        self.values.insert(name.to_string(), Arc::new(value));
    }

    /// GameX `findType`.
    pub fn findValue<T: Any + Clone>(&self, name: &str) -> Option<T> {
        self.values.get(name).and_then(|s| s.downcast_ref::<T>()).cloned()
    }

    pub fn names(&self) -> Vec<String> {
        let mut out: Vec<String> = self.rTypes.keys().map(|s| (*s).to_string()).collect();
        for (ns, t) in &self.lTypes { out.extend(t.keys().map(|n| format!("{ns}.{n}"))); }
        out.sort();
        out
    }

    pub fn len(&self) -> usize {
        self.rTypes.len() + self.lTypes.values().map(HashMap::len).sum::<usize>()
    }

    pub fn isEmpty(&self) -> bool { self.len() == 0 }
}

/// Process-wide registry. GameX `registerType` / `findType` use this.
static GLOBAL: OnceLock<RwLock<TypeRegistry>> = OnceLock::new();

pub fn global() -> &'static RwLock<TypeRegistry> {
    GLOBAL.get_or_init(|| RwLock::new(TypeRegistry::new()))
}

/// GameX `registerType(name, factory)`.
pub fn registerType<T: Any + Send + Sync>(name: &str, value: T) {
    global().write().unwrap_or_else(|p| p.into_inner()).registerValue(name, value);
}

/// GameX `findType(name)`.
pub fn findType<T: Any + Clone>(name: &str) -> Option<T> {
    global().read().unwrap_or_else(|p| p.into_inner()).findValue(name)
}

/// Declares a registration next to the type, the way `[RType]` did.
#[macro_export]
macro_rules! register_type {
    ($ty:ty, $name:expr) => {
        $crate::type_x::TypeRegistration {
            name: $name,
            namespace: None,
            typeId: || ::std::any::TypeId::of::<$ty>(),
            rustName: ::std::stringify!($ty),
            factory: || ::std::boxed::Box::new(<$ty as ::std::default::Default>::default()),
        }
    };
    ($ty:ty, $name:expr, ns = $ns:expr) => {
        $crate::type_x::TypeRegistration {
            name: $name,
            namespace: ::std::option::Option::Some($ns),
            typeId: || ::std::any::TypeId::of::<$ty>(),
            rustName: ::std::stringify!($ty),
            factory: || ::std::boxed::Box::new(<$ty as ::std::default::Default>::default()),
        }
    };
}

#[cfg(test)]
mod tests {
    use super::*;

    #[derive(Debug, Default, PartialEq)]
    struct Dds { width: u32 }

    #[derive(Debug, Default, PartialEq)]
    struct Vpk;

    fn registry() -> TypeRegistry {
        let mut r = TypeRegistry::new();
        r.register(crate::register_type!(Dds, "Binary_Dds")).unwrap();
        r.register(crate::register_type!(Vpk, "Binary_Vpk", ns = "Formats.Valve")).unwrap();
        r
    }

    #[test]
    fn createsARegisteredTypeByName() {
        let r = registry();
        assert_eq!(*r.createAs::<Dds>("Binary_Dds").unwrap(), Dds { width: 0 });
    }

    #[test]
    fn resolvesNamespaceQualifiedNames() {
        let r = registry();
        assert!(r.createAs::<Vpk>("Formats.Valve.Binary_Vpk").is_ok());
        assert!(matches!(r.createAs::<Vpk>("Binary_Vpk"), Err(RegistryError::NotFound { .. })));
    }

    #[test]
    fn duplicateRegistrationIsRejected() {
        let mut r = registry();
        let err = r.register(crate::register_type!(Vpk, "Binary_Dds")).unwrap_err();
        assert!(matches!(err, RegistryError::Duplicate { .. }));
    }

    #[test]
    fn wrongTypeIsCaughtAtTheDowncast() {
        let r = registry();
        assert!(matches!(r.createAs::<Vpk>("Binary_Dds"), Err(RegistryError::WrongType { .. })));
    }

    #[test]
    fn unknownNamesReportNotFound() {
        assert!(matches!(registry().create("Nope"), Err(RegistryError::NotFound { .. })));
    }

    #[test]
    fn redirectsApplyBeforeLookup() {
        let mut r = registry();
        r.redirect("Legacy_Dds", "Binary_Dds");
        assert!(r.createAs::<Dds>("Legacy_Dds").is_ok());
    }

    #[test]
    fn namesListsBothTables() {
        assert_eq!(registry().names(), vec!["Binary_Dds", "Formats.Valve.Binary_Vpk"]);
    }

    #[test]
    fn eachCreateReturnsAnIndependentInstance() {
        let r = registry();
        let mut a = r.createAs::<Dds>("Binary_Dds").unwrap();
        a.width = 99;
        assert_eq!(r.createAs::<Dds>("Binary_Dds").unwrap().width, 0);
    }

    #[test]
    fn valuesRoundTripByName() {
        let mut r = TypeRegistry::new();
        r.registerValue("GameX.Uncore.UncoreArchive, GameX.Uncore", 7u32);
        assert_eq!(r.findValue::<u32>("GameX.Uncore.UncoreArchive, GameX.Uncore"), Some(7));
        assert!(r.findValue::<u8>("GameX.Uncore.UncoreArchive, GameX.Uncore").is_none());
    }
}
