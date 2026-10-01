# Wilds Forge (Windows 11)

แอปค้นหาชุดเกราะและคำนวณดาเมจ Monster Hunter Wilds ที่ทำงานออฟไลน์บน Windows 11 สร้างด้วย WPF / .NET 10 มีแท็บ **Build Planner**, **Damage Calculator** และ **Info** แอปไม่ใช้บริการออนไลน์ระหว่างการค้นหาหรือคำนวณ

## เปิดแอป

เปิด `release/win-x64/MHWsTool.exe` ได้ทันที ไฟล์นี้รวม .NET runtime ไว้แล้ว การเปิดครั้งแรกอาจใช้เวลาสั้น ๆ เพราะ .NET แตกไฟล์ประกอบลงโฟลเดอร์ชั่วคราว

แท็บ Damage Calculator ใช้ Microsoft Edge WebView2 Runtime ซึ่งเครื่อง Windows 11 ส่วนใหญ่มีอยู่แล้ว หากเครื่องยังไม่มี แอปจะแสดงข้อความพร้อมปุ่ม Retry ให้ติดตั้ง [WebView2 Runtime จาก Microsoft](https://developer.microsoft.com/microsoft-edge/webview2/) แล้วลองใหม่ การคำนวณไม่ต้องใช้อินเทอร์เน็ตหลังติดตั้ง Runtime

## วิธีใช้

1. เลือก High Rank หรือ Low Rank แล้วกดชื่อที่ต้องการจากตาราง **Choose from catalog**: **Weapon**, **Armor**, **Set / Series** และ **Group**
2. รายการที่เลือกจะแสดงใน **Selected requirements · set Lv** พร้อมช่อง **Lv ที่ต้องการ** เลือกหลายชื่อร่วมกันแล้วปรับระดับของแต่ละรายการได้
3. กด **SEARCH BUILDS** เพื่อค้นหาชุดเกราะที่มีสกิลครบตามเลเวลที่เลือก

ตารางแสดงชื่อทั้ง 179 รายการ กดชื่อเพื่อเลือกก่อน แล้วกำหนด Lv ในรายการที่เลือกด้านบน ช่อง Lv ใช้รูปแบบเดียวกับ Damage Calculator เมื่อเพิ่มชื่อใหม่จะเริ่มจากระดับแรกที่ใช้ได้ โบนัส Set / Group แสดงจำนวนชิ้นเกราะที่ต้องใช้ในตัวเลือกระดับ กด **×** หรือกดชื่อเดิมในตารางอีกครั้งเพื่อเอารายการออก

ช่องค้นหาชื่อค้นได้ทุกหมวดพร้อมกัน และกรองเฉพาะตารางรายชื่อ รายการที่เลือกกับระดับของแต่ละรายการยังอยู่ให้แก้ได้เสมอ จำนวนคอลัมน์ปรับตามความกว้างหน้าต่าง ปุ่ม Search อยู่ให้ใช้ได้ตลอดโดยไม่ต้องสลับขั้น

ผลลัพธ์แต่ละชุดแสดงเกราะทั้ง 5 ชิ้นกับเครื่องรางทางซ้าย และแสดงสกิลทั้งหมดเป็นป้ายทางขวา ด้านล่างมีสล็อตที่เหลือแยกตามขนาด เพชรตกแต่ง Total Defense และค่าต้าน Fire / Water / Ice / Thunder / Dragon รวมของเกราะ การเปลี่ยนระดับหรือเงื่อนไขจะล้างผลเก่า ให้กด Search อีกครั้ง

เมื่อไม่ได้ใช้ Talisman แบบกรอกสกิลเอง ระบบจะลองเครื่องรางในฐานข้อมูล รวมถึงเครื่องรางที่เคยบันทึกไว้ใน `%APPDATA%\WildsForge\custom-talismans.json`

### สกิลที่ติดมากับอาวุธและ Talisman เพื่อลด Req

- เลือก **Add to: On Weapon** หรือ **On Talisman** แล้วกดชื่อ Skill / Set / Group จากตารางเดียวกัน ตั้งระดับของสกิลปกติใน **Skills on weapon / talisman**
- เลือก **Required** เพื่อกำหนดระดับรวมที่ต้องการ แต่ละรายการจะแสดง **From gear** และ **Need** เช่น ต้องการ Lv 3 มีจากอุปกรณ์รวม Lv 2 จะหาเพิ่มเพียง Lv 1
- โบนัส Set / Group ที่ติดบนอุปกรณ์แต่ละชิ้นนับเป็น **1 piece** ช่วยลดจำนวนชิ้นเกราะที่ต้องใช้ เช่นโบนัสระดับ 2 ต้องใช้ 4 ชิ้น หากมีบนอาวุธและ Talisman อย่างละหนึ่ง จะต้องหาเกราะที่มีโบนัสเดียวกันอีก 2 ชิ้น
- การเพิ่มรายการ On Weapon / On Talisman เปิดโหมด **Use manual … skills** ให้อัตโนมัติ สกิลที่กรอกจะนำไปหัก Req ก่อนค้นหาชุดเกราะ
- เอาเครื่องหมาย **Use manual … skills** ออกเพื่อหยุดนำสกิลที่กรอกไว้ไปหัก Req รายการที่กรอกไว้ยังอยู่ เมื่อใช้ Talisman แบบกรอกเอง ระบบจะยึดสกิลเหล่านี้ในการค้นหา

เมื่อค้นหาชื่อ ตารางจะเลื่อนไปยังชื่อที่ตรงกับคำค้น เมื่อเพิ่มชื่อแล้วจะเลื่อนไปยังช่องระดับของรายการนั้น เพื่อกำหนด Lv ได้ทันที **Clear all settings** ล้างทั้ง Req และสกิลจากอุปกรณ์

## แท็บ Damage Calculator

ระบบคำนวณจาก [MH Wilds Calculator](https://mhwilds-calculator.netlify.app/calc) ฝังไว้ในแอปพร้อมข้อมูลและสูตรต้นฉบับ ปรับเป็นธีม navy / cyan และฟอนต์ Segoe UI ให้เข้ากับหน้าค้นหาชุดเกราะ

1. เลือกประเภทอาวุธจากทั้งหมด 14 ประเภท แล้วกรอก Attack, Affinity, Sharpness และ Element
2. เลือกเลเวลสกิลอาวุธ เกราะ โบนัสเซ็ต/กรุ๊ป และบัฟที่ต้องการ ผล Stats และ Hit / Crit / Avg เปลี่ยนทันที
3. กรอกค่า hitzone หรือกด **Monsters** เพื่อเลือกมอนสเตอร์และส่วนที่โจมตี เปิด **Wound** เมื่อจำลองการโจมตีแผล
4. กด **Combo Builder** แล้วกดท่าที่ต้องการเพิ่มลงคอมโบ ดูผลรวมใน **Combo** ได้ มีโหมด Dynamic และ Snapshot

สลับกลับไป Build Planner แล้วกลับมาคำนวณต่อได้โดยค่าที่กรอกยังอยู่ระหว่างเปิดแอป ค่าเริ่มต้นจะกลับมาเมื่อเปิดแอปใหม่ กรอกค่าสำหรับชุดที่ต้องการจำลองเอง ผลค้นหาชุดเกราะยังไม่ได้ส่งเข้าตัวคำนวณอัตโนมัติ

ไฟล์ตัวคำนวณถูกฝังใน executable และแตกลง `%LOCALAPPDATA%\WildsForge\Calculator\<asset-hash>` เมื่อเปิดแท็บครั้งแรก WebView2 โหลดไฟล์ผ่าน virtual host ภายในเครื่อง ไม่เปิด Netlify และบล็อกคำขอไปยังเว็บภายนอก ข้อมูลตัวคำนวณเป็น snapshot ของซอร์สต้นฉบับ จึงอัปเดตแยกจากฐานข้อมูลค้นหาชุดเกราะ

## แท็บ Info

อ่านรายละเอียด Skill, Set / Series และ Group ครบทั้ง 179 รายการได้ออฟไลน์ เลือกหมวด **All / Skills / Set / Group** แล้วค้นหาด้วยชื่อสกิล ชื่อโบนัสที่เปิดใช้ (เช่น `Fortify`) หรือคำในคำอธิบายผล (เช่น `affinity`)

เมื่อเลือกชื่อ ด้านขวาจะแสดงผลของสกิล วิธีเปิดใช้ ผลแต่ละเลเวล และรายชื่อเซ็ตเกราะที่มีสกิลนั้น สำหรับ Set / Group จะแสดงชื่อโบนัสและจำนวนชิ้นเกราะที่ต้องใช้แยกตามระดับ เช่น 2 / 4 ชิ้น ข้อมูลและคำอธิบายเป็นภาษาอังกฤษตามฐานข้อมูลเกม เก็บคำค้นและรายการที่เลือกไว้เมื่อสลับแท็บระหว่างเปิดแอป

ใช้ [Game8 List of Skills](https://game8.co/games/Monster-Hunter-Wilds/archives/482545) เป็นแนวทางการจัดข้อมูล คำอธิบายที่ฝังในแอปมาจาก [Monster Hunter Wilds DB skills endpoint](https://wilds.mhdb.io/en/skills) พร้อมวัน snapshot ของคำอธิบายในแท็บ Info อัปเดตเฉพาะคำอธิบายโดยคงข้อมูลค้นหาชุดเกราะเดิมได้ด้วย `tools/Import-SkillDetails.ps1` สคริปต์จะตรวจชื่อ ระดับ และจำนวนชิ้นก่อนบันทึก หากโครงสร้างเปลี่ยนให้ใช้ `tools/Import-Data.ps1` อัปเดตฐานข้อมูลทั้งหมด

## ขอบเขตการค้นหา

- แอปใช้การค้นหาแบบ heuristic: เก็บตัวเลือกเกราะที่เกี่ยวข้อง 38 ชิ้นต่อช่อง แล้วจัดอันดับชุดที่น่าจะดีที่สุด จึงอาจพลาดชุดที่เป็นไปได้บางชุด
- ผลลัพธ์แสดงเฉพาะชุดที่มีสกิลครบตามเลเวลที่เลือก (MATCH) หากไม่พบจะไม่แสดงชุดที่ขาดสกิล
- เพชรตกแต่งถือว่ามีจำนวนไม่จำกัด แอปยังไม่เก็บจำนวนเพชรที่ผู้เล่นเป็นเจ้าของ
- กรอกสกิลของเครื่องรางแบบสุ่มผ่าน **On Talisman** แอปไม่ได้สร้างความเป็นไปได้แบบสุ่มทั้งหมด
- ตัวเลข Defense ที่แสดงเป็นค่าเริ่มต้นของเกราะ ไม่รวมการอัปเกรด
- สูตรพิเศษของอาวุธ Artian/Gogma และระบบ Transcended Armor ไม่ได้จำลองครบทุกเงื่อนไข โปรดตรวจชุดในเกมก่อนสร้างจริง

## แหล่งข้อมูล

ฐานข้อมูลที่ฝังในแอปเป็น snapshot ของ [Monster Hunter Wilds DB](https://wilds.mhdb.io/) จาก endpoint เกราะ เพชรตกแต่ง เครื่องราง ทักษะ และอาวุธ ดูวัน snapshot ในแถบล่างของแอป อัปเดตฐานข้อมูลสำหรับ build ใหม่ด้วย PowerShell แบบ native Windows:

```powershell
.\tools\Import-Data.ps1
dotnet build .\MHWsTool.slnx -c Release
```

ชื่อเกม ชื่ออุปกรณ์ และข้อมูลเกมเป็นของเจ้าของสิทธิ์ที่เกี่ยวข้อง แอปนี้เป็นเครื่องมือแฟนเมด ไม่ใช่ผลิตภัณฑ์ของ Capcom หรือ Game8 และไม่คัดลอกโค้ดหรือภาพจาก Game8

ตัวคำนวณใช้ซอร์ส [chanleyou/mhwilds-calculator](https://github.com/chanleyou/mhwilds-calculator) โดย Chan Le You ภายใต้ MIT license ดูข้อความลิขสิทธิ์ใน `third_party/mhwilds-calculator/LICENSE.md` (รวมอยู่ในไฟล์ที่ฝังใน executable ด้วย) และรายละเอียดการปรับใช้ใน `third_party/mhwilds-calculator/WILDS-FORGE.md`

ไอคอนช่องเกราะในผลค้นหานำมาจากซอร์สตัวคำนวณที่ฝังอยู่ ส่วนไอคอนเครื่องราง สล็อต และธาตุใช้ [Font Awesome Free](https://fontawesome.com/license/free) ภายใต้ CC BY 4.0 ดูรายละเอียดใน `MHWsTool/Assets/results/ATTRIBUTION.md`

## พัฒนาและตรวจสอบ

```powershell
dotnet run --project .\MHWsTool\MHWsTool.csproj -c Release
dotnet run --project .\MHWsTool.Checks\MHWsTool.Checks.csproj -c Release
dotnet run --project .\tools\InfoSmoke\InfoSmoke.csproj -c Release
.\tools\PlannerSmoke.ps1
dotnet run --project .\tools\CalculatorSmoke\CalculatorSmoke.csproj -c Release
```

ซอร์สตัวคำนวณอยู่ใน `third_party/mhwilds-calculator` หากแก้ UI / สูตร / ข้อมูลตัวคำนวณ ให้สร้างไฟล์ที่ฝังใหม่ก่อน build .NET (ต้องใช้ Node.js และ npm เฉพาะตอนพัฒนา):

```powershell
.\tools\Build-Calculator.ps1
Push-Location .\third_party\mhwilds-calculator
npm test -- --run test/offline.test.ts
Pop-Location
dotnet build .\MHWsTool.slnx -c Release
```

`test/offline.test.ts` เป็นชุดตรวจสำหรับการฝังในแอปโดยเฉพาะ ชุดทดสอบเก่าจากต้นฉบับ (`index.test.ts`, `builder.test.ts`) ยังใช้ API และชื่อสกิลรุ่นก่อน จึงไม่ได้ใช้เป็นเกณฑ์ผ่านของ integration นี้

สร้าง executable แบบรวม runtime:

```powershell
dotnet publish .\MHWsTool\MHWsTool.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o .\release\win-x64
```
