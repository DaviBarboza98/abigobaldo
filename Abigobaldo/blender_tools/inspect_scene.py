import json
import math
import os
import sys

import bpy
from mathutils import Vector


def script_args():
    if "--" not in sys.argv:
        return []
    return sys.argv[sys.argv.index("--") + 1 :]


def rounded(values, digits=5):
    return [round(float(value), digits) for value in values]


def world_bounds(obj):
    if not hasattr(obj, "bound_box") or not obj.bound_box:
        return None
    corners = [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]
    minimum = Vector((min(point.x for point in corners), min(point.y for point in corners), min(point.z for point in corners)))
    maximum = Vector((max(point.x for point in corners), max(point.y for point in corners), max(point.z for point in corners)))
    return {
        "min": rounded(minimum),
        "max": rounded(maximum),
        "size": rounded(maximum - minimum),
        "center": rounded((minimum + maximum) * 0.5),
    }


def object_record(obj):
    record = {
        "name": obj.name,
        "type": obj.type,
        "collections": [collection.name for collection in obj.users_collection],
        "location": rounded(obj.location),
        "rotation_degrees": rounded([math.degrees(value) for value in obj.rotation_euler], 3),
        "scale": rounded(obj.scale),
        "dimensions": rounded(obj.dimensions),
        "visible_render": not obj.hide_render,
        "visible_viewport": not obj.hide_viewport,
        "parent": obj.parent.name if obj.parent else None,
        "bounds_world": world_bounds(obj),
    }
    if obj.type == "MESH" and obj.data:
        record.update(
            {
                "vertices": len(obj.data.vertices),
                "edges": len(obj.data.edges),
                "polygons": len(obj.data.polygons),
                "materials": [slot.material.name if slot.material else None for slot in obj.material_slots],
                "modifiers": [modifier.type for modifier in obj.modifiers],
            }
        )
    elif obj.type == "CAMERA" and obj.data:
        record.update(
            {
                "camera_type": obj.data.type,
                "lens": obj.data.lens,
                "sensor_width": obj.data.sensor_width,
                "clip_start": obj.data.clip_start,
                "clip_end": obj.data.clip_end,
            }
        )
    elif obj.type == "LIGHT" and obj.data:
        record.update(
            {
                "light_type": obj.data.type,
                "energy": obj.data.energy,
                "color": rounded(obj.data.color),
                "size": getattr(obj.data, "size", None),
            }
        )
    return record


args = script_args()
if not args:
    raise SystemExit("Expected output JSON path after --")

output_path = os.path.abspath(args[0])
scene = bpy.context.scene
objects = [object_record(obj) for obj in scene.objects]
renderable_bounds = [item["bounds_world"] for item in objects if item["type"] == "MESH" and item["visible_render"] and item["bounds_world"]]

overall_bounds = None
if renderable_bounds:
    minimum = [min(bounds["min"][axis] for bounds in renderable_bounds) for axis in range(3)]
    maximum = [max(bounds["max"][axis] for bounds in renderable_bounds) for axis in range(3)]
    overall_bounds = {
        "min": rounded(minimum),
        "max": rounded(maximum),
        "size": rounded([maximum[axis] - minimum[axis] for axis in range(3)]),
        "center": rounded([(maximum[axis] + minimum[axis]) * 0.5 for axis in range(3)]),
    }

payload = {
    "blender_version": bpy.app.version_string,
    "file": bpy.data.filepath,
    "active_scene": scene.name,
    "scenes": [item.name for item in bpy.data.scenes],
    "collections": [
        {
            "name": collection.name,
            "objects": [obj.name for obj in collection.objects],
            "hide_render": collection.hide_render,
            "hide_viewport": collection.hide_viewport,
        }
        for collection in bpy.data.collections
    ],
    "world": scene.world.name if scene.world else None,
    "engine": scene.render.engine,
    "resolution": [scene.render.resolution_x, scene.render.resolution_y, scene.render.resolution_percentage],
    "camera": scene.camera.name if scene.camera else None,
    "view_transform": scene.view_settings.look if hasattr(scene.view_settings, "look") else None,
    "overall_renderable_mesh_bounds": overall_bounds,
    "materials": [
        {
            "name": material.name,
            "use_nodes": material.use_nodes,
            "diffuse_color": rounded(material.diffuse_color),
        }
        for material in bpy.data.materials
    ],
    "objects": objects,
}

os.makedirs(os.path.dirname(output_path), exist_ok=True)
with open(output_path, "w", encoding="utf-8") as handle:
    json.dump(payload, handle, ensure_ascii=False, indent=2)

print(f"WROTE_SCENE_REPORT={output_path}")
