# Server Defender (Web Version) - Kế hoạch Phát triển & Lộ trình Thực thi

Tài liệu này vạch ra lộ trình phát triển từng bước cho **Server Defender**, một tựa game tương tác 2D nhịp độ nhanh sử dụng computer vision (CV) để nhận diện cử chỉ tay (hand tracking). Kiến trúc này được thiết kế để chạy hoàn toàn trên trình duyệt web sử dụng Unity WebGL và JavaScript.

## Phase 0: Kế hoạch & Thiết kế Kiến trúc
**Mục tiêu:** Xác định bài toán, lựa chọn công nghệ và thiết kế luồng dữ liệu tối ưu nhất.
- [x] **Task 0.1:** Phân tích yêu cầu và ràng buộc môi trường (sự kiện offline).
- [x] **Task 0.2:** Quyết định kiến trúc hệ thống (Chuyển từ Desktop Python/UDP sang WebGL + JS MediaPipe).
- [x] **Task 0.3:** Lên giải pháp khắc phục rủi ro hiệu năng (Chốt dùng Web Worker và `.jslib` raw string).
- [x] **Task 0.4:** Cấu trúc lại tài liệu dự án (Thư mục `/docs`).

## Phase 1: Môi trường WebGL & Tích hợp CV (Computer Vision)
**Mục tiêu:** Thiết lập foundational tech stack và xác thực luồng giao tiếp cốt lõi giữa JavaScript và Unity WebGL.

### Milestones & Tasks (Cột mốc & Nhiệm vụ)
- [x] **Task 1.1: Khởi tạo Project**
    -   Khởi tạo project Unity 2D (dùng URP - Universal Render Pipeline).
    -   Cài đặt target platform thành WebGL trong Unity Build Settings.
    -   Thiết lập version control (Git).
- [x] **Task 1.2: WebGL Template & MediaPipe Integration**
    -   Tạo một Unity WebGL Template tùy chỉnh (`index.html`).
    -   Tích hợp thư viện `@mediapipe/hands` v0.4.x trên **Main Thread** (thư viện không hỗ trợ Web Worker do dùng DOM APIs nội bộ). Binary assets (`.tflite`, `.wasm`, `.data`) được host cục bộ trong `mediapipe/` để tránh lỗi CORS/MIME. Phần ML inference nặng chạy trong WASM runtime tách biệt. Camera capture throttle ~30fps.
    -   Viết logic để truyền video frames vào MediaPipe và lưu tọa độ bàn tay (hand coordinates) vào biến global `latestHandData`.
- [x] **Task 1.3: Giao tiếp JS to Unity (Sử dụng .jslib)**
    -   Tránh sử dụng `SendMessage(JSON)` để ngăn chặn hiện tượng rác bộ nhớ (Garbage Collection - GC spikes) gây giật lag trong Unity.
    -   Tạo một plugin `.jslib` trong Unity. Plugin này cho phép mã C# đọc trực tiếp tọa độ mới nhất từ bộ nhớ đệm (buffer) của JavaScript thông qua một định dạng chuỗi thô (raw string) cực kỳ tối ưu (ví dụ: `"0.45,0.62,0"`).
- [x] **Task 1.4: Xác thực (Validation) & Fix Web Worker Blocker**
    -   Tải các file nhị phân của MediaPipe (`.tflite`, `.data`, `.wasm`) về lưu nội bộ trong `WebGLTemplates/ServerDefender` để khắc phục lỗi Strict MIME type của CDN khi gọi qua Web Worker.
    -   Map (ánh xạ) tọa độ chuẩn hóa sang World Space của Unity.
    -   Hiển thị vị trí bàn tay bằng `HandCursor2D` và verify thành công.

## Phase 2: Cơ chế Gameplay Cốt lõi (Core Mechanics)
**Mục tiêu:** Lập trình các vòng lặp tương tác chính: Tát/Quẹt (Slap/Swipe) bọ (bugs) và Cầm/Kéo (Pinch/Drag) dây cáp mạng.

