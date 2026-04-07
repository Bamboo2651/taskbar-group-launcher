# StackBar

Windows タスクバー グループランチャー

---

## 概要

純正タスクバーのアイコンが増えすぎる問題を解決するアプリ。  
複数のアプリをグループにまとめ、スマホのフォルダのようにポップアップで管理できる。

---

## 仕組み

1. StackBar の設定アプリでグループを作成し、アプリを追加する
2. グループのショートカットを作成して純正タスクバーにピン留めする
3. タスクバーのグループアイコンをクリックするとアプリ一覧がポップアップ表示される
4. アプリをクリックして起動する

---

## アプリ構成

| コンポーネント | 役割 |
|---|---|
| ポップアップ本体 | タスクバーのショートカットから起動。グループのアプリ一覧を表示 |
| 設定・管理アプリ | グループの作成・編集・削除、アプリの追加・削除・移動を管理 |

---

## 機能一覧

### ポップアップ

- タスクバーのグループアイコンをクリックで起動
- アプリをクリックして起動、または他の場所をクリックで閉じる
- アイコン＋アプリ名を表示
- 中身に合わせて自動でサイズが決まる
- 半透明のダークテーマ

### グループの管理

- 設定アプリから新規グループを作成
- グループが空になってからでないと削除できない（誤操作防止）
- グループ名は設定アプリで編集可能
- サイドバーの幅をドラッグで変えられる

### アプリの管理

- ファイル選択ダイアログでexeを追加
- アプリのアイコンをexeから自動取得して表示
- 右クリックメニューから「このグループから外す」「別のグループに移動」
- グループ間の移動ダイアログ対応

### ショートカット・ピン留め

- 設定アプリから「ショートカット作成」ボタンを押すとデスクトップに.lnkを作成
- .lnkを純正タスクバーにドラッグしてピン留め
- ショートカットにはグループIDが引数として含まれる

### 設定アプリ

- タスクトレイのStackBarアイコンをクリックして開く（予定）
- サイドバーでグループ一覧を管理
- メインコンテンツでアプリ一覧を管理

---

## 開発ステップ

| ステップ | 内容 | 状態 |
|---|---|---|
| 1 | タスクトレイ常駐アプリの基盤を作る | 🔲 未着手 |
| 2 | 設定アプリのUIを作る（グループ管理） | 🔄 進行中 |
| 3 | ショートカット作成・ポップアップ起動 | ✅ 完了 |
| 4 | ポップアップウィンドウを作る | 🔄 進行中 |
| 5 | ドラッグ&ドロップ機能を実装する | 🔲 未着手 |
| 6 | デザイン設定・多言語対応 | 🔲 未着手 |
| 7 | インストーラーを作る | 🔲 未着手 |

---

## 技術スタック

| 技術 | 用途 |
|---|---|
| C# | メインの開発言語 |
| WPF (.NET 10.0) | UIフレームワーク |
| Visual Studio 2022 | 開発環境 |
| PowerShell | ショートカット作成 |
| Windows DWM API | ウィンドウエフェクト（調査中） |

---

## リポジトリ構成

```
stackbar/
├── README.md
├── src/
│   └── TaskbarLauncher/
│       ├── Models/
│       │   ├── AppConfig.cs         ← アプリ1つ分のデータ
│       │   └── GroupConfig.cs       ← グループ1つ分のデータ
│       ├── ConfigManager.cs         ← JSONの読み書き・ショートカット作成
│       ├── InputDialog.xaml         ← テキスト入力ダイアログ
│       ├── MoveAppDialog.xaml       ← アプリ移動先選択ダイアログ
│       ├── PopupWindow.xaml         ← ポップアップウィンドウ
│       ├── MainWindow.xaml          ← 設定アプリのメイン画面
│       └── App.xaml                 ← 起動時の引数処理
└── docs/
    └── progress.md
```

---

## データ保存先

```
C:\Users\ユーザー名\AppData\Roaming\StackBar\groups.json
```

---

## 学習ログ

### 2025-04-05

**今日やったこと**

- WPFプロジェクトの初期作成
- 設定アプリのウィンドウ基本スタイル設定
- 設定アプリのレイアウト骨格作成（サイドバー＋メインコンテンツ）
- GroupConfig・AppConfigモデルの作成
- ConfigManagerの作成（JSONの読み書き）
- InputDialogの自作（グループ名入力ダイアログ）
- グループ追加機能の実装
- バリデーションの実装（空のグループ名を弾く）

