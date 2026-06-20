# Unity Bootstrap — Phase 0

ใช้เมื่อสร้าง Unity project ใหม่สำหรับ FPS Game Template

## ขั้นตอน (ครั้งเดียว)

### 1. สร้าง Unity project

1. Unity Hub → **New project**
2. Template: **3D (Built-in)** หรือ **3D (URP)** ตามที่ต้องการ
3. ชื่อโปรเจกต์ เช่น `FPSGame_Unity`
4. เก็บโปรเจกต์แยกจาก vault (แนะนำ) หรือเป็น sibling ของ `MyGameProject_Vault_base`

### 2. Copy โค้ดจาก vault

Copy ทั้งโฟลเดอร์:

```
MyGameProject_Vault_base/template/Scripts/
  →  <UnityProject>/Assets/FPSGame/Scripts/
```

รวม `.asmdef` และโฟลเดอร์ `Editor/`

### 3. รัน Phase 0 ใน Unity Editor

เมนู: **FPSGame → Setup Phase 0 (Project Bootstrap)**

สคริปต์จะสร้างให้อัตโนมัติ:

| รายการ | รายละเอียด |
|--------|------------|
| Folders | `Assets/FPSGame/` ตามโครงใน UnitySetup.md |
| Tags | `Player`, `Enemy` |
| Layers | `Ground`, `Player`, `Enemy`, `Hitbox` |
| Scene | `Assets/FPSGame/Scenes/PrototypeGameplay.unity` |

### 4. Packages (optional แต่แนะนำ)

อ้างอิง `manifest.json.example` ในโฟลเดอร์นี้ — โดยเฉพาะ:

- `com.unity.inputsystem` — ใช้ใน Phase 2 (PlayerController)
- `com.unity.ugui` — HUD ใน Phase 4

บันทึกใน `UnitySetup.md` ว่าเลือก Input System หรือ Legacy

### 5. ทดสอบ Phase 0

- [ ] เมนู **FPSGame → Open PrototypeGameplay Scene** เปิด scene ได้
- [ ] Scene มี Ground, Directional Light, EventSystem, HUD_Canvas, PlayerSpawn
- [ ] Project Settings → Tags and Layers ตรงตามแผน
- [ ] Console ไม่มี compile error จาก `FPSGame.Runtime` / `FPSGame.Editor`

## ขั้นถัดไป

→ **Phase 1** ตาม `Plan/UnitySetup.md` — Character Base + Hero_Default.asset + Player prefab
