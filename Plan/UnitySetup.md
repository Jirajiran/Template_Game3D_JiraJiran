# Unity Setup — Inspector & Portability

เอกสารนี้บันทึกขั้นตอนลงมือใน Unity Inspector ทุกครั้งที่เพิ่มระบบจาก `template/Scripts/`
เพื่อย้ายโปรเจกต์ไปเครื่องอื่นได้โดยอ้างอิง asset path ในโปรเจกต์

## โครงสร้าง Unity project ที่แนะนำ

```
Assets/FPSGame/
  Scripts/              ← copy จาก vault template/Scripts/
  Data/
    Characters/         ← CharacterStats .asset
    Weapons/            ← WeaponData .asset
  Prefabs/
    Player/
    Weapons/
  Scenes/
    PrototypeGameplay.unity
  UI/
```

## Global — ทำครั้งเดียวตอนสร้างโปรเจกต์

### Tags
| Tag | ใช้กับ |
|-----|--------|
| Player | รากผู้เล่น |
| Enemy | ศัตรู AI |

### Layers
| Layer | ใช้กับ |
|-------|--------|
| Ground | พื้นเดิน |
| Player | ผู้เล่น (optional) |
| Enemy | ศัตรู |
| Hitbox | melee trigger ชั่วคราว |

### Packages (manifest.json)
- Input System (แนะนำ) หรือ Legacy Input Manager — เลือกอย่างใดอย่างหนึ่งแล้วบันทึกไว้ด้านล่างนี้

```
Input: Input System (แนะนำ — ดู template/UnityBootstrap/manifest.json.example)
Render pipeline: Built-in หรือ URP (เลือกตอนสร้าง project)
```

---

## Phase 0 — Project Bootstrap (ทำครั้งเดียว) — 2025-06-20

### Prerequisites
- Unity 2022.3 LTS ขึ้นไป (แนะนำ)
- Vault: `MyGameProject_Vault_base/template/Scripts/` พร้อม copy

### วิธีที่ 1 — อัตโนมัติ (แนะนำ)

1. สร้าง Unity project ใหม่ (3D)
2. Copy `template/Scripts/` → `Assets/FPSGame/Scripts/` (รวม `Editor/` และ `.asmdef`)
3. รอ Unity compile
4. เมนู **FPSGame → Setup Phase 0 (Project Bootstrap)**
5. เมนู **FPSGame → Open PrototypeGameplay Scene** เพื่อตรวจ scene

### วิธีที่ 2 — มือ (ถ้าไม่ใช้ Editor menu)

#### Folder layout
```
Assets/FPSGame/
  Scripts/              ← copy จาก vault
  Data/Characters/
  Data/Weapons/
  Prefabs/Player/
  Prefabs/Weapons/
  Scenes/
  UI/HUD/
  Animation/
```

#### Tags (Project Settings → Tags and Layers)
| Tag | ใช้กับ |
|-----|--------|
| Player | รากผู้เล่น |
| Enemy | ศัตรู AI |

#### Layers
| Layer | ใช้กับ |
|-------|--------|
| Ground | พื้นเดิน |
| Player | ผู้เล่น (optional) |
| Enemy | ศัตรู |
| Hitbox | melee trigger ชั่วคราว |

#### Scene: `Assets/FPSGame/Scenes/PrototypeGameplay.unity`
| Object | Setup |
|--------|-------|
| Ground | Plane, Layer `Ground`, Collider |
| Directional Light | rotation ~(50, -30, 0) |
| EventSystem | EventSystem + StandaloneInputModule |
| HUD_Canvas | Canvas (Screen Space Overlay) + CanvasScaler + GraphicRaycaster |
| PlayerSpawn | Empty transform at (0, 1, 0) — ลาก Player prefab ตรงนี้ใน Phase 1 |

### Scripts ที่เพิ่มใน Phase 0 (vault)
| File | หน้าที่ |
|------|---------|
| `Scripts/Editor/FPSGameProjectSetup.cs` | Menu bootstrap Tags/Layers/Folders/Scene |
| `Scripts/FPSGame.Runtime.asmdef` | Runtime assembly |
| `Scripts/Editor/FPSGame.Editor.asmdef` | Editor assembly |

### Portability (ย้ายเครื่อง)
- [ ] Copy ทั้ง Unity project folder (ไม่ใช่แค่ Scripts)
- [ ] Commit `.meta` คู่ asset ทุกตัวใน `Assets/FPSGame/`
- [ ] บันทึก `Packages/manifest.json`
- [ ] ดู `template/UnityBootstrap/README.md` สำหรับ project ใหม่

### ทดสอบ Phase 0
- [ ] Compile ผ่าน — ไม่มี error จาก FPSGame assemblies
- [ ] Tags/Layers ครบตามตาราง
- [ ] เปิด PrototypeGameplay — Ground layer ถูกต้อง, มี PlayerSpawn
- [ ] Play mode — scene โหลดได้ (ยังไม่มี Player จนกว่า Phase 1)

