# FPS Game Template (Vault)

โค้ดและแผนสำหรับเกม FPS — ทำงานร่วมกับ Cursor skill **`/goal`**

| โฟลเดอร์ | use |
|----------|------|
| `Plan/Plantext.txt` | แผนระบบ (design source of truth) |
| `Plan/UnitySetup.md` | ขั้นตอน Inspector + ย้ายเครื่อง |
| `Scripts/` | C# เป้าหมาย → copy ไป `Assets/FPSGame/Scripts/` ใน Unity |

## วิธีใช้

1. **Phase 0** — สร้าง Unity project → copy `Scripts/` → เมนู **FPSGame → Setup Phase 0** (ดู `UnityBootstrap/README.md`)
2. ใน Cursor พิมพ์ `/goal` แล้วบอกหัวข้อที่จะทำ (เช่น "ทำ Phase 1 Character base")
3. Agent จะเขียนโค้ดใน `Scripts/`, อัปเดต `Plantext.txt` ถ้าจำเป็น, และเติม `UnitySetup.md`
4. Copy `template/Scripts` ไป Unity project ตาม path ใน UnitySetup

## อ้างอิง (ไม่ copy ตรง)

- `_DraftCodesOnly/` — โค้ดอ้างอิงจาก template หลายโปรเจกต์
- `source/` — ต้นฉบับเต็ม (มี Packages/Plugins ปน)
