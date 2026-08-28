# Match3 Intern Test 2025 — Tile Match Game

## Tổng quan

Dự án ban đầu là một game Match-3 dạng hoán đổi (swap) truyền thống. Toàn bộ gameplay đã được thiết kế lại thành **Tile Match** (dạng game như Tile Master / Triple Match 3D).

---

## Gameplay

### Chế độ thường (Play)
- Board 6×5 (30 ô), mỗi loại item xuất hiện theo bội số của 3.
- Đảm bảo **tất cả các loại item** đều xuất hiện trên board khi bắt đầu.
- Bấm vào item trên board → item bay xuống **khay (Tray)** 5 ô phía dưới.
- Khi có đúng **3 item cùng loại** trong khay → chúng tự động bị xóa.
- **Không thể trả item về board** sau khi đã đưa vào khay.
- **Thắng**: Xóa hết toàn bộ board.
- **Thua**: Khay đầy 5 ô mà không có match.

### Chế độ Time Attack
- Giống chế độ thường nhưng có **đồng hồ đếm ngược 60 giây**.
- **Có thể bấm vào item trong khay để trả nó về ô ban đầu** trên board.
- Khay đầy không kết thúc game ngay — người chơi có thể trả item để tiếp tục.
- **Thắng**: Xóa hết board trước khi hết giờ.
- **Thua**: Hết 60 giây mà chưa xóa hết board.

---

## Tính năng

| Tính năng | Mô tả |
|---|---|
| Tile Match gameplay | Tap để gửi item xuống khay, match 3 để xóa |
| Return item | Bấm vào item trong khay → bay về đúng ô gốc trên board |
| All types guaranteed | Board luôn có đủ tất cả các loại item khi bắt đầu |
| Time Attack mode | 60 giây đếm ngược, không thua khi đầy khay |
| Move animation | Item bay vào khay với hiệu ứng OutBack + punch scale |
| Clear animation | Item match bị xóa với hiệu ứng scale về 0 (InBack) |
| Win / Lose screen | Hiển thị kết quả sau khi game kết thúc |
| Autoplay (Win) | Bot tự động chọn và bấm đúng bộ 3 để thắng |
| Auto Lose | Bot bấm ngẫu nhiên 5 loại khác nhau để thua nhanh |
| Home Screen | 4 nút: Play, Autoplay, Auto Lose, Time Attack |

---

## Kiến trúc

```
GameManager          — Quản lý state (MAIN_MENU / GAME_STARTED / WIN / LOSE)
├── BoardController  — Xử lý input, tạo Board và TrayManager
│   ├── Board        — Dữ liệu lưới, sinh item đảm bảo chia hết 3
│   └── TrayManager  — Logic khay 5 ô, match check, win/lose check
├── AutoplayService  — Coroutine tự động chọn ô mỗi 0.5s
└── LevelTime        — Đếm ngược 60s (Time Attack)

UIMainManager        — Điều phối hiển thị các panel UI
├── UIPanelMain      — Home screen (4 nút SerializeField)
├── UIPanelGame      — Giao diện trong game
├── UIPanelPause     — Menu tạm dừng
└── UIPanelGameOver  — Màn hình kết quả (Win / Lose)
```

---

## Input Detection

- **Board**: Dùng `BoxCollider2D` sẵn có trên `Cell` prefab để detect click.
- **Tray**: Mỗi item có `CircleCollider2D` (radius 0.45) được **enable khi vào khay**, **disable khi trên board**.
- Sử dụng `Physics2D.RaycastAll` để tránh bỏ sót hit khi collider chồng nhau.

---

## Hướng dẫn Setup (Unity)

1. Mở scene chính trong Unity Editor.
2. Trên GameObject `UIPanelMain`, kéo thả các Button vào đúng slot trong Inspector:
   - `Btn Play`
   - `Btn Autoplay`
   - `Btn Auto Lose`
   - `Btn Time Attack`
3. Bấm Play để chạy game.

---

## Công nghệ

- **Unity** (2D, Orthographic Camera)
- **DOTween** — Animation di chuyển và hiệu ứng scale
- **C#** — Toàn bộ logic game
