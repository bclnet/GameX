//! C ABI to `native/unreal_shim.cpp`.
//!
//! The default build links a stub. `linked` is false. This is not an Unreal host.

use std::ffi::{c_char, CStr};

extern "C" {
    fn unreal_shim_linked() -> i32;
    fn unreal_shim_version() -> *const c_char;
}

/// True only when this build linked Unreal Engine. The default shim returns false.
pub fn linked() -> bool {
    // Stub returns 0 or, in a real host, non-zero. No Unreal state.
    unsafe { unreal_shim_linked() != 0 }
}

/// Shim identity. The default build contains "stub", not an Unreal version.
pub fn version() -> &'static str {
    unsafe {
        CStr::from_ptr(unreal_shim_version())
            .to_str()
            .unwrap_or("")
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn defaultBuildIsAStub() {
        assert!(!linked());
        assert!(version().contains("stub"));
    }
}
