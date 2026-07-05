# Server Defender (Web Version) - Kiến trúc Hệ thống & Thiết kế Kỹ thuật

Tài liệu này trình bày chi tiết về kiến trúc hệ thống tổng thể (high-level), luồng dữ liệu (data pipelines), và thiết kế hướng đối tượng (OOP design) cho tựa game **Server Defender**, được tối ưu hóa riêng cho Trình duyệt Web (Unity WebGL).

## 1. High-Level System Design (Thiết kế Hệ thống Tổng quan)

Kiến trúc được thiết kế để chạy hoàn toàn bên trong trình duyệt web của người dùng. Hệ thống được tách rời (decoupled) thành hai layer (tầng) chính chạy trên cùng một trang web, và được hỗ trợ bởi một Cloud database.

### Data Flow Diagram (Sơ đồ Luồng Dữ liệu)

```text
[ Không gian Thực ]      [ Browser: Web Worker (worker.js) ]       [ Browser: Unity WebGL (WASM) ]    [ Cloud Layer: Firebase ]
       |                                |                                       |                               |
(Bàn tay User) --Webcam--> [ MediaPipe JS Library ]                             |                               |
                                        |                                       |                               |
                                        v                                       |                               |
                                [ Data Formatter ]                              |                               |
                                        |                                       |                               |
                                        v                                       |                               |
                          [ Global JS Buffer (index.html) ] <--(.jslib Read)-- [ WebGL Receiver C# ]            |
                                                                                |                               |
                                                                                v                               |
                                                                      [ Game State Machine ]                    |
                                                                                |                               |
                                                                                v                               |
                                                                      [ Game Over Event ] --(REST POST)--> [ Realtime DB ]
```

**Giải thích:**
1. Người dùng truy cập trang web. JavaScript yêu cầu quyền truy cập (permissions) webcam.
2. Thư viện MediaPipe của Google (phiên bản JavaScript) sẽ xử lý hình ảnh webcam trên một luồng chạy ngầm (Web Worker) độc lập với game.
3. JavaScript định dạng (formats) tọa độ bàn tay thành một chuỗi (string) đơn giản và lưu vào biến Global.
4. Game Unity WebGL sử dụng plugin `.jslib` để đọc trực tiếp chuỗi này từ bộ nhớ của trình duyệt, phân tích nó, và cập nhật vị trí con trỏ (cursor) trong game.
5. Khi game kết thúc (Game Over), Unity gửi một HTTP POST request đến Firebase Realtime Database REST API để lưu điểm số.

---

## 2. CV to Unity Pipeline (Đường ống dữ liệu từ CV sang Unity)

### Protocol Choice (Lựa chọn Giao thức): Unity `.jslib` Plugin & Web Worker
Vì WebGL chạy trong môi trường sandbox của trình duyệt, việc sử dụng UDP sockets là bất khả thi. Để tránh việc Main Thread bị chiếm dụng và hiện tượng rớt FPS do Garbage Collection (GC spikes), chúng ta áp dụng một kiến trúc cực kỳ tối ưu:

1.  **Web Worker:** MediaPipe chạy trong một luồng nền riêng biệt (`worker.js`). Nó sẽ gửi kết quả tọa độ (coordinates) về một biến toàn cục (global variable) nằm ở file `index.html`.
2.  **`.jslib` Interop:** Thay vì dùng hàm `unityInstance.SendMessage(JSON)` quá chậm và sinh ra nhiều rác bộ nhớ, chúng ta tạo một C# `.jslib` plugin. Unity sẽ gọi một hàm C# `[DllImport("__Internal")]` trong vòng lặp `Update()`. Hàm này sẽ đọc trực tiếp giá trị của biến global từ bộ nhớ trình duyệt mà không cần thông qua bước ép kiểu nặng nề.

### Data Structure (Cấu trúc Dữ liệu: Raw String)
Để loại bỏ hoàn toàn chi phí xử lý JSON (JSON parsing overhead) trong C#, Web Worker sẽ định dạng dữ liệu thành một chuỗi thô phân cách bằng dấu phẩy (comma-separated raw string), ví dụ: `X,Y,isPinching`.

```text
// Cấu trúc: RightX,RightY,RightPinch,LeftX,LeftY,LeftPinch
"0.45,0.62,0,0.15,0.80,1"
```
*C# sẽ nhận chuỗi này, cắt (split) bằng dấu `,`, và parse thành kiểu floats với chi phí cấp phát bộ nhớ (memory allocation overhead) gần như bằng không.*

---

## 3. Core Class Diagram / OOP Structure (Cấu trúc OOP)

Việc triển khai code Unity tuân thủ nghiêm ngặt các nguyên tắc Lập trình hướng đối tượng (OOP principles).

### Key Classes & Patterns (Các Class và Design Pattern chính)

*   **`GameManager` (Pattern: Singleton / State Machine)**
    *   **Role (Vai trò):** Bộ não của game. Quản lý trạng thái toàn cầu (`MainMenu`, `Playing`, `GameOver`), bộ đếm thời gian (session timer), và tổng điểm.

*   **`WebGLHandReceiver` (Pattern: Facade)**
    *   **Role:** Một MonoBehaviour làm nhiệm vụ giao tiếp với trình duyệt thông qua plugin `.jslib`. Trong hàm `Update()`, nó sẽ gọi hàm JS bên ngoài `GetHandDataString()` và parse chuỗi thô (raw string) thành một `struct` dữ liệu dùng chung.

*   **`HandCursor2D` (Pattern: Observer)**
    *   **Role:** Đọc data mới nhất từ `WebGLHandReceiver` trong mỗi khung hình `Update()`.
    *   **Logic:** Cập nhật vị trí `Transform`. Tự động tính toán vận tốc (velocity) bên trong C# bằng cách so sánh tọa độ với frame trước đó. Đánh giá vận tốc này để kích hoạt một `CircleCollider2D` tạm thời dùng cho cú "Tát" (Slapping).

*   **`ObjectPooler` (Pattern: Object Pool)**
    *   **Role:** Khởi tạo sẵn (Pre-instantiates) một danh sách `List<GameObject>` chứa các con Bug. Đây là pattern **Cực Kỳ Quan Trọng** đối với hiệu năng WebGL để tránh những đợt khựng hình do Garbage Collection khi tạo/xóa object liên tục.

*   **`BugSpawner`**
    *   **Role:** Xử lý logic sinh ra quái (spawn logic) và yêu cầu object từ kho `ObjectPooler`.

*   **`MalwareBug` (Base Class) / `WormBug`, `TrojanBug` (Derived Classes - Tính Kế thừa)**
    *   **Role:** Tượng trưng cho kẻ địch (enemies). Chứa các trường dữ liệu (fields) như `health`, `speed`, `scoreValue`.

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
