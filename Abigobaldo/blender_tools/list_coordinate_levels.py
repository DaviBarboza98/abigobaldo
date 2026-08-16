import json
import os
import sys

import bpy


def script_args():
    if "--" not in sys.argv:
        return []
    return sys.argv[sys.argv.index("--") + 1 :]


def axis_levels(obj, axis):
    values = [round(float((obj.matrix_world @ vertex.co)[axis]), 5) for vertex in obj.data.vertices]
    counts = {}
    for value in values:
        counts[str(value)] = counts.get(str(value), 0) + 1
    return dict(sorted(counts.items(), key=lambda item: float(item[0])))


args = script_args()
if not args:
    raise SystemExit("Expected output JSON path after --")

payload = {
    obj.name: {"x": axis_levels(obj, 0), "y": axis_levels(obj, 1), "z": axis_levels(obj, 2)}
    for obj in bpy.context.scene.objects
    if obj.type == "MESH" and obj.data
}

output_path = os.path.abspath(args[0])
os.makedirs(os.path.dirname(output_path), exist_ok=True)
with open(output_path, "w", encoding="utf-8") as handle:
    json.dump(payload, handle, indent=2)

print(f"WROTE_COORDINATE_LEVELS={output_path}")
