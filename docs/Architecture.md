# Server Defender (Web Version) - Kiến trúc Hệ thống & Thiết kế Kỹ thuật

Tài liệu này trình bày chi tiết về kiến trúc hệ thống tổng thể (high-level), luồng dữ liệu (data pipelines), và thiết kế hướng đối tượng (OOP design) cho tựa game **Server Defender**, được tối ưu hóa riêng cho Trình duyệt Web (Unity WebGL).

## 1. High-Level System Design (Thiết kế Hệ thống Tổng quan)

Kiến trúc được thiết kế để chạy hoàn toàn bên trong trình duyệt web của người dùng. Hệ thống được tách rời (decoupled) thành hai layer (tầng) chính chạy trên cùng một trang web, và được hỗ trợ bởi một Cloud database.

### Data Flow Diagram (Sơ đồ Luồng Dữ liệu)

```text
[ Không gian Thực ]      [ Browser: Main Thread (index.html) ]        [ Browser: Unity WebGL (WASM) ]    [ Cloud Layer: Firebase ]
       |                                |                                       |                               |
(Bàn tay User) --Webcam--> [ MediaPipe Hands JS (Main Thread) ]                |                               |
                            [ WASM inference chạy tách biệt ]                  |                               |
                                        |                                       |                               |
                                        v                                       |                               |
                            [ latestHandData = "X,Y,P" ]                        |                               |
                            [ Global JS Variable ]  <---(.jslib Read)--- [ WebGLHandReceiver.cs ]               |
                                                                                |                               |
                                                                                v                               |
                                                                      [ HandCursor2D.cs ]                      |
                                                                                |                               |
                                                                                v                               |
                                                                      [ Game State Machine ]                    |
                                                                                |                               |
                                                                                v                               |
                                                                      [ Game Over Event ] --(REST POST)--> [ Realtime DB ]
```

**Giải thích:**
1. Người dùng truy cập trang web. JavaScript yêu cầu quyền truy cập (permissions) webcam.
2. Thư viện MediaPipe Hands (`@mediapipe/hands` v0.4.x) chạy trên **Main Thread** của trình duyệt. Phần ML inference nặng chạy bên trong **WASM runtime** tách biệt, JS chỉ làm orchestration nhẹ (~<1ms/frame). Camera capture throttle ~30fps bằng `setTimeout`.
3. JavaScript định dạng (formats) tọa độ bàn tay thành chuỗi thô `"X,Y,isPinching"` và lưu vào biến Global `latestHandData`.
4. Game Unity WebGL sử dụng plugin `.jslib` (`CVBridge.jslib`) để đọc trực tiếp chuỗi này từ bộ nhớ của trình duyệt, `WebGLHandReceiver.cs` parse chuỗi thành `HandData` struct, và `HandCursor2D.cs` cập nhật vị trí con trỏ (cursor) trong game.
5. Khi game kết thúc (Game Over), Unity gửi một HTTP POST request đến Firebase Realtime Database REST API để lưu điểm số.

> **Lưu ý kiến trúc:** Ban đầu thiết kế dùng Web Worker cho MediaPipe, nhưng thư viện `@mediapipe/hands` v0.4.x sử dụng DOM APIs nội bộ (canvas, WebGL context) nên **không thể chạy trong Web Worker**. Đã chuyển sang Main Thread — validated thành công ngày 2026-07-05.

---

## 2. CV to Unity Pipeline (Đường ống dữ liệu từ CV sang Unity)

### Protocol Choice: Unity `.jslib` Plugin & Main Thread MediaPipe
Vì WebGL chạy trong môi trường sandbox của trình duyệt, việc sử dụng UDP sockets là bất khả thi. Để tránh hiện tượng rớt FPS do Garbage Collection (GC spikes), chúng ta áp dụng kiến trúc tối ưu:

1.  **Main Thread MediaPipe:** `@mediapipe/hands` được load bằng `<script src="mediapipe/hands.js">` trong `index.html`. Toàn bộ binary assets (`.tflite`, `.wasm`, `.data`) được host cục bộ trong thư mục `mediapipe/` (same-origin) để tránh lỗi CORS/MIME type. Camera capture throttle ~30fps bằng `setTimeout`.
2.  **`.jslib` Interop:** Thay vì dùng hàm `unityInstance.SendMessage(JSON)` quá chậm và sinh ra nhiều rác bộ nhớ, chúng ta tạo một C# `.jslib` plugin (`CVBridge.jslib`). Unity sẽ gọi hàm `GetHandDataStringJS()` qua `[DllImport("__Internal")]` trong vòng lặp `Update()` để đọc trực tiếp giá trị của biến global `latestHandData`.

