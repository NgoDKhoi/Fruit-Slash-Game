# Project Context & Overview

## 1. Project Objective (Mục tiêu Dự án)
- **Tên Game:** Server Defender (Bảo vệ Máy chủ).
- **Mục tiêu:** Xây dựng một tựa game tương tác 2D nhịp độ cực nhanh (1-2 phút/lượt) sử dụng công nghệ computer vision (nhận diện cử chỉ tay qua webcam). 
- **Tech Stack:** Unity WebGL (C#) cho Game Engine + JavaScript MediaPipe cho Computer Vision + Firebase cho Leaderboard.
- **Môi trường Deploy:** Chạy trực tiếp trên trình duyệt Web để dễ chia sẻ link. Tránh dùng cài đặt app Desktop phức tạp.

## 2. Target Audience & Users (Đối tượng Người dùng)
- Sinh viên tham gia lễ hội công nghệ (IT/Tech Festival). Yêu cầu game phải phản hồi tức thời (zero latency input), đồ họa bắt mắt, và cơ chế tính điểm cạnh tranh (Leaderboard).

## 3. Core Domain Glossary (Thuật ngữ Cốt lõi)
- **Hand Tracking / CV:** Phân tích hình ảnh bàn tay qua Webcam, được xử lý bởi thư viện MediaPipe chạy trong một *Web Worker* ở trình duyệt.
- **Slap / Swipe:** Cử chỉ Quẹt / Tát nhanh bằng tay để tiêu diệt các bọ mạng (MalwareBug) rơi xuống màn hình.
- **Pinch / Drag:** Cử chỉ Chụm ngón trỏ và ngón cái để cầm, nắm, và kéo nối các dây cáp mạng bị đứt (VLAN/OSPF).
- **GC Spikes (Garbage Collection Spikes):** Hiện tượng khựng hình/lag khi Unity WebGL phải liên tục dọn dẹp bộ nhớ (do string parsing tạo rác). Kiến trúc này CẤM các thao tác sinh rác bộ nhớ mỗi frame (như parse JSON bằng hàm SendMessage).
- **.jslib Interop:** Cơ chế giao tiếp siêu tốc giữa JavaScript và Unity WebGL bằng cách map thẳng function. C# sẽ gọi hàm JS để lấy chuỗi tọa độ (raw string).