### Portability checklist (ทุกครั้งก่อนย้ายเครื่อง)
- [ ] Commit ไฟล์ `.meta` คู่กับ asset ทุกตัวใน `Assets/FPSGame/`
- [ ] เปิด prefab Player — ไม่มี Missing Script
- [ ] เปิด Scene prototype — ไม่มี reference สีแดงใน Inspector
- [ ] บันทึก `Packages/manifest.json` ใน git
- [ ] Zip หรือ clone ทั้งโฟลเดอร์ Unity project (ไม่ใช่แค่ Scripts)

---

## Phase 1 — Character Base (หัวข้อ 1) — 2025-06-20

### Prerequisites
- Phase 0 complete (folders, tags, layers)
- Scripts compiled (`FPSGame.Runtime`, `FPSGame.Editor`)

### วิธีที่ 1 — อัตโนมัติ (แนะนำ)

เมนู: **FPSGame → Setup Phase 1 (Character + Player Prefab)**

สร้างให้อัตโนมัติ:
- `Assets/FPSGame/Data/Characters/Hero_Default.asset`
- `Assets/FPSGame/Prefabs/Player/Player.prefab`
- วาง Player ใน `PrototypeGameplay` ที่ `PlayerSpawn`

### วิธีที่ 2 — มือ

#### ScriptableObject
| Asset path | Create menu | ค่าเริ่มต้น |
|------------|-------------|------------|
| `Assets/FPSGame/Data/Characters/Hero_Default.asset` | FPSGame/Character Stats | characterId=`hero_default`, maxHealth=100, walkSpeed=5, faction=Friendly |

> Faction อยู่ที่ `CharacterStats.faction` (ไม่ใช่ field บน CharacterBase)

#### Prefab: `Assets/FPSGame/Prefabs/Player/Player.prefab`

| GameObject | Component | Field | มอบหมาย |
|------------|-----------|-------|---------|
| Player (root) | CharacterBase | stats | Hero_Default.asset |
| Player (root) | CharacterController | height/radius | 2 / 0.4, center Y=1 |
| Player (root) | PlayerController | cameraPivot / weaponHandler | Camera / WeaponMount |
| Player (root) | CharacterDamageTester | — | prototype test only |
| Player/Camera | Camera | tag | MainCamera |
| Player/WeaponMount | WeaponHandler | — | wire weapons in Phase 2 |

Hierarchy:
```
Player (tag Player, layer Player)
├── Camera (local Y=1.6)
└── WeaponMount (local 0, 1.4, 0.4)
```

#### Animator (placeholder — Phase 2+)
| Field | ค่า |
|-------|-----|
| Base Controller | `PlayerBase.controller` (สร้างภายหลัง) |
| Override (optional) | `PlayerOverride.overrideController` |

Lower-body Blend Tree parameters แนะนำ: `MoveX`, `MoveY`, `Speed`

#### Scene: `PrototypeGameplay.unity`
| Object | การตั้งค่า |
|--------|------------|
| Player | prefab instance ที่ PlayerSpawn |
| Ground | Plane + Layer Ground |
| Directional Light | ค่าเริ่มต้น |
| EventSystem + Canvas | สำหรับ HUD (Phase 4) |

### Scripts ที่เพิ่มใน Phase 1 (vault)
| File | หน้าที่ |
|------|---------|
| `Core/CharacterBase.cs` | TakeDamage, Heal, Die, Revive, events, delay HP |
| `Core/CharacterDamageTester.cs` | K/H/Y test keys (prototype) |
| `Editor/FPSGamePhase1Setup.cs` | Menu สร้าง asset + prefab |

### CharacterBase API (สรุป)
| Member | หน้าที่ |
|--------|---------|
| `TakeDamage(float)` | ลด HP ทันที → `OnHealthChanged` → `Die()` ถ้า HP=0 |
| `Heal(float)` | เพิ่ม HP (ไม่เกิน max) → `OnHealed` |
| `Revive(bool full)` | ฟื้นหลังตาย |
| `IsAlive` | false หลัง Die จนกว่า Revive |
| `OnDied` / `OnRevived` | hook สำหรับ UI / game flow |

### ทดสอบ Phase 1
- [ ] Play → กด **K** → Primary HP ลดทันที (ดู Inspector `CurrentHealth`)
- [ ] กด **H** → HP เพิ่ม (ไม่เกิน 100)
- [ ] HP = 0 → `IsAlive` false, `OnDied` fire
- [ ] กด **Y** → Revive กลับเต็ม
- [ ] `DisplayDelayHealth` ค่อยๆ ตาม Primary (delay bar logic พร้อม — UI ใน Phase 4)
- [ ] Prefab ไม่มี Missing Script

### Portability
- [ ] Commit `Hero_Default.asset` + `.meta` + `Player.prefab` + `.meta`
- [ ] ลบหรือ disable `CharacterDamageTester` ก่อน build จริง

---

## Phase 2 — Player Controller — 2025-06-20

### Prerequisites
- Phase 1 complete (Player prefab + CharacterBase)
- Input: **Legacy Input Manager** (default Unity axes: Horizontal, Vertical, Jump, Mouse X/Y, Fire1)

### วิธีที่ 1 — อัตโนมัติ (แนะนำ)

เมนู: **FPSGame → Setup Phase 2 (Player Controller)**

อัปเกรด `Player.prefab`:
- ลบ Rigidbody + CapsuleCollider (legacy Phase 1)
- เพิ่ม `CharacterController` + `PlayerController`
- wire Camera → `cameraPivot`, WeaponMount → `WeaponHandler.muzzle`