### Data Structure (Cấu trúc Dữ liệu: Raw String)
Để loại bỏ hoàn toàn chi phí xử lý JSON (JSON parsing overhead) trong C#, dữ liệu được định dạng thành chuỗi thô phân cách bằng dấu phẩy (comma-separated raw string):

```text
// Cấu trúc hiện tại (1 tay): X,Y,IsPinching
"0.45,0.62,0"

// Mở rộng tương lai (2 tay): RightX,RightY,RightPinch,LeftX,LeftY,LeftPinch
"0.45,0.62,0,0.15,0.80,1"
```
*C# sẽ nhận chuỗi này, cắt (split) bằng dấu `,`, và parse thành kiểu floats với chi phí cấp phát bộ nhớ (memory allocation overhead) gần như bằng không.*

---

## 3. Core Class Diagram / OOP Structure (Cấu trúc OOP)

Việc triển khai code Unity tuân thủ nghiêm ngặt các nguyên tắc Lập trình hướng đối tượng (OOP principles).

### Key Classes & Patterns (Các Class và Design Pattern chính)

*   **`GameManager` (Pattern: Singleton / State Machine)**
    *   **Role (Vai trò):** Bộ não của game. Quản lý trạng thái toàn cầu (`MainMenu`, `Playing`, `GameOver`), bộ đếm thời gian (session timer), và tổng điểm.

*   **`WebGLHandReceiver` (Pattern: Singleton / Facade)** ✅ *Đã triển khai*
    *   **Role:** Một MonoBehaviour làm nhiệm vụ giao tiếp với trình duyệt thông qua plugin `.jslib`. Trong hàm `Update()`, nó sẽ gọi hàm JS bên ngoài `GetHandDataStringJS()` và parse chuỗi thô (raw string) thành `HandData` struct.
    *   **Editor Fallback:** Trong Unity Editor (không phải WebGL), hỗ trợ mouse input để test.

*   **`HandCursor2D` (Pattern: Observer)** ✅ *Đã triển khai hoàn thiện (Task 2.1)*
    *   **Role:** Đọc data mới nhất từ `WebGLHandReceiver` trong mỗi khung hình `Update()`. Điểm tương tác vật lý chính của người chơi.
    *   **Logic:** Cập nhật vị trí `Transform` với Lerp smoothing. Ánh xạ tọa độ MediaPipe (Y đảo ngược) sang Unity Viewport. Đổi màu khi Pinch hoặc Swipe.
    *   **Vật lý & Tương tác:** Tính toán vận tốc (velocity) dựa trên vị trí cũ/mới để phát hiện hành động "Tát" (Slapping / `isSwiping`). Tích hợp sẵn `CircleCollider2D` (Trigger) và `Rigidbody2D` (Kinematic, Continuous Collision Detection) sẵn sàng cho tương tác với bọ.

*   **`GameManager` (Pattern: Singleton / State Machine)** ✅ *Đã triển khai (Task 2.4)*
    *   **Role:** Bộ não của game. Quản lý trạng thái toàn cầu (`MainMenu`, `Playing`, `GameOver`), timer đếm ngược (mặc định 90s), tổng điểm, và máu server.
    *   **API chính:** `StartGame()`, `AddScore(int)`, `TakeDamage(int)`, `ReturnToMainMenu()`.
    *   **Events:** `OnStateChanged`, `OnScoreChanged`, `OnServerHealthChanged`, `OnTimerUpdated` — để UI và các hệ thống khác lắng nghe.

*   **`ObjectPooler` (Pattern: Object Pool)** ✅ *Đã triển khai (Task 2.2)*
    *   **Role:** Khởi tạo sẵn (Pre-instantiates) một danh sách `List<GameObject>` chứa các con Bug. Pattern **Cực Kỳ Quan Trọng** đối với hiệu năng WebGL để tránh GC spikes.

*   **`BugSpawner`** ✅ *Đã triển khai (Task 2.2)*
    *   **Role:** Xử lý logic sinh ra quái (spawn logic) và yêu cầu object từ kho `ObjectPooler`. Được điều khiển bởi `GameManager` thông qua `ResetAndStart()` / `StopSpawning()`.

