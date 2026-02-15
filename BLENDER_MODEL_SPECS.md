# Blender Pool Table Model Specifications

**Date:** 2026-02-14
**Blender Version:** 4.3.2
**Table Type:** 8-foot regulation pool table
**Export Target:** Unity 6 (6000.3.6f1) HDRP

---

## Table Dimensions

### Felt (Playing Surface)
- **Length (X):** 2.24682m (exactly)
- **Width (Y):** 1.12631m (exactly)
- **Height (Z):** 0.7530m (center)
- **Scale:** 1.0, 1.0, 1.0 (transforms applied)
- **Material:** wire_006135006

### Overall Table
- **Frame:** woodframe object
- **Rails:** rail_01 through rail_06
- **Pockets:** pocket_01 through pocket_06 (6 pockets total)

---

## Ball Specifications

### All Balls (16 total)
- **Diameter:** 0.05715m (57.15mm) - EXACTLY
- **Radius:** 0.028575m (28.575mm)
- **Scale:** 1.0, 1.0, 1.0 (transforms applied)
- **Z Position (table height):** 0.795m

### Ball List
- **cueball** - White cue ball
- **ball_01** through **ball_15** - Numbered balls in regulation rack formation

### Ball Materials
- Cueball: wire_087225087
- Numbered balls: not sure 
---

## Cuestick Specifications

### Pivot/Origin Point
- **Location:** At the TIP (narrow end with radius 0.0032m)
- **Method:** Set using 3D cursor to tip vertex (minimum X in local mesh)

### Position (Break Shot Setup)
- **X:** 0.620280m (right side of cueball, positive X side)
- **Y:** 0.000000m (aligned with cueball and rack apex)
- **Z:** 0.795000m (table height, same as balls)

### Orientation
- **Rotation X:** 0.0°
- **Rotation Y:** 0.0°
- **Rotation Z:** 0.0°

### Geometry
- **Tip (origin) X:** 0.620280m
- **Butt end X:** 2.117280m (extends away from rack toward positive X)
- **Total length:** 1.497m
- **Material:** side_wood

### Tip Positioning
- **Gap from cueball surface:** 3cm (0.03m)
- **Cueball surface (right side):** 0.590280m
- **Tip position:** 0.620280m
- **Direction:** Tip on right side of cueball, ready to strike toward rack (negative X)

---

## Standard Break Shot Setup

### Cueball Position
- **X:** 0.561705m (at head string - 1/4 table length from center)
- **Y:** 0.000000m (centered on table width)
- **Z:** 0.795000m (table height)

### Rack Position
- **Apex ball (ball_01) X:** -0.526027m (at foot spot - opposite end)
- **Apex ball Y:** 0.000000m (centered)
- **Rack center X:** -0.658008m

### Alignment
- **Distance cueball to rack apex:** 1.087732m
- **Y-axis alignment:** Perfect (0.00mm difference)
- **Shot direction:** From positive X toward negative X (toward rack)

### Table Landmarks
- **Felt center:** X = 0.000m, Y = 0.000m
- **Head string:** X = ±0.561705m (cueball break zone)
- **Foot spot:** X = ±0.561705m (rack apex position)

---

## Rack Formation

### Triangle Configuration
- **15 balls** in standard 5-4-3-2-1 triangle
- **Ball spacing:** 0.05715m center-to-center (touching)
- **Apex orientation:** Points toward cueball (positive X direction)

### Rack Measurements
- **Apex (ball_01) X:** -0.526027m
- **Base (row of 5) X:** ~-0.724m
- **Spread:** Equilateral triangle, all balls barly touching

---

## Export Notes

### Coordinate System
- **X-axis:** Table length (cueball side = positive, rack side = negative)
- **Y-axis:** Table width (left/right from player view)
- **Z-axis:** Vertical (up = positive)

### Unity Import Considerations
1. All objects have scale 1,1,1 with transforms applied
2. Cuestick pivot is at tip for Unity rotation
3. Cueball and rack are pre-positioned for break shot
4. All measurements are in meters (Unity default)

### Physics Materials Required (Unity)
- **Ball:** friction 0.05, bounce 0.95, mass 0.17kg
- **Felt:** friction 0.6-0.8, bounce 0
- **Rails:** friction 0.2, bounce 0.9

---

## Verification Checklist

- [x] Felt dimensions: 2.24682m x 1.12631m (8-foot regulation)
- [x] All 16 balls: 0.05715m diameter exactly
- [x] Cuestick pivot at tip (narrow end)
- [x] Cuestick positioned behind cueball for break shot
- [x] Cuestick tip 2cm from cueball surface
- [x] Cuestick oriented correctly (tip toward cueball, butt away from table)
- [x] Cueball at head string (X = 0.562m)
- [x] Rack apex at foot spot (X = -0.526m)
- [x] Cueball and rack aligned on Y-axis (0.00mm difference)
- [x] All scales applied (1.0, 1.0, 1.0)

---

## Change Log

### 2026-02-14
- Set felt to exact 8-foot dimensions (2.24682 x 1.12631m)
- Scaled all 16 balls to exact 0.05715m diameter
- Re-racked 15 balls in proper triangle formation at X=-0.658m
- Set cuestick pivot to tip (narrow end) using 3D cursor method
- Positioned cuestick for break shot setup
- Fixed cuestick orientation: tip on RIGHT side of cueball (positive X)
- Set rotation to (0, 0, 0) - no rotation needed
- Final position: tip at X=0.620m, butt at X=2.117m
- Gap from cueball: 3cm from right surface
- Verified cueball at standard break position (head string X=0.562m)
- Confirmed perfect Y-axis alignment between cueball and rack apex (0.00mm difference)
- Cuestick ready to strike cueball toward rack (negative X direction)

---

**Status:** Ready for Unity export
**Next Step:** Export as FBX/GLTF and import into Unity 6 project