### Milestones & Tasks
- [x] **Task 2.1: Nhận diện Cử chỉ (Gesture Recognition)**
    -   **Slap/Swipe:** Phát hiện sự thay đổi vận tốc (velocity) đột ngột của bàn tay trong một khoảng thời gian ngắn (`DeltaPosition / DeltaTime > Threshold`).
    -   **Pinch:** Tính toán khoảng cách (distance) giữa Ngón trỏ (Index Finger Tip) và Ngón cái (Thumb Tip). Nếu khoảng cách < Threshold, kích hoạt trạng thái "Cầm/Nắm" (Pinch/Grab).
- [x] **Task 2.2: Hệ thống Bug/Malware (Lập trình OOP)**
    -   Triển khai Pattern `ObjectPooler` để spawn (tạo) và destroy (hủy) các con bug một cách tối ưu hiệu năng.
    -   Tạo class `BugSpawner` để quản lý tỷ lệ spawn, các đợt tấn công (waves) và độ khó tăng dần.
    -   Tạo base class `MalwareBug` và các biến thể bug cụ thể (ví dụ: fast bugs, tanky bugs).
    -   Tích hợp Physics 2D (Colliders) để bắt va chạm (intersections) giữa `HandCursor2D` (khi đang ở trạng thái swipe) và `MalwareBug`.
- [x] **Task 2.3: Hệ thống Nối cáp mạng (Cable Reconnection)**
    -   Lập trình các đầu cáp có thể kéo thả (draggable) gắn liền với trạng thái Pinch (tương đương với `OnMouseDrag` truyền thống).
    -   Tạo các cổng kết nối (Ports - snap targets). Khi nhả cáp mạng ra trong bán kính của đúng cổng quy định, kết nối được thiết lập (hoàn thành cấu hình VLAN/OSPF).
- [x] **Task 2.4: Game Manager & State Machine**
    -   Triển khai class `GameManager` (áp dụng Singleton pattern) để xử lý các Game States: `MainMenu`, `Playing`, `GameOver`.
    -   Tạo vòng lặp đếm ngược 1-2 phút (timer loop) và hệ thống tính điểm (score tracking).
    -   Tách biệt `GameMode`: Cho phép chọn chơi mini-game Bắt bọ hoặc Nối cáp thông qua UI, ẩn thành phần của mini-game kia.

## Phase 3: Đồ họa, Âm thanh & UI/UX (Polish)
**Mục tiêu:** Nâng tầm "Game Feel", làm cho game phản hồi nhanh và cuốn hút.

### Milestones & Tasks
- [x] **Task 3.1: Visual Feedback & Particles**
    -   Thêm particle effects (hiệu ứng hạt) khi tiêu diệt Bug (hiệu ứng nổ kỹ thuật số/glitches).
    -   Thêm particle trails (vệt sáng) cho Hand Cursor để nhấn mạnh các cú quẹt tay (swipes).
    -   Triển khai hiệu ứng rung màn hình (screen shake) và khựng hình (hit-stop) để tăng lực sát thương khi đánh trúng.
- [x] **Task 3.2: Lập trình UI**
    -   Thiết kế và tích hợp HUD (Điểm số, Thời gian còn lại, Thanh máu hệ thống mạng).
    -   Tạo các pop-ups text như sát thương nảy lên (floating damage) hoặc thông báo "Access Denied".
- [ ] **Task 3.3: Thiết kế Âm thanh (Audio)** (🚧 *Tạm hoãn phần kéo thả Editor, code đã hoàn tất*)
    -   Tích hợp nhạc nền (background music) mang phong cách synth-wave năng động.
    -   Thêm các SFX (hiệu ứng âm thanh) rõ ràng cho hành động quẹt tay, đập bug, nối cáp, và còi báo động (alarms).

## Phase 4: Backend & Tích hợp Leaderboard
**Mục tiêu:** Lưu trữ điểm số để tạo sự cạnh tranh giữa các sinh viên tham gia sự kiện, sử dụng BaaS (Backend as a Service) để tiết kiệm thời gian và chi phí.

### Milestones & Tasks
- [ ] **Task 4.1: Thiết lập Firebase / Supabase**
    -   Tạo project Firebase và thiết lập Realtime Database hoặc Firestore.
    -   Cấu hình security rules (quy tắc bảo mật) cho phép đọc không cần xác thực (unauthenticated reads) nhưng phải xác thực chặt chẽ khi ghi điểm (write validation).