*   **`MalwareBug` (Base Class) / `WormBug`, `TrojanBug` (Derived Classes - Tính Kế thừa)** ✅ *Đã triển khai (Task 2.2)*
    *   **Role:** Tượng trưng cho kẻ địch (enemies). Chứa các trường dữ liệu (fields) như `health`, `speed`, `scoreValue`. Khi bị tiêu diệt → `GameManager.AddScore()`. Khi lọt qua phòng tuyến → `GameManager.TakeDamage()`.

*   **`CableEndpoint`** ✅ *Đã triển khai (Task 2.3)*
    *   **Role:** Đầu cáp mạng có thể kéo thả bằng cử chỉ Pinch. Bám theo `HandCursor2D` khi đang Pinch, snap vào `ConnectionPort` đúng ID khi nhả, hoặc spring back về vị trí gốc.

*   **`ConnectionPort`** ✅ *Đã triển khai (Task 2.3)*
    *   **Role:** Cổng kết nối đích (snap target). Chứa `portID` để match với `CableEndpoint.targetPortID`. Visual feedback: xám (idle) → xanh dương (hover) → xanh lá (connected).

*   **`CableRenderer`** ✅ *Đã triển khai (Task 2.3)*
    *   **Role:** Vẽ đường cáp Bézier bậc 2 giữa điểm neo cố định và đầu cáp đang kéo, sử dụng `LineRenderer`.

*   **`CableManager` (Pattern: Singleton)** ✅ *Đã triển khai (Task 2.3)*
    *   **Role:** Theo dõi tổng số cáp đã nối đúng. Fire event `OnAllCablesConnected` khi hoàn thành → `GameManager` thưởng điểm.

### Interaction Resolution (Cơ chế Vật lý và Tương tác)
```csharp
// Nằm trong file HandCursor2D.cs
// QUAN TRỌNG: Rigidbody2D của HandCursor2D BẮT BUỘC phải thiết lập Collision Detection là "Continuous"
void OnTriggerEnter2D(Collider2D other) {
    // Vì chúng ta dùng Continuous Collision Detection (CCD), Unity sẽ bắt được cú tát
    // ngay cả khi bàn tay di chuyển xuyên qua con Bug chỉ trong một frame duy nhất.
    if (isSwiping && other.CompareTag("Bug")) {
        MalwareBug bug = other.GetComponent<MalwareBug>();
        if (bug != null) {
            bug.TakeDamage(100); // One hit kill (Đấm phát chết luôn)
            GameManager.Instance.AddScore(bug.scoreValue);
        }
    }
}
```

---

## 4. Cloud Architecture (Backend Bảng Xếp Hạng)

Với tựa game WebGL, chúng ta sử dụng phương pháp **BaaS (Backend as a Service)** để đạt tiêu chí "Zero Maintenance" (không cần bảo trì server).

### Tech Stack
*   **Database:** Firebase Realtime Database.
*   **Integration:** REST API. (Do Native Firebase Unity SDK hỗ trợ WebGL khá hạn chế và mang tính thử nghiệm, việc dùng lệnh HTTP REST gốc qua `UnityWebRequest` sẽ an toàn và giúp dung lượng bản build nhỏ hơn rất nhiều).

### Deployment Strategy (Chiến lược Triển khai)
1.  **Frontend Hosting:** Toàn bộ bản build Unity WebGL (gồm HTML, JS wrapper, các file `.wasm`) sẽ được upload lên **Firebase Hosting**, **Vercel**, hoặc **GitHub Pages**. Những nền tảng này cung cấp Global CDN và chứng chỉ SSL miễn phí (Lưu ý: Bắt buộc phải có HTTPS thì trình duyệt mới cho phép bật webcam).
2.  **Database Rules:** Cấu hình Security Rules (quy tắc bảo mật) của Firebase để cho phép bất kỳ ai cũng xem được điểm số (read access), nhưng chỉ cho phép ghi dữ liệu (write) nếu nó khớp với schema của cấu trúc điểm số, tránh bị hack.

---

## 5. Unity Hierarchy (Cấu trúc Scene trong Unity Editor)

Dưới đây là cấu trúc chuẩn của Hierarchy trong Unity Scene. Các Empty GameObject có tên đặt trong dấu `[ ]` đóng vai trò làm thư mục (Folder) để gom nhóm các object có chung mục đích.

