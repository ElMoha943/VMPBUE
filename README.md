# Udon Material Property Block Applier for VRChat

This tool is designed to help you assign and apply material property blocks to renderers on your VRChat scene.

## Installation

You can download the latest version of the tool from the [releases page](https://github.com/elmoha943/VMPBUE/releases) or get it directly on your VCC/Alcom from my [package listing](https://valenvrc.com).

## Usage

1. Add an **MPB Component** to any GameObject with a Renderer (MeshRenderer, SkinnedMeshRenderer, etc.).
2. The component will automatically scan the renderer's materials and list all available properties (colors, floats, textures).
3. You can edit these properties directly in the inspector, and they will be applied to the renderer using a Material Property Block at runtime.

## Why use material property blocks instead of different materials?

Material Property Blocks (MPBs) let you override material properties per renderer without touching the shared material asset. Compared to changing material properties directly or creating multiple materials, they provide clear advantages:

- No material instancing: Changing a material at runtime creates a unique instance, increasing memory usage. MPBs avoid this entirely.
- Better batching & GPU instancing: Objects can still share the same material and be batched or GPU-instanced, improving draw-call performance.
- Lower memory footprint: One material, many visual variations (colors, floats, textures) without duplicating materials.
- Safe runtime changes: Modifying materials directly can unintentionally affect all objects using that material; MPBs are isolated per renderer.