- [ ] **Task 4.2: Tích hợp REST API vào WebGL**
    -   Vì Native Firebase Unity SDK hỗ trợ WebGL khá hạn chế, chúng ta sẽ dùng HTTP REST calls tiêu chuẩn (`UnityWebRequest`) hoặc JavaScript interop để giao tiếp với Firebase REST API.
- [ ] **Task 4.3: Hiển thị UI Leaderboard**
    -   Đẩy (Push) điểm số cuối cùng trực tiếp lên database ngay khi vào state `GameOver`.
    -   Fetch (tải về) top 10 điểm cao nhất để hiển thị trên `MainMenu` hoặc màn hình kết thúc.

## Phase 5: Web Deployment & Kiểm thử thực tế
**Mục tiêu:** Deploy game lên một URL public và đảm bảo nó chạy mượt mà trên nhiều trình duyệt khác nhau.

### Milestones & Tasks
- [ ] **Task 5.1: Web Hosting Deployment**
    -   Build project Unity WebGL.
    -   Deploy các file HTML/JS/WASM lên Vercel, GitHub Pages, hoặc Firebase Hosting.
- [ ] **Task 5.2: Tool Cân chỉnh In-Game (Calibration Tooling)**
    -   Xây dựng một settings menu ẩn trong Unity để điều chỉnh độ nhạy tracking, ngưỡng tốc độ của cú tát (swipe velocity thresholds), và khoảng cách chụm ngón tay (pinch distances) ngay tại sự kiện.
- [ ] **Task 5.3: Cross-Browser Testing**
    -   Test game trên Chrome, Firefox, và Edge để đảm bảo tính ổn định về quyền truy cập camera (MediaPipe permissions) và hiệu năng WebGL.

---

## Chiến lược Quản trị Rủi ro (Risk Management)

### 1. Giới hạn Hiệu năng WebGL (Main Thread Blocking & GC Spikes)
*   **Risk (Rủi ro):** Chạy Unity và MediaPipe đồng thời trên Main Thread sẽ gây ra hiện tượng giật cục (stuttering) nghiêm trọng. Thêm vào đó, việc phân tích chuỗi JSON thông qua hàm `SendMessage` mỗi khung hình sẽ gây ra các đợt Garbage Collection (GC spikes) làm đứng game.
*   **Mitigation (Khắc phục):** 
    *   **Threading:** Bắt buộc chạy MediaPipe trong **Web Worker** để không tranh chấp CPU với Unity.
    *   **Memory Optimization:** Dùng plugin `.jslib` và truyền data dưới dạng chuỗi thô (raw string) để triệt tiêu hoàn toàn GC Spikes.
    *   **Build Optimization:** Giữ cho bản build Unity WebGL thật nhẹ (nén textures/audio, giới hạn số lượng particles) và thiết lập `modelComplexity` của MediaPipe về mức `0` để ưu tiên tốc độ.

### 2. Quyền truy cập Camera của Trình duyệt (Browser Camera Permissions)
*   **Risk:** Trình duyệt mặc định chặn truy cập camera. Game sẽ không hoạt động nếu user từ chối cấp quyền.
*   **Mitigation:**
    *   Thiết kế một lớp UI (overlay) rõ ràng với thông báo "Waiting for Camera Permission...". Cung cấp hướng dẫn cụ thể cách bật lại camera nếu lỡ tay bấm từ chối.

### 3. Môi trường Thực tế: Ánh sáng & Chuyển động nhanh
*   **Risk:** MediaPipe hand tracking hoạt động kém trong điều kiện thiếu sáng. Chuyển động tay quá nhanh sẽ gây nhòe hình (motion blur) trên các webcam tiêu chuẩn.
*   **Mitigation:** 
    *   Khuyến cáo người chơi ở nơi đủ sáng. Nếu triển khai tại sự kiện thực tế, bắt buộc phải trang bị đèn vòng (ring lights) trợ sáng và webcam **60fps** (đã chỉnh giảm exposure) để có trải nghiệm tốt nhất.
