import math
import os
import sys

import bpy
from bpy_extras.object_utils import world_to_camera_view
from mathutils import Matrix, Vector


BENCH_SOURCE_NAME = "Cube.001"
SHELF_SOURCE_NAME = "Cube.010"
PRESENTATION_SCENE_NAME = "Apresentacao_Cliente"


def arguments():
    if "--" not in sys.argv:
        return []
    return sys.argv[sys.argv.index("--") + 1 :]


def look_at(obj, target):
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat("-Z", "Y").to_euler()


def make_material(name, color, roughness=0.65, metallic=0.0):
    material = bpy.data.materials.new(name)
    material.use_nodes = True
    shader = material.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*color, 1.0)
    shader.inputs["Roughness"].default_value = roughness
    shader.inputs["Metallic"].default_value = metallic
    if "Specular IOR Level" in shader.inputs:
        shader.inputs["Specular IOR Level"].default_value = 0.34
    return material


def mesh_points_world(obj):
    return [obj.matrix_world @ Vector(corner) for corner in obj.bound_box]


def bounds_for(objects):
    points = []
    for obj in objects:
        points.extend(mesh_points_world(obj))
    low = Vector(tuple(min(point[axis] for point in points) for axis in range(3)))
    high = Vector(tuple(max(point[axis] for point in points) for axis in range(3)))
    return low, high, points


def bake_world_transform(source, target_scene, name, material):
    duplicate = source.copy()
    duplicate.data = source.data.copy()
    duplicate.name = name
    duplicate.data.name = f"{name}_Mesh"
    target_scene.collection.objects.link(duplicate)

    # Bake the source transform into independent mesh data. This preserves the
    # visible geometry while making presentation placement predictable.
    duplicate.data.transform(source.matrix_world)
    duplicate.matrix_world = Matrix.Identity(4)

    points = [vertex.co.copy() for vertex in duplicate.data.vertices]
    low = Vector(tuple(min(point[axis] for point in points) for axis in range(3)))
    high = Vector(tuple(max(point[axis] for point in points) for axis in range(3)))
    center_xy = Vector(((low.x + high.x) * 0.5, (low.y + high.y) * 0.5, low.z))
    duplicate.data.transform(Matrix.Translation(-center_xy))

    duplicate.data.materials.clear()
    duplicate.data.materials.append(material)

    dimensions = high - low
    bevel = duplicate.modifiers.new("Acabamento_suave", "BEVEL")
    bevel.width = max(0.018, min(dimensions) * 0.009)
    bevel.segments = 3
    bevel.limit_method = "ANGLE"
    bevel.angle_limit = math.radians(24.0)
    if hasattr(bevel, "harden_normals"):
        bevel.harden_normals = True
    return duplicate


def add_sun(scene, name, location, energy, angle_degrees, color, target):
    data = bpy.data.lights.new(name, "SUN")
    data.energy = energy
    data.angle = math.radians(angle_degrees)
    data.color = color
    obj = bpy.data.objects.new(name, data)
    scene.collection.objects.link(obj)
    obj.location = Vector(location)
    look_at(obj, target)
    return obj


def add_area(scene, name, location, energy, size, color, target):
    data = bpy.data.lights.new(name, "AREA")
    data.energy = energy
    data.shape = "DISK"
    data.size = size
    data.color = color
    obj = bpy.data.objects.new(name, data)
    scene.collection.objects.link(obj)
    obj.location = Vector(location)
    look_at(obj, target)
    return obj