> Phase 1 setup ใหม่จะรวม Phase 2 อัตโนมัติแล้ว — ใช้เมนูนี้ถ้ามี prefab เก่าจาก Phase 1

### วิธีที่ 2 — มือ

#### Prefab: `Player.prefab` (root)

| Component | Field | ค่า |
|-----------|-------|-----|
| CharacterController | height / radius / center | 2 / 0.4 / (0, 1, 0) |
| PlayerController | cameraPivot | Camera transform |
| PlayerController | weaponHandler | WeaponMount → WeaponHandler |
| PlayerController | mouseSensitivity | 2 |
| PlayerController | aimMoveSpeedMultiplier | 0.5 |

#### Input mapping (Legacy)
| Input | การทำงาน |
|-------|----------|
| WASD | เดิน (ทิศตาม camera yaw) |
| Shift | sprint (`CharacterStats.sprintSpeed`) |
| Space | jump (`CharacterStats.jumpHeight`) |
| Mouse | look (pitch บน Camera, yaw บน Player root) |
| LMB (Fire1) | `WeaponHandler.TryPrimaryAction()` |
| RMB | aim — ช้าลง (`aimMoveSpeedMultiplier`) |
| Scroll / 1–2–3 | เปลี่ยน weapon slot |

### Scripts ที่เพิ่มใน Phase 2 (vault)
| File | หน้าที่ |
|------|---------|
| `Core/PlayerController.cs` | FPS input + movement + weapon commands |
| `Editor/FPSGamePhase2Setup.cs` | อัปเกรด prefab |
| `Weapons/WeaponHandler.cs` | `CycleActiveSlot()` สำหรับ scroll |

### OOP note
`PlayerController` **ไม่สืบ** `CharacterBase` — อยู่บน GameObject เดียวกัน (composition) ตาม architecture lock  
AI จะใช้ `AIController` แทนใน Phase 7

### Animator (optional)
ถ้ามี Animator บน Player — ตั้ง params: `MoveX`, `MoveY`, `Speed` (lower-body blend ภายหลัง)

### ทดสอบ Phase 2
- [ ] Play → cursor lock, mouse look ได้
- [ ] WASD เดินบน Ground, Shift วิ่งเร็วขึ้น
- [ ] Space กระโดด (jumpLimit จาก Hero_Default)
- [ ] RMB กดค้าง → เดินช้าลง
- [ ] LMB เรียก WeaponHandler (ยังไม่ยิงจริงจนกว่า Phase 3 ใส่ weapon asset)
- [ ] Scroll / 1–2–3 เปลี่ยน slot ไม่ error
- [ ] ตายแล้ว (K) → หยุดเคลื่อนที่

### Portability
- [ ] Commit `Player.prefab` + `.meta` หลัง upgrade
- [ ] บันทึก Input choice ใน UnitySetup Global section

---

## Phase 3 — Weapon Runtime (หัวข้อ 2) — 2025-06-20

### Prerequisites
- Phase 2 complete (PlayerController + WeaponHandler on prefab)
- Layers: `Enemy`, `Hitbox`, `Ground`

### วิธีที่ 1 — อัตโนมัติ (แนะนำ)

เมนู: **FPSGame → Setup Phase 3 (Weapons)**

สร้างและ wire:
| Asset | Path |
|-------|------|
| Pistol (Hitscan) | `Data/Weapons/pistol_hitscan.asset` |
| Knife (Melee) | `Data/Weapons/knife_melee.asset` |
| Bullet hole placeholder | `Prefabs/Weapons/bullet_hole_placeholder.prefab` |
| Enemy stats | `Data/Characters/Enemy_Default.asset` |
| Test dummy | `Prefabs/Player/TestDummy_Enemy.prefab` @ (5,1,0) |

Player weapon slots: `[0]=pistol`, `[1]=knife`, `[2]=empty`

### วิธีที่ 2 — มือ

#### ScriptableObjects
| Asset | Type | ค่าสำคัญ |
|-------|------|----------|
| `pistol_hitscan.asset` | HitscanWeaponData | clip=12, mags=3, penetration=0, reloadPhase=0.2s |
| `knife_melee.asset` | MeleeWeaponData | activeTime=0.01s, damage=50 |

#### Prefab Player — WeaponMount
| Field | ค่า |
|-------|-----|
| weaponSlots[0] | pistol_hitscan |
| weaponSlots[1] | knife_melee |
| muzzle | WeaponMount transform |

### Runtime features (WeaponHandler)
| Feature | พฤติกรรม |
|---------|----------|
| Ammo | clip + reserve magazines ต่อ slot (ranged เท่านั้น) |
| Reload | 3 phase: Eject → Insert → Chamber (~0.2s/phase) |
| Reload cancel | กด LMB ระหว่าง reload → ยกเลิก |
| Reload dump | ทิ้งกระสุนในแมกปัจจุบันเมื่อเริ่ม reload |
| Hitscan delay | 0.01s + 0.01s/100m เกิน reference range |
| Penetration | units เท่านั้น; count = penetration+1 targets |
| Bullet hole | spawn prefab เมื่อชนสิ่งกีดขวาง (ไม่ใช่ unit) |
| Melee | trigger hitbox ชั่วคราว, 1 hit/target/swing |

