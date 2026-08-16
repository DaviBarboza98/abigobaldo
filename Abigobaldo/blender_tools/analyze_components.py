import json
import os
import sys
from collections import deque

import bpy
from mathutils import Vector


def script_args():
    if "--" not in sys.argv:
        return []
    return sys.argv[sys.argv.index("--") + 1 :]


def rounded(vector):
    return [round(float(value), 5) for value in vector]


def connected_component_bounds(obj):
    mesh = obj.data
    adjacency = [set() for _ in mesh.vertices]
    for edge in mesh.edges:
        a, b = edge.vertices
        adjacency[a].add(b)
        adjacency[b].add(a)

    remaining = set(range(len(mesh.vertices)))
    components = []
    while remaining:
        seed = next(iter(remaining))
        queue = deque([seed])
        remaining.remove(seed)
        indices = []
        while queue:
            index = queue.popleft()
            indices.append(index)
            for neighbor in adjacency[index]:
                if neighbor in remaining:
                    remaining.remove(neighbor)
                    queue.append(neighbor)

        points = [obj.matrix_world @ mesh.vertices[index].co for index in indices]
        minimum = Vector((min(point.x for point in points), min(point.y for point in points), min(point.z for point in points)))
        maximum = Vector((max(point.x for point in points), max(point.y for point in points), max(point.z for point in points)))
        components.append(
            {
                "vertex_count": len(indices),
                "min": rounded(minimum),
                "max": rounded(maximum),
                "size": rounded(maximum - minimum),
                "center": rounded((minimum + maximum) * 0.5),
            }
        )

    components.sort(key=lambda item: (item["center"][2], item["center"][1], item["center"][0]))
    return components


args = script_args()
if not args:
    raise SystemExit("Expected output JSON path after --")

output_path = os.path.abspath(args[0])
payload = {}
for obj in bpy.context.scene.objects:
    if obj.type == "MESH" and obj.data:
        payload[obj.name] = connected_component_bounds(obj)

os.makedirs(os.path.dirname(output_path), exist_ok=True)
with open(output_path, "w", encoding="utf-8") as handle:
    json.dump(payload, handle, indent=2)

print(f"WROTE_COMPONENT_REPORT={output_path}")
