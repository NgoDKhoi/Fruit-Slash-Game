# AI Coding Rules & Standards (Anti-Hallucination Guardrails)

## 1. Context & Grounding (Tuyệt đối tuân thủ)
- **Đọc trước khi làm:** BẮT BUỘC đọc file `docs/context.md` và `docs/state.md` ở đầu mỗi phiên làm việc để lấy ngữ cảnh.
- **Không tự bịa (Zero Hallucination):** Chỉ sử dụng các thư viện, framework, và kiến trúc đã được quy định trong `context.md` và `plan.md`. Không tự ý gợi ý các công nghệ cũ đã bị loại bỏ (như Python, UDP, AWS).
- **Thừa nhận thiếu sót:** Nếu yêu cầu của người dùng vượt quá những gì có trong tài liệu, phải báo cáo "Tôi không có đủ thông tin / Vấn đề này chưa được chốt", tuyệt đối không đoán mò hay tự viết code theo logic cá nhân mà không hỏi ý kiến.

## 2. Code Style & Cấu trúc
- **Self-Correction & Verification:** Trước khi output code ra cho người dùng, AI phải tự đối chiếu xem code đó có vi phạm quyết định kiến trúc nào trong `state.md` không.
- **No Magic Numbers:** Sử dụng Hằng số (Constants) hoặc Variables rõ ràng thay vì số cứng (hardcoded).
- **Micro-steps:** Khi thực hiện task, hãy chia nhỏ và verify (kiểm tra) từng bước. Nếu gặp lỗi, ghi log vào `state.md` rồi mới đi tiếp.

## 3. Lệnh cấm nghiêm ngặt (Strict Prohibitions / Anti-Patterns)
- **KHÔNG** sử dụng `SendMessage()` để truyền chuỗi JSON lớn từ JS vào Unity (sẽ gây rớt FPS do Garbage Collection Spikes). Phải dùng `.jslib` và Raw String.
- **LƯU Ý:** `@mediapipe/hands` v0.4.x KHÔNG hỗ trợ Web Worker (dùng DOM APIs nội bộ). MediaPipe chạy trên Main Thread, nhưng phần ML inference nặng vẫn chạy trong WASM runtime riêng. Throttle camera capture xuống ~30fps để giảm tải cho Unity.
- **KHÔNG** tự ý thay đổi các cấu hình quan trọng (như Firebase rules, Unity Project Settings) mà không có xác nhận rõ ràng từ người dùng.
- **KHÔNG** tự bịa ra URL, API endpoint, hoặc tên hàm không tồn tại trong tài liệu SDK chính thức.

## 4. Documentation & State Management (MANDATORY)
- **Auto-Update Rule:** Upon completing a logical milestone, finishing a feature, or before asking the user to review a major code change, you MUST automatically propose updates to `/docs/state.md` and `/docs/plan.md`.
- **No Reminders Needed:** Do not wait for the user to explicitly remind you to update the documentation.
- **Action Items:**
  - In `plan.md`: Mark completed tasks with `[x]`.
  - In `state.md`: Update the "Work in Progress (WIP)", "Known Bugs", and log any new "Architectural Decisions" made during the current task.

## 5. Unity Scene & Hierarchy Conventions
- **Single Responsibility:** Mỗi GameObject chỉ gắn 1 script quản lý chính. **KHÔNG** gom `GameManager`, `BugSpawner`, `ObjectPooler` vào cùng 1 object.
- **Thư mục bằng Empty GameObject:** Dùng `[MANAGERS]`, `[GAMEPLAY]` làm parent để gom nhóm. Đặt tên thư mục trong dấu `[ ]` để phân biệt với object thật.
- **Nhóm theo chức năng:** Mỗi bộ cáp (VLAN) gom 3 thành phần (Endpoint, Anchor, Port) vào 1 parent chung. Khi cần thêm cáp, chỉ cần Duplicate (Ctrl+D) cả nhóm và đổi ID.
- **Quy tắc đặt tên GameObject:**
  - Tiền tố mô tả vai trò: `Endpoint_`, `Anchor_`, `Port_`.
  - Hậu tố là ID logic: `VLAN10`, `VLAN20`.
  - Ví dụ: `Endpoint_VLAN10`, `Port_VLAN20`.
- **Reference Wiring:** Khi tạo GameObject mới, phải kéo thả đầy đủ các trường tham chiếu trong Inspector. Xem bảng chi tiết trong [`Architecture.md` — Mục 5: Sơ đồ Tham chiếu](Architecture.md#sơ-đồ-tham-chiếu-reference-wiring-trong-inspector).