### Input (PlayerController)
| Key | การทำงาน |
|-----|----------|
| LMB | fire / melee |
| R | reload (ranged only) |
| 1 / 2 / 3 | weapon slot |
| Scroll | cycle slot |

### Scripts ที่เพิ่มใน Phase 3 (vault)
| File | หน้าที่ |
|------|---------|
| `Weapons/WeaponHandler.cs` | ammo, reload FSM, hitscan+penetration, melee |
| `Weapons/MeleeHitbox.cs` | anti-double-hit trigger |
| `Weapons/WeaponSlotState.cs` | runtime ammo struct |
| `Weapons/ReloadPhase.cs` | reload enum |
| `Editor/FPSGamePhase3Setup.cs` | สร้าง assets + test dummy |

### ทดสอบ Phase 3
- [ ] LMB ยิง pistol → TestDummy HP ลด (delay เล็กน้อย)
- [ ] ชนผนัง/Ground → bullet hole placeholder ปรากฏ
- [ ] R reload → 3 phase ~0.6s, reserve mag ลด 1
- [ ] clip หมด → auto พยายาม reload
- [ ] Slot 2 (knife) + LMB → melee hitbox, dummy โดน 1 ครั้ง/swing
- [ ] penetration=0 → กระสุนหยุดที่ศัตรูตัวแรก

### Portability
- [ ] Commit weapon `.asset` + bullet hole prefab + `.meta`
- [ ] TestDummy เป็น prototype — ลบหรือแทนด้วย AI Phase 7

---

## Phase 4 — HUD Gameplay — 2025-06-20

### Prerequisites
- Phase 3 complete (weapons wired)
- `com.unity.ugui` ใน project (ดู `UnityBootstrap/manifest.json.example`)
- `FPSGame.Runtime.asmdef` references `Unity.UGUI`

### วิธีที่ 1 — อัตโนมัติ (แนะนำ)

เมนู: **FPSGame → Setup Phase 4 (HUD)**

สร้างและวาง:
| Item | Path / ตำแหน่ง |
|------|----------------|
| GameplayHUD prefab | `UI/HUD/GameplayHUD.prefab` |
| Scene instance | child ของ `HUD_Canvas` ใน PrototypeGameplay |
| PlayerWallet | component บน Player.prefab |

### HUD layout
| Widget | ตำแหน่ง | ข้อมูล |
|--------|---------|--------|
| DualLayerHealthBarUI | ล่างซ้าย | Primary (ทันที) + Delay (ตาม CharacterBase) |
| WeaponSlotHudUI | ล่างขวา | slot 1–3, highlight active, ammo `[clip\|reserve]` |
| WalletHudUI | บนขวา | `$ {balance}` จาก PlayerWallet |

### Scripts ที่เพิ่มใน Phase 4 (vault)
| File | หน้าที่ |
|------|---------|
| `UI/GameplayHudManager.cs` | bind HUD → Player |
| `UI/DualLayerHealthBarUI.cs` | HP สองชั้น |
| `UI/WeaponSlotHudUI.cs` | weapon slot display |
| `UI/WalletHudUI.cs` | เงิน |
| `Core/PlayerWallet.cs` | balance + event (Save sync Phase 9) |
| `Editor/FPSGamePhase4Setup.cs` | สร้าง HUD prefab + scene |

### Data flow
```
CharacterBase.OnHealthChanged → Primary bar (ทันที)
CharacterBase.DisplayDelayHealth → Delay bar (Update)
WeaponHandler.OnActiveSlotChanged / OnAmmoChanged → WeaponSlotHudUI
PlayerWallet.OnBalanceChanged → WalletHudUI
```

### ทดสอบ Phase 4
- [ ] Play → wallet แสดง `$ 100` (default)
- [ ] กด K → Primary HP ลดทันที, Delay bar ตามหลัง
- [ ] Scroll / 1–2–3 → slot highlight เปลี่ยน, ชื่ออาวุธ + ammo อัปเดต
- [ ] ยิง → ammo `[clip|reserve]` ลด
- [ ] HUD ไม่มี Missing Script

### Portability
- [ ] Commit `GameplayHUD.prefab` + `.meta`
- [ ] Player prefab มี PlayerWallet

---

## Phase 5 — Pause & Gameplay Flow — 2025-06-20

### Prerequisites
- Phase 4 complete (HUD_Canvas in scene)
- `PrototypeGameplay` ใน Build Settings (Phase 5 menu ใส่ให้อัตโนมัติ)

### วิธีที่ 1 — อัตโนมัติ (แนะนำ)

เมนู: **FPSGame → Setup Phase 5 (Pause)**

สร้างและวาง:
| Item | Path / ตำแหน่ง |
|------|----------------|
| PauseMenu prefab | `UI/HUD/PauseMenu.prefab` |
| Scene instance | child ของ `HUD_Canvas` |
| Build Settings | `PrototypeGameplay.unity` enabled |

### Pause UI
| Control | การทำงาน |
|---------|----------|
| **Pause** button (มุมบนซ้าย) | เปิด/ปิด pause menu |
| **Escape** | toggle pause menu |
| **Resume** | ปิด menu, `Time.timeScale = 1` |
| **Restart** | โหลด scene ปัจจุบันใหม่ |
| **Back to Menu** | โหลด `MainMenu` (Phase 10 — ต้องอยู่ใน Build Settings) |

