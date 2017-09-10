# exports each object into its own file
# expects one argument for output path

import bpy
import os
import sys

argv = sys.argv
argv = argv[argv.index("--") + 1:]
outputPath = argv[0]

scene = bpy.context.scene

bpy.ops.object.select_all(action='DESELECT')

for obj in scene.objects:

    obj.select = True

    # rename object materials so that the texture filepath gets exported to the mtl files
    materials = []
    for s in obj.material_slots:
        if s.material and s.material.use_nodes:
            for n in s.material.node_tree.nodes:
                if n.type == 'TEX_IMAGE':
                    materials.append({"material": s.material, "name": s.material.name})
                    s.material.name = n.image.filepath
                    break

    # some exporters only use the active object
    scene.objects.active = obj

    name = bpy.path.clean_name(obj.name)
    fn = os.path.join(outputPath, name)

    bpy.ops.export_scene.obj(filepath=fn + ".obj", use_selection=True, axis_up='Z', axis_forward='X')

    # undo the renaming step
    for material in materials:
        material["material"].name = material["name"]

    obj.select = False