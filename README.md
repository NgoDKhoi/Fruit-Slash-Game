# 🍉 Fruit Slash — Chém Hoa Quả Bằng Cử Chỉ Tay

> Game chém hoa quả điều khiển bằng **cử chỉ tay qua Webcam**, sử dụng AI nhận diện bàn tay (MediaPipe) kết hợp Unity WebGL. Không cần chuột, không cần bàn phím — chỉ cần vung tay là chém!

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?logo=unity)
![WebGL](https://img.shields.io/badge/Platform-WebGL-blue)
![MediaPipe](https://img.shields.io/badge/AI-MediaPipe%20Hands-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

---

## 🎮 Giới Thiệu

**Fruit Slash** là một trò chơi tương tác thời gian thực, lấy cảm hứng từ Fruit Ninja. Người chơi sử dụng **bàn tay thật** trước Webcam để chém các loại hoa quả rơi từ trên trời xuống.

Trò chơi được phát triển như một project kết hợp giữa **Computer Vision** và **Game Development**, chạy hoàn toàn trên trình duyệt web (WebGL) mà không cần cài đặt phần mềm.

### ✨ Tính Năng Chính

- 🖐️ **Điều khiển bằng cử chỉ tay** — AI nhận diện vị trí bàn tay qua Webcam và chuyển thành con trỏ trong game.
- 🧠 **Bộ lọc chống rung thông minh (Adaptive Smoothing)** — Con trỏ mượt mà: đứng yên thì ổn định, vung tay thì phản hồi tức thời.
- 🍌 **Nhiều loại hoa quả** — Quả thường rơi thẳng, quả đặc biệt (WavyFruit) bay dích dắc khó đoán.
- ❤️ **Hệ thống 3 mạng** — Để hoa quả rơi đất sẽ mất 1 mạng, hết 3 mạng là Game Over.
- ⏱️ **Đếm ngược 60 giây** — Chém càng nhiều càng được nhiều điểm trong thời gian giới hạn.
- 💥 **Hiệu ứng Game Feel** — Screen Shake khi mất mạng, Hit-Stop khi chém trúng, hiệu ứng nổ particle.
- 📷 **Khung Camera thông minh** — Tự động ẩn ở Menu, hiện khi chơi, đặc 100% không bị mờ.
- 🔄 **Object Pooling** — Tối ưu hiệu năng cho WebGL, không tạo/xóa object liên tục.

---

## 🏗️ Kiến Trúc Hệ Thống

```
┌─────────────────────────────────────────────────────┐
│                    TRÌNH DUYỆT WEB                  │
│                                                     │
│  ┌──────────────┐    ┌───────────────────────────┐  │
│  │   Webcam      │───▶│  MediaPipe Hands (AI)     │  │
│  │   (Camera)    │    │  Nhận diện 21 điểm tay    │  │
│  └──────────────┘    └─────────┬─────────────────┘  │
│                                │ Tọa độ (x, y)      │
│                                ▼                     │
│  ┌─────────────────────────────────────────────────┐│
│  │              UNITY WEBGL ENGINE                  ││
│  │                                                  ││
│  │  WebGLHandReceiver ──▶ HandCursor2D ──▶ Fruit    ││
│  │  (Nhận tọa độ JS)    (Bộ lọc mượt)   (Va chạm) ││
│  │                                                  ││
│  │  GameManager ◀──▶ UIManager ◀──▶ FruitSpawner    ││
│  │  (Điều phối)      (Giao diện)    (Sinh hoa quả) ││
│  └─────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────┘
```

---

## 🛠️ Công Nghệ Sử Dụng

| Thành phần | Công nghệ | Mô tả |
|---|---|---|
| Game Engine | **Unity 2022.3+** | Xây dựng gameplay, vật lý, UI |
| Nền tảng | **WebGL** | Chạy trên trình duyệt, không cần cài đặt |
| AI Hand Tracking | **MediaPipe Hands** | Nhận diện 21 điểm trên bàn tay real-time |
| Ngôn ngữ | **C#** (Unity) + **JavaScript** (Web) | Logic game + Cầu nối Camera↔Unity |
| Giao tiếp | **JSLib Bridge** | Truyền dữ liệu từ JS sang C# qua WebAssembly |

---

## 📁 Cấu Trúc Thư Mục

```
Server-Defender-Game/
├── Server-Defender-Game/          # Unity Project
│   ├── Assets/
│   │   ├── Scripts/
│   │   │   ├── CV/                # Computer Vision
│   │   │   │   ├── HandCursor2D.cs        # Con trỏ tay + Adaptive Smoothing
│   │   │   │   └── WebGLHandReceiver.cs   # Nhận dữ liệu từ JS
│   │   │   ├── Gameplay/          # Logic trò chơi
│   │   │   │   ├── GameManager.cs         # Điều phối game (State Machine)
│   │   │   │   ├── Fruit.cs               # Lớp cơ sở hoa quả
│   │   │   │   ├── WavyFruit.cs           # Hoa quả bay dích dắc
│   │   │   │   ├── FruitSpawner.cs        # Sinh hoa quả từ trên trời
│   │   │   │   ├── ObjectPooler.cs        # Tái sử dụng object (tối ưu)
│   │   │   │   ├── CameraShake.cs         # Hiệu ứng rung + khựng hình
│   │   │   │   └── AudioManager.cs        # Quản lý âm thanh
│   │   │   └── UI/                # Giao diện người dùng
│   │   │       ├── UIManager.cs           # Quản lý Menu, HUD, GameOver
│   │   │       └── BackgroundFitter.cs    # Tự động co giãn ảnh nền
│   │   ├── Plugins/WebGL/
│   │   │   └── CVBridge.jslib             # Cầu nối C# ↔ JavaScript
│   │   ├── WebGLTemplates/ServerDefender/
│   │   │   ├── index.html                 # Template web + MediaPipe setup
│   │   │   └── mediapipe/                 # AI models (offline)
│   │   ├── Prefabs/               # Prefab hoa quả, hiệu ứng
│   │   └── Scenes/                # Scene chính của game
│   └── ProjectSettings/
├── docs/                          # Tài liệu thiết kế
└── README.md
```

---

## 🚀 Hướng Dẫn Cài Đặt & Chạy

### Yêu Cầu
- **Unity 2022.3 LTS** trở lên
- **Webcam** được kết nối
- Trình duyệt **Google Chrome** hoặc **Microsoft Edge**

### Chạy Trong Unity Editor
1. Clone repository:
   ```bash
   git clone https://github.com/NgoDKhoi/Server-Defender-Game.git
   ```
2. Mở thư mục `Server-Defender-Game/Server-Defender-Game` bằng Unity Hub.
3. Mở scene `Assets/Scenes/SampleScene.unity`.
4. Bấm **Play** để chạy thử (dùng chuột giả lập tay trong Editor).

### Build & Chạy Trên Web
1. Trong Unity: **File → Build Settings** → Chọn **WebGL** → **Build**.
2. Mở terminal tại thư mục build, chạy local server:
   ```bash
   python -m http.server 8000
   ```
3. Mở trình duyệt, truy cập `http://localhost:8000`.
4. Cho phép quyền truy cập Webcam khi được hỏi.

---

## 📦 Triển Khai Offline (Sự Kiện / Nhiều Máy)

Game có thể chạy **hoàn toàn không cần Internet** vì toàn bộ AI nhận diện tay (MediaPipe) đã được đóng gói sẵn thành các file offline bên trong thư mục game. Mọi xử lý AI đều chạy bằng CPU/GPU của máy tính, không gửi dữ liệu lên mạng.

### 1. Yêu Cầu Phần Cứng

| Thiết bị | Yêu cầu |
|---|---|
| Máy tính | PC hoặc Laptop chạy Windows / macOS |
| Webcam | Bắt buộc — Cắm sẵn hoặc dùng webcam tích hợp laptop |
| USB | Để chép file game sang các máy sự kiện |
| Trình duyệt | **Google Chrome** hoặc **Microsoft Edge** (khuyên dùng) |

### 2. Xuất File Game (Trên Máy Dev)

1. Mở Unity Editor, chọn **File → Build Settings**.
2. Đảm bảo nền tảng đang chọn là **WebGL**.
3. Nhấn nút **Build**, chọn một thư mục trống (VD: `FruitSlash_Web`).
4. Chờ Unity biên dịch xong (có thể mất vài phút).
5. Sau khi xong, thư mục build sẽ có cấu trúc như sau:

```
FruitSlash_Web/
├── index.html              ← Trang web chính
├── Build/                  ← Mã nguồn game (WASM + JS + Data)
│   ├── Build.data.gz
│   ├── Build.framework.js.gz
│   ├── Build.loader.js
│   └── Build.wasm.gz
├── TemplateData/           ← Logo loading, CSS trang trí
├── mediapipe/              ← AI nhận diện tay (QUAN TRỌNG!)
│   ├── hands.js
│   ├── hand_landmark_lite.tflite
│   ├── hands_solution_simd_wasm_bin.wasm
│   └── ... (các file model khác)
└── mongoose.exe            ← Máy chủ ảo (bạn tự thêm vào)
```

> ⚠️ **QUAN TRỌNG:** Phải copy **toàn bộ** thư mục build (không được thiếu file nào). Nếu thiếu bất kỳ file nào, game sẽ bị treo khi tải hoặc camera sẽ không hoạt động.

### 3. Chuẩn Bị Máy Chủ Ảo (Mongoose)

Trình duyệt web có cơ chế bảo mật: **chặn quyền truy cập Webcam** nếu trang web được mở trực tiếp từ file (`file://`). Vì vậy bạn **không thể** nháy đúp chuột vào `index.html` để chơi. Bắt buộc phải chạy qua một máy chủ ảo (`localhost`).

**Cách lấy Mongoose Web Server:**
1. Truy cập [https://mongoose.ws/](https://mongoose.ws/) và tải file `mongoose.exe` (dung lượng siêu nhẹ ~5MB, **không cần cài đặt**).
2. Copy file `mongoose.exe` thả trực tiếp vào bên trong thư mục `FruitSlash_Web` (nằm **ngang hàng** với file `index.html`).
3. Copy toàn bộ thư mục `FruitSlash_Web` (đã có `mongoose.exe` bên trong) vào **USB**.

### 4. Thiết Lập Tại Máy Sự Kiện

Lặp lại các bước sau cho **mỗi máy tính** tại sự kiện:

1. **Cắm USB** vào máy, copy thư mục `FruitSlash_Web` ra Desktop (hoặc ổ đĩa bất kỳ).
2. **Cắm Webcam** vào máy (nếu chưa có webcam tích hợp).
3. **Nháy đúp chuột** vào file `mongoose.exe` bên trong thư mục.
   - Nếu Windows Firewall hỏi → Chọn **Allow Access**.
   - Mongoose sẽ tự động mở tab trình duyệt với địa chỉ `http://localhost:8000`.
4. **Cấp quyền Webcam:** Trình duyệt sẽ hiện thông báo:
   > `"localhost:8000" wants to use your camera`
   
   → Bấm **Allow (Cho phép)**.
5. **Phóng to màn hình:** Bấm phím **F11** để vào chế độ toàn màn hình (Fullscreen) cho trải nghiệm Arcade tốt nhất.
6. **Hoàn tất!** Bắt đầu vung tay chém hoa quả! 🍉

### 5. Xử Lý Sự Cố Thường Gặp

| Vấn đề | Nguyên nhân | Cách khắc phục |
|---|---|---|
| Game tải mãi không xong | Thiếu file trong thư mục Build | Copy lại **toàn bộ** thư mục build từ USB |
| Camera không bật | Chưa cấp quyền Webcam | Nhấp vào biểu tượng 🔒 trên thanh địa chỉ → Cho phép Camera |
| Camera bật nhưng con trỏ không xuất hiện | Thiếu thư mục `mediapipe/` | Kiểm tra thư mục `mediapipe/` có đầy đủ file AI model không |
| Mở `index.html` trực tiếp, game không chạy | Thiếu máy chủ ảo (localhost) | Phải chạy qua `mongoose.exe`, không được nháy đúp `index.html` |
| Mongoose bị Firewall chặn | Firewall chưa cho phép | Chọn **Allow Access** khi Windows Firewall hỏi |
| Game bị giật trên máy yếu | CPU/GPU không đủ mạnh | Đóng các tab/ứng dụng khác, dùng Chrome thay vì Firefox |

> 💡 **Mẹo:** Bạn có thể triển khai trên bao nhiêu máy tùy thích. Mỗi máy hoạt động hoàn toàn độc lập, không cần kết nối mạng hay kết nối với nhau.

---

## 🎯 Cách Chơi

1. Đứng trước Webcam, đưa **một bàn tay** vào khung hình.
2. Con trỏ trên màn hình sẽ bám theo tay bạn.
3. **Vung tay** qua các hoa quả đang rơi để chém chúng.
4. Mỗi quả chém trúng = **+10 điểm**.
5. Để quả rơi xuống đất = **-1 mạng** (tổng 3 mạng).
6. Hết **60 giây** hoặc hết mạng → Game Over.
7. Bấm **Play Again** để chơi lại hoặc **Main Menu** để về menu.

---

## 👨‍💻 Tác Giả

- **Ngô Đình Khôi** — [GitHub](https://github.com/NgoDKhoi)

---

## 📄 Giấy Phép

Project này được phân phối dưới giấy phép **MIT**. Xem file [LICENSE](LICENSE) để biết thêm chi tiết.