### Scripts ที่เพิ่มใน Phase 5 (vault)
| File | หน้าที่ |
|------|---------|
| `Core/GameplayPauseService.cs` | `Time.timeScale` + pause depth |
| `Core/GameSceneFlow.cs` | Restart / LoadMainMenu |
| `UI/GameplayPauseUI.cs` | pause panel + buttons |
| `Editor/FPSGamePhase5Setup.cs` | สร้าง UI + Build Settings |

### Systems gated while paused
- `PlayerController` — ไม่รับ input
- `CharacterDamageTester` — ไม่รับ K/H/Y
- Gameplay `Time.time` / `Time.deltaTime` — หยุด (reload, delay HP หยุดด้วย)

### ทดสอบ Phase 5
- [ ] Play → Escape → เวลาหยุด, cursor ปลดล็อก, เมนูแสดง
- [ ] Resume → เล่นต่อ, cursor lock กลับ
- [ ] Restart → scene โหลดใหม่, timeScale = 1
- [ ] Back to Menu → โหลด MainMenu + เปิด Play sub-panel (ต้องรัน Phase 10 ก่อน)

### Portability
- [ ] Commit `PauseMenu.prefab` + `.meta`
- [ ] บันทึก Build Settings ใน git (`ProjectSettings/EditorBuildSettings.asset`)

---

## Phase 6 — Hit Registry (polish) — 2025-06-20

### Prerequisites
- Phase 3 complete (WeaponHandler + TestDummy)
- Layer `Hitbox` สำหรับ melee trigger

### ไม่มี Editor menu — เป็น code refactor

Copy `template/Scripts/` ใหม่แล้ว compile ใน Unity

### Architecture
| Type | หน้าที่ |
|------|---------|
| `IDamageable` | interface รับ damage (`IsAlive`, `Faction`, `TakeDamage`) |
| `CharacterBase` | implements `IDamageable` |
| `HitRegistry` | hitscan delay, ray resolve, penetration, melee anti-double-hit, `TryApplyDamage` |

### Hitscan rules (HitRegistry)
| Rule | พฤติกรรม |
|------|----------|
| Delay | `baseHitDelay` + `extraDelayPer100m` ทุก 100m เกิน `referenceRangeMeters` |
| Penetration | เฉพาะ hostile `IDamageable` / Unit; count = `penetration + 1` |
| Friendly/dead | ข้าม (ไม่กิน penetration) |
| Obstacle | non-trigger collider ที่ไม่ใช่ unit → bullet hole |
| Apply | หลัง delay ผ่าน `HitRegistry.TryApplyDamage` |

### Melee rules (HitRegistry + MeleeHitbox)
| Rule | พฤติกรรม |
|------|----------|
| Active time | ~0.01s (จาก `MeleeWeaponData.activeTime`) |
| Anti-double-hit | `swingId` + `HashSet<IDamageable>` ต่อ swing |
| Trigger | BoxCollider + kinematic Rigidbody |
| Layer | `Hitbox` |

### Scripts ที่เพิ่ม/แก้ใน Phase 6 (vault)
| File | หน้าที่ |
|------|---------|
| `Core/IDamageable.cs` | damage interface |
| `Weapons/HitRegistry.cs` | centralized hit logic |
| `Weapons/MeleeHitbox.cs` | swingId + HitRegistry |
| `Weapons/WeaponHandler.cs` | ใช้ HitRegistry แทน inline logic |
| `Core/CharacterBase.cs` | `: IDamageable` |

### ทดสอบ Phase 6
- [ ] ยิง TestDummy → damage หลัง delay เล็กน้อย
- [ ] ยิงผนัง → bullet hole (ไม่ damage unit ข้างหลังถ้า penetration=0)
- [ ] Knife swing → dummy โดน 1 ครั้งต่อ swing (ไม่ double)
- [ ] penetration=1 asset test (optional) → ทะลุ 2 units

### Portability
- [ ] ไม่มี asset ใหม่ — แค่ recompile scripts

---

## Phase 7 — AI Enemy — 2025-06-20

### Prerequisites
- Phase 3 complete (weapons + Enemy_Default.asset)
- Phase 5 recommended (pause gates AI)
- Navigation: built-in NavMesh (Editor bake)

### วิธีที่ 1 — อัตโนมัติ (แนะนำ)

เมนู: **FPSGame → Setup Phase 7 (AI Enemy)**

สร้างและวาง:
| Item | Path / รายละเอียด |
|------|-------------------|
| Perception config | `Data/AI/EnemyPerception_Default.asset` |
| Enemy prefab | `Prefabs/Enemy/Enemy_AI.prefab` |
| Patrol points | `PatrolPoint_A/B` ใน scene |
| NavMesh | bake อัตโนมัติ (Ground plane) |
| PlayerNoiseEmitter | เพิ่มบน Player.prefab |

ลบ `TestDummy_Enemy` จาก scene (แทนด้วย Enemy_AI)

