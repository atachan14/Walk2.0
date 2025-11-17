### 開発リスト ###
- Ending以降の流れ修正
    - [GoToHome] 座標移動
    - MainVisualManagerでEnding表示に分岐

- HomeManager
    - HomeDir
        0. 寝る
        1. ResultMap
        2. アルバム


ClearRecords
    └── <auto-id>
        ├── uid
        ├── name
        ├── mapSize
        ├── walk
        ├── turn
        ├── time
        └── date

ParsonalData
    └── <uid>
        ├── name
        ├── created
        └── ...（必要なら設定類）

SaveData
    └── <uid>
        ├── mapSize
        ├── date
        ├── map:  [...]
        └── steps: [...]
