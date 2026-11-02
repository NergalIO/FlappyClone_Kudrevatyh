# FlappyClone_Kudrevatyh

Практика № 3 по дисциплине «Программирование мобильных игр» — мини-игра «Flappy Clone» («Пролети в трубу»)
на физике Rigidbody2D. Вариант B (индивидуальное задание). Автор: Кудреватых Артем Андреевич.

![Геймплей](Media/gameplay_editor.gif)

## Правила

Птица летит вправо по сумеречному небу мимо синих колонн. Каждый тап по экрану — взмах крыльями: птица подскакивает
вверх, а между тапами падает под действием гравитации. Нужно пролетать в проходы между верхней и нижней
трубой: за каждую пройденную пару начисляется очко. Касание трубы или земли — «Игра окончена»: на экране
показываются счёт и рекорд, кнопка «Играть снова» перезапускает уровень.

Игра начинается с первого тапа: до него птица покачивается на месте, а внизу горит подсказка
«Тапни, чтобы взлететь».

## Как устроено

| Скрипт | Объект | Что делает |
|---|---|---|
| `BirdController.cs` | Bird | Читает тап через Input System (`Pointer.current`), в `FixedUpdate` задаёт `rb.linearVelocity.y = jumpForce`, ограничивает скорость падения и высоту, наклоняет спрайт по скорости |
| `BirdCollision.cs` | Bird | `OnCollisionEnter2D` с тегом `Pipe` или `Ground` → `rb.simulated = false` и `GameManager.GameOver()` |
| `Pipe.cs` | префаб Pipe | Kinematic Rigidbody2D, движение влево через `rb.MovePosition`, выключение за левым краем; триггер ScoreZone даёт +1 очко (флаг `scored`) |
| `PipeSpawner.cs` | PipeSpawner | `InvokeRepeating` каждые 1,5 с, случайная высота прохода, пул объектов вместо `Instantiate`/`Destroy` |
| `Parallax.cs` | Background, GroundVisual | Бесконечная прокрутка Tiled-спрайта: фон 0,6 ед/с, земля 4 ед/с (как трубы) |
| `GameManager.cs` | GameManager | Синглтон: состояния Ready → Running → GameOver, счёт, рекорд в PlayerPrefs, перезапуск сцены |

Префаб `Assets/Prefabs/Pipe.prefab`: корень с Kinematic Rigidbody2D и `Pipe.cs`, дочерние PipeTop и PipeBottom
(SpriteRenderer в режиме Sliced, BoxCollider2D, тег `Pipe`) и ScoreZone (BoxCollider2D, Is Trigger) в проходе.
Теги: `Player` (птица), `Pipe`, `Ground`.

## Параметры

| Параметр | Значение | Почему |
|---|---|---|
| Камера, Orthographic Size | 9,6 | Видимая высота 19,2 ед. — фон 1080×1920 при PPU 100 ровно на весь экран |
| Gravity Scale | 4,5 | Ускорение 44 ед/с²; вместе с jumpForce даёт подскок ≈ 1,9 ед. (10 % экрана) за 0,3 с |
| jumpForce | 13 | См. выше; пересчитано из значений методички (7 и 2,5 для камеры размером 5) |
| Скорость труб и земли | 4 ед/с | Пролёт сквозь трубу (≈ 1,9 ед. по коллайдерам) занимает ≈ 0,5 с — меньше одного цикла «тап → подъём → падение» (0,59 с) |
| spawnInterval | 1,5 с | Расстояние между трубами 6 ед.; на экране одновременно видно две пары |
| Проход | 4 ед. | ≈ 2 высоты подскока, ≈ 5 диаметров коллайдера птицы |
| Высота прохода | −4…4,5, шаг не больше 3,5 ед. | Каждая труба видна хотя бы на 2,3 ед.; соседние проходы всегда достижимы |

## Окружение

- Unity 6000.6.5f1 (Unity 6.6), шаблон Universal 2D (URP)
- uGUI + TextMeshPro, Input System (Active Input Handling: Input System Package)
- Android: IL2CPP, ARM64, минимальный API 26, портретная ориентация, Package Name `com.kudrevatyh.flappyclone`

Спрайты нарисованы самостоятельно скриптом [`Tools/make_sprites.py`](Tools/make_sprites.py) (Python + Pillow).

## Сборка

File → Build Profiles → Android → Switch Platform → Build (или Build And Run с подключённым телефоном).

## Медиа

- `Media/gameplay_editor.mp4`, `Media/gameplay_editor.gif` — 18 секунд геймплея (запись из редактора)
- `Media/gameover_editor.png` — экран Game Over