### Enemy prefab components
| Component | หน้าที่ |
|-----------|---------|
| CharacterBase | Enemy_Default stats |
| NavMeshAgent | pathfinding |
| AiPerception | sight + hearing |
| AIController | FSM Patrol → Chase → Attack |
| WeaponHandler | pistol + knife (slots 0/1) |

### FSM (AIController)
| State | พฤติกรรม |
|-------|----------|
| Patrol | วน `patrolPoints`, รอ perception |
| Chase | `SetDestination` ไล่ target |
| Attack | หยุด, หันหา target, ยิง/มีดตามระยะ |

### Perception (AiPerception)
| Sense | เงื่อนไข |
|-------|----------|
| Sight | ระยะ + FOV + line-of-sight ray |
| Hearing | sprint (`PlayerNoiseEmitter`) หรือ gunshot ล่าสุด |
| Memory | ลืม target หลัง `loseTargetDelay` |

### Scripts ที่เพิ่มใน Phase 7 (vault)
| File | หน้าที่ |
|------|---------|
| `AI/AIController.cs` | FSM + NavMesh |
| `AI/AiPerception.cs` | detection |
| `AI/AiPerceptionConfig.cs` | SO tuning |
| `AI/AiState.cs` | enum |
| `Core/PlayerNoiseEmitter.cs` | sprint/gunshot สำหรับ hearing |
| `Editor/FPSGamePhase7Setup.cs` | prefab + bake |

### ทดสอบ Phase 7
- [ ] Play → Enemy ลาดตระเวน patrol points
- [ ] เดินเข้า FOV → Chase
- [ ] Shift sprint ใกล้ๆ (นอก FOV) → ได้ยิน → Chase
- [ ] ยิง → ได้ยิน → Chase
- [ ] เข้าระยะโจมตี → Enemy ยิงกลับ
- [ ] ใกล้มาก → สลับมีด (slot 2)
- [ ] Enemy ตาย → หยุดเคลื่อนไหว

### Portability
- [ ] Commit `Enemy_AI.prefab` + perception asset + `.meta`
- [ ] Scene มี NavMesh data (หลัง bake)

---

## Phase 8 — Enemy Spawn System — 2025-06-20

### Prerequisites
- Phase 7 complete (`Enemy_AI.prefab`)
- NavMesh baked on Ground

### วิธีที่ 1 — อัตโนมัติ (แนะนำ)

เมนู: **FPSGame → Setup Phase 8 (Spawn System)**

สร้างและวาง:
| Item | Path / รายละเอียด |
|------|-------------------|
| Wave sequence | `Data/AI/Wave_Prototype_01.asset` (2 enemies, delay 2s) |
| Spawn point prefab | `Prefabs/Enemy/EnemySpawnPoint.prefab` |
| Scene `SpawnPoint_01` | @ (8, 0, 8) + child patrol points |
| Scene `SpawnTrigger_01` | trigger zone @ (4, 1, 4) |

ลบ `Enemy_AI` / `TestDummy` / `PatrolPoints` จาก scene — **เริ่มฉากไม่มีศัตรู**

### Flow
```
Player enters SpawnTrigger_01
        ↓
EnemySpawnTrigger → EnemySpawnPoint.BeginSpawn()
        ↓
EnemyWaveSequence steps (prefab + count + delay)
        ↓
Instantiate Enemy_AI + NavMeshAgent.Warp + ConfigurePatrol
```

### Scripts ที่เพิ่มใน Phase 8 (vault)
| File | หน้าที่ |
|------|---------|
| `AI/EnemySpawnStep.cs` | step struct (prefab, count, delay) |
| `AI/EnemyWaveSequence.cs` | ScriptableObject sequence |
| `AI/EnemySpawnPoint.cs` | spawn coroutine ที่จุด |
| `AI/EnemySpawnTrigger.cs` | trigger → activate spawn points |
| `AI/AIController.cs` | `ConfigurePatrol()` |
| `Editor/FPSGamePhase8Setup.cs` | scene + assets |

### ทดสอบ Phase 8
- [ ] Play → ไม่มี enemy ใน scene ตอนเริ่ม
- [ ] เดินเข้าโซน trigger (แนว ~ x=4) → enemy spawn 2 ตัว (ห่าง 2s)
- [ ] Enemy patrol รอบ spawn point children
- [ ] Trigger ครั้งเดียว — เข้าโซนซ้ำไม่ spawn ซ้ำ

### Portability
- [ ] Commit wave `.asset` + `EnemySpawnPoint.prefab` + `.meta`

---

## Phase 9 — Save System (Core) — 2025-06-20

### Prerequisites
- Phase 1–3 (hero + weapon assets with ids)
- Phase 4 (PlayerWallet on player)

### วิธีที่ 1 — อัตโนมัติ (แนะนำ)

เมนู: **FPSGame → Setup Phase 9 (Save System)**

สร้าง:
| Asset | Path |
|-------|------|
| Start pack | `Data/Save/StartPack_Default.asset` |
| Content registry | `Data/Save/GameContentRegistry.asset` |
| Scene `SaveSystem` | GameplayProfileBootstrap + SaveProfileDebug |

### JSON files (runtime)
```
persistentDataPath/
  profile_1.json
  profile_2.json
  profile_3.json
```

