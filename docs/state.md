# Active Project State & Debug Ledger

## 1. Work in Progress (WIP)
## Current Status
- **Phase 1 (Computer Vision Integration): ✅ HOÀN TẤT & VALIDATED (2026-07-05)**
  - Luồng dữ liệu end-to-end đã hoạt động: Webcam → MediaPipe (Main Thread) → `latestHandData` (global JS var) → `CVBridge.jslib` → `WebGLHandReceiver.cs` → `HandCursor2D.cs` → Chấm đỏ di chuyển theo tay trên trình duyệt.
  - Build and Run trong Unity WebGL thành công, chấm đỏ hiển thị và tracking tay chính xác.
- **Phase 2 (Core Gameplay): ✅ HOÀN TẤT (2026-07-06)**
  - Task 2.1 & 2.2: Hệ thống `MalwareBug` (OOP), `ObjectPooler`, `BugSpawner`. `HandCursor2D` có thể "tát" bọ.
  - Task 2.3: Hệ thống Cable Reconnection (`CableEndpoint`, `ConnectionPort`, `CableRenderer`, `CableManager`).
  - Task 2.4: `GameManager` Singleton với State Machine, timer, score, server HP.
- **Phase 3 (Polish): ✅ HOÀN TẤT (Ngoại trừ setup Audio) (2026-07-07)**
  - ✅ Task 3.1 (Code): `CameraShake.cs`, particle explosion, TrailRenderer.
  - ✅ Task 3.2 (Code): `UIManager.cs` điều khiển MainMenu, HUD, GameOver. Xóa `[TEMP] Auto-Start` trong GameManager.
  - ⚠️ Task 3.3 (Code): `AudioManager.cs` quản lý BGM và SFX đã code xong. Tích hợp âm thanh vào các sự kiện game đã hoàn tất. **Nhưng người dùng yêu cầu tạm hoãn việc kéo thả Audio Source/Clips trong Unity Editor sang lúc khác**.
  - Task tiếp theo: **Phase 4 — Backend & Tích hợp Leaderboard**.

## 2. Next Steps (Ưu tiên theo thứ tự)
1. **Setup AudioManager (Thủ công):** Tạo AudioManager trong Scene và gán các file âm thanh theo hướng dẫn walkthrough.
2. **Phase 4 - Task 4.1:** Thiết lập Firebase / Supabase cho Leaderboard.

## 3. Resolved Blockers (Đã giải quyết)
- **[2026-07-05] CDN MIME type errors:** Lỗi `strict MIME type checking` khi `importScripts()` tải file từ CDN → Fix bằng cách tải tất cả MediaPipe assets về `mediapipe/` cục bộ (same-origin).
- **[2026-07-17] Vercel Deploy & Tách Repo:** Đã cấu hình Vercel Rewrites để proxy `ngodkhoi.vercel.app/Fruit-Slash` trỏ thẳng sang project Vercel mới của repo Fruit-Slash-Game, giúp code sạch sẽ, không dính líu đến code web cá nhân.
- **[2026-07-17] Thuật toán Smart Hand Tracking:** Cải tiến tracking từ 1 tay lên 4 tay, kết hợp thuật toán chấm điểm (ưu tiên Tay phải và Tay to nhất) để tăng độ chính xác trong sự kiện đông người.
- **[2026-07-17] Cấu trúc "Mẹ bồng con":** Đã gộp thư mục Unity Project (trước đây bị lồng ghép sâu) ra ngoài Root, xóa toàn bộ các thư mục rác (Assets rỗng) dư thừa.
- **[2026-07-06] Lỗi ngược trục Y trong Editor (Mouse Fallback):** Trục Y của chuột trong Unity (0 ở đáy) ngược với MediaPipe (0 ở đỉnh) → Fix bằng cách đảo ngược (`1f - mousePos.y`) trong `WebGLHandReceiver.cs` để test Editor chuẩn như chạy web thật.
- **[2026-07-06] UI Landmark Overlay X mirroring fix:** Đã điều chỉnh logic vẽ landmark trong `index.html` để mirror trục X cho cả các đường nối và điểm, khớp với video webcam mirrored.
- **[2026-07-07] Khó đánh trúng bug (Hitbox/Swipe logic):** Bỏ yêu cầu vận tốc swipe (`isSwiping`) trong `HandCursor2D.cs` để hễ chạm là diệt bọ (giống Fruit Ninja). Hitbox của bọ và tay cần được kéo to hơn hình ảnh thật 10-20% trong Editor để có Game Feel tốt hơn.

