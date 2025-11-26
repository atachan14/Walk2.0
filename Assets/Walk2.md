### 開発リスト ###
- Diary
    - Scroll消す
    - somooneの記録
        - 

    - mapSize==0
        - select
        - retry

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
 