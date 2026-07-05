# Active Project State & Debug Ledger

## 1. Work in Progress (WIP)
- [x] Đã hoàn thành Phase 0: Chốt kiến trúc hệ thống và setup tài liệu `/docs`.
- [ ] **NEXT STEP (Dành cho phiên làm việc sau):** Bắt tay ngay vào **Phase 1**. Khởi tạo project Unity 2D (URP), set platform thành WebGL. Sau đó bắt đầu tạo template `index.html` và viết mã cho `worker.js` (MediaPipe JS).

## 2. Known Bugs & Blockers
- Chưa có bug hay blocker nào (Chưa bắt đầu giai đoạn code).
- **Lưu ý Môi trường:** Khi test luồng WebGL + Webcam ở máy local (localhost), trình duyệt có thể sẽ chặn Webcam nếu không dùng kết nối HTTPS hoặc live server chuẩn. Đừng quên setup Local Web Server (như Python `http.server` hoặc VSCode Live Server) cho phiên sau.

## 3. Architectural Decisions & Discarded Approaches
- **[2026-07-05] Bỏ Python/UDP (Discarded):** Quyết định loại bỏ hoàn toàn ý tưởng dùng Python/OpenCV/UDP vì nó không thể chạy trực tiếp trên trình duyệt WebGL. Chốt kiến trúc **Unity WebGL + JS MediaPipe**.
- **[2026-07-05] Bắt buộc dùng Web Worker:** Tuyệt đối không chạy MediaPipe trên Main Thread chung với Unity để tránh chiếm CPU, làm tụt FPS của game.
- **[2026-07-05] Loại bỏ SendMessage(JSON):** Đã cấm dùng hàm này để truyền data mỗi frame vì sinh ra quá nhiều rác bộ nhớ (Garbage Collection Spikes). Bắt buộc dùng plugin **`.jslib`** và chuỗi raw data (Ví dụ: `"0.45,0.62,0"`).
- **[2026-07-05] Dùng Firebase (Leaderboard):** Chốt dùng Firebase Realtime DB thay vì tự build AWS EC2/Docker để tối ưu thời gian deploy và bảo trì.