## 4. Architectural Decisions & Discarded Approaches
- **[2026-07-17] Tắt nén Brotli cho Offline:** Do Mongoose không hỗ trợ chuẩn header Brotli (.br) khi chạy HTTP thường, đã quyết định Tắt nén Fallback trong Unity để đảm bảo file chạy 100% khi mang USB ra sự kiện offline.
- **[2026-07-07] Tạm hoãn Âm thanh (Task 3.3):** Code AudioManager và trigger event đã hoàn tất, nhưng việc gán AudioClip trong Editor bị người dùng yêu cầu dời lại sau. Nhờ null check (`if (AudioManager.Instance != null)`), game vẫn chạy an toàn không lỗi ném ra console.
- **[2026-07-05] Dùng Firebase (Leaderboard):** Firebase Realtime DB + REST API qua `UnityWebRequest`.
- **[2026-07-17] Model MediaPipe:** Bắt buộc dùng `modelComplexity: 0` (Lite) do thư mục local không đi kèm model Full, tránh lỗi Aborted trong WASM.
- **[2026-07-05] Main Thread MediaPipe (không dùng Web Worker):** Thư viện `@mediapipe/hands` v0.4.x yêu cầu DOM APIs → Main Thread. ML inference nặng chạy trong WASM. Camera throttle ~30fps.
- **[2026-07-07] Xóa TEMP Auto-Start:** Đã xóa logic ép game tự chạy. Giờ người chơi phải bấm nút "Start Game" từ Main Menu.

## 5. Pending Inspector Setup (Checklist cho Unity Editor)

Sau khi tách Hierarchy và tách Game Mode, các tham chiếu trong Inspector bị mất. Cần kiểm tra và kéo thả lại:

| # | GameObject | Trường (Field) | Kéo thả (Drag) | Trạng thái |
|---|---|---|---|---|
| ① | `GameManager` | `bugSpawner` | → `FruitSpawner` (Đã đổi tên) | ✅ Đã xác nhận |
| ② | `GameManager` | `cableManager` | → `CableManager` | ✅ Đã xác nhận |
| ③ | `ObjectPooler` | `Pools[]` | Worm, Trojan, BugExplosion (prefab, size:10) | ✅ Đã xác nhận |
| ④ | `FruitSpawner` | `Bug Tags[]` | `"Worm"`, `"Trojan"` | ✅ Đã xác nhận |
| ⑤ | `Main Camera` | `CameraShake.cs` | Gắn component | ✅ Đã xác nhận |
| ⑥ | `HandCursor` | `TrailRenderer` | Thêm component, Time:0.15, Width:0.3→0 | ✅ Đã xác nhận |
| ⑦ | `Canvas` | `UIManager.cs` | Kéo thả Panels và TextMeshPro/Slider UI | ✅ Đã xác nhận |
| ⑧ | `CableManager`| `cableSystemContainer`| → `CableSystemContainer` (Tắt mặc định) | ✅ Đã xác nhận |
| ⑨ | `Canvas` | `2 Nút Play` | 1 nút trỏ tới `OnStartBugDefenderClicked`, 1 nút trỏ tới `OnStartCableReconnectClicked` | ✅ Đã xác nhận |
| ⑩ | `[Background]` | `Tùy chỉnh UI` | Chứa Menu_Background, GamePlay_Background, GameOver_Background | ✅ Đã xác nhận |

| ⑪ | `AudioManager` | `Audio Sources & Clips`| (TẠM HOÃN) Gán 2 component AudioSource và kéo thả âm thanh | ❓ Chưa xác nhận |

> **Lưu ý:** Hierarchy đã được tổ chức lại vô cùng gọn gàng với các thẻ `[Managers]`, `[GamePlay]`, `[Background]`!
