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
        ├── NameManager.Instance.Name
        ├── NightSession.Instance.CurrentSize
        ├── GameData.Instance.WalkCount
        ├── GameData.Instance.TurnCount
        ├── DateTime.Now - GameData.Instance.StartTime
        └── GameData.Instance.StartTime

ParsonalData
    └── <uid>
        ├── name
        ├── created
        └── ...（必要なら設定類）

SaveData
    └── <uid>
        ├── mapSize:NightSession.Instance.CurrentSize
        ├── StartTime:GameData.Instance.StartTime
        ├── map:  [...]
        ├── steps:  [...]
        └── isEnd:GameData.Instance.Time
