// PORT-SOURCE: Engines/GameX.Engine.Unity/Unity_Black.cs
// PORT-SHA: unstamped
// PORT-STATUS: partial
//
// Tilemap, Grid, and Sprite are C# only. There is no UnityEngine.
// `buildObject` counts floor ids that are not 0 or 1 and names each elevation.

use std::any::Any;
use std::io::{Error, ErrorKind};

use gamex_black::formats::binary::Binary_Map;

use crate::unity_render::UnityObjectSummary;

/// Fallout MAP, without a Unity `GameObject`.
pub fn buildObject(path: &(dyn Any + Send + Sync)) -> Result<UnityObjectSummary, Error> {
    let map = path.downcast_ref::<Binary_Map>().ok_or_else(|| Error::new(ErrorKind::InvalidInput, "InvalidCastException: Binary_Map"))?;
    let name = if map.Name.is_empty() { "map".to_string() } else { map.Name.clone() };
    let mut vertexCount = 0usize;
    let mut groupNames = Vec::new();
    for level in &map.Levels {
        vertexCount += level.Floor.iter().filter(|&&id| id != 0 && id != 1).count();
        groupNames.push(format!("Floor{}", level.Elevation));
        groupNames.push(format!("Roof{}", level.Elevation));
    }
    Ok(UnityObjectSummary {
        Name: name,
        VertexCount: vertexCount,
        SubmeshCount: map.Levels.len(),
        GroupNames: groupNames,
    })
}