**今日学んだこと**

- WPFはUIとロジックが分離されている（`.xaml`が見た目、`.xaml.cs`が動き）
- `Grid`の`ColumnDefinitions`と`RowDefinitions`でレイアウトを組む
- `ObservableCollection`を使うとリストの変更がUIに自動反映される
- `DataContext = this`でXAMLからC#のプロパティを参照できる（データバインディング）
- `{Binding Name}`でC#のプロパティをXAMLに繋げる
- `JsonSerializer`でC#のクラスとJSONを相互変換できる
- アプリの設定ファイルは`AppData/Roaming`に保存するのがWindowsの作法
- `string.IsNullOrWhiteSpace()`で空文字・スペースのバリデーションができる
- `ShowDialog()`でダイアログを開き、`DialogResult`で結果を受け取る
- namespaceはプロジェクト内で統一しないとエラーになる

---

### 2026-04-06

**今日やったこと**

- グループ選択時にメインコンテンツエリアにアプリ一覧を表示する機能を実装
- アプリ追加機能を実装（ファイル選択ダイアログでexeを選択）
- サイドバーの幅をドラッグで変えられるGridSplitterを実装
- グループ名が長いときにサイドバーの幅に合わせて省略表示（TextTrimming）
- 横スクロールバーを非表示に（ScrollViewer.HorizontalScrollBarVisibility）

**今日学んだこと**

- `SelectionChanged`イベントでListBoxの選択変更を検知できる
- `SelectedItem is GroupConfig selected`でキャスト（型変換）と取得を同時にできる
- `OpenFileDialog`でWindowsのファイル選択ダイアログを呼び出せる
- `Path.GetFileNameWithoutExtension()`でファイル名から拡張子を除去できる
- `ItemsSource = null`してから再セットすることでListBoxの表示を強制更新できる
- `GridSplitter`を使うとドラッグでパネルの幅を変えられる
- `GridSplitter`は外側のGridの直接の子要素に置く必要がある（入れ子のGridに入れると動かない）
- `TextTrimming="CharacterEllipsis"`で長いテキストを`...`で省略できる
- `ScrollViewer.HorizontalScrollBarVisibility="Disabled"`で横スクロールバーを消せる
- `MinWidth`・`MaxWidth`でサイドバーのリサイズ範囲を制限できる

---

### 2026-04-07

**今日やったこと**

- グループの削除機能を実装（空でないと削除できない安全設計）
- アプリのアイコンをexeから自動取得して表示する機能を実装
- アプリの削除機能を実装（右クリックメニュー）
- アプリの別グループへの移動機能を実装（MoveAppDialogを自作）
- GroupConfigにGUIDベースのIDを追加
- PowerShellを使ったショートカット作成機能を実装
- App.xamlのStartupUriを削除してOnStartupで起動制御に変更
- タスクバーのショートカットからポップアップウィンドウを起動する仕組みを実装
- ポップアップウィンドウの基本UIを実装（アイコン・アプリ名・グループ名）
- ポップアップの位置をタスクバーの上に固定
- ポップアップの半透明デザインを実装

**今日学んだこと**

- `ContextMenu`と`MenuItem`で右クリックメニューを実装できる
- `Icon.ExtractAssociatedIcon()`でexeからアイコンを取得できる
- `[JsonIgnore]`をつけるとJSONの保存対象から除外できる
- `Imaging.CreateBitmapSourceFromHIcon()`でアイコンをWPFで使える形式に変換できる
- `Guid.NewGuid().ToString()`で一意なIDを生成できる
- PowerShellの`New-Object -ComObject WScript.Shell`でショートカットを作成できる
- `StartupUri`を削除して`OnStartup`をオーバーライドすると起動時の処理を制御できる
- `e.Args`で起動時の引数を取得できる
- `SizeToContent="WidthAndHeight"`でウィンドウを中身に合わせて自動サイズにできる
- `SystemParameters.WorkArea.Height`でタスクバーを除いた画面の高さを取得できる
- ビルドしないと古いexeが実行されるため、コード変更後は必ずビルドが必要
- `System.Windows.Forms`と`System.Windows`を同時に使うと名前が競合してエラーになる
- Windows 11の新しいバージョンではDWM APIのAcrylic効果が効かない場合がある

**次回やること**

- ポップアップのデザインをさらに改善する
- タスクトレイ常駐機能を実装する
- アプリの並び替え（ドラッグ&ドロップ）を実装する

---

## 作者

ひろや — HAL Tokyo IT専門学校