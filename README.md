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

Game có thể chạy **hoàn toàn không cần Internet** vì toàn bộ AI model đã được đóng gói sẵn.

1. Build game ra thư mục từ Unity.
2. Tải [Mongoose Web Server](https://mongoose.ws/) (1 file `.exe` nhỏ, không cần cài).
3. Copy thư mục build + `mongoose.exe` vào USB.
4. Tại máy sự kiện: Chép ra, nháy đúp `mongoose.exe` → Game tự mở trên trình duyệt.

> ⚠️ **Lưu ý:** Không thể mở trực tiếp `index.html` bằng cách nháy đúp chuột vì trình duyệt sẽ chặn quyền Webcam. Bắt buộc phải chạy qua localhost.

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