### ProfileSaveData fields
| Field | หน้าที่ |
|-------|---------|
| `unlockedHeroIds` | ฮีโร่ที่ปลดล็อก |
| `unlockedWeaponIds` | อาวุธที่ปลดล็อก |
| `unlockedLevelKeys` | ด่านที่ปลด (เช่น `c1_l1`) |
| `secretStageUnlocked` | secret stage flag |
| `selectedHeroId` | ฮีโร่ที่เลือก |
| `loadoutWeaponIds[3]` | weaponId ต่อ slot |
| `wallet` | เงิน |

### Start pack defaults
- Heroes: `hero_default`
- Weapons: `pistol_01`, `knife_01`
- Level: `c1_l1` unlocked
- Loadout: pistol + knife
- Wallet: 100

### Scripts (vault)
| File | หน้าที่ |
|------|---------|
| `Save/SaveProfileService.cs` | load/save/unlock API |
| `Save/ProfileSaveData.cs` | JSON model |
| `Save/SaveStartPack.cs` | defaults สำหรับ profile ใหม่ |
| `Save/GameContentRegistry.cs` | id → SO lookup |
| `Save/GameplayProfileBootstrap.cs` | apply loadout ตอนเข้า gameplay |
| `Save/SaveProfileDebug.cs` | F1–F3 / F5 debug |
| `Save/CampaignKeys.cs` | level key helpers |

### Debug keys (prototype)
| Key | การทำงาน |
|-----|----------|
| F1 / F2 / F3 | โหลด profile 1–3 + apply loadout |
| F5 | บันทึก profile ปัจจุบัน |

### ทดสอบ Phase 9
- [ ] Play → profile 1 สร้าง JSON ครั้งแรก
- [ ] Wallet HUD = 100 จาก save
- [ ] Weapon slots ตรง start pack
- [ ] F2 → profile ใหม่ (wallet/loadout default แยก)
- [ ] F5 → JSON อัปเดตบนดิสก์

### Portability
- [ ] Commit StartPack + GameContentRegistry `.asset`
- [ ] JSON อยู่ persistentDataPath (ไม่ commit) — ใช้ debug keys ทดสอบ

---

## Phase 10 — Scene Flow & Main Menu — 2025-06-20

### Prerequisites
- Phase 9 complete (`StartPack_Default.asset` สำหรับ Profile Selection)
- Phase 5 complete (pause → Back to Menu ใช้ `MenuFlowState`)

### วิธีที่ 1 — อัตโนมัติ (แนะนำ)

เมนู: **FPSGame → Setup Phase 10 (Scene Flow)**

สร้าง scenes:
| Scene | Path |
|-------|------|
| Intro | `Scenes/Intro.unity` |
| Profile Selection | `Scenes/ProfileSelection.unity` |
| Main Menu | `Scenes/MainMenu.unity` |

Build Settings (ลำดับ):
1. `Intro.unity`
2. `ProfileSelection.unity`
3. `MainMenu.unity`
4. `PrototypeGameplay.unity`

### Scene flow
```
Intro → (any key) → ProfileSelection → (pick 1–3) → MainMenu
MainMenu → Play → Loadout / Campaign / Prototype Gameplay
Prototype Gameplay → Pause → Back to Menu → MainMenu (Play sub-panel)
```

### Main Menu panels (MenuNavigator + global Back)
| Panel | ปุ่ม / เนื้อหา |
|-------|----------------|
| Main | Play, Settings, Credits, Quit |
| Play | Loadout, Campaign (Phase 12), Prototype Gameplay |
| Settings / Credits | placeholder text |
| Quit confirm | Yes Quit, Cancel |
| Loadout / Campaign | Loadout (Phase 11) / Campaign (Phase 12) |

### Scripts (vault)
| File | หน้าที่ |
|------|---------|
| `Core/GameSceneFlow.cs` | scene names + load helpers |
| `UI/MenuNavigator.cs` | stack navigation + global Back |
| `UI/MenuFlowState.cs` | restore Play sub-panel หลัง pause → menu |
| `UI/IntroSceneController.cs` | skip intro → ProfileSelection |
| `UI/ProfileSelectionController.cs` | profile 1–3 → JSON → MainMenu |
| `UI/MainMenuController.cs` | wire buttons + Prototype Play |
| `Editor/FPSGamePhase10Setup.cs` | สร้าง 3 scenes + Build Settings |

### ทดสอบ Phase 10
- [ ] Play จาก **Intro** (index 0) — ไม่ใช่ PrototypeGameplay
- [ ] Intro → กดปุ่มใดก็ได้ → Profile Selection
- [ ] เลือก Profile → Main Menu แสดง
- [ ] Play → Prototype Gameplay → เล่นได้
- [ ] Pause → Back to Menu → กลับ Main Menu ที่ **Play** sub-panel (Back กลับ Main)
- [ ] Quit confirm → หยุด Play mode (Editor) / ออกแอป (build)

### Portability
- [ ] Commit 3 menu scenes + `.meta`
- [ ] Commit `ProjectSettings/EditorBuildSettings.asset` (ลำดับ 4 scenes)

---

## Phase 11 — Loadout Panel — 2025-06-20

### Prerequisites
- Phase 9 complete (`GameContentRegistry`, `StartPack_Default`)
- Phase 10 complete (`MainMenu.unity`)