def add_cyclorama(scene, material, width=64.0, floor_depth=42.0, curve_start=15.0, radius=8.0, height=28.0):
    profile = [(-floor_depth, -0.055), (curve_start, -0.055)]
    center_y = curve_start
    center_z = radius - 0.055
    for step in range(1, 9):
        theta = math.radians(-90.0 + 90.0 * step / 8.0)
        profile.append((center_y + radius * math.cos(theta), center_z + radius * math.sin(theta)))
    profile.append((curve_start + radius, height))

    vertices = []
    half_width = width * 0.5
    for y, z in profile:
        vertices.extend([(-half_width, y, z), (half_width, y, z)])

    faces = []
    for index in range(len(profile) - 1):
        a = index * 2
        faces.append((a, a + 1, a + 3, a + 2))

    mesh = bpy.data.meshes.new("Ciclorama_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new("Ciclorama", mesh)
    scene.collection.objects.link(obj)
    obj.data.materials.append(material)
    for polygon in mesh.polygons:
        polygon.use_smooth = True
    return obj


def frame_perspective(scene, camera, objects, view_vector=(0.0, -1.0, 0.30), margin=0.10):
    low, high, points = bounds_for(objects)
    target = (low + high) * 0.5
    target.z = low.z + (high.z - low.z) * 0.47
    direction = Vector(view_vector).normalized()
    radius = max((point - target).length for point in points)
    half_angle = min(camera.data.angle_x, camera.data.angle_y) * 0.5
    distance = radius / max(math.sin(half_angle), 0.05) * 1.08

    target_span = 1.0 - margin * 2.0
    for _ in range(18):
        camera.location = target + direction * distance
        look_at(camera, target)
        scene.view_layers[0].update()
        projected = [world_to_camera_view(scene, camera, point) for point in points]
        min_x = min(point.x for point in projected)
        max_x = max(point.x for point in projected)
        min_y = min(point.y for point in projected)
        max_y = max(point.y for point in projected)
        span_x = max_x - min_x
        span_y = max_y - min_y
        scale_ratio = max(span_x / target_span, span_y / target_span)
        inside = min_x >= margin and max_x <= 1.0 - margin and min_y >= margin and max_y <= 1.0 - margin
        if inside and 0.965 <= scale_ratio <= 0.995:
            break
        if scale_ratio < 0.965:
            distance *= max(0.68, scale_ratio * 1.02)
        else:
            distance *= min(1.32, scale_ratio * 1.035)

    camera.data.clip_start = max(0.01, distance * 0.01)
    camera.data.clip_end = distance * 12.0
    return low, high


def render_shot(scene, camera, model_objects, visible_objects, output_path, resolution):
    for obj in model_objects:
        obj.hide_render = obj not in visible_objects

    scene.render.resolution_x = resolution[0]
    scene.render.resolution_y = resolution[1]
    scene.render.resolution_percentage = 100
    scene.render.filepath = output_path
    frame_perspective(scene, camera, visible_objects)
    bpy.ops.render.render(write_still=True)
    print(f"CLIENT_RENDER={output_path}")


args = arguments()
if not args:
    raise SystemExit("Expected output directory after --")

output_dir = os.path.abspath(args[0])
os.makedirs(output_dir, exist_ok=True)
preview_mode = len(args) > 1 and args[1].lower() == "preview"

source_scene = bpy.context.scene
missing = [name for name in (BENCH_SOURCE_NAME, SHELF_SOURCE_NAME) if name not in source_scene.objects]
if missing:
    raise RuntimeError(f"Missing expected model object(s): {', '.join(missing)}")

old_scene = bpy.data.scenes.get(PRESENTATION_SCENE_NAME)
if old_scene:
    bpy.data.scenes.remove(old_scene)

scene = bpy.data.scenes.new(PRESENTATION_SCENE_NAME)
bpy.context.window.scene = scene

bench_material = make_material("Bancada_Branco_Gelo", (0.72, 0.77, 0.83), 0.61)
shelf_material = make_material("Prateleira_Branco_Gelo", (0.79, 0.82, 0.86), 0.64)
floor_material = make_material("Piso_Neutro", (0.58, 0.60, 0.63), 0.91)

bench = bake_world_transform(source_scene.objects[BENCH_SOURCE_NAME], scene, "Bancada", bench_material)
shelf = bake_world_transform(source_scene.objects[SHELF_SOURCE_NAME], scene, "Prateleira", shelf_material)
models = [bench, shelf]

# Both models use the same rotation and retain their original relative scale.
# This exposes the shelves and the long counter face in a readable 3/4 view.
rotation = math.radians(-35.0)
bench.rotation_euler.z = rotation
shelf.rotation_euler.z = rotation
scene.view_layers[0].update()

gap = 1.55
bench_low, bench_high, _ = bounds_for([bench])
shelf_low, shelf_high, _ = bounds_for([shelf])
bench.location.x += -gap * 0.5 - bench_high.x
shelf.location.x += gap * 0.5 - shelf_low.x
scene.view_layers[0].update()

combined_low, combined_high, _ = bounds_for(models)
horizontal_center = (combined_low.x + combined_high.x) * 0.5
bench.location.x -= horizontal_center
shelf.location.x -= horizontal_center
scene.view_layers[0].update()

combined_low, combined_high, _ = bounds_for(models)
center = (combined_low + combined_high) * 0.5

# A curved matte studio backdrop creates soft contact shadows and removes a hard horizon.
floor = add_cyclorama(scene, floor_material)

camera_data = bpy.data.cameras.new("Camera_Cliente")
camera_data.type = "PERSP"
camera_data.lens = 55.0
camera_data.sensor_width = 36.0
camera = bpy.data.objects.new("Camera_Cliente", camera_data)
scene.collection.objects.link(camera)
scene.camera = camera

lighting_target = Vector((center.x, center.y, 2.0))
add_sun(scene, "Luz_Principal", (-10.0, -14.0, 18.0), 2.1, 11.0, (1.0, 0.93, 0.86), lighting_target)
add_sun(scene, "Luz_Preenchimento", (13.0, -5.0, 10.0), 0.58, 20.0, (0.77, 0.87, 1.0), lighting_target)
add_sun(scene, "Luz_Recorte", (4.0, 12.0, 16.0), 0.72, 18.0, (1.0, 1.0, 1.0), lighting_target)
add_area(scene, "Softbox_Frontal", (-4.0, -9.0, 12.0), 650.0, 9.0, (1.0, 0.96, 0.91), lighting_target)

world = bpy.data.worlds.new("Fundo_Neutro")
world.use_nodes = True
background = world.node_tree.nodes.get("Background")
background.inputs["Color"].default_value = (0.70, 0.72, 0.76, 1.0)
background.inputs["Strength"].default_value = 0.46
scene.world = world

scene.render.engine = "BLENDER_EEVEE"
if hasattr(scene, "eevee") and hasattr(scene.eevee, "taa_render_samples"):
    scene.eevee.taa_render_samples = 96
scene.render.image_settings.file_format = "PNG"
scene.render.image_settings.color_mode = "RGB"
scene.render.image_settings.color_depth = "8"
scene.render.film_transparent = False
scene.render.use_file_extension = True
scene.render.resolution_percentage = 100

try:
    scene.view_settings.look = "AgX - Medium High Contrast"
except TypeError:
    pass
scene.view_settings.exposure = 0.15

if preview_mode:
    render_shot(
        scene,
        camera,
        models,
        models,
        os.path.join(output_dir, "farmacia_apresentacao_preview.png"),
        (1200, 800),
    )
else:
    render_shot(
        scene,
        camera,
        models,
        models,
        os.path.join(output_dir, "farmacia_apresentacao_cliente.png"),
        (2400, 1600),
    )
    render_shot(
        scene,
        camera,
        models,
        [bench],
        os.path.join(output_dir, "bancada_cliente.png"),
        (2200, 1600),
    )
    render_shot(
        scene,
        camera,
        models,
        [shelf],
        os.path.join(output_dir, "prateleira_cliente.png"),
        (1800, 1800),
    )

    # Leave the saved copy in the combined presentation state.
    for obj in models:
        obj.hide_render = False
    frame_perspective(scene, camera, models)
    copy_path = os.path.join(output_dir, "farmacia_apresentacao_cliente.blend")
    bpy.ops.wm.save_as_mainfile(filepath=copy_path)
    print(f"CLIENT_BLEND_COPY={copy_path}")

print("CLIENT_PRESENTATION_DONE")
