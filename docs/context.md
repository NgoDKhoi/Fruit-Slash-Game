# Project Context & Overview

## 1. Project Objective (Mục tiêu Dự án)
- **Tên Game:** Fruit Slash (Trước đây là Server Defender).
- **Mục tiêu:** Xây dựng một tựa game chém hoa quả (phiên bản bảo vệ máy chủ / chém bug) tương tác 2D nhịp độ cực nhanh sử dụng công nghệ computer vision (nhận diện cử chỉ tay qua webcam). 
- **Tech Stack:** Unity WebGL (C#) cho Game Engine + JavaScript MediaPipe cho Computer Vision.
- **Môi trường Deploy:** 
  - **Online:** Vercel Rewrites (Proxy Cấp Cao) giúp URL luôn là `ngodkhoi.vercel.app/Fruit-Slash/` dù repo game nằm độc lập.
  - **Offline (Sự kiện):** Dùng Mongoose.exe (Windows) hoặc Python HTTP Server (macOS) để chạy Local.

## 2. Target Audience & Users (Đối tượng Người dùng)
- Sinh viên tham gia lễ hội công nghệ (IT/Tech Festival). Yêu cầu game phải phản hồi tức thời (zero latency input), đồ họa bắt mắt, và cơ chế tính điểm cạnh tranh (Leaderboard).

## 3. Core Domain Glossary (Thuật ngữ Cốt lõi)
- **Hand Tracking / CV:** Phân tích hình ảnh bàn tay qua Webcam, được xử lý bởi thư viện MediaPipe (`@mediapipe/hands` v0.4.x) chạy trên **Main Thread** của trình duyệt. Phần ML inference nặng chạy trong WASM runtime tách biệt, JS chỉ làm orchestration nhẹ. Camera capture throttle ~30fps.
- **Slap / Swipe:** Cử chỉ Quẹt / Tát nhanh bằng tay để tiêu diệt các bọ mạng (MalwareBug) rơi xuống màn hình.
- **Pinch / Drag:** Cử chỉ Chụm ngón trỏ và ngón cái để cầm, nắm, và kéo nối các dây cáp mạng bị đứt (VLAN/OSPF).
- **GC Spikes (Garbage Collection Spikes):** Hiện tượng khựng hình/lag khi Unity WebGL phải liên tục dọn dẹp bộ nhớ (do string parsing tạo rác). Kiến trúc này CẤM các thao tác sinh rác bộ nhớ mỗi frame (như parse JSON bằng hàm SendMessage).
- **.jslib Interop:** Cơ chế giao tiếp siêu tốc giữa JavaScript và Unity WebGL bằng cách map thẳng function. C# sẽ gọi hàm JS để lấy chuỗi tọa độ (raw string).

## 4. Cấu trúc File Hiện tại (Key Files)
## 4. Cấu trúc File Hiện tại (Key Files)
```
Fruit-Slash-Game/                        (Repo root & Unity Project root đã hợp nhất)
├── FruitSlash-Web/                      (Thư mục xuất WebGL để deploy lên Vercel)
│   ├── index.html                       (File chạy chính cho Web)
│   ├── mediapipe/                       (MediaPipe assets cho Web)
│   └── Build/                           (WASM và data của Unity)
├── docs/                                (Tài liệu dự án)
├── Assets/                              (Mã nguồn Unity)
│   ├── Scripts/CV/
│   │   ├── WebGLHandReceiver.cs         (Singleton — đọc raw string từ JS qua .jslib)
│   │   └── HandCursor2D.cs              (Đọc HandData, ánh xạ viewport→world, hiển thị cursor)
│   ├── Plugins/WebGL/
│   │   └── CVBridge.jslib               (JS plugin — đọc window.GetHandDataString())
│   └── WebGLTemplates/ServerDefender/
│       ├── index.html                   (WebGL Template — load mediapipe/hands.js, Smart Tracking)
│       └── mediapipe/                   (Bản gốc MediaPipe assets — Unity copy ra khi build)
```