```text
▼ SampleScene
    Main Camera
  ▼ [MANAGERS]                        ← Thư mục chứa các script quản lý hệ thống
        GameManager                   ← GameManager.cs (Singleton)
        CableManager                  ← CableManager.cs (Singleton, kéo thả cableEndpoints[] và connectionPorts[] vào Inspector)
        BugSpawner                    ← BugSpawner.cs (bugTags: ["Worm", "Trojan"])
        ObjectPooler                  ← ObjectPooler.cs (Pool các prefab Bug)
        ReceiverObject                ← WebGLHandReceiver.cs (Singleton, nhận tọa độ tay từ JS)
  ▼ [GAMEPLAY]                        ← Thư mục chứa các vật thể tương tác trên màn hình
        HandCursor                    ← HandCursor2D.cs + SpriteRenderer + CircleCollider2D (Trigger) + Rigidbody2D (Kinematic)
      ▼ VLAN10                        ← Nhóm 1 bộ cáp mạng (có thể nhân bản thành VLAN20, VLAN30...)
            Endpoint_VLAN10           ← CableEndpoint.cs + CableRenderer.cs + LineRenderer (targetPortID: "VLAN10")
            Anchor_VLAN10             ← Empty GameObject (điểm neo cố định — đầu cáp bắt đầu)
            Port_VLAN10               ← ConnectionPort.cs + SpriteRenderer + CircleCollider2D (portID: "VLAN10")
```

> **Quy tắc sắp xếp Hierarchy và đặt tên:** Xem [`rules.md` — Mục 5: Unity Scene & Hierarchy](rules.md#5-unity-scene--hierarchy-conventions).

### Sơ đồ Tham chiếu (Reference Wiring trong Inspector)

Dưới đây là bảng mô tả các trường tham chiếu (Reference fields) cần kéo thả trong Inspector khi setup scene:

| GameObject | Script | Trường (Field) | Kéo thả (Drag) |
|---|---|---|---|
| `GameManager` | `GameManager.cs` | `bugSpawner` | → `BugSpawner` |
| `GameManager` | `GameManager.cs` | `cableManager` | → `CableManager` |
| `CableManager` | `CableManager.cs` | `cableEndpoints[]` | → Tất cả `Endpoint_*` |
| `CableManager` | `CableManager.cs` | `connectionPorts[]` | → Tất cả `Port_*` |
| `Endpoint_VLAN10` | `CableEndpoint.cs` | `anchorPoint` | → `Anchor_VLAN10` |
| `Endpoint_VLAN10` | `CableRenderer.cs` | *(tự khởi tạo)* | — |

### Cấu trúc Thư mục Code (Script Folder Structure)

```text
Assets/
├── Scripts/
│   ├── CV/                           ← Computer Vision layer
│   │   ├── WebGLHandReceiver.cs      ← Singleton, nhận raw string từ .jslib
│   │   └── HandCursor2D.cs           ← Con trỏ tay, vật lý, gesture detection
│   └── Gameplay/                     ← Core gameplay layer
│       ├── GameManager.cs            ← Singleton điều phối State (MainMenu, Playing) & Mode (Bug, Cable)
│       ├── AudioManager.cs           ← Singleton quản lý BGM và SFX
│       ├── ObjectPooler.cs           ← Object Pool pattern
│       ├── BugSpawner.cs             ← Spawn logic, điều khiển bởi GameManager
│       ├── MalwareBug.cs             ← Base class enemy
│       ├── WormBug.cs                ← Biến thể: di chuyển nhanh
│       ├── TrojanBug.cs              ← Biến thể: nhiều máu
│       ├── CableEndpoint.cs          ← Đầu cáp kéo thả (Pinch)
│       ├── ConnectionPort.cs         ← Cổng snap target
│       ├── CableRenderer.cs          ← Vẽ dây cáp Bézier
│       └── CableManager.cs           ← Quản lý hệ thống cáp
│   ├── UI/                           ← Giao diện người dùng
│   │   └── UIManager.cs              ← Quản lý MainMenu, HUD, GameOver
├── Plugins/
│   └── WebGL/
│       └── CVBridge.jslib            ← JS-to-Unity bridge (raw string)
└── WebGLTemplates/
    └── ServerDefender/
        ├── index.html                ← WebGL template + MediaPipe integration
        └── mediapipe/                ← Local MediaPipe assets (.tflite, .wasm, .data)
```
