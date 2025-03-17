# Procedural World Generation System

A comprehensive procedural generation system inspired by games like "Minecraft", "Terraria", "Core Keeper", and "Tinkerland", utilizing multiple algorithms to create natural-looking, interconnected environments.

## 1. Terrain Generation Algorithms

### Perlin Noise
The primary algorithm for creating natural-looking terrain:
- Generates smooth transitions between noise values
- Creates high-quality, unique terrain layouts
- Enables biome division based on noise values:
  - Lower values: Water bodies
  - Medium values: Plains
  - Higher values: Mountains

### Random Walk for Environmental Elements
Used for natural placement of landscape features (trees, rocks, vegetation):
- Selects multiple random starting points on specific terrain types
- Implements a modified 4-directional movement system
- Creates paths until reaching a specified step count
- Results in naturally distributed landscape elements
- Avoids rigid, repetitive patterns

## 2. Structure Generation Algorithms

### Binary Space Partitioning (BSP) for Settlements
Creates organized settlement layouts:
- Divides the map into sections using random axis splits
- Recursively subdivides areas until reaching:
  - Specified iteration count
  - Minimum settlement size requirement
- Validates terrain compatibility for structure placement
- Generates primary buildings followed by surrounding structures
- Creates complete settlements in viable zones

### L-Systems for Path Generation
Implements Lindenmayer Systems for connecting structures:
- Creates natural-looking path networks
- Generates branching patterns similar to organic growth
- Ensures logical connectivity between buildings
- Results in realistic-looking settlement layouts

## 3. Dungeon Generation Algorithms

### Modified BSP for Dungeon Rooms
Specialized implementation for underground structures:
- Uses wider partition ranges for varied room sizes
- Creates more numerous, smaller sections than settlement BSP
- Implements room count limiting
- Ensures proper room spacing and layout

### Random Walk for Corridor Generation
Modified for dungeon connectivity:
- Selects nearby room pairs for connection
- Implements directional constraints:
  - Moves only in two directions
  - Targets specific coordinates
  - Stops upon reaching destination
- Creates logical pathways between rooms

## Implementation Features

- Configurable generation parameters via SO files
- Biome-aware structure placement
- Natural terrain transitions
- Interconnected generation systems
- Performance-optimized algorithms

## Technical Considerations

- Terrain validation for structure placement
- Settlement density control
- Path optimization for dungeons
- Room connectivity verification
- Environmental feature distribution

This system creates cohesive, natural-looking worlds while maintaining gameplay functionality and performance efficiency.
