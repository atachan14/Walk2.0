### 開発リスト ###
- sound
    - bgm
    - se

- 川/橋
- 4796表示

- レイアウト諸々

### 保留リスト ###
- Diary
    - somooneの記録
        - フォロー
    - フォローの記録

ClearRecords
    └── <auto-id>
        ├── uid
        ├── NameManager.Instance.Name
        ├── NightSession.Instance.CurrentSize
        ├── GameData.Instance.WalkCount
        ├── GameData.Instance.TurnCount
        ├── GameData.Instance.NotchCount
        ├── GameData.Instance.EndTime
        ├── GameData.Instance.EndTime - GameData.Instance.StartTime
        └── GameData.Instance.StartTime

ParsonalData
    └── <uid>
        ├── name
        ├── created
        └── ...（必要なら設定類）

SaveData
    └── <uid>
        ├── mapSize:NightSession.Instance.CurrentSize
        ├── GameData.Instance.WalkCount
        ├── GameData.Instance.TurnCount
        ├── GameData.Instance.NotchCount
        ├── StartTime:GameData.Instance.StartTime
        ├── EndTime:GameData.Instance.EndTime
        ├── map:  [...]
        └── steps:  [...]
 