### วิธีที่ 1 — อัตโนมัติ (แนะนำ)

เมนู: **FPSGame → Setup Phase 11 (Loadout Panel)**

สร้าง/อัปเดต:
| Item | Path |
|------|------|
| Loadout item prefab | `UI/Menu/LoadoutItem.prefab` |
| Loadout UI | ภายใน `Scenes/MainMenu.unity` → `LoadoutPanel` |

### Loadout UI layout
| Area | การใช้งาน |
|------|-----------|
| Heroes (ซ้าย) | คลิกเลือก hero ที่ปลดล็อกแล้ว → `selectedHeroId` |
| Center slots 1–3 | ลากอาวุธมาวาง → `loadoutWeaponIds[]` |
| Unequip pool | ลากจาก slot มาวาง → ล้าง slot |
| Weapons (ขวา) | ลากอาวุธที่ปลดล็อกไปใส่ slot |

การบันทึก: auto-save ไป `profile_N.json` ทุกครั้งที่เปลี่ยน loadout

### Scripts (vault)
| File | หน้าที่ |
|------|---------|
| `UI/Loadout/LoadoutPanelController.cs` | populate grids, drag/drop logic, save |
| `UI/Loadout/LoadoutItemView.cs` | hero/weapon row |
| `UI/Loadout/LoadoutDragHandle.cs` | drag ghost |
| `UI/Loadout/LoadoutDropTarget.cs` | slot + unequip drop |
| `UI/Loadout/LoadoutDragSession.cs` | drag payload |
| `Save/GameplayProfileBootstrap.cs` | ใช้ profile ที่เลือกจากเมนู (ไม่ reload profile 1 ทับ) |
| `Editor/FPSGamePhase11Setup.cs` | สร้าง prefab + wire MainMenu |

### ทดสอบ Phase 11
- [ ] Intro → Profile 1 → Main Menu → Play → **Loadout**
- [ ] คลิก hero → label "Selected hero" อัปเดต
- [ ] ลาก Pistol/Knife ไป slot 1–3 → JSON เปลี่ยน (F5 หรือดูไฟล์)
- [ ] Prototype Gameplay → weapon slots ตรง loadout ที่ตั้งในเมนู
- [ ] Profile 2 แยก loadout จาก Profile 1

### Portability
- [ ] Commit `LoadoutItem.prefab` + อัปเดต `MainMenu.unity`

---

## Phase 12 — Campaign & Level Select — 2025-06-20

### Prerequisites
- Phase 9 complete (JSON unlock keys: `c1_l1` …)
- Phase 10 complete (`MainMenu.unity`)

### วิธีที่ 1 — อัตโนมัติ (แนะนำ)

เมนู: **FPSGame → Setup Phase 12 (Campaign Select)**

สร้าง/อัปเดต:
| Item | Path |
|------|------|
| Level button prefab | `UI/Menu/CampaignLevelButton.prefab` |
| Campaign UI | `MainMenu.unity` → `CampaignPanel` |

### Campaign flow
```
Play → Campaign → Campaign 1–4 → Level 1–4 (locked/unlocked)
→ Prototype Gameplay (MVP scene สำหรับทุกด่าน)
Pause → Back to Menu → กลับ Campaign level list เดิม
```

### Unlock rules
| Rule | พฤติกรรม |
|------|----------|
| Linear | ชนะด่าน n → `UnlockNextLevelAfterWin` ปลด n+1 (หรือ campaign ถัดไป) |
| Start pack | profile ใหม่เริ่มที่ `c1_l1` unlocked |
| Secret stage | `secretStageUnlocked` ใน JSON + key `secret_c{n}` |

### Scripts (vault)
| File | หน้าที่ |
|------|---------|
| `UI/Campaign/CampaignPanelController.cs` | campaign + level UI |
| `UI/Campaign/CampaignSessionState.cs` | จำ campaign ตอน pause → menu |
| `UI/MenuFlowState.cs` | restore campaign level list |
| `UI/MenuNavigator.cs` | sub-back จาก level list |
| `Save/SaveProfileService.cs` | `UnlockSecretStage`, `IsSecretStageAvailable` |
| `Save/CampaignKeys.cs` | `c{n}_l{m}`, `secret_c{n}` |
| `Editor/FPSGamePhase12Setup.cs` | wire MainMenu |

### Debug keys (gameplay)
| Key | การทำงาน |
|-----|----------|
| F6 | ปลด Secret Stage (`secretStageUnlocked` + keys) |

### ทดสอบ Phase 12
- [ ] Profile ใหม่ → Campaign 1 → Level 1 เล่นได้, Level 2–4 ล็อก
- [ ] เลือก Level 1 → Prototype Gameplay โหลดได้
- [ ] Pause → Back to Menu → กลับ **Campaign X level list** (ไม่ใช่ Play root)
- [ ] Global Back จาก level list → campaign list | จาก campaign list → Play
- [ ] F6 ใน gameplay → Secret Stage โผล่ใน level list

### Portability
- [ ] Commit `CampaignLevelButton.prefab` + อัปเดต `MainMenu.unity`

---

## บันทึกเพิ่ม (Agent เติมต่อท้ายไฟล์นี้เมื่อทำฟีเจอร์ใหม่)

<!-- /goal skill: append new sections below this line -->
