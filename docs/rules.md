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
- **KHÔNG** chạy MediaPipe JS trên Main Thread, BẮT BUỘC dùng Web Worker.
- **KHÔNG** tự ý thay đổi các cấu hình quan trọng (như Firebase rules, Unity Project Settings) mà không có xác nhận rõ ràng từ người dùng.
- **KHÔNG** tự bịa ra URL, API endpoint, hoặc tên hàm không tồn tại trong tài liệu SDK chính thức.
