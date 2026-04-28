# CharacterController Conversion Guide

## Changes Made

### 1. **FirstPersonMovement.cs**
- **Removed**: `Rigidbody rb` component
- **Added**: `CharacterController controller` with built-in gravity system
- **Key additions**:
  - `gravity` field (default 20) - controls how fast you fall
  - `stepOffset` field (default 0.3) - **CRITICAL for stairs** - allows smooth climbing over small obstacles
  - `verticalVelocity` property - shared velocity managed by Jump script
  - Gravity automatically applied when not grounded
  - Uses `controller.Move()` instead of velocity assignment

### 2. **Jump.cs**
- **Removed**: `Rigidbody rb` and `AddForce()`
- **Changed**: Now modifies `movement.verticalVelocity` directly
- **Key change**: Uses physics formula `√(jumpStrength × 2 × gravity)` for accurate jump heights

### 3. **Crouch.cs**
- **Removed**: References to separate `CapsuleCollider`
- **Changed**: Now resizes the `CharacterController` height directly when crouching
- This maintains proper collision detection while crouching

### 4. **Other scripts** (unchanged)
- `FirstPersonLook.cs` - Works as-is (camera rotation only)
- `FirstPersonAudio.cs` - Works as-is (audio detection unchanged)
- `Zoom.cs` - Works as-is (camera zoom only)
- `GroundCheck.cs` - Works as-is (raycasting still functional)

## Why This Fixes Stairs

**The Problem with Rigidbody:**
- Physics-based movement doesn't handle stepped surfaces smoothly
- Rigidbody must overcome full step height via jumping physics
- Results in character getting stuck on stairs or bouncing

**The CharacterController Solution:**
- **`stepOffset` parameter** automatically smooths movement over small height changes
- Set to `0.3` (30cm) - allows the character to walk up stairs without jumping
- Handles slope detection and smooth transitions
- Maintains full player control while climbing

## Unity Setup Instructions

### Step 1: Remove Old Rigidbody
1. Select your character/player GameObject in the hierarchy
2. In the Inspector, find the **Rigidbody** component
3. Click the three-dot menu and select **Remove Component**
   - Do **NOT** just disable it - remove it completely

### Step 2: Add CharacterController
1. With the character selected, click **Add Component** in the Inspector
2. Search for and add **Character Controller**
3. Configure these settings:
   - **Center**: `(0, 0, 0)` or adjust vertically to match your character's pivot
   - **Radius**: `0.4` (or match your character's width)
   - **Height**: `1.8` (or your character's height)
   - **Step Offset**: `0.3` ← **This is what fixes stairs!**
   - **Slope Limit**: `45` (allows climbing 45° slopes)
   - **Skin Width**: `0.08` (collision buffer)

### Step 3: Update FirstPersonMovement Component
1. Select the character again
2. In the **FirstPersonMovement** script inspector:
   - **Gravity**: `20` (default) - increase for faster falling, decrease for floaty feel
   - **Step Offset**: `0.3` (auto-synced from CharacterController)
   - These are now exposed for tweaking

### Step 4: Verify Jump Script
1. The **Jump** script should auto-update and reference `FirstPersonMovement`
2. **Jump Strength**: `2` (default) - adjust for higher/lower jumps
3. The jump velocity is now calculated based on gravity for realistic physics

### Step 5: Verify Crouch Script
1. The **Crouch** script now uses the CharacterController
2. Ensure these are assigned in Inspector:
   - **Movement**: Should auto-assign
   - **Head To Lower**: Should auto-assign from Camera child
   - **Controller**: Should auto-assign from parent
3. Adjust **Crouch Y Head Position**: `1` (how low head goes when crouching)

### Step 6: Physics Layers (Optional but Recommended)
1. Ensure your character's CharacterController is on a **Player** layer
2. Ensure stairs/ramps have **ground colliders** (Box, Mesh, or Terrain colliders)
3. Set Physics layer collision matrix so CharacterController collides with ground

## Testing Checklist

- [ ] Character moves smoothly on flat ground
- [ ] Character walks up stairs **without jumping** (test the fix!)
- [ ] Character lands properly from jumps
- [ ] Gravity feels natural (adjust if too fast/slow)
- [ ] Crouch works and lowers height properly
- [ ] Running speed increase works
- [ ] Jump height is reasonable
- [ ] Character doesn't get stuck on stairs or slopes

## Common Issues & Fixes

### Issue: Character keeps sliding down stairs
- **Solution**: Increase `stepOffset` to `0.4` or `0.5`

### Issue: Character floats/falls too slowly
- **Solution**: Increase `gravity` value (e.g., to `25` or `30`)

### Issue: Jump feels too weak
- **Solution**: Increase `Jump Strength` (e.g., from `2` to `3`)

### Issue: Movement feels sluggish
- **Solution**: Ensure `FixedUpdate()` timing is correct; check Time settings in Edit → Project Settings → Time

### Issue: Collision detection not working
- **Solution**: Verify CharacterController isn't set to "Is Trigger"

## Advanced Tuning

### For Steeper Stairs/Slopes
- Increase `stepOffset` up to `1.0` (1 meter) for dramatic slopes
- Keep `Slope Limit` at `45°` unless you want steeper climbing

### For Smoother Movement Feel
- Increase **Gravity** for snappier falling
- Decrease for a more floaty, low-gravity feel

### Jump Adjustments
- Formula: `jumpVelocity = √(jumpStrength × 2 × gravity)`
- Higher gravity + lower jumpStrength = controlled jumps
- Lower gravity + higher jumpStrength = floaty jumps

## Migration Notes

All original functionality is preserved:
- Movement input detection (WASD)
- Running (Shift)
- Jumping (Space)
- Looking (Mouse)
- Crouching (Ctrl)
- Audio cues
- Zoom (Mouse Scroll)

The **only change is the physics layer** - everything is now CharacterController-based instead of Rigidbody-based.
