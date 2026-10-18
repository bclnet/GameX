//! C ABI to `native/diligent_shim.cpp`.
//!
//! The default build links a stub. `linked` is false. This is not a Diligent host.

use std::ffi::{c_char, CStr};

extern "C" {
    fn diligentShimLinked() -> i32;
    fn diligentShimVersion() -> *const c_char;
}

/// True only when this build linked Diligent Engine. The default shim returns false.
pub fn linked() -> bool {
    // Stub returns 0 or, in a real host, non-zero. No Diligent state.
    unsafe { diligentShimLinked() != 0 }
}

/// Shim identity. The default build returns "not linked", not a Diligent version.
pub fn version() -> &'static str {
    unsafe {
        CStr::from_ptr(diligentShimVersion())
            .to_str()
            .unwrap_or("")
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn defaultBuildIsNotLinked() {
        assert!(!linked());
        assert_eq!(version(), "not linked");
    }
}
